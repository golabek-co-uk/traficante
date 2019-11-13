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
        
        private int _selectedQueryIndex;
        [DataMember]
        public int SelectedQueryIndex
        {
            get => _selectedQueryIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedQueryIndex, value);
        }
    }
}
