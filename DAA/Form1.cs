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

//Дата первого обнаружения, ожидаемая дата выхода лота с аукциона, выкупили(да/нет), времени до выхода с аукциона, наличие, дата текущего обнаружения(Если поле меньше текущей даты на n минут - предмета нет)
//Сделать нормальные типы данных у столбцов таблиц

namespace DAA
{
    public partial class Form1 : Form
    {
        public Form1()
        {      
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            try
            {
                DwarAPI.login();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Thread myThread = new Thread(DwarAPI.startNewThread);
                myThread.Start();
                Thread statisticsThread = new Thread(Statistics.collectStatistics);
                statisticsThread.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {                
                DwarAPI.getCategories();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Thread myThread = new Thread(DwarAPI.getAllItems);
                myThread.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
    }
}
