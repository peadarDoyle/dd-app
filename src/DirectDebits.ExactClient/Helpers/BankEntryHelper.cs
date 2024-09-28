using System.Linq;
using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using DirectDebits.Common.Utility;
using System.IO;
using System.Xml.Linq;
using DirectDebits.ExactClient.Models;

namespace DirectDebits.ExactClient.Helpers
{
    public static class BankEntryHelper
    {
        public static string Create(ExactTransaction dd)
        {
            XDocument document = Document(dd.Data);
            IList<XElement> accountTransactions = AccountTransactions(dd);
            XElement bankTransaction = BankTransaction(dd.Data);

            document.Root.Element("GLTransactions").Element("GLTransaction").Add(accountTransactions);
            document.Root.Element("GLTransactions").Element("GLTransaction").Add(bankTransaction);

            // we use the StringWriter approach because we need to call Save on the XDocument
            // we use the XMLTextWriter since we need the XML encoded with utf-8
            using (var stream = new MemoryStream())
            using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                document.WriteTo(writer);
                writer.Flush();
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream))
                {
                    var xml = reader.ReadToEnd();
                    return xml;
                }
            }
        }

        private static XDocument Document(ExactUploadData data)
        {
            string ns1 = "http://www.w3.org/2001/XMLSchema-instance";
            string ns2 = "eExact-XML.xsd";

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("eExact", new XAttribute(XNamespace.Xmlns + "xsi", ns1), new XAttribute("{" + ns1 + "}noNamespaceSchemaLocation", ns2),
                    new XElement("GLTransactions",
                        new XElement("GLTransaction",
                            new XElement("TransactionType", new XAttribute("number", "40"), // 40 is cash flow
                                new XElement("Description", "Cash flow")
                            ),
                            new XElement("Journal", new XAttribute("code", data.BankJournalCode), new XAttribute("type", "12"),
                                new XElement("Description", "Bank journal")
                            ),
                            new XElement("PaymentCondition", new XAttribute("code", data.PaymentCondition.Code)),
                            new XElement("Date", data.ProcessDate)
                        )
                    ),
                    new XElement("Topics",
                        new XElement("Topic", new XAttribute("code", "GLTransactions"))
                    ),
                    new XElement("Messages")
                )
            );

            return document;
        }

        private static IList<XElement> AccountTransactions(ExactTransaction dd)
        {
            IList<XElement> elements = new List<XElement>();

            int offset = 1;

            foreach (var account in dd.Accounts)
            {
                IList<XElement> elementsPerAccount = PerAccountTransactions(dd.Data, account, ref offset);
                elements = elements.Union(elementsPerAccount).ToList();
            }

            return elements;
        }

        private static IList<XElement> PerAccountTransactions(ExactUploadData data, ExactUploadAccount account, ref int offset)
        {
            IList<XElement> elements = new List<XElement>();

            if (account.FullAllocations.Count > 0)
            {
                decimal fullAllocationsTotal = account.FullAllocations.Select(x => x.Amount).Sum();
                string description = $"{data.BatchNumber}";
                XElement mainTransactionLine = TransactionLine(description, fullAllocationsTotal, data, account, offset);
                offset++;
                elements.Add(mainTransactionLine);
            }

            if (account.PartialAllocations.Count > 0)
            {
                IList<XElement> partialTransactionLines = PartialPayments(data, account, ref offset);
                elements = elements.Union(partialTransactionLines).ToList();
            }

            return elements;
        }

        private static IList<XElement> PartialPayments(ExactUploadData data, ExactUploadAccount account, ref int offset)
        {
            var elements = new List<XElement>();

            foreach(var allocation in account.PartialAllocations)
            {
                string description = $"{data.BatchNumber} (partial)";
                var element = TransactionLine(description, allocation.Amount, data, account, offset);
                elements.Add(element);
                offset++;
            }

            return elements;
        }

        private static XElement TransactionLine(string description, decimal amount, ExactUploadData data, ExactUploadAccount account, int offset)
        {
            return new XElement("GLTransactionLine", new XAttribute("type", "40"), new XAttribute("linetype", "0"), new XAttribute("line", "1"), new XAttribute("offsetline", offset), new XAttribute("status", "20"), 
                        new XElement("Date", data.ProcessDate.ToString("yyyy-MM-dd")),
                        new XElement("VATType", "S"),
                        new XElement("FinYear", new XAttribute("number", data.FinacialYear)),
                        new XElement("FinPeriod", new XAttribute("number", data.FinacialPeriod)),
                        new XElement("GLAccount", new XAttribute("code", data.TradeGlCode), new XAttribute("type", "20"),
                            new XElement("Description", "Trade Debtors")
                        ),
                        new XElement("GLOffset", new XAttribute("code", data.BankGlCode)),
                        new XElement("Description", $"{data.PaymentCondition.Name} Batch {description}"),
                        new XElement("Account", new XAttribute("ID", "{" + account.ExternalId + "}"), new XAttribute("code", account.ExternalDisplayId)),
                        new XElement("Amount",
                            new XElement("Currency", new XAttribute("code", "EUR")),
                            new XElement("Value", amount * data.PaymentCondition.BankEntryTradeCoefficient)
                        ));
        }

        private static XElement BankTransaction(ExactUploadData data)
        {
            return new XElement("GLTransactionLine", new XAttribute("type", "40"), new XAttribute("linetype", "0"), new XAttribute("line", "1"), new XAttribute("status", "20"), 
                        new XElement("Date", data.ProcessDate.ToString("yyyy-MM-dd")),
                        new XElement("VATType", "S"),
                        new XElement("FinYear", new XAttribute("number", data.FinacialYear)),
                        new XElement("FinPeriod", new XAttribute("number", data.FinacialPeriod)),
                        new XElement("GLAccount", new XAttribute("code", data.BankGlCode), new XAttribute("type", "12"),
                            new XElement("Description", "Bank")
                        ),
                        new XElement("Description", $"{data.PaymentCondition.Name} Batch {data.BatchNumber}"),
                        new XElement("Amount",
                            new XElement("Currency", new XAttribute("code", "EUR")),
                            new XElement("Value", data.Total  * data.PaymentCondition.BankEntryBankCoefficient)
                        ));
        }

        public static Result<string> ParseBankEntryResult(XDocument document)
        {
            string  bankEntryId =  
               document.Descendants("Messages")
                        .Elements("Message")
                        .Last()
                        .Element("Topic")
                        .Element("Data")
                        .Attribute("keyAlt").Value;

            return Result.Ok(bankEntryId);
        }
    }
}