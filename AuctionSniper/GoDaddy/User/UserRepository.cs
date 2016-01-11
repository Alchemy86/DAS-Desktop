using System;
using DAS.Domain.Users;

namespace AuctionSniper.GoDaddy.User
{
    public class UserRepository : IUserRepository
    {
        public DAS.Domain.GoDaddy.Users.GoDaddySessionModel GetSessionDetails(string username)
        {
            throw new NotImplementedException();
        }

        public void LogError(string message, string type = "Error")
        {
            throw new NotImplementedException();
        }

        public void AddHistoryRecord(string message, Guid auctionLink)
        {
            throw new NotImplementedException();
        }
    }
}
