using System.ComponentModel;
using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Settings
{
    public class ReadPeriodsViewModel
    {
        public ReadPeriodsViewModel(BatchType type, BatchSettings settings)
        {
            BatchType = type;
            Period1 = settings.Period1;
            Period2 = settings.Period2;
            Period3 = settings.Period3;
        }

        public BatchType BatchType { get; set; }

        [DisplayName("Period 1")]
        public int Period1 { get; set; }

        [DisplayName("Period 2")]
        public int Period2 { get; set; }

        [DisplayName("Period 3")]
        public int Period3 { get; set; }
    }
}
