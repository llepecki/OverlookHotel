namespace OverlookHotel.DbInit;

using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.IO;

public record DataFileLocation
{
    public static IDictionary<string, string> ArgMappings => new Dictionary<string, string>
    {
        ["--hotels"] = nameof(HotelsFilePath),
        ["--bookings"] = nameof(BookingsFilePath)
    };

    public required string HotelsFilePath { get; init; }

    public required string BookingsFilePath { get; init; }

    public Result<DataFileLocation, IReadOnlyCollection<string>> EnsureValidOptions()
    {
        List<string> errors = new();

        if (string.IsNullOrWhiteSpace(HotelsFilePath))
        {
            errors.Add("Path to the hotels file is required (example: '--hotels hotels.json')");
        }

        if (!File.Exists(HotelsFilePath))
        {
            errors.Add($"Path to the hotels file does not exist: {HotelsFilePath}");
        }

        if (string.IsNullOrWhiteSpace(BookingsFilePath))
        {
            errors.Add("Path to the bookings file is required (example: '--bookings bookings.json')");
        }

        if (!File.Exists(BookingsFilePath))
        {
            errors.Add($"Path to the booking file does not exist: {BookingsFilePath}");
        }

        if (errors.Count == 0)
        {
            return Result.Success<DataFileLocation, IReadOnlyCollection<string>>(this);
        }

        return Result.Failure<DataFileLocation, IReadOnlyCollection<string>>(errors);
    }
}