using System;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using FlightCheckInSystem.Core.Enums;
using Microsoft.Extensions.Logging;

namespace FlightCheckInSystem.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly string _dbFilePath;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(string dbFilePath, ILogger<DatabaseInitializer> logger = null)
        {
            _dbFilePath = dbFilePath ?? throw new ArgumentNullException(nameof(dbFilePath));
            _connectionString = $"Data Source={_dbFilePath};Foreign Keys=True;";
            _logger = logger;

            EnsureDirectoryExists();
            LogDatabaseInfo();
        }
        private void EnsureDirectoryExists()
        {
            try
            {
                var directory = Path.GetDirectoryName(_dbFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger?.LogInformation("Created database directory: {Directory}", directory);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error ensuring database directory exists");
                throw;
            }
        }

        private void LogDatabaseInfo()
        {
            _logger?.LogInformation("Database file path: {DbFilePath}", _dbFilePath);
            _logger?.LogInformation("Connection string: {ConnectionString}", _connectionString.Replace("Password=***", "[REDACTED]"));
            _logger?.LogInformation("Database file exists: {Exists}", File.Exists(_dbFilePath));
        }

        public async Task InitializeAsync()
        {
            _logger?.LogInformation("Starting database initialization...");
            var sw = Stopwatch.StartNew();

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger?.LogInformation("Successfully opened database connection");

                    // Enable foreign keys and other PRAGMAs
                    await ExecuteNonQueryAsync(connection, "PRAGMA journal_mode=WAL;");
                    await ExecuteNonQueryAsync(connection, "PRAGMA foreign_keys = 1;");
                    await ExecuteNonQueryAsync(connection, "PRAGMA busy_timeout = 30000;");

                    const string createPassengersTable = @"
                    CREATE TABLE IF NOT EXISTS Passengers (
                        PassengerId INTEGER PRIMARY KEY AUTOINCREMENT,
                        PassportNumber TEXT UNIQUE NOT NULL,
                        FirstName TEXT NOT NULL,
                        LastName TEXT NOT NULL,
                        Email TEXT,
                        Phone TEXT,
                        CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                        UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                    );
                    
                    CREATE TRIGGER IF NOT EXISTS UpdatePassengerTimestamp
                    AFTER UPDATE ON Passengers
                    BEGIN
                        UPDATE Passengers SET UpdatedAt = datetime('now') WHERE PassengerId = NEW.PassengerId;
                    END;";

                    string createFlightsTable = @"
                    CREATE TABLE IF NOT EXISTS Flights (
                        FlightId INTEGER PRIMARY KEY AUTOINCREMENT,
                        FlightNumber TEXT NOT NULL,
                        DepartureAirport TEXT NOT NULL,
                        ArrivalAirport TEXT NOT NULL,
                        DepartureTime TEXT NOT NULL,
                        ArrivalTime TEXT NOT NULL,
                        Status TEXT NOT NULL DEFAULT 'Scheduled'
                    );";

                    string createSeatsTable = @"
                    CREATE TABLE IF NOT EXISTS Seats (
                        SeatId INTEGER PRIMARY KEY AUTOINCREMENT,
                        FlightId INTEGER NOT NULL,
                        SeatNumber TEXT NOT NULL,
                        IsBooked BOOLEAN NOT NULL DEFAULT 0,
                        Class TEXT CHECK(Class IN ('Economy', 'Business', 'First')) NOT NULL,
                        Price DECIMAL(10, 2) NOT NULL,
                        UNIQUE(FlightId, SeatNumber),
                        FOREIGN KEY (FlightId) REFERENCES Flights(FlightId) ON DELETE CASCADE
                    );";

                    string createBookingsTable = @"
                    CREATE TABLE IF NOT EXISTS Bookings (
                        BookingId INTEGER PRIMARY KEY AUTOINCREMENT,
                        PassengerId INTEGER NOT NULL,
                        FlightId INTEGER NOT NULL,
                        SeatId INTEGER UNIQUE,
                        ReservationDate TEXT NOT NULL,
                        IsCheckedIn INTEGER NOT NULL DEFAULT 0,
                        CheckInTime TEXT,
                        FOREIGN KEY (PassengerId) REFERENCES Passengers(PassengerId) ON DELETE CASCADE,
                        FOREIGN KEY (FlightId) REFERENCES Flights(FlightId) ON DELETE CASCADE,
                        FOREIGN KEY (SeatId) REFERENCES Seats(SeatId) ON DELETE SET NULL
                    );";

                    // Execute all table creation scripts
                    await using var transaction = await connection.BeginTransactionAsync();
                    try
                    {
                        await ExecuteNonQueryAsync(connection, createPassengersTable, transaction);
                        await ExecuteNonQueryAsync(connection, createFlightsTable, transaction);
                        await ExecuteNonQueryAsync(connection, createSeatsTable, transaction);
                        await ExecuteNonQueryAsync(connection, createBookingsTable, transaction);
                        
                        // Add any indexes
                        await CreateIndexesAsync(connection, transaction);
                        
                        await transaction.CommitAsync();
                        _logger?.LogInformation("Database schema created successfully");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                
                sw.Stop();
                _logger?.LogInformation("Database initialization completed in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing database");
                throw new Exception($"Failed to initialize database: {ex.Message}", ex);
            }
        }
        
        private async Task CreateIndexesAsync(SqliteConnection connection, SqliteTransaction transaction = null)
        {
            try
            {
                // Add any necessary indexes here
                await ExecuteNonQueryAsync(connection, 
                    "CREATE INDEX IF NOT EXISTS IX_Flights_FlightNumber ON Flights(FlightNumber);", 
                    transaction);
                    
                await ExecuteNonQueryAsync(connection, 
                    "CREATE INDEX IF NOT EXISTS IX_Bookings_PassengerId ON Bookings(PassengerId);", 
                    transaction);
                    
                await ExecuteNonQueryAsync(connection, 
                    "CREATE INDEX IF NOT EXISTS IX_Bookings_FlightId ON Bookings(FlightId);", 
                    transaction);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating indexes");
                throw;
            }
        }
        
        private async Task ExecuteInTransactionAsync(SqliteConnection connection, Func<SqliteTransaction, Task<bool>> action)
        {
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var result = await action(transaction);
                if (result)
                {
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        private async Task ExecuteNonQueryAsync(SqliteConnection connection, string commandText, SqliteTransaction? transaction = null)
        {
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = commandText;
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing command: {CommandText}", commandText);
                throw;
            }
        }

        public async Task SeedDataAsync()
        {
            _logger?.LogInformation("Starting database seeding...");
            var sw = Stopwatch.StartNew();

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger?.LogInformation("Connected to database for seeding");

                    await using var transaction = await connection.BeginTransactionAsync();
                    try
                    {
                        // Check if we already have data
                        var checkFlightsCmd = connection.CreateCommand();
                        checkFlightsCmd.CommandText = "SELECT COUNT(*) FROM Flights;";
                        checkFlightsCmd.Transaction = transaction;
                        var flightCount = Convert.ToInt64(await checkFlightsCmd.ExecuteScalarAsync());

                        if (flightCount == 0)
                        {
                            _logger?.LogInformation("No flights found, seeding initial data...");

                            // Flight 1
                            var flight1Departure = DateTime.UtcNow.AddDays(1);
                            var flight1Arrival = flight1Departure.AddHours(3);
                            var flight1Id = await AddFlight(connection, transaction, "OM201", "ULN", "ICN", 
                                flight1Departure, flight1Arrival, FlightStatus.Scheduled.ToString());

                            // Flight 2
                            var flight2Departure = DateTime.UtcNow.AddDays(2);
                            var flight2Arrival = flight2Departure.AddHours(2.5);
                            var flight2Id = await AddFlight(connection, transaction, "OM302", "ULN", "PEK", 
                                flight2Departure, flight2Arrival, FlightStatus.Scheduled.ToString());

                            // Add seats for flight 1
                            if (flight1Id > 0)
                            {
                                await AddSeatsForFlight(connection, transaction, flight1Id, 2, 2);
                                _logger?.LogInformation("Added seats for flight OM201");
                            }

                            // Add seats for flight 2
                            if (flight2Id > 0)
                            {
                                await AddSeatsForFlight(connection, transaction, flight2Id, 3, 3);
                                _logger?.LogInformation("Added seats for flight OM302");
                            }
                            
                            await transaction.CommitAsync();
                            _logger?.LogInformation("Database seeding completed successfully");
                        }
                        else
                        {
                            _logger?.LogInformation("Database already contains data, skipping seeding");
                            await transaction.CommitAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger?.LogError(ex, "Error during database seeding");
                        throw;
                    }
                }
                
                sw.Stop();
                _logger?.LogInformation("Database seeding completed in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error seeding database");
                throw new Exception($"Failed to seed database: {ex.Message}", ex);
            }
        }
        
        private async Task AddSeatsForFlight(SqliteConnection connection, SqliteTransaction transaction, long flightId, int rows, int seatsPerRow)
        {
            try
            {
                var seatLetters = new[] { "A", "B", "C", "D", "E", "F" };
                for (int row = 1; row <= rows; row++)
                {
                    for (int seatIndex = 0; seatIndex < seatsPerRow && seatIndex < seatLetters.Length; seatIndex++)
                    {
                        var seatNumber = $"{row}{seatLetters[seatIndex]}";
                        var seatClass = row <= 2 ? "First" : (row <= 5 ? "Business" : "Economy");
                        var price = seatClass switch
                        {
                            "First" => 1000.00m,
                            "Business" => 500.00m,
                            _ => 200.00m
                        };

                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = @"
                            INSERT INTO Seats (FlightId, SeatNumber, IsBooked, Class, Price)
                            VALUES (@FlightId, @SeatNumber, 0, @Class, @Price);";

                        command.Parameters.AddWithValue("@FlightId", flightId);
                        command.Parameters.AddWithValue("@SeatNumber", seatNumber);
                        command.Parameters.AddWithValue("@Class", seatClass);
                        command.Parameters.AddWithValue("@Price", price);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding seats for flight {FlightId}", flightId);
                throw;
            }
        }

        private async Task<long> AddFlight(SqliteConnection connection, SqliteTransaction transaction, string flightNumber, 
            string departureAirport, string arrivalAirport, DateTime departureTime, DateTime arrivalTime, string status)
        {
            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO Flights (FlightNumber, DepartureAirport, ArrivalAirport, DepartureTime, ArrivalTime, Status)
                    VALUES (@FlightNumber, @DepartureAirport, @ArrivalAirport, @DepartureTime, @ArrivalTime, @Status);
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@FlightNumber", flightNumber);
                command.Parameters.AddWithValue("@DepartureAirport", departureAirport);
                command.Parameters.AddWithValue("@ArrivalAirport", arrivalAirport);
                command.Parameters.AddWithValue("@DepartureTime", departureTime.ToString("O"));
                command.Parameters.AddWithValue("@ArrivalTime", arrivalTime.ToString("O"));
                command.Parameters.AddWithValue("@Status", status);

                var result = await command.ExecuteScalarAsync();
                var flightId = Convert.ToInt64(result);
                _logger?.LogInformation("Added flight {FlightNumber} with ID: {FlightId}", flightNumber, flightId);
                return flightId;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding flight {FlightNumber}", flightNumber);
                throw;
            }
        }
    }
}