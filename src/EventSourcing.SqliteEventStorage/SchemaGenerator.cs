using System.Data;
using System.Data.SQLite;

namespace EventSourcing.SqliteEventStorage
{
	public static class SchemaGenerator
	{
		public static void EnsureSchemaExists(SQLiteConnection connection)
		{
			if (connection.State != ConnectionState.Open)
				connection.Open();

			var command = connection.CreateCommand();
			
			command.CommandText = "CREATE TABLE IF NOT EXISTS [EventStore] (" +
				"[StreamId] GUID NOT NULL, " +
				"[Sequence] INTEGER NOT NULL, " +
				"[GeneratedOn] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, " +
				"[EventData] BLOB NOT NULL, " +
				"PRIMARY KEY ([StreamId], [Sequence]));";
			command.ExecuteNonQuery();

			command.CommandText = "CREATE INDEX IF NOT EXISTS [IX_EventStore_StreamId] ON [EventStore] ([StreamId]);";
			command.ExecuteNonQuery();

			command.CommandText = "CREATE INDEX IF NOT EXISTS [IX_EventStore_StreamId_GeneratedOn] ON [EventStore] ([StreamId], [GeneratedOn]);";
			command.ExecuteNonQuery();

			command.CommandText = "CREATE TABLE IF NOT EXISTS [SnapshotStore] (" +
				"[SourceId] GUID PRIMARY KEY NOT NULL, " +
				"[SnapshotData] BLOB NOT NULL, " +
				"[Version] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);";
			command.ExecuteNonQuery();
		}
	}
}