using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using DirectDebits.Common.Utility;
using DirectDebits.Models.Entities;

namespace DirectDebits.Core.Banking
{
    public abstract class BankFileBuilder
    {
        protected const string Ns2 = "http://www.w3.org/2001/XMLSchema-instance";

        public abstract Stream Create(Batch batch, IList<BankAgent> bankAgents, DateTime? updatedProcessingDate);

        public static Result Validate(string bankFile)
        {
            try
            {
                XDocument.Parse(bankFile);
            }
            catch
            {
                return Result.Fail("The format of the bank file is not correct");
            }

            return Result.Ok();
        }
    }
}
