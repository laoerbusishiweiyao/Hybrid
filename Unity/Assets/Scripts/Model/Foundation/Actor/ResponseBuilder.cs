using System;

namespace Chaos
{
    public static class ResponseBuilder
    {
        public static IResponse Build(Type requestType, int requestId, int statusCode)
        {
            var responseType = OpcodeTypeRegistry.Default.GetResponseType(requestType);
            var response = (IResponse)ObjectPool.Rent(responseType);
            response.StatusCode = statusCode;
            response.RequestId = requestId;
            return response;
        }
    }
}