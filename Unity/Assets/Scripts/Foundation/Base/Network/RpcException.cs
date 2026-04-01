using System;

namespace Chaos
{
    public sealed class RpcException : Exception
    {
        public int StatusCode { get; }

        public RpcException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            var statusCode = StatusCode;
            if (StatusCode > StatusCodes.WithException)
            {
                statusCode -= StatusCodes.WithException;
            }

            return $"RpcException: {StatusCode} package: {statusCode / 1000} id: {statusCode % 1000}\n{base.ToString()}";
        }
    }
}