using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace tracert
{
    class Tracert
    {
        public List<ResultSet> Run(IPAddress ip, int hop, int timeout)
        {
            /*
            int hop = 30;
            int timeout = 1000;
            */
            List<ResultSet> resultSet;

            PingAction ping = new PingAction(hop);
            ping.Run(ip, timeout);
            resultSet = ping.Disp();
            ping.Dispose();
            return resultSet;
        }
    }

    class GetIPAddr
    {
        public static IPAddress GetIPAddress(string target)
        {
            IPAddress ip = ParseIPAddress(target);
            if (ip == null)
            {
                ip = ResolveDomain(target);
            }
            return ip;
        }

        private static IPAddress ResolveDomain(string target)
        {
            IPAddress address = null;
            try
            {
                IPHostEntry iphe = Dns.GetHostEntry(target);
                foreach (var o in iphe.AddressList)
                {
                    if (o.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        address = new IPAddress(o.GetAddressBytes());
                        break;
                    }
                }
            }
            catch
            {
                address = null;
            }
            return address;
        }

        private static IPAddress ParseIPAddress(string target)
        {
            IPAddress address;
            if (IPAddress.TryParse(target, out address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        return address;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        return null;
                    default: return null;
                }
            }
            return null;
        }
    }
}
