using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using HtmlAgilityPack;
using Lunchboxweb.BaseFunctions;

namespace Lunchboxweb
{
    public class HttpBase : HtmlParser
    {
        /// <summary>
        /// Contains the session cookies
        /// </summary>
        public CookieContainer CookieContainer { get; set; }
        private readonly UTF8Encoding _utf8Encoding;
        private int _timeout = 8000;

        public TextManipulation TextModifier { get; private set; }

        public HttpBase()
        {
            CookieContainer = new CookieContainer();
            _utf8Encoding = new UTF8Encoding();
            TextModifier = new TextManipulation();

            UseFixedBrowser = false;
            SetSaveCookies = true;
            SaveCookies = true;
            AllowAutoRedirect = true;
            UseSystemProxy = false;
        }

        /// <summary>
        /// Request timeout : default 8 seconds (ms)
        /// </summary>
        public int TimeoutInterval
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public enum RequestType
        {
            // ReSharper disable once InconsistentNaming
            POST, 
            // ReSharper disable once InconsistentNaming
            GET
        }

        /// <summary>
        /// New html document instance
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public HtmlDocument HtmlDocument(string html)
        {
            var d = new HtmlDocument();
            d.LoadHtml(html);

            return d;
        }

        public WebProxy Proxy { get; set; }
        public string Referer { get; set; }

        public bool AllowAutoRedirect { get; set; }

        public bool SaveCookies { get; set; }

        public bool UseSystemProxy { get; set; }

        public bool SetSaveCookies { get; set; }

        public bool UseFixedBrowser { get; set; }

        private IWebProxy _webServiceProxy;
        private bool? _webServiceProxyDiscovered;

        private IWebProxy DiscoverWebServiceProxy()
        {
            if (_webServiceProxyDiscovered.HasValue) return _webServiceProxy;
            try
            {
                _webServiceProxy = WebRequest.GetSystemWebProxy();
                _webServiceProxyDiscovered = true;
                if (_webServiceProxy != null)
                {
                    _webServiceProxy.Credentials = CredentialCache.DefaultCredentials;
                }
                return _webServiceProxy;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Standard web request
        /// </summary>
        /// <param name="url">Url to get</param>
        /// <returns>HttpWebRequest</returns>
        protected HttpWebRequest GetWebRequest(string url)
        {
            HttpWebRequest webRequest = null;
            Exception ex = null;
            {
                try
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(url);
                    if (UseSystemProxy)
                    {
                        Proxy = (WebProxy)DiscoverWebServiceProxy();
                    }
                    if (Proxy != null)
                    {
                        webRequest.PreAuthenticate = true;
                        webRequest.Proxy = Proxy;
                        webRequest.Timeout = TimeoutInterval;
                    }
                }
                catch (Exception exception1)
                {
                    Console.WriteLine(exception1.Message);
                    ex = exception1;
                }
            }
            if (ex != null)
            {
                //throw ex;
            }

            return webRequest;

        }

        /// <summary>
        /// Download an image from a webaddress stream
        /// </summary>
        /// <param name="url">Image url</param>
        /// <returns></returns>
        public Image StreamImage(string url)
        {
            try
            {
                var request = GetWebRequest(url);

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())

                {
                    Debug.Assert(stream != null, "stream != null");
                    return Image.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// Randomises the browser details submitted in the requests
        /// </summary>
        /// <returns></returns>
        public string RefreshUserAgent()
        {
            if (UseFixedBrowser)
            {
                return "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.95 Safari/537.36";
            }
            var userAgents = new List<string>
            {
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727)",
                "Mozilla/4.0 (compatible; MSIE 8.0; AOL 9.5; AOLBuild 4337.43; Windows NT 6.0; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618)",
                "Mozilla/4.0 (compatible; MSIE 7.0; AOL 9.5; AOLBuild 4337.34; Windows NT 6.0; WOW64; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.30729; .NET CLR 3.0.30618)",
                "Mozilla/5.0 (X11; U; Linux i686; pl-PL; rv:1.9.0.2) Gecko/20121223 Ubuntu/9.25 (jaunty) Firefox/3.8",
                "Mozilla/5.0 (Windows; U; Windows NT 5.1; ja; rv:1.9.2a1pre) Gecko/20090402 Firefox/3.6a1pre (.NET CLR 3.5.30729)",
                "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.1b4) Gecko/20090423 Firefox/3.5b4 GTB5 (.NET CLR 3.5.30729)",
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Avant Browser; .NET CLR 2.0.50727; MAXTHON 2.0)",
                "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; Media Center PC 6.0; InfoPath.2; MS-RTC LM 8)",
                "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; InfoPath.2; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618)",
                "Mozilla/4.0 (compatible; MSIE 7.0b; Windows NT 6.0)",
                "Mozilla/4.0 (compatible; MSIE 7.0b; Windows NT 5.1; Media Center PC 3.0; .NET CLR 1.0.3705; .NET CLR 1.1.4322; .NET CLR 2.0.50727; InfoPath.1)",
                "Opera/9.70 (Linux i686 ; U; zh-cn) Presto/2.2.0",
                "Opera 9.7 (Windows NT 5.2; U; en)",
                "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.8.1.8pre) Gecko/20070928 Firefox/2.0.0.7 Navigator/9.0RC1",
                "Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.8.1.7pre) Gecko/20070815 Firefox/2.0.0.6 Navigator/9.0b3",
                "Mozilla/5.0 (Windows; U; Windows NT 5.1; en) AppleWebKit/526.9 (KHTML, like Gecko) Version/4.0dp1 Safari/526.8",
                "Mozilla/5.0 (Windows; U; Windows NT 6.0; ru-RU) AppleWebKit/528.16 (KHTML, like Gecko) Version/4.0 Safari/528.16",
                "Opera/9.64 (X11; Linux x86_64; U; en) Presto/2.1.1"
            };
            var r = new Random();
            return userAgents[r.Next(0, userAgents.Count)];
        }

        /// <summary>
        /// Gathers all avaiable cookie info from a cookie container into a collection
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static CookieCollection GetAllCookies(CookieContainer container)
        {
            var allCookies = new CookieCollection();
            var domainTableField = container.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
            if (domainTableField == null) return allCookies;
            var domains = (IDictionary)domainTableField.GetValue(container);

            foreach (var cookies in from object val in domains.Values 
                let type = val.GetType().GetRuntimeFields().First(x => x.Name == "m_list") 
                select (IDictionary)type.GetValue(val) into values 
                from CookieCollection cookies in values.Values select cookies)
            {
                allCookies.Add(cookies);
            }
            return allCookies;
        }

        /// <summary>
        /// Processes additional cookies in request repsonse
        /// </summary>
        /// <param name="req"></param>
        /// <param name="resp"></param>
        private void CollectCookies(HttpWebRequest req, HttpWebResponse resp)
        {
            if (!SaveCookies)
                return;

            foreach (Cookie ck in resp.Cookies)
            {
                req.CookieContainer.Add(ck);
            }

            GetAllCookies(CookieContainer);

            if (!SetSaveCookies)
                return;

            for (var i = 0; i < resp.Headers.Count; i++)
            {
                var name = resp.Headers.GetKey(i);
                if (name != "Set-Cookie")
                    continue;
                var value = resp.Headers.Get(i);
                foreach (var singleCookie in value.Split(','))
                {
                    try
                    {
                        var match = Regex.Match(singleCookie, "(.+?)=(.+?);");
                        if (match.Captures.Count == 0)
                            continue;
                        resp.Cookies.Add(
                            new Cookie(
                                match.Groups[1].ToString(),
                                match.Groups[2].ToString(),
                                "/",
                                req.Host.Split(':')[0]));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }

        /// <summary>
        /// Clear all cookies
        /// </summary>
        public void ClearCookies()
        {
            CookieContainer = new CookieContainer();
        }

        protected string Post(string url, string postData)
        {
            return Request(RequestType.POST, url, postData, null);
        }

        protected string Post(string url, string postData, Dictionary<string, string> requestHeaders)
        {
            return Request(RequestType.POST, url, postData, requestHeaders);
        }

        protected string Get(string url)
        {
            return Request(RequestType.GET, url, "", null);
        }

        protected string Get(string url, Dictionary<string, string> requestHeaders)
        {
            return Request(RequestType.GET, url, "", requestHeaders);
        }

        /// <summary>
        /// Pull an image from a url - includes cookie and session info
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Image GetImage(string url)
        {
            var request = GetWebRequest(url);

            if (request == null)
                return null;

            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = CookieContainer;
            request.UserAgent = RefreshUserAgent();
            request.KeepAlive = true;
            request.AllowAutoRedirect = AllowAutoRedirect;
            request.Referer = Referer;
            request.Timeout = TimeoutInterval;

            //Decode Response
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                // ReSharper disable once AssignNullToNotNullAttribute
                return Image.FromStream(response.GetResponseStream());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return null;
        }

        private string Request(RequestType reqType, string url, string postData, Dictionary<string, string> requestHeaders)
        {
            var request = GetWebRequest(url);
            var responseData = "";

            if (request == null)
                return "";

            request.Method = reqType.ToString();
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = CookieContainer;
            request.UserAgent = RefreshUserAgent();
            request.KeepAlive = true;
            request.AllowAutoRedirect = AllowAutoRedirect;
            request.Referer = Referer;
            request.Timeout = TimeoutInterval;

            if (reqType == RequestType.POST)
                request.ContentLength = postData.Length;

            if (requestHeaders != null)
            {
                foreach (var item in requestHeaders)
                {
                    switch (item.Key)
                    {
                        case "Accept":
                            request.Accept = item.Value;
                            break;
                        case "Content-Type":
                            request.ContentType = item.Value;
                            break;
                        default:
                            request.Headers.Add(item.Key, item.Value);
                            break;
                    }

                }
            }

            //Decode Response
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (reqType == RequestType.POST)
            {
                var stOut = new StreamWriter(request.GetRequestStream());
                stOut.Write(postData);
                stOut.Flush();
                stOut.Close();
            }
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                // ReSharper disable once AssignNullToNotNullAttribute
                var responseReader = new StreamReader(response.GetResponseStream(), _utf8Encoding, true);

                responseData = responseReader.ReadToEnd();
                response.Close();
                responseReader.Close();

                CollectCookies(request, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed: " + ex.Message);
            }

            return responseData;
        }
    }
}
