namespace OverlookHotel.Application.Search;

using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

public class SearchHandler :
    IRequestHandler<SearchQuery, Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>>,
    IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TimeProvider _timeProvider;

    public SearchHandler(SqliteConnection connection, TimeProvider timeProvider)
    {
        _connection = connection;
        _timeProvider = timeProvider;
    }

    public async Task<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>> Handle(SearchQuery query, CancellationToken cancellationToken)
    {
        var startDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);

        await _connection.OpenAsync(cancellationToken);
        
        const string totalRoomsSql = @"
            SELECT COUNT(*)
            FROM Rooms
            WHERE HotelId = @HotelId AND RoomType = @RoomType;";

        int totalRooms = await _connection.QuerySingleAsync<int>(
            totalRoomsSql,
            new { HotelId = query.HotelId, RoomType = query.RoomType },
            null,
            null,
            CommandType.Text);

        var dailyAvailability = new List<KeyValuePair<DateOnly, int>>();

        for (int day = 0; day <= query.DaysAhead; day++)
        {
            var currentDay = startDate.AddDays(day);
            var nextDay = currentDay.AddDays(1);

            string currentDayStr = currentDay.ToString("O", CultureInfo.InvariantCulture);
            string nextDayStr = nextDay.ToString("O", CultureInfo.InvariantCulture);

            const string bookingOverlapSql = @"
                SELECT COUNT(*)
                FROM Bookings
                WHERE HotelId = @HotelId
                  AND RoomType = @RoomType
                  AND Arrival < @NextDay
                  AND Departure >= @CurrentDay";

            int overlappingBookings = await _connection.QuerySingleAsync<int>(
                bookingOverlapSql,
                new
                {
                    HotelId = query.HotelId,
                    RoomType = query.RoomType,
                    CurrentDay = currentDayStr,
                    NextDay = nextDayStr
                },
                null,
                null,
                CommandType.Text
            );

            dailyAvailability.Add(KeyValuePair.Create(currentDay, totalRooms - overlappingBookings));
        }

        var results = GroupDailyAvailability(dailyAvailability);
        return Result.Success<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>(results);
    }

    private static IReadOnlyCollection<SearchResult> GroupDailyAvailability(List<KeyValuePair<DateOnly, int>> dailyAvailability)
    {
        if (dailyAvailability.Count == 0)
        {
            return Array.Empty<SearchResult>();
        }

        var results = new List<SearchResult>();
        var begin = dailyAvailability[0].Key;
        var end = dailyAvailability[0].Key;
        int currentRangeAvailability = dailyAvailability[0].Value;

        for (int i = 1; i < dailyAvailability.Count; i++)
        {
            var currentDay = dailyAvailability[i].Key;
            var currentDayAvailability = dailyAvailability[i].Value;

            bool changed = currentDayAvailability != currentRangeAvailability;
            bool gap = currentDay != end.AddDays(1);

            if (changed || gap)
            {
                if (currentRangeAvailability > 0)
                {
                    results.Add(new SearchResult(begin, end, currentRangeAvailability));
                }

                begin = currentDay;
                currentRangeAvailability = currentDayAvailability;
            }

            end = currentDay;
        }

        if (currentRangeAvailability > 0)
        {
            var lastDayAvailability = dailyAvailability[^1];
            results.Add(new SearchResult(begin, lastDayAvailability.Key, currentRangeAvailability));
        }

        return results;
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}