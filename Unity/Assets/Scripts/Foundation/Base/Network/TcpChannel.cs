using System;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Chaos
{
    public sealed class TcpChannel : Channel
    {
        private readonly TcpService service;
		private Socket socket;
		private SocketAsyncEventArgs innArgs = new();
		private SocketAsyncEventArgs outArgs = new();

		private readonly CircularBuffer recvBuffer = new();
		private readonly CircularBuffer sendBuffer = new();

		private bool isSending;

		private bool isConnected;

		private readonly PacketParser parser;

		private readonly byte[] sendCache = new byte[Packet.OpcodeLength + Packet.FiberInstanceIdLength];
		
		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			service.Queue.Enqueue(new TcpEventArgs() {ChannelId = Id, SocketAsyncEventArgs = e});
		}
		
		public TcpChannel(long id, IPEndPoint ipEndPoint, TcpService service)
		{
			this.service = service;
			ChannelType = ChannelType.Connect;
			Id = id;
			socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.NoDelay = true;
			parser = new PacketParser(recvBuffer, this.service);
			innArgs.Completed += OnComplete;
			outArgs.Completed += OnComplete;

			RemoteAddress = ipEndPoint;
			isConnected = false;
			isSending = false;
			
			this.service.Queue.Enqueue(new TcpEventArgs(){Operation = TcpOperation.Connect,ChannelId = Id});
		}
		
		public TcpChannel(long id, Socket socket, TcpService service)
		{
			this.service = service;
			ChannelType = ChannelType.Accept;
			Id = id;
			this.socket = socket;
			this.socket.NoDelay = true;
			parser = new PacketParser(recvBuffer, this.service);
			innArgs.Completed += OnComplete;
			outArgs.Completed += OnComplete;

			RemoteAddress = (IPEndPoint)socket.RemoteEndPoint;
			isConnected = true;
			isSending = false;
			
			this.service.Queue.Enqueue(new TcpEventArgs() { Operation = TcpOperation.StartSend, ChannelId = Id});
			this.service.Queue.Enqueue(new TcpEventArgs() { Operation = TcpOperation.StartRecv, ChannelId = Id});
		}
		
		

		public override void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			Log.Information($"channel dispose: {Id} {RemoteAddress} {StatusCode}");
			
			long id = Id;
			Id = 0;
			service.Remove(id);
			socket.Close();
			innArgs.Dispose();
			outArgs.Dispose();
			innArgs = null;
			outArgs = null;
			socket = null;
		}

		public void Send(MemoryBuffer stream)
		{
			if (IsDisposed)
			{
				throw new Exception("TChannel已经被Dispose, 不能发送消息");
			}
			
			switch (service.ServiceType)
			{
				case ServiceType.Internal:
				{
					int messageSize = (int) (stream.Length - stream.Position);
					if (messageSize > ushort.MaxValue * 16)
					{
						throw new Exception($"send packet too large: {stream.Length} {stream.Position}");
					}

					sendCache.WriteTo(0, messageSize);
					sendBuffer.Write(sendCache, 0, PacketParser.InnerPacketSizeLength);
					break;
				}
				case ServiceType.External:
				{
					ushort messageSize = (ushort) (stream.Length - stream.Position);
					sendCache.WriteTo(0, messageSize);
					sendBuffer.Write(sendCache, 0, PacketParser.OuterPacketSizeLength);
					break;
				}
			}
			
			sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
			if (!isSending)
			{
				service.Queue.Enqueue(new TcpEventArgs() { Operation = TcpOperation.StartSend, ChannelId = Id});
			}
			
			service.Recycle(stream);
		}

		public void ConnectAsync()
		{
			outArgs.RemoteEndPoint = RemoteAddress;
			if (socket.ConnectAsync(outArgs))
			{
				return;
			}
			OnConnectComplete(outArgs);
		}

		public void OnConnectComplete(SocketAsyncEventArgs e)
		{
			if (socket == null)
			{
				return;
			}
			
			if (e.SocketError != SocketError.Success)
			{
				OnError((int)e.SocketError);	
				return;
			}

			e.RemoteEndPoint = null;
			isConnected = true;
			
			service.Queue.Enqueue(new TcpEventArgs() { Operation = TcpOperation.StartSend, ChannelId = Id});
			service.Queue.Enqueue(new TcpEventArgs() { Operation = TcpOperation.StartRecv, ChannelId = Id});
		}

		public void OnDisconnectComplete(SocketAsyncEventArgs e)
		{
			OnError((int)e.SocketError);
		}

		public void StartRecv()
		{
			while (true)
			{
				try
				{
					if (socket == null)
					{
						return;
					}
					
					int size = recvBuffer.ChunkSize - recvBuffer.LastIndex;
					innArgs.SetBuffer(recvBuffer.Last, recvBuffer.LastIndex, size);
				}
				catch (Exception e)
				{
					Log.Error($"tchannel error: {Id}\n{e}");
					OnError(StatusCodes.TChannelRecvError);
					return;
				}
			
				if (socket.ReceiveAsync(innArgs))
				{
					return;
				}
				HandleRecv(innArgs);
			}
		}

		public void OnRecvComplete(SocketAsyncEventArgs o)
		{
			HandleRecv(o);
			
			if (socket == null)
			{
				return;
			}
			
			service.Queue.Enqueue(new TcpEventArgs() { Operation = TcpOperation.StartRecv, ChannelId = Id});
		}

		private void HandleRecv(SocketAsyncEventArgs e)
		{
			if (socket == null)
			{
				return;
			}
			if (e.SocketError != SocketError.Success)
			{
				OnError((int)e.SocketError);
				return;
			}

			if (e.BytesTransferred == 0)
			{
				OnError(StatusCodes.PeerDisconnect);
				return;
			}

			recvBuffer.LastIndex += e.BytesTransferred;
			if (recvBuffer.LastIndex == recvBuffer.ChunkSize)
			{
				recvBuffer.AddLast();
				recvBuffer.LastIndex = 0;
			}

			// 收到消息回调
			while (true)
			{
				// 这里循环解析消息执行，有可能，执行消息的过程中断开了session
				if (socket == null)
				{
					return;
				}
				try
				{
					if (recvBuffer.Length == 0)
					{
						break;
					}
					bool ret = parser.Parse(out MemoryBuffer memoryBuffer);
					if (!ret)
					{
						break;
					}
					
					OnRead(memoryBuffer);
				}
				catch (Exception ee)
				{
					Log.Error($"ip: {RemoteAddress} {ee}");
					OnError(StatusCodes.SocketError);
					return;
				}
			}
		}

		public void StartSend()
		{
			if(!isConnected)
			{
				return;
			}

			if (isSending)
			{
				return;
			}
			
			while (true)
			{
				try
				{
					if (socket == null)
					{
						isSending = false;
						return;
					}
					
					// 没有数据需要发送
					if (sendBuffer.Length == 0)
					{
						isSending = false;
						return;
					}

					isSending = true;

					int sendSize = sendBuffer.ChunkSize - sendBuffer.FirstIndex;
					if (sendSize > sendBuffer.Length)
					{
						sendSize = (int)sendBuffer.Length;
					}
					outArgs.SetBuffer(sendBuffer.First, sendBuffer.FirstIndex, sendSize);
					
					if (socket.SendAsync(outArgs))
					{
						return;
					}
				
					HandleSend(outArgs);
				}
				catch (Exception e)
				{
					throw new Exception($"socket set buffer error: {sendBuffer.First.Length}, {sendBuffer.FirstIndex}", e);
				}
			}
		}

		public void OnSendComplete(SocketAsyncEventArgs o)
		{
			HandleSend(o);
			
			isSending = false;
			
			service.Queue.Enqueue(new TcpEventArgs() { Operation = TcpOperation.StartSend, ChannelId = Id});
		}

		private void HandleSend(SocketAsyncEventArgs e)
		{
			if (socket == null)
			{
				return;
			}

			if (e.SocketError != SocketError.Success)
			{
				OnError((int)e.SocketError);
				return;
			}
			
			if (e.BytesTransferred == 0)
			{
				OnError(StatusCodes.PeerDisconnect);
				return;
			}
			
			sendBuffer.FirstIndex += e.BytesTransferred;
			if (sendBuffer.FirstIndex == sendBuffer.ChunkSize)
			{
				sendBuffer.FirstIndex = 0;
				sendBuffer.RemoveFirst();
			}
		}
		
		private void OnRead(MemoryBuffer memoryStream)
		{
			try
			{
				service.ReadCallback(Id, memoryStream);
			}
			catch (Exception exception)
			{
				Log.Error("{exception}",exception);
				OnError(StatusCodes.PacketParserError);
			}
		}

		private void OnError(int error)
		{
			Log.Information($"TChannel OnError: {error} {RemoteAddress}");
			
			long channelId = Id;
			
			service.Remove(channelId);
			
			service.ErrorCallback(channelId, error);
		}
    }
}