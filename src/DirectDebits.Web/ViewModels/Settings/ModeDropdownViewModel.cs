using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Settings
{
    public class ModeDropdownViewModel
    {
        public bool HasDirectDebitsFeature { get; set; }
        public bool HasPaymentsFeature { get; set; }

        public ModeDropdownViewModel(Organisation org)
        {
            HasDirectDebitsFeature = org.HasDirectDebitsFeature;
            HasPaymentsFeature = org.HasPaymentsFeature;
        }
    }
}