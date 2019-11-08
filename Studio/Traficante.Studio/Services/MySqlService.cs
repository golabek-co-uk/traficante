using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Traficante.Studio.Models;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Studio.Services
{
    public class MySqlService
    {
        public async Task TryConnectAsync(MySqlConnectionInfo connectionString, CancellationToken ct)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = connectionString.ToConnectionString();
                await connection.OpenAsync(ct);
            }
        }

        public void TryConnect(MySqlConnectionInfo connectionString, string databaseName)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = connectionString.ToConnectionString();
                connection.Open();
                connection.ChangeDatabase(databaseName);
            }
        }

        public IEnumerable<object> Run(MySqlConnectionInfo connectionString, string text, Action<Type> returnTypeCreated = null)
        {
            using (var connection = new MySqlConnection())
            {
                connection.ConnectionString = connectionString.ToConnectionString();
                connection.Open();
                //sqlConnection.ChangeDatabase(databaseName);
                using (var sqlCommand = new MySqlCommand(text, connection))
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
                                returnType.GetField(name).SetValue(item, sqlReader.GetValue(i));
                            }
                            yield return item;
                        }
                    }
                }
            }
        }


        public List<string> GetDatabases(MySqlConnectionInfo connectionInfo)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = connectionInfo.ToConnectionString();
                connection.Open();
                var databasesNames = connection.GetSchema("Databases").AsEnumerable().Select(s => s[1].ToString()).ToList();
                return databasesNames;
            }
        }

        public List<string> GetTables(MySqlConnectionInfo connectionInfo, string database)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = connectionInfo.ToConnectionString();
                connection.Open();
                connection.ChangeDatabase(database);
                var tables = connection.GetSchema("Tables")
                    .AsEnumerable()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "BASE TABLE")
                    .Where(x => x["TABLE_SCHEMA"]?.ToString() == database)
                    .Select(t => t["TABLE_NAME"]?.ToString())
                    .ToList();
                return tables;
            }
        }

        public List<string> GetViews(MySqlConnectionInfo connectionInfo, string database)
        {
            using (MySqlConnection connection = new MySqlConnection())
            {
                connection.ConnectionString = connectionInfo.ToConnectionString();
                connection.Open();
                connection.ChangeDatabase(database);
                var views = connection.GetSchema("Tables")
                    .AsEnumerable()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "VIEW")
                    .Where(x => x["TABLE_SCHEMA"]?.ToString() == database)
                    .Select(t => t["TABLE_NAME"]?.ToString())
                    .ToList();
                return views;
            }
        }
    }
}
