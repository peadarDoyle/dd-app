using System.Linq;
using System.Collections.Generic;
using DirectDebits.Models;
using System;
using DirectDebits.Models.Entities;

namespace DirectDebits.ExactClient.Models
{
    /// <summary>
    /// This is the data structure from which Exact Bank Entries and Match Sets will be created.
    /// </summary>
    public class ExactTransaction
    {
        public ExactUploadData Data { get; set; }
        public IList<ExactUploadAccount> Accounts { get; set; }

        public ExactTransaction(Batch batch, int finYear, int finPeriod)
        {
            BatchSettings settings = batch.Organisation.GetSettings(batch.BatchType);

            Data = new ExactUploadData
            {
                TradeJournalCode = settings.TradeJournalCode,
                BankJournalCode = settings.BankJournalCode,
                BankGlCode = settings.BankGlCode,
                TradeGlCode = settings.TradeGlCode,
                ProcessDate = batch.ProcessDate,
                FinacialYear = finYear,
                FinacialPeriod = finPeriod,
                BatchNumber = batch.Number,
                Total = batch.Allocations.Sum(x => x.Amount),
                PaymentCondition = new PaymentCondition(batch.BatchType)
            };

            IList<IGrouping<Account, Allocation>> groupedAccountAllocations = batch.Allocations.GroupBy(x => x.Account).ToList();
            Accounts = groupedAccountAllocations.Select(ToExactUploadAccount).ToList();
        }

        private static ExactUploadAccount ToExactUploadAccount(IGrouping<Account, Allocation> accountAllocations)
        {
            string externalId = accountAllocations.Key.ExternalId;
            string externalCode = accountAllocations.Key.ExternalDisplayId;
            string name = accountAllocations.Key.Name;

            IList<Allocation> fullAllocations = accountAllocations.Where(x => x.Amount == x.InvoiceTotal).ToList();
            // we use the absolue value so the same clause will work for both credit notes and invoices
            IList<Allocation> partialAllocations = accountAllocations.Where(x => Math.Abs(x.Amount) < Math.Abs(x.InvoiceTotal)).ToList();

            return new ExactUploadAccount
            {
                ExternalId = externalId,
                ExternalDisplayId = externalCode,
                Name = name,
                FullAllocations = fullAllocations,
                PartialAllocations = partialAllocations
            };
        }
    }
}