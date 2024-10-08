﻿using System;
using System.Net;
using System.Text;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using System.Web.Configuration;
using System.Collections.Generic;
using DirectDebits.Common.Utility;

namespace DirectDebits.ExactClient.Services
{
    public class ExactToken
    {
        /// <summary>
        /// The point in time which the access token will expire, this value is not trustworthy
        /// and any token refresh should be done well before this point in time.
        /// </summary>
        private DateTime _expiresOn;
        private string _accessToken;
        private string _refreshToken;

        /// <summary>
        /// It is expected that this constructor is to be called following OWIN authentication.
        /// </summary>
        /// <param name="accessToken">The access token generated by Exact Online.</param>
        /// <param name="refreshToken">The refresh token to be used to retrieve a new access token.</param>
        /// <param name="expiresIn">How long the access token is valid (in seconds).</param>
        public ExactToken(string accessToken, string refreshToken, string expiresIn)
        {
            _accessToken = EncryptToken(accessToken);
            _refreshToken = EncryptToken(refreshToken);
            long expiresInSeconds = long.Parse(expiresIn);
            _expiresOn = DateTime.Now.AddSeconds(expiresInSeconds);
        }

        private static string EncryptToken(string plainTextToken)
        {
            string passPhrase = WebConfigurationManager.AppSettings["TokenPassPhrase"];
            string encryptedToken = StringCipher.Encrypt(plainTextToken, passPhrase);
            return encryptedToken;
        }

        private static string DecryptToken(string encryptedToken)
        {
            var passPhrase = WebConfigurationManager.AppSettings["TokenPassPhrase"];
            string decryptedToken = StringCipher.Decrypt(encryptedToken, passPhrase);
            return decryptedToken;
        }

        /// <summary>
        /// Updates accessToken, refreshToken, and expiresOn.
        /// </summary>
        /// <param name="serializedToken">The serialized token is the JSON response from the /api/oath2/token endpoint.</param>
        private void Update(string serializedToken)
        {
            var format = new { access_token = "", token_type = "", expires_in = "", refresh_token = "" };
            var token = JsonConvert.DeserializeAnonymousType(serializedToken, format);

            _accessToken = EncryptToken(token.access_token);
            _refreshToken = EncryptToken(token.refresh_token);
            long expiresInSeconds = long.Parse(token.expires_in);
            _expiresOn = DateTime.Now.AddSeconds(expiresInSeconds);
        }

        public string GetAccessToken()
        {
            if (ShouldRefresh())
            {
                Refresh();
            }

            return DecryptToken(_accessToken);
        }

        /// <summary>
        /// Checks if the access token needs to be refreshed.
        /// </summary>
        /// <returns>Returns true if the token needs to be refreshed.</returns>
        private bool ShouldRefresh()
        {
            // We give 30 seconds to account for time between when the token was issued by the OAuth
            // server and when the expiration time was registerd by the OAuth consumer. And also to
            // provide grace time to ensure the token doesn't expire between the point where the
            // consumer checks the token is still valid and the server checks the token is still valid. 
            return DateTime.Now > _expiresOn.AddSeconds(-30);
        }

        /// <summary>
        /// Gets a new access token and refresh token.
        /// </summary>
        private void Refresh()
        {
            string clientId = WebConfigurationManager.AppSettings["ClientId"];
            string clientSecret = WebConfigurationManager.AppSettings["ClientSecret"];
            string tokenEndpoint = WebConfigurationManager.AppSettings["TokenEndpoint"];

            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", DecryptToken(_refreshToken))
            };

            string content = Post(tokenEndpoint, parameters);

            Update(content);
        }

        private static string Post(string url, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var uri = new Uri(url);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "POST";
            request.KeepAlive = true;
            request.ContentType = "application/x-www-form-urlencoded";

            string postParameters = GetPostParameters(parameters);

            var bytes = Encoding.UTF8.GetBytes(postParameters);

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }

            using (WebResponse response = request.GetResponse())
            using (Stream rs = response.GetResponseStream())
            using (var sr = new StreamReader(rs))
            {
                string jsonResponse = sr.ReadToEnd();
                return jsonResponse;
            }
        }
        private static string GetPostParameters(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            string postParameters = string.Empty;

            foreach (var parameter in parameters)
            {
                postParameters += $"&{parameter.Key}={HttpUtility.HtmlEncode(parameter.Value)}";
            }

            return postParameters.Substring(1);
        }

        private string GetQueryStringParameters(IEnumerable<KeyValuePair<string, string>> parameters = null)
        {
            string queryStringParameters = string.Empty;

            foreach (var parameter in parameters)
            {
                queryStringParameters += $"&{parameter.Key}={HttpUtility.HtmlEncode(parameter.Value)}";
            }

            return queryStringParameters.Substring(1);
        }
    }
}