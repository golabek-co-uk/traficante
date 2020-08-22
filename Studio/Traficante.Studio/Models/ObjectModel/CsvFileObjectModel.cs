using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.Services;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class CsvFileObjectModel : ObjectModel
    {
        private FilesObjectModel Files { get; }
        private FileConnectionModel File { get; }
        public override string Title => File.Name;
        public override object Icon => Icons.File;

        public CsvFileObjectModel(FilesObjectModel files, FileConnectionModel file)
        {
            Files = files;
            File = file;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new Traficante.Connect.Connectors.CsvHelper().GetFields(File.Path)))
                .SelectMany(x => x)
                .Select(x => new CsvFileFieldObjectModel(this, x.Name, x.Type, x.NotNull))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class CsvFileFieldObjectModel : ObjectModel, IFieldObjectModel
    {
        public CsvFileObjectModel File { get; }
        public string Name { get; set; }
        public override object Icon => Icons.Field;

        public CsvFileFieldObjectModel(CsvFileObjectModel file, string name, string type, bool? notNull)
        {
            File = file;
            Title = $"{name} {type}";
            Name = name;
        }

        public override ObservableCollection<object> Items => null;

        public string GetFieldName()
        {
            return Name;
        }
    }
}
