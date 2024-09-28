using System.Threading.Tasks;

namespace DirectDebits.OwinAuthentication.Provider
{

	public interface IExactOnlineAuthenticationProvider
	{

		/// <summary>
		/// Invoked whenever Exact Online succesfully authenticates a user
		/// </summary>
		/// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
		/// <returns>A <see cref="Task"/> representing the completed operation.</returns>
		Task Authenticated(ExactOnlineAuthenticatedContext context);

		/// <summary>
		/// Invoked prior to the <see cref="System.Security.Claims.ClaimsIdentity"/> being saved in a local cookie and the browser being redirected to the originally requested URL.
		/// </summary>
		/// <param name="context"></param>
		/// <returns>A <see cref="Task"/> representing the completed operation.</returns>
		Task ReturnEndpoint(ExactOnlineReturnEndpointContext context);

	}

}
