using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ECommerce.API.Json;

// بيرجّع كل التواريخ بعلامة UTC (Z) عشان المتصفح يعرضها بتوقيت المستخدم صح
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.Parse(
            reader.GetString()!,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        writer.WriteStringValue(utc);
    }
}