using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using Traficante.Studio.Models;
using Traficante.Studio.Services;
using Traficante.Studio.Views;

namespace Traficante.Studio.ViewModels
{
    public class ObjectExplorerViewModel : ToolTab
    {
        public ObjectExplorerViewModel()
        {
            Connect = ReactiveCommand.Create<string,string>(RunConnect);

            var items = new SqlServerService().GetSchema(
                new SqlServerConnectionString
                {
                    Server = ""
                }, CancellationToken.None).Result;

            DbSources = new ObservableCollection<RelationalDatabaseModel>(items);
        }

        public ReactiveCommand<string, string> Connect { get; }

        public ObservableCollection<RelationalDatabaseModel> DbSources { get; set; }
        

        string RunConnect(string parameter)
        {


            //new ConnectToSqlServerWindow()
            //{
            //    DataContext = new ConnectToSqlServerWindowViewModel()
            //}.ShowDialog(((ModelData)this.Context).MainWindow);
            return parameter;
        }
    }
}
