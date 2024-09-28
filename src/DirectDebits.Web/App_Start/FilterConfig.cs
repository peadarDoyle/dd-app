using System.Web.Mvc;

namespace DirectDebits
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // when the application is not in debug mode force everything to occur over https
            #if!DEBUG
            filters.Add(new RequireHttpsAttribute());
            #endif
        }
    }
}
