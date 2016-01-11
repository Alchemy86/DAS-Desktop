using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AuctionSniper.UI
{
    public partial class Error : Form
    {

        public Error(string error)
        {
            InitializeComponent();
            this.textBox1.Text = error;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
