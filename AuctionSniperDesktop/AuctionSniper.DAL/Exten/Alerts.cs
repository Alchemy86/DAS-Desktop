using System;
using DAS.Domain.Enum;
using DAS.Domain.GoDaddy.Alerts;

namespace AuctionSniper.DAL
{
    public partial class Alerts
    {
        public Alert ToDomainObject()
        {
            return new Alert()
            {
                AlertId = AlertID,
                Custom = Custom,
                Processed = Processed,
                TriggerTime = TriggerTime,
                Type = (AlertType)Enum.Parse(typeof(AlertType), AlertType),
                AuctionId = AuctionID,
                Auction = Auctions.ToDomainObject()
            };
        }

        public void FromDomainObject(Alert alert)
        {
            AlertID = alert.AlertId;
            Custom = alert.Custom;
            Processed = alert.Processed;
            TriggerTime = alert.TriggerTime;
            AlertType = alert.Type.ToName();
            Description = alert.Description;
            AuctionID = alert.AuctionId;
        }
    }
}
