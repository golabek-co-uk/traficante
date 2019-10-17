using System.Collections.ObjectModel;

namespace Traficante.Studio.Models
{
    public class ObjectModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        //public string Type { get; set; }
        public bool IsConnected { get; set; }
        //public object ConnectionInfo { get; set; }
        //public object ModelInfo { get; set; }
        public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();
    }
}
