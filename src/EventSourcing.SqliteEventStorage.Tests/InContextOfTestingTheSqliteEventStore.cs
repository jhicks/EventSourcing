using System.Configuration;
using System.Data.SQLite;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnitTests;

namespace EventSourcing.SqliteEventStorage.Tests
{
    public abstract class InContextOfTestingTheSqliteEventStore : BaseTestFixture<EventStore>
    {
        protected string _connectionString;
        protected IFormatter _formatter;
    	private SQLiteConnection _connection;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();

            _formatter = new BinaryFormatter();
            _connectionString = ConfigurationManager.ConnectionStrings["EventStoreConnectionString"].ConnectionString;
			_connection = new SQLiteConnection(_connectionString);

			SchemaGenerator.EnsureSchemaExists(_connection);

			_subjectUnderTest = new EventStore(_connection, _formatter);
        }

        protected override void Finally()
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
            base.Finally();
        }
    }
}
