using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Toolkit.Controls;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public class MenuItemViewModel : Mvvm.NodeViewModel
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly MenuItem _menuItem;
        #endregion

        #region Constructors
        public MenuItemViewModel(int groupIndex, int index, MenuItem menuItem)
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            GroupIndex = groupIndex;
            Index = index;
            _menuItem = menuItem;
           // RegisterHamburgerMenuBarPaneOpenEvent();
        }

        public MenuItemViewModel() : base()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();

            _menuItem = null;
            //Parent = null;

            //_children = new();
            //RegisterHamburgerMenuBarPaneOpenEvent();
        }

        public MenuItemViewModel(MenuItem menuItem, MenuItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();

            _menuItem = menuItem;

            //Parent = parent;
            //Parent?.Children.Add(this);

            //_children = new();
           // RegisterHamburgerMenuBarPaneOpenEvent();
        }
        #endregion

        #region Register HamburgerMenuBarPaneOpen Event
        private void RegisterHamburgerMenuBarPaneOpenEvent()
        {
            _eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>().Subscribe(async (hmbpoe) =>
            {
                IsPaneOpen = hmbpoe.IsPaneOpen;
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Properties
        public MenuItem MenuItem => _menuItem;
        public int GroupIndex { get; set; }
        public int Index { get; set; }
        public string WorkspaceViewEventName { get; set; }
        private bool IsNextNavigation => _menuItem.IsNextNavigation;
        private bool HasNavigationName => !string.IsNullOrEmpty(_menuItem.NavigationName);
        private bool IsNexOnNotLeaf => _menuItem.IsNexOnNotLeaf;
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

        public bool IsPaneOpen
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        }

        public PackIconKind IconKind =>
                    _menuItem.IconKind.ToPackIconKind();

        //public PackIconKind IconKind
        //{
        //    get
        //    {
        //        PackIconKind kind = PackIconKind.None;

        //        _ = Enum.TryParse(_menuItem.IconKind, out kind);

        //        return kind;
        //    }
        //}

        public bool IsEnabled
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        }
        #endregion

        #region Add View To RightContent Method
        public void AddViewToRightContent()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            ActiveContentManagerExtensions.AddViewToRandomContentAsync(_menuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
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

            ActiveContentManagerExtensions.NavigationToRandomContentAsync(_menuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar, new() { { "CurrentMenuItem", _menuItem } }).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
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
