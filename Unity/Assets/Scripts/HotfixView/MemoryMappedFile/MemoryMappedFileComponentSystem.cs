using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.Json;
using Serilog;
using UnityEngine;

namespace Chaos
{
    [EntitySystemOf(typeof(MemoryMappedFileComponent))]
    public static partial class MemoryMappedFileComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MemoryMappedFileComponent self, string mapName, MemoryMappedFileRole role)
        {
            switch (role)
            {
                case MemoryMappedFileRole.Server:
                {
                    self.File = MemoryMappedFile.CreateNew(mapName, MemoryMappedFileComponent.Size, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, HandleInheritability.None);
                    self.Accessor = self.File.CreateViewAccessor(0, MemoryMappedFileComponent.Size);
                    self.ReaderPipe = new MemoryMappedFilePipe(MemoryMappedFileComponent.BufferSize, self.Accessor);
                    self.WriterPipe = new MemoryMappedFilePipe(0, self.Accessor);
                    break;
                }
                case MemoryMappedFileRole.Client:
                {
                    self.File = MemoryMappedFile.OpenExisting(mapName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
                    self.Accessor = self.File.CreateViewAccessor(0, MemoryMappedFileComponent.Size);
                    self.ReaderPipe = new MemoryMappedFilePipe(0, self.Accessor);
                    self.WriterPipe = new MemoryMappedFilePipe(MemoryMappedFileComponent.BufferSize, self.Accessor);
                    break;
                }
            }

            Log.Information("MemoryMappedFile({role}) {mapName} started", role, mapName);

            var now = TimeInfo.Default.ClientNow();
            self.LastRecvTime = now;
            self.LastSendTime = now;

            self.RequestCallbacks.Clear();
        }

        [EntitySystem]
        private static void Destroy(this MemoryMappedFileComponent self)
        {
            foreach (var info in self.RequestCallbacks.Values.ToArray())
            {
                info.SetException(new RpcException(StatusCodes.Cancel, $"MemoryMappedFile dispose: {self.Id}"));
            }

            self.RequestCallbacks.Clear();

            self.Shutdown();

            self.ProcessId = 0;

            self.SendQueue.Clear();
            self.File?.Dispose();
            self.Accessor?.Dispose();
            self.ReaderPipe?.Dispose();
            self.WriterPipe?.Dispose();
        }

        [EntitySystem]
        public static void Update(this MemoryMappedFileComponent self)
        {
            try
            {
                while (self.ReaderPipe.Read() is MessageObject message)
                {
                    switch (message)
                    {
                        case IWebResponse response:
                        {
                            self.OnResponse(response);
                            break;
                        }
                        case IWebRequest:
                        case IWebMessage:
                        {
                            WebMessageDispatcher.Default.Handle(self.GetParent<WebUiComponent>(), message);
                            break;
                        }
                        default:
                        {
                            Log.Warning("Received unknown message type {type}", message.GetType());
                            break;
                        }
                    }
                }

                var count = self.SendQueue.Count;
                while (count-- > 0)
                {
                    if (!self.SendQueue.TryDequeue(out var info))
                    {
                        break;
                    }

                    self.WriterPipe.Write(info.Opcode, info.Payload);
                }
            }
            catch (Exception exception)
            {
                Log.Error("{s}", exception);
            }
        }

        public static void Initialize(this MemoryMappedFileComponent self, string mapName, string address, string launchFile, bool showDevTools)
        {
            var launchOptions = new LaunchOptions
            {
                ProcessId = Process.GetCurrentProcess().Id,
                Left = ScreenInformation.Left,
                Top = ScreenInformation.Top,
                Width = ScreenInformation.Width,
                Height = ScreenInformation.Height,
                SessionName = mapName,
                SessionRole = MemoryMappedFileRole.Client,
                Address = address,
                ShowDevTools = showDevTools,
            };

#if UNITY_EDITOR
            File.WriteAllText(Path.Combine("../Release/Windows", LaunchOptions.DefaultFilePath), JsonSerializer.Serialize(new { LaunchOptions = launchOptions }, Options.DefaultJsonSerializerOptions));
#else
            File.WriteAllText(LaunchOptions.DefaultFilePath, JsonSerializer.Serialize(new { LaunchOptions = launchOptions }, Options.DefaultJsonSerializerOptions));
#endif

            Application.OpenURL(Path.GetFullPath(launchFile));
        }

        private static void Shutdown(this MemoryMappedFileComponent self)
        {
            if (self.ProcessId is 0)
            {
                Log.Warning("CefSharp Process ID is 0");
                return;
            }

            if (Process.GetProcessById(self.ProcessId) is not { HasExited: false } process)
            {
                return;
            }

            process.CloseMainWindow();
            if (process.WaitForExit(5000))
            {
                return;
            }

            process.Kill();
            process.WaitForExit(1000);
        }

        public static void OnResponse(this MemoryMappedFileComponent self, IResponse response)
        {
            WebUiLogger.Default.Recv(self.Fiber(), response);
            self.LastRecvTime = TimeInfo.Default.ClientNow();
            if (!self.RequestCallbacks.TryGetValue(response.RequestId, out var request))
            {
                return;
            }

            request.SetResult(response);
        }

        public static void Send(this MemoryMappedFileComponent self, IMessage message)
        {
            WebUiLogger.Default.Send(self.Fiber(), message);
            self.LastSendTime = TimeInfo.Default.ClientNow();
            var opcode = OpcodeTypeRegistry.Default.GetOpcode(message.GetType());
            var content = JsonSerializer.Serialize(message, message.GetType(), Options.DefaultJsonSerializerOptions);
            var payload = Encoding.UTF8.GetBytes(content);
            self.SendQueue.Enqueue((opcode, payload));
        }

        public static async ThreadTask<IResponse> SendAsync(this MemoryMappedFileComponent self, IRequest request)
        {
            var requestId = ++self.RequestId;
            var requestInfo = new RequestInfo(request.GetType());
            self.RequestCallbacks[requestId] = requestInfo;

            request.RequestId = requestId;

            self.Send(request);

            void CancelAction()
            {
                if (!self.RequestCallbacks.Remove(requestId, out var info))
                {
                    return;
                }

                var responseType = OpcodeTypeRegistry.Default.GetResponseType(info.RequestType);
                var response = (IResponse)Activator.CreateInstance(responseType);
                response.StatusCode = StatusCodes.Cancel;
                info.SetResult(response);
            }

            var cancelSignal = await ThreadTask.GetContextAsync<CancelSignal>();
            IResponse result;
            try
            {
                cancelSignal?.Add(CancelAction);
                result = await requestInfo.WaitAsync();
            }
            finally
            {
                cancelSignal?.Remove(CancelAction);
            }

            return result;
        }
    }
}