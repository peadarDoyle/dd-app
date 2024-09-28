using System.Web.Mvc;

namespace DirectDebits.Attributes.Security
{
    /// <summary>
    /// This action filter should be placed on actions that get external data
    /// and send it to the client via JSON. This needs to be done since this
    /// action is Cross Origing Resource Sharing (CORS) and for security reasons
    /// it must be explicitly allowed. Otherwise the browser thinks it may be
    /// a Cross Site Scriping (XSS) attack.
    /// 
    /// See: http://stackoverflow.com/questions/27218240/cors-in-asp-net-mvc5 which
    /// provides MVC 5 specific information about the approach taken here.
    /// </summary>
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "https://start.exactonline.co.uk");
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Methods", "GET");
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "accept, x-devtools-emulate-network-conditions-client-id");
            base.OnActionExecuting(filterContext);
        }
    }
}