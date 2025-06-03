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
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading;

// Initialize SQLitePCL with the correct provider
SQLitePCL.Batteries_V2.Init();
raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var dbFileName = "flightCheckInSystem.db";
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "Data");
string dbFilePath = Path.Combine(dataDirectory, dbFileName);

// Ensure Data directory exists with proper permissions
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
    // Set directory permissions to allow read/write
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

// Log the database file path
var startupLogger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddDebug();
}).CreateLogger("Program");

startupLogger.LogInformation("Application starting...");
startupLogger.LogInformation("Content Root Path: {ContentRootPath}", builder.Environment.ContentRootPath);
startupLogger.LogInformation("Database file path: {DbFilePath}", dbFilePath);

string connectionString = $"Data Source={dbFilePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True";

// Register DatabaseInitializer with DI
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

// Configure HTTPS
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 7106; // Match the port used in the client
    });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7039", "http://localhost:5177", "https://localhost:7106")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register repositories
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>(sp => new PassengerRepository(connectionString));
builder.Services.AddScoped<IFlightRepository, FlightRepository>(sp => new FlightRepository(connectionString));
builder.Services.AddScoped<ISeatRepository, SeatRepository>(sp => new SeatRepository(connectionString));
builder.Services.AddScoped<IBookingRepository, BookingRepository>(sp => new BookingRepository(connectionString));

// Register services
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IFlightManagementService, FlightManagementService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("Database", tags: new[] { "ready", "sqlite" });

var app = builder.Build();

// Initialize and seed database
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
            
            // Ensure directory exists and is writable
            var dbDirectory = Path.GetDirectoryName(dbFilePath);
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
                startupLogger.LogInformation("Created database directory: {Directory}", dbDirectory);
            }
            
            // Test write permissions
            try
            {
                using (var fs = File.Create(Path.Combine(dbDirectory, "test.txt")))
                {
                    fs.Close();
                }
                File.Delete(Path.Combine(dbDirectory, "test.txt"));
                startupLogger.LogInformation("Directory write permissions verified");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot write to database directory: {dbDirectory}", ex);
            }
            
            await databaseInitializer.InitializeAsync();

            if (app.Environment.IsDevelopment())
            {
                startupLogger.LogInformation("Development environment detected. Seeding database...");
                await databaseInitializer.SeedDataAsync();
            }
            
            // Verify database state after initialization
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                
                // Check table structure
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

                // Check seeded flights
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
                    startupLogger.LogInformation("Verifying seeded flight data:");
                    while (await reader.ReadAsync())
                    {
                        startupLogger.LogInformation(
                            "Flight: {FlightNumber} ({FlightId}) | Route: {Departure}-{Arrival} | Seats: {BookedSeats}/{TotalSeats}",
                            reader.GetString(1),  // FlightNumber
                            reader.GetInt32(0),   // FlightId
                            reader.GetString(2),  // DepartureAirport
                            reader.GetString(3),  // ArrivalAirport
                            reader.GetInt32(5),   // BookedSeats
                            reader.GetInt32(4)    // TotalSeats
                        );
                    }
                }

                // Check seat distribution
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
                    startupLogger.LogInformation("Seat class distribution:");
                    while (await reader.ReadAsync())
                    {
                        startupLogger.LogInformation(
                            "Class: {Class} | Total Seats: {Total} | Booked: {Booked} | Avg Price: ${AvgPrice:F2}",
                            reader.GetString(0),  // Class
                            reader.GetInt32(1),   // SeatCount
                            reader.GetInt32(2),   // BookedCount
                            reader.GetDouble(3)   // AveragePrice
                        );
                    }
                }

                // Verify database constraints
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA foreign_key_check;";
                    using var reader = await command.ExecuteReaderAsync();
                    var hasRows = false;
                    if (await reader.ReadAsync())
                    {
                        hasRows = true;
                        startupLogger.LogWarning("Foreign key constraint violations detected!");
                        do
                        {
                            startupLogger.LogWarning(
                                "Constraint violation in table {Table} for row {RowId}",
                                reader.GetString(0),  // Table
                                reader.GetInt64(1)    // RowId
                            );
                        } while (await reader.ReadAsync());
                    }
                    else
                    {
                        startupLogger.LogInformation("All foreign key constraints are valid");
                    }
                }

                // Check database size and page stats
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA page_count; PRAGMA page_size;";
                    var pageCount = Convert.ToInt64(await command.ExecuteScalarAsync());
                    command.CommandText = "PRAGMA page_size;";
                    var pageSize = Convert.ToInt64(await command.ExecuteScalarAsync());
                    var dbSizeInMB = (pageCount * pageSize) / (1024.0 * 1024.0);
                    
                    startupLogger.LogInformation(
                        "Database size: {Size:F2} MB (Pages: {Pages:N0}, Page Size: {PageSize:N0} bytes)",
                        dbSizeInMB, pageCount, pageSize
                    );
                }
            }
            
            startupLogger.LogInformation("Database initialization and verification completed successfully.");
            break; // Success - exit the retry loop
        }
        catch (Exception ex)
        {
            retryCount++;
            if (retryCount >= maxRetries)
            {
                startupLogger.LogError(ex, "Failed to initialize database after {RetryCount} attempts. Last error: {Error}", 
                    retryCount, ex.Message);
                throw; // Rethrow after all retries are exhausted
            }
            
            startupLogger.LogWarning(ex, "Database initialization attempt {RetryCount} failed. Retrying...", retryCount);
            await Task.Delay(TimeSpan.FromSeconds(2 * retryCount)); // Exponential backoff
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Configure middleware in the correct order
app.UseRouting();

app.UseHttpsRedirection();

// Configure CORS after routing but before endpoints
app.UseCors("CorsPolicy");

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

app.UseAuthorization();

// Configure endpoints last
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<FlightHub>("/flighthub");
    endpoints.MapHealthChecks("/health");
});

// Log the URLs the server is listening on
var urls = app.Urls.ToList();
if (!urls.Any())
{
    urls.Add("https://localhost:7106");
    urls.Add("http://localhost:5177");
}

foreach (var url in urls)
{
    startupLogger.LogInformation($"Server listening on: {url}");
}

app.Run();

// Add the DatabaseHealthCheck class right after the health check registration
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
                
                // Test basic query
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1;";
                    await command.ExecuteScalarAsync(cancellationToken);
                }
                
                // Test table existence
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