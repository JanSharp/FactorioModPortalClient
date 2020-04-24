using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    public class Pagination
    {
        /// <summary>
        /// Total number of mods that match your specified filters. 
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }

        /// <summary>
        /// Utility links to mod portal api requests, preserving all filters and search queries.
        /// </summary>
        [JsonPropertyName("links")]
        public PaginationLinks Links { get; set; }

        /// <summary>
        /// The current page number.
        /// </summary>
        [JsonPropertyName("page")]
        public int Page { get; set; }

        /// <summary>
        /// The total number of pages returned.
        /// </summary>
        [JsonPropertyName("page_count")]
        public int PageCount { get; set; }

        /// <summary>
        /// The number of results per page.
        /// </summary>
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }
    }
}
