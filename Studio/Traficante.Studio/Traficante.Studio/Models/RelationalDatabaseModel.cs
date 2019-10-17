using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Traficante.Studio.Models
{
    public class RelationalDatabaseModel
    {
        public string Name { get; set; }
        public ObservableCollection<RelationalFolderModel> Items { get; set; } = new ObservableCollection<RelationalFolderModel>();
    }

    public class RelationalFolderModel
    {
        public string Name { get; set; }
        public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();
    }

    public class RelationalTableMode
    {
        public string Name { get; set; }
        public string Schema { get; set; }

        public string NameWithSchema
        {
            get { return this.Schema + "." + this.Name; }
        }

        public List<RelationalTableColumnModel> Columns { get; set; } = new List<RelationalTableColumnModel>();
    }

    public class RelationalTableColumnModel
    {
        public string Name { get; set; }
        public string DataType { get; set; }
    }
}
