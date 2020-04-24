using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    public class ModListResponse
    {
        [JsonPropertyName("pagination")]
        public Pagination Pagination { get; set; }

        /// <summary>
        /// A list of mods, matching any filters you specified.
        /// </summary>
        [JsonPropertyName("results")]
        public List<ResultEntry> Results { get; set; }
    }
}
