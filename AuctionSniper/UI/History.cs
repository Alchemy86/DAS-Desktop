using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using AuctionSniper.DAL;

namespace AuctionSniper.UI
{
    public partial class History : Form
    {
        public List<DAS.Domain.Auctions.AuctionHistory> AuctionHistory;
        public History(List<DAS.Domain.Auctions.AuctionHistory> auctionHistory)
        {
            InitializeComponent();
            AuctionHistory = auctionHistory;
            GetData();
        }

        private void GetData()
        {
            var text = from a in AuctionHistory.OrderBy(x=>x.EventDate)
                                select new { a.EventDate, a.Text };
            var toSay = "";
            foreach (var data in text)
            {
                toSay += string.Format("{0} : {1}", data.EventDate, data.Text + Environment.NewLine);
            }
            richTextBox1.Text = toSay;
        }
    }
}
