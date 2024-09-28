using System;

namespace DirectDebits.Models.Entities
{
    public abstract class TrackedEntity : BaseEntity
    {
        protected TrackedEntity()
        {
            DateTime now = DateTime.Now;

            CreatedOn = now;
            CreatedBy = null;
            UpdatedOn = null;
            UpdatedBy = null;
        }

        public DateTime CreatedOn { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public ApplicationUser UpdatedBy { get; set; }
    }
}
