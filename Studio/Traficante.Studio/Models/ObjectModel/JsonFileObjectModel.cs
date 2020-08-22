namespace Traficante.Studio.Models
{
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
}
