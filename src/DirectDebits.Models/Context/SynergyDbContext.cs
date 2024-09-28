using System.Data.Entity;
using DirectDebits.Models.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using DirectDebits.Models.Migrations;

namespace DirectDebits.Models.Context
{
    public class SynergyDbContext : IdentityDbContext<ApplicationUser>, ISynergyDbContext
    {
        public SynergyDbContext() : base("name=SynergyDbContext")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SynergyDbContext, Configuration>());
        }

        public virtual IDbSet<Bank> Banks { get; set; }
        public virtual IDbSet<Organisation> Organisations { get; set; }
        public virtual IDbSet<BatchSettings> BatchSettings { get; set; }
        public virtual IDbSet<Account> Accounts { get; set; }
        public virtual IDbSet<Batch> Batches { get; set; }
        public virtual IDbSet<Allocation> Allocations { get; set; }

        public static SynergyDbContext Create()
        {
            return new SynergyDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.Batches)
                .WithRequired(e => e.CreatedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Organisation>()
                .HasKey(e => e.Id)
                .HasMany(e => e.Users)
                .WithRequired(e => e.Organisation)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Organisation>()
                .HasMany(e => e.Accounts)
                .WithRequired(e => e.Organisation)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Organisation>()
                .HasMany(e => e.Batches)
                .WithRequired(e => e.Organisation)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Account>()
                .HasKey(e => e.Id)
                .HasMany(e => e.Allocations)
                .WithRequired(e => e.Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Batch>()
                .HasKey(e => e.Id)
                .HasMany(e => e.Allocations)
                .WithRequired(e => e.Batch)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Allocation>()
                .HasKey(e => e.Id)
                .Property(e => e.Amount)
                .HasPrecision(18, 4);
        }
    }
}
