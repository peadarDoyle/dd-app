using System;
using System.Collections.Generic;
using DirectDebits.Common;
using ExactOnline.Client.Models.CRM;

namespace DirectDebits.ExactClient.Contracts
{
    public interface IExactCrmService
    {
        IList<AccountClassification> GetAccountClassificationById(Guid id);
        AccountClassificationName GetAccountClassificationNameByCode(int code);
        IList<Account> GetAccountsByBatchType(BatchType type);
        IList<Account> GetAccountsByClassificationAndBatchType(int classificationCode, IList<Guid> filters, BatchType type);
        IList<Account> GetAccountsById(string[] accountIds);
    }
}