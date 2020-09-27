using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Traficante.Connect;
using Traficante.TSQL;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Connect.Connectors
{
    public class MySqlConnector : Connector
    {
        new public MySqlConnectorConfig Config => (MySqlConnectorConfig)base.Config;

        public MySqlConnector(MySqlConnectorConfig config)
        {
            base.Config = config;
        }

        public override Delegate ResolveTable(string[] path, CancellationToken ct)
        {
            Func<Task<object>> @delegate = async () =>
            {
                MySqlConnection connection = null;
                try
                {
                    string sqlPath = string.Join(".", path.Skip(1).Select(x => $"`{x}`")); ;
                    connection = new MySqlConnection(this.Config.ToConnectionString());
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = $"SELECT * FROM {sqlPath}";
                    await connection.OpenAsync(ct);
                    return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
                }
                catch
                {
                    connection?.Dispose();
                    throw;
                }
            };
            return @delegate;
        }

        public async Task TryConnect(CancellationToken ct = default)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = this.Config.ToConnectionString();
                await connection.OpenAsync(ct);
            }
        }

        public async Task TryConnect(string databaseName, CancellationToken ct = default)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = this.Config.ToConnectionString();
                await connection.OpenAsync(ct);
                connection.ChangeDatabase(databaseName);
            }
        }

        public override async Task<object> RunQuery(string query, string language, string[] path, CancellationToken ct)
        {
            if (language == QueryLanguage.MySQLSQL.Id)
            {
                MySqlConnection connection = new MySqlConnection(this.Config.ToConnectionString());
                MySqlCommand command = new MySqlCommand();
                command.Connection = connection;
                command.CommandText = query;
                await connection.OpenAsync(ct);
                if (path.Any())
                    connection.ChangeDatabase(path.First());
                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
            }
            throw new TSQLException($"Not supported language: {language}");
        }

        public async Task<IEnumerable<string>> GetDatabases()
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = this.Config.ToConnectionString();
                await connection.OpenAsync();
                var databasesNames = connection.GetSchema("Databases").Select().Select(s => s[1].ToString()).ToList();
                return databasesNames;
            }
        }

        public async Task<IEnumerable<string>> GetTables(string database)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = this.Config.ToConnectionString();
                await connection.OpenAsync();
                connection.ChangeDatabase(database);
                var tables = connection.GetSchema("Tables", new string[2] { null, database })
                    .Select()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "BASE TABLE")
                    .Where(x => x["TABLE_SCHEMA"]?.ToString() == database)
                    .Select(t => t["TABLE_NAME"]?.ToString())
                    .OrderBy(x => x)
                    .ToList();
                return tables;
            }
        }

        public async Task<IEnumerable<string>> GetViews(string database)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = this.Config.ToConnectionString();
                await connection.OpenAsync();
                connection.ChangeDatabase(database);
                var views = connection.GetSchema("Views", new string[2] { null, database })
                    .Select()
                    //.Where(x => x["TABLE_TYPE"]?.ToString() == "VIEW")
                    .Where(x => x["TABLE_SCHEMA"]?.ToString() == database)
                    .Select(t => t["TABLE_NAME"]?.ToString())
                    .OrderBy(x => x)
                    .ToList();
                return views;
            }
        }

        public async Task<IEnumerable<(string Name, string Type, bool? NotNull)>> GetFields(string database, string tableOrView)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = this.Config.ToConnectionString();
                await connection.OpenAsync();
                connection.ChangeDatabase(database);
                var fields = connection.GetSchema("Columns", new string[3] { null, database, tableOrView })
                    .Select()
                    .Select(f => (f["COLUMN_NAME"]?.ToString(), f["DATA_TYPE"]?.ToString(), (bool?)!(string.Equals(f["IS_NULLABLE"]?.ToString(), "NO", StringComparison.OrdinalIgnoreCase))))
                    .OrderBy(x => x.Item1)
                    .ToList();
                return fields;
            }
        }
    }

    public class MySqlConnectorConfig : ConnectorConfig
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        public string ToConnectionString()
        {
            return $"Server={Server};User Id={UserId};Password={Password};";
        }
    }
}
