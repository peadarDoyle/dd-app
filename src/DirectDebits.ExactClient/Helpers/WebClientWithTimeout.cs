using System;
using System.Net;

namespace DirectDebits.ExactClient.Helpers
{
    internal class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 900 * 1000; // 15 minutes in milliseconds
            return w;
        }
    }
}
