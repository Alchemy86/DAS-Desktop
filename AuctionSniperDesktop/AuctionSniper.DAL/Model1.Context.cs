﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AuctionSniper.DAL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ASEntities : DbContext
    {
        public ASEntities()
            : base("name=ASEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Auctions> Auctions { get; set; }
        public virtual DbSet<GoDaddyAccount> GoDaddyAccount { get; set; }
        public virtual DbSet<SystemConfig> SystemConfig { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<AuctionHistory> AuctionHistory { get; set; }
        public virtual DbSet<Alerts> Alerts { get; set; }
    }
}