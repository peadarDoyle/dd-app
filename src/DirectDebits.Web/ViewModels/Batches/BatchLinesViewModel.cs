using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Batches
{
    public class BatchLinesViewModel
    {
        public BatchLinesViewModel(Batch batch)
        {
            Total = batch.TotalProcessed;
            BatchLines = batch.Allocations
                              .GroupBy(x => x.Account)
                              .Select(x => new BatchLineViewModel(x.Select(y => y)))
                              .ToList();
        }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        public List<BatchLineViewModel> BatchLines { get; set; }
    }
}
