using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OverlookHotel.Application;
using OverlookHotel.Application.Availability;
using OverlookHotel.Application.Search;
using OverlookHotel.DbInit;
using OverlookHotel.Runner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) => cancellationTokenSource.Cancel();

const string dataFileName = "OverlookHotel.db";

var host = Host
    .CreateDefaultBuilder()
    .ConfigureAppConfiguration((_, builder) =>
    {
        builder
            .AddCommandLine(args, DataFileLocation.ArgMappings)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(ConnectionStrings)}:{nameof(ConnectionStrings.Default)}"] = $"Data Source={dataFileName}"
            });
    })
    .ConfigureLogging(builder => builder.ClearProviders().AddConsole())
    .ConfigureServices((context, services) =>
    {
        services
            .AddOptions()
            .Configure<ConnectionStrings>(context.Configuration.GetSection(nameof(ConnectionStrings)))
            .Configure<DataFileLocation>(context.Configuration);

        services
            .AddTransient<SqliteConnection>(provider => new SqliteConnection(provider.GetRequiredService<IOptions<ConnectionStrings>>().Value.Default)) // Open the connection or singleton?
            .AddTransient<DataLoader>(provider => new DataLoader(provider.GetRequiredService<IOptions<DataFileLocation>>().Value, provider.GetRequiredService<SqliteConnection>()))
            .AddSingleton(TimeProvider.System)
            .AddApplicationComponents();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

if (File.Exists(dataFileName))
{
    File.Delete(dataFileName);
}

await using var dataLoader = host.Services.GetRequiredService<DataLoader>();
var dataLoadResult = await dataLoader.Load(cancellationTokenSource.Token);

if (dataLoadResult.IsFailure)
{
    logger.LogError("Failed to load data:\n{Errors}", string.Join(Environment.NewLine, dataLoadResult.Error));
    return -1;
}

logger.LogInformation("Inserted {HotelCount} hotels and {BookingCount} bookings", dataLoadResult.Value.HotelCount, dataLoadResult.Value.BookingCount);
await Task.Delay(10);

var mediator = host.Services.GetRequiredService<IMediator>();

Console.Write("Command > ");
var commandStr = Console.ReadLine();
var commandResult = CommandParser.ParseCommand(commandStr);

if (commandResult.IsFailure)
{
    logger.LogError("Failed to parse command:\n{Errors}", string.Join(Environment.NewLine, commandResult.Error));
    return -2;
}

if (commandResult.Value is AvailabilityQuery availabilityQuery)
{
    var availabilityResult = await mediator.Send(availabilityQuery, cancellationTokenSource.Token);

    if (availabilityResult.IsFailure)
    {
        logger.LogError("{QueryName}\n{Errors}", nameof(AvailabilityQuery), string.Join(Environment.NewLine, availabilityResult.Error));
        return -3;
    }

    logger.LogInformation("Available rooms: {AvailableRooms}", availabilityResult.Value.AvailableRooms);
}

if (commandResult.Value is SearchQuery searchQuery)
{
    var searchResult = await mediator.Send(searchQuery, cancellationTokenSource.Token);

    if (searchResult.IsFailure)
    {
        logger.LogError("{QueryName}\n{Errors}", nameof(SearchQuery), string.Join(Environment.NewLine, searchResult.Error));
        return -4;
    }

    logger.LogInformation("Search results:\n{Result}", string.Join(Environment.NewLine, searchResult.Value));
}

return 0;