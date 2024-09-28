using System.Collections.Generic;
using System.Web.Mvc;

namespace DirectDebits.ViewModels.User
{
    public class GetRegisterViewModel
    {
        public IEnumerable<SelectListItem> DivisionSelection { get; set; }
        public string CompanyName { get; set; }
        public string ExistingDivisionName { get; set; }
        public string Username { get; set; }
    }
}
