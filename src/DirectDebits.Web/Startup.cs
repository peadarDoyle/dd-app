using DirectDebits.Models.Entities;
using DirectDebits.OwinAuthentication;
using DirectDebits.OwinAuthentication.Provider;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Configuration;

[assembly: OwinStartupAttribute(typeof(DirectDebits.Startup))]
namespace DirectDebits
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/user/login"),
                ExpireTimeSpan = TimeSpan.FromHours(12),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        TimeSpan.FromMinutes(30),
                        (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseExactOnlineAuthentication(new ExactOnlineAuthenticationOptions
            {
                ClientId = WebConfigurationManager.AppSettings["ClientId"],
                ClientSecret = WebConfigurationManager.AppSettings["ClientSecret"],
                Provider = new ExactOnlineAuthenticationProvider()
                {
                    OnAuthenticated = context =>
                        {
                            context.Identity.AddClaim(new Claim("urn:tokens:exactonline:accesstoken", context.AccessToken));
                            context.Identity.AddClaim(new Claim("urn:tokens:exactonline:refreshtoken", context.RefreshToken));
                            context.Identity.AddClaim(new Claim("urn:tokens:exactonline:expiresin", context.ExpiresIn.Value.TotalSeconds.ToString()));

                            return Task.FromResult(true);
                        }
                }
            });
        }
    }
}
