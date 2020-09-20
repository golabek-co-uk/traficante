using System;
using System.Collections.Generic;
using Traficante.TSQL;
using Traficante.Connect.Connectors;
using System.Linq;
using System.Collections;
using System.Threading;
using System.Data;
using Traficante.TSQL.Evaluator.Helpers;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Traficante.Connect
{
    public class ConnectEngine
    {
        public List<Connector> Connectors { get; } = new List<Connector>();

        public void AddConector(ConnectorConfig connector)
        {
            if (connector is FilesConnectorConfig fileConfig)
                Connectors.Add(new FilesConnector(fileConfig));
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

        public async Task<object> Run(string query, CancellationToken ct = default)
        {
            using (TSQLEngine sqlEngine = new TSQLEngine())
            {
                sqlEngine.AddTableResolver((name, path) =>
                {
                    var alias = path.FirstOrDefault() ?? name;
                    var connector = Connectors.FirstOrDefault(x => string.Equals(x.Config.Alias, alias, StringComparison.InvariantCultureIgnoreCase));
                    if (connector == null)
                        throw new ApplicationException($"Cannot find the connector with the alias '{alias}'");
                    Delegate @delegate = connector.ResolveTable(path.Append(name).ToArray(), ct);
                    return @delegate;
                });

                sqlEngine.AddMethodResolver((name, path, arguments) =>
                {
                    var alias = path.FirstOrDefault() ?? name;
                    var connector = Connectors.FirstOrDefault(x => string.Equals(x.Config.Alias, alias, StringComparison.InvariantCultureIgnoreCase));
                    if (connector == null)
                        throw new ApplicationException($"Cannot find the connector with the alias '{alias}'");
                    Delegate @delegate = connector.ResolveMethod(path.Append(name).ToArray(), arguments, ct);
                    return @delegate;
                });

                return sqlEngine.Run(query, ct);
            }
        }

        public async Task<object> Run(string query, string language, string[] path, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(language) || language == QueryLanguage.TraficantSQL.Id)
                return await Run(query, ct);

            var alias = path.FirstOrDefault();
            var connector = Connectors.FirstOrDefault(x => x.Config.Alias == alias);
            var results = await connector.RunQuery(query, language, path.Skip(1).ToArray(), ct);

            List<(string Name, Type FieldType)> resultFields = null;
            if (results is Task)
            {
                Task<object> resultsTask = (Task<object>)results;
                await Task.WhenAll(resultsTask);
                try
                {
                    resultsTask.Wait();
                    results = resultsTask.Result;
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerException;
                }
            }
            var resultType = results.GetType();
            if (typeof(IDataReader).IsAssignableFrom(resultType))
            {
                var resultReader = (IDataReader)results;
                var resultItemsType = typeof(object[]);
                resultFields = Enumerable
                    .Range(0, resultReader.FieldCount)
                    .Select(x => (resultReader.GetName(x), resultReader.GetFieldType(x)))
                    .ToList();
                results = new DataReaderEnumerable(resultReader, ct);
                Expression sequence = ExpressionHelpers.AsParallel(results, resultItemsType);
                sequence = sequence.Select(resultItemExpression =>
                {
                    Type resultItemType = new AnonymousTypeBuilder().CreateAnonymousType(resultFields);
                    return resultItemExpression.MapTo(resultItemType);
                });
                return ExpressionHelpers.Execute(sequence, ct);
            }
            return results;
        }
    }

}
