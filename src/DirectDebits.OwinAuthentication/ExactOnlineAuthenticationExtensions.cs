using System;
using Owin;

namespace DirectDebits.OwinAuthentication
{

	public static class ExactOnlineAuthenticationExtensions
	{

		public static IAppBuilder UseExactOnlineAuthentication(this IAppBuilder app, ExactOnlineAuthenticationOptions options)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (options == null) throw new ArgumentNullException("options");

			return app.Use<ExactOnlineAuthenticationMiddleware>(app, options);
		}

		public static IAppBuilder UseExactOnlineAuthentication(this IAppBuilder app, string clientId, string clientSecret)
		{
			return app.UseExactOnlineAuthentication(new ExactOnlineAuthenticationOptions()
			{
				ClientId = clientId,
				ClientSecret = clientSecret
			});
		}

	}

}
