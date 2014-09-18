using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

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
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dwarUrl);
        request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:16.0) Gecko/20100101 Firefox/17.0";
        request.AllowAutoRedirect = true;
        request.CookieContainer = cookie;
        request.Referer = dwarReferer;
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        return(new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd());
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
        return(new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd());
    }
  }
}
