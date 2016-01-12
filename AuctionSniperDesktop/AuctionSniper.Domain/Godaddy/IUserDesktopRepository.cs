using DAS.Domain;
using DAS.Domain.GoDaddy;

namespace AuctionSniper.Domain.Godaddy
{
    public interface IUserDesktopRepository
    {
        SortableBindingList<Auction> LoadMyAuctions();
    }
}