using System;
using System.Linq;
using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.Core.Banking.Models.AIB
{
    internal class PaymentData
    {
        public string PmtInfId { get; }
        public string PmtMtd { get; }
        public string NbOfTxs { get; }
        public string CtrlSum { get; }
        public string DateOfRequiredAction { get; }
        public string AccountName { get; }
        public string Iban { get; }
        public string Bic { get; }
        public string PmtTpInfCd { get; } = "SEPA";
        public string LclInstrmCd { get; } = "CORE";
        public string SeqTp { get; } = "RCUR";

        public PaymentData(Batch batch, DateTime processingDate)
        {
            BatchSettings settings = batch.Organisation.GetSettings(batch.BatchType);

            PmtInfId = "PAYMENT1";
            DateOfRequiredAction = processingDate.ToString("yyyy-MM-dd");
            AccountName = SepaSpecification.TrunctuateAccountName(settings.BankAccName);
            Iban = settings.Iban;
            Bic = settings.Bic;

            NbOfTxs = batch.Allocations
                           .Select(x => x.Account)
                           .Distinct()
                           .Count()
                           .ToString();

            CtrlSum = batch.Allocations
                           .Sum(x => x.Amount)
                           .ToString("F2");

            PmtMtd = batch.BatchType == BatchType.DirectDebit ? "DD" : "TRF";
        }
    }
}
