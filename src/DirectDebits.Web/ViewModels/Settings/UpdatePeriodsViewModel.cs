using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DirectDebits.Attributes.Validation;

namespace DirectDebits.ViewModels.Settings
{
    public class UpdatePeriodsViewModel
    {
        [Required]
        [DisplayName("Period 1")]
        [IsLessThan("Period2", ErrorMessage = "Period 2 must be a greater value than Period 1")]
        [Range(0, 3650, ErrorMessage = "Period 1 must be a value between 0 and 3650")]
        public int Period1 { get; set; }

        [Required]
        [DisplayName("Period 2")]
        [IsLessThan("Period3", ErrorMessage = "Period 3 must be a greater value than Period 2")]
        [Range(0, 3650, ErrorMessage = "Period 2 must be a value between 0 and 3650")]
        public int Period2 { get; set; }

        [Required]
        [DisplayName("Period 3")]
        [Range(0, 3650, ErrorMessage = "Period 3 must be a value between 0 and 3650")]
        public int Period3 { get; set; }
    }
}
