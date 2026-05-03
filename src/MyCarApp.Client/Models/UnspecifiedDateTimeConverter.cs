using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyCarApp.Client.Models;

public class UnspecifiedDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (DateTime.TryParse(value, out var dt))
            return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Always write as plain datetime string without timezone offset
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}