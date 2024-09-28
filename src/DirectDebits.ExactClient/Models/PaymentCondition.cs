using DirectDebits.Common;
using System;

namespace DirectDebits.ExactClient.Models
{
    public class PaymentCondition
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int BankEntryBankCoefficient { get; set; }
        public int BankEntryTradeCoefficient { get; set; }
        public int MatchingBankCoefficient { get; set; }
        public int MatchingTradeCoefficient { get; set; }

        public PaymentCondition(BatchType type)
        {
            switch (type)
            {
                case BatchType.DirectDebit:
                    Code = "DD";
                    Name = "DD";
                    BankEntryBankCoefficient = 1;
                    break;
                case BatchType.Payment:
                    Code = "EF";
                    Name = "EFT";
                    BankEntryBankCoefficient = -1;
                    break;
                default:
                    throw new ArgumentException($"The batch type [{type}] is not recognised");
            }

            BankEntryTradeCoefficient = BankEntryBankCoefficient * -1;
            MatchingBankCoefficient = BankEntryTradeCoefficient;
            MatchingTradeCoefficient = MatchingBankCoefficient * -1;
        }
    }
}