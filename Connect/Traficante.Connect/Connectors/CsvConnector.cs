using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Traficante.TSQL;

namespace Traficante.Studio.Services
{
    public class CsvConnector
    {
        public object Run(string sql)
        {
            Engine engine = new Engine();
            engine.AddFunction("csv", (string pathToFile) =>
            {
                var reader = new StreamReader("csv.csv");
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture, false);
                return new CsvDataReader(csvReader);
            });

            return engine.Run(sql);
        }
    }

    public class CsvConnectorConfig
    {
        public string FilePath { get; set; }
    }
}
