using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utility
{
    public class ProtocolHandler
    {
        private Logger logger = new Logger("ProtocolHandler");

        private string partialProtocal;

        public ProtocolHandler()
        {
            partialProtocal = "";
        }

        public string[] GetProtocol(string input)
        {
            return GetProtocol(input, null);
        }

        public string[] GetProtocol(string msg, List<string> outputList) // Keep msg until xml end
        {
            try
            {
                if (outputList == null)
                    outputList = new List<string>();

                if (string.IsNullOrEmpty(msg))
                    return outputList.ToArray();

                if (!string.IsNullOrEmpty(partialProtocal))
                    msg = partialProtocal + msg;

                string pattern = @"^<CEID[\s\S]+</CEID>";

                if (Regex.IsMatch(msg, pattern))
                {
                    string match = Regex.Match(msg, pattern).Groups[0].Value;
                    outputList.Add(match);
                    partialProtocal = "";

                    msg = msg.Substring(match.Length);
                    GetProtocol(msg, outputList);
                }
                else
                {
                    partialProtocal = msg;
                }
                return outputList.ToArray();
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in GetProtocol:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
            return new string[0];
        }
    }
}
