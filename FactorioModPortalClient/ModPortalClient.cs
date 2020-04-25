using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
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

        public ModPortalClient(string userName, string userToken)
        {
            httpClient = new HttpClient();
            this.userName = userName;
            this.userToken = userToken;
        }

        string BuildUrl(string main, Dictionary<string, string> parameters = null)
        {
            UriBuilder uriBuilder = new UriBuilder(main) { Port = -1 };
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            if (parameters != null)
                foreach (KeyValuePair<string, string> param in parameters)
                    query[param.Key] = param.Value;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        async Task<HttpResponseMessage> GetResponseAsync(string url)
        {
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

        public async Task<ModListResponse> GetAsync(int? page = null, int? pageSize = null, IEnumerable<string> namelist = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (page.HasValue)
                parameters.Add("page", page.ToString());
            if (pageSize.HasValue)
                parameters.Add("page_size", pageSize.ToString());
            if (namelist != null)
                throw new NotImplementedException(); // TODO
            return await GetTInternalAsync<ModListResponse>(BuildUrl($"{BaseUrl}/api/mods", parameters));
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

        public async IAsyncEnumerable<ResultEntry> EnumerateAsync(int pageSize = 64, IEnumerable<string> namelist = null)
        {
            ModListResponse currentPage = await GetAsync(pageSize: pageSize, namelist: namelist);

            while (true)
            {
                Task<ModListResponse> nextPage;
                if (currentPage.Pagination.Links.Next == null)
                    nextPage = null;
                else
                    nextPage = GetTInternalAsync<ModListResponse>(currentPage.Pagination.Links.Next);

                foreach (ResultEntry entry in currentPage.Results)
                    yield return entry;

                if (nextPage == null)
                    break;
                currentPage = await nextPage;
            }
        }

        public async IAsyncEnumerable<ResultEntryFull> EnumerateFullAsync(int pageSize = 64, IEnumerable<string> namelist = null)
        {
            yield return await GetResultEntryFullAsync("");
        }
    }
}
