using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using workWithXmlByRusskijMir;

namespace DAA
{
    class Statistics
    {
        public static void collectStatistics()
        {
            MySqlConnection connection = new MySqlConnection(globals.connectionString);
            MySqlConnection connection2 = new MySqlConnection(globals.connectionString);
            MySqlDataReader itemReader = null;
            MySqlDataReader statReader = null;
            Int64 totalPrice = 0;
            Int32 minPrice = 0;
            Int32 maxPrice = 0;
            Int32 temp = 0;
            int count = 0;
            try
            {
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT itemName FROM allitems;";
                itemReader = command.ExecuteReader();
                while (itemReader.Read())
                {
                    command.CommandText = "SELECT pricePerItem FROM items WHERE itemName = " + itemReader[0] + " AND buyedOut = 1 AND pricePerItem <> \"\" AND TO_DAYS(NOW()) - TO_DAYS(detectionTime) <= 7";
                    statReader = command.ExecuteReader();
                    while (statReader.Read())
                    {
                        temp = Convert.ToInt32(statReader[0]);
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
                    //сформ строку и создать табл

                }
                itemReader.Close();
            }
            catch (Exception exception)
            {

            }

        }
    }
}
