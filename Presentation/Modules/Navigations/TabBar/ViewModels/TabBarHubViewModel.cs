using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Modules.TabBar.Views;
using Aksl.TabStrip;
using Aksl.TabStrip.ViewModels;
using Aksl.TabStrip.Views;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Unity;

namespace Aksl.Modules.TabBar.ViewModels
{
    public class TabBarHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        #endregion

        #region Constructors
        public TabBarHubViewModel()
        {
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();
            _menuService = PrismUnityExtensions.GetMenuService();
        }
        #endregion

        #region Properties
        public TabViewModel TopTabViewModel
        {
            get => field;
            set => SetProperty<TabViewModel>(ref field, value);
        } = new();

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        }
        #endregion

        #region Create TabViewModel Method
        private async Task CreateTabViewModelAsync(Aksl.Infrastructure.MenuItem menuItem)
        {
            IsLoading = true;

            try
            {
                #region Create TabViewModel Method
                //IEnumerable<MenuItem> subMenus = default;
                //List<MenuItem> allLeafMenuItems = new();

                //if (!string.IsNullOrEmpty(currentMenuItem.NavigationName))
                //{
                //    var parentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                //    subMenus = parentMenuItem.SubMenus;
                //}

                //if (string.IsNullOrEmpty(currentMenuItem.NavigationName) && HasSubMenu(currentMenuItem) && IsExistsViewInSubMenu(currentMenuItem))
                //{
                //    subMenus = currentMenuItem.SubMenus.Where(sm => !string.IsNullOrEmpty(sm.ViewName)).ToList();
                //}

                //bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

                //bool IsExistsViewInSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));

                //if (subMenus is not null)
                //{
                //    await GetLeafMenuItemsAsync();
                //    AddTabViewModels();
                //}

                //async Task GetLeafMenuItemsAsync()
                //{
                //    foreach (var smi in subMenus)
                //    {
                //        var leafMenuItems = await GetAllLeafMenuItemsAsync(smi);
                //        allLeafMenuItems.AddRange(leafMenuItems);
                //    }
                //}

                //void AddTabViewModels()
                //{
                //    foreach (var mi in allLeafMenuItems)
                //    {
                //        TabInformation tabInformation = new()
                //        {
                //            Name = mi.Name,
                //            Title = mi.Title,
                //            IconKind = mi.IconKind,
                //            ViewName = mi.ViewName,
                //            CloseTabButtonVisibility = Visibility.Collapsed
                //        };

                //        TabViewModel.Add(tabInformation);
                //    }

                //    TabViewModel.SetFirstActiveTabItem();
                //}
                #endregion

                if (menuItem.HasNextSubMenu())
                {
                    IEnumerable<Aksl.Infrastructure.MenuItem> subMenu = await menuItem.GetNextSubMenuAsync();
                    foreach (var smi in subMenu)
                    {
                        var subTabView = TopTabViewModel.GetStoreViewElementByName(menuItem.Name) as TabView;
                        if (subTabView is null)
                        {
                            CreateTopTabView(smi, TopTabViewModel);

                            await AddSubTabViewAsync(smi, TopTabViewModel);
                        }
                        //else
                        //{
                        //    CreateTopTabView(smi, TopTabViewModel);

                        //    await AddSubTabViewAsync(smi, TopTabViewModel);
                        //}
                    }

                    TopTabViewModel.SetFirstActiveTabItem();

                }
                else if (menuItem.HasViewName())
                {
                    TabStripManager.Instance.AddViewToTabContent(menuItem, TopTabViewModel);
                }
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Unable to create tab view : \"{ex.Message}\"", title: "Error: Create TabView");
            }
            finally
            {
                if (IsLoading)
                {
                    IsLoading = false;
                }
            }
        }
        #endregion

        #region Add View To RightTab Method
        private void CreateTopTabView(MenuItem menuItem, TabViewModel tabViewModel)
        {
            TabInformation topTabInfo = new()
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
                                Aksl.TabStrip.TabInformation subTabInformation = new()
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

        #region Get All LeafMenuItems Method
        private async Task<IEnumerable<MenuItem>> GetAllLeafMenuItemsAsync(MenuItem menuItem)
        {
            List<MenuItem> leafMenuItems = new();

            //if (HasSubMenu(menuItem))
            //{
            //    foreach (var smi in menuItem.SubMenus)
            //    {
            //        await RecursiveSubMenuItem(smi);
            //    }
            //}
            //else if (HasNavigationName(menuItem) && IsLeaf(menuItem))
            //{
            //    var root = await _menuService.GetMenuAsync(menuItem.NavigationName);
            //    foreach (var smi in root.SubMenus)
            //    {
            //        await RecursiveSubMenuItem(smi);
            //    }
            //}
            //else
            //{
            //    await RecursiveSubMenuItem(menuItem);
            //}

            await RecursiveSubMenuItem(menuItem);

            async Task RecursiveSubMenuItem(MenuItem currentMenuItem)
            {
                //if (!AnyEqualsMenuItem(leafMenuItems, currentMenuItem) && IsLeaf(currentMenuItem) && HasTitle(currentMenuItem))
                //{
                //    leafMenuItems.Add(currentMenuItem);
                //}

                var isAddOnLeaf = IsLeaf(currentMenuItem) && (!HasNavigationName(currentMenuItem) || (HasNavigationName(currentMenuItem) && !IsNextNavigation(currentMenuItem)));
                var isAddOnNotLeaf = !IsLeaf(currentMenuItem) && !IsNexOnNotLeaf(currentMenuItem);
                //  if (!AnyEqualsMenuItem(leafMenuItems, currentMenuItem) && IsLeaf(currentMenuItem) && !HasNavigationName(currentMenuItem) && HasTitle(currentMenuItem))
                if (!AnyEqualsMenuItem(leafMenuItems, currentMenuItem) && HasTitle(currentMenuItem) && (isAddOnLeaf || isAddOnNotLeaf))
                {
                    leafMenuItems.Add(currentMenuItem);
                }

                // if (HasNavigationName(currentMenuItem) && IsLeaf(currentMenuItem))
                // if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem) && IsLeaf(currentMenuItem))
                if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
                {
                    currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                }

                //if (HasSubMenu(currentMenuItem))
                if (HasSubMenu(currentMenuItem) && IsNexOnNotLeaf(currentMenuItem))
                {
                    foreach (var smi in currentMenuItem.SubMenus)
                    {
                        await RecursiveSubMenuItem(smi);
                    }
                }
            }

            bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            bool IsLeaf(MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

            bool HasTitle(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

            bool IsNextNavigation(MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

            bool HasNavigationName(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

            bool IsNexOnNotLeaf(MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

            return leafMenuItems;
        }
        #endregion

        #region Contain Methods
        private bool AnyEqualsMenuItem(IEnumerable<MenuItem> menuItems, MenuItem menuItem)
        {
            var isEquals = menuItems.Any(mi => IsEqualsNameOrTitle(mi.Title, menuItem.Title) || IsEqualsNameOrTitle(mi.Name, menuItem.Name));

            return isEquals;
        }

        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isEquals = (!string.IsNullOrEmpty(nameOrTitle) && otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase)) ||
                           (!string.IsNullOrEmpty(nameOrTitle) && otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase));

            return isEquals;
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            if (parameters.TryGetValue("CurrentMenuItem", out MenuItem currentMenuItem))
            {
                CreateTabViewModelAsync(currentMenuItem).Await();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
