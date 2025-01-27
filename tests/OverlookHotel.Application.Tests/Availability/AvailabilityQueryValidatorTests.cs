namespace OverlookHotel.Application.Tests.Availability;

using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using OverlookHotel.Application.Availability;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class AvailabilityQueryValidatorTests
{
    [Fact]
    public async Task Handle_ValidQuery_CallsNext()
    {
        // Arrange
        var query = new AvailabilityQuery("H1", DateOnly.Parse("2025-09-01"), DateOnly.Parse("2025-09-05"), "SGL");
        var mockNext = new Mock<RequestHandlerDelegate<Result<AvailabilityResult, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new AvailabilityQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        mockNext.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingHotelId_ReturnsFailure()
    {
        // Arrange
        var query = new AvailabilityQuery(string.Empty, DateOnly.Parse("2025-09-01"), DateOnly.Parse("2025-09-05"), "SGL");
        var mockNext = new Mock<RequestHandlerDelegate<Result<AvailabilityResult, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new AvailabilityQueryValidator();
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
        var query = new AvailabilityQuery("H1", DateOnly.Parse("2025-09-01"), DateOnly.Parse("2025-09-05"), string.Empty);
        var mockNext = new Mock<RequestHandlerDelegate<Result<AvailabilityResult, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new AvailabilityQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("RoomType is required", result.Error);
        mockNext.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_FromDateAfterToDate_ReturnsFailure()
    {
        // Arrange
        var query = new AvailabilityQuery("H1", DateOnly.Parse("2025-09-15"), DateOnly.Parse("2025-09-05"), "SGL");
        var mockNext = new Mock<RequestHandlerDelegate<Result<AvailabilityResult, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new AvailabilityQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("'To' date can't precede 'From' date", result.Error);
        mockNext.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var query = new AvailabilityQuery(string.Empty, DateOnly.Parse("2025-09-15"), DateOnly.Parse("2025-09-05"), string.Empty);
        var mockNext = new Mock<RequestHandlerDelegate<Result<AvailabilityResult, IReadOnlyCollection<string>>>>();

        // Act
        var validator = new AvailabilityQueryValidator();
        var result = await validator.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("HotelId is required", result.Error);
        Assert.Contains("RoomType is required", result.Error);
        Assert.Contains("'To' date can't precede 'From' date", result.Error);
        mockNext.Verify(n => n(), Times.Never);
    }
}