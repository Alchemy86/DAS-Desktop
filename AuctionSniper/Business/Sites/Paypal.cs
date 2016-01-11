using System.Text;
using System.Web;

namespace AuctionSniper.Business.Sites
{
    class PayPal
    {
        /// <summary>
        /// Creates Payments URL
        /// </summary>
        /// <param name="itemName">Name</param>
        /// <param name="itemNumber">Number</param>
        /// <param name="price">Price</param>
        /// <param name="tax">Tax</param>
        /// <param name="shipping">Shipping</param>
        /// <param name="Currecny">USD?GBP?</param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static string GeneratePaymentRequest(string itemName, string itemNumber,
            double price, double tax, double shipping, string Currecny, string userName)
        {
            StringBuilder url = new StringBuilder();
            string serverURL = "https://www.paypal.com/us/cgi-bin/webscr"; //"https://www.sandbox.paypal.com/us/cgi-bin/webscr";

            url.Append(serverURL + "?cmd=_xclick&currency_code=" + Currecny + "&business=" +
                  HttpUtility.UrlEncode(userName));
            url.Append("&amount=" + price.ToString().Replace(",", "."));


            if (tax > 0)
                url.AppendFormat("&tax=" + tax.ToString().Replace(",", "."));

            if (shipping > 0)
                url.AppendFormat("&shipping=" + shipping.ToString().Replace(",", "."));

            url.AppendFormat("&item_name={0}", HttpUtility.UrlEncode(itemName));
            url.AppendFormat("&item_number={0}", HttpUtility.UrlEncode(itemNumber));
            url.AppendFormat("&custom={0}", HttpUtility.UrlEncode(userName));

            return url.ToString();

        }
    }
}
