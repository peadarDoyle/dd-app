using System.Collections.Generic;
using System.Linq;
using DirectDebits.Models;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Batches
{
    public class BatchLineViewModel
    {
        public BatchLineViewModel(IEnumerable<Allocation> allocations)
        {
            Id = allocations.First().Account.ExternalDisplayId;
            Name = allocations.First().Account.Name;
            Amount = allocations.Sum(x => x.Amount);
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }
    }
}
