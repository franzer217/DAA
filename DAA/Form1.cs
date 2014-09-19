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
using System.Threading;

//Дата первого обнаружения, дата выхода лота с аукциона, выкупили/вышло по таймауту, времени до выхода с аукциона, наличие, дата текущего обнаружения(Если поле меньше текущей даты на n минут - предмета нет)
//Сделать нормальные типы данных у столбцов таблиц
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
                command.CommandText = "CREATE TABLE IF NOT EXISTS items (lotID NVARCHAR(30) PRIMARY KEY, itemName NVARCHAR(50), itemCategory NVARCHAR(50), itemStrength NVARCHAR(10), itemCount NVARCHAR(10), pricePerPiece NVARCHAR(50), itemBid NVARCHAR(50), itemBuyOut NVARCHAR(50));";
                command.ExecuteNonQuery();
                command.CommandText = "SELECT browserValue FROM categories";
                MySqlDataReader reader = command.ExecuteReader();
                command.CommandText = "REPLACE INTO items (lotID, itemName, itemCategory, itemStrength, itemCount, pricePerPiece, itemBid, itemBuyOut) VALUES(@lotID, @itemName, @itemCategory, @itemStrength, @itemCount, @pricePerPiece, @itemBid, @itemBuyOut)";
                while (reader.Read())
                {
                    command.Connection = connection2;
                    html = DwarRequest.getRequest("http://w1.dwar.ru/area_auction.php?&_filter%5Btitle%5D=&_filter%5Bcount_min%5D=&_filter%5Bcount_max%5D=&_filter%5Blevel_min%5D=&_filter%5Blevel_max%5D=&_filter%5Bkind%5D=" + reader[0] + "&_filter%5Bquality%5D=-1&_filterapply=%D0%9E%D0%BA&page=0", ref cookie);

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    List<HtmlNode> items = dwarAPI.getItemNodes(doc);
                    if (items == null)
                        continue;
                    int number = dwarAPI.pageCount(doc);

                    dwarAPI.addItems(items, command);

                    for (int i = 1; i < number; i++)
                    {
                        html = DwarRequest.getRequest("http://w1.dwar.ru/area_auction.php?&_filter%5Btitle%5D=&_filter%5Bcount_min%5D=&_filter%5Bcount_max%5D=&_filter%5Blevel_min%5D=&_filter%5Blevel_max%5D=&_filter%5Bkind%5D=" + reader[0] + "&_filter%5Bquality%5D=-1&_filterapply=%D0%9E%D0%BA&page=" + i, ref cookie);
                        doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);

                        items = dwarAPI.getItemNodes(doc);
                        dwarAPI.addItems(items, command);
                    }
                    command.Connection = connection;
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
