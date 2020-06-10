using System;
using System.Collections.Generic;
using Traficante.TSQL;
using Traficante.Connect.Connectors;
using System.Linq;
using System.Collections;
using System.Threading;

namespace Traficante.Connect
{
    public class ConnectEngine
    {
        public List<Connector> Connectors { get; } = new List<Connector>();

        public void AddConector(ConnectorConfig connector)
        {
            if (connector is CsvConnectorConfig csvConfig)
                Connectors.Add(new CsvConnector(csvConfig));
            if (connector is JsonConnectorConfig jsonConfig)
                Connectors.Add(new JsonConnector(jsonConfig));
            if (connector is MySqlConnectorConfig mySqlConfig)
                Connectors.Add(new MySqlConnector(mySqlConfig));
            if (connector is SqlServerConnectorConfig sqlServerConfig)
                Connectors.Add(new SqlServerConnector(sqlServerConfig));
            if (connector is SqliteConnectorConfig sqliteConfig)
                Connectors.Add(new SqliteConnector(sqliteConfig));
            if (connector is ElasticSearchConnectorConfig elasticConfig)
                Connectors.Add(new ElasticSearchConnector(elasticConfig));
        }

        public void AddConector(Connector connector)
        {
            Connectors.Add(connector);
        }

        public IEnumerable Run(string sql, CancellationToken ct = default)
        {
            using (TSQLEngine sqlEngine = new TSQLEngine())
            {
                sqlEngine.AddTableResolver((name, path) =>
                {
                    var alias = path.FirstOrDefault();
                    var connector = Connectors.FirstOrDefault(x => string.Equals(x.Config.Alias, alias, StringComparison.InvariantCultureIgnoreCase));
                    if (connector == null)
                        throw new ApplicationException($"Cannot find the connector with the alias '{alias}'");
                    Delegate @delegate = connector.ResolveTable(name, path, ct);
                    return @delegate;
                });

                sqlEngine.AddMethodResolver((name, path, arguments) =>
                {
                    var alias = path.FirstOrDefault();
                    var connector = Connectors.FirstOrDefault(x => string.Equals(x.Config.Alias, alias, StringComparison.InvariantCultureIgnoreCase));
                    if (connector == null)
                        throw new ApplicationException($"Cannot find the connector with the alias '{alias}'");
                    Delegate @delegate = connector.ResolveMethod(name, path, arguments, ct);
                    return @delegate;
                });

                return sqlEngine.Run(sql, ct);
            }
        }
    }

}
