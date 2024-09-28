using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectDebits.Models.Entities
{
    [Table("Bank")]
    public partial class Bank : BaseEntity
    {
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(10)]
        public string Shorthand { get; set; }
        [MaxLength(3)]
        public string CountryCode { get; set; }
    }
}
