using System;
using System.Security.Principal;
using System.Web.Mvc;

namespace DirectDebits.Controllers.ModelBinding
{
    /// <summary>
    /// as per http://www.hanselman.com/blog/IPrincipalUserModelBinderInASPNETMVCForEasierTesting.aspx
    /// when an IPincipal is passed to an action method it will have the controllers httpcontext user property bound to it
    /// the binding is done in the global.asax application_start method
    /// </summary>
    public class IPrincipalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }
            IPrincipal p = controllerContext.HttpContext.User;
            return p;
        }
    }
}