using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;

namespace DAA
{
    public partial class Form1 : Form
    {
        CookieContainer cookie;
        public Form1()
        {      
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            try
            {
                cookie = new CookieContainer();
                string test = DwarRequest.postRequest("http://w1.dwar.ru/login.php", ref cookie, "email=zadisa2006@mail.ru&passwd=ee34nf3o&x=59&y=17");
                test = DwarRequest.getRequest("http://w1.dwar.ru/area_auction.php", ref cookie);
              //  MessageBox.Show("Авторизован");

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(test);
                HtmlNode filter = doc.DocumentNode.SelectSingleNode("//select[@name='_filter[kind]']");
                HtmlNodeCollection categories = filter.SelectNodes(".//option");
            
                MySqlConnection connection = new MySqlConnection(@"server=localhost;userid=root;password=1547;Database=fordwar;charset=utf8");
                string dbCommand = "CREATE TABLE IF NOT EXISTS categories (browserValue NVARCHAR(10) PRIMARY KEY, categoryName NVARCHAR(50));";
                MySqlCommand command = new MySqlCommand(dbCommand, connection);
                connection.Open();
                command.ExecuteNonQuery();
                //dynamic categories = getCategories(webControl1);
                for (var i = 0; i < categories.Count; i++)
                {
                    command.CommandText = "REPLACE INTO categories (browserValue, categoryName) VALUES(@browserValue,@categoryName)";
                    command.Parameters.AddWithValue("@browserValue", categories[i].GetAttributeValue("value",""));
                    if (command.Parameters[0].Value.ToString().Length < 10 && command.Parameters[0].Value != "")
                    {
                        command.Parameters.AddWithValue("@categoryName", HtmlAgilityPack.HtmlEntity.DeEntitize(categories[i].NextSibling.InnerText).Trim());
                        command.ExecuteNonQuery();
                    }
                    command.Parameters.Clear();
                }
                connection.Close();
                MessageBox.Show("Категории получены");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
                MySqlConnection connection = new MySqlConnection(@"server=localhost;userid=root;password=1547;Database=fordwar;charset=utf8");
                MySqlConnection connection2 = new MySqlConnection(@"server=localhost;userid=root;password=1547;Database=fordwar;charset=utf8");
                MySqlCommand command = new MySqlCommand();

                string html;

                connection.Open();
                connection2.Open();
                command.Connection = connection;
                command.CommandText = "CREATE TABLE IF NOT EXISTS items (lotID NVARCHAR(30) PRIMARY KEY, lotID NVARCHAR(30), itemName NVARCHAR(50), itemCategory NVARCHAR(50), itemDurability NVARCHAR(10), itemTime NVARCHAR(10), itemCount NVARCHAR(10), pricePerOne NVARCHAR(50), itemBid NVARCHAR(50), itemBuyOut NVARCHAR(50));";
                command.ExecuteNonQuery();
                command.CommandText = "SELECT browserValue FROM categories";
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int i = 0;
                    command.Connection = connection2;
                    html = DwarRequest.getRequest("http://w1.dwar.ru/area_auction.php?&_filter%5Btitle%5D=&_filter%5Bcount_min%5D=&_filter%5Bcount_max%5D=&_filter%5Blevel_min%5D=&_filter%5Blevel_max%5D=&_filter%5Bkind%5D=" + reader[0] + "&_filter%5Bquality%5D=-1&_filterapply=%D0%9E%D0%BA&page=" + i, ref cookie);

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    HtmlNode itemList = doc.DocumentNode.SelectSingleNode(".//table[@id='item_list']");
                    if (itemList == null)
                        continue;
                    List<HtmlNode> items = itemList.Descendants("tr").Where(d => d.Attributes.Contains("class") && (d.Attributes["class"].Value.Contains("brd2-top"))).ToList<HtmlNode>();
                    foreach(HtmlNode item in items)
                    {
                        command.CommandText = "REPLACE INTO items (lotID, itemID, itemName, itemCategory, itemStrength, itemTime, itemCount, itemBid, itemBuyOut) VALUES(@lotID, @itemID, @itemName, @itemCategory, @itemStrength, @itemTime, @itemCount, @itemBid, @itemBuyOut)";
                        command.Parameters.AddWithValue("@lotID", (string)item.SelectSingleNode());
                        command.Parameters.AddWithValue("@itemName", (string)record[1]);
                        command.Parameters.AddWithValue("@itemCategory", (string)record[2]);
                        command.Parameters.AddWithValue("@itemStrength", (string)record[3]);
                        command.Parameters.AddWithValue("@itemTime", (string)record[4]);
                        command.Parameters.AddWithValue("@itemCount", (string)record[5]);
                        command.Parameters.AddWithValue("@itemBid", (string)record[6]);
                        command.Parameters.AddWithValue("@itemBuyOut", (string)record[7]);
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
 /*                   if (items.Count!=0)
                    {
                        
                        foreach (HtmlNode item  in items)
                        {
                            command.CommandText = "REPLACE INTO items (itemID, itemName, itemCategory, itemStrength, itemTime, itemCount, itemBid, itemBuyOut) VALUES(@itemID, @itemName, @itemCategory, @itemStrength, @itemTime, @itemCount, @itemBid, @itemBuyOut)";
                            command.Parameters.AddWithValue("@itemID", (string)record[0]);
                            command.Parameters.AddWithValue("@itemName", (string)record[1]);
                            command.Parameters.AddWithValue("@itemCategory", (string)record[2]);
                            command.Parameters.AddWithValue("@itemStrength", (string)record[3]);
                            command.Parameters.AddWithValue("@itemTime", (string)record[4]);
                            command.Parameters.AddWithValue("@itemCount", (string)record[5]);
                            command.Parameters.AddWithValue("@itemBid", (string)record[6]);
                            command.Parameters.AddWithValue("@itemBuyOut", (string)record[7]);
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                        }
                    }

                    JSValue itemData = webControl1.ExecuteJavascriptWithResult(itemsForDb);
                    addItems(itemData, command);

                    JSValue pages = webControl1.ExecuteJavascriptWithResult(pagesNumber);
                    int number = Convert.ToInt32((int)pages);
                    for (; i < number; i++)
                    {
                        webControl1.LoadingFrameComplete += Awesomium_Windows_Forms_WebControl_LoadingFrameComplete;
                        webControl1.Source = new Uri("http://w1.dwar.ru/area_auction.php?&_filter%5Btitle%5D=&_filter%5Bcount_min%5D=&_filter%5Bcount_max%5D=&_filter%5Blevel_min%5D=&_filter%5Blevel_max%5D=&_filter%5Bkind%5D=" + reader[0] + "&_filter%5Bquality%5D=-1&_filterapply=%D0%9E%D0%BA&page=" + i);
                        loadPage();

                        itemData = webControl1.ExecuteJavascriptWithResult(itemsForDb);
                        addItems(itemData, command);
                    }
                    command.Connection = connection;*/
                }
                connection.Close();
                connection2.Close();
                reader.Close();
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.Message);
            //    MessageBox.Show(exception.Data.Values.ToString());

            //}
        }
    }
}
