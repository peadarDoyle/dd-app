using System;
using System.Collections.Generic;
using System.Linq;
using ExactOnline.Client.Models.Cashflow;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Batches
{
    public class AccountInvoicesViewModel
    {
        public AccountInvoicesViewModel(IList<Receivable> receivables, BatchSettings settings)
        {
            var cutoffPeriods = GetCutOffPeriods(settings);

            Id = receivables.First().Account.Value.ToString();
            DisplayId = receivables.First().AccountCode.Trim();
            Name = receivables.First().AccountName.Trim();

            P1 = GetInvoiceViewModels(receivables, x => x.InvoiceDate > cutoffPeriods[0]);
            P2 = GetInvoiceViewModels(receivables, x => x.InvoiceDate <= cutoffPeriods[0] && x.InvoiceDate > cutoffPeriods[1]);
            P3 = GetInvoiceViewModels(receivables, x => x.InvoiceDate <= cutoffPeriods[1] && x.InvoiceDate > cutoffPeriods[2]);
            Older = GetInvoiceViewModels(receivables, x => x.InvoiceDate <= cutoffPeriods[2]);
        }

        public AccountInvoicesViewModel(IList<Payment> payments, BatchSettings settings)
        {
            var cutoffPeriods = GetCutOffPeriods(settings);

            Id = payments.First().Account.Value.ToString();
            DisplayId = payments.First().AccountCode.Trim();
            Name = payments.First().AccountName.Trim();

            P1 = GetInvoiceViewModels(payments, x => x.InvoiceDate > cutoffPeriods[0]);
            P2 = GetInvoiceViewModels(payments, x => x.InvoiceDate <= cutoffPeriods[0] && x.InvoiceDate > cutoffPeriods[1]);
            P3 = GetInvoiceViewModels(payments, x => x.InvoiceDate <= cutoffPeriods[1] && x.InvoiceDate > cutoffPeriods[2]);
            Older = GetInvoiceViewModels(payments, x => x.InvoiceDate <= cutoffPeriods[2]);
        }

        private static DateTime[] GetCutOffPeriods(BatchSettings settings)
        {
            return new []
            {
                DateTime.Now - TimeSpan.FromDays(settings.Period1),
                DateTime.Now - TimeSpan.FromDays(settings.Period2),
                DateTime.Now - TimeSpan.FromDays(settings.Period3)
            };
        }

        private InvoiceViewModel[] GetInvoiceViewModels(IList<Receivable> payments, Func<Receivable, bool> predicate)
        {
            return payments.Where(predicate)
                .Select(x => new InvoiceViewModel(x))
                .OrderBy(x => x.InvoiceDate)
                .ToArray();
        }

        private InvoiceViewModel[] GetInvoiceViewModels(IList<Payment> payments, Func<Payment, bool> predicate)
        {
            return payments.Where(predicate)
                .Select(x => new InvoiceViewModel(x))
                .OrderBy(x => x.InvoiceDate)
                .ToArray();
        }

        public string Id { get; set; }
        public string DisplayId { get; set; }
        public string Name { get; set; }
        public  InvoiceViewModel[] P1 { get; set; }
        public  InvoiceViewModel[] P2 { get; set; }
        public  InvoiceViewModel[] P3 { get; set; }
        public  InvoiceViewModel[] Older { get; set; }
    }
}
