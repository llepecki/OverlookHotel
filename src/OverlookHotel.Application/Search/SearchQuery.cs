namespace OverlookHotel.Application.Search;

using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;

public record SearchQuery(string HotelId, string RoomType, int DaysAhead) :
    IRequest<Result<IReadOnlyCollection<SearchResult>, IReadOnlyCollection<string>>>;