using System;
using AuctionSniper.Business.Encryption;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Xml.Serialization;
using DAS.Domain;
using DAS.Domain.GoDaddy;
using DAS.Domain.GoDaddy.Users;
using DAS.Domain.Users;
using GoDaddy;

namespace AuctionSniper.Business
{
    public class AppSettings : ManagedObject
    {
        public static AppSettings Instance = new AppSettings();
        private GoDaddyAuctionSniper godaddy;
        private bool testMode = false;
        private MySqlConnection mysqlConn = new MySqlConnection();
        private SortableBindingList<Auction> allProperties = new SortableBindingList<Auction>();
        private SortableBindingList<Auction> myAuctions = new SortableBindingList<Auction>();
        private List<string> Areas = new List<string>();
        private Auction selectedAuction = new Auction();
        private IUserRepository UserRepository;

        //055253075052187226128207233220040152152032035046195202074078254081075028076127031045172195038169141118199243064212129091192167107176166003254184236133056033107091155065009100204161181134040023198005237180084216208250147177192109190224003224176006155200168216249015086108126048252004135184
        public AppSettings()
        {
            #region Areas
            Areas.Add("AL");
            Areas.Add("AK");
            Areas.Add("AS");
            Areas.Add("AZ");
            Areas.Add("AR");
            Areas.Add("CA");
            Areas.Add("CO");
            Areas.Add("CT");
            Areas.Add("DE");
            Areas.Add("DC");
            Areas.Add("FM");
            Areas.Add("FL");
            Areas.Add("GA");
            Areas.Add("GU");
            Areas.Add("HI");
            Areas.Add("ID");
            Areas.Add("IL");
            Areas.Add("IN");
            Areas.Add("IA");
            Areas.Add("KS");
            Areas.Add("KY");
            Areas.Add("LA");
            Areas.Add("ME");
            Areas.Add("MH");
            Areas.Add("MD");
            Areas.Add("MA");
            Areas.Add("MI");
            Areas.Add("MN");
            Areas.Add("MS");
            Areas.Add("MO");
            Areas.Add("MT");
            Areas.Add("NE");
            Areas.Add("NV");
            Areas.Add("NH");
            Areas.Add("NJ");
            Areas.Add("NM");
            Areas.Add("NY");
            Areas.Add("NC");
            Areas.Add("ND");
            Areas.Add("MP");
            Areas.Add("OH");
            Areas.Add("OK");
            Areas.Add("OR");
            Areas.Add("PW");
            Areas.Add("PA");
            Areas.Add("PR");
            Areas.Add("RI");
            Areas.Add("SC");
            Areas.Add("SD");
            Areas.Add("TN");
            Areas.Add("TX");
            Areas.Add("UT");
            Areas.Add("VT");
            Areas.Add("VI");
            Areas.Add("VA");
            Areas.Add("WA");
            Areas.Add("WV");
            Areas.Add("WI");
            Areas.Add("WY");
            #endregion
        }
        public List<string> US_States
        {
            get { return this.Areas; }
            set
            {
                this.CheckPropertyChanged<List<string>>
                ("US_States", ref this.Areas, ref value);
            }
        }

        public User LiveUserAccount { get; set; }

        public GoDaddySessionModel SessionDetails { get; set; }

        public bool TestMode
        {
            get { return this.testMode; }
            set
            {
                this.CheckPropertyChanged<bool>
                ("TestMode", ref this.testMode, ref value);
            }
        }

        public MySqlConnection MySqlConn
        {
            get
            {
                mysqlConn.ConnectionString = EncryptionHelper.Instance.DecryptString("055253075052187226128207233220040152152032035046195202074078254081075028076127031045172195038169141118199243064212129091192167107176166003254184236133056033107091155065009100204161181134040023198005237180084216208250147177192109190224003224176006155200168216249015086108126048252004135184");
                return mysqlConn;
            }
        }

        public Auction CurrentAuction
        {
            get { return this.selectedAuction; }
            set
            {
                this.CheckPropertyChanged<Auction>
                ("CurrentAuction", ref this.selectedAuction, ref value);
            }
        }

        public GoDaddyAuctionSniper GoDaddy
        {
            get { return this.godaddy; }
            set
            {
                this.CheckPropertyChanged
                ("GoDaddy", ref godaddy, ref value);
            }
        }

        public SortableBindingList<Auction> AllAuctions
        {   
            get { return this.allProperties; }
            set
            {
                this.CheckPropertyChanged<SortableBindingList<Auction>>
                ("AllProperties", ref this.allProperties, ref value);
            }
        }

        public SortableBindingList<Auction> MyAuctions
        {
            get { return myAuctions; }
            set
            {
                CheckPropertyChanged
                ("MyAuctions", ref myAuctions, ref value);
            }
        }


    }
}
