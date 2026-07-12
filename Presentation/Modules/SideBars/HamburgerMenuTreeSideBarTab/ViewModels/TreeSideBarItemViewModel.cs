using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Toolkit.Controls;

using Aksl.Infrastructure;

namespace Aksl.Modules.HamburgerMenuTreeSideBarTab.ViewModels
{
    public class TreeSideBarItemViewModel : Mvvm.NodeViewModel
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        //protected readonly TreeSideBarItemViewModel _parent;
        //protected ObservableCollection<TreeSideBarItemViewModel> _children;
        private readonly MenuItem _menuItem;
        #endregion

        #region Constructors
        public TreeSideBarItemViewModel() : base()
        {
            _menuItem = null;
            //Parent = null;

            //_children = new();
        }

        public TreeSideBarItemViewModel(MenuItem menuItem) : base(menuItem.Name, menuItem.Title, null)
        {
            _menuItem = menuItem;
            //Parent = null;

            _eventAggregator = PrismUnityExtensions.GetEventAggregator();

            //_children = new();
        }

        public TreeSideBarItemViewModel(MenuItem menuItem, TreeSideBarItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
        {
            _menuItem = menuItem;
            //Parent = parent;

            _eventAggregator = PrismUnityExtensions.GetEventAggregator();

            //Parent?.Children.Add(this);

            //_children = new();
        }

        //public TreeSideBarItemViewModel(IEventAggregator eventAggregator, MenuItem menuItem) : this(eventAggregator, menuItem, null)
        //{
        //}

        //public TreeSideBarItemViewModel(IEventAggregator eventAggregator, MenuItem menuItem, TreeSideBarItemViewModel parent)
        //{
        //    _eventAggregator = eventAggregator;
        //    _menuItem = menuItem;
        //    _parent = parent;

        //    _children = new((from child in _menuItem.SubMenus
        //                     select new TreeSideBarItemViewModel(eventAggregator, child, this)).ToList<TreeSideBarItemViewModel>());
        //}
        #endregion

        #region Properties 
        public MenuItem MenuItem => _menuItem;
        public bool IsNavigationToRightContent =>
                        IsLeaf && _menuItem.HasNextSubMenu() && _menuItem.HasViewName() && _menuItem.IsNexApplication;
        public bool IsAddViewToRightContent =>
                        IsLeaf && !_menuItem.HasNextSubMenu() && _menuItem.HasViewName() && !_menuItem.IsNexApplication;

        public bool IsSelected
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    int level = Level;
                    if (field)
                    {
                        if (field && IsAddViewToRightContent)
                        {
                            AddViewToRightContent();
                        }

                        if (field && IsNavigationToRightContent)
                        {
                            NavigationToRightContent();
                        }
                    }
                }
            }
        }

        public bool IsExpanded
        {
            get => field;
            set
            {
                SetProperty<bool>(ref field, value);

                if (field && Parent is not null)
                {
                    if (!(Parent as TreeSideBarItemViewModel).IsExpanded)
                    {
                        (Parent as TreeSideBarItemViewModel).IsExpanded = true;
                    }
                }
            }
        } = false;

        public PackIconKind IconKind =>
                  _menuItem.IconKind.ToPackIconKind();

        public bool IsEnabled
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    foreach (var children in this.Children)
                    {
                        (children as TreeSideBarItemViewModel).IsEnabled = field;
                    }
                }
            }
        } = true;
        #endregion

        #region Add View To RightContent Method
        public void AddViewToRightContent()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            ActiveContentManagerExtensions.AddViewToRandomContentAsync(_menuItem, ActiveContentNames.RightContentHamburgerMenuTreeSideBar).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                {
                    await dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View");
                });
            });
        }
        #endregion

        #region Navigation To RightContent Method
        public void NavigationToRightContent()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            ActiveContentManagerExtensions.NavigationToRandomContentAsync(_menuItem, ActiveContentNames.RightContentHamburgerMenuTreeSideBar, new() { { "CurrentMenuItem", _menuItem } }).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                {
                    await dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To RightContent");
                });
            });
        }
        #endregion
    }
}