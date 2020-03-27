using System;
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
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Connect.Connectors
{
    public class MySqlConnector : Connector
    {
        public MySqlConnectorConfig Config => (MySqlConnectorConfig)base.Config;

        public MySqlConnector(MySqlConnectorConfig config)
        {
            base.Config = config;
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
                var tables = connection.GetSchema("Tables")
                    .Select()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "BASE TABLE")
                    .Where(x => x["TABLE_SCHEMA"]?.ToString() == database)
                    .Select(t => t["TABLE_NAME"]?.ToString())
                    .OrderBy(x => x);
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
                var views = connection.GetSchema("Tables")
                    .Select()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "VIEW")
                    .Where(x => x["TABLE_SCHEMA"]?.ToString() == database)
                    .Select(t => t["TABLE_NAME"]?.ToString())
                    .OrderBy(x => x);
                return views;
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
