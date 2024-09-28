using DirectDebits.Common;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Serilog;
using System.Diagnostics;

namespace DirectDebits.Controllers
{
    public class HomeController : SecureController
    {
        public HomeController(
            ILogger logger,
            IOrganisationRepository organisationRepository,
            ApplicationUserManager userManager)
            : base(logger, organisationRepository)
        {
            UserManager = userManager;
        }

        [Route("")]
        [HttpGet]
        public async Task<ActionResult> Index(int? page)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ApplicationUser user = await GetCurrentUser();

            var endpoint = ControllerContext.HttpContext.Request.RawUrl;
            var method = ControllerContext.HttpContext.Request.HttpMethod;

            Logger.Information("Begin processing request {Method} {Endpoint} for {Orginisation}/{User}, (page: {@Page})",
                method, endpoint, user.Organisation.Name, user.UserName, page);

            if (CanAccessFeature(user, BatchType.DirectDebit))
            {
                Logger.Information("Completed processing request {Method} {Endpoint} for {Orginisation}/{User}, (elapsed:{Elapsed}ms)}",
                    method, endpoint, user.Organisation.Name, user.UserName, stopwatch.ElapsedMilliseconds);

                return new RedirectResult("/batches/directdebit");
            }
            if (CanAccessFeature(user, BatchType.Payment))
            {
                Logger.Information("Completed processing request {Method} {Endpoint} for {Orginisation}/{User}, (elapsed:{Elapsed}ms)}",
                    method, endpoint, user.Organisation.Name, user.UserName, stopwatch.ElapsedMilliseconds);

                return new RedirectResult("/batches/payment");
            }
            else
            {
                Logger.Information("User has no access to any features, this scenario should only occur due to a misconfiguration of the feature flags");
                return View("NoFeaturesAvailable");
            }
        }
    }

}
