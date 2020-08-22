namespace Traficante.Studio.Models
{
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
