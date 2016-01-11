using System.Windows.Forms;

namespace AuctionSniper.Business.DataAccess
{
    using System;
    using System.Data;
    using MySql.Data.MySqlClient;
    using AuctionSniper.Business.Encryption;

    public class DBHelper
    {
    /// <summary>
        /// Query a database and return the dataset
        /// </summary>
        /// <param name="query"> Query String</param>
        /// <returns></returns>
        public static DataTable SQLSelect(string query, string connString)
        {
            DataTable dataset = new DataTable();
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            adapter.SelectCommand = new MySqlCommand(query, conn);
            adapter.Fill(dataset);
            conn.Close();
            return dataset;
        }

        /// <summary>
        /// Run an update query
        /// </summary>
        /// <param name="query">Update string</param>
        /// <returns>Success Status</returns>
        public static bool SQLUpdate(string query)
        {
            MySqlConnection conn =
                new MySqlConnection(EncryptionHelper.Instance.DecryptString(Properties.Settings.Default.MySQLConn.Trim()));
            try
            {
            MySqlCommand command = conn.CreateCommand();
            conn.Open();
            command.CommandText = query;
            
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                conn.Close();
                return false;
            }
            conn.Close();
            return true;
        }

        public static string MD5(string password)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(password);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            password = s.ToString();
            return password;
        }

        public static void LogBug(string software, string error, string user)
        {
            var api = new domainauctionsniperAPI.LunchboxCodeAPI();
            api.ReportBug("Auction Sniper : Desktop", error, user);
            api.Email("codebyexample@gmail.com", "Auction Sniper : Desktop Error", user + ": " +error, "Auction Sniper : Desktop");
        }


        public static bool Login(string email, string password, string application)
        {
            var login = false;
            try
            {
                var api = new domainauctionsniperAPI.LunchboxCodeAPI();
                login = api.Login(email, MD5(password), application).Equals("1");
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return login;
        }
    }
}
