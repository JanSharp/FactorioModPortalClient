using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace FactorioModPortalClient
{
    public class ModPortalClient
    {
        readonly HttpClient httpClient;
        const string BaseUrl = "https://mods.factorio.com";
        public string userName;
        public string userToken;
        public int maxRequestsPerMinute;
        int msBetweenRequests;
        DateTime lastRequestTime;
        int activeThrottleCount;

        public int MaxRequestsPerMinute
        {
            get => maxRequestsPerMinute;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxRequestsPerMinute must be > 0.");
                maxRequestsPerMinute = value;
                msBetweenRequests = (1 * 1000 * 60) / value;
            }
        }

        public ModPortalClient(string userName, string userToken, int maxRequestsPerMinute = 10)
        {
            httpClient = new HttpClient();
            this.userName = userName;
            this.userToken = userToken;
            MaxRequestsPerMinute = maxRequestsPerMinute; // note this setting the value to the property not the field
            lastRequestTime = DateTime.Now.AddMilliseconds(-msBetweenRequests);
            activeThrottleCount = 0;
        }

        string BuildUrl(string main, Dictionary<string, string>? parameters = null)
        {
            UriBuilder uriBuilder = new UriBuilder(main) { Port = -1 };
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            if (parameters != null)
                foreach (KeyValuePair<string, string> param in parameters)
                    query[param.Key] = param.Value;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        async Task ThrottleRequest()
        {
            TimeSpan span = DateTime.Now - lastRequestTime;
            int msToWait = msBetweenRequests - (int)span.TotalMilliseconds;
            if (msToWait > 0)
            {
                await Task.Delay(msToWait + activeThrottleCount++ * msBetweenRequests);
                --activeThrottleCount;
            }
            lastRequestTime = DateTime.Now;
        }

        async Task<HttpResponseMessage> GetResponseAsync(string url)
        {
            await ThrottleRequest();
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new NotImplementedException(); // TODO
            return response;
        }

        async Task<T> GetTInternalAsync<T>(string url)
        {
            HttpResponseMessage response = await GetResponseAsync(url);
            // TODO: check if the result is an Error object
            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ResultEntryShort> GetResultEntryShortAsync(string modName)
        {
            return await GetTInternalAsync<ResultEntryShort>(BuildUrl($"{BaseUrl}/api/mods/{modName}"));
        }

        public async Task<ResultEntryFull> GetResultEntryFullAsync(string modName)
        {
            return await GetTInternalAsync<ResultEntryFull>(BuildUrl($"{BaseUrl}/api/mods/{modName}/full"));
        }

        async Task<ModListResponse> GetInternalAsync(string? page = null, string? pageSize = null, IEnumerable<string>? namelist = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (page != null)
                parameters.Add("page", page);
            if (pageSize != null)
                parameters.Add("page_size", pageSize);
            if (namelist != null)
                throw new NotImplementedException(); // TODO
            return await GetTInternalAsync<ModListResponse>(BuildUrl($"{BaseUrl}/api/mods", parameters));
        }

        public async Task<ModListResponse> GetAsync(int? page = null, int? pageSize = null, IEnumerable<string>? namelist = null)
        {
            return await GetInternalAsync(page?.ToString(), pageSize?.ToString(), namelist);
        }

        /// <summary>
        /// keep in mind that <see cref="ZipArchive"/> implements <see cref="IDisposable"/>
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
        public async Task<ZipArchive> DownloadModAsync(Release release)
        {
            HttpResponseMessage response = await GetResponseAsync(BuildUrl($"{BaseUrl}/{release.DownloadUrl}", new Dictionary<string, string>()
            {
                { "username", userName },
                { "token", userToken },
            }));
            Stream contentStream = await response.Content.ReadAsStreamAsync();

            // sha1 validation
            using SHA1 sha = SHA1.Create("SHA1CryptoServiceProvider"); // TODO: test if this even works (also, i hate it... that string)
            byte[] actualSha1Bytes = sha.ComputeHash(contentStream);
            string actualSha1Str = ByteArrayToHex(actualSha1Bytes);
            if (release.Sha1.ToLower() != actualSha1Str.ToLower())
                throw new Exception($"Sha1 validation failed."); // TODO: improve error handling, maybe try downloading again or

            try
            {
                return new ZipArchive(contentStream, ZipArchiveMode.Read, false, Encoding.UTF8);
            }
            catch (Exception e)
            {
                contentStream.Dispose();
                throw e;
            }
        }
        static string ByteArrayToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append($"{b:x2}");
            return sb.ToString();
        }

        /// <summary>
        /// Gets with max page size, so basically everything
        /// </summary>
        /// <param name="namelist"></param>
        /// <returns></returns>
        public async Task<ModListResponse> GetMaxAsync(IEnumerable<string>? namelist = null)
        {
            return await GetInternalAsync(pageSize: "max", namelist: namelist);
        }

        /// <summary>
        /// Helper method calling <see cref="GetMaxAsync(IEnumerable{string}?)"/> and enumerating <see cref="ModListResponse.Results"/>
        /// </summary>
        /// <param name="namelist"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<ResultEntry> EnumerateAsync(IEnumerable<string>? namelist = null)
        {
            ModListResponse page = await GetMaxAsync(namelist);
            foreach (ResultEntry entry in page.Results)
                yield return entry;
        }
    }
}
