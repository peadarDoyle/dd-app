using System.ComponentModel.DataAnnotations;

namespace DirectDebits.Models.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
