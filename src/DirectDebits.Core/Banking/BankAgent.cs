using System.Linq;
using DirectDebits.Common.Utility;
using ExactOnline.Client.Models.CRM;
using DirectDebits.Common;
using System;

namespace DirectDebits.Core.Banking
{
    public abstract class BankAgent
    {
        protected string ValidationErrorMsg;

        protected BankAgent() { }
        protected BankAgent(Account account)
        {
            ValidationErrorMsg = $"There is an issue with {account.Name} bank details.";

            Id = account.ID.ToString();
            Name = account.Name;

            BankAccount bankAcc = account.BankAccounts.Single(x => x.Main.Value);
            BankAccName = RemoveSpecialChars(bankAcc.BankAccountHolderName);
            Bic = bankAcc.BICCode;
            Iban = bankAcc.BankAccount;
        }

        public static BankAgent Create(BatchType type, Account account)
        {
            switch (type)
            {
                case BatchType.DirectDebit:
                    return new Debtor(account);
                case BatchType.Payment:
                    return new Creditor(account);
                default:
                    throw new ArgumentException($"The batch type [{type}] is not recognised");
            }
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string BankAccName { get; set; }
        public string Bic { get; set; }
        public string Iban { get; set; }

        public virtual Result Validate()
        {
            if (string.IsNullOrWhiteSpace(BankAccName) || string.IsNullOrWhiteSpace(Bic) || string.IsNullOrWhiteSpace(Iban))
            {
                return Result.Fail(ValidationErrorMsg);
            }

            return Result.Ok();
        }

        protected string RemoveSpecialChars(string str)
        {
            return str.Replace("<", string.Empty).Replace(">", string.Empty).Replace("&", "and");
        }
    }
}
