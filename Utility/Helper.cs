using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Utility
{
    public static class Helper
    {
        public static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        public static int GetAvailablePort()
        {
            Random rd = new Random();
            int port = rd.Next(1025, 65535);
            while (!IsPortAvaliable(port))
            {
                port = rd.Next(1025, 65535);
            }
            return port;
        }

        public static bool IsPortAvaliable(int port)
        {

            Socket sk = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            bool result = false;
            try
            {
                sk.Bind(new IPEndPoint(IPAddress.Any, port));
                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                sk.Close();
            }

            return result;
        }
    }
}
