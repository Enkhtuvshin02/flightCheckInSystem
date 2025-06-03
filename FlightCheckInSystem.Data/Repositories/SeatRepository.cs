using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Data.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace FlightCheckInSystem.Data.Repositories
{
    public class SeatRepository : BaseRepository, ISeatRepository
    {
        public SeatRepository(string connectionString) : base(connectionString) { }

        public async Task<Seat> GetSeatByIdAsync(int seatId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand("SELECT * FROM Seats WHERE SeatId = @SeatId", connection);
                command.Parameters.AddWithValue("@SeatId", seatId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapToSeat(reader);
                    }
                }
            }
            return null;
        }

        public async Task<Seat> GetSeatByNumberAndFlightAsync(string seatNumber, int flightId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand("SELECT * FROM Seats WHERE SeatNumber = @SeatNumber AND FlightId = @FlightId", connection);
                command.Parameters.AddWithValue("@SeatNumber", seatNumber);
                command.Parameters.AddWithValue("@FlightId", flightId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapToSeat(reader);
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Seat>> GetSeatsByFlightIdAsync(int flightId)
        {
            var seats = new List<Seat>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand("SELECT * FROM Seats WHERE FlightId = @FlightId ORDER BY SeatNumber", connection);
                command.Parameters.AddWithValue("@FlightId", flightId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        seats.Add(MapToSeat(reader));
                    }
                }
            }
            return seats;
        }

        public async Task<IEnumerable<Seat>> GetAvailableSeatsByFlightIdAsync(int flightId)
        {
            var seats = new List<Seat>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand("SELECT * FROM Seats WHERE FlightId = @FlightId AND IsBooked = 0 ORDER BY SeatNumber", connection);
                command.Parameters.AddWithValue("@FlightId", flightId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        seats.Add(MapToSeat(reader));
                    }
                }
            }
            return seats;
        }

        public async Task<bool> UpdateSeatAsync(Seat seat)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand(@"
                    UPDATE Seats 
                    SET IsBooked = @IsBooked, Class = @Class, Price = @Price
                    WHERE SeatId = @SeatId", connection);

                command.Parameters.AddWithValue("@IsBooked", seat.IsBooked);
                command.Parameters.AddWithValue("@Class", seat.Class ?? "Economy");
                command.Parameters.AddWithValue("@Price", seat.Price);
                command.Parameters.AddWithValue("@SeatId", seat.SeatId);

                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> BookSeatAsync(int seatId, int bookingId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check if seat is available
                        var checkCommand = new SqliteCommand("SELECT IsBooked FROM Seats WHERE SeatId = @SeatId", connection, transaction);
                        checkCommand.Parameters.AddWithValue("@SeatId", seatId);
                        var isBooked = Convert.ToBoolean(await checkCommand.ExecuteScalarAsync());

                        if (isBooked)
                        {
                            transaction.Rollback();
                            return false; // Seat already booked
                        }

                        // Book the seat
                        var updateCommand = new SqliteCommand("UPDATE Seats SET IsBooked = 1 WHERE SeatId = @SeatId", connection, transaction);
                        updateCommand.Parameters.AddWithValue("@SeatId", seatId);
                        await updateCommand.ExecuteNonQueryAsync();

                        // Update booking with seat information
                        var bookingCommand = new SqliteCommand("UPDATE Bookings SET SeatId = @SeatId WHERE BookingId = @BookingId", connection, transaction);
                        bookingCommand.Parameters.AddWithValue("@SeatId", seatId);
                        bookingCommand.Parameters.AddWithValue("@BookingId", bookingId);
                        await bookingCommand.ExecuteNonQueryAsync();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ReleaseSeatAsync(int seatId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Release the seat
                        var updateSeatCommand = new SqliteCommand("UPDATE Seats SET IsBooked = 0 WHERE SeatId = @SeatId", connection, transaction);
                        updateSeatCommand.Parameters.AddWithValue("@SeatId", seatId);
                        var seatUpdated = await updateSeatCommand.ExecuteNonQueryAsync() > 0;

                        // Remove seat from any bookings
                        var updateBookingCommand = new SqliteCommand("UPDATE Bookings SET SeatId = NULL WHERE SeatId = @SeatId", connection, transaction);
                        updateBookingCommand.Parameters.AddWithValue("@SeatId", seatId);
                        await updateBookingCommand.ExecuteNonQueryAsync();

                        transaction.Commit();
                        return seatUpdated;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> BookSeatByNumberAsync(string seatNumber, int flightId, int bookingId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Get seat ID and check availability
                        var getSeatCommand = new SqliteCommand("SELECT SeatId, IsBooked FROM Seats WHERE SeatNumber = @SeatNumber AND FlightId = @FlightId", connection, transaction);
                        getSeatCommand.Parameters.AddWithValue("@SeatNumber", seatNumber);
                        getSeatCommand.Parameters.AddWithValue("@FlightId", flightId);

                        using (var reader = await getSeatCommand.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                            {
                                transaction.Rollback();
                                return false; // Seat not found
                            }

                            var seatId = reader.GetInt32("SeatId");
                            var isBooked = reader.GetBoolean("IsBooked");

                            if (isBooked)
                            {
                                transaction.Rollback();
                                return false; // Seat already booked
                            }

                            reader.Close();

                            // Book the seat
                            var updateCommand = new SqliteCommand("UPDATE Seats SET IsBooked = 1 WHERE SeatId = @SeatId", connection, transaction);
                            updateCommand.Parameters.AddWithValue("@SeatId", seatId);
                            await updateCommand.ExecuteNonQueryAsync();

                            // Update booking with seat information
                            var bookingCommand = new SqliteCommand("UPDATE Bookings SET SeatId = @SeatId WHERE BookingId = @BookingId", connection, transaction);
                            bookingCommand.Parameters.AddWithValue("@SeatId", seatId);
                            bookingCommand.Parameters.AddWithValue("@BookingId", bookingId);
                            await bookingCommand.ExecuteNonQueryAsync();

                            transaction.Commit();
                            return true;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<int> AddSeatAsync(Seat seat)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand(@"
                    INSERT INTO Seats (FlightId, SeatNumber, IsBooked, Class, Price) 
                    VALUES (@FlightId, @SeatNumber, @IsBooked, @Class, @Price);
                    SELECT last_insert_rowid();", connection);

                command.Parameters.AddWithValue("@FlightId", seat.FlightId);
                command.Parameters.AddWithValue("@SeatNumber", seat.SeatNumber);
                command.Parameters.AddWithValue("@IsBooked", seat.IsBooked);
                command.Parameters.AddWithValue("@Class", seat.Class ?? "Economy");
                command.Parameters.AddWithValue("@Price", seat.Price);

                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        public async Task<bool> UnbookSeatAsync(int seatId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqliteCommand("UPDATE Seats SET IsBooked = 0 WHERE SeatId = @SeatId", connection);
                command.Parameters.AddWithValue("@SeatId", seatId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        private Seat MapToSeat(DbDataReader reader)
        {
            return new Seat
            {
                SeatId = reader.GetInt32("SeatId"),
                FlightId = reader.GetInt32("FlightId"),
                SeatNumber = reader.GetString("SeatNumber"),
                IsBooked = reader.GetBoolean("IsBooked"),
                Class = reader.IsDBNull(reader.GetOrdinal("Class")) ? "Economy" : reader.GetString("Class"),
                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : reader.GetDecimal("Price")
            };
        }
    }
}