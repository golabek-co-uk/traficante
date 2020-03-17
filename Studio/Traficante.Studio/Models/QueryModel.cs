using ReactiveUI;
using System.Runtime.Serialization;

namespace Traficante.Studio.Models
{
    public class QueryModel : ReactiveObject
    {
        private string _id;
        [DataMember]
        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        private string _path;
        [DataMember]
        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }
    }
}
