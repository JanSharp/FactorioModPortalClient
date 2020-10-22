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
    public class ModPortalClient : IModPortalClient
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

        public Task<ResultEntryShort> GetResultEntryShortAsync(string modName)
        {
            return GetTInternalAsync<ResultEntryShort>(BuildUrl($"{BaseUrl}/api/mods/{modName}"));
        }

        public Task<ResultEntryFull> GetResultEntryFullAsync(string modName)
        {
            return GetTInternalAsync<ResultEntryFull>(BuildUrl($"{BaseUrl}/api/mods/{modName}/full"));
        }

        Task<ModListResponse> GetInternalAsync(string? page = null, string? pageSize = null, IEnumerable<string>? namelist = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (page != null)
                parameters.Add("page", page);
            if (pageSize != null)
                parameters.Add("page_size", pageSize);
            if (namelist != null)
                throw new NotImplementedException(); // TODO
            return GetTInternalAsync<ModListResponse>(BuildUrl($"{BaseUrl}/api/mods", parameters));
        }

        public Task<ModListResponse> GetAsync(int? page = null, int? pageSize = null, IEnumerable<string>? namelist = null)
        {
            return GetInternalAsync(page?.ToString(), pageSize?.ToString(), namelist);
        }

        async Task<T> DownloadModInternalAsync<T>(Release release, Func<HttpContent, Task<T>> readContent, Func<SHA1, T, byte[]> computeSha1Hash)
        {
            HttpResponseMessage response = await GetResponseAsync(BuildUrl($"{BaseUrl}/{release.DownloadUrl}", new Dictionary<string, string>()
            {
                { "username", userName },
                { "token", userToken },
            }));
            T content = await readContent(response.Content);

            try
            {
                // sha1 validation
                using SHA1 sha = SHA1.Create(); // considering this uses the default implementation, i'm not sure if it will work in every environment
                byte[] actualSha1Bytes = computeSha1Hash(sha, content);
                string actualSha1Str = ByteArrayToHex(actualSha1Bytes);
                if (release.Sha1.ToLower() != actualSha1Str.ToLower())
                    throw new Exception($"Sha1 validation failed."); // TODO: improve error handling, maybe try downloading again or
            }
            catch
            {
                if (content is IDisposable disposable)
                    disposable.Dispose();
                throw;
            }

            return content;
        }

        public Task<byte[]> DownloadModAsByteArrayAsync(Release release)
        {
            return DownloadModInternalAsync(release, async content => await content.ReadAsByteArrayAsync(), (sha, bytes) => sha.ComputeHash(bytes));
        }

        /// <summary>
        /// keep in mind that <see cref="Stream"/> implements <see cref="IDisposable"/>
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
        public Task<Stream> DownloadModAsStreamAsync(Release release)
        {
            return DownloadModInternalAsync(release, async content => await content.ReadAsStreamAsync(), (sha, stream) => sha.ComputeHash(stream));
        }

        /// <summary>
        /// keep in mind that <see cref="ZipArchive"/> implements <see cref="IDisposable"/>
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
        public async Task<ZipArchive> DownloadModAsZipAsync(Release release)
        {
            Stream stream = await DownloadModAsStreamAsync(release);
            try
            {
                return new ZipArchive(stream, ZipArchiveMode.Read, false, Encoding.UTF8);
            }
            catch (Exception e)
            {
                stream.Dispose();
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
        public Task<ModListResponse> GetMaxAsync(IEnumerable<string>? namelist = null)
        {
            return GetInternalAsync(pageSize: "max", namelist: namelist);
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
