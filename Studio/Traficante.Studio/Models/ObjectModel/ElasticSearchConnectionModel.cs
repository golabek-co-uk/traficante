using ReactiveUI;
using System.Runtime.Serialization;
using Traficante.Connect.Connectors;

namespace Traficante.Studio.Models
{
    [DataContract]
    public class ElasticSearchConnectionModel : ReactiveObject
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Server { get; set; }
        
        public ElasticSearchConnectorConfig ToConectorConfig()
        {
            return new ElasticSearchConnectorConfig()
            {
                Alias = this.Alias,
                Server = this.Server
            };
        }
    }
}
