using System.Linq;
using System.Collections.Generic;
using ExactOnline.Client.Models.Cashflow;
using ExactOnline.Client.Models.CRM;
using ExactOnline.Client.Models.FinancialTransaction;
using ExactOnline.Client.Models.Financial;
using DirectDebits.ExactClient.Contracts;
using DirectDebits.Common.Utility;
using System;
using Serilog;

namespace DirectDebits.ExactClient.Services
{
    public class ExactFinancialService : ExactServiceBase, IExactFinancialService
    {
        private const int LIMIT = 5000; // arbitrary document limiter

        public ExactFinancialService(ILogger logger, int? division) : base (logger, division) { }

        public IList<Receivable> GetOpenCashflowReceivables(IList<Account> accounts)
        {
            Logger.Information("Getting all open cashflow receivables in bulk...");

            int numAccountsToQuery = 60;

            var receivables = new List<Receivable>();
            
            for (var i = 0; ; i++)
            {
                var accountsToQuery = accounts.Skip(numAccountsToQuery * i)
                                              .Take(numAccountsToQuery);

                var queries = accountsToQuery.Select(x => $"Account eq guid'{x.ID}'");
                
                if (!queries.Any()) break;

                string filterParam = $"({string.Join(" or ", queries)}) and (Status eq 20)";
                string selectParam = "AccountCode,Account,AccountName,InvoiceNumber,EntryNumber,InvoiceDate,AmountDC,YourRef,Description,Status";

                string path = "bulk/Cashflow/Receivables";
                Result<List<Receivable>> result = RestApiGetMany<Receivable>(path, filterParam, selectParam);
                receivables.AddRange(result.Value);

                Logger.Information("Chunk of {@Total} receivables received", result.Value);
            }

            Logger.Information("Final total of {@Total} receivables received", receivables.Count);

            return receivables;
        }

        public IList<Receivable> GetAllCashflowReceivables(IList<Account> accounts)
        {
            Logger.Information("Getting all cashflow receivables in bulk...");

            int numAccountsToQuery = 60;

            var receivables = new List<Receivable>();
            
            for (var i = 0; ; i++)
            {
                var accountsToQuery = accounts.Skip(numAccountsToQuery * i)
                                              .Take(numAccountsToQuery);

                var queries = accountsToQuery.Select(x => $"Account eq guid'{x.ID}'");
                
                if (!queries.Any()) break;

                string filterParam = $"({string.Join(" or ", queries)})";
                string selectParam = "AccountCode,Account,AccountName,InvoiceNumber,EntryNumber,InvoiceDate,AmountDC,YourRef,Description,Status";

                string path = "bulk/Cashflow/Receivables";
                Result<List<Receivable>> result = RestApiGetMany<Receivable>(path, filterParam, selectParam);
                receivables.AddRange(result.Value);

                Logger.Information("Chunk of {@Total} receivables received", result.Value);
            }

            Logger.Information("Final total of {@Total} receivables received", receivables.Count);

            var thirty = receivables.Where(x => x.Status == 30 && x.AmountDC != 0).Count();
            if (thirty > 0)
            {
                Logger.Information("30 status with outstanding, total of {@ThirtyStatus}", thirty);
            }

            var forty = receivables.Where(x => x.Status == 40 && x.AmountDC != 0).Count();
            if (forty > 0)
            {
                Logger.Information("40 status with outstanding, total of {@ThirtyStatus}", forty);
            }

            var fifty = receivables.Where(x => x.Status == 50 && x.AmountDC != 0).Count();

            if (fifty > 0)
            {
                Logger.Information("50 status with outstanding, total of {@ThirtyStatus}", fifty);
            }

            return receivables;
        }

        public IList<Receivable> GetFinancialReceivablesHavingInvoiceNumber(IList<Account> accounts)
        {
            Logger.Information("Getting financial receivables that have an invoice number...");

            int numAccountsToQuery = 60;

            var receivables = new List<ReceivablesList>();
            
            for (var i = 0; ; i++)
            {
                var accountsToQuery = accounts.Skip(numAccountsToQuery * i)
                                              .Take(numAccountsToQuery);

                var queries = accountsToQuery.Select(x => $"AccountId eq guid'{x.ID}'");
                
                if (!queries.Any()) break;

                string accountQuery = string.Join(" or ", queries);
                string query  = $"({accountQuery}) and (InvoiceNumber gt 0)";
                string fields = "AccountCode,AccountId,AccountName,InvoiceNumber,EntryNumber,InvoiceDate,Amount,YourRef,Description";

                var chunk = GetPaginatedRecords<ReceivablesList>(fields, query);
                receivables.AddRange(chunk);

                Logger.Information("Chunk of {@Total} receivables received", chunk.Count);
            }

            Logger.Information("Final total of {@Total} receivables received", receivables.Count);

            return receivables.Select(x => new Receivable
            {
                Account = x.AccountId,
                AccountName = x.AccountName,
                AccountCode = x.AccountCode,
                InvoiceNumber = x.InvoiceNumber,
                EntryNumber = x.EntryNumber,
                InvoiceDate = x.InvoiceDate,
                AmountDC = x.Amount * -1,
                YourRef = x.YourRef,
                Description = x.Description
            }).ToList();
        }

        public IList<Receivable> GetNonZeroCashflowReceivables(IList<Account> accounts)
        {
            Logger.Information("Getting the non-zero cashflow receivables...");

            var fields = "AccountCode,Account,AccountName,InvoiceNumber,EntryNumber,InvoiceDate,AmountDC,YourRef,Description,Status";

            int accountsToQuery = 60;
            string amountQuery = "(AmountDC lt 0 or AmountDC gt 0)"; // get invoices and credit notes

            var receivables = new List<Receivable>();

            for (var i = 0; ; i++)
            {
                var queries = accounts.Skip(accountsToQuery * i)
                                      .Take(accountsToQuery)
                                      .Select(x => $"Account eq guid'{x.ID}'");

                if (!queries.Any()) break;

                string accountQuery = string.Join(" or ", queries);
                string query = $"({accountQuery}) and {amountQuery}";

                var chunk = GetPaginatedRecords<Receivable>(fields, query);
                receivables.AddRange(chunk);

                Logger.Information("Chunk of {@Total} receivables received", chunk.Count);
            }

            Logger.Information("Final total of {@Total} receivables received", receivables.Count);

            return receivables;
        }

        public IList<Payment> GetPayments(IList<Account> accounts)
        {
            string fields = "AccountCode,Account,AccountName,InvoiceNumber,EntryNumber,InvoiceDate,AmountDC,YourRef,Description";

            const int accountsToQuery = 10;
            const string amountQuery = "(AmountDC+lt+0+or+AmountDC+gt+0)"; // get invoices and credit notes

            var payments = new List<Payment>();

            for (var i = 0; ; i++)
            {
                var queries = accounts.Skip(accountsToQuery * i)
                                      .Take(accountsToQuery)
                                      .Select(x => $"Account+eq+guid'{x.ID}'");

                if (!queries.Any()) break;

                string accountQuery = string.Join("+or+", queries);
                string query = $"({accountQuery})+and+{amountQuery}";

                var results = Client.For<Payment>().Select(fields).Where(query).Get();

                payments.AddRange(results);

                if (queries.Count() < accountsToQuery) break;
            }

            return payments;
        }

        public IList<Transaction> GetTransactions(IEnumerable<int> entryNumbers)
        {
            var fields = "EntryNumber,FinancialPeriod,FinancialYear";

            const int entryNumberToQuery = 60;

            var transactions = new List<Transaction>();

            for (var i = 0; ; i++)
            {
                var queries = entryNumbers.Skip(entryNumberToQuery * i)
                                          .Take(entryNumberToQuery)
                                          .Select(x => "EntryNumber+eq+" + x);

                if (!queries.Any()) break;

                string query = string.Join("+or+", queries);

                transactions.AddRange(GetPaginatedRecords<Transaction>(fields, query));

                if (queries.Count() < entryNumberToQuery) break;
            }

            return transactions;
        }

        public Result<FinancialPeriod> GetFinancialPeriodForDate(DateTime date)
        {
            string fields = "FinYear,FinPeriod";
            string query = $"StartDate+le+datetime'{date:yyyy-MM-dd}'+and+EndDate+ge+datetime'{date:yyyy-MM-dd}'";

            var period = Client.For<FinancialPeriod>()
                               .Select(fields)
                               .Where(query)
                               .Get()
                               .SingleOrDefault();

            return ValidateFinancialPeriod(period, date);
        }

        private Result<FinancialPeriod> ValidateFinancialPeriod(FinancialPeriod period, DateTime date)
        {
            if (period == null)
            {
                string failureMsg = $"Could not find a {nameof(FinancialPeriod)} in Exact Online for the date {date:dd-MMM-yyyy}.";
                return Result.Fail<FinancialPeriod>(failureMsg);
            }

            if (!period.FinYear.HasValue || !period.FinPeriod.HasValue)
            {
                string failureMsg = $"A {nameof(FinancialPeriod)} was found in Exact Online for the date {date:dd-MMM-yyyy} but the Year/Month are not defined correctly.";
                return Result.Fail<FinancialPeriod>(failureMsg);
            }

            return Result.Ok(period);
        }

    }
}