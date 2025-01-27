namespace OverlookHotel.Application.Tests.Search;

using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using OverlookHotel.Application.Search;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class SearchQueryValidatorTests
{
    [Fact]
    public async Task Handle_ValidQuery_CallsNext()
    {
        // Arrange
        var query = new SearchQuery("H1", "SGL", 10);
        var mockNext = new Mock<RequestHandlerDelegate<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new SearchQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        mockNext.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingHotelId_ReturnsFailure()
    {
        // Arrange
        var query = new SearchQuery(string.Empty, "SGL", 10);

        var mockNext = new Mock<RequestHandlerDelegate<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new SearchQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("HotelId is required", result.Error);
        mockNext.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingRoomType_ReturnsFailure()
    {
        // Arrange
        var query = new SearchQuery("H1", string.Empty, 10);

        var mockNext = new Mock<RequestHandlerDelegate<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new SearchQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("RoomType is required", result.Error);
        mockNext.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_NegativeDaysAhead_ReturnsFailure()
    {
        // Arrange
        var query = new SearchQuery("H1", "SGL", -5);

        var mockNext = new Mock<RequestHandlerDelegate<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new SearchQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("DaysAhead can't be negative", result.Error);
        mockNext.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var query = new SearchQuery(string.Empty, string.Empty, -1);

        var mockNext = new Mock<RequestHandlerDelegate<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new SearchQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("HotelId is required", result.Error);
        Assert.Contains("RoomType is required", result.Error);
        Assert.Contains("DaysAhead can't be negative", result.Error);
        mockNext.Verify(n => n(), Times.Never);
    }
}