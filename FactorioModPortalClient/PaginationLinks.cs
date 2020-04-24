using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    public class PaginationLinks
    {
        /// <summary>
        /// URL to the first page of the results, or null if you're already on the first page.
        /// </summary>
        [JsonPropertyName("first")]
        public string First { get; set; }

        /// <summary>
        /// URL to the previous page of the results, or null if you're already on the first page.
        /// </summary>
        [JsonPropertyName("prev")]
        public string Prev { get; set; }

        /// <summary>
        /// URL to the next page of the results, or null if you're already on the last page.
        /// </summary>
        [JsonPropertyName("next")]
        public string Next { get; set; }

        /// <summary>
        /// URL to the last page of the results, or null if you're already on the last page.
        /// </summary>
        [JsonPropertyName("last")]
        public string Last { get; set; }
    }
}
