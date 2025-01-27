namespace OverlookHotel.Application.Tests.Search;

using OverlookHotel.Application.Search;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class SearchHandlerTests : IClassFixture<DataFixture>
{
    private readonly DataFixture _dataFixture;
    private readonly TestTimeProvider _timeProvider;

    public SearchHandlerTests(DataFixture dataFixture)
    {
        _dataFixture = dataFixture;
        _timeProvider = new TestTimeProvider();
    }

    [Fact]
    public async Task Search_H22_SGL_For10Days_ShouldReturn3Results()
    {
        // Arrange
        _timeProvider.SetUtcNow(DateTimeOffset.Parse("2025-03-01"));
        await using var connection = _dataFixture.GetConnection();
        await connection.OpenAsync();

        var query = new SearchQuery("H22", "SGL", 10);
        await using var handler = new SearchHandler(connection, _timeProvider);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);

        var expected = new List<SearchResult>
        {
            new SearchResult(DateOnly.Parse("2025-03-01"), DateOnly.Parse("2025-03-05"), 4),
            new SearchResult(DateOnly.Parse("2025-03-06"), DateOnly.Parse("2025-03-09"), 5),
            new SearchResult(DateOnly.Parse("2025-03-10"), DateOnly.Parse("2025-03-11"), 4)
        };

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Search_H99_SGL_For5Days_ShouldReturn4Results()
    {
        // Arrange
        _timeProvider.SetUtcNow(DateTimeOffset.Parse("2025-06-01"));
        await using var connection = _dataFixture.GetConnection();
        await connection.OpenAsync();

        var query = new SearchQuery("H99", "SGL", 5);
        await using var handler = new SearchHandler(connection, _timeProvider);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.Count);

        var expected = new List<SearchResult>
        {
            new SearchResult(DateOnly.Parse("2025-06-01"), DateOnly.Parse("2025-06-01"), 3),
            new SearchResult(DateOnly.Parse("2025-06-02"), DateOnly.Parse("2025-06-04"), 1),
            new SearchResult(DateOnly.Parse("2025-06-05"), DateOnly.Parse("2025-06-05"), 3),
            new SearchResult(DateOnly.Parse("2025-06-06"), DateOnly.Parse("2025-06-06"), 5)
        };

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Search_GLL2_TRP_For7Days_ShouldReturn3Results()
    {
        // Arrange
        _timeProvider.SetUtcNow(DateTimeOffset.Parse("2026-06-10"));
        await using var connection = _dataFixture.GetConnection();
        await connection.OpenAsync();

        var query = new SearchQuery("GLL2", "TRP", 7);
        await using var handler = new SearchHandler(connection, _timeProvider);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);

        var expected = new List<SearchResult>
        {
            new SearchResult(DateOnly.Parse("2026-06-10"), DateOnly.Parse("2026-06-11"), 5),
            new SearchResult(DateOnly.Parse("2026-06-12"), DateOnly.Parse("2026-06-15"), 3),
            new SearchResult(DateOnly.Parse("2026-06-16"), DateOnly.Parse("2026-06-17"), 5)
        };

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Search_HT10_SGL_For14Days_ShouldReturn3Results()
    {
        // Arrange
        _timeProvider.SetUtcNow(DateTimeOffset.Parse("2025-01-04"));
        await using var connection = _dataFixture.GetConnection();
        await connection.OpenAsync();

        var query = new SearchQuery("HT10", "SGL", 14);
        await using var handler = new SearchHandler(connection, _timeProvider);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);

        var expected = new List<SearchResult>
        {
            new SearchResult(DateOnly.Parse("2025-01-04"), DateOnly.Parse("2025-01-04"), 10),
            new SearchResult(DateOnly.Parse("2025-01-05"), DateOnly.Parse("2025-01-08"), 9),
            new SearchResult(DateOnly.Parse("2025-01-09"), DateOnly.Parse("2025-01-18"), 10)
        };

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Search_AB5_DBL_For10Days_ShouldReturn1Result()
    {
        // Arrange
        _timeProvider.SetUtcNow(DateTimeOffset.Parse("2026-12-25"));
        await using var connection = _dataFixture.GetConnection();
        await connection.OpenAsync();

        var query = new SearchQuery("AB5", "DBL", 10);
        await using var handler = new SearchHandler(connection, _timeProvider);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);

        var expected = new List<SearchResult>
        {
            new SearchResult(DateOnly.Parse("2026-12-25"), DateOnly.Parse("2027-01-04"), 6)
        };

        Assert.Equal(expected, result.Value);
    }
}