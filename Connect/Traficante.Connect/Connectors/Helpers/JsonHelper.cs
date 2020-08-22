using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Threading;

namespace Traficante.Connect.Connectors
{
    public class JsonHelper
    {
        //public JsonConnector(JsonConnectorConfig config)
        //{
        //    this.Config = config;
        //}

        //public override Delegate ResolveMethod(string name, string[] path, Type[] arguments, CancellationToken ct)
        //{
        //    if (string.Equals(name, "fromFile", StringComparison.InvariantCultureIgnoreCase)
        //        && arguments.Length == 1
        //        && arguments[0] == typeof(string))
        //    {
        //        Func<string, JsonDataReader> fromFile = (pathToFile) =>
        //        {
        //            var reader = new JsonTextReader(new StreamReader(File.OpenRead(pathToFile)));
        //            return new JsonDataReader(reader);
        //        };
        //        return fromFile;
        //    }
        //    return null;
        //}
    }

  

    public class JsonDataReader : IDataReader
    {
        private readonly JsonTextReader _reader;

        public JsonDataReader(JsonTextReader jsonTextReader)
        {
            this._reader = jsonTextReader;
        }

        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => throw new NotImplementedException();

        public int RecordsAffected => throw new NotImplementedException();

        public int FieldCount => throw new NotImplementedException();

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            throw new NotImplementedException();
        }
    }
}
