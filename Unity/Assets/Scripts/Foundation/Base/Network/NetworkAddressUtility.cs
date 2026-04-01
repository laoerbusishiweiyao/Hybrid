using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Chaos
{
    public class NetworkAddressUtility
    {
        public static string[] GetAddressIPs()
        {
            var list = new List<string>();
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                {
                    continue;
                }

                foreach (var add in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    list.Add(add.Address.ToString());
                }
            }

            return list.ToArray();
        }

        // 优先获取IPV4的地址
        public static IPAddress GetHostAddress(string hostName)
        {
            var ipAddresses = Dns.GetHostAddresses(hostName);
            IPAddress returnIpAddress = null;
            foreach (var ipAddress in ipAddresses)
            {
                returnIpAddress = ipAddress;
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipAddress;
                }
            }

            return returnIpAddress;
        }

        public static IPEndPoint ToIPEndPoint(string host, int port)
        {
            return new IPEndPoint(IPAddress.Parse(host), port);
        }

        public static IPEndPoint ToIPEndPoint(string address)
        {
            var index = address.LastIndexOf(':');
            var host = address.Substring(0, index);
            var p = address.Substring(index + 1);
            var port = int.Parse(p);
            return ToIPEndPoint(host, port);
        }

        public static int IPStringToInt(string ipString)
        {
            var ipAddress = IPAddress.Parse(ipString);
            var bytes = ipAddress.GetAddressBytes();
            if (bytes.Length != 4)
            {
                throw new ArgumentException("Only IPv4 addresses are supported");
            }

            return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }

        public static string IntToIPString(int ipInt)
        {
            var bytes = new byte[4];
            bytes[0] = (byte)((ipInt >> 24) & 0xFF);
            bytes[1] = (byte)((ipInt >> 16) & 0xFF);
            bytes[2] = (byte)((ipInt >> 8) & 0xFF);
            bytes[3] = (byte)(ipInt & 0xFF);
            return new IPAddress(bytes).ToString();
        }

        public static void SetSioUdpConnReset(Socket socket)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            const uint iocIn = 0x80000000;
            const uint iocVendor = 0x18000000;
            const int sioUDPConnectionReset = unchecked((int)(iocIn | iocVendor | 12));

            socket.IOControl(sioUDPConnectionReset, new[] { Convert.ToByte(false) }, null);
        }
    }
}