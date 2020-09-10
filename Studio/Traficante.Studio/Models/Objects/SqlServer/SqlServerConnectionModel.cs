using ReactiveUI;
using System.Runtime.Serialization;
using Traficante.Connect.Connectors;

namespace Traficante.Studio.Models
{
    [DataContract]
    public class SqlServerConnectionModel : ReactiveObject
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string Password { get; set; }

        private SqlServerAuthentication _authentication = SqlServerAuthentication.Windows;
        [DataMember]
        public SqlServerAuthentication Authentication
        {
            get => _authentication;
            set => this.RaiseAndSetIfChanged(ref _authentication, value);
        }

        public SqlServerConnectorConfig ToConectorConfig()
        {
            return new SqlServerConnectorConfig()
            {
                Alias = this.Alias,
                Server = this.Server,
                Authentication = (Traficante.Connect.Connectors.SqlServerAuthentication)this.Authentication,
                UserId = this.UserId,
                Password = this.Password
            };
        }
    }

    public enum SqlServerAuthentication : int
    {
        Windows = 0,
        SqlServer = 1
    }
}
