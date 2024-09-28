using System.Collections.Generic;
using System.Linq;
using DirectDebits.Models.Context;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using Serilog;

namespace DirectDebits.Persistence.Repositories
{
    public class BankRepository : IBankRepository
    {
        private readonly ILogger _log;
        private readonly ISynergyDbContext _context;

        public BankRepository(ILogger log, ISynergyDbContext context)
        {
            _log = log;
            _context = context;
        }

        public Bank Get(int id)
        {
            _log.Information("Attempting to retrieve a bank for the id:{@Id}...", id);
            Bank bank = _context.Banks.Single(x => x.Id == id);
            _log.Information("Bank ({@Name}) retrieved for the id:{@Id}", bank.Name, id);

            return bank;
        }

        public IList<Bank> GetAll()
        {
            _log.Information("Attempting to retrieve all banks...");
            IList<Bank> banks = _context.Banks.ToList();
            _log.Information("All banks (numberOfBanks:{@NumberOfBanks}) retrieved", banks.Count);

            return banks;
        }
    }
}
