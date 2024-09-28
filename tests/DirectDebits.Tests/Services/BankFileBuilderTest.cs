using System;
using System.IO;
using System.Collections.Generic;
using DirectDebits.Core.Banking;
using DirectDebits.Core.Banking.DirectDebit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Xml.Schema;
using DirectDebits.Common;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using DirectDebits.Tests.Persistence.Repositories;

namespace DirectDebits.Tests.Services
{
    [TestClass]
    public class BankFileBuilderTest
    {
        private readonly string _painXsd = AppDomain.CurrentDomain.BaseDirectory + @"\BankFile\pain.008.001.02.xsd";
        private readonly XmlSchemaSet _schemas = new XmlSchemaSet();
        private readonly IBatchRepository _batches = new FakeBatchRepo();
        private readonly Batch _batch;

        private BankFileBuilder _bankFileBuilder;

        private readonly IList<BankAgent> _bankAgents = new List<BankAgent>
        {
            new Debtor { Id = Guid.Parse("e6973f50-d262-418e-0001-4016ea345386").ToString(), BankAccName = "Name 1", Bic = "AIBKIE2D", Iban = "IE22AIBXXXXXXXXXXXXXXX", MandateId = "MND1", MandateSignatureDate = "2016-01-01" },
            new Debtor { Id = Guid.Parse("e6973f50-d262-418e-0002-4016ea345386").ToString(), BankAccName = "Name 2", Bic = "AIBKIE2D", Iban = "IE22AIBXXXXXXXXXXXXXXX", MandateId = "MND2", MandateSignatureDate = "2016-01-01" },
            new Debtor { Id = Guid.Parse("e6973f50-d262-418e-0003-4016ea345386").ToString(), BankAccName = "Name 3", Bic = "AIBKIE2D", Iban = "IE22AIBXXXXXXXXXXXXXXX", MandateId = "MND3", MandateSignatureDate = "2016-01-01" },
            new Debtor { Id = Guid.Parse("e6973f50-d262-418e-0004-4016ea345386").ToString(), BankAccName = "Name 4", Bic = "AIBKIE2D", Iban = "IE22AIBXXXXXXXXXXXXXXX", MandateId = "MND4", MandateSignatureDate = "2016-01-01" },
        };

        public BankFileBuilderTest()
        {
            _schemas.Add(null, _painXsd);
            _batch = _batches.Get(1, BatchType.DirectDebit, 1);
        }

        [TestMethod]
        public void CreateAibBankFile()
        {
            _bankFileBuilder = new AibDirectDebitFileBuilder();

            string bankFile;

            using (Stream stream = _bankFileBuilder.Create(_batch, _bankAgents, null))
            using (var reader = new StreamReader(stream))
            {
                bankFile = reader.ReadToEnd();
            }

            try
            {
                XDocument xdoc = XDocument.Parse(bankFile);
                xdoc.Validate(_schemas, null);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void CreateBoiBankFile()
        {
            _bankFileBuilder = new BoiDirectDebitFileBuilder();

            string bankFile;

            using (Stream stream = _bankFileBuilder.Create(_batch, _bankAgents, null))
            using (var reader = new StreamReader(stream))
            {
                bankFile = reader.ReadToEnd();
            }

            try
            {
                XDocument xdoc = XDocument.Parse(bankFile);
                xdoc.Validate(_schemas, null);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}