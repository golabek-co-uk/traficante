using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Traficante.Studio.Models;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Studio.Services
{
    public class SqlServerService
    {
        public async Task TryConnectAsync(SqlServerConnectionInfo connectionString, CancellationToken ct)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = connectionString.ToConnectionString();
                await sqlConnection.OpenAsync(ct);
            }
        }

        public void TryConnect(SqlServerConnectionInfo connectionString, string databaseName)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = connectionString.ToConnectionString();
                sqlConnection.Open();
                sqlConnection.ChangeDatabase(databaseName);
            }
        }

        public IEnumerable<object> Run(SqlServerConnectionInfo connectionString, string text, Action<Type> returnTypeCreated = null)
        {
            using (var sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = connectionString.ToConnectionString();
                sqlConnection.Open();
                //sqlConnection.ChangeDatabase(databaseName);
                using (var sqlCommand = new SqlCommand(text, sqlConnection))
                {
                    using (var sqlReader = sqlCommand.ExecuteReader())
                    {
                        List<(string name, Type type)> fields = new List<(string name, Type type)>();
                        for (int i = 0; i < sqlReader.FieldCount; i++)
                        {
                            var name = sqlReader.GetName(i);
                            var type = sqlReader.GetFieldType(i);
                            fields.Add((name, type));
                        }
                        Type returnType = new ExpressionHelper().CreateAnonymousType(fields);
                        returnTypeCreated?.Invoke(returnType);
                        while (sqlReader.Read())
                        {
                            var item = Activator.CreateInstance(returnType);
                            for (int i = 0; i < sqlReader.FieldCount; i++)
                            {
                                
                                var name = sqlReader.GetName(i);
                                var field = returnType.GetField(name);
                                var value = sqlReader.GetValue(i);
                                if (value is DBNull)
                                    value = GetDefaultValue(field.FieldType);
                                field.SetValue(item, value);
                            }
                            yield return item;
                        }
                    }
                }
            }
        }

        public object GetDefaultValue(Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }


        // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections?view=netframework-4.8

        public List<string> GetDatabases(SqlServerConnectionInfo connectionInfo)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = connectionInfo.ToConnectionString();
                sqlConnection.Open();
                var databasesNames = sqlConnection.GetSchema("Databases").AsEnumerable().Select(s => s[0].ToString()).ToList();
                return databasesNames;
            }
        }

        public List<(string schema, string name)> GetTables(SqlServerConnectionInfo connectionInfo, string database)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = connectionInfo.ToConnectionString();
                sqlConnection.Open();
                sqlConnection.ChangeDatabase(database);
                var tables = sqlConnection.GetSchema("Tables")
                    .AsEnumerable()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "BASE TABLE")
                    .Select(t => (t["TABLE_SCHEMA"]?.ToString(), t["TABLE_NAME"]?.ToString()))
                    .ToList();
                return tables;
            }
        }

        public List<(string schema, string name)> GetViews(SqlServerConnectionInfo connectionInfo, string database)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = connectionInfo.ToConnectionString();
                sqlConnection.Open();
                sqlConnection.ChangeDatabase(database);
                var views = sqlConnection.GetSchema("Tables")
                    .AsEnumerable()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "VIEW")
                    .Select(t => (t["TABLE_SCHEMA"]?.ToString(), t["TABLE_NAME"]?.ToString()))
                    .ToList();
                return views;
            }
        }
    }
}
