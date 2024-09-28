using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectDebits.Models.Entities
{
    [Table("Account")]
    public partial class Account : BaseEntity
    {
        public Account()
        {
            Allocations = new HashSet<Allocation>();
        }

        public string ExternalId { get; set; }
        public string ExternalDisplayId { get; set; }
        public string Name { get; set; }
        public Organisation Organisation { get; set; }
        public virtual ICollection<Allocation> Allocations { get; set; }
    }
}
