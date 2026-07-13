using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Tabs;
using Aksl.Tabs.ViewModels;
using Aksl.Tabs.Views;
using Aksl.Toolkit.Controls;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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
        public Infrastructure.MenuItem MenuItem => _menuItem;
        public int GroupIndex { get; set; }
        public int Index { get; set; }
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
                    if (field && IsLeaf)
                    {
                       //AddViewToRightTabContent().Await();
                        AddViewToRightTabContentCore().Await();
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

        public bool IsEnabled
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        }
        #endregion

        #region Add View To RightTab Method
        private async Task AddViewToRightTabContentCore()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            try
            {
                var topTabViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabViewModel>(name: ActiveContentNames.TabStripHamburgerMenuNavigationSideBar);
                if (topTabViewModel is not null)
                {
                    await TabStripManager.Instance.AddViewToRightTabContent(_menuItem, topTabViewModel);
                }
            }
            catch (Exception ex) when (!string.IsNullOrEmpty(ex.InnerException?.Message))
            {
                await dialogViewService.AlertAsync(message: $"{ex.InnerException.Message}", title: $"Error:Add Ta View");
            }
            catch (Exception ex)
            {
                await dialogViewService.AlertAsync(message: $"{ex.Message}", title: $"Error:Add Ta View");
            }
        }
        #endregion

        #region Add View To RightTab Method
        public async Task AddViewToRightTabContent()
        {
            var topTabViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabViewModel>(name: ActiveContentNames.TabStripHamburgerMenuNavigationSideBar);

            if (topTabViewModel.IsActiveTabItemByName(_menuItem.Name))
            {
                return;
            }

            if (_menuItem.HasNextSubMenu())
            {
                CreateTopTabView(_menuItem, topTabViewModel);

                await AddSubTabViewAsync(_menuItem, topTabViewModel);
            }
            else if (_menuItem.HasViewName())
            {
                AddViewToTabContent(_menuItem, topTabViewModel);
            }
        }

        private void AddViewToTabContent(MenuItem menuItem, TabViewModel topTabViewModel)
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            try
            {
                var viewTypeName = menuItem.GetViewTypeName();

                TabInformation tabInfo = new()
                {
                    Name = menuItem.Name,
                    Title = menuItem.Title,
                    IconKind = menuItem.IconKind,
                    ViewName = menuItem.ViewName
                };

                var currentView = topTabViewModel.GetStoreViewElementByName(menuItem.Name);
                if (currentView is not null)
                {
                    if (menuItem.IsCacheable)
                    {
                        topTabViewModel.SetTabItem(tabInfo);
                    }
                    else
                    {
                        topTabViewModel.RetsetTabItem(tabInfo);
                    }
                }
                else
                {
                    topTabViewModel.Add(tabInfo);
                }
            }
            catch (Exception ex)when(string.IsNullOrEmpty(ex.InnerException?.Message))
            {
                string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

                dialogViewService.AlertAsync(message: $"Unable to find \"{msg}\".", title: $"Error:Missing Type").Await();
            }
        }

        private void CreateTopTabView(MenuItem menuItem, TabViewModel tabViewModel)
        {
            TabInformation topTabInfo = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                IconKind = menuItem.IconKind,
                ViewName = menuItem.ViewName,
                CloseTabButtonVisibility = Visibility.Visible
            };

            var currentView = tabViewModel.GetStoreViewElementByName(menuItem.Name);
            if (currentView is not null)
            {
                if (menuItem.IsCacheable)
                {
                    tabViewModel.SetActiveTabItemByName(menuItem.Name);
                }
                else
                {
                    tabViewModel.RetsetTabItem(topTabInfo);
                }
            }
            else
            {
                tabViewModel.Add(topTabInfo);
            }
        }

        private void CreateSubTopTabView(MenuItem menuItem, TabViewModel tabViewModel)
        {
            TabInformation tabInfo = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                IconKind = menuItem.IconKind,
                ViewName = menuItem.ViewName,
                CloseTabButtonVisibility = Visibility.Collapsed
            };

            var currentView = tabViewModel.GetStoreViewElementByName(menuItem.Name);
            if (currentView is not null)
            {
                if (menuItem.IsCacheable)
                {
                }
                else
                {
                    tabViewModel.RetsetTabItemNoActive(tabInfo);
                }
            }
            else
            {
                tabViewModel.Add(tabInfo, false);
            }
        }

        private async Task AddSubTabViewAsync(MenuItem menuItem, TabViewModel topTabViewModel)
        {
            await RecursiveSubMenuItemViewModelAsync(menuItem, topTabViewModel);

            async Task RecursiveSubMenuItemViewModelAsync(MenuItem currentMenuItem, TabViewModel currentTabViewModel)
            {
                var topTabItemViewModel = currentTabViewModel.GetStoreTabItemViewModelByName(currentMenuItem.Name);

                IEnumerable<MenuItem> nextSubMenu = await currentMenuItem.GetNextSubMenuAsync();
                if (nextSubMenu is not null && nextSubMenu.Any())
                {
                    TabViewModel subTabViewModel = new();
                    var subTabView = await FindTabViewByNameAsync(topTabViewModel, currentMenuItem.Name);
                    if (subTabView is null)
                    {
                        subTabView = new TabView
                        {
                            DataContext = subTabViewModel
                        };

                        topTabItemViewModel.ViewElement = subTabView;
                    }
                    else
                    {
                        // Debug.Assert(topTabItemViewModel.ViewElement == subTabView);
                        subTabViewModel = subTabView.DataContext as TabViewModel;
                    }

                    bool isSetFirst = false;

                    foreach (var smi in nextSubMenu)
                    {
                        var leafMenuItems = await smi.GetLeafMenuItems();
                        var isCurrent = leafMenuItems.IsCurrent(smi);

                        foreach (var lmi in leafMenuItems)
                        {
                            if (lmi.HasNextSubMenu())
                            {
                                CreateSubTopTabView(lmi, subTabViewModel);

                                await RecursiveSubMenuItemViewModelAsync(lmi, subTabViewModel);
                            }
                            else if (lmi.HasViewName())
                            {
                                Aksl.Tabs.TabInformation subTabInformation = new()
                                {
                                    Name = lmi.Name,
                                    Title = lmi.Title,
                                    IconKind = lmi.IconKind,
                                    ViewName = lmi.ViewName,
                                    CloseTabButtonVisibility = Visibility.Collapsed
                                };

                                var currentView = subTabViewModel.GetStoreViewElementByName(lmi.Name);
                                if (currentView is not null)
                                {
                                    if (lmi.IsCacheable)
                                    {
                                    }
                                    else
                                    {
                                        subTabViewModel.RetsetTabItemNoActive(subTabInformation);
                                    }
                                }
                                else
                                {
                                    subTabViewModel.Add(subTabInformation, false);
                                    isSetFirst = true;
                                }
                            }
                        }
                    }

                    if (isSetFirst)
                    {
                        subTabViewModel.SetFirstActiveTabItem();
                    }
                }
            }
        }

        private async Task<TabView> FindTabViewByNameAsync(TabViewModel topTabViewModel, string name)
        {
            TabView findTabView = default;

            await RecursiveSubMenuItemViewModel(topTabViewModel);
            async Task RecursiveSubMenuItemViewModel(TabViewModel currentTabViewModel)
            {
                var subTabViewModels = currentTabViewModel.StoreTabItems.Where(sti => sti.ViewElement is TabView).ToList();
                foreach (var subtvm in subTabViewModels)
                {
                    if (subtvm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        findTabView = subtvm.ViewElement as TabView;
                        return;
                    }
                    else
                    {
                        var nextTabViewModel = (subtvm.ViewElement as TabView).DataContext as TabViewModel;

                        await RecursiveSubMenuItemViewModel(nextTabViewModel);
                    }
                }
            }

            return findTabView;
        }
        #endregion
    }
}
