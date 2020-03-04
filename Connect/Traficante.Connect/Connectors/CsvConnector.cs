using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Traficante.Connect;
using Traficante.TSQL;

namespace Traficante.Connect.Connectors
{
    public class CsvConnector : Connector
    {
        public CsvConnector(CsvConnectorConfig config)
        {
            this.Config = config;
        }

        public override Delegate GetMethod(string name, string[] path, Type[] arguments)
        {
            if (string.Equals(name, "fromFile", StringComparison.InvariantCultureIgnoreCase)
                && arguments.Length == 1
                && arguments[0] == typeof(string))
            {
                Func<string, CsvDataReader> fromFile = (pathToFile) =>
                {
                    var reader = new StreamReader(pathToFile);
                    var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture, false);
                    return new CsvDataReader(csvReader);
                };
                return fromFile;
            }
            return null;
        }
    }

    public class CsvConnectorConfig : ConnectorConfig
    {
        public string FilePath { get; set; }
    }
}
