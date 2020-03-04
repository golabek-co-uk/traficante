using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Traficante.Connect.Connectors
{
    public class SqliteConnector : Connector
    {
        public SqliteConnectorConfig Config => (SqliteConnectorConfig)base.Config;

        public SqliteConnector(SqliteConnectorConfig config)
        {
            base.Config = config;
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
