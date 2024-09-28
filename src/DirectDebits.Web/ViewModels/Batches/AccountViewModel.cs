using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using DirectDebits.Attributes.Validation;

namespace DirectDebits.ViewModels.Batches
{
    public class AccountViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string DisplayId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [JsonProperty("Invoices")]
        [MustHaveOneElement]
        [DisplayName("Invoices")]
        public InvoiceViewModel[] Invoices { get; set; }
    }
}
