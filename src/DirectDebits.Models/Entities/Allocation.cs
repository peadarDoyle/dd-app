using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectDebits.Models.Entities
{
    [Table("Allocation")]
    public partial class Allocation : BaseEntity
    {
        [MaxLength(20)]
        public string InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceCreatedOn { get; set; }
        public decimal InvoiceTotal { get; set; }
        public virtual Account Account { get; set; }
        public virtual Batch Batch { get; set; }
    }
}
