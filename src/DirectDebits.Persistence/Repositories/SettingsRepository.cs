using System;
using System.Data.Entity;
using System.Linq;
using DirectDebits.Models.Context;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;

namespace DirectDebits.Persistence.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly ISynergyDbContext _context;

        public SettingsRepository(ISynergyDbContext context)
        {
            _context = context;
        }

        public BatchSettings Get(int id)
        {
            return _context.BatchSettings
                           .Include("Bank")
                           .Single(x => x.Id == id);
        }

        public void Update(BatchSettings settings)
        {
            if (settings.Bank == null)
            {
                throw new ArgumentException("The Bank property cannot be null");
            }

            BatchSettings persistedSettings = _context.BatchSettings.Single(x => x.Id == settings.Id);
            Bank persistedBank = _context.Banks.Single(x => x.Id == settings.Bank.Id);
            persistedSettings.Bank = persistedBank;
            _context.SaveChanges();
        }
    }
}
