using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class ExcelFileObjectModel : ObjectModel
    {
        public FilesObjectModel Files { get; set; }
        public FileConnectionModel File { get; set; }

        public override string Title => File.Name;
        public override object Icon => BaseLightIcons.File;

        public ExcelFileObjectModel(FilesObjectModel files, FileConnectionModel file)
        {
            this.Files = files;
            this.File = file;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new ExcelHelper().GetSheets(this.File.Path)))
                .SelectMany(x => x)
                .Select(x => new ExcelFileSheetObjectModel(this, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class ExcelFileSheetObjectModel : ObjectModel
    {
        public ExcelFileObjectModel File { get; }
        public string Sheet { get; set; }
        public override object Icon => BaseLightIcons.Table;

        public ExcelFileSheetObjectModel(ExcelFileObjectModel file, string sheet)
        {
            File = file;
            Title = $"{sheet}";
            Sheet = sheet;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new ExcelHelper().GetFields(this.File.File.Path, Sheet)))
                .SelectMany(x => x)
                .Select(x => new ExcelFileFieldObjectModel(this, x.Name, x.Type, x.NotNull))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class ExcelFileFieldObjectModel : ObjectModel, IFieldObjectModel
    {
        public ExcelFileSheetObjectModel Sheet { get; }
        public string Name { get; set; }
        public override object Icon => BaseLightIcons.Field;
        public string FieldName => Name;
        public override ObservableCollection<object> Items => null;

        public ExcelFileFieldObjectModel(ExcelFileSheetObjectModel sheet, string name, string type, bool? notNull)
        {
            Sheet = sheet;
            Title = $"{name} {type}";
            Name = name;
        }
    }
}
