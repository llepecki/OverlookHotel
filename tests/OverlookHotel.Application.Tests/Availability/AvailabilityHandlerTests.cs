using OverlookHotel.Application.Availability;

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OverlookHotel.Application.Tests.Availability;

public class AvailabilityHandlerTests(DataFixture dataFixture) : IClassFixture<DataFixture>
{
    [Theory]
    [InlineData("H22", "2025-03-01", "2025-03-05", "SGL", 4)]
    [InlineData("H22", "2025-03-01", "2025-03-05", "DBL", 5)]
    [InlineData("H22", "2025-03-10", "2025-03-12", "SGL", 4)]
    [InlineData("H99", "2025-06-01", "2025-06-01", "SGL", 3)]
    [InlineData("H99", "2025-06-01", "2025-06-04", "SGL", 1)]
    [InlineData("H99", "2025-06-02", "2025-06-05", "SGL", 1)]
    [InlineData("H99", "2025-06-03", "2025-06-07", "DBL", 3)]
    [InlineData("GLL2", "2026-06-10", "2026-06-12", "SGL", 3)]
    [InlineData("GLL2", "2026-06-12", "2026-06-15", "TRP", 3)]
    [InlineData("HT10", "2025-01-01", "2025-01-03", "SGL", 9)]
    [InlineData("AB5", "2025-03-15", "2025-03-17", "DBL", 5)]
    public async Task Availability_ShouldCorrectlyCountAvailableRooms(string hotelId, string fromStr, string toStr, string roomType, int expectedResult)
    {
        // Arrange
        var from = DateOnly.Parse(fromStr);
        var to = DateOnly.Parse(toStr);

        await using var connection = dataFixture.GetConnection();
        await connection.OpenAsync();

        var query = new AvailabilityQuery(hotelId, from, to, roomType);
        await using var handler = new AvailabilityHandler(connection);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResult, result.Value.AvailableRooms);
    }
}