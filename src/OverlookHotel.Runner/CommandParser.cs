namespace OverlookHotel.Runner;

using System;
using System.Collections.Generic;
using System.Globalization;
using Application.Availability;
using Application.Search;
using CSharpFunctionalExtensions;

public static class CommandParser
{
    private const string DateFormat = "yyyyMMdd";

    public static Result<object, IReadOnlyCollection<string>> ParseCommand(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result.Failure<object, IReadOnlyCollection<string>>(["Input can't be empty"]);
        }
        
        input = input.Trim();

        if (input.StartsWith("Availability(", StringComparison.OrdinalIgnoreCase) && input.EndsWith(")"))
        {
            return ParseAvailability(input);
        }

        if (input.StartsWith("Search(", StringComparison.OrdinalIgnoreCase) && input.EndsWith(")"))
        {
            return ParseSearch(input);
        }

        return Result.Failure<object, IReadOnlyCollection<string>>(["Invalid command format"]);
    }

    private static Result<AvailabilityQuery, IReadOnlyCollection<string>> ParseAvailability(string input)
    {
        var inside = input.Substring("Availability(".Length, input.Length - "Availability(".Length - 1);
        var parts = inside.Split(',', StringSplitOptions.TrimEntries);

        if (parts.Length != 3)
        {
            return Result.Failure<AvailabilityQuery, IReadOnlyCollection<string>>(["Availability command requires 3 arguments (HotelId, date or range, RoomType)"]);
        }

        string hotelId = parts[0];
        string dateOrRange = parts[1];
        string roomType = parts[2];

        DateOnly fromDate;
        DateOnly toDate;

        if (dateOrRange.Contains('-'))
        {
            var rangeParts = dateOrRange.Split('-', StringSplitOptions.TrimEntries);

            if (rangeParts.Length != 2)
            {
                return Result.Failure<AvailabilityQuery, IReadOnlyCollection<string>>(["Invalid date range"]);
            }

            fromDate = ParseDate(rangeParts[0]);
            toDate = ParseDate(rangeParts[1]);
        }
        else
        {
            fromDate = ParseDate(dateOrRange);
            toDate = fromDate;
        }

        return new AvailabilityQuery(hotelId, fromDate, toDate, roomType);
    }

    private static Result<SearchQuery, IReadOnlyCollection<string>> ParseSearch(string input)
    {
        var inside = input.Substring("Search(".Length, input.Length - "Search(".Length - 1);
        var parts = inside.Split(',', StringSplitOptions.TrimEntries);

        if (parts.Length != 3)
        {
            return Result.Failure<SearchQuery, IReadOnlyCollection<string>>(["Search command requires 3 arguments (HotelId, DaysAhead, RoomType)"]);
        }

        string hotelId = parts[0];

        if (!int.TryParse(parts[1], out int daysAhead))
        {
            return Result.Failure<SearchQuery, IReadOnlyCollection<string>>(["Unable to parse days ahead"]);
        }

        string roomType = parts[2];

        return new SearchQuery(hotelId, roomType, daysAhead);
    }

    private static DateOnly ParseDate(string dateStr)
    {
        return DateOnly.ParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture);
    }
}
