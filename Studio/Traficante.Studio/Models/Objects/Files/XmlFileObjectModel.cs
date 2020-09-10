﻿using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class XmlFileObjectModel : ObjectModel
    {
        private readonly FilesObjectModel _files;
        private readonly FileConnectionModel _file;

        public override object Icon => BaseLightIcons.File;

        public XmlFileObjectModel(FilesObjectModel files, FileConnectionModel file)
        {
            this._files = files;
            this._file = file;
        }
    }
}
