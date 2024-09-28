using System;
using System.Linq;
using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.Core.Banking.Models.BOI
{
    internal class PaymentData
    {
        public string PmtInfId { get; }
        public string PaymentMethod { get; }
        public string NbOfTxs { get; }
        public string CtrlSum { get; }
        public string DateOfRequiredAction { get; }
        public string AccountName { get; }
        public string IBAN { get; }
        public string Bic { get; }
        public string CdtrSchmeId { get; }
        public string InstructionPriority { get; } = "NORM";
        public string PaymentTypeCode { get; } = "SEPA";
        public string LocalInstrumentCode { get; } = "CORE";
        public string SequenceType { get; } = "RCUR";
        public string CdtrSchmeName { get; } = "SEPA";

        public PaymentData(Batch batch, DateTime processingDate)
        {
            BatchSettings settings = batch.Organisation.GetSettings(batch.BatchType);

            PmtInfId = "PAYMENT1";
            DateOfRequiredAction = processingDate.ToString("yyyy-MM-dd");
            AccountName = SepaSpecification.TrunctuateAccountName(settings.BankAccName);
            IBAN = settings.Iban;
            Bic = settings.Bic;
            CdtrSchmeId = settings.AuthId;

            NbOfTxs = batch.Allocations
                           .Select(x => x.Account)
                           .Distinct()
                           .Count()
                           .ToString();

            CtrlSum = batch.Allocations
                           .Sum(x => x.Amount)
                           .ToString("F2");

            PaymentMethod = batch.BatchType == BatchType.DirectDebit ? "DD" : "TRF";
        }
    }
}
