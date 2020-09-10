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
                    Path = x.Path,
                    Name = new FileHelper().GetName(x.Path),
                    Type = new FileHelper().GetType(x.Path)
                }).ToList()
            };
        }
    }

    public class FileConnectionModel : ReactiveObject
    {
        [DataMember]
        [Reactive]
        public string Path { get; set; }

        public string Name =>  new FileHelper().GetName(Path);

        public FileType Type => new FileHelper().GetType(Path);

        //[DataMember]
        //[Reactive]
        //public FileType Type { get; set; }
    }
}
