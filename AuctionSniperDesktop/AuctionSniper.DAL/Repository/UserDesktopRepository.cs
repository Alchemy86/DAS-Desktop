using System;
using System.Linq;
using AuctionSniper.Domain.Godaddy;
using DAS.Domain;

namespace AuctionSniper.DAL.Repository
{
    public class UserDesktopRepository : BaseRepository, IUserDesktopRepository
    {
        public UserDesktopRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public SortableBindingList<DAS.Domain.GoDaddy.Auction> LoadMyAuctions()
        {
            var results = new SortableBindingList<DAS.Domain.GoDaddy.Auction>();
            var items = Context.Auctions.AsQueryable();
            foreach (var res  in items)
            {
                results.Add(res.ToDomainObject());
            }

            return results;
        }
    }
}
