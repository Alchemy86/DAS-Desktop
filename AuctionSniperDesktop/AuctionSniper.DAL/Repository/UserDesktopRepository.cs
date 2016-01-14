using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using AuctionSniper.Domain.Godaddy;
using DAS.Domain;
using DAS.Domain.GoDaddy;

namespace AuctionSniper.DAL.Repository
{
    public class UserDesktopRepository : BaseRepository, IUserDesktopRepository
    {
        public UserDesktopRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public List<DAS.Domain.Auctions.AuctionHistory> LoadAuctionHistory(Guid auctionID)
        {
            var results = new List<DAS.Domain.Auctions.AuctionHistory>();
            var data = Context.AuctionHistory.Where(x => x.AuctionLink == auctionID);
            foreach (var res in data)
            {
                results.Add(res.ToDomainObject());
            }
            return results.ToList();
        }

        public SortableBindingList<Auction> LoadMyAuctions()
        {
            var results = new SortableBindingList<Auction>();
            var items = Context.Auctions.AsQueryable();
            foreach (var res  in items)
            {
                results.Add(res.ToDomainObject());
            }

            return results;
        }


        public void SaveAuction(Auction auction)
        {
            var existingRecord = Context.Auctions.FirstOrDefault(x => x.AuctionRef == auction.AuctionRef);
            if (existingRecord != null)
            {
                existingRecord.EndDate = auction.EndDate;
                existingRecord.MinBid = auction.MinBid;
                Context.Auctions.AddOrUpdate(existingRecord);
            }
            else
            {
                var auc = new Auctions();
                auc.FromDomainObject(auction);
                Context.Auctions.AddOrUpdate(auc);
            }

            Context.Save();

        }

    }
}
