using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DAA
{
    public static class dwarAPI
    {
        public static string getMoney(HtmlNode node)
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
                        if(gold == "")
                            money += silver;
                        else
                            money += '0' + silver;
                if (bronze.Length > 1)
                    money += bronze;
                else
                    money += '0' + bronze;
                return money;
            }
            catch(NullReferenceException e)
            {
                return "";
            }     
        }

        public static int pageCount(HtmlAgilityPack.HtmlDocument doc)
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

        public static void addItems(List<HtmlNode> nodes, MySqlCommand command)
        {
            foreach (HtmlNode item in nodes)
            {   try
                {
                    command.Parameters.AddWithValue("@lotID", item.SelectSingleNode("td[7]/descendant::input[2]").GetAttributeValue("aid", ""));
                    command.Parameters.AddWithValue("@itemName", item.SelectSingleNode("td[2]/a").InnerText);
                    command.Parameters.AddWithValue("@itemCategory", item.SelectSingleNode("td[2]/span[1]").InnerText.TrimStart());
                    command.Parameters.AddWithValue("@itemStrength", item.SelectSingleNode("td[2]/span[2]").InnerText);
                    command.Parameters.AddWithValue("@itemCount", item.SelectSingleNode("td[5]").InnerText);
                    command.Parameters.AddWithValue("@pricePerPiece", dwarAPI.getMoney(item.SelectSingleNode("td[6]")));
                    command.Parameters.AddWithValue("@itemBid", dwarAPI.getMoney(item.SelectSingleNode("td[7]")));
                    command.Parameters.AddWithValue("@itemBuyOut", dwarAPI.getMoney(item.SelectSingleNode("td[8]")));
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                catch(NullReferenceException)
                {
                    //Здесь будет сообщение для логгера
                }
            }
        }

        public static List<HtmlNode> getItemNodes(HtmlAgilityPack.HtmlDocument doc)
        {       
            HtmlNode itemList = doc.DocumentNode.SelectSingleNode(".//table[@id='item_list']");
            if (itemList != null)
                return itemList.Descendants("tr").Where(d => d.Attributes.Contains("class") && (d.Attributes["class"].Value.Contains("brd2-top"))).ToList<HtmlNode>();
            return null;
        }
    }
}
