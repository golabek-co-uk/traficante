using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Traficante.Studio.Models
{
    public class ObjectModel : ReactiveObject
    {
        public virtual string Name { get; set; }

        private ObservableCollection<object> _items;
        public ObservableCollection<object> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<object>();
                    _items.Add(new LoadItemsObjectModel(() =>
                    {
                        ((LoadItemsObjectModel)Items[0]).Name = "";
                        LoadItems();
                        //RxApp.MainThreadScheduler.Schedule(() =>
                        //{
                        //Items.Clear() ;
                        //});
                        //LoadItems();
                    }));
                }
                return _items;
            }
        }

        public virtual void LoadItems()
        {
            throw new NotImplementedException();
        }
    }

    public class LoadItemsObjectModel : ReactiveObject
    {
        public string Name { get; set; } = "...";
        public Action LoadItems { get; }

        public LoadItemsObjectModel(Action loadItems)
        {

            LoadItems = loadItems;
        }

        private ObservableCollection<object> _items;
        public ObservableCollection<object> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<object>();
                    LoadItems();
                }
                return _items;
            }
        }
    }
}
