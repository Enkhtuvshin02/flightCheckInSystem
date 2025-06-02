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
        public async Task<bool> BookSeatAsync(int seatId, int bookingId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("UPDATE Seats SET IsBooked = 1 WHERE SeatId = @SeatId", connection);
                command.Parameters.AddWithValue("@SeatId", seatId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

    }
}