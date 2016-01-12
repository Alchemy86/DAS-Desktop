namespace AuctionSniper.DAL
{
    public partial class Auctions
    {
        public DAS.Domain.GoDaddy.Auction ToDomainObject()
        {
            return new DAS.Domain.GoDaddy.Auction
            { 
                
                AccountId = AccountID,
                AuctionId = AuctionID,
                AuctionRef = AuctionRef,
                DomainName = DomainName,
                EndDate = EndDate,
                MinBid = MinBid,
                MyBid = MyBid,
                Processed = Processed,
                GoDaddyAccount = GoDaddyAccount.ToDomainObject()
            };
        }
    }
}
