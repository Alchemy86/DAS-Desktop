using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using AuctionSniper.Business.DataAccess;
using AuctionSniper.Domain;
using AuctionSniper.UI;
using Ninject;

namespace AuctionSniper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [HandleProcessCorruptedStateExceptions]
        [DebuggerNonUserCode]
        static void Main()
        {         

            try
            {
                // Add the event handler for handling UI thread exceptions to the event.
                Application.ThreadException += Application_ThreadException;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

                // Add the event handler for handling non-UI thread exceptions to the event. 
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                IKernel kernel = new StandardKernel(new Bindings());
                UI.Login tmpStart = kernel.Get<Login>();
                //kernel.Load(Assembly.GetExecutingAssembly());
                if (tmpStart.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(kernel.Get<Form1>());
                }
            }
            catch (Exception e)
            {
                DBHelper.LogBug("Auction Sniper", e.Message + "\r\n" + e.StackTrace
                + "\r\n\r\n" + e.InnerException, "Unknown");
            }
            
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            DBHelper.LogBug("Auction Sniper", e.Exception.Message + "\r\n" + e.Exception + "\r\n" + e.Exception.StackTrace
                + "\r\n\r\n" + e.Exception.InnerException, "Unknown");
            new Error(e.Exception.Message + "\r\n" + e.Exception + "\r\n" + e.Exception.StackTrace
                + "\r\n\r\n" + e.Exception.InnerException).ShowDialog();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            DBHelper.LogBug("Auction Sniper", ex.Message + "\r\n\r\n" + ex.Source + "\r\n\r\n" + ex.StackTrace
                + "\r\n\r\n" + ex.InnerException, "Unknown");
            new Error(ex.Message + "\r\n\r\n" + ex.Source + "\r\n\r\n" + ex.StackTrace
                + "\r\n\r\n" + ex.InnerException).ShowDialog();
        }
    }
}
