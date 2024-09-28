using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;

namespace DirectDebits.OwinAuthentication.Provider
{

	public class ExactOnlineReturnEndpointContext : ReturnEndpointContext
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context">OWIN environment</param>
		/// <param name="ticket">The authentication ticket</param>
		public ExactOnlineReturnEndpointContext(
			IOwinContext context,
			AuthenticationTicket ticket)
			: base(context, ticket)
		{ }

	}

}
