using System.Linq;
using DirectDebits.Common.Utility;
using ExactOnline.Client.Models.CRM;

namespace DirectDebits.Core.Banking
{
    public class Debtor : BankAgent
    {
        public Debtor() { }
        public Debtor(Account account) : base (account)
        {
            MandateId = account.BankAccounts.Single(x => x.Main.Value).Description;

            if (account.StartDate.HasValue)
            {
                MandateSignatureDate = account.StartDate.Value.ToString("yyyy-MM-dd");
            }
        }

        public string MandateId { get; set; }
        public string MandateSignatureDate { get; set; }

        public override Result Validate()
        {
            Result baseResult = base.Validate();

            if (!baseResult.IsSuccess || string.IsNullOrWhiteSpace(MandateId) || string.IsNullOrWhiteSpace(MandateSignatureDate))
            {
                return Result.Fail(ValidationErrorMsg);
            }

            return Result.Ok();
        }
    }
}
