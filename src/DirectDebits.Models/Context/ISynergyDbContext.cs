using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using DirectDebits.Models.Entities;

namespace DirectDebits.Models.Context
{
    public interface ISynergyDbContext : IDbContext, IDisposable
    {
        IDbSet<Account> Accounts { get; set; }
        IDbSet<Allocation> Allocations { get; set; }
        IDbSet<Bank> Banks { get; set; }
        IDbSet<Batch> Batches { get; set; }
        IDbSet<Organisation> Organisations { get; set; }
        IDbSet<BatchSettings> BatchSettings { get; set; }
        IDbSet<ApplicationUser> Users { get; set; }

        DbEntityEntry Entry(object entity);
    }
}