using System.Collections.Generic;
using DirectDebits.Models.Entities;

namespace DirectDebits.Persistence.Contracts
{
    public interface IAccountRepository
    {
        IList<Account> GetManyByExactId(IEnumerable<string> exactIds);
        Account Create(Account account);
        void UpdateNameIfChanged(int organisationId, List<Account> accounts);
        IList<Account> CreateIfNotExists(Organisation organisation, List<Account> incomingAccounts);
        IList<Account> GetAccountsForBatch(int batchId);
    }
}