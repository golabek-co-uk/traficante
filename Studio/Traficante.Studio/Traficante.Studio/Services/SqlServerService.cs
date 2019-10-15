using System;
using System.Collections.Generic;
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
