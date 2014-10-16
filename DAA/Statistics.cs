using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using workWithXmlByRusskijMir;
using System.Windows.Forms;

namespace DAA
{
    class Statistics
    { 
        public static void createStatisticsTable()
        {
            MySqlConnection connection = new MySqlConnection(globals.connectionString);
            try
            {
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "CREATE TABLE IF NOT EXISTS statistics (itemName NVARCHAR(50) PRIMARY KEY, minPrice INT, maxPrice INT, averagePrice INT, itemCount INT);";
                command.Connection = connection;
                connection.Open();
                command.ExecuteNonQuery();                
            }
            catch(Exception exception)
            {
                globals.dwarLog.Error(exception.ToString());
                MessageBox.Show(exception.Message);
            }
            finally
            {
                if(connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
        }

        public static void collectStatistics()
        {
            MySqlConnection connection = new MySqlConnection(globals.connectionString);
            MySqlConnection connection2 = new MySqlConnection(globals.connectionString);
            MySqlConnection connection3 = new MySqlConnection(globals.connectionString);
            MySqlDataReader itemReader = null;
            MySqlDataReader priceReader = null;
            Int64 totalPrice = 0;
            Int32 minPrice = 0;
            Int32 maxPrice = 0;
            Int32 temp = 0;
            int averagePrice = 0;
            int count = 0;
            createStatisticsTable();
            string commandText = "REPLACE INTO statistics (itemName, minPrice, maxPrice, averagePrice, itemCount) VALUES";
            try
            {
                MySqlCommand itemCommand = new MySqlCommand();
                MySqlCommand getPriceCommand = new MySqlCommand();
                MySqlCommand statCommand = new MySqlCommand();                
                itemCommand.Connection = connection;
                getPriceCommand.Connection = connection2;
                statCommand.Connection = connection3;
                itemCommand.CommandText = "SELECT itemName FROM allitems;";
                connection.Open();
                connection2.Open();
                connection3.Open();
                itemReader = itemCommand.ExecuteReader();
                while (itemReader.Read())
                {

                    getPriceCommand.CommandText = "SELECT pricePerPiece FROM items WHERE itemName = '" + MySqlHelper.EscapeString(itemReader[0].ToString()) + "' AND buyedOut = '1' AND pricePerPiece <> '' AND TO_DAYS(NOW()) - TO_DAYS(detectionTime) <= 30";                   
                    priceReader = getPriceCommand.ExecuteReader();
                    while (priceReader.Read())
                    {
                        temp = Convert.ToInt32(priceReader[0]);
                        if (count == 0)
                        {
                            minPrice = temp;
                            maxPrice = temp;
                        }
                        if (temp < minPrice)
                            minPrice = temp;
                        if (temp > maxPrice)
                            maxPrice = temp;
                        totalPrice += temp;
                        count++;
                    }
                    priceReader.Close();
                    if (count != 0)
                    {
                        averagePrice = Convert.ToInt32(totalPrice / count);
                        commandText += "('" + MySqlHelper.EscapeString(itemReader[0].ToString()) + "','" + minPrice + "','" + maxPrice + "','" + averagePrice + "','" + count + "'),";
                        count = 0;
                        minPrice = 0;
                        maxPrice = 0;
                        totalPrice = 0;
                    }                        
                }
                
                itemReader.Close();
                statCommand.CommandText = commandText.TrimEnd(',') + ";";
                statCommand.ExecuteNonQuery();
                MessageBox.Show("Статистика добавлена");
                onStatisticsCollectionFinish();
            }
            catch (Exception exception)
            {
                globals.dwarLog.Error(exception.ToString() + " count=" + count);
                MessageBox.Show(exception.Message);
            }
            finally
            {
                if(itemReader.IsClosed == false)
                {
                    itemReader.Close();
                }
                if (priceReader.IsClosed == false)
                {
                    priceReader.Close();
                }
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
                if (connection2.State != System.Data.ConnectionState.Closed)
                    connection2.Close();                
            }
        }

        public delegate void MethodContainer();
        public static event MethodContainer onStatisticsCollectionFinish;
    }
}
