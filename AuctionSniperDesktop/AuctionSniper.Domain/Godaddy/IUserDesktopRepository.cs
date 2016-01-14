using System;
using System.Collections.Generic;
using DAL;
using DAS.Domain;
using DAS.Domain.GoDaddy;

namespace AuctionSniper.Domain.Godaddy
{
    public interface IUserDesktopRepository
    {
        SortableBindingList<Auction> LoadMyAuctions();
        void SaveAuction(Auction auction);
        List<DAS.Domain.Auctions.AuctionHistory> LoadAuctionHistory(Guid auctionID);
    }
}