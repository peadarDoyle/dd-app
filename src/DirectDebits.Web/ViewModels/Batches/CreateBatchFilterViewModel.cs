using ExactOnline.Client.Models.CRM;
using System.Collections.Generic;
using System.Linq;

namespace DirectDebits.ViewModels.Batches
{
    public class CreateBatchFilterViewModel
    {
        public CreateBatchFilterViewModel(string filterName, IList<AccountClassification> classifications)
        {
            Name = filterName;
            Values = classifications.ToDictionary(x =>  x.ID.ToString(), x => x.Description);
        }

        public string Name { get; set; }
        public IDictionary<string,string> Values { get; set; }
    }
}