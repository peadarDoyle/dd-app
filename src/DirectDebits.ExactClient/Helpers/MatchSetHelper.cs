using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using DirectDebits.ExactClient.Models;
using DirectDebits.Models.Entities;

namespace DirectDebits.ExactClient.Helpers
{
    public static class MatchSetHelper
    {
        public static string Create(ExactTransaction transaction, string bankEntryId)
        {
            XDocument document = Document();
            IEnumerable<XElement> matches = Matches(transaction, bankEntryId);

            document.Root.Element("MatchSets").Add(matches);

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

        private static XDocument Document()
        {
            const string ns1 = "http://www.w3.org/2001/XMLSchema-instance";
            const string ns2 = "eExact-XML.xsd";

            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("eExact", new XAttribute(XNamespace.Xmlns + "xsi", ns1), new XAttribute("{" + ns1 + "}noNamespaceSchemaLocation", ns2),
                    new XElement("MatchSets"),
                    new XElement("Topics",
                        new XElement("Topic", new XAttribute("code", "MatchSets"))
                    ),
                    new XElement("Messages")
                )
            ); ;
        }

        private static IEnumerable<XElement> Matches(ExactTransaction transaction, string bankEntryId)
        {
            IEnumerable<XElement> matches = new List<XElement>();

            foreach (var account in transaction.Accounts)
            {
                var matchesPerAccount = PerAccountMatches(transaction.Data, account, bankEntryId);
                matches = matches.Union(matchesPerAccount).ToList();
            }

            return matches;
        }

        private static IEnumerable<XElement> PerAccountMatches(ExactUploadData data, ExactUploadAccount account, string bankEntryId)
        {
            var matches = new List<XElement>();

            if (account.FullAllocations.Any())
            {
                XElement mainMatch = FullMatchLine(data, account, bankEntryId);
                matches.Add(mainMatch);
            }

            if (!account.PartialAllocations.Any())
            {
                return matches;
            }

            IEnumerable<XElement> partialTransactionLines = PartialMatches(data, account, bankEntryId);
            return matches.Union(partialTransactionLines);
        }

        private static IEnumerable<XElement> PartialMatches(ExactUploadData data, ExactUploadAccount account, string bankEntryId)
        {
            return account.PartialAllocations.Select(allocation => PartialMatchLine(data, allocation, bankEntryId));
        }

        private static XElement FullMatchLine(ExactUploadData data, ExactUploadAccount account, string bankEntryId)
        {
            IList<XElement> matchLines = account.FullAllocations.Select(alloc =>
                new XElement("MatchLine", new XAttribute("finyear", alloc.InvoiceCreatedOn.Year), new XAttribute("finperiod", alloc.InvoiceCreatedOn.Month),
                new XAttribute("journal", data.TradeJournalCode), new XAttribute("entry", alloc.InvoiceId), new XAttribute("amountdc", alloc.Amount * data.PaymentCondition.MatchingTradeCoefficient))
            ).ToList();

            decimal bankAmount = account.FullAllocations.Sum(x => x.Amount) * data.PaymentCondition.MatchingBankCoefficient;

            var match = new XElement("MatchSet",
                new XElement("GLAccount", new XAttribute("code", data.TradeGlCode)),
                new XElement("Account", new XAttribute("code", account.ExternalDisplayId)),
                new XElement("MatchLines",
                    // bank entry match line
                    new XElement("MatchLine", new XAttribute("finyear", data.FinacialYear), new XAttribute("finperiod", data.FinacialPeriod),
                    new XAttribute("journal", data.BankJournalCode), new XAttribute("entry", bankEntryId), new XAttribute("amountdc", bankAmount))
                )
            );

            match.Element("MatchLines").Add(matchLines);

            return match;
        }

        private static XElement PartialMatchLine(ExactUploadData data, Allocation allocation, string bankEntryId)
        {
            decimal tradeAmount = allocation.InvoiceTotal * data.PaymentCondition.MatchingTradeCoefficient;
            decimal bankAmount = allocation.Amount * data.PaymentCondition.MatchingBankCoefficient;

            return new XElement("MatchSet",
                new XElement("GLAccount", new XAttribute("code", data.TradeGlCode)),
                new XElement("Account", new XAttribute("code", allocation.Account.ExternalDisplayId)),
                new XElement("MatchLines",
                    // invoice match line
                    new XElement("MatchLine", new XAttribute("finyear", allocation.InvoiceCreatedOn.Year), new XAttribute("finperiod", allocation.InvoiceCreatedOn.Month),
                    new XAttribute("journal", data.TradeJournalCode), new XAttribute("entry", allocation.InvoiceId), new XAttribute("amountdc", tradeAmount)),
                    // bank entry match line
                    new XElement("MatchLine", new XAttribute("finyear", data.FinacialYear), new XAttribute("finperiod", data.FinacialPeriod),
                    new XAttribute("journal", data.BankJournalCode), new XAttribute("entry", bankEntryId), new XAttribute("amountdc", bankAmount))
                ),
                new XElement("WriteOff", new XAttribute("type", "0"))
            );
        }
    }
}