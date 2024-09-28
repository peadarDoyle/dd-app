using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DirectDebits.Common;

namespace DirectDebits.Models.Entities
{
    [Table("Batch")]
    public class Batch : TrackedEntity
    {
        public Batch()
        {
            Allocations = new HashSet<Allocation>();
        }

        public BatchType BatchType { get; set; }
        public int Number { get; set; }
        public decimal TotalProcessed { get; set; }
        public int AccountsAffected { get; set; }
        public int InvoicesAffected { get; set; }
        public DateTime ProcessDate { get; set; }
        public virtual Organisation Organisation { get; set; }
        public virtual ICollection<Allocation> Allocations { get; set; }
    }
}
