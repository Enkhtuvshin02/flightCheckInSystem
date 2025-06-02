// FlightCheckInSystem.Data/DatabaseInitializer.cs
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using FlightCheckInSystem.Core.Enums; // For FlightStatus

namespace FlightCheckInSystem.Data
{
    public class DatabaseInitializer
    {
        
            private readonly string _connectionString;
            private readonly string _dbFilePath; // Store the full file path

        public DatabaseInitializer(string dbFilePath)
        {
            _dbFilePath = dbFilePath;
            _connectionString = $"Data Source={_dbFilePath};Version=3;foreign keys=true;";

            // 1. Ensure the DIRECTORY for the database file exists.
            string directoryPath = Path.GetDirectoryName(_dbFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"DatabaseInitializer: Created directory at {directoryPath}");
            }

            // 2. Create the database FILE only if it doesn't already exist.
            if (!File.Exists(_dbFilePath))
            {
                SQLiteConnection.CreateFile(_dbFilePath);
                Console.WriteLine($"DatabaseInitializer: Created database file at {_dbFilePath}");
            }
            else
            {
                Console.WriteLine($"DatabaseInitializer: Database file already exists at {_dbFilePath}");
            }
        }

        public async Task InitializeAsync()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string createPassengersTable = @"
                CREATE TABLE IF NOT EXISTS Passengers (
                    PassengerId INTEGER PRIMARY KEY AUTOINCREMENT,
                    PassportNumber TEXT UNIQUE NOT NULL,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT,
                    Phone TEXT
                );";

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
                    IsBooked INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (FlightId) REFERENCES Flights(FlightId) ON DELETE CASCADE,
                    UNIQUE(FlightId, SeatNumber)
                );";

                string createBookingsTable = @"
                CREATE TABLE IF NOT EXISTS Bookings (
                    BookingId INTEGER PRIMARY KEY AUTOINCREMENT,
                    PassengerId INTEGER NOT NULL,
                    FlightId INTEGER NOT NULL,
                    SeatId INTEGER UNIQUE, -- A seat can only be in one booking
                    ReservationDate TEXT NOT NULL,
                    IsCheckedIn INTEGER NOT NULL DEFAULT 0,
                    CheckInTime TEXT,
                    FOREIGN KEY (PassengerId) REFERENCES Passengers(PassengerId) ON DELETE CASCADE,
                    FOREIGN KEY (FlightId) REFERENCES Flights(FlightId) ON DELETE CASCADE,
                    FOREIGN KEY (SeatId) REFERENCES Seats(SeatId) ON DELETE SET NULL, -- If seat is deleted, set SeatId to NULL
                    UNIQUE(PassengerId, FlightId) -- A passenger can only book a specific flight once
                );";

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "PRAGMA foreign_keys = ON;"; // Ensure foreign keys are enforced per connection
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = createPassengersTable;
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = createFlightsTable;
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = createSeatsTable;
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = createBookingsTable;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SeedDataAsync()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Only seed if Flights table is empty
                var checkFlightsCmd = new SQLiteCommand("SELECT COUNT(*) FROM Flights;", connection);
                var flightCount = Convert.ToInt32(await checkFlightsCmd.ExecuteScalarAsync());
                if (flightCount == 0)
                {
             
                // Seed Flights
                var flightCmdText = @"
                    INSERT OR IGNORE INTO Flights (FlightNumber, DepartureAirport, ArrivalAirport, DepartureTime, ArrivalTime, Status) VALUES 
                    (@FlightNumber, @DepartureAirport, @ArrivalAirport, @DepartureTime, @ArrivalTime, @Status);
                    SELECT last_insert_rowid();"; // Get FlightId

                var flight1Departure = DateTime.UtcNow.AddDays(1).ToString("o"); // ISO 8601
                var flight1Arrival = DateTime.UtcNow.AddDays(1).AddHours(3).ToString("o");

                long flight1Id;
                using (var flightCmd1 = new SQLiteCommand(flightCmdText, connection))
                {
                    flightCmd1.Parameters.AddWithValue("@FlightNumber", "OM201");
                    flightCmd1.Parameters.AddWithValue("@DepartureAirport", "ULN");
                    flightCmd1.Parameters.AddWithValue("@ArrivalAirport", "ICN");
                    flightCmd1.Parameters.AddWithValue("@DepartureTime", flight1Departure);
                    flightCmd1.Parameters.AddWithValue("@ArrivalTime", flight1Arrival);
                    flightCmd1.Parameters.AddWithValue("@Status", FlightStatus.Scheduled.ToString());
                    flight1Id = (long)await flightCmd1.ExecuteScalarAsync();
                }

                var flight2Departure = DateTime.UtcNow.AddDays(2).ToString("o");
                var flight2Arrival = DateTime.UtcNow.AddDays(2).AddHours(2).ToString("o");
                long flight2Id;
                using (var flightCmd2 = new SQLiteCommand(flightCmdText, connection))
                {
                    flightCmd2.Parameters.AddWithValue("@FlightNumber", "OM302");
                    flightCmd2.Parameters.AddWithValue("@DepartureAirport", "ULN");
                    flightCmd2.Parameters.AddWithValue("@ArrivalAirport", "PEK");
                    flightCmd2.Parameters.AddWithValue("@DepartureTime", flight2Departure);
                    flightCmd2.Parameters.AddWithValue("@ArrivalTime", flight2Arrival);
                    flightCmd2.Parameters.AddWithValue("@Status", FlightStatus.Scheduled.ToString());
                    flight2Id = (long)await flightCmd2.ExecuteScalarAsync();
                }


                // Seed Seats for Flight 1 (e.g., 2 rows, A-B)
                if (flight1Id > 0)
                {
                    for (int row = 1; row <= 2; row++)
                    {
                        for (char seatLetter = 'A'; seatLetter <= 'B'; seatLetter++)
                        {
                            var seatCmd = new SQLiteCommand("INSERT OR IGNORE INTO Seats (FlightId, SeatNumber, IsBooked) VALUES (@FlightId, @SeatNumber, 0)", connection);
                            seatCmd.Parameters.AddWithValue("@FlightId", flight1Id);
                            seatCmd.Parameters.AddWithValue("@SeatNumber", $"{row}{seatLetter}");
                            await seatCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                // Seed Seats for Flight 2 (e.g., 3 rows, A-C)
                if (flight2Id > 0)
                {
                    for (int row = 1; row <= 3; row++)
                    {
                        for (char seatLetter = 'A'; seatLetter <= 'C'; seatLetter++)
                        {
                            var seatCmd = new SQLiteCommand("INSERT OR IGNORE INTO Seats (FlightId, SeatNumber, IsBooked) VALUES (@FlightId, @SeatNumber, 0)", connection);
                            seatCmd.Parameters.AddWithValue("@FlightId", flight2Id);
                            seatCmd.Parameters.AddWithValue("@SeatNumber", $"{row}{seatLetter}");
                            await seatCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                }
            }
        }
    }
}