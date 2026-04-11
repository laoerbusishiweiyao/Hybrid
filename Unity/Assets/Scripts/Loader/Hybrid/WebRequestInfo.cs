// using System;
// using System.Threading.Tasks;
//
// namespace Chaos
// {
//     public readonly struct WebRequestInfo
//     {
//         public readonly Type RequestType;
//         private readonly TaskCompletionSource<IResponse> taskCompletionSource;
//
//         public WebRequestInfo(Type requestType)
//         {
//             RequestType = requestType;
//             taskCompletionSource = new TaskCompletionSource<IResponse>();
//         }
//
//         public void SetResult(IResponse response)
//         {
//             taskCompletionSource.SetResult(response);
//         }
//
//         public void SetException(Exception exception)
//         {
//             taskCompletionSource.SetException(exception);
//         }
//
//         public async Task<IResponse> WaitAsync()
//         {
//             return await taskCompletionSource.Task;
//         }
//     }
// }