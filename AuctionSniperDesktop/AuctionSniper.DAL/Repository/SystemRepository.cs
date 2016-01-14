using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using DAS.Domain;
using DAS.Domain.GoDaddy;

namespace AuctionSniper.DAL.Repository
{
    public class SystemRepository : BaseRepository, ISystemRepository
    {
        public SystemRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public string ServiceEmail
        {
            get { return ""; }
        }

        public string ServiceEmailPassword
        {
            get { return ""; }
        }

        public string ServiceEmailPort
        {
            get { return ""; }
        }

        public string ServiceEmailSmtp
        {
            get { return ""; }
        }

        public string ServiceEmailUser
        {
            get { return ""; }
        }

        public string AlertEmail
        {
            get { return Context.SystemConfig.First(x => x.PropertyID == "AlertEmail").Value; }
        }

        public string BidTime
        {
            get { return Context.SystemConfig.First(x => x.PropertyID == "BidTime").Value; }
        }

        public IEnumerable<Auction> GetEndingAuctions()
        {
            var tomorrow = Global.GetPacificTime.AddDays(3);
            var now = Global.GetPacificTime;
            //var auctions = Context.Auctions.Include("GoDaddyAccount").Where(x => x.Processed == false && x.EndDate <= tomorrow);
            //var moo = Context.Auctions.Where(x => x.Processed == false && x.EndDate <= tomorrow).ToList();
            var results = (from e in Context.Auctions.Include("GoDaddyAccount")
                           select e).Where(x => x.Processed == false && x.EndDate <= tomorrow)
                .AsEnumerable()
                .Select(x => x.ToDomainObject());

            return results;
        }

        public void MarkAlertAsProcesed(Guid alertId)
        {
            var alert = Context.Alerts.First(x => x.AlertID == alertId);
            alert.Processed = true;
            Context.Alerts.AddOrUpdate(alert);
            Context.Save();
        }

        public IEnumerable<DAS.Domain.GoDaddy.Alerts.Alert> GetAlerts()
        {
            return (from e in Context.Alerts.Include("Auction")
                    select e).Where(x => x.Processed == false)
                       .AsEnumerable()
                       .Select(x => x.ToDomainObject());
        }

        public void MarkAuctionAsProcess(Guid auctionId)
        {
            var auction = Context.Auctions.First(x => x.AuctionID == auctionId);
            auction.Processed = true;
            Context.Auctions.AddOrUpdate(auction);
            Context.Save();
        }


        public void SaveGodaddyAccount(DAS.Domain.GoDaddy.Users.GoDaddyAccount account)
        {
            var acc = new GoDaddyAccount();
            acc.FromDomainObject(account);
            var existingAccount = Context.GoDaddyAccount.FirstOrDefault(x => x.GoDaddyUsername == account.Username);
            if (existingAccount != null)
            {
                acc.AccountID = Context.Users.First(x=>x.Username == account.AccountUsername).UserID;
                Context.GoDaddyAccount.AddOrUpdate(acc);
            }
            else
            {
                existingAccount = new GoDaddyAccount
                {
                    GoDaddyPassword = account.Password,
                    Verified = account.Verified,
                    GoDaddyUsername = account.Username,
                    AccountID = account.AccountId,
                    UserID = account.UserID
                };
                Context.GoDaddyAccount.AddOrUpdate(existingAccount);
            }
            Context.Save();
        }


        public DAS.Domain.Users.User SaveAccount(DAS.Domain.Users.User account)
        {
            var existingAccount = Context.Users.FirstOrDefault(x => x.Username == account.Username);
            if (existingAccount == null)
            {
                var newUser = new Users()
                {
                    AccessLevel = 0,
                    Password = account.Password,
                    ReceiveEmails = false,
                    UseAccountForSearch = true,
                    UserID = account.AccountID,
                    Username = account.Username
                };
                Context.Users.Add(newUser);
                Context.Save();
            }

            return account;
        }
    }
}
