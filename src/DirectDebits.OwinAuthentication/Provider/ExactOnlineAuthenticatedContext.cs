using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Security.Claims;

namespace DirectDebits.OwinAuthentication
{

	public class ExactOnlineAuthenticatedContext : BaseContext
	{

		public ExactOnlineAuthenticatedContext(IOwinContext context, JObject user, string accessToken,  string refreshToken, string expires)
            : base(context)
        {
            User = user;
            AccessToken = accessToken;
            RefreshToken = refreshToken;

            int expiresValue;
            if (Int32.TryParse(expires, NumberStyles.Integer, CultureInfo.InvariantCulture, out expiresValue))
            {
                ExpiresIn = TimeSpan.FromSeconds(expiresValue);
            }

            Id = TryGetValue(user, "UserID");
			UserName = TryGetValue(user, "UserName");
			FullName = TryGetValue(user, "FullName");
        }

		/// <summary>
		/// Gets the Exact Online user ID
		/// </summary>
		public string Id { get; private set; }

		/// <summary>
		/// Gets the user's name
		/// </summary>
		public string UserName { get; private set; }

		/// <summary>
		/// Gets the user's given name
		/// </summary>
		public string FullName { get; private set; }

		/// <summary>
		/// Gets the Exact Online access token
		/// </summary>
		public string AccessToken { get; private set; }

		/// <summary>
		/// Gets the Exact Online refresh token
		/// </summary>
		/// <remarks>
		/// This value is not null only when access_type authorize parameter is offline.
		/// </remarks>
		public string RefreshToken { get; private set; }

		public JObject User { get; private set; }

		/// <summary>
		/// Gets the Exact Online access token expiration time
		/// </summary>
		public TimeSpan? ExpiresIn { get; set; }

		/// <summary>
		/// Gets the <see cref="ClaimsIdentity"/> representing the user
		/// </summary>
		public ClaimsIdentity Identity { get; set; }

		/// <summary>
		/// Gets or sets a property bag for common authentication properties
		/// </summary>
		public AuthenticationProperties Properties { get; set; }

		private static string TryGetValue(JObject user, string propertyName)
		{
			JToken value;
			var userResults = (JArray)user["d"]["results"];
			if (userResults.Count == 0) { return ""; };

			var userResult = (JObject)userResults[0];

			return userResult.TryGetValue(propertyName, out value) ? value.ToString() : null;
		}

	}

}
