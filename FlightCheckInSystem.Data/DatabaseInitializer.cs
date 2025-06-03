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
        private readonly ILogger<DatabaseInitializer>? _logger;

        public DatabaseInitializer(string dbFilePath, ILogger<DatabaseInitializer>? logger = null)
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
                await using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                _logger?.LogInformation("Successfully opened database connection");

                // Enable foreign keys
                await using var pragmaCmd = connection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
                await pragmaCmd.ExecuteNonQueryAsync();

                // Create tables
                //test
                using var transaction = connection.BeginTransaction();
                try
                {
                    // Create tables
                    await using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Passengers (
                            PassengerId INTEGER PRIMARY KEY AUTOINCREMENT,
                            PassportNumber TEXT UNIQUE NOT NULL,
                            FirstName TEXT NOT NULL,
                            LastName TEXT NOT NULL,
                            Email TEXT,
                            Phone TEXT
                        );

                        CREATE TABLE IF NOT EXISTS Flights (
                            FlightId INTEGER PRIMARY KEY AUTOINCREMENT,
                            FlightNumber TEXT NOT NULL,
                            DepartureAirport TEXT NOT NULL,
                            ArrivalAirport TEXT NOT NULL,
                            DepartureTime TEXT NOT NULL,
                            ArrivalTime TEXT NOT NULL,
                            Status TEXT NOT NULL DEFAULT 'Scheduled'
                        );

                        CREATE TABLE IF NOT EXISTS Seats (
                            SeatId INTEGER PRIMARY KEY AUTOINCREMENT,
                            FlightId INTEGER NOT NULL,
                            SeatNumber TEXT NOT NULL,
                            IsBooked BOOLEAN NOT NULL DEFAULT 0,
                            Class TEXT CHECK(Class IN ('Economy', 'Business', 'First')) NOT NULL,
                            Price DECIMAL(10, 2) NOT NULL,
                            UNIQUE(FlightId, SeatNumber),
                            FOREIGN KEY (FlightId) REFERENCES Flights(FlightId) ON DELETE CASCADE
                        );

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
                        );

                        CREATE INDEX IF NOT EXISTS IX_Flights_FlightNumber ON Flights(FlightNumber);
                        CREATE INDEX IF NOT EXISTS IX_Bookings_PassengerId ON Bookings(PassengerId);
                        CREATE INDEX IF NOT EXISTS IX_Bookings_FlightId ON Bookings(FlightId);";

                    await cmd.ExecuteNonQueryAsync();
                    transaction.Commit();
                    _logger?.LogInformation("Database schema created successfully");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger?.LogError(ex, "Error creating database schema");
                    throw;
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
        
        private async Task CreateIndexesAsync(SqliteConnection connection, SqliteTransaction? transaction = null)
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
                await using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                _logger?.LogInformation("Connected to database for seeding");

                using var transaction = connection.BeginTransaction();
                try
                {
                    // First check if we have any flights
                    var checkCommand = connection.CreateCommand();
                    checkCommand.Transaction = transaction;
                    checkCommand.CommandText = "SELECT COUNT(*) FROM Flights;";
                    var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                    if (count == 0)
                    {
                        _logger?.LogInformation("No flights found, seeding initial flight data...");
                        
                        // Add initial flight
                        var flightCommand = connection.CreateCommand();
                        flightCommand.Transaction = transaction;
                        flightCommand.CommandText = @"
                            INSERT INTO Flights (FlightNumber, DepartureAirport, ArrivalAirport, DepartureTime, ArrivalTime, Status)
                            VALUES (@FlightNumber, @DepartureAirport, @ArrivalAirport, @DepartureTime, @ArrivalTime, @Status);
                            SELECT last_insert_rowid();";

                        var departure = DateTime.UtcNow.AddDays(1);
                        flightCommand.Parameters.AddWithValue("@FlightNumber", "OM201");
                        flightCommand.Parameters.AddWithValue("@DepartureAirport", "ULN");
                        flightCommand.Parameters.AddWithValue("@ArrivalAirport", "ICN");
                        flightCommand.Parameters.AddWithValue("@DepartureTime", departure.ToString("O"));
                        flightCommand.Parameters.AddWithValue("@ArrivalTime", departure.AddHours(3).ToString("O"));
                        flightCommand.Parameters.AddWithValue("@Status", "Scheduled");

                        var flightId = Convert.ToInt32(await flightCommand.ExecuteScalarAsync());
                        _logger?.LogInformation("Created flight with ID: {FlightId}", flightId);

                        // Add seats for the flight
                        var seatCommand = connection.CreateCommand();
                        seatCommand.Transaction = transaction;
                        seatCommand.CommandText = @"
                            INSERT INTO Seats (FlightId, SeatNumber, IsBooked, Class, Price)
                            VALUES (@FlightId, @SeatNumber, 0, @Class, @Price);";
                        seatCommand.Parameters.AddWithValue("@FlightId", flightId);
                        seatCommand.Parameters.Add("@SeatNumber", Microsoft.Data.Sqlite.SqliteType.Text);
                        seatCommand.Parameters.Add("@Class", Microsoft.Data.Sqlite.SqliteType.Text);
                        seatCommand.Parameters.Add("@Price", Microsoft.Data.Sqlite.SqliteType.Real);

                        // Create seats for each row
                        for (int row = 1; row <= 20; row++) // 20 rows total
                        {
                            string seatClass;
                            decimal price;
                            
                            if (row <= 2) // First 2 rows are First Class
                            {
                                seatClass = "First";
                                price = 1000.00m;
                            }
                            else if (row <= 6) // Next 4 rows are Business Class
                            {
                                seatClass = "Business";
                                price = 500.00m;
                            }
                            else // Remaining rows are Economy
                            {
                                seatClass = "Economy";
                                price = 200.00m;
                            }

                            // Create seats A through F for each row
                            for (char seatLetter = 'A'; seatLetter <= 'F'; seatLetter++)
                            {
                                seatCommand.Parameters["@SeatNumber"].Value = $"{row}{seatLetter}";
                                seatCommand.Parameters["@Class"].Value = seatClass;
                                seatCommand.Parameters["@Price"].Value = price;
                                await seatCommand.ExecuteNonQueryAsync();
                            }
                        }

                        _logger?.LogInformation("Created seats for flight {FlightId}", flightId);
                    }

                    transaction.Commit();
                    _logger?.LogInformation("Database seeding completed");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger?.LogError(ex, "Error seeding database");
                    throw;
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
        
        private async Task<long> AddFlight(SqliteConnection connection, SqliteTransaction? transaction, string flightNumber, 
            string departureAirport, string arrivalAirport, DateTime departureTime, DateTime arrivalTime, string status)
        {
            try
            {
                var command = connection.CreateCommand();
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
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

        private async Task AddSeatsForFlight(SqliteConnection connection, SqliteTransaction? transaction, long flightId, int rows, int seatsPerRow)
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
                        if (transaction != null)
                        {
                            command.Transaction = transaction;
                        }
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
    }
}