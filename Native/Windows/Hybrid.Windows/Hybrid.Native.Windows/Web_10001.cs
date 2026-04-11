// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Hybrid.Native.Windows;

[Message(Opcode.Wpf2UnityLoadedMessage)]
public sealed class Wpf2UnityLoadedMessage : MessageObject, IWebMessage
{
	public required int ProcessId { get; set; }
}
[Message(Opcode.Web2UnityLoadedMessage)]
public sealed class Web2UnityLoadedMessage : MessageObject, IWebMessage
{
}
[Message(Opcode.Unity2WebLoadedMessage)]
public sealed class Unity2WebLoadedMessage : MessageObject, IWebMessage
{
}
[Message(Opcode.Unity2WpfFocusChangedMessage)]
public sealed class Unity2WpfFocusChangedMessage : MessageObject, IWebMessage
{
	public required bool HasFocus { get; set; }
}
[Message(Opcode.Unity2WpfShutdownMessage)]
public sealed class Unity2WpfShutdownMessage : MessageObject, IWebMessage
{
}
[Message(Opcode.Web2UnityVersionRequest)]
[ResponseType(nameof(Unity2WebVersionResponse))]
public sealed class Web2UnityVersionRequest : MessageObject, IWebRequest
{
	public required int RequestId { get; set; }
}
[Message(Opcode.Unity2WebVersionResponse)]
public sealed class Unity2WebVersionResponse : MessageObject, IWebResponse
{
	public required int RequestId { get; set; }
	public required int StatusCode { get; set; }
	public required string Message { get; set; }
	public required string Version { get; set; }
	public required string UnityVersion { get; set; }
}
[Message(Opcode.Unity2WebUserAgentRequest)]
[ResponseType(nameof(Web2UnityUserAgentResponse))]
public sealed class Unity2WebUserAgentRequest : MessageObject, IWebRequest
{
	public required int RequestId { get; set; }
}
[Message(Opcode.Web2UnityUserAgentResponse)]
public sealed class Web2UnityUserAgentResponse : MessageObject, IWebResponse
{
	public required int RequestId { get; set; }
	public required int StatusCode { get; set; }
	public required string Message { get; set; }
	public required string UserAgent { get; set; }
}
[Message(Opcode.Web2UnityTouchDataMessage)]
public sealed class Web2UnityTouchDataMessage : MessageObject, IWebMessage
{
	public required int Id { get; set; }
	public required int Phase { get; set; }
	public required int X { get; set; }
	public required int Y { get; set; }
}
[Message(Opcode.Web2UnityMouseDataMessage)]
public sealed class Web2UnityMouseDataMessage : MessageObject, IWebMessage
{
	public required int Buttons { get; set; }
	public required int X { get; set; }
	public required int Y { get; set; }
	public required int? DeltaX { get; set; }
	public required int? DeltaY { get; set; }
}
public static class Opcode
{
	public const ushort Wpf2UnityLoadedMessage = 10001;
	public const ushort Web2UnityLoadedMessage = 10002;
	public const ushort Unity2WebLoadedMessage = 10003;
	public const ushort Unity2WpfFocusChangedMessage = 10004;
	public const ushort Unity2WpfShutdownMessage = 10005;
	public const ushort Web2UnityVersionRequest = 10006;
	public const ushort Unity2WebVersionResponse = 10007;
	public const ushort Unity2WebUserAgentRequest = 10008;
	public const ushort Web2UnityUserAgentResponse = 10009;
	public const ushort Web2UnityTouchDataMessage = 10010;
	public const ushort Web2UnityMouseDataMessage = 10011;
}
