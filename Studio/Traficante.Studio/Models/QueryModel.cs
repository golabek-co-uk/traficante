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

        private string _selectedObjectName;
        [DataMember]
        public string SelectedObjectName
        {
            get => _selectedObjectName;
            set => this.RaiseAndSetIfChanged(ref _selectedObjectName, value);
        }
    }
}
