namespace OverlookHotel.Application.Availability;

using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;

public record AvailabilityQuery(string HotelId, DateOnly From, DateOnly To, string RoomType) :
    IRequest<Result<AvailabilityResult, IReadOnlyCollection<string>>>;