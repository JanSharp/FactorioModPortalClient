using System;
using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    public class Release
    {
        /// <summary>
        /// Path to download for a mod. starts with "/download" and does not include a full url.
        /// </summary>
        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// The file name of the release. Always seems to follow the pattern "{name}_{version}.zip"
        /// </summary>
        [JsonPropertyName("file_name")]
        public string FileName { get; set; }

        /// <summary>
        /// A copy of the mod's info.json file, only contains factorio_version in short version, also contains an array of dependencies in full version
        /// </summary>
        [JsonPropertyName("info_json")]
        public InfoJson InfoJson { get; set; }

        /// <summary>
        /// ISO 6501 for when the mod was released.
        /// </summary>
        [JsonPropertyName("released_at")]
        public DateTime ReleasedAt { get; set; }

        /// <summary>
        /// The version string of this mod release. Used to determine dependencies.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; }

        /// <summary>
        /// The sha1 key for the file
        /// </summary>
        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }
    }
}
