using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Connect.Connectors
{
    public class SqlServerConnector : Connector
    {
        public SqlServerConnectorConfig Config => (SqlServerConnectorConfig)base.Config;

        public SqlServerConnector(SqlServerConnectorConfig config)
        {
            base.Config = config;
        }

        public override Delegate ResolveTable(string name, string[] path, CancellationToken ct)
        {
            Func<Task<object>> @delegate = async () =>
            {
                SqlConnection sqlConnection = null;
                try
                {
                    string sqlPath = "";
                    if (path.Length > 1)
                        sqlPath = string.Join(".", path.Skip(1).Select(x => $"[{x}]")) + ".";
                    string sqlName = $"[{name}]";

                    sqlConnection = new SqlConnection(this.Config.ToConnectionString());
                    SqlCommand sqliteCommand = new SqlCommand();
                    sqliteCommand.Connection = sqlConnection;
                    sqliteCommand.CommandText = $"SELECT * FROM {sqlPath}{sqlName}";
                    await sqlConnection.OpenAsync(ct);
                    return await sqliteCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection, ct);
                }
                catch
                {
                    sqlConnection?.Dispose();
                    throw;
                }
            };
            return @delegate;
        }

        public async Task TryConnect(CancellationToken ct = default)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync(ct);
            }
        }

        public async Task TryConnect(string databaseName, CancellationToken ct = default)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync(ct);
                sqlConnection.ChangeDatabase(databaseName);
            }
        }

        // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections?view=netframework-4.8

        public async Task<IEnumerable<string>> GetDatabases()
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync();
                var databasesNames = sqlConnection.GetSchema("Databases").Select().Select(s => s[0].ToString()).ToList();
                return databasesNames;
            }
        }

        public async Task<IEnumerable<(string Schema, string Name)>> GetTables(string database)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync();
                sqlConnection.ChangeDatabase(database);
                var tables = sqlConnection.GetSchema("Tables")
                    .Select()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "BASE TABLE")
                    .Select(t => (t["TABLE_SCHEMA"]?.ToString(), t["TABLE_NAME"]?.ToString()))
                    .OrderBy(x => x.Item1).ThenBy(x => x.Item2);
                return tables;
            }
        }

        public async Task<IEnumerable<(string Schema, string Name)>> GetViews(string database)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync();
                sqlConnection.ChangeDatabase(database);
                var views = sqlConnection.GetSchema("Tables")
                    .Select()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "VIEW")
                    .Select(t => (t["TABLE_SCHEMA"]?.ToString(), t["TABLE_NAME"]?.ToString()))
                    .OrderBy(x=> x.Item1).ThenBy(x => x.Item2);
                return views;
            }
        }

        public async Task<IEnumerable<(string Name, string Type, bool? NotNull)>> GetFields(string database, string tableOrView)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync();
                sqlConnection.ChangeDatabase(database);
                var fields = sqlConnection.GetSchema("Columns")
                    .Select()
                    .Where(x => x["TABLE_NAME"].ToString() == tableOrView)
                    .Select(x => (x["COLUMN_NAME"]?.ToString(), x["DATA_TYPE"]?.ToString(), x["IS_NULLABLE"] as Nullable<bool>))
                    .ToList();
                return fields;
            }
        }
    }

    public class SqlServerConnectorConfig  : ConnectorConfig
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public SqlServerAuthentication Authentication { get; set; } = SqlServerAuthentication.Windows;
        public string ToConnectionString()
        {
            if (Authentication == SqlServerAuthentication.Windows)
                return $"Server={Server};Database=master;Trusted_Connection=True;";
            else
                return $"Server={Server};Database=master;User Id={UserId};Password={Password};";
        }
    }

    public enum SqlServerAuthentication : int
    {
        Windows = 0,
        SqlServer = 1
    }
}
