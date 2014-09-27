using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DAA
{
  static class DwarRequest
  {
    /// <summary>
    /// Выполняет get запрос по адресу dwarUrl, передавая туда cookie.
    /// </summary>
    /// <param name="dwarUrl">Адрес, к которому обращаемся</param>
    /// <param name="cookie">Куки текущей сессии</param>
    /// <param name="dwarReferer">Параметр запроса Referer. Необязателен</param>
    /// <returns></returns>
    public static string getRequest(string dwarUrl, ref CookieContainer cookie, string dwarReferer = "http://w1.dwar.ru/")
    {
        //try
        //{
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dwarUrl);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:16.0) Gecko/20100101 Firefox/17.0";
            request.AllowAutoRedirect = true;
            request.CookieContainer = cookie;
            request.Referer = dwarReferer;
            HttpWebResponse response;
            string streamReader;
            do
            {
                response = (HttpWebResponse)request.GetResponse();
                streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                request.GetResponse().Close();
                response.GetResponseStream().Close();
            }
            while (response.StatusDescription != "OK");
            return streamReader;
        //}
        //catch(Exception exception)
        //{
        //    globals.dwarLog.Error(exception.Message + " " + exception.StackTrace + " " + "ThreadID =  " + Thread.CurrentThread.ManagedThreadId);
        //    globals.dwarLog.Error(dwarUrl);
        //    //MessageBox.Show(exception.Message + " " + exception.StackTrace + " " + Thread.CurrentThread.ManagedThreadId);
        //    HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(dwarUrl);
        //    request2.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:16.0) Gecko/20100101 Firefox/17.0";
        //    request2.AllowAutoRedirect = true;
        //    request2.CookieContainer = cookie;
        //    request2.Referer = dwarReferer;
        //    HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();
        //    string streamReader = new StreamReader(response2.GetResponseStream(), Encoding.UTF8).ReadToEnd();
        //    request2.GetResponse().Close();
        //    response2.GetResponseStream().Close();
        //    globals.dwarLog.Error("Вторая попытка");
        //    return streamReader;
        //    //return "";
        //}
    }

    /// <summary>
    /// Выполняет get запрос по адресу dwarUrl, передавая туда cookie и данные, идущие как параметры запроса.
    /// </summary> 
    /// <param name="dwarUrl">Адрес, к которому обращаемся</param>
    /// <param name="cookie">Куки текущей сессии</param>
    /// <param name="dataToPost">Отправляемые параметры запроса</param>
    /// <param name="dwarReferer">Параметр запроса Referer. Необязателен</param>
    /// <returns></returns>
    public static string postRequest(string dwarUrl, ref CookieContainer cookie, string dataToPost, string dwarReferer = "http://w1.dwar.ru/")
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dwarUrl);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:16.0) Gecko/20100101 Firefox/17.0";
            request.Method = "POST";
            request.AllowAutoRedirect = true;
            request.Referer = dwarReferer;
            request.CookieContainer = cookie;
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] EncodedPostParams = Encoding.ASCII.GetBytes(dataToPost);
            request.ContentLength = EncodedPostParams.Length;
            request.GetRequestStream().Write(EncodedPostParams, 0, EncodedPostParams.Length);
            request.GetRequestStream().Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.ResponseUri.ToString() == "http://w1.dwar.ru/main.php")
            {
                string streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                request.GetResponse().Close();
                response.GetResponseStream().Close();
                return streamReader;
            }
            return "";
        }
        catch(Exception exception)
        {
            globals.dwarLog.Error(exception.Message + " " + exception.StackTrace + " " + Thread.CurrentThread.ManagedThreadId);
            MessageBox.Show(exception.Message + " " + exception.StackTrace + " " + Thread.CurrentThread.ManagedThreadId);
            return "";
        }
    }
  }
}
