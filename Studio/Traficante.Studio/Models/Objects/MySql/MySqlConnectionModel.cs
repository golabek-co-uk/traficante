using ReactiveUI;
using System.Runtime.Serialization;
using Traficante.Connect.Connectors;

namespace Traficante.Studio.Models
{
    [DataContract]
    public class MySqlConnectionModel : ReactiveObject
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string Password { get; set; }

        public MySqlConnectorConfig ToConectorConfig()
        {
            return new MySqlConnectorConfig()
            {
                Alias = this.Alias,
                Server = this.Server,
                UserId = this.UserId,
                Password = this.Password
            };
        }
    }
}
