using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class AppData : ReactiveObject
    {
        public MainWindow MainWindow { get; set; }

        [DataMember]
        public ObservableCollection<ObjectModel> Objects { get; set; } = new ObservableCollection<ObjectModel>();
        [DataMember]
        public ObservableCollection<QueryModel> Queries { get; set; } = new ObservableCollection<QueryModel>();

        public void AddObject(ObjectModel newModel)
        {
            var newModelAlias = (IDataSourceObjectModel)newModel;
            foreach(IDataSourceObjectModel model in Objects)
            {
                if (string.Equals(model.ConnectionAlias, newModelAlias.ConnectionAlias, StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception($"Another connection with '{newModelAlias.ConnectionAlias}' alias already exists.");
            }
            Objects.Add(newModel);
        }

        public void UpdateObject(ObjectModel existingModel, ObjectModel newModel)
        {
            var newModelAlias = (IDataSourceObjectModel)newModel;
            foreach (IDataSourceObjectModel model in Objects)
            {
                if (model != existingModel && string.Equals(model.ConnectionAlias, newModelAlias.ConnectionAlias, StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception($"Another connection with '{newModelAlias.ConnectionAlias}' alias already exists.");
            }
            var index = Objects.IndexOf(existingModel);
            Objects.RemoveAt(index);
            Objects.Insert(index, newModel);
        }

        public ObjectModel GetObject(string alias)
        {
            foreach (IDataSourceObjectModel model in Objects)
            {
                if (model.ConnectionAlias == alias)
                    return (ObjectModel)model;
            }
            return null;
        }

        private int _selectedQueryIndex;
        [DataMember]
        public int SelectedQueryIndex
        {
            get => _selectedQueryIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedQueryIndex, value);
        }

        public QueryModel GetSelectedQuery()
        {
            if (SelectedQueryIndex >= 0)
                return Queries[SelectedQueryIndex];
            return null;
        }
    }
}
