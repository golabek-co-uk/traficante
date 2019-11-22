using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Traficante.TSQL;

namespace Traficante.Studio.Services
{
    public class CsvService
    {
        public object Run(string sql, string pathToFile)
        {
            using (var reader = new StreamReader(pathToFile))
            using (var csv = new CsvReader(reader))
            {
            }
            return null;
        }
    }
}
