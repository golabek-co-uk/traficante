using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Runtime.Serialization;
using Traficante.Connect.Connectors;

namespace Traficante.Studio.Models
{
    [DataContract]
    public class SqliteConnectionModel : ReactiveObject
    {
        [DataMember]
        [Reactive]
        public string Alias { get; set; }

        [DataMember]
        [Reactive]
        public string Database { get; set; }
        
        public SqliteConnectorConfig ToConectorConfig()
        {
            return new SqliteConnectorConfig()
            {
                Alias = this.Alias,
                Database = this.Database,
            };
        }
    }

}
