using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;
using Aksl.Infrastructure;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public class GroupedMenusViewModel : BindableBase
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        private int _currentGroupeIndex = -1;
        #endregion

        #region Constructors
        public GroupedMenusViewModel()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();

            _menuService = PrismUnityExtensions.GetMenuService();

            GroupedMenus = new();
            NoGroupedMenus = new();
            AllMenus = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<GroupedMenuViewModel> GroupedMenus { get; }
        public ObservableCollection<NoGroupedMenuViewModel> NoGroupedMenus { get; }
        public ObservableCollection<GroupedMenuViewModelBase> AllMenus { get; }
        //public string WorkspaceViewEventName { get; set; }

        public MenuItemViewModel SelectedMenuItem
        {
            get => field;
            set
            {
                if (SetProperty(ref field, value))
                {
                    if (field is not null)
                    {
                        ClearSelectedNoGroupedMenuItem();
                    }
                }
            }
        }

        public MenuItemViewModel SelectedNoGroupedMenuItem
        {
            get => field;
            set
            {
                if (SetProperty(ref field, value))
                {
                    if (field is not null)
                    {
                        ClearSelectedMenuItem();
                    }
                }
            }
        }

        public bool IsPaneOpen
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    foreach (var ngmvm in NoGroupedMenus)
                    {
                        ngmvm.IsPaneOpen = field;
                    }

                    foreach (var gmvm in GroupedMenus)
                    {
                        gmvm.IsPaneOpen = field;
                    }
                }
            }
        }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        }
        #endregion

        #region Register PropertyChanged Methods
        private void AddGroupedMenuViewModelPropertyChanged(GroupedMenuViewModel groupedMenuViewModel)
        {
            groupedMenuViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is GroupedMenuViewModel gmvm)
                {
                    //if (e.PropertyName == nameof(GroupedMenuViewModel.IsLoading))
                    //{
                    //    //最后一个
                    //    if (gmvm.GroupIndex == GroupedMenus.Count()-1 && !gmvm.IsLoading)
                    //    {
                    //        IsLoading = false;
                    //    }
                    //}

                    if (e.PropertyName == nameof(GroupedMenuViewModel.SelectedMenuItem))
                    {
                        if (_currentGroupeIndex == gmvm.GroupIndex)
                        {
                            //SelectedMenuItem = gmvm.MenuContent.SelectedMenuItem;
                            if ((gmvm.SelectedMenuItem is not null && gmvm.SelectedMenuItem.IsSelected) && SelectedMenuItem != gmvm.SelectedMenuItem)
                            {
                                SelectedMenuItem = gmvm.SelectedMenuItem;
                            }
                        }
                        else
                        {
                            foreach (var gm in GroupedMenus)
                            {
                                if (_currentGroupeIndex == gm.GroupIndex)
                                {
                                    // _previewSelectedMenuItem = gm.MenuContent.SelectedMenuItem;
                                    gm.MenuContent.ClearSelectedMenuItem();

                                    break;
                                }
                            }

                            _currentGroupeIndex = gmvm.GroupIndex;
                            if ((gmvm.SelectedMenuItem is not null && gmvm.SelectedMenuItem.IsSelected) && SelectedMenuItem != gmvm.SelectedMenuItem)
                            {
                                SelectedMenuItem = gmvm.SelectedMenuItem;
                            }
                            //SelectedMenuItem = gmvm.MenuContent.SelectedMenuItem;
                        }
                    }
                }
            };
        }

       private void AddNoGroupedMenuViewModelPropertyChanged(NoGroupedMenuViewModel noGroupedMenuViewModel)
        {
            noGroupedMenuViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is NoGroupedMenuViewModel ngmvm)
                {
                    //if (e.PropertyName == nameof(NoGroupedMenuViewModel.IsLoading))
                    //{
                    //    //最后一个
                    //    if (ngmvm.Index == NoGroupedMenus.Count()-1 && !ngmvm.IsLoading)
                    //    {
                    //        IsLoading = false;
                    //    }
                    //}

                    if (e.PropertyName == nameof(NoGroupedMenuViewModel.SelectedNoGroupedMenuItem))
                    {
                        if (SelectedNoGroupedMenuItem is null &&
                           (ngmvm.SelectedNoGroupedMenuItem is not null && ngmvm.SelectedNoGroupedMenuItem.IsSelected && ngmvm.SelectedNoGroupedMenuItem != SelectedNoGroupedMenuItem))
                        {
                            SelectedNoGroupedMenuItem = ngmvm.SelectedNoGroupedMenuItem;
                        }

                        if (SelectedNoGroupedMenuItem is not null &&
                            (ngmvm.SelectedNoGroupedMenuItem is not null && ngmvm.SelectedNoGroupedMenuItem.IsSelected && ngmvm.SelectedNoGroupedMenuItem != SelectedNoGroupedMenuItem))
                        {
                            SelectedNoGroupedMenuItem.IsSelected = false;

                            SelectedNoGroupedMenuItem = ngmvm.SelectedNoGroupedMenuItem;
                        }
                    }
                }
            };
        }
        #endregion

        #region Clear Selected NoGroupedMenuItem Method
        internal void ClearSelectedNoGroupedMenuItem()
        {
            if (SelectedNoGroupedMenuItem is not null)
            {
                // var noGroupedMenu = NoGroupedMenus.FirstOrDefault(ngm => ngm.NoGroupedMenuItems.Any(mi => IsEqualsNameOrTitle(mi.MenuItem.Title, SelectedNoGroupedMenuItem.MenuItem.Title) || IsEqualsNameOrTitle(mi.MenuItem.Name, SelectedNoGroupedMenuItem.MenuItem.Name)));
                var noGroupedMenu = NoGroupedMenus.FirstOrDefault(ngm => IsEqualsNoGroupedMenuViewModel(ngm, SelectedNoGroupedMenuItem));

                if (noGroupedMenu is not null)
                {
                    SelectedNoGroupedMenuItem = null;

                    noGroupedMenu.ClearSelectedNoGroupeMenuItem();
                }
            }
        }
        #endregion

        #region Reset/Clear Selected MenuItem Method
        internal void ClearSelectedMenuItem()
        {
            if (SelectedMenuItem is not null)
            {
                var groupedMenu = GroupedMenus.FirstOrDefault(gm => gm.MenuContent.MenuItems.Any(mi => IsEqualsNameOrTitle(mi.MenuItem.Title, SelectedMenuItem.MenuItem.Title) ||
                                                                                                       IsEqualsNameOrTitle(mi.MenuItem.Name, SelectedMenuItem.MenuItem.Name)));

                if (groupedMenu is not null)
                {
                    SelectedMenuItem = null;

                    groupedMenu.MenuContent.ClearSelectedMenuItem();
                    _currentGroupeIndex = -1;
                }
            }
        }

        internal void ResetSelectedMenuItem(MenuItemViewModel selectedMenuItemItem)
        {
            if (selectedMenuItemItem is not null)
            {
                //var previewgGoupedMenu = GroupedMenus.FirstOrDefault(gm => gm.MenuContent.MenuItems.Any(mi => IsEqualsNameOrTitle(mi.MenuItem.Title, _selectedMenuItemItem.MenuItem.Title) || IsEqualsNameOrTitle(mi.MenuItem.Name, _selectedMenuItemItem.MenuItem.Name)));

                //if (previewgGoupedMenu is not null)
                //{
                //    previewgGoupedMenu.MenuContent.ClearSelectedMenuItem();
                //}

                var groupedMenu = GroupedMenus.FirstOrDefault(gm => gm.MenuContent.MenuItems.Any(mi => IsEqualsNameOrTitle(mi.MenuItem.Title, selectedMenuItemItem.MenuItem.Title) || IsEqualsNameOrTitle(mi.MenuItem.Name, selectedMenuItemItem.MenuItem.Name)));

                if (groupedMenu is not null)
                {
                    groupedMenu.MenuContent.SelectedMenuItem = selectedMenuItemItem;
                  
                }
            }
        }
        #endregion

        #region Create GroupedMenu ViewModels Method
        internal async Task CreateGroupedMenuViewModelsAsync()
        {
            IsLoading = true;

            var rootMenuItem = await _menuService.GetMenuAsync("All");

            var subMenuItems = rootMenuItem.SubMenus;
            int index = 0;
            int groupIndex = 0;
            NodeResolver<MenuItemViewModel> nodeResolver = new();

            if (subMenuItems is not null && subMenuItems.Any())
            {
                foreach (var smi in subMenuItems)
                {
                    MenuItemViewModel virtualParent = new();
                    Func<MenuItem, MenuItemViewModel, MenuItemViewModel> childResolver = ((m, p) => { return new MenuItemViewModel(m, p); });
                    var topItem = await nodeResolver.GetTopItemByMenuItemAsync(smi, virtualParent, childResolver, false);
                    var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);

                    if (HasLeafMenus())
                    {
                        GroupedMenuViewModel groupedMenuViewModel = new(groupIndex++, topItem, allTopItemLeafs);

                        GroupedMenus.Add(groupedMenuViewModel);
                        AllMenus.Add(groupedMenuViewModel);

                        AddGroupedMenuViewModelPropertyChanged(groupedMenuViewModel);
                    }
                    else
                    {
                        NoGroupedMenuViewModel noGroupedMenuViewModel = new(index++, allTopItemLeafs);

                        NoGroupedMenus.Add(noGroupedMenuViewModel);
                        AllMenus.Add(noGroupedMenuViewModel);

                        AddNoGroupedMenuViewModelPropertyChanged(noGroupedMenuViewModel);
                    }

                    bool HasLeafMenus()
                    {
                        return !AnyEqualsMenuItemViewModels(allTopItemLeafs, topItem);
                    }
                }
            }

            IsLoading = false;
        }
        #endregion

        #region Get All LeafMenuItems Method
        private async Task<IEnumerable<MenuItem>> GetAllLeafMenuItems(MenuItem menuItem)
        {
            List<MenuItem> leafMenuItems = new();

            await RecursiveSubMenuItem(menuItem);

            async Task RecursiveSubMenuItem(MenuItem currentMenuItem)
            {
                var isAddOnLeaf = IsLeaf(currentMenuItem) && (!HasNavigationName(currentMenuItem) || (HasNavigationName(currentMenuItem) && !IsNextNavigation(currentMenuItem)));
                var isAddOnNotLeaf = !IsLeaf(currentMenuItem) && !IsNexOnNotLeaf(currentMenuItem);
                if (!AnyEqualsMenuItems(leafMenuItems, currentMenuItem) && HasTitle(currentMenuItem) && (isAddOnLeaf || isAddOnNotLeaf))
                {
                    leafMenuItems.Add(currentMenuItem);
                }

                if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
                {
                    currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                }

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
        private bool AnyEqualsMenuItems(IEnumerable<MenuItem> menuItems, MenuItem menuItem)
        {
            var isAny = menuItems.Any(mi => IsEqualsNameOrTitle(mi.Name, menuItem.Name) || IsEqualsNameOrTitle(mi.Title, menuItem.Title));

            return isAny;
        }

        private bool AnyEqualsMenuItemViewModels(IEnumerable<MenuItemViewModel> menuItemViewModels, MenuItemViewModel menuItemViewModel)
        {
            var isAny = menuItemViewModels.Any(mi => IsEqualsNameOrTitle(mi.Name, menuItemViewModel.Name) || IsEqualsNameOrTitle(mi.Title, menuItemViewModel.Title));

            return isAny;
        }

        private bool IsEqualsNoGroupedMenuViewModel(NoGroupedMenuViewModel noGroupedMenuViewModel, MenuItemViewModel moGroupedMenuItemViewModel)
        {
            if (noGroupedMenuViewModel.SelectedNoGroupedMenuItem is null || moGroupedMenuItemViewModel is null)
            {
                return false;
            }

            var isAny = IsEqualsNameOrTitle(noGroupedMenuViewModel.SelectedNoGroupedMenuItem.Name, moGroupedMenuItemViewModel.Name) ||
                        IsEqualsNameOrTitle(noGroupedMenuViewModel.SelectedNoGroupedMenuItem.Title, moGroupedMenuItemViewModel.Title);

            return isAny;
        }

        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isAny = nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase) ||
                        otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase);

            return isAny;
        }

        internal void GetLeafMenuItems(MenuItem currentMenuItem, IList<MenuItem> leafMenuItems)
        {
            if (Isleaf(currentMenuItem) && HasTitle(currentMenuItem))
            {
                leafMenuItems.Add(currentMenuItem);
            }

            if (currentMenuItem.SubMenus.Any())
            {
                RecursiveSubMenuItem(currentMenuItem);
            }

            void RecursiveSubMenuItem(MenuItem parentMenuItem)
            {
                foreach (var smi in parentMenuItem.SubMenus)
                {
                    if (!leafMenuItems.Contains(smi) && Isleaf(smi) && HasTitle(smi))
                    {
                        leafMenuItems.Add(smi);
                    }
                    RecursiveSubMenuItem(smi);
                }
            }

            bool Isleaf(MenuItem mi) => mi.SubMenus.Count <= 0;

            bool HasTitle(MenuItem mi) => !string.IsNullOrEmpty(mi.Title);
        }
        #endregion
    }
}
