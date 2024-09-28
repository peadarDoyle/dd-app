using System;

namespace DirectDebits.Common
{
    public static class BatchTypeHelper
    {
        public static string GetTradeJournalName(BatchType type)
        {
            switch (type)
            {
                case BatchType.DirectDebit:
                    return "Sales";
                case BatchType.Payment:
                    return "Purchase";
                default:
                    throw new ArgumentException($"The batch type [{type}] is not recognised");
            }
        }

        public static string GetTradeGlAccountName(BatchType type)
        {
            switch (type)
            {
                case BatchType.DirectDebit:
                    return "Trade Debtor";
                case BatchType.Payment:
                    return "Trade Creditor";
                default:
                    throw new ArgumentException($"The batch type [{type}] is not recognised");
            }
        }

        public static string GetPaymentCodition(BatchType type)
        {
            switch (type)
            {
                case BatchType.DirectDebit:
                    return "DD";
                case BatchType.Payment:
                    return "EF";
                default:
                    throw new ArgumentException($"The batch type [{type}] is not recognised");
            }
        }

        /// <summary>
        /// the approach recommended by James Doyle is to have 8 characters following
        /// the PAIN008 (or PAIN001 for payments) with the batch number occupying the
        /// least significant numbers
        /// </summary>
        public static string GetFileName(BatchType type, int batchNumber)
        {
            string painNumber;

            switch (type)
            {
                case BatchType.DirectDebit:
                    painNumber = "8";
                    break;
                case BatchType.Payment:
                    painNumber = "1";
                    break;
                default:
                    throw new ArgumentException($"The batch type [{type}] is not recognised");
            }

            const string foundation = "00000000";
            int length = foundation.Length - batchNumber.ToString().Length;
            string fileNameVariant = foundation.Substring(0, length) + batchNumber;

            return $"PAIN00{painNumber}{fileNameVariant}.xml";
        }
    }
}
