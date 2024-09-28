using DirectDebits.OwinAuthentication.Provider;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using System;
using System.Globalization;
using System.Net.Http;

namespace DirectDebits.OwinAuthentication
{

	public class ExactOnlineAuthenticationMiddleware : AuthenticationMiddleware<ExactOnlineAuthenticationOptions>
	{
		private readonly ILogger _logger;
		private readonly HttpClient _client;

		public ExactOnlineAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, ExactOnlineAuthenticationOptions options) : base(next, options)
		{
            if (string.IsNullOrWhiteSpace(Options.ClientId))
            {
                throw new ArgumentException("The 'ClientId' option must be provided.");
            }

            if (string.IsNullOrWhiteSpace(Options.ClientSecret))
            {
                throw new ArgumentException("The 'ClientSecret' option must be provided.");
            }

			_logger = app.CreateLogger<ExactOnlineAuthenticationMiddleware>();

			if (Options.Provider == null)
			{
				Options.Provider = new ExactOnlineAuthenticationProvider();
			}
			if (Options.StateDataFormat == null)
			{
				IDataProtector dataProtector = app.CreateDataProtector(typeof(ExactOnlineAuthenticationMiddleware).FullName, Options.AuthenticationType, "v1");
				Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
			}
			if (String.IsNullOrEmpty(Options.SignInAsAuthenticationType))
			{
				Options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
			}

			_client = new HttpClient(ResolveHttpMessageHandler(Options))
			{
				Timeout = Options.BackchannelTimeout,
				MaxResponseContentBufferSize = 1024 * 1024 //1 Mb)
			};
		}

		protected override AuthenticationHandler<ExactOnlineAuthenticationOptions> CreateHandler()
		{
			return new ExactOnlineAuthenticationHandler(_client,  _logger);
		}

		private static HttpMessageHandler ResolveHttpMessageHandler(ExactOnlineAuthenticationOptions options)
		{
			return new WebRequestHandler();
		}

	}

}
