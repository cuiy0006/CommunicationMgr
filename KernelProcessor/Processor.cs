using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UDPManager;
using Utility;
using TCPManager;

namespace KernelProcessor
{
    public class Processor
    {
        private UDPMgr m_UDPMgr;
        private TCPMgr m_TCPMgr;

        public Processor()
        {
            m_TCPMgr = new TCPMgr();
            m_TCPMgr.RaiseEvent += new TCPMgr.UdpEventNotification(HandleTcpEvent);
            m_UDPMgr = new UDPMgr();
            m_UDPMgr.RaiseEvent += new UDPMgr.UdpEventNotification(HandleUdpEvent);
            
        }

        private void HandleUdpEvent(DataItems DIs)
        {
            m_TCPMgr.CallHandler(DIs);
        }

        private void HandleTcpEvent(DataItems DIs)
        {
            Console.WriteLine("Received from " + DIs.Item(0).Children.Item("IPAddress").Value.ToString() + " : " + DIs.Item(0).Children.Item("Body").Value.ToString());
        }

        public void SendMsG(string msg)
        {
            foreach (string key in m_TCPMgr.TCPSocketDic.Keys)
                m_TCPMgr.SendMessage(key, msg);
        }

        public void SendFile(string path)
        {
            foreach (string key in m_TCPMgr.TCPSocketDic.Keys)
                m_TCPMgr.SendFile(key, path);
        }


    }
}
