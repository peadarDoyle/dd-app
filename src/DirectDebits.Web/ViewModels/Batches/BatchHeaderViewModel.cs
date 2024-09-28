using System;
using System.ComponentModel.DataAnnotations;
using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Batches
{
    public class BatchHeaderViewModel
    {
        public BatchHeaderViewModel(Batch batch)
        {
            ProcessDate = batch.ProcessDate;
            Number = batch.Number;
            CreatedOn = batch.CreatedOn;
            Total = batch.TotalProcessed;
            AccountsAffectedCount = batch.AccountsAffected;
            Type = batch.BatchType;
        }

        [DataType(DataType.Date)]
        [Display(Name = "Process Date")]
        [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy}")]
        public DateTime ProcessDate { get; set; }

        [Display(Name = "Batch")]
        [DisplayFormat(DataFormatString = "{0:D6}")]
        public int Number { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:ddd dd MMM yyyy HH:mm:ss}")]
        public DateTime CreatedOn { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        [Display(Name = "Accounts")]
        public int AccountsAffectedCount { get; set; }

        public BatchType Type { get; set; }
    }
}
