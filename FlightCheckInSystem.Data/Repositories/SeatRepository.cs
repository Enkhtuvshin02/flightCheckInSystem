// FlightCheckInSystem.Data/Repositories/SeatRepository.cs
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Data.Interfaces;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Data.Common;

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
                var command = new SQLiteCommand("SELECT * FROM Seats WHERE SeatId = @SeatId", connection);
                command.Parameters.AddWithValue("@SeatId", seatId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return MapToSeat(reader);
                }
            }
            return null;
        }

        public async Task<Seat> GetSeatByFlightAndNumberAsync(int flightId, string seatNumber)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Seats WHERE FlightId = @FlightId AND SeatNumber = @SeatNumber", connection);
                command.Parameters.AddWithValue("@FlightId", flightId);
                command.Parameters.AddWithValue("@SeatNumber", seatNumber);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return MapToSeat(reader);
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
                var command = new SQLiteCommand("SELECT * FROM Seats WHERE FlightId = @FlightId ORDER BY SeatNumber", connection);
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
                var command = new SQLiteCommand("SELECT * FROM Seats WHERE FlightId = @FlightId AND IsBooked = 0 ORDER BY SeatNumber", connection);
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

        public async Task<bool> BookSeatAsync(int seatId, int bookingId)
        {
            // This method updates the seat's IsBooked status.
            // The booking itself is updated in BookingRepository to link to this seatId.
            // The 'bookingId' parameter here is for logging or future referential integrity if needed,
            // but the primary update for linking booking to seat happens in BookingRepository.
            // The critical part is ensuring IsBooked is 0 before updating.
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("UPDATE Seats SET IsBooked = 1 WHERE SeatId = @SeatId AND IsBooked = 0", connection);
                command.Parameters.AddWithValue("@SeatId", seatId);
                // We don't store bookingId directly in Seats table in this simplified model,
                // but it is good practice to have it if you want a direct link or for audit.
                // For now, it just sets IsBooked = 1.
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> UnbookSeatAsync(int seatId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("UPDATE Seats SET IsBooked = 0 WHERE SeatId = @SeatId", connection);
                command.Parameters.AddWithValue("@SeatId", seatId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        private Seat MapToSeat(DbDataReader reader)
        {
            return new Seat
            {
                SeatId = reader.GetInt32(reader.GetOrdinal("SeatId")),
                FlightId = reader.GetInt32(reader.GetOrdinal("FlightId")),
                SeatNumber = reader.GetString(reader.GetOrdinal("SeatNumber")),
                IsBooked = reader.GetInt32(reader.GetOrdinal("IsBooked")) == 1
            };
        }
    }
}