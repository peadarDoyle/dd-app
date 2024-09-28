using System.Xml.Linq;

namespace DirectDebits.Core.Banking.Payments
{
    public abstract class PaymentsFileBuilder : BankFileBuilder
    {
        protected static readonly XNamespace Ns1 = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";

        protected XDocument CreateDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(Ns1 + "Document", new XAttribute(XNamespace.Xmlns + "xsi", Ns2),
                    new XElement(Ns1 + "CstmrCdtTrfInitn")
                )
            );
        }
    }
}
