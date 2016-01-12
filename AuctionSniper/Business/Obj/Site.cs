using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuctionSniperService.Business;

namespace AuctionSniper.Business.Obj
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using HtmlAgilityPack;
    using System.IO;
    using Business.Http;
    using System.Linq;
    using System.Threading;
    using System.Net;
    using AuctionSniper.Business.DataAccess;
    using System.Net.Security;
    using LunchboxSource.Business.SiteObject.GoDaddy;

    abstract class Site : HtmlParser
    {
        private int RebootCount = 0;
        public static CookieWebClient wc = new CookieWebClient();
        public static CookieContainer cookies = new CookieContainer();

        public string URL { get; set; }
        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public string DateCreated { get; private set; }
        public string GroupID { get; private set; }
        public int jobCounter = 0;
        public Dictionary<string, int> ProxyList = new Dictionary<string, int>();

        public abstract SortableBindingList<Auction> GetAuctions();
        private static System.Text.RegularExpressions.Regex whitespace = new System.Text.RegularExpressions.Regex("\\s{2,}");

        public Site()
        {
            this.Msg(ErrorSeverity.Info, "Processing");
            wc.Headers[HttpRequestHeader.UserAgent] = RefreshUserAgent();
            ServicePointManager.ServerCertificateValidationCallback = new
                RemoteCertificateValidationCallback
                (
                   delegate { return true; }
                );
        }

        public Site(string name, string shortName, string url, string groupID, string dateCreated)
        {
            Name = name;
            ShortName = shortName;
            URL = url;

            this.GroupID = groupID;
            this.DateCreated = dateCreated;
        }
            
        public Auction GenerateAuction()
        {
            Auction p = new Auction();
            return p;
        }

        public static int TryParse_INT(string value)
        {   
            int val;
            return int.TryParse(value, out val) ? int.Parse(value) : 0;
        }

        /// <summary>
        /// Object Class to Thread Resort Gathering..
        /// </summary>
        /// <param name="object2">Node to Pass</param>
        public void GatherDetails(object object2)
        {
            object[] objArray = (object[])object2;
            string link = (string)objArray[0];

            ManualResetEvent event2 = (ManualResetEvent)objArray[1];
            GatherDetails(link);
            if (Interlocked.Decrement(ref this.jobCounter) == 0)
            {
                event2.Set();
            }
        }



        /// <summary>
        /// TO be overwritten by the individual Gather Classes
        /// </summary>
        /// <param name="node">Node</param> 
        public virtual void GatherDetails(string link)
        {

        }

        public enum ErrorSeverity
        {
            Info, Warning, Severe, Fatal, Process
        }

        public HtmlDocument Download(Auction e)
        {
            //this.Msg(ErrorSeverity.Info, "Scraping " + e.Address);
            //return this.Download(e.PropertyUrl);
            return null;
        }

        public HtmlDocument Download(string url)
        {
            HtmlDocument hdoc = new HtmlDocument();
            HtmlNode.ElementsFlags.Remove("option");
            HtmlNode.ElementsFlags.Remove("select");
            Stream read = null;
            wc.Headers[HttpRequestHeader.UserAgent] = RefreshUserAgent();
            try
            {
                read = wc.OpenRead(url);
                RebootCount = 0;
            }
            catch (ArgumentException)
            {
                read = wc.OpenRead(HtmlParser.HTTPEncode(url));
                RebootCount = 0;
            }
            catch (WebException)
            {
                if (RebootCount < 4)
                {
                    RebootCount++;
                    return Download(url);
                }
                else
                {
                    GetSetProxy();
                }

            }
            try
            {
                hdoc.Load(read, true);
            }
            catch (Exception)
            {

                GetSetProxy();
                return this.Download(url);
            }



            return hdoc;
        }


        public string RefreshUserAgent()
        {
            List<string> UserAgents = new List<string>();
            UserAgents.Add("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727)");
            UserAgents.Add("Mozilla/4.0 (compatible; MSIE 8.0; AOL 9.5; AOLBuild 4337.43; Windows NT 6.0; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618)");
            UserAgents.Add("Mozilla/4.0 (compatible; MSIE 7.0; AOL 9.5; AOLBuild 4337.34; Windows NT 6.0; WOW64; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.30729; .NET CLR 3.0.30618)");
            UserAgents.Add("Mozilla/5.0 (X11; U; Linux i686; pl-PL; rv:1.9.0.2) Gecko/20121223 Ubuntu/9.25 (jaunty) Firefox/3.8");
            UserAgents.Add("Mozilla/5.0 (Windows; U; Windows NT 5.1; ja; rv:1.9.2a1pre) Gecko/20090402 Firefox/3.6a1pre (.NET CLR 3.5.30729)");
            UserAgents.Add("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.1b4) Gecko/20090423 Firefox/3.5b4 GTB5 (.NET CLR 3.5.30729)");
            UserAgents.Add("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Avant Browser; .NET CLR 2.0.50727; MAXTHON 2.0)");
            UserAgents.Add("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; Media Center PC 6.0; InfoPath.2; MS-RTC LM 8)");
            UserAgents.Add("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; InfoPath.2; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618)");
            UserAgents.Add("Mozilla/4.0 (compatible; MSIE 7.0b; Windows NT 6.0)");
            UserAgents.Add("Mozilla/4.0 (compatible; MSIE 7.0b; Windows NT 5.1; Media Center PC 3.0; .NET CLR 1.0.3705; .NET CLR 1.1.4322; .NET CLR 2.0.50727; InfoPath.1)");
            UserAgents.Add("Opera/9.70 (Linux i686 ; U; zh-cn) Presto/2.2.0");
            UserAgents.Add("Opera 9.7 (Windows NT 5.2; U; en)");
            UserAgents.Add("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.8.1.8pre) Gecko/20070928 Firefox/2.0.0.7 Navigator/9.0RC1");
            UserAgents.Add("Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.8.1.7pre) Gecko/20070815 Firefox/2.0.0.6 Navigator/9.0b3");
            UserAgents.Add("Mozilla/5.0 (Windows; U; Windows NT 5.1; en) AppleWebKit/526.9 (KHTML, like Gecko) Version/4.0dp1 Safari/526.8");
            UserAgents.Add("Mozilla/5.0 (Windows; U; Windows NT 6.0; ru-RU) AppleWebKit/528.16 (KHTML, like Gecko) Version/4.0 Safari/528.16");
            UserAgents.Add("Opera/9.64 (X11; Linux x86_64; U; en) Presto/2.1.1");
            Random r = new Random();
            return UserAgents[r.Next(0, UserAgents.Count)];
        }


        public bool GetSetProxy()
        {
            RebootCount++;
            if (ProxyList.Count < 2 || RebootCount > 8)
            {
                this.Msg(ErrorSeverity.Process, "Proxy ReSet..");
                ProxyList = new VpnFactory().Generate();
                RebootCount = 0;
            }
            for (int i = 0; i < ProxyList.Count; i++)
            {
                try
                {
                    wc.proxy = new WebProxy(ProxyList.Keys.ElementAt(i), ProxyList.Values.ElementAt(i));
                    this.Msg(ErrorSeverity.Process, "Proxy Set");
                    HtmlDocument hdoc = new HtmlDocument();
                    HtmlNode.ElementsFlags.Remove("option");
                    HtmlNode.ElementsFlags.Remove("select");
                    Stream read = null;
                    wc.Headers[HttpRequestHeader.UserAgent] = RefreshUserAgent();
                    try
                    {
                        if (this.URL == null)
                        {
                            read = wc.OpenRead("http://www.lunchboxcode.com");
                            //RebootCount = 0;
                        }
                        else
                        {
                            read = wc.OpenRead(this.URL);
                            // RebootCount = 0;
                        }

                    }
                    catch (ArgumentException)
                    {
                        read = wc.OpenRead(HtmlParser.HTTPEncode(this.URL));
                        // RebootCount = 0;
                    }
                    //this.Msg(ErrorSeverity.Severe, "Proxy invalid - Removed..");
                    //wc.proxy = null;
                    ProxyList.Remove(ProxyList.Keys.ElementAt(i));
                    return true;

                    //return true;
                }
                catch (Exception)
                {
                    this.Msg(ErrorSeverity.Severe, "Proxy invalid - Removed..");
                    wc.proxy = null;
                    ProxyList.Remove(ProxyList.Keys.ElementAt(i));
                    //return false;
                }
            }
            //ProxyList = new ProxyGen().Generate();
            RebootCount = 10;
            return GetSetProxy();

        }

        public HtmlDocument Download()
        {
            return this.Download(this.URL);
        }

        public void PropertyUpload(Auction prp)
        {
            //Auction chkPrp = ObjectFactory.Query<Auction>(AppSettings.Instance.MySqlConn, string.Format("SELECT * FROM Property WHERE PropertyUrl='{0}'", prp.PropertyUrl));
            //if (chkPrp != null)
            //{
            //    if (prp.Price < chkPrp.Price)
            //    {
            //        DBHelper.SQLUpdate(string.Format("INSERT INTO PropertyPriceDrops (PropertyUrl, OldPrice, NewPrice, ChangeDate) VALUES ('{0}', '{1}', '{2}', '{3}')"
            //        , prp.PropertyUrl, chkPrp.Price, prp.Price, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")));
            //        this.Msg(ErrorSeverity.Info, "Price Change Recorded...");
            //    }
            //}
            //DBHelper.SQLUpdate(string.Format("INSERT INTO Property (Address, City, State, ZipCode, PropertyUrl, Status, PropertyType, Price, SquareFeet, ListDate, YearBuilt, LastChecked) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}' ,'{6}' ,{7} ,{8}, '{9}', '{10}', '{11}') ON DUPLICATE KEY UPDATE Address='{0}', City='{1}', State='{2}', ZipCode='{3}', PropertyUrl='{4}', Status='{5}', PropertyType='{6}', Price={7}, SquareFeet={8}, ListDate='{9}', YearBuilt='{10}', LastChecked='{11}'", prp.Address, prp.City, prp.State, prp.ZipCode, prp.PropertyUrl, prp.Status, prp.Type.ToString(), prp.Price, prp.SquareFeet, prp.ListDate.ToString("yyyy-MM-dd hh:mm:ss") == "0001-01-01 12:00:00" ? DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") : prp.ListDate.ToString("yyyy-MM-dd hh:mm:ss"), prp.YearBuilt.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")));
            ////PauseConsole();
        }

        public HtmlDocument POST(string url, string postData)
        {//string myParameters = "param1=value1&param2=value2&param3=value3";

            HtmlDocument hdoc = new HtmlDocument();
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            using (wc)
            {
                hdoc.LoadHtml(wc.UploadString(url, postData));
            }

            return hdoc;
        }


        public void PauseConsole()
        {
            do
            {
                while (!Console.KeyAvailable)
                {

                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
        }

        public void Msg(ErrorSeverity sev, string msg)
        {
            switch (sev)
            {
                case ErrorSeverity.Fatal:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case ErrorSeverity.Severe:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case ErrorSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case ErrorSeverity.Process:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
            System.Console.WriteLine("*** [" + sev.ToString() + "]: " + this.Name + " - " + msg);
            System.Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
        }


        public static List<string> GetLinksFromFeedburnerXML(string url)
        {

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(wc.DownloadString(url));

            List<string> links = new List<string>();
            foreach (XmlNode link in doc.DocumentElement.GetElementsByTagName("feedburner:origLink"))
            {
                links.Add(link.InnerText.Trim());
            }

            return links;
        }

        public static System.Collections.Generic.List<string> GetLinksFromRSS(string url)
        {
            System.Collections.Generic.List<string> links = new System.Collections.Generic.List<string>();

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(Site.wc.DownloadString(url));

            foreach (System.Xml.XmlElement item in doc.GetElementsByTagName("item"))
            {
                try
                {
                    links.Add(item.GetElementsByTagName("link")[0].InnerText.Trim());
                }
                catch { }
            }
            return links;
        }

        public static System.Collections.Generic.List<string> GetLinksFromRSS(string url, bool date)
        {
            System.Collections.Generic.List<string> links = new System.Collections.Generic.List<string>();

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(Site.wc.DownloadString(url));

            foreach (System.Xml.XmlElement item in doc.GetElementsByTagName("item"))
            {
                try
                {
                    if (date && DateTime.Parse(item.GetElementsByTagName("pubDate")[0].InnerText.Trim()) < DateTime.Now.AddHours(decimal.ToInt32(Properties.Settings.Default.TimeDifference)))
                        continue;
                    links.Add(item.GetElementsByTagName("link")[0].InnerText.Trim());
                }
                catch { }
            }
            return links;
        }

        public static System.Xml.XmlNodeList GetAtomEntries(string url)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(wc.DownloadString(url));
            return doc.GetElementsByTagName("entry");
        }

        public static System.Xml.XmlNodeList GetAtomLinks(string url)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(wc.DownloadString(url));
            return doc.GetElementsByTagName("link");
        }

        public static string ReplaceWhitespace(string content)
        {
            return whitespace.Replace(content.Replace("&nbsp;", " "), "");
        }

        public static string GetGoogleMapsLocation(string link)
        {
            string googleMaps;
            if (!link.Contains("daddr"))
                googleMaps = link.Substring(link.IndexOf("q=") + 2).Replace("%2C", ",").Replace("+", " ");
            else
                googleMaps = link.Substring(link.IndexOf("daddr") + 6).Replace("%2C", ",").Replace("+", " ");
            return googleMaps;
        }
    }
}
