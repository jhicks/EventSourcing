using System.Configuration;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    public abstract class InContextOfTestingTheSqlEventStore : BaseTestFixture<EventStore>
    {
        protected string _connectionString;
        protected IFormatter _formatter;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _formatter = new BinaryFormatter();
            _connectionString = ConfigurationManager.ConnectionStrings["EventStoreConnectionString"].ConnectionString;
            _subjectUnderTest = new EventStore(_connectionString, _formatter);
        }
    }
}
