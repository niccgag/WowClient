using System.Text.Json;
using System.Text.Json.Serialization;
using WowClient.Models;

namespace WowClient.Json;

/// <summary>
/// Reads a <see cref="WowLocalizedName"/> from either a locale-to-value object or a single JSON
/// string, which is treated as an <c>en_US</c> value. Search endpoints return locale maps, while a
/// few payloads flatten the name to a plain string.
/// </summary>
internal sealed class WowLocalizedNameConverter : JsonConverter<WowLocalizedName>
{
    public override WowLocalizedName? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.String:
                return new WowLocalizedName
                {
                    Values = new Dictionary<string, JsonElement>(StringComparer.Ordinal)
                    {
                        [WowLocale.EnglishUs] = JsonSerializer.SerializeToElement(reader.GetString()),
                    },
                };

            case JsonTokenType.StartObject:
            {
                var values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException("Expected a property name in a localized name object.");
                    }

                    var locale = reader.GetString()
                        ?? throw new JsonException("A localized name property must have a name.");
                    reader.Read();
                    using var element = JsonDocument.ParseValue(ref reader);
                    values[locale] = element.RootElement.Clone();
                }

                return new WowLocalizedName { Values = values };
            }

            default:
                throw new JsonException(
                    $"Unexpected token '{reader.TokenType}' when reading a localized name.");
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        WowLocalizedName value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Values is not null)
        {
            foreach (var entry in value.Values)
            {
                writer.WritePropertyName(entry.Key);
                entry.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }
}
