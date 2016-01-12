using AuctionSniper.Properties;
using AuctionSniper.UI.SplashScreen;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AuctionSniper.Business;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using AuctionSniper.DAL.Repository;
using AuctionSniper.Domain.Godaddy;
using DAS.Domain;
using DAS.Domain.GoDaddy;
using DAS.Domain.GoDaddy.Users;
using DAS.Domain.Users;
using GoDaddy;
using LunchboxSource.Business.UI;
using Lunchboxweb.BaseFunctions;
using Ninject;

namespace AuctionSniper
{
    public partial class Form1 : Form
    {
        BindingSource bs = new BindingSource();
        public static Form1 Instance;
        public IKernel Kernel;
        public ISystemRepository SystemRepository;
        public IUserRepository UserRepository;
        public IUserDesktopRepository UserDesktopRepository;

        public Form1(IKernel kernel)
        {
            #region Splash screen Start
            //this.Hide();
            //Thread splashthread = new Thread(new ThreadStart(SplashScreen.ShowSplashScreen));

            //splashthread.IsBackground = true;
            //splashthread.Start();

            #endregion

            Kernel = kernel;
            UserRepository = Kernel.Get<UserRepository>();
            SystemRepository = Kernel.Get<SystemRepository>();
            UserDesktopRepository = Kernel.Get<UserDesktopRepository>();

            InitializeComponent();
            AppSettings.Instance.SessionDetails =
                UserRepository.GetSessionDetails(AppSettings.Instance.LiveUserAccount.Username);

            AppSettings.Instance.GoDaddy = new GoDaddyAuctionSniper(AppSettings.Instance.SessionDetails.Username, Kernel.Get<IUserRepository>());

            LoadAuctions();
            Instance = this;
            //Login();
        }

        public void LoadAuctions()
        {
            UpdateProgress("Logging in..");
            LoadMyBids();
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

        private SortableBindingList<Auction> LoadMyLocalBids()
        {
            return UserDesktopRepository.LoadMyAuctions();
        }

        private void LoadMyBids()
        {
            var thz = new Thread(() =>
            {
                //DBHelper.Restore(Properties.Settings.Default.Username, Properties.Settings.Default.Password);
            });
            thz.SetApartmentState(ApartmentState.STA);
            thz.IsBackground = true;
            thz.Start();

            var th = new Thread(() =>
            {
                UpdateProgress("Login required...");
                if (AppSettings.Instance.SessionDetails.GoDaddyAccount == null)
                {
                    return;
                }
                UpdateProgress("Logging in...");
                if (AppSettings.Instance.GoDaddy.Login())
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        toolStripLabel6.BackgroundImage = Resources.GreenDot_smal;
                    }));
                    var auctions = new SortableBindingList<Auction>();
                    if (LoadMyLocalBids() != null)
                    {

                        foreach (var auction in LoadMyLocalBids())
                        {
                            if (auction.EndDate > GetPacificTime)
                            {
                                AppSettings.Instance.MyAuctions.Add(auction);
                            }

                        }
                    }
                    Invoke(new MethodInvoker(delegate
                    {
                        //AppSettings.Instance.MyAuctions.Clear();
                        //foreach (var item in auctions.ToList().Where(item => item.EndDate > GetPacificTime))
                        //{
                        //    AppSettings.Instance.MyAuctions.Add(item);
                        //}
                        if (AppSettings.Instance.MyAuctions.Count <= 0) return;
                        AppSettings.Instance.CurrentAuction = AppSettings.Instance.MyAuctions[0];
                        AppSettings.Instance.CurrentAuction.EndDate = AppSettings.Instance.GoDaddy.GetEndDate(AppSettings.Instance.CurrentAuction.AuctionRef);
                        SetTimer();
                    }));
                    SaveMyBids();
                    UpdateProgress("Done..");
                }
                else
                {
                    UpdateProgress("Login Failed. Please check your details.");
                }
            });
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region Splash Screen Load
            lblCountdown.Text = "";
            toolStrip1.Renderer = new ToolStripRenderPro();
            toolStrip2.Renderer = new ToolStripRenderPro();
            toolStrip3.Renderer = new ToolStripRenderPro();
            toolStrip4.Renderer = new ToolStripRenderPro();
            var sf = new SplashScreenForm(); // Splash Screen
            SplashScreen.UdpateStatusText("Loading Items...");
            SplashScreen.UdpateStatusTextWithStatus("Loading Defaults", TypeOfMessage.Success);
            SplashScreen.UdpateStatusText("Complete");

            txtVersion.Text = @"v " + "3.4.2.0";
            SetBindings();
            //lblTime.DataBindings.Add(new Binding("Text", AppSettings.Instance.CurrentAuction, "EndDate", false, DataSourceUpdateMode.OnPropertyChanged));

            Show();
            SplashScreen.CloseSplashScreen();
            Activate();

            #endregion
        }

        private void SetBindings()
        {
            bs.DataSource = AppSettings.Instance;
            lblTime.DataBindings.Add("Text", bs, "CurrentAuction.EndDate", false,
                  DataSourceUpdateMode.OnPropertyChanged);

            lblDomainName.DataBindings.Add("Text", bs, "CurrentAuction.DomainName", false,
                  DataSourceUpdateMode.OnPropertyChanged);

            //lblBids.DataBindings.Add("Text", bs, "CurrentAuction.Bids", false,
            //      DataSourceUpdateMode.OnPropertyChanged);

            //lblTraffic.DataBindings.Add("Text", bs, "CurrentAuction.Traffic", false,
            //      DataSourceUpdateMode.OnPropertyChanged);

            //lblValuation.DataBindings.Add("Text", bs, "CurrentAuction.Valuation", false,
            //      DataSourceUpdateMode.OnPropertyChanged);

            lblMinBid.DataBindings.Add("Text", bs, "CurrentAuction.MinBid", false,
                  DataSourceUpdateMode.OnPropertyChanged);

            tbBidValue.DataBindings.Add("Text", bs, "CurrentAuction.MyBid", false,
                  DataSourceUpdateMode.OnPropertyChanged);

            //lblMinOffer.DataBindings.Add("Text", bs, "CurrentAuction.MinOffer", false,
            //      DataSourceUpdateMode.OnPropertyChanged);

            //lblPrice.DataBindings.Add("Text", bs, "CurrentAuction.Price", false,
            //      DataSourceUpdateMode.OnPropertyChanged);

            //lblBIN.DataBindings.Add("Text", bs, "CurrentAuction.BuyItNow", false,
            //      DataSourceUpdateMode.OnPropertyChanged);

            lbAuctions.DataSource = AppSettings.Instance.AllAuctions;
            lbAuctions.DisplayMember = "Domain";

            dgvResults.DataSource = AppSettings.Instance.MyAuctions;
            dgvResults.VirtualMode = true;
            dgvResults.Columns["AuctionRef"].Visible = false;
            //dgvResults.Columns["Valuation"].Visible = false;
            //dgvResults.Columns["MinOffer"].Visible = false;
            //dgvResults.Columns["BuyItNow"].Visible = false;
            //dgvResults.Columns["Bids"].Visible = false;
            //dgvResults.Columns["Price"].Visible = false;
            //dgvResults.Columns["Traffic"].Visible = false;

        }

        delegate void StringParameterDelegate(string Text);

        /// <summary>
        /// Updates the progress label
        /// </summary>
        /// <param name="updateText"></param>
        public void UpdateProgress(string updateText)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new StringParameterDelegate(UpdateProgress), new object[] { updateText });
                return;
            }
            lblProgress.Text = updateText;

        }

        private void UpdateProgress2(string updateText)
        {   
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new StringParameterDelegate(UpdateProgress), new object[] { updateText });
                return;
            }
            lblChecked.Text = updateText;
           // lblProgress.Text = updateText;

        }

        private void lbAuctions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AppSettings.Instance.AllAuctions.Count > 0 && AppSettings.Instance.CurrentAuction.AuctionRef != null)
            {
                AppSettings.Instance.CurrentAuction = (Auction)lbAuctions.SelectedItem;
                AppSettings.Instance.CurrentAuction.EndDate = AppSettings.Instance.GoDaddy.GetEndDate(AppSettings.Instance.CurrentAuction.AuctionRef);
                SetTimer();
            }
        }

        private void LoadAuction()
        {
            if (dgvResults.Rows.Count > 0 && dgvResults.SelectedRows[0] != null)
            {
                tablessControl2.SelectTab(0);
                AppSettings.Instance.CurrentAuction = (Auction)dgvResults.SelectedRows[0].DataBoundItem;
                AppSettings.Instance.CurrentAuction.EndDate = AppSettings.Instance.GoDaddy.GetEndDate(AppSettings.Instance.CurrentAuction.AuctionRef);
                SetTimer();
            }

        }

        private void SetTimer()
        {
            timer1.Interval = 1000;
            timer1.Start();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            tablessControl2.SelectTab(0);
            AppSettings.Instance.AllAuctions.Clear();
            var th = new Thread(() =>
            {
                UpdateProgress("Searching..");
                var auctions = AppSettings.Instance.GoDaddy.Search(tbSearch.Text.Replace(" ", ","), true);
                Invoke(new MethodInvoker(delegate()
                {
                    if (auctions.ToList().Count > 0)
                    {
                        foreach (var item in auctions)
                        {
                            AppSettings.Instance.AllAuctions.Add(item);
                        }
                        AppSettings.Instance.CurrentAuction = (Auction)lbAuctions.SelectedItem;
                        AppSettings.Instance.CurrentAuction.EndDate = AppSettings.Instance.GoDaddy.GetEndDate(AppSettings.Instance.CurrentAuction.AuctionRef);
                        SetTimer();
                    }

                }));

                UpdateProgress("Done..");
            });
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();

        }

        private void CheckForBids()
        {
            var moo = Settings.Default.SingleMaxBid;
            if (AppSettings.Instance.MyAuctions == null || AppSettings.Instance.MyAuctions.Count <= 0) return;
            timerBids.Stop();
            var th = new Thread(() =>
            {
                for (int index = 0; index < AppSettings.Instance.MyAuctions.Count(); index++)
                {
                    var auction = AppSettings.Instance.MyAuctions.ToList()[index];
                    var ts =
                        auction.EndDate.Subtract(GetPacificTime);
                    if (ts.TotalSeconds < Convert.ToInt32(Settings.Default.BidTime) &&
                        (auction.MinBid < auction.MyBid || auction.MinBid == auction.MyBid) && ts.TotalSeconds > 0)
                    {
                        UpdateProgress("Processing Bid..");
                        var holdAuction = auction;
                        if (!Settings.Default.SingleMaxBid)
                        {
                            UpdateProgress("Staggering Bid..");
                            //holdAuction = AppSettings.Instance.GoDaddy.UpdateMinBid(auction);
                            if (holdAuction.MyBid >= holdAuction.MinBid)
                            {
                                //AppSettings.Instance.GoDaddy.PlaceBid(auction.AuctionRef, holdAuction.MinBid);
                            }
                            else
                            {
                                UpdateProgress("Bid now too low for min bid..");
                            }
                        }
                        else
                        {
                            //AppSettings.Instance.GoDaddy.PlaceBid2(auction.AuctionRef, auction.MyBid);
                        }

                        //GoDaddyAuctions.Instance.PlaceBid(auction.AuctionRef, auction.MyBid.ToString(CultureInfo.InvariantCulture));
                        Invoke(new MethodInvoker(delegate
                        {
                            AppSettings.Instance.MyAuctions.Remove(auction);
                            if (!Settings.Default.SingleMaxBid)
                            {
                                AppSettings.Instance.MyAuctions.Add(holdAuction);
                            }
                            SaveMyBids();
                        }));
                        UpdateProgress("Bid placed on " + auction.DomainName);
                    }
                }
                Invoke(new MethodInvoker(() => timerBids.Start()));
            });
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var ts = AppSettings.Instance.CurrentAuction.EndDate.Subtract(GetPacificTime);
            Console.WriteLine(ts.TotalSeconds);
            lblCountdown.Text = ts.ToString("d' Days 'h' Hours 'm' Minutes 's' Seconds'");
            //lblCountdown.Text = ts.TotalSeconds.ToString();
        }

        private void btnMyAuctions_Click(object sender, EventArgs e)
        {
            tablessControl2.SelectTab(1);
        }

        int _waiter = 0;
        bool continueWB = false;
        private void timerWB_Tick(object sender, EventArgs e)
        {
            if (_waiter == 3)
            {
                _waiter = 0;
                continueWB = true;
                timer1.Stop();
            }
            else
            {
                _waiter++;
            }
        }

        private void SaveMyBids()
        {
            var doc = new XmlDocument();
            var x = new XmlSerializer(AppSettings.Instance.MyAuctions.GetType());
            var sb = new System.Text.StringBuilder();
            var writer = new StringWriter(sb);
            x.Serialize(writer, AppSettings.Instance.MyAuctions);
            doc.LoadXml(sb.ToString());
            doc.Save(Path.Combine(Path.GetTempPath(), Settings.Default.BidsFile));

        }


        private void btnSettings_Click(object sender, EventArgs e)
        {
            rbSungleMaxbid.Checked = Settings.Default.SingleMaxBid;
            radioButton2.Checked = !Settings.Default.SingleMaxBid;
            tablessControl2.SelectTab(2);
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            tablessControl2.SelectTab(0);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            tbSearch.Focus();
        }

        private void btnBid_Click(object sender, EventArgs e)
        {
            //AppSettings.Instance.GoDaddy.PlaceBid(AppSettings.Instance.CurrentAuction.AuctionRef, "12");

            var bid = AppSettings.Instance.GoDaddy.TextModifier.TryParse_INT(tbBidValue.Text);
            if (AppSettings.Instance.GoDaddy.TextModifier.TryParse_INT(tbBidValue.Text) > 0)
            {
                var th = new Thread(() =>
                {
                    UpdateProgress("Processing Bid..");
                    AppSettings.Instance.CurrentAuction.MyBid = bid;
                    var got = false;
                    foreach (var auc in AppSettings.Instance.MyAuctions.Where(auc => AppSettings.Instance.CurrentAuction.AuctionRef == auc.AuctionRef))
                    {
                        got = true;
                        var auc1 = auc;
                        Invoke(new MethodInvoker(delegate
                        {
                            auc1.MyBid = AppSettings.Instance.CurrentAuction.MyBid;
                        }));
                    }
                    if (got == false)
                    {
                        Invoke(new MethodInvoker(
                            () => AppSettings.Instance.MyAuctions.Add(AppSettings.Instance.CurrentAuction)));
                    }
                    SaveMyBids();
                    UpdateProgress("Bid Added..");
                });
                th.SetApartmentState(ApartmentState.STA);
                th.IsBackground = true;
                th.Start();
            }
            else
            {
                UpdateProgress("Min bid required");
            }
             
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void timerBids_Tick(object sender, EventArgs e)
        {
            UpdateProgress2(string.Format("Checked: {0}", DateTime.Now));
            CheckForBids();
        }

        private void dgvResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            LoadAuction();
        }

        private void dgvResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;
            var dialogResult = MessageBox.Show(Resources.Form1_dgvResults_KeyDown_Delete_Auction_Entry_, Resources.Form1_dgvResults_KeyDown_Delete, MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                AppSettings.Instance.MyAuctions.Remove((Auction)dgvResults.SelectedRows[0].DataBoundItem);
                dgvResults.ClearSelection();
                SaveMyBids();
                //LoadAuctions();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void toolStripButton1_Click_2(object sender, EventArgs e)
        {
            if (AppSettings.Instance.MyAuctions != null && AppSettings.Instance.MyAuctions.Count > 0)
            {
                Thread th = new Thread(() =>
                {
                    foreach (var auction in AppSettings.Instance.MyAuctions.ToList())
                    {
                        TimeSpan ts = auction.EndDate.Subtract(GetPacificTime);
                        if (ts.TotalSeconds > Convert.ToInt32(Properties.Settings.Default.BidTime) && (auction.MinBid < auction.MyBid || auction.MinBid == auction.MyBid) && ts.TotalSeconds > 0)
                        {
                            UpdateProgress("Processing Bid..");
                            Thread.Sleep(2000);
                            AppSettings.Instance.GoDaddy.PlaceBid(auction);
                            this.Invoke(new MethodInvoker(delegate()
                            {
                                AppSettings.Instance.MyAuctions.Remove(auction);
                                SaveMyBids();
                            }));
                            UpdateProgress("Bids placed on " + auction.DomainName);
                        }
                    }
                    this.Invoke(new MethodInvoker(delegate()
                    {
                        
                    }));

                });
                th.SetApartmentState(ApartmentState.STA);
                th.IsBackground = true;
                th.Start();
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            SystemRepository.SaveGodaddyAccount(new GoDaddyAccount
            {
                AccountId = Guid.NewGuid(),
                Username = tbUsername.Text, Password = tbPassword.Text,
                UserID = AppSettings.Instance.LiveUserAccount.AccountID
            });
            UpdateProgress("Details Updated..");
            Application.Restart();
        }

        private void nudTimeDifference_ValueChanged(object sender, EventArgs e)
        {
            Settings.Default.Save();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.lunchboxcode.com/contact-us/");
        }
    }
}