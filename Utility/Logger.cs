using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utility
{
    public class Logger
    {
        private object StreamWriterLock;
        private string m_PathName;
        public Logger(string PathName)
        {
            StreamWriterLock = new object();
            m_PathName = PathName;
        }

        public void Logline(string content)
        {
            try
            {
                DateTime dt = DateTime.Now;
                string Header = String.Format("{0:yyyy'-'MM'-'dd}", dt);

                string Path = AppParams.AppCurrDir + "\\Log\\" + m_PathName;
                if (File.Exists(Path))
                    File.Delete(Path);
                if (!Directory.Exists(Path))
                    Directory.CreateDirectory(Path);
                lock (StreamWriterLock)
                {
                    StreamWriter writer = new StreamWriter(Path + "\\" + Header + ".txt", true);
                    try
                    {
                        writer.WriteLine(DateTime.Now + "\t" + content);
                    }
                    finally
                    {
                        writer.Close();
                    }
                }    
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + "\t>>>Error Occur in Logline:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }
    }
}
