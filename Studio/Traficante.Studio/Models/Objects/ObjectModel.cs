using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace Traficante.Studio.Models
{
    public class ObjectModel : ReactiveObject, IObjectModel
    {
        public virtual string Title { get; set; }
        public virtual object Icon => null;


        private ObservableCollection<object> _items;
        public virtual ObservableCollection<object> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<object>();
                    _items.Add(new LoadItemsObjectModel(() =>
                    {
                        ((LoadItemsObjectModel)Items[0]).Title = "";
                        LoadItems();
                    }));

                }
                return _items;
            }
            set { _items = value; }
        }

        public virtual void LoadItems()
        {
            throw new NotImplementedException();
        }
    }

    public class LoadItemsObjectModel : ReactiveObject
    {
        public string Title { get; set; } = "...";
        public object Icon => null;
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
