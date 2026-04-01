using System.Net;

namespace Chaos
{
    public static class EndPointExtensions
    {
        public static IPEndPoint Clone(this EndPoint point)
        {
            var ip = (IPEndPoint)point;
            ip = new IPEndPoint(ip.Address, ip.Port);
            return ip;
        }
    }
}