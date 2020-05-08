using System;
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

        [JsonPropertyName("dependencies")]
        public Dependencies Dependencies { get; set; }
    }

    [JsonConverter(typeof(DependenciesConverter))]
    public class Dependencies
    {
        public string[] dependencies;
    }


    public class DependenciesConverter : JsonConverter<Dependencies>
    {
        public override Dependencies Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                using JsonDocument doc = JsonDocument.ParseValue(ref reader);
                string[] result = new string[doc.RootElement.GetArrayLength()];
                int i = 0;
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    result[i++] = item.GetString();
                }
                return new Dependencies() { dependencies = result };
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, Dependencies value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
