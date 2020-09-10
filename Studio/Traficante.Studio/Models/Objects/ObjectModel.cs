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


        private ObservableCollection<IObjectModel> _children;
        public virtual ObservableCollection<IObjectModel> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<IObjectModel>();
                    _children.Add(new LoadChildrenObjectModel(() =>
                    {
                        ((LoadChildrenObjectModel)Children[0]).Title = "";
                        LoadChildren();
                    }));

                }
                return _children;
            }
            set { _children = value; }
        }

        public IObjectModel Parent { get; set; }

        public ObjectModel(IObjectModel parent)
        {
            Parent = parent;
        }

        public virtual void LoadChildren()
        {
            throw new NotImplementedException();
        }
    }

    public class LoadChildrenObjectModel : ReactiveObject, IObjectModel
    {
        public string Title { get; set; } = "...";
        public object Icon => null;
        public Action LoadChildren { get; }

        public LoadChildrenObjectModel(Action loadChildren)
        {

            LoadChildren = loadChildren;
        }

        private ObservableCollection<IObjectModel> _children;
        public ObservableCollection<IObjectModel> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<IObjectModel>();
                    LoadChildren();
                }
                return _children;
            }
        }

        public IObjectModel Parent => throw new NotImplementedException();
    }
}
