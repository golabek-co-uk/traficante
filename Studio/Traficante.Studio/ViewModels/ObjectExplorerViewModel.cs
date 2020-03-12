using Dock.Model.Controls;
using System.Collections.ObjectModel;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class ObjectExplorerViewModel : Tool
    {
        
        public ObservableCollection<ObjectModel> Objects => ((AppData)this.Context).Objects;

        public ObjectExplorerViewModel()
        {
            
        }
    }
}
