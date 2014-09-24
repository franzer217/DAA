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
    }
}
