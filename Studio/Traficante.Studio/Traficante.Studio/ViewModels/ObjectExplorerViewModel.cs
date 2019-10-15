using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using Traficante.Studio.Models;
using Traficante.Studio.Views;

namespace Traficante.Studio.ViewModels
{
    public class ObjectExplorerViewModel : ToolTab
    {
        public ObjectExplorerViewModel()
        {
            Connect = ReactiveCommand.Create<string,string>(RunConnect);
        }

        public ReactiveCommand<string, string> Connect { get; }

        string RunConnect(string parameter)
        {
            new ConnectToSqlServerWindow()
            {
                DataContext = new ConnectToSqlServerWindowViewModel()
            }.ShowDialog(((ModelData)this.Context).MainWindow);
            return parameter;
        }
    }
}
