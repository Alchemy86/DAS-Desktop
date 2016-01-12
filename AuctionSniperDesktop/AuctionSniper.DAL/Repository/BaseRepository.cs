using System;
using DAS.Domain;

namespace AuctionSniper.DAL.Repository
{
    public class BaseRepository
    {
        private readonly ASEntities _context;
        protected ASEntities Context
        {
            get
            {
                return _context;
            }
        }

        public BaseRepository(IUnitOfWork context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            _context = context as ASEntities;
        }
    }
}
