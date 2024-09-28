using System;
using System.Linq;
using System.Collections.Generic;
using DirectDebits.Common.Utility;
using ExactOnline.Client.Models.CRM;
using DirectDebits.Common;
using DirectDebits.ExactClient.Contracts;
using Serilog;

namespace DirectDebits.ExactClient.Services
{
    public class ExactCrmService : ExactServiceBase, IExactCrmService
    {
        private const string DEBTOR_PAYMENT_CONDITION = "DD";
        private const string CREDITOR_PAYMENT_CONDITION = "EF";

        public ExactCrmService(
            ILogger logger,
            int? division) : base (logger, division) { }

        public AccountClassificationName GetAccountClassificationNameByCode(int code)
        {
            const int limit = 100;

            var fields = "ID,Description";
            string query = "SequenceNumber+eq+" + code;

            return Client.For<AccountClassificationName>()
                         .Select(fields)
                         .Where(query)
                         .Get()
                         .Take(limit)
                         .Single();
        }

        public IList<AccountClassification> GetAccountClassificationById(Guid id)
        {
            const int limit = 100;

            var fields = "ID,Description";
            string query = $"AccountClassificationName+eq+guid'{id}'";

            return Client.For<AccountClassification>()
                         .Select(fields)
                         .Where(query)
                         .Get()
                         .Take(limit)
                         .ToList();
        }

        public IList<Account> GetAccountsById(string[] accountIds)
        {
            Logger.Information("Attempting to retrieve a list of accounts from exact");
            Logger.Information("Accounts to be retrieved have the accountIds:{@AccountIds}", string.Join(", ", accountIds));

            const int accountsToQuery = 60;

            var accounts = new List<Account>();

            for (var i = 0; ; i++)
            {
                var queries = accountIds.Skip(accountsToQuery * i)
                                        .Take(accountsToQuery)
                                        .Select(x => "ID+eq+guid'" + x + "'");

                if (!queries.Any()) break;

                string query = string.Join("+or+", queries);

                Logger.Information("Iteration {@Iteration} attempting query:{@Query}", i, query);
                var pageCollection = GetAccounts(query);
                Logger.Information("Iteration {@Iteration} retrieved {@Count}/{@Limit} accounts", i, pageCollection.Count, accountsToQuery);

                accounts.AddRange(pageCollection);

                if (queries.Count() < accountsToQuery) break;
            }

            Logger.Information("Retrieved the list of accounts from exact");

            return accounts;
        }

        public IList<Account> GetAccountsByBatchType(BatchType type)
        {
            string query;

            switch (type)
            {
                case BatchType.DirectDebit:
                    query = $"PaymentConditionSales+eq+'{DEBTOR_PAYMENT_CONDITION}'";
                    break;
                case BatchType.Payment:
                    query = $"PaymentConditionPurchase+eq+'{CREDITOR_PAYMENT_CONDITION}'";
                    break;
                default:
                    throw new ArgumentException($"The batch type is not recognised");
            }

            return GetAccounts(query);
        }

        public IList<Account> GetAccountsByClassificationAndBatchType(int classificationCode, IList<Guid> filters, BatchType type)
        {
            if (filters.IsNullOrEmpty())
            {
                throw new ArgumentException("At least one classification filter must be applied");
            }

            string conditionQuery;

            switch (type)
            {
                case BatchType.DirectDebit:
                    conditionQuery = $"PaymentConditionSales+eq+'{DEBTOR_PAYMENT_CONDITION}'";
                    break;
                case BatchType.Payment:
                    conditionQuery = $"PaymentConditionPurchase+eq+'{CREDITOR_PAYMENT_CONDITION}'";
                    break;
                default:
                    throw new ArgumentException($"The batch type [{type}] is not recognised");
            }

            IEnumerable<string> filterQueryParts = filters.Select(x => $"Classification{classificationCode}+eq+guid'{x}'");
            string filterQuery = string.Join("+or+", filterQueryParts);
            string query = $"{conditionQuery}+and+({filterQuery})";

            return GetAccounts(query);
        }

        private IList<Account> GetAccounts(string query)
        {
            string fields = "ID,Code,BankAccounts,Name,StartDate";
            return GetPaginatedRecords<Account>(fields, query, "BankAccounts");
        }
    }
}