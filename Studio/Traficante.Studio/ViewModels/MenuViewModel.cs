using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class MenuViewModel : Tool
    {
        public AppData AppData => ((AppData)this.Context);

        public ReactiveCommand<Unit, Unit> ConnectToSqlServerCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ConnectToMySqlCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ConnectToSqliteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ConnectToElasticSearchCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ConnectToFileCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NewCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SaveAllCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CopyCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PasteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; set; }

        public MenuViewModel()
        {
            ConnectToSqlServerCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToSqlServer);
            ConnectToMySqlCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToMySql);
            ConnectToSqliteCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToSqlite);
            ConnectToElasticSearchCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToElasticSearch);
            ConnectToFileCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToFile);
            NewCommand = ReactiveCommand.Create<Unit, Unit>(New);
            OpenCommand = ReactiveCommand.Create<Unit, Unit>(Open);
            SaveCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(Save);
            SaveAsCommand = ReactiveCommand.Create<Unit, Unit>(SaveAs);
            SaveAllCommand = ReactiveCommand.Create<Unit, Unit>(SaveAll);
            CloseCommand = ReactiveCommand.Create<Unit, Unit>(Close);
            CopyCommand = ReactiveCommand.Create<Unit, Unit>(Copy);
            PasteCommand = ReactiveCommand.Create<Unit, Unit>(Paste);
            ExitCommand = ReactiveCommand.Create<Unit, Unit>(Exit);
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

        private Unit ConnectToFile(Unit arg)
        {
            Interactions.ConnectToFile.Handle(null).Subscribe();
            return Unit.Default;
        }

        private Unit Exit(Unit arg)
        {
            Interactions.Exit.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }

        private Unit Paste(Unit arg)
        {
            Interactions.Paste.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }

        private Unit Copy(Unit arg)
        {
            Interactions.Copy.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }

        private Unit Close(Unit arg)
        {
            Interactions.CloseQuery.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }

        private Unit SaveAll(Unit arg)
        {
            Interactions.SaveAllQuery.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }

        private Unit SaveAs(Unit arg)
        {
            Interactions.SaveAsQuery.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }

        private async Task<Unit> Save(Unit arg)
        {
            await Interactions.SaveQuery.Handle(Unit.Default);
            return Unit.Default;
        }

        private Unit Open(Unit arg)
        {
            Interactions.OpenQuery.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }

        private Unit New(Unit arg)
        {
            Interactions.NewQuery.Handle(Unit.Default).Subscribe();
            return Unit.Default;
        }
    }
}
