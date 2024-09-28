using System;
using DirectDebits.Common;
using DirectDebits.Core.Banking.DirectDebit;
using DirectDebits.Core.Banking.Payments;

namespace DirectDebits.Core.Banking
{
    public static class BankFileBuilderFactory
    {
        public static BankFileBuilder Create(BatchType type, string bankName)
        {
            if (type == BatchType.DirectDebit)
            {
                switch (bankName)
                {
                    case "Allied Irish Banks":
                        return new AibDirectDebitFileBuilder();
                    case "Bank of Ireland":
                        return new BoiDirectDebitFileBuilder();
                    case "Ulster Bank":
                        return new UbDirectDebitFileBuilder();
                }
            }

            if (type == BatchType.Payment)
            {
                switch (bankName)
                {
                    case "Allied Irish Banks":
                        return new AibPaymentsFileBuilder();
                    case "Bank of Ireland":
                        return new BoiPaymentsFileBuilder();
                }
            }

            throw new ArgumentOutOfRangeException($"There are no bank file builders for [type: {type}] and [bank: {bankName}]");
        }
    }
}
