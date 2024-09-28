using System.Web;
using DirectDebits.Persistence.Contracts;

namespace DirectDebits.ExactClient.Services
{
    public class MyDivision
    {
        private readonly IOrganisationRepository _organisationStorage;

        public MyDivision(IOrganisationRepository organisationStorage)
        {
            _organisationStorage = organisationStorage;
        }

        public int? GetDivision()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var division = HttpContext.Current.Session["Division"] as int?;

            if (division.HasValue)
            {
                return division;
            }

            string username = HttpContext.Current.User.Identity.Name;
            division = _organisationStorage.GetByUserName(username).ExternalAccessCode;
            HttpContext.Current.Session["Division"] = division;

            return division;
        }
    }
}