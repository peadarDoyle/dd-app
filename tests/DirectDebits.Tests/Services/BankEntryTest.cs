using System;
using System.Linq;
using System.Xml.Linq;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DirectDebits.ExactClient.Models;
using DirectDebits.ExactClient.Helpers;
using DirectDebits.Common;
using DirectDebits.Common.Utility;
using DirectDebits.Tests.Persistence.Repositories;

namespace DirectDebits.Tests.Services
{
    [TestClass]
    public class ExactXmlTest
    {
        [TestMethod]
        public void Create_BankEntryIsBalanced_IsTrue()
        {
            const BatchType type = BatchType.DirectDebit;

            IBatchRepository batches = new FakeBatchRepo();
            Batch batch = batches.Get(1, type, 1);

            var dd = new ExactTransaction(batch, 2010, 01);

            string bankEntry = BankEntryHelper.Create(dd);
            XDocument document = XDocument.Parse(bankEntry);

            double balance = document.Descendants("GLTransactionLine").Sum(x => double.Parse(x.Element("Amount").Element("Value").Value));

            Assert.AreEqual(0, balance);
        }

        [TestMethod]
        public void Create_BankEntryTransactionLineCountIsCorrect_IsTrue()
        {
            const BatchType type = BatchType.DirectDebit;

            IBatchRepository batches = new FakeBatchRepo();
            Batch batch = batches.Get(1, type, 1);

            var dd = new ExactTransaction(batch, 2010, 01);

            // we start at 1 so as to include the bank entry transaction line
            int transactionLinesCount = 1;

            foreach (ExactUploadAccount account in dd.Accounts)
            {
                transactionLinesCount += account.FullAllocations.Any() ? 1 : 0;
                transactionLinesCount += account.PartialAllocations.Count();
            }

            string bankEntry = BankEntryHelper.Create(dd);
            XDocument document = XDocument.Parse(bankEntry);

            int count = document.Descendants("GLTransactionLine").Count();

            Assert.AreEqual(transactionLinesCount, count);
        }

        [TestMethod]
        public void ValidateBankEntry_IsValid()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"utf - 8\"?><eExact xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"eExact-XML.xsd\"><GLTransactions><GLTransaction><TransactionType number=\"40\"><Description>Cash flow</Description></TransactionType><Journal code=\"20\" type=\"12\"><Description>Bank journal</Description></Journal><PaymentCondition code=\"DD\"/><Date>2016-09-27</Date><GLTransactionLine type=\"40\" linetype=\"0\" line=\"1\" offsetline=\"1\" status=\"20\"><Date>2016-09-27</Date><VATType>S</VATType><FinYear number=\"2016\" /><FinPeriod number=\"9\" /><GLAccount code=\"15400\" type=\"20\"><Description>Trade Debtors</Description></GLAccount><GLOffset code=\"15800\" /><Description>DD Batch 7</Description><Account ID=\"{e6973f50-d262-418e-b18b-4016ea345386}\" code=\"10\"><Name>sligo trading</Name></Account><Amount><Currency code=\"EUR\" /><Value>-222</Value></Amount><ForeignAmount><Currency code=\"EUR\" /><Value>-222</Value><Rate>1</Rate></ForeignAmount><References><InvoiceNumber>16700009</InvoiceNumber></References></GLTransactionLine><GLTransactionLine type=\"40\" linetype=\"0\" line=\"1\" status=\"20\"><Date>2016-09-27</Date><VATType>S</VATType><FinYear number=\"2016\" /><FinPeriod number=\"9\" /><GLAccount code=\"15800\" type=\"12\"><Description>Bank</Description></GLAccount><Description>DD Batch 7</Description><Account ID=\"{e6973f50-d262-418e-b18b-4016ea345386}\" code=\"10\"><Name>sligo trading</Name></Account><Amount><Currency code=\"EUR\" /><Value>222</Value></Amount><ForeignAmount><Currency code=\"EUR\" /><Value>222</Value><Rate>1</Rate></ForeignAmount><References><InvoiceNumber>16700008</InvoiceNumber></References></GLTransactionLine></GLTransaction></GLTransactions><Topics><Topic code=\"GLTransactions\" ts_d=\"0x00000000393CFB8D\" count=\"1\" pagesize=\"100\" /></Topics><Messages /></eExact>";

            XDocument xdoc = XDocument.Parse(xml);
            string exactXsd = AppDomain.CurrentDomain.BaseDirectory + @"\Exact\eExact-XML.xsd";

            bool isValid = ExactXmlHelper.IsValidForSchema(xdoc, exactXsd);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void ValidateBankEntry_IsNotValid()
        {
            // GLTransactions node is changed to INVALID - this should cause the validation against the XSD to fail
            const string xml = "<?xml version=\"1.0\" encoding=\"utf - 8\"?><eExact xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"eExact-XML.xsd\"><INVALID><GLTransaction><TransactionType number=\"40\"><Description>Cash flow</Description></TransactionType><Journal code=\"20\" type=\"12\"><Description>Bank journal</Description></Journal><PaymentCondition code=\"DD\"/><Date>2016-09-27</Date><GLTransactionLine type=\"40\" linetype=\"0\" line=\"1\" offsetline=\"1\" status=\"20\"><Date>2016-09-27</Date><VATType>S</VATType><FinYear number=\"2016\" /><FinPeriod number=\"9\" /><GLAccount code=\"15400\" type=\"20\"><Description>Trade Debtors</Description></GLAccount><GLOffset code=\"15800\" /><Description>DD Batch 7</Description><Account ID=\"{e6973f50-d262-418e-b18b-4016ea345386}\" code=\"10\"><Name>sligo trading</Name></Account><Amount><Currency code=\"EUR\" /><Value>-222</Value></Amount><ForeignAmount><Currency code=\"EUR\" /><Value>-222</Value><Rate>1</Rate></ForeignAmount><References><InvoiceNumber>16700008</InvoiceNumber></References></GLTransactionLine><GLTransactionLine type=\"40\" linetype=\"0\" line=\"1\" status=\"20\"><Date>2016-09-27</Date><VATType>S</VATType><FinYear number=\"2016\" /><FinPeriod number=\"9\" /><GLAccount code=\"15800\" type=\"12\"><Description>Bank</Description></GLAccount><Description>DD Batch 7</Description><Account ID=\"{e6973f50-d262-418e-b18b-4016ea345386}\" code=\"10\"><Name>sligo trading</Name></Account><Amount><Currency code=\"EUR\" /><Value>222</Value></Amount><ForeignAmount><Currency code=\"EUR\" /><Value>222</Value><Rate>1</Rate></ForeignAmount><References><InvoiceNumber>16700008</InvoiceNumber></References></GLTransactionLine></GLTransaction></INVALID><Topics><Topic code=\"GLTransactions\" ts_d=\"0x00000000393CFB8D\" count=\"1\" pagesize=\"100\" /></Topics><Messages /></eExact>";

            XDocument xdoc = XDocument.Parse(xml);
            string exactXsd = AppDomain.CurrentDomain.BaseDirectory + @"\Exact\eExact-XML.xsd";

            bool isValid = ExactXmlHelper.IsValidForSchema(xdoc, exactXsd);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidateBankEntryResponse_IsValid()
        {
            const string xml = "<?xml version =\"1.0\" encoding=\"utf-8\"?><eExact xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"eExact-XML.xsd\"><Messages><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Journal\"><Data keyAlt=\"20\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"5\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"10\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"3\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"4\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"GLTransaction\"><Data keyAlt=\"16200037\" /></Topic><Date>2016-10-14T13:25:21</Date><Description>Created</Description></Message></Messages></eExact>";

            XDocument xdoc = XDocument.Parse(xml);
            string exactXsd = AppDomain.CurrentDomain.BaseDirectory + @"\Exact\eExact-XML.xsd";

            bool isValid = ExactXmlHelper.IsValidForSchema(xdoc, exactXsd);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void ValidateBankEntryResponse_IsNotValid()
        {
            // messages node is changed to INVALID - this should cause the validation against the XSD to fail
            const string xml = "<?xml version =\"1.0\" encoding=\"utf-8\"?><eExact xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"eExact-XML.xsd\"><INVALID><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Journal\"><Data keyAlt=\"20\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"5\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"10\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"3\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"4\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"GLTransaction\"><Data keyAlt=\"16200037\" /></Topic><Date>2016-10-14T13:25:21</Date><Description>Created</Description></Message></INVALID></eExact>";

            XDocument xdoc = XDocument.Parse(xml);
            string exactXsd = AppDomain.CurrentDomain.BaseDirectory + @"\Exact\eExact-XML.xsd";

            bool isValid = ExactXmlHelper.IsValidForSchema(xdoc, exactXsd);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ParseBankEntryXmlDocument_IsOk()
        {
            const string xml = "<?xml version =\"1.0\" encoding=\"utf-8\"?><eExact xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"eExact-XML.xsd\"><Messages><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Journal\"><Data keyAlt=\"20\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"5\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"10\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"3\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"4\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"GLTransaction\"><Data keyAlt=\"16200037\" /></Topic><Date>2016-10-14T13:25:21</Date><Description>Created</Description></Message></Messages></eExact>";

            Result<XDocument> result = ExactXmlHelper.ValidateFormat(xml);

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void ParseBankEntryXmlDocument_IsFail()
        {
            const string xml = "<?xml version =\"1.0\" encoding=\"utf-8\"?><eExact xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"eExact-XML.xsd\"><Messages><Message type=\"3\"><Date>2016-10-14T12:13:24</Date><Description>Data at the root level is invalid. Line 1, position 1.</Description></Message></Messages></eExact>";

            Result<XDocument> result = ExactXmlHelper.ValidateFormat(xml);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("An error occurred uploading information to Exact Online: Data at the root level is invalid. Line 1, position 1.", result.Error);
        }

        [TestMethod]
        public void ValidateBankEntryXmlDocument_IsOk()
        {
            const string xml = "<?xml version =\"1.0\" encoding=\"utf-8\"?><eExact xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"eExact-XML.xsd\"><Messages><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Journal\"><Data keyAlt=\"20\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"5\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"10\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"3\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"4\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"GLTransaction\"><Data keyAlt=\"16200037\" /></Topic><Date>2016-10-14T13:25:21</Date><Description>Created</Description></Message></Messages></eExact>";

            Result<XDocument> parseResult = ExactXmlHelper.ValidateFormat(xml);
            ExactXmlHelper.ValidateContent(parseResult.Value);

            Assert.IsTrue(parseResult.IsSuccess);
            Assert.AreEqual("16200037", parseResult.Value);
        }

        [TestMethod]
        public void ParseBankEntryId_IsOk()
        {
            const string xml = "<?xml version =\"1.0\" encoding=\"utf-8\"?><eExact xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"eExact-XML.xsd\"><Messages><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Journal\"><Data keyAlt=\"20\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"5\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"10\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"3\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"Account\"><Data keyAlt=\"4\" /></Topic><Date>2016-10-14T13:25:16</Date><Description>Updated</Description></Message><Message type=\"2\"><Topic code=\"GLTransactions\" node=\"GLTransaction\"><Data keyAlt=\"16200037\" /></Topic><Date>2016-10-14T13:25:21</Date><Description>Created</Description></Message></Messages></eExact>";
            Result<XDocument> documentResult = ExactXmlHelper.ValidateFormat(xml);
            Result<string> bankEntryIdResult = BankEntryHelper.ParseBankEntryResult(documentResult.Value);

            Assert.IsTrue(documentResult.IsSuccess);
            Assert.AreEqual("16200037", bankEntryIdResult.Value);
        }
    }
}
