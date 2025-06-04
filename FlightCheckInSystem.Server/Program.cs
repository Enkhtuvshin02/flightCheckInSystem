using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Business.Services;
using FlightCheckInSystem.Data;
using FlightCheckInSystem.Data.Interfaces;
using FlightCheckInSystem.Data.Repositories;
using FlightCheckInSystem.Server.Hubs;
using FlightCheckInSystem.Server.Services; // Add this line
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading;

SQLitePCL.Batteries_V2.Init();
raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var dbFileName = "flightCheckInSystem.db";
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "Data");
string dbFilePath = Path.Combine(dataDirectory, dbFileName);

if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
    var directoryInfo = new DirectoryInfo(dataDirectory);
    var directorySecurity = directoryInfo.GetAccessControl();
    directorySecurity.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
        "Users",
        System.Security.AccessControl.FileSystemRights.FullControl,
        System.Security.AccessControl.InheritanceFlags.ContainerInherit | System.Security.AccessControl.InheritanceFlags.ObjectInherit,
        System.Security.AccessControl.PropagationFlags.None,
        System.Security.AccessControl.AccessControlType.Allow));
    directoryInfo.SetAccessControl(directorySecurity);
}

var startupLogger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddDebug();
}).CreateLogger("Program");

startupLogger.LogInformation("Application starting...");
startupLogger.LogInformation("Database file path: {DbFilePath}", dbFilePath);

string connectionString = $"Data Source={dbFilePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True";

builder.Services.AddSingleton<DatabaseInitializer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DatabaseInitializer>>();
    return new DatabaseInitializer(dbFilePath, logger);
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.MaximumReceiveMessageSize = 102400;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:7039",
                "https://localhost:7039",
                "http://localhost:5177",
                "http://localhost:5001",
                "http://localhost:5000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IPassengerRepository, PassengerRepository>(sp => new PassengerRepository(connectionString));
builder.Services.AddScoped<IFlightRepository, FlightRepository>(sp => new FlightRepository(connectionString));
builder.Services.AddScoped<ISeatRepository, SeatRepository>(sp => new SeatRepository(connectionString));
builder.Services.AddScoped<IBookingRepository, BookingRepository>(sp => new BookingRepository(connectionString));

builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IFlightManagementService, FlightManagementService>();

// Add the new SignalR Hub Service
builder.Services.AddScoped<IFlightHubService, FlightHubService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("Database", tags: new[] { "ready", "sqlite" });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var maxRetries = 3;
    var retryCount = 0;

    while (retryCount < maxRetries)
    {
        try
        {
            startupLogger.LogInformation("Initializing database (Attempt {RetryCount}/{MaxRetries})...", retryCount + 1, maxRetries);
            var databaseInitializer = services.GetRequiredService<DatabaseInitializer>();

            await databaseInitializer.InitializeAsync();

            if (app.Environment.IsDevelopment())
            {
                startupLogger.LogInformation("Development environment detected. Seeding database...");
                await databaseInitializer.SeedDataAsync();
            }

            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
                    using var reader = await command.ExecuteReaderAsync();
                    var tables = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        tables.Add(reader.GetString(0));
                    }
                    startupLogger.LogInformation("Database tables created: {Tables}", string.Join(", ", tables));
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            f.FlightId, 
                            f.FlightNumber, 
                            f.DepartureAirport, 
                            f.ArrivalAirport,
                            COUNT(s.SeatId) as TotalSeats,
                            SUM(CASE WHEN s.IsBooked = 1 THEN 1 ELSE 0 END) as BookedSeats
                        FROM Flights f
                        LEFT JOIN Seats s ON f.FlightId = s.FlightId
                        GROUP BY f.FlightId;";

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        startupLogger.LogInformation(
                            "Flight: {FlightNumber} ({FlightId}) | Route: {Departure}-{Arrival} | Seats: {BookedSeats}/{TotalSeats}",
                            reader.GetString(1),
                            reader.GetInt32(0),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetInt32(5),
                            reader.GetInt32(4)
                        );
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            Class,
                            COUNT(*) as SeatCount,
                            SUM(CASE WHEN IsBooked = 1 THEN 1 ELSE 0 END) as BookedCount,
                            AVG(Price) as AveragePrice
                        FROM Seats
                        GROUP BY Class;";

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        startupLogger.LogInformation(
                            "Class: {Class} | Total Seats: {Total} | Booked: {Booked} | Avg Price: ${AvgPrice:F2}",
                            reader.GetString(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetDouble(3)
                        );
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA foreign_key_check;";
                    using var reader = await command.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        startupLogger.LogInformation("All foreign key constraints are valid");
                    }
                }
            }

            startupLogger.LogInformation("Database initialization completed successfully.");
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            if (retryCount >= maxRetries)
            {
                startupLogger.LogError(ex, "Failed to initialize database after {RetryCount} attempts. Last error: {Error}",
                    retryCount, ex.Message);
                throw;
            }

            startupLogger.LogWarning(ex, "Database initialization attempt {RetryCount} failed. Retrying...", retryCount);
            await Task.Delay(TimeSpan.FromSeconds(2 * retryCount));
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flight Check-In System API v1");
        options.RoutePrefix = "swagger";
        options.ConfigObject.DisplayOperationId = true;
        options.ConfigObject.TryItOutEnabled = true;
        options.ConfigObject.DefaultModelsExpandDepth = -1;
    });
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<FlightHub>("/flighthub");
    endpoints.MapHealthChecks("/health");
});

var urls = app.Urls.ToList();
if (!urls.Any())
{
    urls.Add("http://localhost:5001");
}

foreach (var url in urls)
{
    startupLogger.LogInformation($"Server listening on: {url}");
}

                app.Run();

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public DatabaseHealthCheck(IConfiguration configuration)
    {
        var dbFileName = "flightCheckInSystem.db";
        var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        var dbFilePath = Path.Combine(dataDirectory, dbFileName);
        _connectionString = $"Data Source={dbFilePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1;";
                    await command.ExecuteScalarAsync(cancellationToken);
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) FROM sqlite_master 
                        WHERE type='table' AND (
                            name='Flights' OR 
                            name='Passengers' OR 
                            name='Bookings' OR 
                            name='Seats'
                        );";
                    var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
                    if (tableCount < 4)
                    {
                        return HealthCheckResult.Degraded("Some required tables are missing.");
                    }
                }

                return HealthCheckResult.Healthy("Database is operational");
            }
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex)
        {
            return HealthCheckResult.Unhealthy("Database error: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Unexpected error: " + ex.Message, ex);
        }
    }
}