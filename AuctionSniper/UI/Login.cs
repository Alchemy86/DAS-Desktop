using System;
using System.Windows.Forms;
using System.Diagnostics;
using AuctionSniper.Business;
using AuctionSniper.Business.DataAccess;
using AuctionSniper.DAL.Repository;
using DAS.Domain;
using DAS.Domain.Users;
using Ninject;

namespace AuctionSniper.UI
{
    public partial class Login : Form
    {
        private bool storeDetails = false;
        private IKernel Kernal;
        private ISystemRepository SystemRepository;

        public Login(IKernel kernal)
        {
            this.SuspendLayout();
            // 
            // Login
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.MinimizeBox = false;
            this.Name = "Login";
            this.ResumeLayout(false);
            Kernal = kernal;
            SystemRepository = Kernal.Get<SystemRepository>();

            InitializeComponent();
            tbUsername.Text = Properties.Settings.Default.sStoredUsername;
            tbPassword.Text = Properties.Settings.Default.sStoredPassword;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            PerformLogin();
        }

        private void PerformLogin()
        {
            if (tbUsername.Text == "")
            {
                tbUsername.Focus();
            }
            else if (tbPassword.Text.Length < 1)
            {
                tbPassword.Focus();
            }
            else
            {
                //AuctionSniper.Business.DataAccess.DBHelper.Login(tbUsername.Text, tbPassword.Text)
                if (DBHelper.Login(tbUsername.Text, tbPassword.Text, "Auction Sniper"))
                {
                    this.DialogResult = DialogResult.OK;
                    AppSettings.Instance.LiveUserAccount = SystemRepository.SaveAccount(new User
                    {
                        AccountID = Guid.NewGuid(),
                        Password = tbPassword.Text,
                        Username = tbUsername.Text
                    });
                    //MessageBox.Show(AppSettings.Instance.LiveUserAccount.AccountID.ToString());
                    if (storeDetails)
                    {
                        Properties.Settings.Default.sStoredUsername = tbUsername.Text;
                        Properties.Settings.Default.sStoredPassword = tbPassword.Text;
                        Properties.Settings.Default.Save();
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("No details found in remote system. \nPlease try again", "Login Failure");
                    tbUsername.Focus();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbSave_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSave.Checked)
            {
                storeDetails = true;
            }
            else
            {
                storeDetails = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {   
            Process.Start(Business.Sites.PayPal.GeneratePaymentRequest("Auction Sniper", "002", 45, 0, 0, "GBP", "Aarongioucash@hotmail.com"));
        }

        private void Login_Load(object sender, EventArgs e)
        {
            PerformLogin();
        }

    }
}
