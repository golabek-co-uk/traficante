using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Traficante.Connect.Connectors
{
    public class SqliteConnector : Connector
    {
        new public SqliteConnectorConfig Config => (SqliteConnectorConfig)base.Config;

        public SqliteConnector(SqliteConnectorConfig config)
        {
            base.Config = config;
        }

        public async Task TryConnect(CancellationToken ct = default)
        {
            using (SqliteConnection connection = new SqliteConnection())
            {
                if (File.Exists(this.Config.Database) == false)
                    throw new Exception($"File {this.Config.Database} does not exist.");

                connection.ConnectionString = this.Config.ToConnectionString();
                await connection.OpenAsync(ct);
            }
        }

        public override Delegate ResolveTable(string[] path, CancellationToken ct)
        {
            Func<Task<object>> @delegate = async () =>
            {
                SqliteConnection sqliteConnection = null;
                try
                {
                    string name = path.LastOrDefault();
                    sqliteConnection = new SqliteConnection(this.Config.ToConnectionString());
                    SqliteCommand sqliteCommand = new SqliteCommand();
                    sqliteCommand.Connection = sqliteConnection;
                    sqliteCommand.CommandText = $"SELECT * FROM {name}";
                    await sqliteConnection.OpenAsync(ct);
                    return await sqliteCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
                }
                catch
                {
                    sqliteConnection?.Dispose();
                    throw;
                }
            };
            return @delegate;
        }

        public async Task<IEnumerable<string>> GetTables()
        {
            List<string> tables = new List<string>();
            using (SqliteConnection sqlConnection = new SqliteConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync();
                using (var command = sqlConnection.CreateCommand())
                {
                    command.CommandText = "SELECT name from sqlite_master WHERE type='table' ORDER BY name";
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                            //yield return result.GetString(0);
                            tables.Add(result.GetString(0));
                    }
                }
            }
            return tables;
        }

        public async Task<IEnumerable<string>> GetViews()
        {
            List<string> views = new List<string>();
            using (SqliteConnection sqlConnection = new SqliteConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync();
                using (var command = sqlConnection.CreateCommand())
                {
                    command.CommandText = "SELECT name from sqlite_master WHERE type='view' ORDER BY name";
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                            views.Add(result.GetString(0));
                    }
                }
            }
            return views;
        }

        public async Task<IEnumerable<(string Name, string Type, bool? NotNull)>> GetFields(string tableOrView)
        {
            List<(string name, string type, bool? notNull)> fields = new List<(string name, string type, bool? notNull)>();
            using (SqliteConnection sqlConnection = new SqliteConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync();
                using (var command = sqlConnection.CreateCommand())
                {
                    command.CommandText = $"PRAGMA table_info('{tableOrView}');";
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (result.Read())
                        {
                            fields.Add((result.GetString(1), result.GetString(2), result.GetBoolean(3)));
                        }
                    }
                }
            }
            return fields;
        }
    }

    public class SqliteConnectorConfig : ConnectorConfig
    {
        public string Database { get; set; }

        public string ToConnectionString()
        {
            return $"Datasource={Database};";
        }
    }
}
