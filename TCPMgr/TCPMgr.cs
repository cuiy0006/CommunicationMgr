using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Timers;

namespace TCPManager
{
    public class TCPMgr
    {
        #region Initialize
        private string m_localAddress;
        private string m_machineName;
        private int BufferSize = 1024;
        Logger logger;

        //delegate for returning message
        public delegate void UdpEventNotification(DataItems DIs);
        public event UdpEventNotification RaiseEvent;
        //Key : ip, Value : Socket
        private Dictionary<string, Socket> TcpSocketDic = new Dictionary<string, Socket>();

        public Dictionary<string, Socket> TCPSocketDic
        {
            get
            {
                return TcpSocketDic;
            }
        }

        public TCPMgr()
        {
            m_localAddress = AppParams.LocalAddress;
            m_machineName = AppParams.MachineName;
            logger = new Logger("TCPMgr");

            var timer = new System.Timers.Timer(60 * 1000);
            timer.AutoReset = true;
            timer.Elapsed += TimeAction;
            timer.Start();
        }
        #endregion

        #region Call Handler
        public void CallHandler(DataItems DIs)
        {
            switch (DIs.Name)
            {
                case "UdpConnect":
                    BeginConnect(DIs);
                    break;
                case "ListenToPort":
                    beginListen(DIs);
                    break;
            }
        }
        #endregion

        #region Server
        private void beginListen(DataItems udpDIs)
        {
            Socket socket = null;
            try
            {
                int BuildPort;
                bool IsInt = int.TryParse(udpDIs.Item("Port").Value.ToString(), out BuildPort);
                if (IsInt)
                {
                    IPEndPoint point = new IPEndPoint(IPAddress.Any, BuildPort);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(point);
                    socket.Listen(100);

                    Task Accept = new Task(ListenAccept, socket);
                    Accept.Start();
                }
                else
                {
                    logger.Logline(">>>Error Occur in beginListen: int.TryParse fail");
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in beginListen:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }

        private void ListenAccept(object obj)
        {
            Socket tSocket = null;
            try
            {
                Socket socket = obj as Socket;
                logger.Logline(socket.LocalEndPoint + " start Accept()");
                while (true)
                {
                    tSocket = socket.Accept();

                    lock (TcpSocketDic)
                    {
                        if (TcpSocketDic.ContainsKey(tSocket.RemoteEndPoint.ToString()))
                        {
                            CloseSocket(tSocket);
                            continue;
                        }

                        if (IsContainIpConnection(tSocket.RemoteEndPoint.ToString().Substring(0, tSocket.RemoteEndPoint.ToString().IndexOf(":"))))
                        {
                            CloseSocket(tSocket);
                            continue;
                        }

                        TcpSocketDic.Add(tSocket.RemoteEndPoint.ToString(), tSocket);

                        Task Receive = new Task(ReceiveData, tSocket);
                        Receive.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in beginListen:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }

        }
        #endregion

        #region Client
        private void BeginConnect(DataItems udpDIs)
        {
            try
            {
                string ipAddress = udpDIs.Item("UdpInfo").Children.Item("IPAddress").Value.ToString();
                int port;
                bool res = int.TryParse(udpDIs.Item("UdpInfo").Children.Item("Port").Value.ToString(), out port);
                if (res)
                {
                    lock (TcpSocketDic)
                    {
                        if (TcpSocketDic.ContainsKey(ipAddress + ":" + port))
                            return;

                        if (IsContainIpConnection(ipAddress))
                            return;

                        IPEndPoint point = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        client.Connect(point);
                        TcpSocketDic.Add(ipAddress + ":" + port, client);

                        Task Receive = new Task(ReceiveData, client);
                        Receive.Start();
                    }
                }
                else
                {
                    logger.Logline(">>>Error Occur in BeginConnect: int.TryParse fail");
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in BeginConnect:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }
        #endregion

        #region Send and Receive
        private void ReceiveData(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                ProtocolHandler protocolHandler;
                logger.Logline(client.LocalEndPoint + " start Receive()");
                while (true)
                {
                    byte[] buffer = new byte[BufferSize];
                    int ReceivedBytes = client.Receive(buffer);

                    if (ReceivedBytes == 0)
                    {
                        CloseSocket(client);
                        break;
                    }

                    string msg = Encoding.Unicode.GetString(buffer, 0, ReceivedBytes);
                    logger.Logline(client.LocalEndPoint + " Receive : \n" + msg);

                    Array.Clear(buffer, 0, buffer.Length);

                    protocolHandler = new ProtocolHandler();
                    string[] protocolArray = protocolHandler.GetProtocol(msg);

                    foreach (string protocol in protocolArray)
                    {
                        DataItems DIs = XmlHelper.GetAllElements(protocol);
                        switch (DIs.Name)
                        { 
                            case "SendMessage":
                                DIs.Item(0).Children.AddItem("IPAddress").Value = client.RemoteEndPoint;
                                RaiseEvent(DIs);
                                break;
                            case "FileTransferConnect":
                                Console.WriteLine("Receive from " + client.RemoteEndPoint + " FileTransferConnect Message : \n" + protocol);
                                ReceiveFile(DIs);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in ReceiveData:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
                var Win32ex = ex as Win32Exception;
                if (Win32ex != null)
                    if(Win32ex.ErrorCode == 0x2746 || Win32ex.ErrorCode == 0x2745)
                        CloseSocket(client);
            }
        }

        public void SendMessage(string ipaddress, string body)
        {
            try
            {
                DataItems DIs = XmlHelper.GetAllElements(1);
                DIs.Item(0).Children.Item(0).Value = m_machineName;
                DIs.Item(0).Children.Item(1).Value = body;

                string msg = XmlHelper.GetXml(DIs);
                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                TcpSocketDic[ipaddress].Send(buffer);
                logger.Logline(msg + "\nis sent to " + TcpSocketDic[ipaddress].RemoteEndPoint);
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in SendMessage:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
                var Win32ex = ex as Win32Exception;
                if (Win32ex != null)
                    if (Win32ex.ErrorCode == 0x2746 || Win32ex.ErrorCode == 0x2745)
                        CloseSocket(TcpSocketDic[ipaddress]);
            }
        }

        public void SendFile(string ipaddress, string path)
        {
            FileStream fs = null;
            Socket server = null;
            Socket tSocket = null;
            try
            {
                if (File.Exists(path))
                {
                    int port = Helper.GetAvailablePort();
                    string fileName = Path.GetFileName(path);
                    long fileSize = new FileInfo(path).Length;

                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint point = new IPEndPoint(IPAddress.Any, port);
                    server.Bind(point);
                    server.Listen(10);

                    DataItems DIs = XmlHelper.GetAllElements(2);
                    DIs.Item(0).Children.Item(0).Value = m_machineName;
                    DIs.Item(0).Children.Item(1).Value = port;
                    DIs.Item(0).Children.Item(2).Value = m_localAddress; 
                    DIs.Item(0).Children.Item(3).Value = fileName;
                    DIs.Item(0).Children.Item(4).Value = fileSize;

                    string msg = XmlHelper.GetXml(DIs);
                    logger.Logline(msg + "\nis sent to " + TcpSocketDic[ipaddress].RemoteEndPoint);
                    
                    byte[] buffer = Encoding.Unicode.GetBytes(msg);
                    TcpSocketDic[ipaddress].Send(buffer);

                    tSocket = server.Accept();
                    logger.Logline("FileTransfer Connection is built!");

                    fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    byte[] fileBuffer = new byte[BufferSize];
                    int bytesRead;
                    int sent;
                    long send = 0;
                    while ((bytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length)) > 0)
                    {
                        sent = 0;
                        while ((sent += tSocket.Send(fileBuffer, sent, bytesRead, SocketFlags.None)) < bytesRead)
                        {

                        }
                        send += sent;
                        Console.WriteLine(path + " : " + (int)((double)send / fileSize * 100) + "%");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in SendFile:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
                CloseSocket(server);
                CloseSocket(tSocket);
            }
        }

        private void ReceiveFile(DataItems DIs)
        {
            Socket fileTransSocket = null;
            FileStream fs = null;
            try
            {
                int port;
                bool IsPortInt = int.TryParse(DIs.Item(0).Children.Item("Port").Value.ToString(), out port);
                if (!IsPortInt)
                {
                    logger.Logline(">>>Error Occur in ReceiveFile: int.TryParse fail");
                    return;
                }

                string fileName = DIs.Item(0).Children.Item("FileName").Value.ToString();

                long fileSize;
                bool IsfileSizelong = long.TryParse(DIs.Item(0).Children.Item("FileSize").Value.ToString(), out fileSize);
                if (!IsfileSizelong)
                {
                    logger.Logline(">>>Error Occur in ReceiveFile: long.TryParse fail");
                    return;
                }

                IPAddress ipaddress;
                bool isIPipaddress = IPAddress.TryParse(DIs.Item(0).Children.Item("IPAddress").Value.ToString(), out ipaddress);
                if (!isIPipaddress)
                {
                    logger.Logline(">>>Error Occur in ReceiveFile: IPAddress.TryParse fail");
                    return;
                }

                IPEndPoint endpoint = new IPEndPoint(ipaddress, port);
                fileTransSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                fileTransSocket.Connect(endpoint);

                byte[] fileBuffer = new byte[BufferSize];
                string path = Environment.CurrentDirectory + "\\RemoteFiles\\" + generateFileName(fileName);
                if (!Directory.Exists(Environment.CurrentDirectory + "\\RemoteFiles"))
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\RemoteFiles");

                fs = new FileStream(path, FileMode.Create, FileAccess.Write);

                int bytesRead;
                int Received = 0;
                do
                {
                    bytesRead = fileTransSocket.Receive(fileBuffer);
                    fs.Write(fileBuffer, 0, bytesRead);
                    Received += bytesRead;
                    Console.WriteLine("Receiving " + Received + "(" + fileSize + ")" + " bytes ...");
                } while (bytesRead > 0);

            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in ReceiveFile:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
                CloseSocket(fileTransSocket);
            }
        }
        #endregion

        #region Timer EVENTS
        private void TimeAction(object sender, ElapsedEventArgs e)
        {
            DisconnectSocket();
        } 

        #endregion

        #region Generic
        private void CloseSocket(Socket socket)
        {
            try
            {
                TcpSocketDic.Remove(socket.RemoteEndPoint.ToString());
                if (socket != null)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in CloseSocket:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }

        private void DisconnectSocket()
        {
            try
            {
                var res = TcpSocketDic.Where(x => x.Value.Connected == false).Select(x => x.Key).ToList();
                List<string> lst = new List<string>(res);
                foreach (string key in lst)
                {
                    TcpSocketDic[key].Close();
                    TcpSocketDic.Remove(key);
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in DisconnectSocket:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }

        private bool IsContainIpConnection(string ipaddress)
        {
            bool IsContain = false;
            try
            {
                var res = TcpSocketDic.Where(x => x.Key.Contains(ipaddress)).Select(x => x.Key).ToList();
                IsContain = res.Count() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in IsContainIpConnection:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
            return IsContain;
        }

        private string generateFileName(string fileName)
        {
            DateTime now = DateTime.Now;
            return String.Format(
                "{0}_{1}_{2}_{3}", now.Minute, now.Second, now.Millisecond, fileName
            );
        }
        #endregion
    }
}
