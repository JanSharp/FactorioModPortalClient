using System.Text.Json.Serialization;

namespace FactorioModPortalClient
{
    public class Error
    {
        [JsonPropertyName("detail")]
        public string Detail { get; set; }
    }
}
