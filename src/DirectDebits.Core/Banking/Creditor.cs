using ExactOnline.Client.Models.CRM;

namespace DirectDebits.Core.Banking
{
    public class Creditor : BankAgent
    {
        public Creditor() { }
        public Creditor(Account account) : base (account) { }
    }
}
