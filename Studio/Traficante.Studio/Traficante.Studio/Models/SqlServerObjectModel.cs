using ReactiveUI;
using System.Reactive.Linq;

namespace Traficante.Studio.Models
{
    public class SqlServerObjectModel : ObjectModel
    {
        public SqlServerObjectModel()
        {
            this.Items.Add(new RelationalFolderModel { Name = "Databases" });
        }
        public SqlServerConnectionString ConnectionInfo { get; set; }
    }

    public class SqlServerConnectionString : ReactiveObject
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        private SqlServerAuthentication _authentication = SqlServerAuthentication.Windows;
        public SqlServerAuthentication Authentication
        {
            get => _authentication;
            set => this.RaiseAndSetIfChanged(ref _authentication, value);
        }

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
