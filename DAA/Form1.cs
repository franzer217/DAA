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
                DwarAPI.login();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Thread myThread = new Thread(DwarAPI.scanItems);
                myThread.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                MessageBox.Show(exception.Data.Values.ToString());
            }
        }
    }
}
