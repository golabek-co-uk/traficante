using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect;
using Traficante.TSQL;


namespace Traficante.Connect.Connectors
{
    public class FilesConnector : Connector
    {
        public FilesConnectorConfig Config => (FilesConnectorConfig)base.Config;

        public FilesConnector(FilesConnectorConfig config)
        {
            base.Config = config;
        }

        public override Delegate ResolveMethod(string[] path, Type[] arguments, CancellationToken ct)
        {
            string name = path.LastOrDefault();
            if (string.Equals(name, "fromFile", StringComparison.InvariantCultureIgnoreCase)
                && arguments.Length == 1
                && arguments[0] == typeof(string))
            {
                Func<string, IDataReader> fromFile = (pathToFile) =>
                {
                    switch (new FileHelper().GetType(pathToFile))
                    {
                        case FileType.Csv:
                            return new CsvHelper().OpenReader(pathToFile);
                        case FileType.Excel:
                            return new ExcelHelper().OpenReader(pathToFile);
                    }
                    throw new Exception($"Cannot recognize the file format: {pathToFile}");
                };
                return fromFile;
            }
            if (string.Equals(name, "fromFile", StringComparison.InvariantCultureIgnoreCase)
                && arguments.Length == 2
                && arguments[0] == typeof(string)
                && arguments[1] == typeof(string))
            {
                Func<string, string, IDataReader> fromFile = (pathToFile, arg1) =>
                {
                    switch (new FileHelper().GetType(pathToFile))
                    {
                        case FileType.Csv:
                            return new CsvHelper().OpenReader(pathToFile);
                        case FileType.Excel:
                            return new ExcelHelper().OpenReader(pathToFile, arg1);
                    }
                    throw new Exception($"Cannot recognize the file format: {pathToFile}");
                };
                return fromFile;
            }
            return null;
        }

        public override Delegate ResolveTable(string[] path, CancellationToken ct)
        {
            Func<Task<object>> @delegate = async () =>
            {
                FileConnectorConfig file = null;
                string arg1 = null;
                if(path.Length == 1)
                {
                    file = this.Config.Files.FirstOrDefault();
                }
                if (path.Length == 2)
                {
                    file = this.Config.Files.FirstOrDefault(x => string.Equals(x.Name, path[1], StringComparison.InvariantCultureIgnoreCase));
                }
                if (path.Length == 3)
                {
                    file = this.Config.Files.FirstOrDefault(x => string.Equals(x.Name, path[1], StringComparison.InvariantCultureIgnoreCase));
                    arg1 = path[2];
                }

                switch(file.Type)
                {
                    case FileType.Csv:
                        return new CsvHelper().OpenReader(file.Path);
                    case FileType.Excel:
                        return new ExcelHelper().OpenReader(file.Path, arg1);
                }
                throw new Exception($"Cannot recognize the file format: {file.Path}");
            };
            return @delegate;
        }
    }

    public class FilesConnectorConfig : ConnectorConfig
    {
        public List<FileConnectorConfig> Files { get; set; }
    }

    public class FileConnectorConfig
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public FileType Type {get;set;}
    }

    public class FileHelper
    {
        public FileType GetType(string file)
        {
            string extension = Path.GetExtension(file);
            if (string.Equals(".csv", extension, StringComparison.InvariantCultureIgnoreCase))
                return FileType.Csv;
            if (string.Equals(".json", extension, StringComparison.InvariantCultureIgnoreCase))
                return FileType.Json;
            if (string.Equals(".xml", extension, StringComparison.InvariantCultureIgnoreCase))
                return FileType.Xml;
            if (string.Equals(".txt", extension, StringComparison.InvariantCultureIgnoreCase))
                return FileType.Text;
            if (string.Equals(".xlt", extension, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(".xlsx", extension, StringComparison.InvariantCultureIgnoreCase))
                return FileType.Excel;
            return FileType.Unknown;
        }

        public string GetName(string file)
        {
            return Path.GetFileName(file);
        }
    }

    public enum FileType
    {
        Csv,
        Excel,
        Json,
        Xml,
        Text,
        Unknown
    }

}
