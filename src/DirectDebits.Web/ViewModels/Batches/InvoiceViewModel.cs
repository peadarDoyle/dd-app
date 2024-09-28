using ExactOnline.Client.Models.Cashflow;
using System.ComponentModel.DataAnnotations;
using System;

namespace DirectDebits.ViewModels.Batches
{
    public class InvoiceViewModel
    {
        // empty constructor is used for deserialization
        public InvoiceViewModel() { }

        public InvoiceViewModel(Receivable invoice)
        {
            Id = invoice.InvoiceNumber.Value;
            var amount = Convert.ToDecimal(invoice.AmountDC);
            Amount = decimal.Negate(amount);
            YourRef = invoice.YourRef;
            Description = invoice.Description;

            if (invoice.InvoiceDate.HasValue)
            {
                InvoiceDate = invoice.InvoiceDate.Value;
            }
        }

        public InvoiceViewModel(Payment invoice)
        {
            Id = invoice.InvoiceNumber.Value;
            Amount = Convert.ToDecimal(invoice.AmountDC);
            YourRef = invoice.YourRef;
            Description = invoice.Description;

            if (invoice.InvoiceDate.HasValue)
            {
                InvoiceDate = invoice.InvoiceDate.Value;
            }
        }

        [Required]
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public decimal Alloc { get; set; }

        public string Description { get; set; }
        public string YourRef { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
}
