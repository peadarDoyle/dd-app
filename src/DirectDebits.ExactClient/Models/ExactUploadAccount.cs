using System.Collections.Generic;
using DirectDebits.Models.Entities;

namespace DirectDebits.ExactClient.Models
{
    public class ExactUploadAccount
    {
        public string ExternalId { get; set; }
        public string ExternalDisplayId { get; set; }
        public string Name { get; set; }
        public IList<Allocation> FullAllocations { get; set; }
        public IList<Allocation> PartialAllocations { get; set; }
    }
}