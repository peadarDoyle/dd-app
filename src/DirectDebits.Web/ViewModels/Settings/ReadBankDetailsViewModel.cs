using System.ComponentModel;
using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Settings
{
    public class ReadBankDetailsViewModel
    {
        public ReadBankDetailsViewModel(BatchType type, BatchSettings settings)
        {
            BatchType = type;
            BankId = settings.Bank.Id;
            BankAccName = settings.BankAccName;
            BIC = settings.Bic;
            IBAN = settings.Iban;
            AuthId = settings.AuthId;
            BankName = settings.Bank.Name;
            BankShorthand = settings.Bank.Shorthand;
        }

        public BatchType BatchType { get; set; }
        public int BankId { get; set; }

        [DisplayName("Bank Name")]
        public string BankName { get; set; }

        [DisplayName("Bank Shorthand")]
        public string BankShorthand { get; set; }

        [DisplayName("A/C Name")]
        public string BankAccName { get; set; }

        [DisplayName("BIC")]
        public string BIC { get; set; }

        [DisplayName("IBAN")]
        public string IBAN { get; set; }

        [DisplayName("Auth ID")]
        public string AuthId { get; set; }
    }
}
