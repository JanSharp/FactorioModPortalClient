using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    public class ResultEntryFull
    {
        /// <summary>
        /// Number of downloads.
        /// </summary>
        [JsonPropertyName("downloads_count")]
        public int DownloadsCount { get; set; }

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
        /// A list of different versions of the mod available for download.
        /// </summary>
        [JsonPropertyName("releases")]
        public List<Release> Releases { get; set; }

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

        /// <summary>
        /// A string describing the recent changes to a mod.
        /// </summary>
        [JsonPropertyName("changelog")]
        public string Changelog { get; set; }

        /// <summary>
        /// ISO 6501 for when the mod was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// A longer description of the mod, in text only format.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// A link to the mod's github project page, just prepend "github.com/". Can be blank ("").
        /// </summary>
        [JsonPropertyName("github_path")]
        public string GithubPath { get; set; }

        /// <summary>
        /// Usually a URL to the mod's main project page, but can be any string.
        /// </summary>
        [JsonPropertyName("homepage")]
        public string Homepage { get; set; }

        /// <summary>
        /// A list of tag objects that categorize the mod.
        /// </summary>
        [JsonPropertyName("tag")]
        public List<Tag> Tags { get; set; }
    }
}
