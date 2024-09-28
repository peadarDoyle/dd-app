using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using DirectDebits.Models;
using DirectDebits.Core.Banking.Models.AIB;
using DirectDebits.Models.Entities;

namespace DirectDebits.Core.Banking.Payments
{
    public class AibPaymentsFileBuilder : PaymentsFileBuilder
    {
        public override Stream Create(Batch batch, IList<BankAgent> bankAgents, DateTime? updatedProcessingDate)
        {
            DateTime processingDate = updatedProcessingDate ?? batch.ProcessDate;

            XDocument document = CreateDocument();

            var headerData = new HeaderData(batch);
            XElement header = CreateHeader(headerData);

            var paymentData = new PaymentData(batch, processingDate);
            XElement payment = CreatePayment(paymentData);

            IEnumerable<XElement> transactions = CreateTransactions(batch, bankAgents);

            document.Root.Element(Ns1 + "CstmrCdtTrfInitn").Add(header);
            document.Root.Element(Ns1 + "CstmrCdtTrfInitn").Add(payment);

            foreach (XElement transaction in transactions)
            {
                document.Root.Element(Ns1 + "CstmrCdtTrfInitn").Element(Ns1 + "PmtInf").Add(transaction);
            }

            Stream stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;

            return stream;
        }

        private static XElement CreateHeader(HeaderData data)
        {
            return new XElement(Ns1 + "GrpHdr",
                    new XElement(Ns1 + "MsgId", data.MsgId),
                    new XElement(Ns1 + "CreDtTm", data.CreDtTm),
                    new XElement(Ns1 + "NbOfTxs", data.NbOfTxs),
                    new XElement(Ns1 + "CtrlSum", data.CtrlSum),
                    new XElement(Ns1 + "InitgPty", 
                        new XElement(Ns1 + "Id",
                            new XElement(Ns1 + "OrgId",
                                new XElement(Ns1 + "Othr",
                                    new XElement(Ns1 + "Id", data.InitPtyId)
                                )
                            )
                        )
                    )
                );
        }

        private static XElement CreatePayment(PaymentData data)
        {
            return new XElement(Ns1 + "PmtInf",
                    new XElement(Ns1 + "PmtInfId", data.PmtInfId),
                    new XElement(Ns1 + "PmtMtd", data.PmtMtd),
                    new XElement(Ns1 + "NbOfTxs", data.NbOfTxs),
                    new XElement(Ns1 + "CtrlSum", data.CtrlSum),
                    new XElement(Ns1 + "PmtTpInf",
                        new XElement(Ns1 + "SvcLvl",
                            new XElement(Ns1 + "Cd", data.PmtTpInfCd)
                        )
                    ),
                    new XElement(Ns1 + "ReqdExctnDt", data.DateOfRequiredAction),
                    new XElement(Ns1 + "Dbtr",
                        new XElement(Ns1 + "Nm", data.AccountName)
                    ),
                    new XElement(Ns1 + "DbtrAcct",
                        new XElement(Ns1 + "Id", 
                            new XElement(Ns1 + "IBAN", data.Iban)
                        ),
                        new XElement(Ns1 + "Ccy", "EUR")
                    ),
                    new XElement(Ns1 + "DbtrAgt",
                        new XElement(Ns1 + "FinInstnId", 
                            new XElement(Ns1 + "BIC", data.Bic)
                        )
                    )
                );

        }

        private static IEnumerable<XElement> CreateTransactions(Batch batch, IList<BankAgent> bankAgents)
        {
            var allocationsGroupedByAccount = batch.Allocations.GroupBy(x => x.Account);

            foreach (var accountAllocations in allocationsGroupedByAccount)
            {
                string externalId = accountAllocations.Key.ExternalId;
                BankAgent bankAgent = bankAgents.Single(x => x.Id == externalId);

                IEnumerable<Allocation> allocations = accountAllocations.Select(x => x);
                var transactionData = new TransactionData(batch.Organisation.Name, bankAgent, allocations);
                XElement element = CreateTransaction(transactionData);

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
                        new XElement(Ns1 + "Nm", transaction.AgentName)
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
