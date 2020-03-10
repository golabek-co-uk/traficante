using System;
using System.Collections.Generic;
using System.Data;
using Traficante.TSQL;
using Traficante.Connect.Connectors;
using System.Linq;
using System.Collections;

namespace Traficante.Connect
{
    public class ConnectEngine
    {
        public List<Connector> Connectors { get; } = new List<Connector>();

        public void AddConector(ConnectorConfig connector)
        {
            if (connector is CsvConnectorConfig csvConfig)
                Connectors.Add(new CsvConnector(csvConfig));
            if (connector is MySqlConnectorConfig mySqlConfig)
                Connectors.Add(new MySqlConnector(mySqlConfig));
            if (connector is SqlServerConnectorConfig sqlServerConfig)
                Connectors.Add(new SqlServerConnector(sqlServerConfig));
            if (connector is SqliteConnectorConfig sqliteConfig)
                Connectors.Add(new SqliteConnector(sqliteConfig));
        }

        public TSQL.DataTable RunAndReturnTable(string sql)
        {
            using (TSQLEngine sqlEngine = new TSQLEngine())
            {
                sqlEngine.AddTableResolver((name, path) =>
                {
                    var alias = path.FirstOrDefault();
                    var connector = Connectors.FirstOrDefault(x => string.Equals(x.Config.Alias, alias, StringComparison.InvariantCultureIgnoreCase));
                    if (connector == null)
                        throw new ApplicationException($"Cannot find the connector with the alias '{alias}'");
                    Delegate @delegate = connector.GetTable(name, path);
                    return @delegate;
                });

                sqlEngine.AddMethodResolver((name, path, arguments) =>
                {
                    var alias = path.FirstOrDefault();
                    var connector = Connectors.FirstOrDefault(x => string.Equals(x.Config.Alias, alias, StringComparison.InvariantCultureIgnoreCase));
                    if (connector == null)
                        throw new ApplicationException($"Cannot find the connector with the alias '{alias}'");
                    Delegate @delegate = connector.GetMethod(name, path, arguments);
                    return @delegate;
                });

                return sqlEngine.RunAndReturnTable(sql);
            }
        }

        public IEnumerable RunAndReturnEnumerable(string sql)
        {
            using (TSQLEngine sqlEngine = new TSQLEngine())
            {
                sqlEngine.AddTableResolver((name, path) =>
                {
                    var alias = path.FirstOrDefault();
                    var connector = Connectors.FirstOrDefault(x => string.Equals(x.Config.Alias, alias, StringComparison.InvariantCultureIgnoreCase));
                    if (connector == null)
                        throw new ApplicationException($"Cannot find the connector with the alias '{alias}'");
                    Delegate @delegate = connector.GetTable(name, path);
                    return @delegate;
                });

                sqlEngine.AddMethodResolver((name, path, arguments) =>
                {
                    var alias = path.FirstOrDefault();
                    var connector = Connectors.FirstOrDefault(x => string.Equals(x.Config.Alias, alias, StringComparison.InvariantCultureIgnoreCase));
                    if (connector == null)
                        throw new ApplicationException($"Cannot find the connector with the alias '{alias}'");
                    Delegate @delegate = connector.GetMethod(name, path, arguments);
                    return @delegate;
                });

                return sqlEngine.RunAndReturnEnumerable(sql);
            }
        }
    }

    public class ConnectorConfig
    {
        public string Alias { get; set; }
    }

    public class Connector
    {
        public ConnectorConfig Config { get; set; }
        public IDataReader RunSelect(string name, string[] path)
        {
            return null;
        }

        public virtual Delegate GetMethod(string name, string[] path, Type[] arguments)
        {
            return null;
        }

        public virtual Delegate GetTable(string name, string[] path)
        {
            return null;
        }
    }

}
