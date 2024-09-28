using System.Web.Mvc;

namespace DirectDebits.Attributes.ModelStatePersistence
{
    /// <summary>
    /// Exports the current model state into temp data. The resaon for this is so the model state
    /// can be retrieved after a redirect to another action method takes place.
    /// </summary>
    public class ExportModelStateToTempData : ModelStateTempDataTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //Only export when ModelState is not valid
            if (!filterContext.Controller.ViewData.ModelState.IsValid)
            {
                //Export if we are redirecting
                if ((filterContext.Result is RedirectResult) || (filterContext.Result is RedirectToRouteResult))
                {
                    filterContext.Controller.TempData[Key] = filterContext.Controller.ViewData.ModelState;
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}