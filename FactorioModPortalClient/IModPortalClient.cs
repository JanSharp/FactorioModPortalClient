using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace FactorioModPortalClient
{
    public interface IModPortalClient
    {
        int MaxRequestsPerMinute { get; set; }

        Task<byte[]> DownloadModAsByteArrayAsync(Release release);
        Task<Stream> DownloadModAsStreamAsync(Release release);
        Task<ZipArchive> DownloadModAsZipAsync(Release release);
        IAsyncEnumerable<ResultEntry> EnumerateAsync(IEnumerable<string>? namelist = null);
        Task<ModListResponse> GetAsync(int? page = null, int? pageSize = null, IEnumerable<string>? namelist = null);
        Task<ModListResponse> GetMaxAsync(IEnumerable<string>? namelist = null);
        Task<ResultEntryFull> GetResultEntryFullAsync(string modName);
        Task<ResultEntryShort> GetResultEntryShortAsync(string modName);
    }
}
