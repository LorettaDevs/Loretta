using System.Text.Json;
using System.Text.Json.Serialization;

namespace Loretta.UnusedStuffFinder
{
    internal class SymbolExConverter : JsonConverter<SymbolEx>
    {
        public override SymbolEx? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, SymbolEx value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(SymbolEx.Symbol), PublicApiContainer.GetPublicApiName(value.Symbol));
            writer.WriteBoolean(nameof(SymbolEx.IsUsed), value.IsUsed);
            writer.WritePropertyName(nameof(SymbolEx.RequiredBy));
            writer.WriteStartArray();
            foreach (var requiredBy in value.RequiredBy)
                writer.WriteStringValue(PublicApiContainer.GetPublicApiName(requiredBy.Symbol));
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
