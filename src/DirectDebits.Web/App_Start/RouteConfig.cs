using System.Web.Mvc;
using System.Web.Routing;

namespace DirectDebits
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.LowercaseUrls = true;

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "ValidateToken",
                url: "user/refreshexternallogin",
                defaults: new { controller = "User", action = "RefreshExternalLogin" }
            );

            // default routing
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
