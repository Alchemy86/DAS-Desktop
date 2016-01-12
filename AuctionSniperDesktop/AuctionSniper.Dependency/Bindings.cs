using AuctionSniper.DAL;
using AuctionSniper.DAL.Repository;
using AuctionSniper.Domain.Godaddy;
using DAS.Domain;
using DAS.Domain.Users;
using Ninject.Modules;

namespace AuctionSniper.Domain
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IEmail>().To<Email>();
            Bind<IUserRepository>().To<UserRepository>();
            Bind<ISystemRepository>().To<SystemRepository>();
            Bind<IUnitOfWork>().To<ASEntities>();
            Bind<IUserDesktopRepository>().To<UserDesktopRepository>();
        }
    }
}
