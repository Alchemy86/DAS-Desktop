using HtmlAgilityPack;
using LunchboxSource.Business.SiteObject.GoDaddy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuctionSniperService.Business;

namespace AuctionSniper.Business.Obj
{
    class VpnFactory: Site
    {
        private string navigateUrl = "http://www.vpngeeks.com/proxylist.php?country={0}&port=&speed%5B%5D=3&anon%5B%5D=2&anon%5B%5D=3&type%5B%5D=1&type%5B%5D=2&type%5B%5D=3&conn%5B%5D=3&sort=1&order=1&rows=50&search=Find+Proxy#pagination";

        public VpnFactory()
            : base()
        {

        }

        private string GetCountry()
        {
            List<string> countries = new List<string>();
            countries.Add("af");
            countries.Add("al");
            countries.Add("ar");
            countries.Add("au");
            countries.Add("at");
            countries.Add("bd");
            countries.Add("be");
            countries.Add("bo");
            countries.Add("br");
            countries.Add("bn");
            countries.Add("bg");
            countries.Add("kh");
            countries.Add("af");
            countries.Add("ca");
            countries.Add("cl");
            countries.Add("cn");
            countries.Add("co");
            countries.Add("cr");
            countries.Add("ci");
            countries.Add("hr");
            countries.Add("cz");
            countries.Add("dk");
            countries.Add("ec");
            countries.Add("eg");
            countries.Add("fr");
            countries.Add("ga");
            countries.Add("de");
            countries.Add("gr");
            countries.Add("hk");
            countries.Add("hu");
            countries.Add("in");
            countries.Add("id");
            countries.Add("ir");
            countries.Add("iq");
            countries.Add("ie");
            countries.Add("it");
            countries.Add("jp");
            countries.Add("lv");
            countries.Add("lt");
            countries.Add("mx");
            countries.Add("np");
            countries.Add("nl");
            countries.Add("ng");
            countries.Add("no");
            countries.Add("pk");
            countries.Add("py");
            countries.Add("pe");
            countries.Add("ph");
            countries.Add("pr");
            countries.Add("qa");
            countries.Add("ro");
            countries.Add("ru");
            countries.Add("sa");
            countries.Add("rs");
            countries.Add("sg");
            countries.Add("si");
            countries.Add("es");
            countries.Add("se");
            countries.Add("ch");
            countries.Add("tr");
            countries.Add("gb");
            countries.Add("us");
            countries.Add("uy");
            countries.Add("ve");
            countries.Add("vn");

            Random r = new Random();
            return countries[r.Next(0, countries.Count)];
        }

        public Dictionary<string, int> Generate()
        {
            Dictionary<string, int> proxies = new Dictionary<string, int>();
            string nation = GetCountry();
            HtmlDocument d = this.Download(string.Format(navigateUrl, nation));
            int count = 0;
            foreach (var prox in QuerySelectorAll(d.DocumentNode, "table[width='1100'] tr"))
            {
                if (count != 0)
                {
                    try
                    {
                        proxies.Add(QuerySelector(prox, "td").InnerText.Trim(),
                                                int.Parse(QuerySelector(prox, "td:nth-child(2)").InnerText.Trim()));
                    }
                    catch (Exception) { break; }
                    
                }
                count++;

            }
            if (proxies.Count < 5)
            {
                return Generate();
            }
            this.Msg(ErrorSeverity.Process, "Gathering Proxy List..");
            this.Msg(ErrorSeverity.Process, "Origin set to: " + nation);
            this.Msg(ErrorSeverity.Process, proxies.Count.ToString() + " Located..");
            return proxies;
        }

 
            
        public override SortableBindingList<Auction> GetAuctions()
        {
            return null;
        }
        
    }
}
