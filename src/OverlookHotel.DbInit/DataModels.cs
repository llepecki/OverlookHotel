namespace OverlookHotel.DbInit;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[DataContract]
public record Hotel
{
    [DataMember]
    public required string Id { get; init; }

    [DataMember]
    public required string Name { get; init; }

    [DataMember]
    public RoomType[] RoomTypes { get; init; } = Array.Empty<RoomType>();

    [DataMember]
    public Room[] Rooms { get; init; } = Array.Empty<Room>();
}

[DataContract]
public record RoomType
{
    [DataMember]
    public required string Code { get; init; }

    [DataMember]
    public required string Description { get; init; }

    [DataMember]
    public string[] Amenities { get; init; } = Array.Empty<string>();

    [DataMember]
    public string[] Features { get; init; } = Array.Empty<string>();
}

[DataContract]
public record Room
{
    [DataMember]
    public required string RoomType { get; init; }

    [DataMember]
    public required string RoomId { get; init; }
}

public record Booking
{
    [DataMember]
    public required string HotelId { get; init; }

    [DataMember, JsonConverter(typeof(DateOnlyConverter))]
    public required DateOnly Arrival { get; init; }

    [DataMember, JsonConverter(typeof(DateOnlyConverter))]
    public required DateOnly Departure { get; init; }

    [DataMember]
    public required string RoomType { get; init; }

    [DataMember]
    public required string RoomRate { get; init; }
}