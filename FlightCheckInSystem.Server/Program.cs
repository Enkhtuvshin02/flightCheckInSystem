// FlightCheckInSystem.Server/Program.cs

// ... other using statements ...
using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Business.Services;
using FlightCheckInSystem.Data; // For DatabaseInitializer
using FlightCheckInSystem.Data.Interfaces;
using FlightCheckInSystem.Data.Repositories;
// ...

var builder = WebApplication.CreateBuilder(args);

// --- Define Database Path and Connection String CONSISTENTLY ---
var dbFileName = "flight_checkin_system.db"; // The actual file name

// Path for the database file within the server project's content root
// ContentRootPath is typically the root directory of your Server project.
string dbFilePath = Path.Combine(builder.Environment.ContentRootPath, dbFileName);
Console.WriteLine($"Application ContentRootPath: {builder.Environment.ContentRootPath}");
Console.WriteLine($"Database file path determined as: {dbFilePath}");

string connectionString = $"Data Source={dbFilePath};Version=3;foreign keys=true;";

// --- Dependency Injection ---
// Pass the consistent dbFilePath to DatabaseInitializer
builder.Services.AddSingleton<DatabaseInitializer>(sp => new DatabaseInitializer(dbFilePath));

// Repositories use the connectionString that points to dbFilePath
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>(sp => new PassengerRepository(connectionString));
builder.Services.AddScoped<IFlightRepository, FlightRepository>(sp => new FlightRepository(connectionString));
builder.Services.AddScoped<ISeatRepository, SeatRepository>(sp => new SeatRepository(connectionString));
builder.Services.AddScoped<IBookingRepository, BookingRepository>(sp => new BookingRepository(connectionString));

// ... register your Business services, controllers, WebSocket manager, etc. ...
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IFlightManagementService, FlightManagementService>();
builder.Services.AddSingleton<FlightCheckInSystem.Server.Sockets.WebSocketConnectionManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Initialize the database (MUST be done AFTER builder.Build() and BEFORE app.Run()) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("Program.cs: Attempting to get DatabaseInitializer service.");
        var databaseInitializer = services.GetRequiredService<DatabaseInitializer>();
        Console.WriteLine("Program.cs: DatabaseInitializer service retrieved. Calling InitializeAsync().");
        await databaseInitializer.InitializeAsync(); // Creates schema using the _dbFilePath set in its constructor

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
        // Consider throwing the exception here if the app cannot run without the DB
        // throw;
    }
}
Console.WriteLine("Program.cs: Database initialization step completed.");


// --- Configure the HTTP request pipeline ---
// ... (your app.UseSwagger, UseHttpsRedirection, UseRouting, UseWebSockets, UseEndpoints, etc.)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
var webSocketOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(2) };
app.UseWebSockets(webSocketOptions);
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGet("/ws_checkin", async context =>
    {
        // ... your WebSocket mapping logic ...
        if (context.WebSockets.IsWebSocketRequest)
        {
            var serviceProvider = context.RequestServices;
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var connectionManager = serviceProvider.GetRequiredService<FlightCheckInSystem.Server.Sockets.WebSocketConnectionManager>();
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var handler = new FlightCheckInSystem.Server.Sockets.WebSocketHandler(webSocket, serviceProvider, connectionManager, logger);
            await handler.HandleConnectionAsync();
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    });
});


Console.WriteLine("Program.cs: Starting application (app.Run()).");
app.Run();