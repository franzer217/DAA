using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using workWithXmlByRusskijMir;

namespace DAA
{
    class globals
    {
        public static string xmlFilePath = Application.StartupPath + "\\DAA.xml";
        public static Logger dwarLog = LogManager.GetCurrentClassLogger();
        public static int expectedAuctionScanningTime = Convert.ToInt32(XmlLib.getFromXml(xmlFilePath, "int", "expectedAuctionScanningTime"));
        public static int threadStartDuration = Convert.ToInt32(XmlLib.getFromXml(xmlFilePath, "int", "threadStartDuration"));
        public static int statisticsProcessingPeriod = Convert.ToInt32(XmlLib.getFromXml(xmlFilePath, "int", "statisticsProcessingPeriod")); 
        public static string gameWorld = XmlLib.getFromXml(xmlFilePath, "string", "gameWorld");
        public static string email = XmlLib.getFromXml(xmlFilePath, "string", "email");
        public static string password = XmlLib.getFromXml(xmlFilePath, "string", "password");
        public static string connectionString = @"server=localhost;userid=root;password=1547;Database=fordwar;charset=utf8;pooling=false";      
    }
}
