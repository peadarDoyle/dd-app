using System.Web.Mvc;
using System.Threading.Tasks;
using DirectDebits.Models.Entities;
using Serilog;

namespace DirectDebits.Controllers
{
    public class BaseController : Controller
    {
        protected ApplicationUserManager UserManager;
        protected ILogger Logger;

        public BaseController(ILogger logger)
        {
            Logger = logger;
        }

        protected async Task<ApplicationUser> GetCurrentUser()
        {
            var user = await UserManager.FindByNameAsync(HttpContext.User.Identity.Name);

            Logger = Logger
                .ForContext("OrgId", user.Organisation.Id)
                .ForContext("OrgInfo", new
                {
                    user.Organisation.Name,
                    user.Organisation.ExternalId
                })
                .ForContext("UserId", user.Id)
                .ForContext("UserInfo", new
                {
                    user.UserName,
                    user.Email
                });

            return user;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Logger.Error(filterContext.Exception, "Unhandled exception encountered for path:{@Path}",
                filterContext.RequestContext.HttpContext.Request.Path);
        }
    }
}
