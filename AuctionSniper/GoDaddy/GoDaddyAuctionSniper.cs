using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DAL;
using DAS.Domain;
using DAS.Domain.GoDaddy;
using DAS.Domain.GoDaddy.Users;
using DAS.Domain.Users;
using DeathByCaptcha;
using HtmlAgilityPack;
using Lunchboxweb;
using Exception = System.Exception;

namespace GoDaddy
{
    public class GoDaddyAuctionSniper : HttpBase
    {
        public IUserRepository UserRepository;
        private readonly IGoDaddySession _sessionDetails;
        private string Viewstates { get; set; }
        public GoDaddyAuctionSniper(string userName, IUserRepository userRepository)
        {
            UserRepository = userRepository;
            _sessionDetails = userRepository.GetSessionDetails(userName);
        }

        private const int Timediff = 0;
        private string Viewstate { get; set; }

        public bool CaptchaOverload { get; set; }

        /// <summary>
        /// Checks to see if you are still logged in
        /// </summary>
        /// <returns></returns>
        public bool LoggedIn(string html)
        {
            return html.Contains("sessionTimeout_onLogout");
        }

        public bool LoggedIn()
        {
            const string url = "https://auctions.godaddy.com/";
            var responseData = Get(url);
            return responseData.Contains("sessionTimeout_onLogout");
        }

        /// <summary>
        /// Login to godaddy auctions
        /// </summary>
        /// <returns></returns>
        public bool Login(int attempNo = 0)
        {
            const string url = "https://auctions.godaddy.com/";
            var responseData = Get(url);
            //Skip if we are logged in
            if (LoggedIn(responseData))
            {
                return true;
            }
            var key = GetSubString(responseData, "spkey=", "\"");
            var loginurl = string.Format("https://idp.godaddy.com/login.aspx?SPKey={0}", key);
            var hdoc = HtmlDocument(Get(loginurl));
            if (QuerySelector(hdoc.DocumentNode, "img[class='LBD_CaptchaImage']") != null)
            {
                //Solve Captcha
                var captchaId =
                    QuerySelector(hdoc.DocumentNode, "input[id='LBD_VCID_idpCatpcha']").Attributes["value"]
                        .Value;
                var imagedata =
                    GetImage(QuerySelector(hdoc.DocumentNode, "img[class='LBD_CaptchaImage']").Attributes["src"].Value);


                try
                {
                    imagedata.Save(Path.Combine(Path.GetTempPath(), _sessionDetails.GoDaddyAccount.Username + ".jpg"), ImageFormat.Jpeg);
                    Client client;
                    var user = _sessionDetails.DeathByCapture.Username;
                    var pass = _sessionDetails.DeathByCapture.Password;
                    client = new SocketClient(user, pass);

                    //var balance = client.GetBalance();
                    var captcha = client.Decode(Path.Combine(Path.GetTempPath(), _sessionDetails.GoDaddyAccount.Username + ".jpg"), 20);
                    if (null != captcha)
                    {
                        /* The CAPTCHA was solved; captcha.Id property holds its numeric ID,
                           and captcha.Text holds its text. */
                        Console.WriteLine(@"CAPTCHA {0} solved: {1}", captcha.Id, captcha.Text);
                        var capturetext = captcha.Text;

                        var view = ExtractViewStateSearch(hdoc.DocumentNode.InnerHtml);
                        //var view = QuerySelector(hdoc.DocumentNode, "input[id='__VIEWSTATE'") == null ? "" :
                        //QuerySelector(hdoc.DocumentNode, "input[id='__VIEWSTATE'").Attributes["value"].Value;
                        var postData =
                            string.Format(
                                "__VIEWSTATE={0}&Login%24userEntryPanel2%24UsernameTextBox={1}&Login%24userEntryPanel2%24PasswordTextBox={2}&captcha_value={3}&LBD_VCID_idpCatpcha={4}&Login%24userEntryPanel2%24LoginImageButton.x=0&Login%24userEntryPanel2%24LoginImageButton.y=0",
                                view, _sessionDetails.GoDaddyAccount.Username, _sessionDetails.GoDaddyAccount.Password, capturetext, captchaId);

                        responseData = Post(loginurl, postData);

                        if (!responseData.Contains("sessionTimeout_onLogout"))
                        {
                            client.Report(captcha);
                        }

                        return responseData.Contains("sessionTimeout_onLogout");
                    }
                }
                catch (Exception e)
                {
                    UserRepository.LogError(e.Message);
                    CaptchaOverload = true;
                }
            }
            else
            {
                var secondaryLogin = string.Format("https://sso.godaddy.com/?app=idp&path=%2flogin.aspx%3fSPKey%3d{0}", key);

                var loginData = string.Format("loginName={0}&password={1}",
                    Uri.EscapeDataString(_sessionDetails.GoDaddyAccount.Username),
                    Uri.EscapeDataString(_sessionDetails.GoDaddyAccount.Password));
                var firstLoginResponse = Post(loginurl, loginData);
                var login2Data = string.Format("name={0}&password={1}",
                    Uri.EscapeDataString(_sessionDetails.GoDaddyAccount.Username),
                    Uri.EscapeDataString(_sessionDetails.GoDaddyAccount.Password));

                var secondaryLoginResponse = Post(secondaryLogin, login2Data);

                return LoggedIn(secondaryLoginResponse);
            }

            if (attempNo < 3)
            {
                Login(attempNo++);
            }

            return false;
        }

        private AuctionSearch GenerateAuctionSearch()
        {
            var p = new AuctionSearch();
            return p;
        }

        /// <summary>
        /// Check a domain to confrim you have won
        /// </summary>
        /// <param name="domainName">Domain name to check</param>
        /// <returns></returns>
        public bool WinCheck(string domainName)
        {
            if (!LoggedIn(Get("https://auctions.godaddy.com")))
            {
                Login();
            }
            const string url = "https://auctions.godaddy.com/trpMessageHandler.aspx ";
            //var postData = string.Format("sec=Wo&sort=6&dir=D&page=1&rpp=50&at=0&rnd={0}", RandomDouble(0, 1).ToString("0.00000000000000000"));
            var postData = string.Format("sec=Wo&sort=6&dir=D&page=1&rpp=50&at=0&maadv=0|{0}|||&rnd={1}", domainName, RandomDouble().ToString("0.00000000000000000"));
            var data = Post(url, postData);

            return data.Contains(domainName);
        }

        /// <summary>
        /// Random double generator for requests
        /// </summary>
        /// <returns></returns>
        private static double RandomDouble()
        {
            var rand = new Random();
            return (rand.NextDouble() * Math.Abs(1 - 0)) + 0;
        }

        /// <summary>
        /// Gets the current min bid for the auction
        /// </summary>
        /// <param name="auctionRef"></param>
        /// <returns></returns>
        public int CheckAuction(string auctionRef)
        {
            const string url = "https://auctions.godaddy.com/trpItemListing.aspx?miid={0}";
            var data = Get(string.Format(url, auctionRef));

            var minbid =
                TextModifier.TryParse_INT(
                    GetSubString(HtmlDecode(data), "or more', ",
                        ")").Trim().Replace(",", "").Replace("$", ""));

            //var end = GetEndDate(auctionRef);


            return minbid;
        }

        /// <summary>
        /// Returns the Pacific time
        /// </summary>
        /// <returns></returns>
        public DateTime GetPacificTime
        {
            get
            {
                var tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);

                return localDateTime;
            }
        }

        /// <summary>
        /// Gets the auction details
        /// </summary>
        /// <param name="auctionNo"></param>
        /// <returns></returns>
        private HtmlDocument GetAuctionDetails(string auctionNo)
        {
            var data = Post("https://auctions.godaddy.com/trpMessageHandler.aspx", string.Format("ad={0}&type=Search", auctionNo));
            var hdoc = HtmlDocument(data);

            return hdoc;
        }

        public DateTime GetEndDate(string auctionNo)
        {
            var endDate = GetPacificTime;
            var details = GetAuctionDetails(auctionNo);
            if (QuerySelector(details.DocumentNode, "span.OneLinkNoTx") != null)
            {
                endDate = (QuerySelector(details.DocumentNode, "span.OneLinkNoTx").InnerText.Contains("PM") &&
                    DateTime.Parse(QuerySelector(details.DocumentNode, "span.OneLinkNoTx").InnerText.Replace("AM", "").Replace("PM", "").Replace("(PST)", "").Replace("(PDT)", "").Trim(), new CultureInfo("en-US", false)).Hour < 12) ?
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

            return endDate;
        }

        public IQueryable<Auction> Search(string searchText, bool auctiononly, Guid AccountID)
        {
            var search = Search(searchText);
            var results = new List<Auction>();

            foreach (var e in search)
            {
                results.Add(new Auction
                {
                    AccountId = AccountID,
                    AuctionId = Guid.NewGuid(),
                    AuctionRef = e.AuctionRef,
                    DomainName = e.DomainName,
                    EndDate = e.EndDate,
                    MyBid = e.MyBid,
                    MinBid = e.MinBid
                });
            }

            return results.AsQueryable();
        }

        public IQueryable<AuctionSearch> Search(string searchText)
        {
            const string searchString = "https://auctions.godaddy.com/trpSearchResults.aspx";
            var auctions = new SortableBindingList<AuctionSearch>();

            var doc = HtmlDocument(Post(searchString,
                "t=16&action=search&hidAdvSearch=ddlAdvKeyword:1|txtKeyword:"+
                searchText.Replace(" ",",")
                +"|ddlCharacters:0|txtCharacters:|txtMinTraffic:|txtMaxTraffic:|txtMinDomainAge:|txtMaxDomainAge:|txtMinPrice:|txtMaxPrice:|ddlCategories:0|chkAddBuyNow:false|chkAddFeatured:false|chkAddDash:true|chkAddDigit:true|chkAddWeb:false|chkAddAppr:false|chkAddInv:false|chkAddReseller:false|ddlPattern1:|ddlPattern2:|ddlPattern3:|ddlPattern4:|chkSaleOffer:false|chkSalePublic:true|chkSaleExpired:true|chkSaleCloseouts:false|chkSaleUsed:false|chkSaleBuyNow:false|chkSaleDC:false|chkAddOnSale:false|ddlAdvBids:0|txtBids:|txtAuctionID:|ddlDateOffset:&rtr=2&baid=-1&searchDir=1&rnd=0.899348703911528&jnkRjrZ=6dd022d"));


            if (QuerySelectorAll(doc.DocumentNode, "tr.srRow2, tr.srRow1") != null)
            {
                foreach (var node in QuerySelectorAll(doc.DocumentNode, "tr.srRow2, tr.srRow1"))
                {
                    if (QuerySelector(node, "span.OneLinkNoTx") != null && QuerySelector(node, "td:nth-child(5)") != null)
                    {
                        AuctionSearch auction = GenerateAuctionSearch();
                        auction.DomainName = HtmlDecode(QuerySelector(node, "span.OneLinkNoTx").InnerText);
                        Console.WriteLine(auction.DomainName);

                        auction.BidCount = TextModifier.TryParse_INT(HtmlDecode(QuerySelector(node, "td:nth-child(5)").FirstChild.InnerHtml.Replace("&nbsp;", "")));
                        auction.Traffic = TextModifier.TryParse_INT(HtmlDecode(QuerySelector(node, "td:nth-child(5) > td").InnerText.Replace("&nbsp;", "")));
                        auction.Valuation = TextModifier.TryParse_INT(HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(2)").InnerText.Replace("&nbsp;", "")));
                        auction.Price = TextModifier.TryParse_INT(HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(3)").InnerText).Replace("$", "").Replace(",", "").Replace("C", ""));
                        try
                        {
                            if (QuerySelector(node, "td:nth-child(5) > td:nth-child(4) > div") != null)
                            {
                                if (HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(4) > div").InnerText).Contains("Buy Now for"))
                                {
                                    auction.BuyItNow = TextModifier.TryParse_INT(Regex.Split(HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(4) > div").InnerText), "Buy Now for")[1].Trim().Replace(",", "").Replace("$", ""));
                                }

                            }
                            else
                            {
                                auction.BuyItNow = 0;
                            }

                        }
                        catch (Exception) { auction.BuyItNow = 0; }

                        if (QuerySelector(node, "td:nth-child(5) > td:nth-child(5)") != null &&
                            HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(5)").InnerHtml).Contains("Bid $"))
                        {
                            auction.MinBid = TextModifier.TryParse_INT(GetSubString(HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(5)").InnerHtml), "Bid $", " or more").Trim().Replace(",", "").Replace("$", ""));
                        }
                        if (QuerySelector(node, "td:nth-child(5) > td:nth-child(5)") != null &&
                            !HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(5)").InnerHtml).Contains("Bid $"))
                        {
                            auction.EstimateEndDate = GenerateEstimateEnd(QuerySelector(node, "td:nth-child(5) > td:nth-child(5)"));
                        }
                        if (QuerySelector(node, "td:nth-child(5) > td:nth-child(4)") != null &&
                            HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(4)").InnerHtml).Contains("Bid $"))
                        {
                            auction.MinBid = TextModifier.TryParse_INT(GetSubString(HtmlDecode(QuerySelector(node, "td:nth-child(5) > td:nth-child(4)").InnerHtml), "Bid $", " or more").Trim().Replace(",", "").Replace("$", ""));
                        }
                        if (QuerySelector(node, "td > div > span") != null)
                        {
                            foreach (var item in GetSubStrings(QuerySelector(node, "td > div > span").InnerHtml, "'Offer $", " or more"))
                            {
                                auction.MinOffer = TextModifier.TryParse_INT(item.Replace(",", ""));
                            }
                        }
                        auction.EndDate = GetPacificTime;
                        foreach (var item in GetSubStrings(node.InnerHtml, "ShowAuctionDetails('", "',"))
                        {
                            auction.AuctionRef = item;
                            break;
                        }
                        //AppSettings.Instance.AllAuctions.Add(auction);
                        if (auction.MinBid > 0)
                        {
                            auctions.Add(auction);
                        }

                    }

                }
            }

            return auctions.AsQueryable();

        }

        private DateTime GenerateEstimateEnd(HtmlNode node)
        {
            var estimateEnd = GetPacificTime;
            if (node.InnerText == null) return estimateEnd;
            var vals = HtmlDecode(node.InnerText).Trim().Split(new[] { ' ' });

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

            return estimateEnd;
        }

        private bool UnavailableCheck(string auctionRef)
        {
            var data = Get(string.Format("https://auctions.godaddy.com/trpItemListing.aspx?miid={0}", auctionRef));
            return data.Contains("LONGER AVAILABLE THROUGH AUCTION");
            //LONGER AVAILABLE THROUGH AUCTION
        }

        public void PlaceBid(Auction auction)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var url = "https://auctions.godaddy.com/trpSearchResults.aspx";

            if (UnavailableCheck(auction.AuctionRef))
            {
                UserRepository.AddHistoryRecord("The site is no longer available through auction, bid cancelled", auction.AuctionId);
                return;
            }

            UserRepository.AddHistoryRecord("Logging In", auction.AuctionId);

            if (Login())
            {
                var postData = string.Format("action=review_selected_add&items={0}_B_{1}_1|&rnd={2}&JlPYXTX=347bde7",
                    auction.AuctionRef, auction.MyBid,
                    RandomDouble().ToString("0.00000000000000000"));
                var responseData = Post(url, postData);

                UserRepository.AddHistoryRecord("Setting Max Bid: " + auction.MyBid, auction.AuctionId);

                //Review
                url = "https://auctions.godaddy.com/trpMessageHandler.aspx";
                postData = "q=ReviewDomains";
                responseData = Post(url, postData);
                if (responseData.Contains("Item is closed"))
                {
                    UserRepository.AddHistoryRecord("Bid Process Ended - The item has been closed", auction.AuctionId);
                    return;
                }
                if (responseData.Contains("ERROR: Bid must be a minimum of"))
                {
                    UserRepository.AddHistoryRecord("Bid Process Ended - Your max bid is already too small to place", auction.AuctionId);
                    return;
                }
                if (responseData.Contains("You are currently blocked from bidding due to unpaid items"))
                {
                    UserRepository.AddHistoryRecord("GoDaddy reports you are blocked from bidding due to unpaid items", auction.AuctionId);
                    return;
                }

                //KeepAlive
                Get("https://auctions.godaddy.com/trpMessageHandler.aspx?keepAlive=1");
                Get("https://idp.godaddy.com/KeepAlive.aspx?SPKey=GDDNAEB002");

                url = string.Format("https://img.godaddy.com/pageevents.aspx?page_name=/trphome.aspx&ci=37022" +
                                    "&eventtype=click&ciimpressions=&usrin=&relativeX=659&relativeY=325&absoluteX=659&" +
                                    "absoluteY=1102&r={0}&comview=0", RandomDouble().ToString("0.00000000000000000"));
                responseData = Get(url);

                // Buy
                //var view = ExtractViewStateSearch(responseData);
                url = @"https://auctions.godaddy.com/trpItemListingReview.aspx";
                postData = "__VIEWSTATE=" + Viewstates +
                           string.Format("&hidAdvSearch=ddlAdvKeyword%3A1%7CtxtKeyword%3Aportal" +
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
                                         "B&hidMS95047168=B&hidMS{0}=B&hid_Agree1=on", auction.AuctionRef);

                UserRepository.AddHistoryRecord("Bid Process Completed", auction.AuctionId);
                var hdoc = HtmlDocument(Post(url, postData));
                var confirmed = false;
                foreach (var items in QuerySelectorAll(hdoc.DocumentNode, "tr"))
                {
                    if (items.InnerHtml.Contains(auction.AuctionRef) && items.InnerHtml.Contains("the high bidder"))
                    {
                        confirmed = true;
                        UserRepository.AddHistoryRecord("Bid Confirmed - You are the high bidder!", auction.AuctionId);
                        break;
                    }
                    if (items.InnerHtml.Contains(auction.AuctionRef) && items.InnerHtml.Contains("ERROR: Not an auction"))
                    {
                        confirmed = true;
                        UserRepository.AddHistoryRecord("Bid Failed - The site is no longer an auction", auction.AuctionId);
                        break;
                    }

                }
                if (hdoc.DocumentNode.InnerHtml.Contains("Confirmed Domains") && hdoc.DocumentNode.InnerHtml.Contains(auction.DomainName)
                    && hdoc.DocumentNode.InnerHtml.Contains("the high bidder"))
                {
                    confirmed = true;
                    UserRepository.AddHistoryRecord("Bid Confirmed - You are the high bidder!", auction.AuctionId);
                }
                if (!confirmed)
                {
                    UserRepository.AddHistoryRecord("Bid Not Confirmed - Data logged", auction.AuctionId);
                }
            }
            else
            {
                UserRepository.AddHistoryRecord(
                    CaptchaOverload
                        ? "Appologies - 3rd party capture solve failure. This has been reported."
                        : "Appologies - Login to account has failed. 3 Seperate attempts made", auction.AuctionId);
            }

        }


    }
}
