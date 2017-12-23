using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPManager
{
    public class UDPMgr
    {
        private string m_localAddress;
        private string m_machineName;
        private int BufferSize = 1024;
        Logger logger;

        //Client Local Socket
        private Socket m_udpClientSock;

        //Server Local EndPoint
        private IPEndPoint m_udpServerEP;
        //Server Local Socket
        private Socket m_udpServerSock;

        //delegate for returning message
        public delegate void UdpEventNotification(DataItems DIs);
        public event UdpEventNotification RaiseEvent;

        public UDPMgr()
            : this(true, true)
        { 
        
        }

        public UDPMgr(bool IsInitServer, bool IsInitClient)
        {
            m_localAddress = AppParams.LocalAddress;
            m_machineName = AppParams.MachineName;
            logger = new Logger("UDPMgr");
            if (IsInitServer)
                InitializeUdpServer();
            Thread.Sleep(200);
            if (IsInitClient)
                InitializeUdpClient();
        }

        private void InitializeUdpClient()
        {
            try
            {
                Task SendtoServer = new Task(SendBroadCastData);
                SendtoServer.Start();
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in InitializeUdpClient:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }

        private void SendBroadCastData()
        {
            try
            {
                int AvailablePort = Helper.GetAvailablePort();

                DataItems DIs = XmlHelper.GetAllElements(0);
                DIs.Item(0).Children.Item(0).Value = m_machineName;
                DIs.Item(0).Children.Item(1).Value = AvailablePort;
                DIs.Item(0).Children.Item(2).Value = m_localAddress;

                string Message = XmlHelper.GetXml(DIs);
                SendData(Message, AvailablePort);
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in SendBroadCastData:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }

        private void SendData(string Message, int AvailablePort)
        {
            try
            {
                byte[] data = new byte[BufferSize];
                data = Encoding.Unicode.GetBytes(Message);

                //Remote Endpoint
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 8001);

                //Local udp Socket
                m_udpClientSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_udpClientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                //Listen to the port as a Tcp Server
                DataItems DIs = new DataItems();
                DIs.Name = "ListenToPort";
                DIs.AddItem("Port").Value = AvailablePort;
                RaiseEvent(DIs);

                //Send Message to Server
                m_udpClientSock.SendTo(data, data.Length, SocketFlags.None, ip);
                logger.Logline("Send Udp broadcast : \n" + Message + "\n");
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in SendData:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }

        private void InitializeUdpServer()
        {
            try
            {
                m_udpServerEP = new IPEndPoint(IPAddress.Any, 8001);
                m_udpServerSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_udpServerSock.Bind(m_udpServerEP);

                //Listen to Client
                Task ListenToClient = new Task(ReceiveData);
                ListenToClient.Start();
            
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in InitializeUdpServer:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }

        private void ReceiveData()
        {

            //Listen to Client
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            try
            {
                while (true)
                {
                    byte[] buffer = new byte[BufferSize];
                    logger.Logline(m_udpServerSock.LocalEndPoint + " start to ReceiveFrom() (UDP Server)");
                    int bytesReceived = m_udpServerSock.ReceiveFrom(buffer, ref Remote);
                    
                    string remoteIP = Remote.ToString().Substring(0, Remote.ToString().IndexOf(":"));
                    if (m_localAddress == remoteIP)
                        continue;

                    //After received, get 1.buffer data(DIs); 2.Client EndPoint "Remote".
                    string Message = Encoding.Unicode.GetString(buffer, 0, bytesReceived);
                    DataItems DIs = XmlHelper.GetAllElements(Message);

                    if (DIs.Name == "UdpConnect")
                    {
                        logger.Logline(Remote.ToString() + " -> " + m_udpServerSock.LocalEndPoint + "\n" + Message);
                        RaiseEvent(DIs);
                        //Console.WriteLine(DIs.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in ReceiveData:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }

        }

    }
}
