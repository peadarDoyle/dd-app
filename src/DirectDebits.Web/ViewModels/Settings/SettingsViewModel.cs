using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Settings
{
    public class SettingsViewModel
    {
        public SettingsViewModel(BatchType type, BatchSettings settings)
        {
            Type = type;
            Periods = new ReadPeriodsViewModel(type, settings);
            BankDetails = new ReadBankDetailsViewModel(type, settings);
            ExactConfig = new ReadExactConfigViewModel(type, settings);
            AppConfig = new ReadAppConfigViewModel(type, settings);
        }

        public BatchType Type { get; set; }
        public ReadPeriodsViewModel Periods { get; set; }
        public ReadBankDetailsViewModel BankDetails { get; set; }
        public ReadExactConfigViewModel ExactConfig { get; set; }
        public ReadAppConfigViewModel AppConfig { get; set; }
    }
}
