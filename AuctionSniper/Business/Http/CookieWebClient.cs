namespace AuctionSniper.Business.Http
{
    using System;
    using System.Net;

    public class CookieWebClient : WebClient
    {
            
        public CookieContainer m_container = new CookieContainer();
        public WebProxy proxy = null;

        protected override WebRequest GetWebRequest(Uri address)
        {
            try
            {
                ServicePointManager.DefaultConnectionLimit = 1000000;
                WebRequest request = base.GetWebRequest(address);
                request.Proxy = proxy;
                request.Timeout = 10000;
                HttpWebRequest webRequest = request as HttpWebRequest;
                webRequest.Pipelined = true;
                webRequest.KeepAlive = true;
                webRequest.AllowAutoRedirect = true;
                if (webRequest != null)
                {
                    webRequest.CookieContainer = m_container;
                }

                return request;
            }
            catch
            {
                return null;
            }
        }
    }
}
