using System.ComponentModel.DataAnnotations;

namespace DirectDebits.Common
{
    public enum BatchType
    {
        [Display(Name="Direct Debit")]
        DirectDebit,
        [Display(Name="Payment")]
        Payment
    }
}
