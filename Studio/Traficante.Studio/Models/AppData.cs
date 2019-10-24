using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class AppData
    {
        public MainWindow MainWindow { get; set; }
        public ObservableCollection<ObjectModel> Objects { get; set; } = new ObservableCollection<ObjectModel>();
        public ObservableCollection<QueryModel> Queries { get; set; } = new ObservableCollection<QueryModel>();

    }
}
