using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DirectDebits.Attributes.Validation;

namespace DirectDebits.ViewModels.Batches
{
    public class CreateBatchViewModel
    {
        [Required]
        public DateTime? ProcessDate { get; set; }

        [Required]
        [MustHaveOneElement]
        [DisplayName("Accounts")]
        public AccountViewModel[] Accounts { get; set; }
    }
}
