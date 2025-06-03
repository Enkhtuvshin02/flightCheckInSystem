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

// Initialize SQLitePCL
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var dbFileName = "flight_checkin_system.db";
string dbFilePath = Path.Combine(builder.Environment.ContentRootPath, dbFileName);

// Log the database file path
var startupLogger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddDebug();
}).CreateLogger("Program");

startupLogger.LogInformation("Application starting...");
startupLogger.LogInformation("Content Root Path: {ContentRootPath}", builder.Environment.ContentRootPath);
startupLogger.LogInformation("Database file path: {DbFilePath}", dbFilePath);

string connectionString = $"Data Source={dbFilePath};Foreign Keys=True;Cache=Shared";

// Register DatabaseInitializer with DI
builder.Services.AddSingleton<DatabaseInitializer>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<DatabaseInitializer>>();
    return new DatabaseInitializer(dbFilePath, logger);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

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
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowed(origin => true);
    });
});

builder.Services.AddSingleton<DatabaseInitializer>(sp => new DatabaseInitializer(dbFilePath));
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>(sp => new PassengerRepository(connectionString));
builder.Services.AddScoped<IFlightRepository, FlightRepository>(sp => new FlightRepository(connectionString));
builder.Services.AddScoped<ISeatRepository, SeatRepository>(sp => new SeatRepository(connectionString));
builder.Services.AddScoped<IBookingRepository, BookingRepository>(sp => new BookingRepository(connectionString));

builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IFlightManagementService, FlightManagementService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck("Database", () => {
        try {
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1;";
                    command.ExecuteScalar();
                }
            }
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("Program.cs: Attempting to get DatabaseInitializer service.");
        var databaseInitializer = services.GetRequiredService<DatabaseInitializer>();
        Console.WriteLine("Program.cs: DatabaseInitializer service retrieved. Calling InitializeAsync().");
        await databaseInitializer.InitializeAsync();

        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("Program.cs: Development environment. Calling SeedDataAsync().");
            await databaseInitializer.SeedDataAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the DB.");
        Console.WriteLine($"Program.cs: ERROR during DB initialization: {ex.Message}");
    }
}
Console.WriteLine("Program.cs: Database initialization step completed.");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
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

var urls = string.Join(", ", app.Urls);
Console.WriteLine($"Program.cs: Starting application (app.Run()) on URLs: {urls}");
Console.WriteLine("IMPORTANT: Make sure your client is using the correct URL: https://localhost:5001");
app.Run();