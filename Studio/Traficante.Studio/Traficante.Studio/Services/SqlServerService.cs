using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ReactiveUI;

namespace Traficante.Studio.Services
{
    public class SqlServerService
    {
        public async Task TryConnect(SqlServerConnectionString connectionString, CancellationToken ct)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = connectionString.ToConnectionString();
                await sqlConnection.OpenAsync(ct);
            }

        }

        // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections?view=netframework-4.8
        public async Task<List<RelationalDatabaseModel>> GetSchema(SqlServerConnectionString connectionString, CancellationToken ct)
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
                                    table.Columns.Add(new ColumnModel
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
                                    table.Columns.Add(new ColumnModel
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
    }

    public class RelationalDatabaseModel
    {
        public string Name { get; set; }
        public ObservableCollection<RelationalFolderModel> Items { get; set; } = new ObservableCollection<RelationalFolderModel>();

    }

    public class RelationalFolderModel
    {
        public string Name { get; set; }
        public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();
    }

    public class RelationalTableMode
    {
        public string Name { get; set; }
        public string Schema { get; set; }

        public string NameWithSchema
        {
            get { return this.Schema + "." + this.Name; }
        }

        public List<ColumnModel> Columns { get; set; } = new List<ColumnModel>();
    }

    public class ColumnModel
    {
        public string Name { get; set; }
        public string DataType { get; set; }
    }

    public class SqlServerConnectionString
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public SqlServerAuthentication Authentication { get; set; } = SqlServerAuthentication.Windows;

        public string ToConnectionString()
        {
            if (Authentication == SqlServerAuthentication.Windows)
                return $"Server={Server};Database=master;Trusted_Connection=True;";
            else
                return $"Server={Server};Database=master;User Id={UserId};Password={Password};";
        }
    }

    public enum SqlServerAuthentication : int
    {
        Windows = 0,
        SqlServer = 1
    }
}
