using System;
using System.Net;
using DirectDebits.Common.Utility;
using System.Xml.Linq;
using DirectDebits.ExactClient.Helpers;
using DirectDebits.ExactClient.Contracts;
using Serilog;

namespace DirectDebits.ExactClient.Services
{
    public class ExactXmlService : ExactServiceBase, IExactXmlService
    {
        public ExactXmlService(ILogger logger, int? division) : base (logger, division) { }

        public Result<string> UploadBankEntry(string bankEntryXml)
        {
            if (string.IsNullOrEmpty(bankEntryXml))
            {
                throw new ArgumentNullException(nameof(bankEntryXml));
            }

            Result<XDocument> bankEntryXmlResponse;

            try
            {
                Logger.Information("Attempting to upload the XML bank entry: {@BankEntryXml}", bankEntryXml);
                bankEntryXmlResponse = XmlUpload(bankEntryXml, topic: "GLTransactions", uploadName: nameof(UploadBankEntry));
                Logger.Information("Bank entry upload response was received: {@BankEntryXmlResponse}", bankEntryXmlResponse?.Value.ToString());
            }
            catch (WebException ex)
            {
                const string message = "Could not upload bank entry data to Exact Online";
                Logger.Error(ex, message);
                return Result.Fail<string>(message);
            }

            Result<string> bankEntryResult;

            try
            {
                Logger.Information("Attempting to parse the bank entry XML response");
                bankEntryResult = BankEntryHelper.ParseBankEntryResult(bankEntryXmlResponse.Value);
                Logger.Information("Bank entry information parsed successfully from the bank entry XML response: {@BankEntryResult}", bankEntryResult?.Value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Could not parse the bank entry result, check if the data isn't the expected XML format");
                return Result.Fail<string>("Exact Online encountered an issue with the bank entry we attempted to upload");
            }


            return bankEntryResult;
        }

        public Result UploadMatchSets(string matchSetsXml)
        {
            if (string.IsNullOrEmpty(matchSetsXml))
            {
                throw new ArgumentNullException("The match set XML data is NULL or Empty");
            }

            Result<XDocument> matchSetsXmlResponse;

            try
            {
                Logger.Information("Attempting to upload the XML match sets: {@MatchSetsXml}", matchSetsXml);
                matchSetsXmlResponse = XmlUpload(matchSetsXml, topic: "MatchSets", uploadName: nameof(UploadMatchSets));
                Logger.Information("Match sets upload response was received: {@MatchSetsXmlResponse}", matchSetsXmlResponse?.Value.ToString());
            }
            catch(WebException ex)
            {
                const string message = "Could not upload matching data to Exact Online.";
                Logger.Error(ex, message);
                return Result.Fail(message);
            }

            return matchSetsXmlResponse;
        }
    }
}