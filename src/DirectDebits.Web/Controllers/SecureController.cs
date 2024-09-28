using System;
using Serilog;
using System.Web.Mvc;
using DirectDebits.Attributes.ExactOnline;
using DirectDebits.Common;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;

namespace DirectDebits.Controllers
{
    [Authorize]
    [TokenValidator]
    public class SecureController : BaseController
    {
        protected IOrganisationRepository OrganisationRepository;

        public SecureController(ILogger logger, IOrganisationRepository organisationRepository) : base(logger)
        {
            OrganisationRepository = organisationRepository;
        }

        protected bool FeatureRestricted(ApplicationUser user, BatchType type)
        {
            return !CanAccessFeature(user, type);
        }

        protected bool CanAccessFeature(ApplicationUser user, BatchType type)
        {
            bool canAccessFeature = false;

            switch(type)
            {
                case BatchType.DirectDebit:
                    canAccessFeature = user.Organisation.HasDirectDebitsFeature;
                    break;
                case BatchType.Payment:
                    canAccessFeature = user.Organisation.HasPaymentsFeature;
                    break;
                default: throw new NotImplementedException();
            }

            Logger.Information("Attempted {@Feature} feature access {@CanAccessFeature}",
                type, canAccessFeature ? "succeeded" : "failed");

            return canAccessFeature;
        }
    }
}
