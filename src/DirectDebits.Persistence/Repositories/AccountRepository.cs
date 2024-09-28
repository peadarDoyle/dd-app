using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectDebits.Models;
using DirectDebits.Models.Context;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;

namespace DirectDebits.Persistence.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ISynergyDbContext _context;

        public AccountRepository(ISynergyDbContext context)
        {
            _context = context;
        }

        public IList<Account> GetManyByExactId(IEnumerable<string> exactIds)
        {
            return _context.Accounts
                           .Where(x => exactIds.Contains(x.ExternalId))
                           .ToList();
        }

        public Account Create(Account account)
        {
            Account persistedAccount = _context.Accounts.Add(account);
            _context.SaveChanges();

            return persistedAccount;
        }

        public void UpdateNameIfChanged(int organisationId, List<Account> accounts)
        {
            IEnumerable<string> externalIds = accounts.Select(x => x.ExternalId);

            IEnumerable<Account> attachedAccounts = _context.Accounts
                .Where(x => x.Organisation.Id == organisationId)
                .Where(x => externalIds.Contains(x.ExternalId))
                .AsEnumerable();

            foreach (Account attachedAccount in attachedAccounts)
            {
                string recentAccountName = accounts.Single(x => x.ExternalId == attachedAccount.ExternalId).Name;

                if (attachedAccount.Name != recentAccountName)
                {
                    attachedAccount.Name = recentAccountName;
                    _context.Entry(attachedAccount).State = EntityState.Modified;
                }
            }

            _context.SaveChanges();
        }

        public IList<Account> CreateIfNotExists(Organisation organisation, List<Account> incomingAccounts)
        {
            IEnumerable<string> externalIds = incomingAccounts.Select(x => x.ExternalId);
            IList<Account> existingAccounts = organisation.Accounts.Where(x => externalIds.Contains(x.ExternalId)).ToList();
            IEnumerable<string> existingExternalIds = existingAccounts.Select(x => x.ExternalId);
            IEnumerable<Account> newAccounts = incomingAccounts.Where(x => !existingExternalIds.Contains(x.ExternalId));

            foreach (Account account in newAccounts)
            {
                account.Organisation = organisation;
                Account persistedAccount = _context.Accounts.Add(account);
                existingAccounts.Add(persistedAccount);
            }

            _context.SaveChanges();

            return existingAccounts;
        }

        public IList<Account> GetAccountsForBatch(int batchId)
        {
            return _context.Accounts.Where(x => x.Allocations.Any(y => y.Batch.Id == batchId)).ToList();
        }
    }
}
