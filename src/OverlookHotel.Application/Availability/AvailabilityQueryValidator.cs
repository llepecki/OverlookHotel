namespace OverlookHotel.Application.Availability;

using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AvailabilityQueryValidator :
    IPipelineBehavior<AvailabilityQuery, Result<AvailabilityResult, IReadOnlyCollection<string>>>
{
    public Task<Result<AvailabilityResult, IReadOnlyCollection<string>>> Handle(
        AvailabilityQuery request,
        RequestHandlerDelegate<Result<AvailabilityResult, IReadOnlyCollection<string>>> next,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(request.HotelId))
        {
            errors.Add($"{nameof(AvailabilityQuery.HotelId)} is required");
        }

        if (string.IsNullOrEmpty(request.RoomType))
        {
            errors.Add($"{nameof(AvailabilityQuery.RoomType)} is required");
        }

        if (request.From > request.To)
        {
            errors.Add($"'{nameof(AvailabilityQuery.To)}' date can't precede '{nameof(AvailabilityQuery.From)}' date");
        }

        if (errors.Count > 0)
        {
            return Task.FromResult(Result.Failure<AvailabilityResult, IReadOnlyCollection<string>>(errors));
        }

        return next();
    }
}