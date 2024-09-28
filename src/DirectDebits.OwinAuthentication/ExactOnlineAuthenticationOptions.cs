using DirectDebits.OwinAuthentication.Provider;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;

namespace DirectDebits.OwinAuthentication
{

	public class ExactOnlineAuthenticationOptions : AuthenticationOptions
	{

		public ExactOnlineAuthenticationOptions()
			: base(Constants.DefaultAuthenticationType)
		{
			Caption = Constants.DefaultAuthenticationCaption;
			ForceLogin = false;
			CallbackPath = new PathString("/signin-exactonline");
			AuthenticationMode = AuthenticationMode.Passive;
			BackchannelTimeout = TimeSpan.FromSeconds(60);
		}

		public string Caption
		{
			get { return Description.Caption; }
			set { Description.Caption = value; }
		}

		public string ClientId { get; set; }

		public string ClientSecret { get; set; }

		public bool ForceLogin { get; set; }

		public string SignInAsAuthenticationType { get; set; }

		public TimeSpan BackchannelTimeout { get; set; }

		public PathString CallbackPath { get; set; }

		public IExactOnlineAuthenticationProvider Provider { get; set; }

		public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
		
	}

}
