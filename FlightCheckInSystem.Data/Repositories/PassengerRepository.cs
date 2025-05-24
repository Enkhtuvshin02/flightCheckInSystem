// FlightCheckInSystem.Data/Repositories/PassengerRepository.cs
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Data.Interfaces;
using System.Data.SQLite;
using System.Threading.Tasks;
using System; // For Convert
using System.Data.Common;

namespace FlightCheckInSystem.Data.Repositories
{
    public class PassengerRepository : BaseRepository, IPassengerRepository
    {
        public PassengerRepository(string connectionString) : base(connectionString) { }

        public async Task<Passenger> GetPassengerByIdAsync(int passengerId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT PassengerId, PassportNumber, FirstName, LastName FROM Passengers WHERE PassengerId = @PassengerId", connection);
                command.Parameters.AddWithValue("@PassengerId", passengerId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return MapToPassenger(reader);
                }
            }
            return null;
        }

        public async Task<Passenger> GetPassengerByPassportAsync(string passportNumber)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT PassengerId, PassportNumber, FirstName, LastName FROM Passengers WHERE PassportNumber = @PassportNumber", connection);
                command.Parameters.AddWithValue("@PassportNumber", passportNumber);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return MapToPassenger(reader);
                }
            }
            return null;
        }

        public async Task<int> AddPassengerAsync(Passenger passenger)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("INSERT INTO Passengers (PassportNumber, FirstName, LastName) VALUES (@PassportNumber, @FirstName, @LastName); SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@PassportNumber", passenger.PassportNumber);
                command.Parameters.AddWithValue("@FirstName", passenger.FirstName);
                command.Parameters.AddWithValue("@LastName", passenger.LastName);
                var newId = await command.ExecuteScalarAsync();
                return Convert.ToInt32(newId);
            }
        }

        public async Task<bool> UpdatePassengerAsync(Passenger passenger)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("UPDATE Passengers SET PassportNumber = @PassportNumber, FirstName = @FirstName, LastName = @LastName WHERE PassengerId = @PassengerId", connection);
                command.Parameters.AddWithValue("@PassportNumber", passenger.PassportNumber);
                command.Parameters.AddWithValue("@FirstName", passenger.FirstName);
                command.Parameters.AddWithValue("@LastName", passenger.LastName);
                command.Parameters.AddWithValue("@PassengerId", passenger.PassengerId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> DeletePassengerAsync(int passengerId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("DELETE FROM Passengers WHERE PassengerId = @PassengerId", connection);
                command.Parameters.AddWithValue("@PassengerId", passengerId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        private Passenger MapToPassenger(DbDataReader reader)
        {
            return new Passenger
            {
                PassengerId = reader.GetInt32(reader.GetOrdinal("PassengerId")),
                PassportNumber = reader.GetString(reader.GetOrdinal("PassportNumber")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName"))
            };
        }
    }
}