using System.Web.Mvc;

namespace DirectDebits.Attributes.ModelStatePersistence
{
    /// <summary>
    /// Base class for ModelState exporting and importing.
    /// </summary>
    public class ModelStateTempDataTransfer : ActionFilterAttribute
    {
        // the key for ModelState stored in the TempData dictionary
        protected static readonly string Key = typeof(ModelStateTempDataTransfer).FullName;
    }
}