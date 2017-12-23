using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Utility
{
    public class XmlHelper
    {
        private static string XmlPath = AppParams.AppCurrDir + @"\Protocol.xml";

        private static Logger logger = new Logger("XmlHelper");

        private static XDocument xdoc;

        static XmlHelper()
        {
            try
            {
                xdoc = XDocument.Load(XmlPath);
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in XmlHelper:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
        }

        public static DataItems GetAllElements(int id)
        {
            DataItems DIs = new DataItems();
            try
            {
                var query = from item in xdoc.Element("Protocol").Elements("CEID")
                            where item.Attribute("id").Value == id.ToString()
                            select item;

                DIs = ExtractElements(query);
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in GetAllElements:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }

            return DIs;
        }

        public static DataItems GetAllElements(string xmlString)
        {
            DataItems DIs = new DataItems();
            try
            {
                XElement xe = XElement.Parse(xmlString);

                DIs = ExtractElements(new[]{xe});
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in GetAllElements:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
            return DIs;
        }

        private static DataItems ExtractElements(IEnumerable<XElement> query)
        {
            DataItems DIs = new DataItems();
            try
            {
                foreach (XElement item in query)
                {
                    DIs.Name = item.Attribute("logicalName").Value;

                    var rptQuery = from rptitem in item.Elements("RPTID")
                                   select rptitem;

                    foreach (XElement rptitem in rptQuery)
                    {
                        string rptName = rptitem.Attribute("logicalName").Value;
                        DIs.AddItem(rptName);

                        var svQuery = from svitem in rptitem.Elements("ReportVariable")
                                      select svitem;
                        foreach (XElement svitem in svQuery)
                        {
                            string svName = svitem.Attribute("logicalName").Value;
                            string svValue = svitem.Value.ToString();
                            DIs.Item(rptName).Children.AddItem(svName).Value = svValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in ExtractElements:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
            return DIs;
        }

        public static string GetXml(DataItems DIs)
        {
            try
            {
                ////XElement xe = new XElement("Company", new XAttribute("MyName", "C Y"), new XElement("CompanyName", "ASM"), new XElement("CompanyAddress", new XElement("Country", "Singapore")));
                ////Console.WriteLine(xe.ToString());
                string ceName = DIs.Name;
                XElement xe = new XElement("CEID");
                xe.Add(new XAttribute("logicalName",ceName));

                for (int i = 0; i < DIs.Count(); i++)
                {
                    DataItem rptLayer = DIs.Item(i);
                    string rptName = rptLayer.Name;
                    xe.Add(new XElement("RPTID", new XAttribute("logicalName", rptName)));

                    for (int j = 0; j < rptLayer.Children.Count(); j++)
                    {
                        DataItem svLayer = rptLayer.Children.Item(j);
                        string svName = svLayer.Name;
                        string svValue = svLayer.Value.ToString();

                        var query = from item in xe.Elements("RPTID")
                                    where item.Attribute("logicalName").Value == rptName
                                    select item;

                        foreach (XElement rptitem in query)
                        { 
                            rptitem.Add(new XElement("ReportVariable",svValue,new XAttribute("logicalName",svName)));
                        }
                    }
                }
                return xe.ToString();
            }
            catch (Exception ex)
            {
                logger.Logline(">>>Error Occur in GetXml:" + ex.Message + "\nStack Trace:\n" + ex.StackTrace + "\n");
            }
            return "";
        }

    }
}
