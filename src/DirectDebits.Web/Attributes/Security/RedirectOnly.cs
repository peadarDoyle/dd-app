using System.Web.Mvc;

namespace DirectDebits.Attributes.Security
{
    public class RedirectOnly : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var is302 = filterContext.HttpContext.Response.StatusCode == 302;

            if(is302)
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {
                filterContext.Result = new HttpNotFoundResult();
            }
        }
    }
}