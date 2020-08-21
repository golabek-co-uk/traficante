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

namespace Traficante.Studio.Models
{
    public class FilesObjectModel : ObjectModel, IAliasObjectModel
    {
        [DataMember]
        [Reactive]
        public FilesConnectionModel ConnectionInfo { get; set; }


        public override string Title => this.ConnectionInfo.Alias;

        public FilesObjectModel()
        {
            this.ConnectionInfo = new FilesConnectionModel();
        }

        public FilesObjectModel(FilesConnectionModel connectionInfo)
        {
            this.ConnectionInfo = connectionInfo;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    return ConnectionInfo
                        .Files
                        .Select(x =>
                        {
                            switch (new FilesConnector(null).GetFileType(x.File))
                            {
                                case FileType.Csv:
                                   return (object)new CsvFileObjectModel(this, x);
                                case FileType.Excel:
                                    return (object)new ExcelFileObjectModel(this, x);
                            }
                            return null;
                        })
                        .Where(x => x != null);
                }))
                .SelectMany(x => x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }

        public string GetAlias()
        {
            return this.ConnectionInfo.Alias;
        }
    }

    [DataContract]
    public class FilesConnectionModel : ReactiveObject
    {
        [DataMember]
        [Reactive]
        public string Alias { get; set; }

        [DataMember]
        [Reactive]
        public List<FileConnectionModel> Files { get; set; } = new List<FileConnectionModel>();

        public FilesConnectorConfig ToConectorConfig()
        {
            return new FilesConnectorConfig
            {
                Alias = Alias,
                Files = Files.Select(x => new FileConnectorConfig
                {
                    Path = x.File,
                    Name = Path.GetFileName(x.File)
                }).ToList()
            };
        }
    }
    
    public class FileConnectionModel : ReactiveObject
    {
        [DataMember]
        [Reactive]
        public string File { get; set; }
    }


    public class CsvFileObjectModel : ObjectModel
    {
        private FilesObjectModel Files { get; }
        private FileConnectionModel File { get; }
        public override string Title => Path.GetFileName(File.File);

        public CsvFileObjectModel(FilesObjectModel files, FileConnectionModel file)
        {
            Files = files;
            File = file;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new Traficante.Connect.Connectors.CsvHelper().GetFields(File.File)))
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


    public class ExcelFileObjectModel : ObjectModel
    {
        public FilesObjectModel Files { get; set; }
        public FileConnectionModel File { get; set; }

        public override string Title => Path.GetFileName(File.File);

        public ExcelFileObjectModel(FilesObjectModel files, FileConnectionModel file)
        {
            this.Files = files;
            this.File = file;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new ExcelHelper().GetSheets(this.File.File)))
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

        public ExcelFileSheetObjectModel(ExcelFileObjectModel file, string sheet)
        {
            File = file;
            Title = $"{sheet}";
            Sheet = sheet;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new ExcelHelper().GetFields(this.File.File.File, Sheet)))
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

        public ExcelFileFieldObjectModel(ExcelFileSheetObjectModel sheet, string name, string type, bool? notNull)
        {
            Sheet = sheet;
            Title = $"{name} {type}";
            Name = name;
        }

        public override ObservableCollection<object> Items => null;

        public string GetFieldName()
        {
            return Name;
        }
    }

    public class JsonFileObjectModel : ObjectModel
    {
        private readonly FilesObjectModel _files;
        private readonly FileConnectionModel _file;

        public JsonFileObjectModel(FilesObjectModel files, FileConnectionModel file)
        {
            this._files = files;
            this._file = file;
        }

    }


    public class XmlFileObjectModel : ObjectModel
    {
        private readonly FilesObjectModel _files;
        private readonly FileConnectionModel _file;

        public XmlFileObjectModel(FilesObjectModel files, FileConnectionModel file)
        {
            this._files = files;
            this._file = file;
        }
    }


    public class TextFileObjectModel : ObjectModel
    {
        private readonly FilesObjectModel _files;
        private readonly FileConnectionModel _file;

        public TextFileObjectModel(FilesObjectModel files, FileConnectionModel file)
        {
            this._files = files;
            this._file = file;
        }
    }
}
