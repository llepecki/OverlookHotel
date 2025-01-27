namespace OverlookHotel.Application.Availability;

using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

public class AvailabilityHandler :
    IRequestHandler<AvailabilityQuery, Result<AvailabilityResult, IReadOnlyCollection<string>>>,
    IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public AvailabilityHandler(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<AvailabilityResult, IReadOnlyCollection<string>>> Handle(AvailabilityQuery query, CancellationToken cancellationToken)
    {
        await _connection.OpenAsync(cancellationToken);

        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM Rooms WHERE HotelId = @HotelId AND RoomType = @RoomType) -
                (SELECT COUNT(*) FROM Bookings WHERE HotelId = @HotelId AND RoomType = @RoomType AND Departure > @From AND Arrival < @To)";

        var parameters = new
        {
            HotelId = query.HotelId,
            RoomType = query.RoomType,
            From = query.From.ToString("O", CultureInfo.InvariantCulture),
            To = query.To.ToString("O", CultureInfo.InvariantCulture)
        };

        int count = await _connection.QuerySingleAsync<int>(sql, parameters);

        return Result.Success<AvailabilityResult, IReadOnlyCollection<string>>(new AvailabilityResult(count));
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}