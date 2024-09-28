using System.ComponentModel;
using System.Collections.Generic;
using System.Web.Mvc;
using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.ViewModels.Settings
{
    public class ReadExactConfigViewModel
    {
        public ReadExactConfigViewModel(BatchType type, BatchSettings settings)
        {
            BatchType = type;
            ClassificationFilterId = settings.ClassificationFilterId?.ToString() ?? "None";
            BankJournalCode = settings.BankJournalCode;
            TradeJournalCode = settings.TradeJournalCode;
            BankGlCode = settings.BankGlCode;
            TradeGlCode = settings.TradeGlCode;
        }

        public BatchType BatchType { get; set; }

        [DisplayName("Classification Filter ID")]
        public string ClassificationFilterId { get; set; }

        [DisplayName("Bank Journal Code")]
        public string BankJournalCode { get; set; }

        [DisplayName("Trade Journal Code")]
        public string TradeJournalCode { get; set; }

        [DisplayName("Bank G/L Code")]
        public string BankGlCode { get; set; }

        [DisplayName("Trade G/L Code")]
        public string TradeGlCode { get; set; }

        public IList<SelectListItem> ClassificationSelector => new List<SelectListItem>
        {
            new SelectListItem { Text = "None", Value = "" },
            new SelectListItem { Text = "1", Value = "1" },
            new SelectListItem { Text = "2", Value = "2" },
            new SelectListItem { Text = "3", Value = "3" },
            new SelectListItem { Text = "4", Value = "4" },
            new SelectListItem { Text = "5", Value = "5" },
            new SelectListItem { Text = "6", Value = "6" },
            new SelectListItem { Text = "7", Value = "7" },
            new SelectListItem { Text = "8", Value = "8" }
        };
    }
}
