using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Traficante.Studio.Services
{
    public class CsvService
    {
        public object Run()
        {
            using (var reader = new StreamReader("path\\to\\file.csv"))
            using (var csv = new CsvReader(reader))
            {
            }

            return null;
        }
    }
}
