using System;
using System.Collections.Generic;
using DirectDebits.Common.Utility;
using ExactOnline.Client.Models.Cashflow;
using ExactOnline.Client.Models.CRM;
using ExactOnline.Client.Models.Financial;
using ExactOnline.Client.Models.FinancialTransaction;

namespace DirectDebits.ExactClient.Contracts
{
    public interface IExactFinancialService
    {
        IList<Payment> GetPayments(IList<Account> accounts);
        IList<Receivable> GetFinancialReceivablesHavingInvoiceNumber(IList<Account> accounts);
        IList<Receivable> GetOpenCashflowReceivables(IList<Account> accounts);
        IList<Receivable> GetAllCashflowReceivables(IList<Account> accounts);
        IList<Receivable> GetNonZeroCashflowReceivables(IList<Account> accounts);
        IList<Transaction> GetTransactions(IEnumerable<int> entryNumbers);
        Result<FinancialPeriod> GetFinancialPeriodForDate(DateTime date);
    }
}