using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect;
using Traficante.TSQL;

namespace Traficante.Connect.Connectors
{
    public class CsvHelper
    {
        public async Task<IEnumerable<(string Name, string Type, bool? NotNull)>> GetFields(string filePath)
        {
            using (var csvDataReader = OpenReader(filePath))
            {
                List<(string Name, string Type, bool? NotNull)> fields = new List<(string Name, string Type, bool? NotNull)>();
                for(var i = 0; i < csvDataReader.FieldCount; i++)
                    fields.Add((csvDataReader.GetName(i), null, null));
                return await Task.FromResult(fields);
            }
        }

        public CsvDataReader OpenReader(string filePath)
        {
            var reader = new StreamReader(filePath);
            var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture, false);
            return new CsvDataReader(csvReader);
        }
    }

}
