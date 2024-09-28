using System.Web.Routing;
using System.Web.Mvc;
using ExactOnline.Client.Sdk.Exceptions;
using DirectDebits.ExactClient.Services;

namespace DirectDebits.Attributes.ExactOnline
{
    public class TokenValidator : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var exactToken = filterContext.HttpContext.Session["ExactToken"] as ExactToken;

            if (exactToken == null)
            {
                filterContext.Result = ValidateToken(filterContext);
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception is UnauthorizedException)
            {
                filterContext.Result = ValidateToken(filterContext);
            }

            base.OnActionExecuted(filterContext);
        }

        private ActionResult ValidateToken(ControllerContext filterContext)
        {
            filterContext.Controller.TempData["IsRedirect"] = true;

            var routeValues = new RouteValueDictionary
            {
                { "provider", "exactonline" },
                { "return", filterContext.HttpContext.Request.Path }
            };

            return new RedirectToRouteResult("ValidateToken", routeValues);
        }
    }
}