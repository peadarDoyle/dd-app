using System.Web.Mvc;

namespace DirectDebits.Attributes.ModelStatePersistence
{
    /// <summary>
    /// Imports the model state from temp data. The model state being imported
    /// would be the model state from the previous action method which redirected
    /// to the current method.
    /// </summary>
    public class ImportModelStateFromTempData : ModelStateTempDataTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Controller.TempData[Key] is ModelStateDictionary modelState)
            {
                //Only Import if we are viewing
                if (filterContext.Result is ViewResult)
                {
                    filterContext.Controller.ViewData.ModelState.Merge(modelState);
                }
                else
                {
                    //Otherwise remove it.
                    filterContext.Controller.TempData.Remove(Key);
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}