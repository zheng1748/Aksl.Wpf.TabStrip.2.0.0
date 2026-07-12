using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;

namespace Aksl.Mvvm;

public class NodeViewModel : BindableBase
{
    #region Members;
    #endregion

    #region Constructors
    public NodeViewModel()
    {
        Parent = null;
        Name = "VirtualNode";

        Children = new();
        Path = "Root";
    }

    public NodeViewModel(string name, string title, NodeViewModel parent)
    {
        Name = name;
        Title = title;

        Parent = parent;
        Parent?.Children.Add(this);
        Children = new();

        Path = Parent is not null ? $"{Parent.Path}.{Name}" : Name;
    }

    //public NodeViewModel(string name, string title ) : this(name,title,null)
    //{
    //}

    //public NodeViewModel(string name, string title, NodeViewModel parent)
    //{
    //    Name = name;
    //    Title = title;

    //    _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

    //    _children = new((from child in _menuItem.SubMenus
    //                     select new NodeViewModel( child.Name, child.Title, this)).ToList<NodeViewModel>());

    //}
    #endregion

    #region Properties
    public virtual string Name { get; set; }
    public string Path { get; set; }
    public virtual string Title { get; set; }
    public virtual int Level { get; set; }
    public virtual NodeViewModel Parent { get; set; }
    public virtual ObservableCollection<NodeViewModel> Children { get; set; }
    public virtual bool HasTitle => !string.IsNullOrEmpty(Title);
    public virtual bool HasChildren => (Children is not null) && Children.Any();
    public bool IsLeaf => (Children is not null) && Children.Count <= 0;
    public bool IsTopLevelItem => (Parent is null) && IsLeaf;
    public bool IsTopLevelHeader => (Parent is null) && !IsLeaf;
    public bool IsSubmenuItem => (Parent is not null) && IsLeaf;
    public bool IsSubmenuHeader => (Parent is not null) && !IsLeaf;
    #endregion
}


