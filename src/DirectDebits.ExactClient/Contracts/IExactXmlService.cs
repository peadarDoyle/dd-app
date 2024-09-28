using DirectDebits.Common.Utility;

namespace DirectDebits.ExactClient.Contracts
{
    public interface IExactXmlService
    {
        Result<string> UploadBankEntry(string bankEntryXML);
        Result UploadMatchSets(string matchSetsXML);
    }
}