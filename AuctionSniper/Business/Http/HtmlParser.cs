namespace AuctionSniper.Business.Http
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;
    using System.Collections.Generic;
    using Fizzler.Systems.HtmlAgilityPack;
    using System.Linq;
    using System.Globalization;
    using System.Web;

    public class HtmlParser
    {

        public HtmlParser()
        {

        }


        public IEnumerable<HtmlNode> QuerySelectorAll(HtmlNode node, string query)
        {
            return node.QuerySelectorAll(query);
        }

        public HtmlNode QuerySelector(HtmlNode node, string query)
        {
            return node.QuerySelector(query);
        }

        /// <summary>
        /// Remove Multiple Spaces from a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string TrimSpaces(string str)
        {
            return Regex.Replace(str, @"\s+", " ");
        }

        public string TitleCase(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }


        /// <summary>
        /// Auto-Completes incomplete Urls
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="urlBase"></param>
        /// <returns></returns>
        public List<Uri> GatherLinks(HtmlNode collection, string urlBase)
        {
            var linksOnPage = from lnks in collection.Descendants()
                              where lnks.Name == "a" &&
                                   lnks.Attributes["href"] != null &&
                                   lnks.InnerText.Trim().Length > 0
                              select new
                              {
                                  Url = lnks.Attributes["href"].Value,
                              };
            List<Uri> Uris = new List<Uri>();

            foreach (var link in linksOnPage)
            {
                Uri baseUri = new Uri(urlBase, UriKind.Absolute);
                Uri page = new Uri(baseUri, link.Url.ToString());
                Uris.Add(page);
            }

            return Uris;
        }

        public Uri CompleteUri(string Url, string SiteBaseUrl)
        {
            Uri baseUri = new Uri(SiteBaseUrl, UriKind.Absolute);
            Uri page = new Uri(baseUri, Url);

            return page;
        }


        public IEnumerable<string> GetSubStrings(string input, string start, string end)
        {
            Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            foreach (Match match in matches)
                yield return match.Groups[1].Value;
        }

        public string GetSubString(string input, string start, string end)
        {
            Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            foreach (Match match in matches)
                return match.Groups[1].Value;

            return "";
        }



        private bool Is_URL_Format_OK(string str_url)
        {
            try
            {
                new Uri(str_url);
                return true;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }


        public static bool Does_URL_Exists(string str_url)
        {
            using (var client = new MyClient())
            {
                client.HeadOnly = true;
                try
                {
                    string s1 = client.DownloadString(str_url);
                    return true;
                }
                catch
                {
                    return false;
                }

            }

        }


        public static String HTTPDecode(String m)
        {
            return HttpUtility.HtmlDecode(m);
        }


        public static String HTTPEncode(String m)
        {
            return HttpUtility.HtmlEncode(m);
        }

    }


    class MyClient : WebClient
    {
        public bool HeadOnly { get; set; }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest req = base.GetWebRequest(address);
            if (HeadOnly && req.Method == "GET")
            {
                req.Method = "HEAD";
            }
            return req;
        }
    }
}
