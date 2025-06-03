using Microsoft.Data.Sqlite;

namespace FlightCheckInSystem.Data.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly string ConnectionString;

        protected BaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected SqliteConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }
    }
}