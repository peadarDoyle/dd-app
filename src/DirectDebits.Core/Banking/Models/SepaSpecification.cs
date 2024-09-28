namespace DirectDebits.Core.Banking.Models
{
    public class SepaSpecification
    {
        public const int EndToEndIdMaxLength = 35;
        public const int AccountNameMaxLength = 70;

        public static string TrunctuateEndToEndId(string endToEndId)
        {
            return endToEndId.Length > EndToEndIdMaxLength
                ? endToEndId.Substring(0, EndToEndIdMaxLength)
                : endToEndId;
        }

        public static string TrunctuateAccountName(string accName)
        {
            return accName.Length > AccountNameMaxLength
                ? accName.Substring(0, AccountNameMaxLength)
                : accName;
        }
    }
}
