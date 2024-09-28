using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using DirectDebits.Core.Banking.Models.BOI;
using DirectDebits.Models.Entities;

namespace DirectDebits.Core.Banking.Payments
{
    public class BoiPaymentsFileBuilder : PaymentsFileBuilder
    {
        public override Stream Create(Batch batch, IList<BankAgent> bankAgents, DateTime? updatedProcessingDate)
        {
            DateTime processingDate = updatedProcessingDate ?? batch.ProcessDate;

            XDocument document = CreateDocument();
            XElement header = CreateHeader(batch);
            XElement payment = CreatePayment(batch, processingDate);
            IEnumerable<XElement> transactions = CreateTransactions(batch, bankAgents);

            document.Root.Element(Ns1 + "CstmrCdtTrfInitn").Add(header);
            document.Root.Element(Ns1 + "CstmrCdtTrfInitn").Add(payment);

            foreach(XElement transaction in transactions)
            {
                document.Root.Element(Ns1 + "CstmrCdtTrfInitn").Element(Ns1 + "PmtInf").Add(transaction);
            }

            Stream stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;

            return stream;
        }

        private static XElement CreateHeader(Batch batch)
        {
            var settings = new HeaderData(batch);

            return new XElement(Ns1 + "GrpHdr",
                    new XElement(Ns1 + "MsgId", settings.MsgId),
                    new XElement(Ns1 + "CreDtTm", settings.CreDtTm),
                    new XElement(Ns1 + "NbOfTxs", settings.NbOfTxs),
                    new XElement(Ns1 + "CtrlSum", settings.CtrlSum),
                    new XElement(Ns1 + "InitgPty",
                        new XElement(Ns1 + "Id",
                            new XElement(Ns1 + "PrvtId",
                                new XElement(Ns1 + "Othr",
                                    new XElement(Ns1 + "Id", settings.InitPtyId)
                                )
                            )
                        )
                    )
                );
        }


        private static XElement CreatePayment(Batch batch, DateTime processingDate)
        {
            var payment = new PaymentData(batch, processingDate);

            return new XElement(Ns1 + "PmtInf",
                    new XElement(Ns1 + "PmtInfId", payment.PmtInfId),
                    new XElement(Ns1 + "PmtMtd", payment.PaymentMethod),
                    new XElement(Ns1 + "BtchBookg", "true"),
                    new XElement(Ns1 + "NbOfTxs", payment.NbOfTxs),
                    new XElement(Ns1 + "CtrlSum", payment.CtrlSum),
                    new XElement(Ns1 + "PmtTpInf",
                        new XElement(Ns1 + "InstrPrty", payment.InstructionPriority),
                        new XElement(Ns1 + "SvcLvl",
                            new XElement(Ns1 + "Cd", payment.PaymentTypeCode)
                        )
                    ),
                    new XElement(Ns1 + "ReqdExctnDt", payment.DateOfRequiredAction),
                    new XElement(Ns1 + "Dbtr",
                        new XElement(Ns1 + "Nm", payment.AccountName),
                        new XElement(Ns1 + "PstlAdr",
                            new XElement(Ns1 + "Ctry", "IE")
                        )
                    ),
                    new XElement(Ns1 + "DbtrAcct",
                        new XElement(Ns1 + "Id",
                            new XElement(Ns1 + "IBAN", payment.IBAN)
                        ),
                        new XElement(Ns1 + "Ccy", "EUR")
                    ),
                    new XElement(Ns1 + "DbtrAgt",
                        new XElement(Ns1 + "FinInstnId",
                            new XElement(Ns1 + "BIC", payment.Bic)
                        )
                    ),
                    new XElement(Ns1 + "ChrgBr", "SLEV")
                );
        }

        private static IEnumerable<XElement> CreateTransactions(Batch batch, IList<BankAgent> bankAgents)
        {
            var allocationsGroupedByAccount = batch.Allocations.GroupBy(x => x.Account);

            foreach (var accountAllocations in allocationsGroupedByAccount)
            {
                string externalId = accountAllocations.Key.ExternalId;
                BankAgent bankAgent = bankAgents.Single(x => x.Id == externalId);

                var allocations = accountAllocations.Select(x => x);
                var transaction = new TransactionData(batch.Organisation.Name, bankAgent, allocations);
                XElement element = CreateTransaction(transaction);

                yield return element;
            }
        }

        private static XElement CreateTransaction(TransactionData transaction)
        {
            return new XElement(Ns1 + "CdtTrfTxInf",
                    new XElement(Ns1 + "PmtId",
                        new XElement(Ns1 + "EndToEndId", transaction.EndToEndId)
                    ),
                    new XElement(Ns1 + "Amt",
                        new XElement(Ns1 + "InstdAmt", transaction.InstructedAmount, new XAttribute("Ccy", "EUR"))
                    ),
                    new XElement(Ns1 + "CdtrAgt",
                        new XElement(Ns1 + "FinInstnId",
                            new XElement(Ns1 + "BIC", transaction.AgentBic)
                        )
                    ),
                    new XElement(Ns1 + "Cdtr",
                        new XElement(Ns1 + "Nm", transaction.AgentName),
                        new XElement(Ns1 + "PstlAdr", "IE")
                    ),
                    new XElement(Ns1 + "CdtrAcct",
                        new XElement(Ns1 + "Id",
                            new XElement(Ns1 + "IBAN", transaction.AgentIban)
                        )
                    )
                );
        }
    }
}
