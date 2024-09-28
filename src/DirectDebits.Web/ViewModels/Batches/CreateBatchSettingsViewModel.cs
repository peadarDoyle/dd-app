using DirectDebits.Common;
using ExactOnline.Client.Models.CRM;
using System.Collections.Generic;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Batches
{
    public class CreateBatchSettingsViewModel
    {
        public CreateBatchSettingsViewModel(BatchType type, BatchSettings settings)
        {
            Type = type;
            SetPeriods(settings);
        }

        public CreateBatchSettingsViewModel(BatchType type, BatchSettings settings, string filterName, IList<AccountClassification> classifications)
        {
            Type = type;
            SetPeriods(settings);
            Filter = new CreateBatchFilterViewModel(filterName, classifications);
        }

        private void SetPeriods(BatchSettings settings)
        {
            Periods = new []
            {
                settings.Period1,
                settings.Period2,
                settings.Period3
            };
        }

        public BatchType Type { get; set; }
        public int[] Periods { get; set; }
        public CreateBatchFilterViewModel Filter { get; set; }
    }
}
