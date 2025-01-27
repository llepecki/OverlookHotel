namespace OverlookHotel.Application.Tests;

using DbInit;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class DataFixture : IAsyncLifetime
{
    private readonly string _databaseFileName = $"{Guid.NewGuid():N}.db";
    private readonly List<SqliteConnection> _connectionsToDispose = [];

    public async Task InitializeAsync()
    {
        await using var connection = GetConnection();

        var dataLoader = new DataLoader(new DataFileLocation { HotelsFilePath = "testHotels.json", BookingsFilePath = "testBookings.json" }, connection);
        var result = await dataLoader.Load(CancellationToken.None);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to load test data: {string.Join(Environment.NewLine, result.Error)}");
        }
    }

    public async Task DisposeAsync()
    {
        if (File.Exists(_databaseFileName))
        {
            File.Delete(_databaseFileName);
        }

        foreach (var connection in _connectionsToDispose)
        {
            await connection.DisposeAsync();
        }
    }

    public SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection($"Data Source={_databaseFileName}");
        _connectionsToDispose.Add(connection);
        return connection;
    }
}