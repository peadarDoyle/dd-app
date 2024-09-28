using Autofac;
using Autofac.Integration.Mvc;
using DirectDebits.Controllers.ModelBinding;
using DirectDebits.Dependencies;
using DirectDebits.Models.Context;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using DirectDebits.ViewModels.Batches;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Serilog;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DirectDebits
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterModule<AutofacWebTypesModule>();

            builder.Register(c => SynergyDbContext.Create())
                   .As<ISynergyDbContext>()
                   .InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(IBatchRepository).Assembly)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces();

            builder.RegisterType<ApplicationUserManager>()
                   .AsSelf()
                   .InstancePerRequest();

            builder.RegisterType<ApplicationSignInManager>()
                   .AsSelf()
                   .InstancePerRequest();

            builder.Register(c => new UserStore<ApplicationUser>(c.Resolve<ISynergyDbContext>() as SynergyDbContext))
                   .As<IUserStore<ApplicationUser>>()
                   .InstancePerRequest();

            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication)
                   .As<IAuthenticationManager>();

            builder.Register(c => new IdentityFactoryOptions<ApplicationUserManager>
            {
                DataProtectionProvider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("Application​")
            });

            builder.Register(c => LoggerFactory.Create())
                   .InstancePerLifetimeScope()
                   .As<ILogger>();

            IContainer container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // for binding IPrincipal so controller can unbind the current httpcontext user so it can be mocked for testing
            ModelBinders.Binders.Add(typeof(IPrincipal), new IPrincipalModelBinder());

            // for binding the CreateBatchViewModel class which requires a custom model binder to handle the Customers property
            ModelBinders.Binders.Add(typeof(CreateBatchViewModel), new CreateBatchModelBinder());

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
    }
}
