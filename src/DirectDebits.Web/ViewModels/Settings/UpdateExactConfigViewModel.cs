using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DirectDebits.ViewModels.Settings
{
    public class UpdateExactConfigViewModel
    {
        [Required]
        [DisplayName("Bank Journal Code")]
        [MaxLength(50, ErrorMessage = "The length of the Bank Journal Code cannot exceed 50")]
        public string BankJournalCode { get; set; }

        [Required]
        [DisplayName("Trade Journal Code")]
        [MaxLength(50, ErrorMessage = "The length of the Trade Journal Code cannot exceed 50")]
        public string TradeJournalCode { get; set; }

        [Required]
        [DisplayName("Bank G/L Code")]
        [MaxLength(50, ErrorMessage = "The length of the Bank G/L Code cannot exceed 50")]
        public string BankGlCode { get; set; }

        [Required]
        [DisplayName("Trade G/L Code")]
        [MaxLength(50, ErrorMessage = "The length of the Trade G/L Code cannot exceed 50")]
        public string TradeGlCode { get; set; }

        [DisplayName("Classification Filter ID")]
        [Range(1, 8, ErrorMessage = "The Classification Filter ID value must be between 1 and 8")]
        public int? ClassificationFilterId { get; set; }
    }
}
