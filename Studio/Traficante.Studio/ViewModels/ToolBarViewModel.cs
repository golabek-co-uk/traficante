using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class ToolBarViewModel : Tool
    {
        public AppData AppData { get; set; }
        public ReactiveCommand<Unit, Unit> ConnectToSqlServerCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectToMySqlCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectToSqliteCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectToElasticSearchCommand { get; }
        
        public ReactiveCommand<Unit, Unit> NewQueryCommand { get; }

        public ToolBarViewModel()
        {
            ConnectToSqlServerCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToSqlServer);
            ConnectToMySqlCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToMySql);
            ConnectToSqliteCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToSqlite);
            ConnectToElasticSearchCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToElasticSearch);
            NewQueryCommand = ReactiveCommand.Create<Unit, Unit>(NewQuery);
        }

        private Unit NewQuery(Unit arg)
        {
            Interactions.NewQuery.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }

        private Unit ConnectToSqlServer(Unit arg)
        {
            Interactions.ConnectToSqlServer.Handle(null).Subscribe();
            return Unit.Default;
        }

        private Unit ConnectToMySql(Unit arg)
        {
            Interactions.ConnectToMySql.Handle(null).Subscribe();
            return Unit.Default;
        }

        private Unit ConnectToSqlite(Unit arg)
        {
            Interactions.ConnectToSqlite.Handle(null).Subscribe();
            return Unit.Default;
        }

        private Unit ConnectToElasticSearch(Unit arg)
        {
            Interactions.ConnectToElasticSearch.Handle(null).Subscribe();
            return Unit.Default;
        }
    }
}
