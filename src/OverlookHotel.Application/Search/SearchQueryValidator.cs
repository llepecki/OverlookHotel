namespace OverlookHotel.Application.Search;

using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class SearchQueryValidator :
    IPipelineBehavior<SearchQuery, Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>>
{
    public Task<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>> Handle(
        SearchQuery request,
        RequestHandlerDelegate<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>> next,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(request.HotelId))
        {
            errors.Add($"{nameof(SearchQuery.HotelId)} is required");
        }

        if (string.IsNullOrEmpty(request.RoomType))
        {
            errors.Add($"{nameof(SearchQuery.RoomType)} is required");
        }

        if (request.DaysAhead < 0)
        {
            errors.Add($"{nameof(SearchQuery.DaysAhead)} can't be negative");
        }

        if (errors.Count > 0)
        {
            return Task.FromResult(Result.Failure<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>(errors));
        }

        return next();
    }
}