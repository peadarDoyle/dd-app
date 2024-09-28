using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DirectDebits.Common;

namespace DirectDebits.Models.Entities
{
    [Table("BatchSettings")]
    public class BatchSettings : TrackedEntity
    {
        public BatchSettings()
        {
            Period1 = 30;
            Period2 = 60;
            Period3 = 90;

            Bank = new Bank { Id = 1 };
            BankAccName = "Bank Acc";
            AuthId = "EI00000000000";
            Bic = "AIB00000";
            Iban = "IEXXAIBXXXXXXXXXXXXXXX";
        }

        [Required]
        public int Period1 { get; set; }

        [Required]
        public int Period2 { get; set; }

        [Required]
        public int Period3 { get; set; }

        [Required]
        [MaxLength(100)]
        public string BankAccName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Bic { get; set; }

        [Required]
        [MaxLength(50)]
        public string Iban { get; set; }

        [Required]
        [MaxLength(50)]
        public string AuthId { get; set; }

        [MaxLength(20)]
        public string BankJournalCode { get; set; }

        [MaxLength(20)]
        public string TradeJournalCode { get; set; }

        [MaxLength(20)]
        public string BankGlCode { get; set; }

        [MaxLength(20)]
        public string TradeGlCode { get; set; }

        /// <summary>
        /// 12 characters used for low level application configuration.
        /// [0] is used for configuring querying outstanding receivables
        /// [1] is unused
        /// [2] is unused
        /// [3] is unused
        /// [4] is unused
        /// [5] is unused
        /// [6] is unused
        /// [7] is unused
        /// [8] is unused
        /// [9] is unused
        /// [10] is unused
        /// [11] is unused
        /// </summary>
        [MaxLength(12)]
        public string LowLevelConfig { get; set; }

        [Required]
        public virtual Bank Bank { get; set; }

        public int? ClassificationFilterId { get; set; }

        public List<string> ValidateForBatchCreate(BatchType type)
        {
            var settingsErrors = new List<string>();

            if (BankJournalCode == null)
                settingsErrors.Add("Bank Journal Code");
            if (BankGlCode == null)
                settingsErrors.Add("Bank G/L Code");
            if (TradeJournalCode == null)
                settingsErrors.Add($"{BatchTypeHelper.GetTradeJournalName(type)} Journal Code");
            if (TradeGlCode == null)
                settingsErrors.Add($"{BatchTypeHelper.GetTradeGlAccountName(type)} G/L Code");

            return settingsErrors;
        }

        public int GetReceivableQueryConfigId()
        {
            if (LowLevelConfig == null || LowLevelConfig.Length != 12)
            {
                return 0;
            }
            int result;
            int.TryParse(LowLevelConfig[0].ToString(), out result);
            return result;
        }
    }
}
