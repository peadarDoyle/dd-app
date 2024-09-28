using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DirectDebits.ViewModels.Settings
{
    public class UpdateAppConfigViewModel
    {
        [Required]
        [DisplayName("Low Level Config")]
        [MaxLength(12, ErrorMessage = "The length of the Low Level Config cannot exceed 12")]
        public string LowLevelConfig { get; set; }
    }
}
