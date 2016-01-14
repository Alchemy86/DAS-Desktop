
using System;

namespace AuctionSniper.DAL
{
    public partial class GoDaddyAccount
    {
        public DAS.Domain.GoDaddy.Users.GoDaddyAccount ToDomainObject()
        {
            return new DAS.Domain.GoDaddy.Users.GoDaddyAccount
            {
                AccountId = AccountID,
                Username = GoDaddyUsername,
                Password = GoDaddyPassword,
                Verified = Verified,
                AccountUsername = Users.Username,
                ReceiveEmail = Users.ReceiveEmails,
                UserID = Users.UserID
            };
        }

        public void FromDomainObject(DAS.Domain.GoDaddy.Users.GoDaddyAccount account)
        {
            AccountID = account.AccountId;
            GoDaddyUsername = account.Username;
            GoDaddyPassword = account.Password;
            Verified = account.Verified;
            UserID = account.UserID;
        }
    }
}
