using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    /// <summary>
    /// <para>used by <see cref="Release"/></para>
    /// <para>A copy of the mod's info.json file, only contains factorio_version in short version, also contains an array of dependencies in full version</para>
    /// </summary>
    public class InfoJson
    {
        [JsonPropertyName("factorio_version")]
        public string FactorioVersion { get; set; }

        /// <summary>
        /// TODO: check if there is a better solution for dependencies
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement> Rest { get; set; }
    }
}
