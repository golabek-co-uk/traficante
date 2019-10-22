using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Traficante.Studio.Models;
using Traficante.Studio.Services;
using Traficante.Studio.Views;

namespace Traficante.Studio.ViewModels
{
    public class ObjectExplorerViewModel : ToolTab
    {
        
        public ObservableCollection<ObjectModel> Objects => ((AppData)this.Context).Objects;

        public ObjectExplorerViewModel()
        {
            
        }
    }
}
