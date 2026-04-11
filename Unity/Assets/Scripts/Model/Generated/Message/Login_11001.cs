using MemoryPack;
using System.Collections.Generic;

namespace Chaos
{
	[MemoryPackable]
	[Message(Opcode.Main2NetClientLoginRequest)]
	[ResponseType(nameof(NetClient2MainLoginResponse))]
	public sealed partial class Main2NetClientLoginRequest : MessageObject, IRequest
	{
		public static Main2NetClientLoginRequest Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Main2NetClientLoginRequest>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public int OwnerFiberId { get; set; }
		[MemoryPackOrder(3)]
		public string Address { get; set; }
		[MemoryPackOrder(4)]
		public string Account { get; set; }
		[MemoryPackOrder(5)]
		public string Password { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			OwnerFiberId = default;
			Address = default;
			Account = default;
			Password = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.NetClient2MainLoginResponse)]
	public sealed partial class NetClient2MainLoginResponse : MessageObject, IResponse
	{
		public static NetClient2MainLoginResponse Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<NetClient2MainLoginResponse>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public int StatusCode { get; set; }
		[MemoryPackOrder(3)]
		public string Message { get; set; }
		[MemoryPackOrder(4)]
		public long PlayerId { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			StatusCode = default;
			Message = default;
			PlayerId = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Client2GatePingRequest)]
	[ResponseType(nameof(Gate2ClientPingResponse))]
	public sealed partial class Client2GatePingRequest : MessageObject, ISessionRequest
	{
		public static Client2GatePingRequest Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Client2GatePingRequest>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Gate2ClientPingResponse)]
	public sealed partial class Gate2ClientPingResponse : MessageObject, ISessionResponse
	{
		public static Gate2ClientPingResponse Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Gate2ClientPingResponse>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public int StatusCode { get; set; }
		[MemoryPackOrder(3)]
		public string Message { get; set; }
		[MemoryPackOrder(4)]
		public long Time { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			StatusCode = default;
			Message = default;
			Time = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Client2RealmLoginRequest)]
	[ResponseType(nameof(Realm2ClientLoginResponse))]
	public sealed partial class Client2RealmLoginRequest : MessageObject, ISessionRequest
	{
		public static Client2RealmLoginRequest Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Client2RealmLoginRequest>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public string Account { get; set; }
		[MemoryPackOrder(3)]
		public string Password { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			Account = default;
			Password = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Realm2ClientLoginResponse)]
	public sealed partial class Realm2ClientLoginResponse : MessageObject, ISessionResponse
	{
		public static Realm2ClientLoginResponse Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Realm2ClientLoginResponse>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public int StatusCode { get; set; }
		[MemoryPackOrder(3)]
		public string Message { get; set; }
		[MemoryPackOrder(4)]
		public string Address { get; set; }
		[MemoryPackOrder(5)]
		public long Token { get; set; }
		[MemoryPackOrder(6)]
		public long GateId { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			StatusCode = default;
			Message = default;
			Address = default;
			Token = default;
			GateId = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Client2GateLoginRequest)]
	[ResponseType(nameof(Gate2ClientLoginResponse))]
	public sealed partial class Client2GateLoginRequest : MessageObject, ISessionRequest
	{
		public static Client2GateLoginRequest Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Client2GateLoginRequest>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public long Token { get; set; }
		[MemoryPackOrder(3)]
		public long GateId { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			Token = default;
			GateId = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Gate2ClientLoginResponse)]
	public sealed partial class Gate2ClientLoginResponse : MessageObject, ISessionResponse
	{
		public static Gate2ClientLoginResponse Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Gate2ClientLoginResponse>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public int StatusCode { get; set; }
		[MemoryPackOrder(3)]
		public string Message { get; set; }
		[MemoryPackOrder(4)]
		public long PlayerId { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			StatusCode = default;
			Message = default;
			PlayerId = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Client2GateLogoutRequest)]
	[ResponseType(nameof(Gate2ClientLogoutResponse))]
	public sealed partial class Client2GateLogoutRequest : MessageObject, ISessionRequest
	{
		public static Client2GateLogoutRequest Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Client2GateLogoutRequest>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Gate2ClientLogoutResponse)]
	public sealed partial class Gate2ClientLogoutResponse : MessageObject, ISessionResponse
	{
		public static Gate2ClientLogoutResponse Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Gate2ClientLogoutResponse>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public int StatusCode { get; set; }
		[MemoryPackOrder(3)]
		public string Message { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			StatusCode = default;
			Message = default;
			ObjectPool.Recycle(this);
		}
	}
	public static partial class Opcode
	{
		public const ushort Main2NetClientLoginRequest = 11001;
		public const ushort NetClient2MainLoginResponse = 11002;
		public const ushort Client2GatePingRequest = 11003;
		public const ushort Gate2ClientPingResponse = 11004;
		public const ushort Client2RealmLoginRequest = 11005;
		public const ushort Realm2ClientLoginResponse = 11006;
		public const ushort Client2GateLoginRequest = 11007;
		public const ushort Gate2ClientLoginResponse = 11008;
		public const ushort Client2GateLogoutRequest = 11009;
		public const ushort Gate2ClientLogoutResponse = 11010;
	}
}
