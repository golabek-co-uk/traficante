﻿using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Traficante.TSQL;

namespace Traficante.Connect.Connectors
{
    public class ExcelHelper
    {
        static ExcelHelper()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
        
        public async Task<IEnumerable<string>> GetSheets(string filePath)
        {
            List<string> sheets = new List<string>();
            using (var reader = OpenReader(filePath))
            {
                do
                {
                    sheets.Add(reader.Name);
                } 
                while (reader.NextResult());
            }
            return await Task.FromResult(sheets);
        }

        public async Task<IEnumerable<(string Name, string Type, bool? NotNull)>> GetFields(string filePath, string sheet)
        {
            using (var reader = OpenReader(filePath, sheet))
            {
                List<(string Name, string Type, bool? NotNull)> fields = new List<(string Name, string Type, bool? NotNull)>();
                for (int i = 0; i < reader.FieldCount; i++)
                    fields.Add((reader.GetName(i), null, null));
                return await Task.FromResult(fields);
            }
        }

        public ExcelDataReader OpenReader(string filePath, string sheet = null)
        {
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            var reader = ExcelReaderFactory.CreateReader(stream);
            var exelReader = new ExcelDataReader(reader);
            if (string.IsNullOrEmpty(sheet))
            {
                return exelReader;
            }
            else
            {
                do
                {
                    if (string.Equals(reader.Name?.Trim(), sheet?.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        return new ExcelDataReader(reader);
                }
                while (reader.NextResult());
                throw new TSQLException($"Sheet does not exist: {sheet}");
            }
        }
    }

    public class ExcelDataReader : IDataReader
    {
        private readonly IExcelDataReader _reader;

        public ExcelDataReader(IExcelDataReader reader)
        {
            this._reader = reader;
            //var tables = result.Tables
            //                   .Cast<DataTable>()
            //                   .Select(t => new {
            //                       TableName = t.TableName,
            //                       Columns = t.Columns
            //                                                .Cast<DataColumn>()
            //                                                .Select(x => x.ColumnName)
            //                                                .ToList()
            //                   });
        }

        public string Name => this._reader.Name;

        public Type GetFieldType(int i)
        {
            //return this._reader.GetFieldType(i);
            return typeof(object);
        }

        public string GetName(int i)
        {
            //return this._reader.GetName(i);
            return ExcelColumnFromNumber(++i);
        }

        public static string ExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }

        public object this[int i] => this._reader[0];

        public object this[string name] => this._reader[name];

        public int Depth => this._reader.Depth;

        public bool IsClosed => this._reader.IsClosed;

        public int RecordsAffected => this._reader.RecordsAffected;

        public int FieldCount => this._reader.FieldCount;

        public void Close()
        {
            this._reader.Close();
        }

        public void Dispose()
        {
            this._reader.Dispose();
        }

        public bool GetBoolean(int i)
        {
            return this._reader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return this._reader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return this._reader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return this._reader.GetData(i);
        }

        public DateTime GetDateTime(int i)
        {
            return this._reader.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return this._reader.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return this._reader.GetDouble(i);
        }

        public string GetDataTypeName(int i)
        {
            return this._reader.GetDataTypeName(i);
        }

        public float GetFloat(int i)
        {
            return this._reader.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return this._reader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return this._reader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return this._reader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return this._reader.GetInt64(i);
        }

        public int GetOrdinal(string name)
        {
            return this._reader.GetOrdinal(name);
        }

        public DataTable GetSchemaTable()
        {
            return this._reader.GetSchemaTable();
        }

        public string GetString(int i)
        {
            return this._reader.GetString(i);
        }

        public object GetValue(int i)
        {
            return this._reader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
                values[i] = GetValue(i);
            return values.Length;
            //return this._reader.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return this._reader.IsDBNull(i);
        }

        public bool NextResult()
        {
            return this._reader.NextResult();
        }

        public bool Read()
        {
            return this._reader.Read();
        } 
    }
}
