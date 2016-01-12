using System;
using System.Collections.Generic;
using DAS.Domain.GoDaddy;

namespace AuctionSniper.Business
{
    public class ConvertToAuction
    {
        public static List<Auction> Convert(List<Auction> auctionsToConvert)
        {
            List<Auction> auctions = new List<Auction>();

            foreach (var auc in auctionsToConvert)
            {
                var auction = new Auction();
                auction.AuctionRef = auc.AuctionRef;
                auction.MinBid = auc.MinBid;
                auction.DomainName = auc.DomainName;
                auction.EndDate = auc.EndDate;
                auction.EndDate = (DateTime)auc.EndDate;
                auction.MinBid = auc.MinBid;
                auction.MyBid = (int)auc.MyBid;
                auctions.Add(auction);
            }

            return auctions;
        }
    }
}
