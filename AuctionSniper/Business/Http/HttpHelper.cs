namespace AuctionSniper.Business.Http
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.Drawing;
    using System.Web;
    using System.IO;
    using AuctionSniper.Business.Http;
    using System.Text.RegularExpressions;

    public class HttpHelper
    {
        public HttpHelper()
        {

        }

        public CookieContainer m_container = new CookieContainer();
        public string POST(string urlToCall, string postData)
        {
            HttpWebRequest webReq = new HttpHelper().GetWebRequest(urlToCall);
            webReq.Method = "POST";

            byte[] lbPostBuffer = System.Text.Encoding.GetEncoding(1252).GetBytes(HtmlParser.HTTPEncode(postData));
            webReq.ContentLength = lbPostBuffer.Length;
            webReq.Credentials = CredentialCache.DefaultCredentials;
            webReq.ServicePoint.Expect100Continue = false;
            webReq.AllowAutoRedirect = true;
            webReq.Timeout = 6000;
            webReq.KeepAlive = true;
            webReq.CookieContainer = m_container;
           
            try
            {
                Stream loPostData = webReq.GetRequestStream();
                loPostData.Write(lbPostBuffer, 0, lbPostBuffer.Length);
                loPostData.Close();

                HttpWebResponse loWebResponse = (HttpWebResponse)webReq.GetResponse();
                Encoding enc = System.Text.Encoding.GetEncoding(1252);
                StreamReader loResponseStream = new StreamReader(loWebResponse.GetResponseStream(), enc);

                string lcHtml = loResponseStream.ReadToEnd();

                loWebResponse.Close();
                loResponseStream.Close();

                return lcHtml;
            }
            catch (Exception)
            {

            }


            return "";
        }

        public HttpWebRequest GetWebRequest(string requestPath)
        {
            HttpWebRequest webRequest = null;
            Exception ex = null;
            {
                try
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(requestPath);
                    webRequest.PreAuthenticate = true;
                    webRequest.Timeout = 8000;
                }
                catch (Exception exception1)
                {
                    Console.WriteLine(exception1.Message);
                    ex = exception1;
                }
            }
            if (ex != null)
            {
                throw ex;
            }

            return webRequest;

        }
        public IEnumerable<string> GetSubStrings(string input, string start, string end)
        {
            var r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            return from Match match in matches select match.Groups[1].Value;
        }

        public string GetSubString(string input, string start, string end)
        {
            Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            foreach (Match match in matches)
                return match.Groups[1].Value;

            return "";
        }

        public string ResponseToString(string urlToCall, WebProxy prox = null)
        {
            Console.WriteLine("Working...");
            string responceText = "";
            Exception ex = null;
            {
                try
                {
                    HttpWebRequest webReq = new HttpHelper().GetWebRequest(urlToCall);
                    webReq.Proxy = prox;
                    webReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    webReq.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
                    webReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.11 (KHTML, like Gecko) Chrome/17.0.963.83 Safari/535.11";
                    webReq.Referer = "http://www.google.com";

                    HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            responceText = reader.ReadToEnd();
                        }
                    }
                    response.Close();

                }
                catch (WebException wEx)
                {
                    ex = wEx;
                    if (wEx.Status == WebExceptionStatus.Timeout)
                        Console.WriteLine("The Request Timed Out");
                    else
                        Console.WriteLine("Failed: " + wEx.Message);
                }
            }
            if (ex != null)
            {
                Console.WriteLine("Failed: " + ex.Message);
            }
            //System.Windows.Forms.MessageBox.Show(responceText);
            return responceText;
        }

        public Image DownloadImage(string imageUrl)
        {
            Image _tmpImage = null;
            try
            {
                HttpWebRequest _HttpWebRequest = new HttpHelper().GetWebRequest(imageUrl);
                _HttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                _HttpWebRequest.Referer = "http://www.google.com/";

                System.Net.WebResponse _WebResponse = _HttpWebRequest.GetResponse();
                System.IO.Stream _WebStream = _WebResponse.GetResponseStream();

                _tmpImage = Image.FromStream(_WebStream);
                _WebResponse.Close();
                _WebResponse.Close();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return _tmpImage;
        }


        public static int WordToNumber(string[] words)
        {
            foreach (var word in words)
            {
                switch (word.ToLower())
                {
                    case "zero":
                        return 0;
                    case "one":
                        return 1;
                    case "two":
                        return 2;
                    case "three":
                        return 3;
                    case "four":
                        return 4;
                    case "five":
                        return 5;
                    case "six":
                        return 6;
                    case "seven":
                        return 7;
                    case "eight":
                        return 8;
                    case "nine":
                        return 9;
                    case "ten":
                        return 10;
                }
            }

            return 1;
        }

        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }


    }
}
