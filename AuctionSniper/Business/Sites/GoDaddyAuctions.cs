using LunchboxSource.Business.Export;

namespace AuctionSniper.Business.Sites
{
    using AuctionSniper.Business.Obj;
    using HtmlAgilityPack;
    using System;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.IO;
    using System.Web;
    using System.Xml.Serialization;
    using System.Linq;
    using System.Net.Security;
    using LunchboxSource.Business.SiteObject.GoDaddy;

    partial class GoDaddyAuctions : Site
    {
        public static GoDaddyAuctions Instance = new GoDaddyAuctions();
        public System.Windows.Forms.WebBrowser wb = new System.Windows.Forms.WebBrowser();

        private string searchString = "https://auctions.godaddy.com/trpSearchResults.aspx";
        private string viewstates = "";

        private GoDaddyAuctions()
            : base()
        {

        }

        public override SortableBindingList<Auction> GetAuctions()
        {

            return null;
        }

        public SortableBindingList<Auction> GetBidsandOffers()
        {
            SortableBindingList<Auction> auctions = new SortableBindingList<Auction>();
            #region Get The data from the server
            HttpWebRequest request;
            HttpWebResponse response;

            var strUrl = @"https://auctions.godaddy.com/";
            var postData = "";
            var responseData = "";
            var encoding = new System.Text.UTF8Encoding();

            strUrl = @"https://auctions.godaddy.com/trpMessageHandler.aspx ";
            postData = string.Format("sec=Bi&sort=7&dir=A&page=1&at=3&rpp=50&rnd={0}",
                randomDouble(0, 1).ToString("0.00000000000000000"));
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.Accept = "gzip,deflate,sdch";
            request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-powerpoint, application/vnd.ms-excel, application/x-silverlight, application/x-silverlight-2-b2, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "deflate");
            request.Referer = "https://auctions.godaddy.com/trpMyAccount.aspx?ci=22373&s=2&sc=Bi";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;
            request.Timeout = Timeout.Infinite;

            request.CookieContainer = cookies;

            var stOut = new StreamWriter(request.GetRequestStream());
            stOut.Write(postData);
            stOut.Flush();
            stOut.Close();
            stOut = null;


            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

            encoding = new System.Text.UTF8Encoding();
            var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();
            #endregion
            //responseData;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(responseData);

            foreach (var row in QuerySelectorAll(doc.DocumentNode, "tr[class^=marow]"))
            {
                var auction = GenerateAuction();
                auction.AuctionRef = QuerySelector(row, "td:nth-child(2) > a").Attributes["href"].Value.Split(new char[] { '=' })[1];
                auction.Domain = HttpUtility.HtmlDecode(QuerySelector(row, "td:nth-child(2)").InnerText).Trim();
                auction.MinBid = TryParse_INT(HttpUtility.HtmlDecode(QuerySelector(row, "td:nth-child(5)").InnerText).Trim().Replace("$", "").Replace("C", ""));
                auction.Status = HttpUtility.HtmlDecode(QuerySelector(row, "td:nth-child(7)").InnerText.Trim());
                GetMyOfferDetails(ref auction);

                if (LoadMyLocalBids() != null)
                {
                    foreach (var auc in LoadMyLocalBids())
                    {
                        if (auc.AuctionRef == auction.AuctionRef)
                        {
                            auction.MyBid = auc.MyBid;
                        }
                    }
                }
                if (auction.MyBid == 0)
                {
                    auction.MyBid = auction.MinBid;
                }
                auctions.Add(auction);

            }

            foreach (var auction in LoadMyLocalBids())
            {

                if (!auctions.Select(s => s.AuctionRef).Distinct().Contains(auction.AuctionRef) &&
                    auction.EndDate > DateTime.Now.AddHours(decimal.ToInt32(Properties.Settings.Default.TimeDifference)))
                {
                    var searchList = Search(auction.Domain);
                    if (searchList != null && searchList.Count > 0)
                    {
                        auction.Bids = searchList[0].Bids;
                        auction.MinBid = searchList[0].MinBid;
                        auction.Traffic = searchList[0].Traffic;
                        auction.Status = searchList[0].Status;
                        auction.Price = searchList[0].Price;
                    }
                    auctions.Add(auction);
                }

            }


            return auctions;
        }

        private SortableBindingList<Auction> LoadMyLocalBids()
        {
            SortableBindingList<Auction> SavedLocalBids = new SortableBindingList<Auction>();
            if (File.Exists(Properties.Settings.Default.BidsFile))
            {
                using (Stream loadstream = new FileStream(Properties.Settings.Default.BidsFile, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SortableBindingList<Auction>));
                    SavedLocalBids = (SortableBindingList<Auction>)serializer.Deserialize(loadstream);
                }
            }

            return SavedLocalBids;
        }


        public static double randomDouble(double start, double end)
        {
            Random rand = new Random();
            return (rand.NextDouble() * Math.Abs(end - start)) + start;
        }

        private void GetMyOfferDetails(ref Auction auction)
        {
            HttpWebRequest request;
            HttpWebResponse response;
            #region Post details
            var responseData = "";
            var strUrl = string.Format("https://auctions.godaddy.com/trpMessageHandler.aspx?h={0}&rnd={1}",
                auction.AuctionRef, randomDouble(0, 1).ToString("0.00000000000000000"));

            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = 0;
            request.CookieContainer = cookies;
            request.Timeout = System.Threading.Timeout.Infinite;
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;

            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            var encoding = new System.Text.UTF8Encoding();
            var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();
            #endregion

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(responseData);

            foreach (var row in QuerySelectorAll(doc.DocumentNode, "tr"))
            {
                if (QuerySelector(row, "td:nth-child(3)") != null && QuerySelector(row, "td:nth-child(3)").InnerText == "My&nbsp;Offer")
                {
                    auction.MyBid = TryParse_INT(
                        QuerySelector(row, "td:nth-child(4)").InnerText
                            .Replace("C", "").Replace("$", "").Replace(",", ""));
                    break;
                }
            }

            auction.EndDate = GetEndDate(auction.AuctionRef);
        }

        public SortableBindingList<Auction> Search(string searchText)
        {
            SortableBindingList<Auction> auctions = new SortableBindingList<Auction>();
            HtmlDocument doc = POST(searchString, "t=22&action=search&hidAdvSearch=ddlAdvKeyword:1|txtKeyword:" + searchText.Replace(" ", ",") + "&rtr=7&baid=-1&searchMode=1&searchDir=1&event=&rnd=0.698455864796415&EqnJYig=561ef36");


            if (QuerySelectorAll(doc.DocumentNode, "tr.srRow2, tr.srRow1") != null)
            {
                foreach (var node in QuerySelectorAll(doc.DocumentNode, "tr.srRow2, tr.srRow1"))
                {
                    if (QuerySelector(node, "span.OneLinkNoTx") != null && QuerySelector(node, "td:nth-child(5)") != null)
                    {
                        Auction auction = this.GenerateAuction();
                        auction.Domain = HTTPDecode(QuerySelector(node, "span.OneLinkNoTx").InnerText);
                        Console.WriteLine(auction.Domain);

                        auction.Bids = TryParse_INT(HTTPDecode(QuerySelector(node, "td:nth-child(5)").FirstChild.InnerHtml.Replace("&nbsp;", "")));
                        auction.Traffic = TryParse_INT(HTTPDecode(QuerySelector(node, "td:nth-child(5) > td").InnerText.Replace("&nbsp;", "")));
                        auction.Valuation = TryParse_INT(HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(2)").InnerText.Replace("&nbsp;", "")));
                        auction.Price = TryParse_INT(HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(3)").InnerText).Replace("$", "").Replace(",", "").Replace("C", ""));
                        try
                        {
                            if (QuerySelector(node, "td:nth-child(5) > td:nth-child(4) > div") != null)
                            {
                                if (HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(4) > div").InnerText).Contains("Buy Now for"))
                                {
                                    auction.BuyItNow = TryParse_INT(Regex.Split(HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(4) > div").InnerText), "Buy Now for")[1].Trim().Replace(",", "").Replace("$", ""));
                                }

                            }
                            else
                            {
                                auction.BuyItNow = 0;
                            }

                        }
                        catch (Exception) { auction.BuyItNow = 0; }

                        if (QuerySelector(node, "td:nth-child(5) > td:nth-child(5)") != null &&
                            HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(5)").InnerHtml).Contains("Bid $"))
                        {
                            auction.MinBid = TryParse_INT(GetSubString(HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(5)").InnerHtml), "Bid $", " or more").Trim().Replace(",", "").Replace("$", ""));
                        }
                        if (QuerySelector(node, "td:nth-child(5) > td:nth-child(5)") != null &&
                            !HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(5)").InnerHtml).Contains("Bid $"))
                        {
                            auction.EstimateEndDate = GenerateEstimateEnd(QuerySelector(node, "td:nth-child(5) > td:nth-child(5)"));
                        }
                        if (QuerySelector(node, "td:nth-child(5) > td:nth-child(4)") != null &&
                            HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(4)").InnerHtml).Contains("Bid $"))
                        {
                            auction.MinBid = TryParse_INT(GetSubString(HTTPDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(4)").InnerHtml), "Bid $", " or more").Trim().Replace(",", "").Replace("$", ""));
                        }
                        if (QuerySelector(node, "td > div > span") != null)
                        {
                            foreach (var item in GetSubStrings(QuerySelector(node, "td > div > span").InnerHtml, "'Offer $", " or more"))
                            {
                                auction.MinOffer = TryParse_INT(item.Replace(",", ""));
                            }
                        }
                        auction.EndDate = DateTime.Now;
                        foreach (var item in this.GetSubStrings(node.InnerHtml, "ShowAuctionDetails('", "',"))
                        {
                            auction.AuctionRef = item;
                            break;
                        }
                        Console.WriteLine(auction.Bids.ToString());
                        Console.WriteLine(auction.Traffic.ToString());
                        Console.WriteLine(auction.Valuation.ToString());
                        Console.WriteLine(auction.Bids.ToString());
                        Console.WriteLine("BIN: " + auction.BuyItNow.ToString());
                        //AppSettings.Instance.AllAuctions.Add(auction);
                        if (auction.MinBid > 0)
                        {
                            auctions.Add(auction);
                        }

                    }

                }
            }

            Console.WriteLine("Done");
            return auctions;
        }

        private DateTime GenerateEstimateEnd(HtmlNode node)
        {
            var estimateEnd = DateTime.Now.AddHours(decimal.ToInt32(Properties.Settings.Default.TimeDifference));
            if (node.InnerText != null)
            {
                var vals = HTTPDecode(node.InnerText).Trim().Split(new char[] { ' ' });

                foreach (var item in vals)
                {
                    if (item.Contains("D"))
                    {
                        estimateEnd = estimateEnd.AddDays(double.Parse(item.Replace("D", "")));
                    }
                    else if (item.Contains("H"))
                    {
                        estimateEnd = estimateEnd.AddHours(double.Parse(item.Replace("H", "")));
                    }
                    else if (item.Contains("M"))
                    {
                        estimateEnd = estimateEnd.AddMinutes(double.Parse(item.Replace("M", "")));
                    }
                }

            }

            return estimateEnd;
        }

        public DateTime GetEndDate(string auctionNo)
        {
            var endDate = DateTime.Now.AddHours(decimal.ToInt32(Properties.Settings.Default.TimeDifference));
            var details = this.GetAuctionDetails(auctionNo);
            if (QuerySelector(details.DocumentNode, "span.OneLinkNoTx") != null)
            {
                endDate = (QuerySelector(details.DocumentNode, "span.OneLinkNoTx").InnerText.Contains("PM") &&
                    DateTime.Parse(QuerySelector(details.DocumentNode, "span.OneLinkNoTx").InnerText.Replace("AM", "").Replace("PM", "").Replace("(PST)", "").Replace("(PDT)", "").Trim(), new CultureInfo("en-US", false)).Hour < 12 ) ?
                    DateTime.Parse(QuerySelector(details.DocumentNode, "span.OneLinkNoTx").InnerText.Replace("AM", "").Replace("PM", "").Replace("(PST)", "").Replace("(PDT)", "").Trim(), new CultureInfo("en-US", false)).AddHours(12) :
                    DateTime.Parse(QuerySelector(details.DocumentNode, "span.OneLinkNoTx").InnerText.Replace("AM", "").Replace("PM", "").Replace("(PST)", "").Replace("(PDT)", "").Trim(), new CultureInfo("en-US", false));
            }
            else if (QuerySelector(details.DocumentNode, "td[style*=background-color]") != null)
            {
                endDate = (QuerySelector(details.DocumentNode, "td[style*=background-color]").InnerText.Contains("PM") && 
                    DateTime.Parse(QuerySelector(details.DocumentNode, "td[style*=background-color]").InnerText.Replace("AM", "").Replace("PM", "").Replace("(PST)", "").Replace("(PDT)", "").Trim(), new CultureInfo("en-US", false)).Hour < 12) ?
                    DateTime.Parse(QuerySelector(details.DocumentNode, "td[style*=background-color]").InnerText.Replace("AM", "").Replace("PM", "").Replace("(PST)", "").Replace("(PDT)", "").Trim(), new CultureInfo("en-US", false)).AddHours(12) :
                    DateTime.Parse(QuerySelector(details.DocumentNode, "td[style*=background-color]").InnerText.Replace("AM", "").Replace("PM", "").Replace("(PST)", "").Replace("(PDT)", "").Trim(), new CultureInfo("en-US", false));
            }

            //TimeSpan ts = endDate.Subtract(AppSettings.Instance.CurrentAuction.EstimateEndDate);
            //if (ts.TotalHours > 1)
            //{
            //    //Form1.Instance.UpdateProgress("Enddate Corrected..");
            //    endDate = endDate.AddHours(-12);
            //}
            //else if (ts.TotalHours < -1)
            //{
            //    //Form1.Instance.UpdateProgress("Enddate Corrected..");
            //    endDate = endDate.AddHours(12);
            //}

            return endDate;
        }

        private HtmlDocument GetAuctionDetails(string auctionNo)
        {
            return POST("https://auctions.godaddy.com/trpMessageHandler.aspx", string.Format("ad={0}&type=Search", auctionNo));
        }

        public void GetAuctionHistory()
        {
            //POST https://auctions.godaddy.com/trpMessageHandler.aspx HTTP/1.1
            //Host: auctions.godaddy.com
            //Connection: keep-alive
            //Content-Length: 10
            //Origin: https://auctions.godaddy.com
            //User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11
            //Content-type: application/x-www-form-urlencoded
            //Accept: */*
            //Referer: https://auctions.godaddy.com/
            //Accept-Encoding: gzip,deflate,sdch
            //Accept-Language: en-GB,en-US;q=0.8,en;q=0.6
            //Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3
            //Cookie: optimizelyEndUserId=oeu1357419547114r0.3686775981914252; ASP.NET_SessionId=lzzq0345n24var55tqgfd355; PCSplitValue1=2; pathway=855482c4-3df5-4b09-aa89-3ed17919a6d1; vistorpromo1=firsttime; SplitValue1=64; Idp.FBPixel1.46311038=; mobile.redirect.browser.checked=1; serverVersion=A; isChase=potableSourceStr=; domainYardVal=%2D1; MemPDC1=; MemPDCLoc1=; GoogleADServicesgoogleremarketing=kiniggvgujhbbewjbildpgbemajjtflh; BlueLithium_yahooremarketing=ththwcigmeygpacjmgfgoiejldhbjfmh; __utma=247200914.511980882.1357766472.1357766472.1357766472.1; __utmc=247200914; __utmz=247200914.1357766472.1.1.utmcsr=idp.godaddy.com|utmccn=(referral)|utmcmd=referral|utmcct=/logout.aspx; adc1=US; countrysite1=uk; preferences1=_sid=mctibgffpgebcbehidwicemdifwejimd&gdshop_currencyType=CAD&countryFlag=us; MemAuthId1=njhbfhtdlamambjjdagamjfjyjsejhwgwhleebecdfsbpjojjcyhyjgdmiciocgh; ShopperId1=ahuijcvauaoabasdbfvihczffioapbfh; MemShopperId1=potableSourceStr=ahuijcvauaoabasdbfvihczffioapbfh; .ASPXAUTH=F9A44EAC87873A9BAA5979A508FCE6A7483E11D9A8A3F36042D146B32115DD79EC580F401F8CE1C9CB62AD252BC9B966279E2B4302613A3D8F50A044A9E67FDF99828D4ADCF425A92956CD059A9371CD77868785C560F56DC5527D3E7C878BB61625838BA9F6CAD600FF793D662ED0E4B3B13E09; fbiTrafficSettings=cDepth=32&resX=1366&resY=768&fMajorVer=-1&fMinorVer=-1&slMajorVer=-1&slMinorVer=-1; optimizelySegments=%7B%7D; traffic=; currency1=potableSourceStr=CAD; flag1=cflag=us; optimizelyBuckets=%7B%22125723167%22%3A%22125705946%22%2C%22134763940%22%3A%22134768852%22%2C%22138125576%22%3A%22138105718%22%2C%22167670554%22%3A%22167676213%22%7D; __utma=6661410.1037971913.1357419548.1357419548.1357766154.2; __utmb=6661410.8.10.1357766154; __utmc=6661410; __utmz=6661410.1357419548.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); SSOTimeStamp46311038=634933405541997282; pagecount=13; fb_sessionpagecount=13; actioncount=; fb_actioncount=; app_pathway=; fb_sessiontraffic=S_TOUCH=01/09/2013 22:00:10&pathway=855482c4-3df5-4b09-aa89-3ed17919a6d1&V_DATE=01/09/2013 14:15:56; visitor=vid=8fc6dfa5-d5d9-41b2-868d-57ce5979a64d

            //h=94322651
        }

        public void PlaceBid(string auctionRef, string bid)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var logindata = Login(Properties.Settings.Default.Username, Properties.Settings.Default.Password);
            HttpWebRequest request;
            HttpWebResponse response;
            var responseData = "";
            var strUrl = "https://auctions.godaddy.com/trpSearchResults.aspx";

            var postData = string.Format("action=review_selected_add&items={0}_B_{1}_1|&rnd={2}&JlPYXTX=347bde7", auctionRef, bid,
                randomDouble(0, 1).ToString("0.00000000000000000"));
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-powerpoint, application/vnd.ms-excel, application/x-silverlight, application/x-silverlight-2-b2, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";
            request.Headers.Add("Accept-Encoding", "deflate");
            request.Referer = "auctions.godaddy.com";
            request.Headers["my-header"] = "the-value";
            request.KeepAlive = true;
            request.CookieContainer = cookies;
            request.Timeout = Timeout.Infinite;

            var stOut = new StreamWriter(request.GetRequestStream());
            stOut.Write(postData);
            stOut.Flush();
            stOut.Close();
            stOut = null;

            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            var encoding = new System.Text.UTF8Encoding();
            var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();

            foreach (Cookie ck in response.Cookies)
            {
                request.CookieContainer.Add(ck);
            }

            strUrl = "https://auctions.godaddy.com/trpMessageHandler.aspx";
            postData = "q=ReviewDomains";
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-powerpoint, application/vnd.ms-excel, application/x-silverlight, application/x-silverlight-2-b2, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "deflate");
            request.Referer = "auctions.godaddy.com";
            request.Headers["my-header"] = "the-value";
            request.KeepAlive = true;
            request.CookieContainer = cookies;
            request.Timeout = Timeout.Infinite;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            stOut = new StreamWriter(request.GetRequestStream());
            stOut.Write(postData);
            stOut.Flush();
            stOut.Close();
            stOut = null;

            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();

            foreach (Cookie ck in response.Cookies)
            {
                request.CookieContainer.Add(ck);
            }

            strUrl = "https://auctions.godaddy.com/trpMessageHandler.aspx?keepAlive=1";
            postData = "";
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.CookieContainer = cookies;
            request.Referer = "https://auctions.godaddy.com/?t=22";
            request.Timeout = System.Threading.Timeout.Infinite;
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;

            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();
            foreach (Cookie ck in response.Cookies)
            {
                request.CookieContainer.Add(ck);
            }

            strUrl = "https://idp.godaddy.com/KeepAlive.aspx?SPKey=GDDNAEB003";
            postData = "";
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.CookieContainer = cookies;
            request.Referer = "https://auctions.godaddy.com/?t=22";
            request.Timeout = System.Threading.Timeout.Infinite;
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;

            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();
            foreach (Cookie ck in response.Cookies)
            {
                request.CookieContainer.Add(ck);
            }

            strUrl = string.Format("https://img.godaddy.com/pageevents.aspx?page_name=/trphome.aspx&ci=37022" +
                "&eventtype=click&ciimpressions=&usrin=&relativeX=659&relativeY=325&absoluteX=659&" +
                "absoluteY=1102&r={0}&comview=0", randomDouble(0, 1).ToString("0.00000000000000000"));
            postData = "";
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.CookieContainer = cookies;
            request.Referer = "https://auctions.godaddy.com/?t=22";
            request.Timeout = System.Threading.Timeout.Infinite;
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();
            foreach (Cookie ck in response.Cookies)
            {
                request.CookieContainer.Add(ck);
            }


            strUrl = @"https://auctions.godaddy.com/trpItemListingReview.aspx";
            postData = viewstates + string.Format("&hidAdvSearch=ddlAdvKeyword%3A1%7CtxtKeyword%3Aportal" +
                "%7CddlCharacters%3A0%7CtxtCharacters%3A%7CtxtMinTraffic%3A%7CtxtMaxTraffic%3A%" +
                "7CtxtMinDomainAge%3A%7CtxtMaxDomainAge%3A%7CtxtMinPrice%3A%7CtxtMaxPrice%3A%7Cdd" +
                "lCategories%3A0%7CchkAddBuyNow%3Afalse%7CchkAddFeatured%3Afalse%7CchkAddDash%3Atrue" +
                "%7CchkAddDigit%3Atrue%7CchkAddWeb%3Afalse%7CchkAddAppr%3Afalse%7CchkAddInv%3Afalse%7" +
                "CchkAddReseller%3Afalse%7CddlPattern1%3A%7CddlPattern2%3A%7CddlPattern3%3A%7CddlPattern4" +
                "%3A%7CchkSaleOffer%3Afalse%7CchkSalePublic%3Atrue%7CchkSaleExpired%3Afalse%7CchkSaleCloseouts" +
                "%3Afalse%7CchkSaleUsed%3Afalse%7CchkSaleBuyNow%3Afalse%7CchkSaleDC%3Afalse%7CchkAddOnSale" +
                "%3Afalse%7CddlAdvBids%3A0%7CtxtBids%3A%7CtxtAuctionID%3A%7CddlDateOffset%3A%7CddlRecordsPerPageAdv" +
                "%3A3&hidADVAction=p_&txtKeywordContext=&ddlRowsToReturn=3&hidAction=&hidItemsAddedToCart=" +
                "&hidGetMemberInfo=&hidShopperId=46311038&hidValidatedMemberInfo=&hidCheckedDomains=&hidMS90483566" +
                "=O&hidMS86848023=O&hidMS70107049=O&hidMS91154790=O&hidMS39351987=O&hidMS94284110=O&hidMS53775077=" +
                "O&hidMS75408187=O&hidMS94899096=B&hidMS94899097=B&hidMS94899098=B&hidMS94899099=B&hidMS94937468=" +
                "B&hidMS95047168=B&hidMS{0}=B&hid_Agree1=on", auctionRef);
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-powerpoint, application/vnd.ms-excel, application/x-silverlight, application/x-silverlight-2-b2, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "deflate");
            request.Referer = "auctions.godaddy.com";
            request.Headers["my-header"] = "the-value";
            request.KeepAlive = true;
            request.CookieContainer = cookies;
            request.Timeout = Timeout.Infinite;

            stOut = new StreamWriter(request.GetRequestStream());
            stOut.Write(postData);
            stOut.Flush();
            stOut.Close();
            stOut = null;

            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();
        }


        public static string ExtractViewStateSearch(string viewstatestring)
        {
            try
            {
                string viewStateNameDelimiter = "__VIEWSTATE";
                string valueDelimiter = "value=\"";

                int viewStateNamePosition = viewstatestring.IndexOf(viewStateNameDelimiter);
                int viewStateValuePosition = viewstatestring.IndexOf(valueDelimiter, viewStateNamePosition);

                int viewStateStartPosition = viewStateValuePosition + valueDelimiter.Length;
                int viewStateEndPosition = viewstatestring.IndexOf("\"", viewStateStartPosition);

                viewstatestring = viewstatestring.Substring(
                            viewStateStartPosition,
                            viewStateEndPosition - viewStateStartPosition);
            }
            catch (Exception)
            {
                    
            }
            

            return viewstatestring;
        }

        public string Login(string UserName, string Pwd)
        {   
            HttpWebRequest request;
            HttpWebResponse response;

            var responseData = "";
            var strUrl = @"https://auctions.godaddy.com/";
            var postData = "";
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.CookieContainer = cookies;
            request.Timeout = System.Threading.Timeout.Infinite;
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;


            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            var encoding = new System.Text.UTF8Encoding();
            var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();

            var key = GetSubString(responseData, "spkey=", "&");

            foreach (Cookie ck in response.Cookies)
            {
                request.CookieContainer.Add(ck);
            }

            strUrl = string.Format("https://idp.godaddy.com/login.aspx?SPKey={0}", key);
            postData = string.Format("loginName={0}&password={1}", UserName, Pwd);
            //viewstates = string.Format("__VIEWSTATE={0}", HttpUtility.UrlEncode(ExtractViewStateSearch(responseData)));
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-powerpoint, application/vnd.ms-excel, application/x-silverlight, application/x-silverlight-2-b2, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "deflate");
            request.Referer = "auctions.godaddy.com";
            request.Headers["my-header"] = "the-value";
            request.KeepAlive = true;
            request.CookieContainer = cookies;
            request.Timeout = Timeout.Infinite;

            var stOut = new StreamWriter(request.GetRequestStream());
            stOut.Write(postData);
            stOut.Flush();
            stOut.Close();
            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();

            //tester();

            return responseData;
        }

        private void tester()
        {
            HttpWebRequest request;
            HttpWebResponse response;

            var responseData = "";
            var strUrl = @"https://mya.godaddy.com/payments.aspx?ci=83668&paymentstab=productbilling";
            var postData = "";
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.CookieContainer = cookies;
            request.Timeout = System.Threading.Timeout.Infinite;
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;


            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            var encoding = new System.Text.UTF8Encoding();
            var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();
        }

        public string LoginOld(string UserName, string Pwd)
        {
            HttpWebRequest request;
            HttpWebResponse response;

            var responseData = "";
            var strUrl = @"https://auctions.godaddy.com/";
            var postData = "";
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.CookieContainer = cookies;
            request.Timeout = System.Threading.Timeout.Infinite;
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;


            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            var encoding = new System.Text.UTF8Encoding();
            var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();

            int _firstindex = responseData.IndexOf("idp.godaddy.com/login.aspx?");
            int _lastindex = responseData.IndexOf("nvar pcj_navnm");

            string caseID = responseData.Substring(_firstindex, (_lastindex + 64)).Replace("\">\r\n\t\t\t\t\t\t<input typ", "");

            strUrl = @"https://" + caseID + "&target=http://auctions.godaddy.com/";
            postData = "";
            //cookies = new CookieContainer();
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-powerpoint, application/vnd.ms-excel, application/x-silverlight, application/x-silverlight-2-b2, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
            request.Headers.Add("Accept-Language", "en-us");
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "");
            request.KeepAlive = true;
            request.CookieContainer = cookies;
            request.Timeout = System.Threading.Timeout.Infinite;


            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();

            foreach (Cookie ck in response.Cookies)
            {
                request.CookieContainer.Add(ck);
            }

            strUrl = @"https://" + caseID + "&target=https://auctions.godaddy.com";
            postData = string.Format("__VIEWSTATE={0}&Login%24userEntryPanel2%24UsernameTextBox=" + UserName + "&Login%24userEntryPanel2%24PasswordTextBox=" + Pwd + "&Login%24userEntryPanel2%24LoginImageButton.x=42&Login%24userEntryPanel2%24LoginImageButton.y=8", HttpUtility.UrlEncode(ExtractViewStateSearch(responseData)));
            viewstates = string.Format("__VIEWSTATE={0}", HttpUtility.UrlEncode(ExtractViewStateSearch(responseData)));
            //cookies = new CookieContainer();
            request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-powerpoint, application/vnd.ms-excel, application/x-silverlight, application/x-silverlight-2-b2, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.Headers.Add("Accept-Encoding", "deflate");
            request.Referer = "auctions.godaddy.com";
            request.Headers["my-header"] = "the-value";
            request.KeepAlive = true;
            request.CookieContainer = cookies;
            request.Timeout = Timeout.Infinite;

            var stOut = new StreamWriter(request.GetRequestStream());
            stOut.Write(postData);
            stOut.Flush();
            stOut.Close();
            stOut = null;

            response = (HttpWebResponse)request.GetResponse();
            response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

            encoding = new System.Text.UTF8Encoding();
            responseReader = new StreamReader(response.GetResponseStream(), encoding, true);

            responseData = responseReader.ReadToEnd();
            response.Close();
            responseReader.Close();



            return responseData;
        }

    }
}