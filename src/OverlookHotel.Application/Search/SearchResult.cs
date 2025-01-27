namespace OverlookHotel.Application.Search;

using System;

public record SearchResult(DateOnly From, DateOnly To, int RoomCount);