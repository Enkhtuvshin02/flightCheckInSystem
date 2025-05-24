// FlightCheckInSystem.Data/Repositories/BaseRepository.cs
using System.Data.SQLite;

namespace FlightCheckInSystem.Data.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly string ConnectionString;

        protected BaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}