using System;
using System.Linq;
using DirectDebits.Models.Entities;

namespace DirectDebits.Core.Banking.Models.BOI
{
    internal class HeaderData
    {
        public string MsgId { get; }
        public string CreDtTm { get; }
        public string NbOfTxs { get; }
        public string CtrlSum { get; }
        public string InitPtyId { get; }

        public HeaderData(Batch batch)
        {
            // MsgId is a unique field so should be the batch id and not the batch number
            MsgId = $"MSGID{batch.Organisation.Id}{batch.Id}";
            CreDtTm = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            InitPtyId = batch.Organisation.GetSettings(batch.BatchType).AuthId;

            NbOfTxs = batch.Allocations
                           .Select(x => x.Account)
                           .Distinct()
                           .Count()
                           .ToString();

            CtrlSum = batch.Allocations
                           .Sum(x => x.Amount)
                           .ToString("F2");
        }
    }
}