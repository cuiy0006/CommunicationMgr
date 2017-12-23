using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility;
using KernelProcessor;

namespace UDPTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            //DataItems DIs = XmlHelper.GetAllElements(0);
            //DIs.Item(0).Children.Item(0).Value = "Fuccccc";
            //DIs.Item(0).Children.Item(1).Value = "Fuccccc1";
            //Console.WriteLine(DIs.ToString());

            //string xml = XmlHelper.GetXml(DIs);
            //Console.WriteLine(xml);

            //DataItems DIs1 = XmlHelper.GetAllElements(xml);
            //Console.WriteLine(DIs1.ToString());

            Processor P = new Processor();

            while (true)
            {
                string str;
                str = Console.ReadLine();

                if (str == "image")
                {
                    P.SendFile(@"D:\RMC.png");
                    continue;
                }

                P.SendMsG(str);
            }
        }
    }
}
