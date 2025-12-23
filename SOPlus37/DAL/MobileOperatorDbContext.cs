using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using SOPlus37.DAL.Entities;
using System.Configuration;

namespace SOPlus37.DAL
{
    public class MobileOperatorDbContext : DbContext
    {
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<LegalEntity> LegalEntities { get; set; }
        public DbSet<BalanceReplenishment> BalanceReplenishments { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<CallType> CallTypes { get; set; }
        public DbSet<SOPlus37.DAL.Entities.Service> Services { get; set; }
        public DbSet<SOPlus37.DAL.Entities.SubscriberService> SubscriberServices { get; set; }
        public DbSet<Sms> Smses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var cs = ConfigurationManager.ConnectionStrings["SO5"]?.ConnectionString;
                optionsBuilder.UseSqlServer(cs);
            }
        }
    }
}
