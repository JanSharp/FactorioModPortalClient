using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    public class Tag
    {
        /// <summary>
        /// A numerical ID unique to this tag.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// An all lower-case string used to identify this tag internally.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The tag's human-readable tag name.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// A short description for the tag.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
