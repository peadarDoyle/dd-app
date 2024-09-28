using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DirectDebits.ViewModels.Settings
{
    public class UpdateBankDetailsViewModel
    {
        [Required]
        public int BankId { get; set; }

        [Required]
        [DisplayName("A/C Name")]
        [MaxLength(500, ErrorMessage = "The name of the bank account must be less than 100 characters")]
        public string BankAccName { get; set; }

        [Required]
        [DisplayName("BIC")]
        [MaxLength(50, ErrorMessage = "The BIC is limited to 50 characters")]
        public string Bic { get; set; }

        [Required]
        [DisplayName("IBAN")]
        [MaxLength(50, ErrorMessage = "The IBAN is limited to 50 characters")]
        public string Iban { get; set; }

        [Required]
        [DisplayName("Auth ID")]
        [MaxLength(50, ErrorMessage = "The Auth ID is limited to 50 characters")]
        public string AuthId { get; set; }
    }
}
