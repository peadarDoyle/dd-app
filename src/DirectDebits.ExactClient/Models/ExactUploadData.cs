using DirectDebits.Common;
using System;

namespace DirectDebits.ExactClient.Models
{
    public class ExactUploadData
    {
        public string TradeJournalCode { get; set; }
        public string BankJournalCode { get; set; }
        public string BankGlCode { get; set; }
        public string TradeGlCode { get; set; }
        public int BatchNumber { get; set; }
        public DateTime ProcessDate { get; set; }
        public int FinacialYear { get; set; }
        public int FinacialPeriod { get; set; }
        public decimal Total { get; set; }
        public PaymentCondition PaymentCondition { get; set; }
    }
}