using DAS.Domain.GoDaddy;

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
                Bids = BidCount
            };
        }

        public void FromDomainObject(Auction auction)
        {
            AccountID = auction.AccountId;
            AuctionID = auction.AuctionId;
            AuctionRef = auction.AuctionRef;
            DomainName = auction.DomainName;
            EndDate = auction.EndDate;
            MinBid = auction.MinBid;
            MyBid = auction.MyBid;
            Processed = auction.Processed;
            BidCount = auction.Bids;
        }

    }
}
