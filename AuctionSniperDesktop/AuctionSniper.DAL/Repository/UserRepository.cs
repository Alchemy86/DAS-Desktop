using System;
using System.Linq;
using DAS.Domain;
using DAS.Domain.DeathbyCapture;
using DAS.Domain.GoDaddy.Users;
using DAS.Domain.Users;

namespace AuctionSniper.DAL.Repository
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        private DeathByCaptureDetails GetDeathByCaptureDetailsDetails()
        {
            var username = Context.SystemConfig.First(x => x.PropertyID == "DBCUser").Value;
            var password = Context.SystemConfig.First(x => x.PropertyID == "DBCPass").Value;

            return new DeathByCaptureDetails
            {
                Password = password,
                Username = username
            };
        }

        public GoDaddySessionModel GetSessionDetails(string username)
        {
            var users = Context.Users;
            var gd = Context.GoDaddyAccount;
            var aucions = Context.Auctions;

            var details = Context.Users.Include("GoDaddyAccount").FirstOrDefault(x => x.Username == username);
            var gdnotused = Context.GoDaddyAccount.ToList();
            if (details == null) return null;
            var gdAccount = details.GoDaddyAccount.FirstOrDefault() != null
                ? details.GoDaddyAccount.First().ToDomainObject()
                : null;
            return new GoDaddySessionModel(details.Username, details.Password, gdAccount, GetDeathByCaptureDetailsDetails());
        }

        public void LogError(string message, string type = "Error")
        {
            
        }

        public void AddHistoryRecord(string message, Guid auctionLink)
        {
            Context.AuctionHistory.Add(new AuctionHistory
            {
                AuctionLink = auctionLink,
                CreatedDate = Global.GetPacificTime,
                HistoryID = Guid.NewGuid(),
                Text = message
            });
            Context.Save();
        }
    }
}
