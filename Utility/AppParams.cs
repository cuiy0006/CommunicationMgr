using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility
{
    public class AppParams
    {
        private static string m_AppCurrDir = Environment.CurrentDirectory;
        public static string AppCurrDir
        {
            get
            {
                return m_AppCurrDir;
            }
        }

        private static string m_MachineName = Environment.MachineName;
        public static string MachineName
        {
            get
            {
                return m_MachineName;
            }
        }

        private static string m_LocalAddress = Helper.LocalIPAddress();
        public static string LocalAddress
        {
            get
            {
                return m_LocalAddress;
            }
        }
    }
}
