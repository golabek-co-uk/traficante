using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Traficante.Connect.Connectors
{
    public class SqliteConnector : Connector
    {
        public SqliteConnectorConfig Config => (SqliteConnectorConfig)base.Config;

        public SqliteConnector(SqliteConnectorConfig config)
        {
            base.Config = config;
        }

        public async Task TryConnectAsync(CancellationToken ct)
        {
            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = this.Config.ToConnectionString();
                await connection.OpenAsync(ct);
            }
        }

        public void TryConnect()
        {
            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = this.Config.ToConnectionString();
                connection.Open();
            }
        }
        

        public override Delegate GetTable(string name, string[] path)
        {
            Func<IDataReader> @delegate = () =>
            {
                SqliteConnection sqliteConnection = null;
                try
                {
                    sqliteConnection = new SqliteConnection(this.Config.ToConnectionString());
                    SqliteCommand sqliteCommand = new SqliteCommand();
                    sqliteCommand.Connection = sqliteConnection;
                    sqliteCommand.CommandText = $"SELECT * FROM {name}";
                    sqliteConnection.Open();
                    return sqliteCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch
                {
                    sqliteConnection?.Dispose();
                    throw;
                }
            };
            return @delegate;
        }

        public List<string> GetDatabases()
        {
            List<string> databases = new List<string>();
            databases.Add(this.Config.Database);
            return databases;
        }

        public List<(string schema, string name)> GetTables()
        {
            List<(string schema, string name)> tables = new List<(string schema, string name)>();
            using (SqliteConnection sqlConnection = new SqliteConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                sqlConnection.Open();
                using (var command = sqlConnection.CreateCommand())
                {
                    command.CommandText = "SELECT name from sqlite_master WHERE type='table'";
                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            tables.Add((null, result.GetString(0)));
                        }
                    }
                }
            }
            return tables;
        }

        public List<(string schema, string name)> GetViews()
        {
            List<(string schema, string name)> views = new List<(string schema, string name)>();
            using (SqliteConnection sqlConnection = new SqliteConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                sqlConnection.Open();
                using (var command = sqlConnection.CreateCommand())
                {
                    command.CommandText = "SELECT name from sqlite_master WHERE type='view'";
                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            views.Add((null, result.GetString(0)));
                        }
                    }
                }
            }
            return views;
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
