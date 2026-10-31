using System.Text.Json;
using System.Text.Json.Serialization;

namespace JavaMaster.Api.Converters;

/// <summary>
/// Writes every DateTime in an API response as UTC, with a trailing Z.
///
/// SQL Server's datetime2 does not store a timezone. EF therefore reads a UTC
/// value back as DateTimeKind.Unspecified, and System.Text.Json would send it
/// without a Z. A browser then treats it as local time, which can move a date by
/// several hours - or onto the wrong day near midnight.
///
/// The app only ever stores UTC, so an Unspecified value is treated as UTC.
/// </summary>
public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        writer.WriteStringValue(utc);
    }
}
