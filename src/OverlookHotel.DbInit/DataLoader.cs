namespace OverlookHotel.DbInit;

using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class DataLoader : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly DataFileLocation _options;
    private readonly SqliteConnection _connection;

    public DataLoader(DataFileLocation options, SqliteConnection connection)
    {
        _options = options;
        _connection = connection;
    }
    
    public async Task<Result<DataLoadSummary, IReadOnlyCollection<string>>> Load(CancellationToken cancellationToken)
    {
        var result = _options.EnsureValidOptions();

        if (result.IsFailure)
        {
            return Result.Failure<DataLoadSummary, IReadOnlyCollection<string>>(result.Error);
        }

        await using var hotelFileStream = File.OpenRead(_options.HotelsFilePath);
        await using var bookingFileStream = File.OpenRead(_options.BookingsFilePath);

        var hotels = await JsonSerializer.DeserializeAsync<Hotel[]>(hotelFileStream, JsonSerializerOptions, cancellationToken) ?? Array.Empty<Hotel>();
        var bookings = await JsonSerializer.DeserializeAsync<Booking[]>(bookingFileStream, JsonSerializerOptions, cancellationToken) ?? Array.Empty<Booking>();

        await _connection.OpenAsync(cancellationToken);
        await using var transaction = _connection.BeginTransaction();

        await CreateTables(_connection, transaction);
        await InsertHotels(_connection, transaction, hotels);
        await InsertBookings(_connection, transaction, bookings);

        transaction.Commit();

        return Result.Success<DataLoadSummary, IReadOnlyCollection<string>>(new DataLoadSummary(hotels.Length, bookings.Length));
    }

    private static async Task CreateTables(SqliteConnection connection, IDbTransaction transaction)
    {
        // Hotels
        const string createHotelsTable = @"
            CREATE TABLE IF NOT EXISTS Hotels (
                HotelId TEXT PRIMARY KEY,
                Name TEXT NOT NULL
            );";

        await connection.ExecuteAsync(createHotelsTable, null, transaction, null, CommandType.Text);

        // RoomTypes
        var createRoomTypesTable = @"
            CREATE TABLE IF NOT EXISTS RoomTypes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                HotelId TEXT NOT NULL,
                Code TEXT NOT NULL,
                Description TEXT
            );";

        await connection.ExecuteAsync(createRoomTypesTable, null, transaction, null, CommandType.Text);

        // Separate tables for Amenities and Features (FK to RoomTypes.Id)
        var createRoomTypeAmenitiesTable = @"
            CREATE TABLE IF NOT EXISTS RoomTypeAmenities (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RoomTypeId INTEGER NOT NULL,
                Amenity TEXT NOT NULL,
                FOREIGN KEY(RoomTypeId) REFERENCES RoomTypes(Id)
            );";

        await connection.ExecuteAsync(createRoomTypeAmenitiesTable, null, transaction, null, CommandType.Text);
        
        var createRoomTypeFeaturesTable = @"
            CREATE TABLE IF NOT EXISTS RoomTypeFeatures (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RoomTypeId INTEGER NOT NULL,
                Feature TEXT NOT NULL,
                FOREIGN KEY(RoomTypeId) REFERENCES RoomTypes(Id)
            );";
        
        await connection.ExecuteAsync(createRoomTypeFeaturesTable, null, transaction, null, CommandType.Text);

        // Rooms
        var createRoomsTable = @"
            CREATE TABLE IF NOT EXISTS Rooms (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                HotelId TEXT NOT NULL,
                RoomType TEXT NOT NULL,
                RoomId TEXT NOT NULL
            );";
        
        await connection.ExecuteAsync(createRoomsTable, null, transaction, null, CommandType.Text);

        // Bookings
        var createBookingsTable = @"
            CREATE TABLE IF NOT EXISTS Bookings (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                HotelId TEXT NOT NULL,
                Arrival TEXT NOT NULL,
                Departure TEXT NOT NULL,
                RoomType TEXT NOT NULL,
                RoomRate TEXT NOT NULL
             );";

        await connection.ExecuteAsync(createBookingsTable, null, transaction, null, CommandType.Text);
    }

    private static async Task InsertHotels(SqliteConnection connection, IDbTransaction transaction, IReadOnlyCollection<Hotel> hotels)
    {
        foreach (var hotel in hotels)
        {
            // Insert or skip existing Hotel
            const string insertHotel = @"
                INSERT INTO Hotels (HotelId, Name)
                VALUES (@id, @name)
                ON CONFLICT(HotelId) DO NOTHING;";

            await connection.ExecuteAsync(
                insertHotel,
                new { id = hotel.Id, name = hotel.Name },
                transaction,
                null,
                CommandType.Text);

            // Insert RoomTypes, Amenities and Features
            foreach (var roomType in hotel.RoomTypes)
            {
                const string insertRoomType = @"
                    INSERT INTO RoomTypes (HotelId, Code, Description)
                    VALUES (@hotelId, @code, @desc);
                    SELECT last_insert_rowid();";

                var roomTypeId = connection.ExecuteScalar<long>(
                    insertRoomType,
                    new { hotelId = hotel.Id, code = roomType.Code, desc = roomType.Description },
                    transaction,
                    null,
                    CommandType.Text);

                foreach (var amenity in roomType.Amenities)
                {
                    const string insertAmenity = @"
                        INSERT INTO RoomTypeAmenities (RoomTypeId, Amenity)
                        VALUES (@roomTypeId, @amenity);";

                    await connection.ExecuteAsync(
                        insertAmenity,
                        new { roomTypeId, amenity },
                        transaction,
                        null,
                        CommandType.Text);
                }

                foreach (var feature in roomType.Features)
                {
                    const string insertFeature = @"
                        INSERT INTO RoomTypeFeatures (RoomTypeId, Feature)
                        VALUES (@roomTypeId, @feature);";

                    await connection.ExecuteAsync(
                        insertFeature,
                        new { roomTypeId, feature },
                        transaction,
                        null,
                        CommandType.Text);
                }
            }

            // Insert Rooms
            foreach (var room in hotel.Rooms)
            {
                const string insertRoom = @"
                    INSERT INTO Rooms (HotelId, RoomType, RoomId)
                    VALUES (@hotelId, @roomType, @roomId);";

                await connection.ExecuteAsync(
                    insertRoom,
                    new
                    {
                        hotelId = hotel.Id,
                        roomType = room.RoomType,
                        roomId = room.RoomId
                    },
                    transaction,
                    null,
                    CommandType.Text);
            }
        }
    }

    private static async Task InsertBookings(SqliteConnection connection, IDbTransaction transaction, IReadOnlyCollection<Booking> bookings)
    {
        foreach (var booking in bookings)
        {
            const string insertBooking = @"
                INSERT INTO Bookings (HotelId, Arrival, Departure, RoomType, RoomRate)
                VALUES (@hotelId, @arrival, @departure, @roomType, @roomRate);";

            await connection.ExecuteAsync(
                insertBooking,
                new
                {
                    hotelId = booking.HotelId,
                    arrival = booking.Arrival.ToString("O", CultureInfo.InvariantCulture),
                    departure = booking.Departure.ToString("O", CultureInfo.InvariantCulture),
                    roomType = booking.RoomType,
                    roomRate = booking.RoomRate
                },
                transaction,
                null,
                CommandType.Text);
        }
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}