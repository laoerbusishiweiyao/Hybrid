// using System;
// using System.Threading.Tasks;
// using UnityEngine;
//
// namespace Chaos
// {
//     public sealed class UnityInformationRequest : MessageObject, IWebRequest
//     {
//         public int RequestId { get; set; }
//     }
//
//     public sealed class UnityInformationResponse : MessageObject, IWebResponse
//     {
//         public int RequestId { get; set; }
//         public int StatusCode { get; set; }
//         public string Message { get; set; }
//
//         public string Version { get; set; }
//         public string UnityVersion { get; set; }
//     }
//
//     public sealed class UnityInformationRequestHandler : WebMessageHandler<UnityInformationRequest, UnityInformationResponse>
//     {
//         protected override Task RunAsync(UnityInformationRequest request, UnityInformationResponse response)
//         {
//             response.Version = Application.version;
//             response.UnityVersion = Application.unityVersion;
//             return Task.CompletedTask;
//         }
//     }
// }