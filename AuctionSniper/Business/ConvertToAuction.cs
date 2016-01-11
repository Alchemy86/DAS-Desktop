using System;
using System.Collections.Generic;
using ASEntityFramework;
using LunchboxSource.Business.SiteObject.GoDaddy;

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
                auction.Bids = auc.BidCount;
                auction.BuyItNow = auc.BuyItNow;
                auction.Domain = auc.DomainName;
                auction.EndDate = auc.EndDate;
                auction.EstimateEndDate = (DateTime)auc.EstimateEndDate;
                auction.MinBid = auc.MinBid;
                auction.MinOffer = auc.MinOffer;
                auction.MyBid = (int)auc.MyBid;
                auction.Price = auc.Price;
                auction.Status = auc.Status;
                auction.Traffic = auc.Traffic;
                auction.Valuation = auc.Valuation;
                auctions.Add(auction);
            }

            return auctions;
        }
    }
}
