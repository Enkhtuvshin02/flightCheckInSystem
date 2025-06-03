using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Data.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Threading.Tasks;
using System.Data.Common;

namespace FlightCheckInSystem.Data.Repositories
{
    public class FlightRepository : BaseRepository, IFlightRepository
    {
        public FlightRepository(string connectionString) : base(connectionString) { }

        public async Task<Flight> GetFlightByIdAsync(int flightId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand("SELECT * FROM Flights WHERE FlightId = @FlightId", connection);
                command.Parameters.AddWithValue("@FlightId", flightId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return MapToFlight(reader);
                }
            }
            return null;
        }

        public async Task<IEnumerable<Flight>> GetAllFlightsAsync()
        {
            var flights = new List<Flight>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand("SELECT * FROM Flights ORDER BY DepartureTime", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        flights.Add(MapToFlight(reader));
                    }
                }
            }
            return flights;
        }

        public async Task<int> AddFlightAsync(Flight flight)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand(@"
                    INSERT INTO Flights (FlightNumber, DepartureAirport, ArrivalAirport, DepartureTime, ArrivalTime, Status) 
                    VALUES (@FlightNumber, @DepartureAirport, @ArrivalAirport, @DepartureTime, @ArrivalTime, @Status);
                    SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@FlightNumber", flight.FlightNumber);
                command.Parameters.AddWithValue("@DepartureAirport", flight.DepartureAirport);
                command.Parameters.AddWithValue("@ArrivalAirport", flight.ArrivalAirport);
                command.Parameters.AddWithValue("@DepartureTime", flight.DepartureTime.ToString("o"));                 command.Parameters.AddWithValue("@ArrivalTime", flight.ArrivalTime.ToString("o"));                   command.Parameters.AddWithValue("@Status", flight.Status.ToString());
                var newId = await command.ExecuteScalarAsync();
                return Convert.ToInt32(newId);
            }
        }

        public async Task CreateFlightWithSeatsAsync(Flight flight, int totalRows, char lastSeatLetterInRow)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                                                var flightCommand = new SqliteCommand(@"
                            INSERT INTO Flights (FlightNumber, DepartureAirport, ArrivalAirport, DepartureTime, ArrivalTime, Status) 
                            VALUES (@FlightNumber, @DepartureAirport, @ArrivalAirport, @DepartureTime, @ArrivalTime, @Status);
                            SELECT last_insert_rowid();", connection, transaction);
                        flightCommand.Parameters.AddWithValue("@FlightNumber", flight.FlightNumber);
                        flightCommand.Parameters.AddWithValue("@DepartureAirport", flight.DepartureAirport);
                        flightCommand.Parameters.AddWithValue("@ArrivalAirport", flight.ArrivalAirport);
                        flightCommand.Parameters.AddWithValue("@DepartureTime", flight.DepartureTime.ToString("o"));
                        flightCommand.Parameters.AddWithValue("@ArrivalTime", flight.ArrivalTime.ToString("o"));
                        flightCommand.Parameters.AddWithValue("@Status", flight.Status.ToString());
                        var flightId = Convert.ToInt32(await flightCommand.ExecuteScalarAsync());
                        flight.FlightId = flightId;

                                                var seatCommand = new SqliteCommand("INSERT INTO Seats (FlightId, SeatNumber, IsBooked) VALUES (@FlightId, @SeatNumber, 0)", connection, transaction);
                        seatCommand.Parameters.AddWithValue("@FlightId", flightId);
                        seatCommand.Parameters.Add("@SeatNumber", Microsoft.Data.Sqlite.SqliteType.Text);

                        for (int row = 1; row <= totalRows; row++)
                        {
                            for (char seatLetter = 'A'; seatLetter <= lastSeatLetterInRow; seatLetter++)
                            {
                                seatCommand.Parameters["@SeatNumber"].Value = $"{row}{seatLetter}";
                                await seatCommand.ExecuteNonQueryAsync();
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public async Task<bool> UpdateFlightAsync(Flight flight)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand(@"
                    UPDATE Flights SET FlightNumber = @FlightNumber, DepartureAirport = @DepartureAirport, 
                    ArrivalAirport = @ArrivalAirport, DepartureTime = @DepartureTime, ArrivalTime = @ArrivalTime, Status = @Status
                    WHERE FlightId = @FlightId", connection);
                command.Parameters.AddWithValue("@FlightNumber", flight.FlightNumber);
                command.Parameters.AddWithValue("@DepartureAirport", flight.DepartureAirport);
                command.Parameters.AddWithValue("@ArrivalAirport", flight.ArrivalAirport);
                command.Parameters.AddWithValue("@DepartureTime", flight.DepartureTime.ToString("o"));
                command.Parameters.AddWithValue("@ArrivalTime", flight.ArrivalTime.ToString("o"));
                command.Parameters.AddWithValue("@Status", flight.Status.ToString());
                command.Parameters.AddWithValue("@FlightId", flight.FlightId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        private Flight MapToFlight(DbDataReader reader)
        {
            return new Flight
            {
                FlightId = reader.GetInt32(reader.GetOrdinal("FlightId")),
                FlightNumber = reader.GetString(reader.GetOrdinal("FlightNumber")),
                DepartureAirport = reader.GetString(reader.GetOrdinal("DepartureAirport")),
                ArrivalAirport = reader.GetString(reader.GetOrdinal("ArrivalAirport")),
                DepartureTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("DepartureTime")), null, DateTimeStyles.RoundtripKind),
                ArrivalTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("ArrivalTime")), null, DateTimeStyles.RoundtripKind),
                Status = Enum.Parse<FlightStatus>(reader.GetString(reader.GetOrdinal("Status")))
            };
        }
        public async Task<bool> UpdateFlightStatusAsync(int flightId, FlightStatus newStatus)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand("UPDATE Flights SET Status = @Status WHERE FlightId = @FlightId", connection);
                command.Parameters.AddWithValue("@Status", newStatus.ToString());
                command.Parameters.AddWithValue("@FlightId", flightId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

    }
}