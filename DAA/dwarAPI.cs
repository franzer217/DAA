using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using workWithXmlByRusskijMir;

namespace DAA
{ 
    public static class DwarAPI
    {
        public static CookieContainer cookie = new CookieContainer();
        private static string getMoney(HtmlNode node)
        {
            try
            {
                string money = "";
                string gold = HtmlEntity.DeEntitize(node.SelectSingleNode("descendant::span[@class='mgold']").InnerText).TrimStart();
                string silver = HtmlEntity.DeEntitize(node.SelectSingleNode("descendant::span[@class='msilver']").InnerText).TrimStart();
                string bronze = HtmlEntity.DeEntitize(node.SelectSingleNode("descendant::span[@class='mbronze']").InnerText).TrimStart();
                if (gold != "")
                    money += gold;
                if (silver != "")
                    if (silver.Length > 1)
                        money += silver;
                    else
                        if (gold == "")
                            money += silver;
                        else
                            money += '0' + silver;
                if (bronze.Length > 1)
                    money += bronze;
                else
                    money += '0' + bronze;
                return money;
            }
            catch (NullReferenceException e)
            {
                return "";
            }
        }

        private static string getItemPrice(HtmlNode itemPriceNode)
        {
            HtmlNodeCollection spans = itemPriceNode.SelectNodes("span");
            string price = itemPriceNode.InnerText.Replace("&nbsp;", " ").TrimStart();

            string result = parsePriceRecursion(spans[0].Attributes["title"].Value, price);
            return parsePrice(result);
        } 

        private static string parsePriceRecursion(string coinType, string price)
        {
            string result = " ";
            switch (coinType)
            {
                case "Золотой":
                    if(price.IndexOf(' ') != -1)
                        result = price.Substring(0, price.IndexOf(' ')) + 'з';
                    else
                    {
                        result = price.Substring(0, price.Length) + 'з';
                        return result;
                    } 
                    price = price.Remove(0, result.Length);
                    result += parsePriceRecursion("Серебряный", price);
                    return result;

                case "Серебряный":
                    price = price.TrimStart();
                    if(price.IndexOf(' ') != -1)
                        result = price.Substring(0, price.IndexOf(' ')) + 'с';
                    else
                    {
                        result = price.Substring(0, price.Length) + 'с';
                        return result;
                    } 
                    price = price.Remove(0, result.Length);
                    result += parsePriceRecursion("Медный", price);
                    return result;

                case "Медный":
                    price = price.TrimStart();
                    result = price.Substring(0, price.Length) + 'м';
                    return result;

                default:
                    return "";

            }
        }

        private static string parsePrice(string price)
        {
            string money = "";
            string gold = "";
            string silver = "";
            string bronze = "";
            if(price.IndexOf('з') != -1)
            {
                gold = price.Substring(0, price.IndexOf('з'));
                price = price.Remove(0, gold.Length + 1);
            }
            if(price.IndexOf('с') != -1)
            {
                silver = price.Substring(0, price.IndexOf('с'));
                price = price.Remove(0, silver.Length + 1);
            } 
            if(price.IndexOf('м') != -1)
            {
                bronze = price.Substring(0, price.IndexOf('м'));                
            } 
            if (gold != "")
                money += gold;
            if (silver != "")
                if (silver.Length > 1)
                    money += silver;
                else
                    if (gold == "")
                        money += silver;
                    else
                        money += '0' + silver;
            else
                if(gold != "")
                    money += "00";               
            if (bronze != "")
                if (bronze.Length > 1)
                    money += bronze;
                else
                    if (gold == "" || silver == "")
                        money += bronze;
                    else             
                        money += '0' + bronze;
            else
                money += "00";
            return money;
        }

        private static int pageCount(HtmlAgilityPack.HtmlDocument doc)
        {
            try
            {
                return Convert.ToInt32(doc.DocumentNode.SelectSingleNode("(.//a[@class='pg-inact_lnk'])[last()]").InnerText);
            }
            catch (NullReferenceException)
            {
                return 0;
            }
        }
        public static void login()
        {
            if(DwarRequest.postRequest("http://w1.dwar.ru/login.php", ref cookie, "email=" + globals.email + "&passwd=" + globals.password)!="")
            {
                MessageBox.Show("Авторизация прошла успешно");
                globals.dwarLog.Trace("--------------------------------------------------------------------------------");
                globals.dwarLog.Trace("Авторизация прошла успешно");
            }
            else
            {
                MessageBox.Show("Ошибка авторизации");
                globals.dwarLog.Trace("--------------------------------------------------------------------------------");
                globals.dwarLog.Trace("Ошибка авторизации");
            }
        }

        public static void getCategories()
        {
            MySqlConnection connection = new MySqlConnection(globals.connectionString);
            string dbCommand = "CREATE TABLE IF NOT EXISTS categories (browserValue NVARCHAR(10) PRIMARY KEY, categoryName NVARCHAR(50));";
            try
            {
                string html = DwarRequest.getRequest("http://w1.dwar.ru/area_auction.php", ref cookie);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode filter = doc.DocumentNode.SelectSingleNode("//select[@name='_filter[kind]']");
                HtmlNodeCollection categories = filter.SelectNodes(".//option");                
                MySqlCommand command = new MySqlCommand(dbCommand, connection);
                connection.Open();
                command.ExecuteNonQuery();

                for (var i = 0; i < categories.Count; i++)
                {
                    command.CommandText = "REPLACE INTO categories (browserValue, categoryName) VALUES(@browserValue,@categoryName)";
                    command.Parameters.AddWithValue("@browserValue", categories[i].GetAttributeValue("value", ""));
                    if (command.Parameters[0].Value.ToString().Length < 10 && command.Parameters[0].Value != "")
                    {
                        command.Parameters.AddWithValue("@categoryName", HtmlAgilityPack.HtmlEntity.DeEntitize(categories[i].NextSibling.InnerText).Trim());
                        command.ExecuteNonQuery();
                    }
                    command.Parameters.Clear();
                }
                MessageBox.Show("Категории получены");
                globals.dwarLog.Trace("Категории получены");
            }
            catch (Exception exception)
            {
                globals.dwarLog.Error(exception.ToString());
                MessageBox.Show(exception.Message);
            }
            finally
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
        }
        private static void addItems(List<HtmlNode> nodes, MySqlCommand command)
        {
            DateTime localDateTime;
            DateTime lotExpiration;
            string localDateTimeStr;
            foreach (HtmlNode item in nodes)
            {
                try
                {
                localDateTime = DateTime.UtcNow.ToUniversalTime();
                localDateTimeStr = localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                lotExpiration = lotEndTime(item, localDateTime);
                command.CommandText += "('" + Convert.ToInt64(item.SelectSingleNode("td[7]/descendant::input[2]").GetAttributeValue("aid", "")) + "','" + item.SelectSingleNode("td[2]/a").InnerText + "','"
                + item.SelectSingleNode("td[2]/span[1]").InnerText.TrimStart() + "','" + item.SelectSingleNode("td[2]/span[2]").InnerText.Remove(0, item.SelectSingleNode("td[2]/span[2]").InnerText.IndexOf('/') + 1) + "','" + item.SelectSingleNode("td[5]").InnerText + "','"
                + DwarAPI.getMoney(item.SelectSingleNode("td[6]")) + "','" + DwarAPI.getMoney(item.SelectSingleNode("td[7]")) + "','" + DwarAPI.getMoney(item.SelectSingleNode("td[8]")) + "','"
                + localDateTimeStr + "','" + lotExpiration.ToString("yyyy-MM-dd HH:mm:ss") + "','" + localDateTimeStr + "','"
                + (int)(lotExpiration - localDateTime).TotalSeconds + "','" + "1" + "'),";
                }
                catch (NullReferenceException exception)
                {
                   // globals.dwarLog.Error(exception.Message + " " + exception.StackTrace + " " + Thread.CurrentThread.ManagedThreadId);
                }
            }
        }

        private static List<HtmlNode> getItemNodes(HtmlAgilityPack.HtmlDocument doc)
        {
            HtmlNode itemList = doc.DocumentNode.SelectSingleNode(".//table[@id='item_list']");
            if (itemList != null)
                return itemList.Descendants("tr").Where(d => d.Attributes.Contains("class") && (d.Attributes["class"].Value.Contains("brd2-top"))).ToList<HtmlNode>();
            return null;
        }

        public static void scanItems()
        {
            globals.dwarLog.Trace("Сканирование началось; " + "ThreadID = " + Thread.CurrentThread.ManagedThreadId);
            MySqlConnection connection = new MySqlConnection(globals.connectionString);
            MySqlConnection connection2 = new MySqlConnection(globals.connectionString);
            MySqlDataReader reader = null;
            try
            {                                               
                MySqlCommand command = new MySqlCommand();
                MySqlCommand command2 = new MySqlCommand();

                string html;

                connection.Open();
                connection2.Open();
                command.Connection = connection;
                command2.Connection = connection2;
                command2.CommandText = "CREATE TABLE IF NOT EXISTS items (lotID BIGINT PRIMARY KEY, itemName NVARCHAR(50), itemCategory NVARCHAR(50), itemStrength INT(3), itemCount NVARCHAR(10), pricePerPiece NVARCHAR(50), itemBid NVARCHAR(50), itemBuyOut NVARCHAR(50), detectionTime DATETIME, lotEndTime DATETIME, lastUpdateTime DATETIME, secondsLeft INT, buyedOut TINYINT, subsists TINYINT);";
                command2.ExecuteNonQuery();
                command.CommandText = "SELECT browserValue FROM categories";
                reader = command.ExecuteReader();
                command2.CommandText = "INSERT INTO items (lotID, itemName, itemCategory, itemStrength, itemCount, pricePerPiece, itemBid, itemBuyOut, detectionTime, lotEndTime, lastUpdateTime, secondsLeft, subsists) VALUES";

                while (reader.Read())
                {
                    html = DwarRequest.getRequest("http://" + globals.gameWorld + ".dwar.ru/area_auction.php?&_filter%5Btitle%5D=&_filter%5Bcount_min%5D=&_filter%5Bcount_max%5D=&_filter%5Blevel_min%5D=&_filter%5Blevel_max%5D=&_filter%5Bkind%5D=" + reader[0] + "&_filter%5Bquality%5D=-1&_filterapply=%D0%9E%D0%BA&page=0", ref cookie);
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    List<HtmlNode> items = getItemNodes(doc);
                    if (items == null)
                        continue;
                    int number = DwarAPI.pageCount(doc);

                    addItems(items, command2);

                    for (int i = 1; i < number; i++)
                    {
                        html = DwarRequest.getRequest("http://" + globals.gameWorld +".dwar.ru/area_auction.php?&_filter%5Btitle%5D=&_filter%5Bcount_min%5D=&_filter%5Bcount_max%5D=&_filter%5Blevel_min%5D=&_filter%5Blevel_max%5D=&_filter%5Bkind%5D=" + reader[0] + "&_filter%5Bquality%5D=-1&_filterapply=%D0%9E%D0%BA&page=" + i, ref cookie);
                        doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);

                        items = getItemNodes(doc);
                        addItems(items, command2);
                    }
                }
                connection.Close();
                reader.Close();
                command2.CommandText = command2.CommandText.TrimEnd(',') + " ON DUPLICATE KEY UPDATE itemBid = VALUES(itemBid), lastUpdateTime = VALUES(lastUpdateTime), secondsLeft = TIMESTAMPDIFF(SECOND,'" + DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + "',lotEndTime);";
                command2.ExecuteNonQuery();
                command2.CommandText = "UPDATE items SET buyedOut = IF ('" + DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + "'>DATE_ADD(lotEndTime, INTERVAL " + globals.expectedAuctionScanningTime + " MINUTE),'2',IF('" + DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + "'>DATE_ADD(lastUpdateTime, INTERVAL " + globals.expectedAuctionScanningTime + " MINUTE),'1','0')) WHERE subsists='1'; UPDATE items SET subsists = '0' WHERE DATE_ADD(lastUpdateTime, INTERVAL " + globals.expectedAuctionScanningTime + " MINUTE)<'" + DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + "';";
                command2.ExecuteNonQuery();     
                globals.dwarLog.Trace("Сканирование завершено; " + "ThreadID = " + Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception exception)
            {
                globals.dwarLog.Error(exception.Message + " " + exception.StackTrace + " " + Thread.CurrentThread.ManagedThreadId);
                MessageBox.Show(exception.Message + " " + exception.StackTrace + " " + Thread.CurrentThread.ManagedThreadId);
            }
            finally
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
                if (connection2.State != System.Data.ConnectionState.Closed)
                    connection2.Close();
                if (reader != null && reader.IsClosed == false)
                    reader.Close();

            }
        }
        private static DateTime lotEndTime(HtmlNode node, DateTime currentTime)
        {
            string timeLeft = node.SelectSingleNode("td[3]").InnerText;
            switch(timeLeft)
            {
                case "Мало":
                    return currentTime.AddHours(2.0);
                case "Средне":
                    return currentTime.AddHours(8.0);
                case "Много":
                    return currentTime.AddDays(1.0);
                default:
                    return DateTime.Now;
            }
        }

        public static void startNewThread()
        {
            try
            {
                while(true)
                {               
                    Thread myThread = new Thread(scanItems);
                    myThread.Start();
                    Thread.Sleep(globals.threadStartDuration);
                }         
            }
            catch (Exception exception)
            {
                globals.dwarLog.Error(exception.ToString());
                MessageBox.Show(exception.ToString());
            }
        }
        public static bool pageStatus(string URL)
        {
            string html = DwarRequest.getRequest(URL, ref cookie);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode page = doc.DocumentNode.SelectSingleNode("//html");
            if (page != null)
                return true;
            else
                return false;
        }
        public static void getAllItems()
        {
            int i = 1;
            MySqlConnection connection = new MySqlConnection(globals.connectionString);
            MySqlDataReader reader = null;
            try
            {
                string dbCommand = "CREATE TABLE IF NOT EXISTS allItems (itemID INT PRIMARY KEY, itemName NVARCHAR(50), itemPrice NVARCHAR(50), itemBaseStrength INT(3));";
                MySqlCommand command = new MySqlCommand(dbCommand, connection);
                connection.Open();
                command.ExecuteNonQuery();

                List<string> categories = new List<string>();
                command.CommandText = "SELECT categoryName FROM categories";
                reader = command.ExecuteReader();
                while(reader.Read())
                {
                    categories.Add(reader[0].ToString());
                }

                string html;

                command.CommandText = "REPLACE INTO allItems (itemID, itemName, itemPrice, itemBaseStrength) VALUES";
                int pageCount = Convert.ToInt32(XmlLib.getFromXml(globals.xmlFilePath, "int", "gameElementsNumber"));
                HtmlAgilityPack.HtmlDocument doc = null;
                HtmlNode itemPriceNode = null;               
                HtmlNode itemStrengthNode = null;
                while(i < pageCount)
                {
                    html = DwarRequest.getRequest("http://w1.dwar.ru/artifact_info.php?artikul_id=" + i, ref cookie);
                    doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    itemPriceNode = doc.DocumentNode.SelectSingleNode("//*[@title='Цена']");
                    itemStrengthNode = doc.DocumentNode.SelectSingleNode("//*[@title='Прочность предмета']");

                    if (pageStatus("http://w1.dwar.ru/artifact_info.php?artikul_id=" + i) && ((itemPriceNode != null)&&(itemPriceNode.SelectNodes("span")!=null)) && categories.Contains(doc.DocumentNode.SelectSingleNode("//*[@title='Тип предмета']").InnerText) && doc.DocumentNode.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("redd")) != null)
                    {
                        string nameItem = doc.DocumentNode.SelectNodes("//h1")[1].InnerText;
                        if(nameItem.Length<51)
                            command.CommandText += "('" + i + "','" + MySqlHelper.EscapeString(nameItem) + "','" + getItemPrice(itemPriceNode) + "',"; 
                        if(itemStrengthNode != null)
                        {
                            command.CommandText += "'" + itemStrengthNode.InnerText.Remove(0, itemStrengthNode.InnerText.IndexOf('/') + 1) + "'";
                        }
                        else
                        {
                            command.CommandText += "NULL";
                        }
                        command.CommandText += "),";
                    }

                    i++;
                }
                reader.Close();
                command.CommandText = command.CommandText.TrimEnd(',')+";";
                command.ExecuteNonQuery();
                MessageBox.Show("Предметы получены");
                globals.dwarLog.Trace("Предметы получены");
            }
            catch (Exception exception)
            {
                globals.dwarLog.Error(exception.ToString() + "i=" + i);
                MessageBox.Show(exception.Message);
            }
            finally
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
                if (reader != null && reader.IsClosed == false)
                    reader.Close();
            }
        }
    }
}

