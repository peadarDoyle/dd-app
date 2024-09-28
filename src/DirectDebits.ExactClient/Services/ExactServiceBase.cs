using System;
using System.Web.Configuration;
using ExactOnline.Client.Sdk.Controllers;
using ExactOnline.Client.Sdk.Delegates;
using DirectDebits.Common.Utility;
using System.Xml.Linq;
using DirectDebits.ExactClient.Helpers;
using System.Net;
using System.Web;
using System.Collections.Generic;
using ExactOnline.Client.Sdk.Helpers;
using Serilog;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DirectDebits.ExactClient.Services
{
    public abstract class ExactServiceBase
    {
        protected ILogger Logger;
        protected ExactOnlineClient Client;
        protected AccessTokenManagerDelegate AccessTokenManager;

        protected ExactServiceBase(ILogger logger, int? division)
        {
            Logger = logger;

            string baseUri = WebConfigurationManager.AppSettings["BaseUri"];

            var token = HttpContext.Current.Session["ExactToken"] as ExactToken;
            AccessTokenManager += token.GetAccessToken;

            if (division.HasValue)
                Client = new ExactOnlineClient(baseUri, division.Value, AccessTokenManager);
            else
                Client = new ExactOnlineClient(baseUri, AccessTokenManager);
        }

        protected Result<List<T>> RestApiGetMany<T>(string path, string filterParam, string selectParam)
        {
            string response;
            string baseUri = WebConfigurationManager.AppSettings["BaseUri"];
            string uri = $"{baseUri}/api/v1/{Client.GetDivision()}/{path}?$filter={filterParam}&$select={selectParam}";
            bool hasNext = true;
            var objects = new List<T>();

            while (hasNext)
            {
                using (var webclient = new WebClientWithTimeout())
                {
                    webclient.Headers[HttpRequestHeader.Accept] = "application/json";
                    webclient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    webclient.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessTokenManager.Invoke();

                    Logger.Information("Beginning rest API get many query: {@Query}", uri);

                    var sw = new Stopwatch();
                    sw.Start();
                    response = webclient.DownloadString(uri);
                    sw.Stop();
                    Logger.Information("Response after {@ElapsedMs}ms: {@Response}", sw.ElapsedMilliseconds, response);

                    var dayRemaining = webclient.ResponseHeaders["X-RateLimit-Remaining"];
                    var minuteRemaining = webclient.ResponseHeaders["X-RateLimit-Minutely-Remaining"];
                    Logger.Information("Remaining limits: daily={@XRateLimitRemaining} AND minutely={@XRateLimitMinutelyRemaining}", dayRemaining, minuteRemaining);

                    JObject json = JObject.Parse(response);
                    IList<JToken> results = json["d"]["results"].Children().ToList();

                    foreach (JToken result in results)
                    {
                        T obj = result.ToObject<T>();
                        objects.Add(obj);
                    }

                    var next = json["d"]["__next"];

                    if (next == null)
                    {
                        hasNext = false;
                    }
                    else
                    {
                        uri = next.ToString();
                    }
                }
            }

            Logger.Information("No 'next ' data to be collected");

            return Result.Ok(objects);
        }

        protected Result<XDocument> XmlUpload(string xml, string topic, string uploadName)
        {
            string baseUri = WebConfigurationManager.AppSettings["BaseUri"];
            string xmlUploadPath = WebConfigurationManager.AppSettings["XMLUploadPath"];

            string uri = $"{baseUri}{xmlUploadPath}?Topic={topic}&_Division_={Client.GetDivision()}";
            string response;

            using (var webclient = new WebClientWithTimeout())
            {
                webclient.Headers[HttpRequestHeader.ContentType] = "application/xml";
                webclient.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessTokenManager.Invoke();

                var sw = new Stopwatch();
                sw.Start();
                response = webclient.UploadString(uri, xml);
                sw.Stop();
                Logger.Information("XML upload duration {@ElapsedMs} for {@UploadName}", sw.ElapsedMilliseconds, uploadName);
            }

            var formatResult = ExactXmlHelper.ValidateFormat(response);

            if (!formatResult.IsSuccess)
            {
                return formatResult;
            }

            Result contentResult = ExactXmlHelper.ValidateContent(formatResult.Value);

            return contentResult.IsSuccess ? formatResult : Result.Fail<XDocument>(contentResult.Error);
        }

        protected IList<T> GetPaginatedRecords<T>(ExactOnlineQuery<T> exactQuery)
        {
            var records = new List<T>();
            var skipToken = string.Empty;

            int pageIndex = 0;

            do
            {
                pageIndex++;
                Logger.Information("Get page({PageIndex})...", pageIndex);
                var sw1 = new Stopwatch();
                sw1.Start();

                try
                {
                    List<T> transactionsPage = exactQuery.Get(ref skipToken);
                    sw1.Stop();
                    Logger.Information("Received page({@PageIndex}) after {@ElapsedMs}ms on first attempt", pageIndex, sw1.ElapsedMilliseconds);
                    records.AddRange(transactionsPage);
                }
                catch (Exception ex1)
                {
                    Logger.Error(ex1, "Could not read, retrying once more...", pageIndex, sw1.ElapsedMilliseconds);

                    try
                    {
                        var sw2 = new Stopwatch();
                        sw2.Start();
                        List<T> transactionsPage = exactQuery.Get(ref skipToken);
                        sw2.Stop();
                        Logger.Information("Received page({@PageIndex}) after {@ElapsedMs}ms on second attempt", pageIndex, sw2.ElapsedMilliseconds);
                        records.AddRange(transactionsPage);
                    }
                    catch (Exception ex2)
                    {
                        Logger.Error(ex2, "Second attempt also failed, no more retries", pageIndex, sw1.ElapsedMilliseconds);
                        throw;
                    }
                }

            }
            while (skipToken != "");

            Logger.Information("Pagination complete for {@Records} records", records.Count);
            return records;
        }

        protected IList<T> GetPaginatedRecords<T>(string fields, string query) where T : class
        {
            Logger.Information("Starting to paginate with no 'expand'");
            var exactQuery = Client.For<T>().Select(fields).Where(query);
            return GetPaginatedRecords<T>(exactQuery);
        }

        protected IList<T> GetPaginatedRecords<T>(string fields, string query, string expand) where T : class
        {
            Logger.Information("Starting to paginate with 'expand'");
            var exactQuery = Client.For<T>().Select(fields).Where(query).Expand(expand);
            return GetPaginatedRecords<T>(exactQuery);
        }
    }
}