using DirectDebits.OwinAuthentication.Provider;
using System.Web.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DirectDebits.OwinAuthentication
{

	public class ExactOnlineAuthenticationHandler : AuthenticationHandler<ExactOnlineAuthenticationOptions>
	{
		private readonly ILogger _logger;
		private readonly HttpClient _client;

		public ExactOnlineAuthenticationHandler(HttpClient client, ILogger logger)
		{
			_client = client;
			_logger = logger;
		}

		protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
		{
			AuthenticationProperties properties = null;
			try
			{
				string code = null;
				string state = null;


				var query = Request.Query;
				code = GetValueFromQuery(query, "code");
				state = GetValueFromQuery(query, "state");

				properties = Options.StateDataFormat.Unprotect(state);
				if (properties == null) return null;

				// OAuth2 10.12 CSRF
				if (!ValidateCorrelationId(properties, _logger)) return new AuthenticationTicket(null, properties);

				string requestPrefix = Request.Scheme + "://" + Request.Host;
				string redirectUri = requestPrefix + Request.PathBase + Options.CallbackPath;

				// Build up the body for the token request
				var body = new Dictionary<string, string>()
				{
					{"grant_type", "authorization_code"},
					{"code", code},
					{"redirect_uri", redirectUri},
					{"client_id", Options.ClientId},
					{"client_secret", Options.ClientSecret}
				};

				// Request the token
                var tokenEndpoint = WebConfigurationManager.AppSettings["TokenEndpoint"];
				var tokenResponse = await _client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(body));
				tokenResponse.EnsureSuccessStatusCode();
				var text = await tokenResponse.Content.ReadAsStringAsync();

				// Deserializes the token response
				var response = JObject.Parse(text);
				var accessToken = response.Value<string>("access_token");
				var expires = response.Value<string>("expires_in");
				var refreshToken = response.Value<string>("refresh_token");

				// Get the Exact Online user
                var userInfoEndpoint = WebConfigurationManager.AppSettings["UserInfoEndpoint"];
				var request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
				request.Headers.Add("Accept", "application/json");
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				var graphResponse = await _client.SendAsync(request, Request.CallCancelled);
				graphResponse.EnsureSuccessStatusCode();
				text = await graphResponse.Content.ReadAsStringAsync();
				var user = JObject.Parse(text);

				if (string.IsNullOrWhiteSpace(accessToken))
				{
					_logger.WriteWarning("Access token was not found");
					return new AuthenticationTicket(null, properties);
				}

				var context = new ExactOnlineAuthenticatedContext(Context, user, accessToken, refreshToken, expires);
				context.Identity = new ClaimsIdentity(Options.AuthenticationType, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
				if (!string.IsNullOrEmpty(context.Id))
				{
					context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.Id, ClaimValueTypes.String, Options.AuthenticationType));
				}
				if (!string.IsNullOrEmpty(context.UserName))
				{
					context.Identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName, ClaimValueTypes.String, Options.AuthenticationType));
				}
				if (!string.IsNullOrEmpty(context.FullName))
				{
					context.Identity.AddClaim(new Claim("urn:exactonline:fullname", context.FullName, ClaimValueTypes.String, Options.AuthenticationType));
				}
				context.Properties = properties;

				await Options.Provider.Authenticated(context);
				return new AuthenticationTicket(context.Identity, context.Properties);
			}
			catch (Exception ex)
			{
				_logger.WriteError("Authentication failed", ex);
				return new AuthenticationTicket(null, properties);
			}
		}

		protected override Task ApplyResponseChallengeAsync()
		{
            if (Response.StatusCode != 401)
            {
                return Task.FromResult<object>(null);
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);
            if (challenge == null) return Task.FromResult<object>(null);

            var baseUri = String.Concat(Request.Scheme, Uri.SchemeDelimiter, Request.Host, Request.PathBase);
			var currentUri = String.Concat(baseUri, Request.Path, Request.QueryString);
			var redirectUri = String.Concat(baseUri, Options.CallbackPath);

			var properties = challenge.Properties;
			if (string.IsNullOrEmpty(properties.RedirectUri))
			{
				properties.RedirectUri = currentUri;
			}

			// OAuth2 10.12 CSRF
			GenerateCorrelationId(properties);

			// comma separated
			var state = Options.StateDataFormat.Protect(properties);
            var authorizeEndpoint = WebConfigurationManager.AppSettings["AuthorizeEndpoint"];
			var authorizationEndpoint = String.Concat(authorizeEndpoint, "?response_type=code"
				,"&client_id=" + Uri.EscapeDataString(Options.ClientId)
				,"&redirect_uri=" + Uri.EscapeDataString(redirectUri)
				,"&state=" + Uri.EscapeDataString(state));
			if (Options.ForceLogin)
			{
				authorizationEndpoint = String.Concat(authorizationEndpoint, "&force_login=1");
			}

			Response.Redirect(authorizationEndpoint);

			return Task.FromResult<object>(null);
		}

		public override async Task<bool> InvokeAsync()
		{
			return await InvokeReplyPathAsync();
		}

		private async Task<bool> InvokeReplyPathAsync()
		{
			if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
			{
				var ticket = await AuthenticateAsync();
				if (ticket == null)
				{
					_logger.WriteWarning("Invalid return state, unable to redirect.");
					Response.StatusCode = 500;
					return true;
				}

				var context = new ExactOnlineReturnEndpointContext(Context, ticket)
				{
					RedirectUri = ticket.Properties.RedirectUri,
					SignInAsAuthenticationType = Options.SignInAsAuthenticationType,
				};

				await Options.Provider.ReturnEndpoint(context);

				if (context.SignInAsAuthenticationType != null && context.Identity != null)
				{
					var grantIdentity = context.Identity;
					if (!string.Equals(grantIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
					{
						grantIdentity = new ClaimsIdentity(grantIdentity.Claims, context.SignInAsAuthenticationType, grantIdentity.NameClaimType, grantIdentity.RoleClaimType);
					}
					Context.Authentication.SignIn(context.Properties, grantIdentity);
				}

				if (context.IsRequestCompleted || context.RedirectUri == null) return context.IsRequestCompleted;
				var redirectUri = context.RedirectUri;
				if (context.Identity == null)
				{
					// add a redirect hint that sign-in failed in some way
					redirectUri = WebUtilities.AddQueryString(redirectUri, "error", "access_denied");
				}
				Response.Redirect(redirectUri);
				context.RequestCompleted();

				return context.IsRequestCompleted;
			}
			return false;
		}

		private string GetValueFromQuery(IReadableStringCollection query, string key)
		{
			var values = query.GetValues(key);
			if (values != null && values.Count == 1)
			{
				return values[0];
			}
			return null;
		}

	}

}
