using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using DirectDebits.Attributes.Security;
using DirectDebits.ViewModels.User;
using DirectDebits.ExactClient.Services;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using ExactOnline.Client.Models.Current;
using ExactOnline.Client.Models.HRM;
using Serilog;

namespace DirectDebits.Controllers
{
    public class UserController : BaseController
    {
        protected IOrganisationRepository OrganisationStorage;
        protected ApplicationSignInManager SignInManager;

        public UserController(
            ILogger logger,
            IOrganisationRepository organisationStorage,
            ApplicationSignInManager signInManager,
            ApplicationUserManager userManager)
            :base(logger)
        {
            SignInManager = signInManager;
            UserManager = userManager;
            OrganisationStorage = organisationStorage;
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // stops issues with AuthenticationManager.GetExternalLoginInfoAsync() from occuring
            // see http://stackoverflow.com/questions/20180562/mvc5-null-reference-with-facebook-login/20948631#20948631
            HttpContext.Session.RemoveAll();

            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "User", new { returnUrl = returnUrl }));
        }

        [HttpGet]
        [AllowCrossSiteJson]
        public ActionResult RefreshExternalLogin(string provider, string returnUrl)
        {
            if(!TempData.ContainsKey("IsRedirect"))
            {
                return new HttpNotFoundResult();
            }
            else
            {
                TempData.Remove("IsRedirect");
            }

            // stops issues with AuthenticationManager.GetExternalLoginInfoAsync() from occuring
            // see http://stackoverflow.com/questions/20180562/mvc5-null-reference-with-facebook-login/20948631#20948631
            HttpContext.Session.RemoveAll();

            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "User", new { ReturnUrl = returnUrl }));
        }

        [HttpGet]
        [AllowCrossSiteJson]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);

            var exactToken = new ExactToken(
                loginInfo.ExternalIdentity.Claims.Single(c => c.Type == "urn:tokens:exactonline:accesstoken").Value,
                loginInfo.ExternalIdentity.Claims.Single(c => c.Type == "urn:tokens:exactonline:refreshtoken").Value,
                loginInfo.ExternalIdentity.Claims.Single(c => c.Type == "urn:tokens:exactonline:expiresin").Value
            );
            HttpContext.Session["ExactToken"] = exactToken;

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("VerificationRequired");
                // If the user does not have an account, then prompt the user to create an account
                case SignInStatus.Failure:
                    GetRegisterViewModel model = await CreateGetRegisterViewModel();
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", model);
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected {nameof(SignInStatus)} encountered [{result}]");
            }
        }

        // todo: it would be good to put this into some kind of user service
        private async Task<GetRegisterViewModel> CreateGetRegisterViewModel()
        {
            var subscriptionService = new ExactSubscriptionService(Logger, null);
            Me me = subscriptionService.GetMe();
            var divisions = await subscriptionService.GetAllDivisionsAsync();
            bool orgExists = OrganisationStorage.Exists(me.DivisionCustomer.Value);

            var model = new GetRegisterViewModel
            {
                CompanyName = me.DivisionCustomerName,
                Username = me.UserName
            };

            if (orgExists)
            {
                Organisation organisation = OrganisationStorage.Get(me.DivisionCustomer.Value);
                Division existingDivision = divisions.Single(x => x.Code == organisation.ExternalAccessCode);
                model.ExistingDivisionName = existingDivision.Description;
            }
            else
            {
                model.DivisionSelection = divisions.Select(x => new SelectListItem
                {
                    Text = x.Description,
                    Value = x.Code.ToString()
                }).ToList();
            }

            return model;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(PostRegisterViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {

                ExternalLoginInfo info = await AuthenticationManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var subscriptionService = new ExactSubscriptionService(Logger, null);
                Me me = subscriptionService.GetMe();

                Organisation organisation;

                // create an account for the ExactOnline division
                if (model.DivisionCode.HasValue)
                {
                    Division division = subscriptionService.GetDivisionFromCode(model.DivisionCode.Value);
                    organisation = new Organisation(division.Customer.Value, division.Code, division.CustomerName);
                    OrganisationStorage.Create(organisation);
                }
                else
                {
                    organisation = OrganisationStorage.Get(me.DivisionCustomer.Value);
                }

                var user = new ApplicationUser { UserName = me.UserName, Email = me.Email, Organisation = organisation, LockoutEnabled = true, LockoutEndDateUtc = DateTime.MaxValue };
                IdentityResult result = await UserManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);

                    if (result.Succeeded)
                    {
                        await SignInManager.ExternalSignInAsync(info, false);
                        return View("VerificationRequired");
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("ExternalLoginFailure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult LogOff()
        {
            Session.Abandon();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (UserManager != null)
                {
                    UserManager.Dispose();
                    UserManager = null;
                }

                if (SignInManager != null)
                {
                    SignInManager.Dispose();
                    SignInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}