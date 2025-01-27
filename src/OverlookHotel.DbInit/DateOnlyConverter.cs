namespace OverlookHotel.DbInit;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DateOnlyConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyyMMdd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();

        if (DateOnly.TryParseExact(str, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            return value;
        }

        throw new JsonException($"Invalid date format for '{str}', expected '{DateFormat}'");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}