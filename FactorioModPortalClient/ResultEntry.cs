using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    /// <summary>
    /// for api/mods endpoint, see <see cref="ModListResponse"/>
    /// </summary>
    public class ResultEntry
    {
        /// <summary>
        /// Number of downloads.
        /// </summary>
        [JsonPropertyName("downloads_count")]
        public int DownloadsCount { get; set; }

        /// <summary>
        /// The latest version of the mod available for download.
        /// </summary>
        [JsonPropertyName("latest_release")]
        public Release LatestRelease { get; set; }

        /// <summary>
        /// The mod's machine-readable ID string.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The Factorio username of the mod's author.
        /// </summary>
        [JsonPropertyName("owner")]
        public string Owner { get; set; }

        /// <summary>
        /// A shorter mod description.
        /// </summary>
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// The mod's human-readable name.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
