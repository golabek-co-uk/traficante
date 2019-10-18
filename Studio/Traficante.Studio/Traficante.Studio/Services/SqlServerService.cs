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

        // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections?view=netframework-4.8
        public async Task<List<RelationalDatabaseModel>> GetSchema(SqlServerConnectionInfo connectionString, CancellationToken ct)
        {
            List<RelationalDatabaseModel> databases = new List<RelationalDatabaseModel>();
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = connectionString.ToConnectionString();
                //await sqlConnection.OpenAsync();
                sqlConnection.Open();

                var databasesNames = sqlConnection.GetSchema("Databases").AsEnumerable().Select(s => s[0].ToString()).ToList();
                foreach(var databaseName in databasesNames)
                {
                    var database = new RelationalDatabaseModel
                    {
                        Name = databaseName
                    };
                    databases.Add(database);
                    try
                    {
                        RelationalFolderModel tablesFolder = new RelationalFolderModel { Name = "Tables" };
                        RelationalFolderModel viewsFolder = new RelationalFolderModel { Name = "Views" };
                        database.Items.Add(tablesFolder);
                        database.Items.Add(viewsFolder);

                        await sqlConnection.ChangeDatabaseAsync(databaseName);

                        var tables = sqlConnection.GetSchema("Tables")
                            .AsEnumerable()
                            .Where(x => x["TABLE_TYPE"]?.ToString() == "BASE TABLE")
                            .Select(t => new RelationalTableMode
                            {
                                Name = t["TABLE_NAME"]?.ToString(),
                                Schema = t["TABLE_SCHEMA"]?.ToString()
                            })
                            .ToList();
                        tables.ForEach(x => tablesFolder.Items.Add(x));
                        sqlConnection.GetSchema("Columns")
                            .AsEnumerable()
                            .ToList()
                            .ForEach(c =>
                            {
                                var table = tables.FirstOrDefault(t => t.Schema == c["TABLE_SCHEMA"]?.ToString() && t.Name == c["TABLE_NAME"]?.ToString());
                                if (table != null)
                                {
                                    table.Columns.Add(new RelationalTableColumnModel
                                    {
                                        Name = c["COLUMN_NAME"]?.ToString(),
                                        DataType = c["DATA_TYPE"].ToString()
                                    });
                                }
                            });

                        var views = sqlConnection.GetSchema("Tables")
                            .AsEnumerable()
                            .Where(x => x["TABLE_TYPE"]?.ToString() == "VIEW")
                            .Select(t => new RelationalTableMode
                            {
                                Name = t["TABLE_NAME"]?.ToString(),
                                Schema = t["TABLE_SCHEMA"]?.ToString()
                            })
                            .ToList();
                        views.ForEach(x => viewsFolder.Items.Add(x));
                        sqlConnection.GetSchema("Columns")
                            .AsEnumerable()
                            .ToList()
                            .ForEach(c =>
                            {
                                var table = views.FirstOrDefault(t => t.Schema == c["TABLE_SCHEMA"]?.ToString() && t.Name == c["TABLE_NAME"]?.ToString());
                                if (table != null)
                                {
                                    table.Columns.Add(new RelationalTableColumnModel
                                    {
                                        Name = c["COLUMN_NAME"]?.ToString(),
                                        DataType = c["DATA_TYPE"].ToString()
                                    });
                                }
                            });
                    }
                    catch { }
                }
                
            }
            return databases;
        }

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
