using System.ComponentModel;
using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Settings
{
    public class ReadAppConfigViewModel
    {
        public ReadAppConfigViewModel(BatchType type, BatchSettings settings)
        {
            BatchType = type;
            LowLevelConfig = settings.LowLevelConfig;
        }

        public BatchType BatchType { get; set; }

        [DisplayName("Low Level Setting")]
        public string LowLevelConfig { get; set; }
    }
}
