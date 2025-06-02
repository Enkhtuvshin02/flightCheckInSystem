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
                var command = new SQLiteCommand("SELECT PassengerId, PassportNumber, FirstName, LastName, Email, Phone FROM Passengers WHERE PassportNumber = @PassportNumber", connection);
                command.Parameters.AddWithValue("@PassportNumber", passportNumber);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return MapToPassenger(reader);
                }
            }
            return null;
        }

        public async Task<Passenger> GetPassengerByPassportNumberAsync(string passportNumber)
        {
            return await GetPassengerByPassportAsync(passportNumber);
        }

        public async Task<Passenger> CreatePassengerAsync(Passenger passenger)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var command = new SQLiteCommand("INSERT INTO Passengers (PassportNumber, FirstName, LastName, Email, Phone) VALUES (@PassportNumber, @FirstName, @LastName, @Email, @Phone); SELECT last_insert_rowid();", connection, transaction);
                        command.Parameters.AddWithValue("@PassportNumber", passenger.PassportNumber);
                        command.Parameters.AddWithValue("@FirstName", passenger.FirstName);
                        command.Parameters.AddWithValue("@LastName", passenger.LastName);
                        command.Parameters.AddWithValue("@Email", passenger.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Phone", passenger.Phone ?? (object)DBNull.Value);

                        var passengerId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        passenger.PassengerId = passengerId;

                        transaction.Commit();
                        return passenger;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private Passenger MapToPassenger(DbDataReader reader)
        {
            var passenger = new Passenger
            {
                PassengerId = reader.GetInt32(reader.GetOrdinal("PassengerId")),
                PassportNumber = reader.GetString(reader.GetOrdinal("PassportNumber")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName"))
            };

            // Check if Email and Phone columns exist in the result set
            try
            {
                var emailOrdinal = reader.GetOrdinal("Email");
                if (!reader.IsDBNull(emailOrdinal))
                    passenger.Email = reader.GetString(emailOrdinal);
            }
            catch (IndexOutOfRangeException) {  }

            try
            {
                var phoneOrdinal = reader.GetOrdinal("Phone");
                if (!reader.IsDBNull(phoneOrdinal))
                    passenger.Phone = reader.GetString(phoneOrdinal);
            }
            catch (IndexOutOfRangeException) {  }

            return passenger;
        }
        public async Task<int> AddPassengerAsync(Passenger passenger)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("INSERT INTO Passengers (PassportNumber, FirstName, LastName, Email, Phone) VALUES (@PassportNumber, @FirstName, @LastName, @Email, @Phone); SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@PassportNumber", passenger.PassportNumber);
                command.Parameters.AddWithValue("@FirstName", passenger.FirstName);
                command.Parameters.AddWithValue("@LastName", passenger.LastName);
                command.Parameters.AddWithValue("@Email", (object?)passenger.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object?)passenger.Phone ?? DBNull.Value);
                var passengerId = await command.ExecuteScalarAsync();
                return Convert.ToInt32(passengerId);
            }
        }

    }
}