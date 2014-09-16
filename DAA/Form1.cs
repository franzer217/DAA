using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

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
      CookieContainer cookie = new CookieContainer();
      string test = DwarRequest.postRequest("http://w1.dwar.ru/login.php", ref cookie, "email=zadisa2006@mail.ru&passwd=ee34nf3o&x=59&y=17");
      test = DwarRequest.getRequest("http://w1.dwar.ru/area_auction.php", ref cookie);
    }
  }
}
