using MemoryPack;
using System.Collections.Generic;

namespace Chaos
{
	[MemoryPackable]
	[Message(Opcode.Web2UnityLoadedMessage)]
	public sealed partial class Web2UnityLoadedMessage : MessageObject, IWebMessage
	{
		public static Web2UnityLoadedMessage Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Web2UnityLoadedMessage>(isFromPool);
		}
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Unity2WebLoadedMessage)]
	public sealed partial class Unity2WebLoadedMessage : MessageObject, IWebMessage
	{
		public static Unity2WebLoadedMessage Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Unity2WebLoadedMessage>(isFromPool);
		}
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Wpf2UnityLoadedMessage)]
	public sealed partial class Wpf2UnityLoadedMessage : MessageObject, IWebMessage
	{
		public static Wpf2UnityLoadedMessage Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Wpf2UnityLoadedMessage>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int ProcessId { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			ProcessId = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Unity2WpfFocusChangedMessage)]
	public sealed partial class Unity2WpfFocusChangedMessage : MessageObject, IWebMessage
	{
		public static Unity2WpfFocusChangedMessage Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Unity2WpfFocusChangedMessage>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public bool HasFocus { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			HasFocus = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Web2UnityVersionRequest)]
	[ResponseType(nameof(Unity2WebVersionResponse))]
	public sealed partial class Web2UnityVersionRequest : MessageObject, IWebRequest
	{
		public static Web2UnityVersionRequest Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Web2UnityVersionRequest>(isFromPool);
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
	[Message(Opcode.Unity2WebVersionResponse)]
	public sealed partial class Unity2WebVersionResponse : MessageObject, IWebResponse
	{
		public static Unity2WebVersionResponse Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Unity2WebVersionResponse>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public int StatusCode { get; set; }
		[MemoryPackOrder(3)]
		public string Message { get; set; }
		[MemoryPackOrder(4)]
		public string Version { get; set; }
		[MemoryPackOrder(5)]
		public string UnityVersion { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			StatusCode = default;
			Message = default;
			Version = default;
			UnityVersion = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Unity2WebUserAgentRequest)]
	[ResponseType(nameof(Web2UnityUserAgentResponse))]
	public sealed partial class Unity2WebUserAgentRequest : MessageObject, IWebRequest
	{
		public static Unity2WebUserAgentRequest Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Unity2WebUserAgentRequest>(isFromPool);
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
	[Message(Opcode.Web2UnityUserAgentResponse)]
	public sealed partial class Web2UnityUserAgentResponse : MessageObject, IWebResponse
	{
		public static Web2UnityUserAgentResponse Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Web2UnityUserAgentResponse>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int RequestId { get; set; }
		[MemoryPackOrder(2)]
		public int StatusCode { get; set; }
		[MemoryPackOrder(3)]
		public string Message { get; set; }
		[MemoryPackOrder(4)]
		public string UserAgent { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			RequestId = default;
			StatusCode = default;
			Message = default;
			UserAgent = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Web2UnityTouchDataMessage)]
	public sealed partial class Web2UnityTouchDataMessage : MessageObject, IWebMessage
	{
		public static Web2UnityTouchDataMessage Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Web2UnityTouchDataMessage>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int Id { get; set; }
		[MemoryPackOrder(2)]
		public int Phase { get; set; }
		[MemoryPackOrder(3)]
		public int X { get; set; }
		[MemoryPackOrder(4)]
		public int Y { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			Id = default;
			Phase = default;
			X = default;
			Y = default;
			ObjectPool.Recycle(this);
		}
	}
	[MemoryPackable]
	[Message(Opcode.Web2UnityMouseDataMessage)]
	public sealed partial class Web2UnityMouseDataMessage : MessageObject, IWebMessage
	{
		public static Web2UnityMouseDataMessage Create(bool isFromPool = false)
		{
			return ObjectPool.Rent<Web2UnityMouseDataMessage>(isFromPool);
		}
		[MemoryPackOrder(1)]
		public int Buttons { get; set; }
		[MemoryPackOrder(2)]
		public int X { get; set; }
		[MemoryPackOrder(3)]
		public int Y { get; set; }
		[MemoryPackOrder(4)]
		public int? DeltaX { get; set; }
		[MemoryPackOrder(5)]
		public int? DeltaY { get; set; }
		public override void Dispose()
		{
			if (!IsFromPool)
			{
				return;
			}
			Buttons = default;
			X = default;
			Y = default;
			DeltaX = default;
			DeltaY = default;
			ObjectPool.Recycle(this);
		}
	}
	public static partial class Opcode
	{
		public const ushort Web2UnityLoadedMessage = 10001;
		public const ushort Unity2WebLoadedMessage = 10002;
		public const ushort Wpf2UnityLoadedMessage = 10003;
		public const ushort Unity2WpfFocusChangedMessage = 10004;
		public const ushort Web2UnityVersionRequest = 10005;
		public const ushort Unity2WebVersionResponse = 10006;
		public const ushort Unity2WebUserAgentRequest = 10007;
		public const ushort Web2UnityUserAgentResponse = 10008;
		public const ushort Web2UnityTouchDataMessage = 10009;
		public const ushort Web2UnityMouseDataMessage = 10010;
	}
}
