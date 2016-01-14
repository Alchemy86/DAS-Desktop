using DAS.Domain.GoDaddy;

namespace AuctionSniper.DAL
{
    public partial class AuctionHistory
    {
        public DAS.Domain.Auctions.AuctionHistory ToDomainObject()
        {
            return new DAS.Domain.Auctions.AuctionHistory
            {
                EventDate = CreatedDate,
                Text =  Text
            };
        }
    }
}
