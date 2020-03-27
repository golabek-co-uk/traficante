using ReactiveUI;
using System.IO;
using System.Runtime.Serialization;

namespace Traficante.Studio.Models
{
    public class QueryModel : ReactiveObject
    {
        private string _id;
        [DataMember]
        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        private string _path;
        [DataMember]
        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public string AutoSavePath => System.IO.Path.Combine("AutoSave", this.Id);

        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set => this.RaiseAndSetIfChanged(ref _isDirty, value);
        }

        private string _text;
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }
    }

}
