using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Aksl.Infrastructure;

public class NodeResolver<T> where T : Mvvm.NodeViewModel
{
    #region Members
    private readonly IMenuService _menuService;
    #endregion

    #region Constructors
    public NodeResolver()
    {
        _menuService = PrismUnityExtensions.GetMenuService();
    }
    #endregion

    #region Get MenuItem Leafs Method
    public async Task<IEnumerable<T>> GetMenuItemLeafsAsync(MenuItem menuItem, Func<MenuItem, T> childResolver)
    {
        List<MenuItem> travelMenuItems = new();
        List<T> menuItemLeafs = new();

        await RecursiveSubMenuItem(menuItem);

        async Task RecursiveSubMenuItem(MenuItem currentMenuItem)
        {
            var isAddOnLeaf = currentMenuItem.IsLeaf() && (!HasNavigationName(currentMenuItem) || (HasNavigationName(currentMenuItem) && !IsNextNavigation(currentMenuItem)));
            var isAddOnNotLeaf = !currentMenuItem.IsLeaf() && !IsNexOnNotLeaf(currentMenuItem);
            if (!AnyEqualsMenuItems(travelMenuItems, currentMenuItem) && HasTitle(currentMenuItem) && (isAddOnLeaf || isAddOnNotLeaf))
            {
                menuItemLeafs.Add(childResolver(currentMenuItem));
                travelMenuItems.Add(currentMenuItem);
            }

            if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
            {
                currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
            }

            if (currentMenuItem.HasSubMenu() && IsNexOnNotLeaf(currentMenuItem))
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

        return menuItemLeafs;
    }
    #endregion

    #region Get TopMenuItem Leafs Method
    public async Task<IEnumerable<T>> GetTopMenuItemLeafsAsync(MenuItem menuItem, T virtualParent, Func<MenuItem, T, T> childResolver)
    {
        List<MenuItem> travelMenuItems = new();
        List<T> topMenuItemLeafs = new();

        await RecursiveSubMenuItem(menuItem, virtualParent);

        async Task RecursiveSubMenuItem(MenuItem currentMenuItem, T paren)
        {
            T child = default;

            if (!AnyEqualsMenuItems(travelMenuItems, currentMenuItem))
            {
                travelMenuItems.Add(currentMenuItem);

                child = childResolver(currentMenuItem, paren);
            }

            if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
            {
                currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
            }

            if (currentMenuItem.HasSubMenu() && IsNexOnNotLeaf(currentMenuItem))
            {
                foreach (var smi in currentMenuItem.SubMenus)
                {
                    await RecursiveSubMenuItem(smi, child);
                }
            }
        }

        var topItem = virtualParent.Children.FirstOrDefault() as T;
        if (topItem is not null)
        {
            topItem.Parent = null;

            topMenuItemLeafs = await GetTopItemLeafsAsync(topItem);
        }

        bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

        bool IsLeaf(MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

        bool HasTitle(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

        bool IsNextNavigation(MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

        bool HasNavigationName(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

        bool IsNexOnNotLeaf(MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

        return topMenuItemLeafs;
    }
    #endregion

    #region Get TopItem By MenuItem Method
    public async Task<T> GetTopItemByMenuItemAsync(MenuItem menuItem, T parent, Func<MenuItem, T, T> childResolver, bool isKeepParent = false)
    {
        List<MenuItem> travelMenuItems = new();

        await RecursiveSubMenuItem(menuItem, parent);

        async Task RecursiveSubMenuItem(MenuItem currentMenuItem, T paren)
        {
            T child = default;

            if (!AnyEqualsMenuItems(travelMenuItems, currentMenuItem))
            {
                travelMenuItems.Add(currentMenuItem);

                child = childResolver(currentMenuItem, paren);
            }

            if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
            {
                currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
            }

            if (currentMenuItem.HasSubMenu() && IsNexOnNotLeaf(currentMenuItem))
            {
                foreach (var smi in currentMenuItem.SubMenus)
                {
                    await RecursiveSubMenuItem(smi, child);
                }
            }
        }

        bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

        bool IsLeaf(MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

        bool HasTitle(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

        bool IsNextNavigation(MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

        bool HasNavigationName(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

        bool IsNexOnNotLeaf(MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

        if (isKeepParent)
        {
            return parent as T;
        }
        else
        {
            var topHeaderItem = parent.Children.FirstOrDefault();
            if (topHeaderItem is not null)
            {
                topHeaderItem.Parent = null;
            }
            return topHeaderItem as T;
        }
    }
    #endregion

    #region Get TopItem Leafs Method
    public async Task<List<T>> GetTopItemLeafsAsync(T topHeaderItem)
    {
        List<T> leafsOfTopHeaderItem = new();

        await RecursiveSubMenuItemViewModel(topHeaderItem);

        async Task RecursiveSubMenuItemViewModel(T currenySubItem)
        {
            if (!AnyEqualsNodeViewModels(leafsOfTopHeaderItem, currenySubItem) && currenySubItem.IsLeaf && currenySubItem.HasTitle)
            {
                leafsOfTopHeaderItem.Add(currenySubItem);
            }

            if (currenySubItem.HasChildren)
            {
                foreach (var children in currenySubItem.Children)
                {
                    await RecursiveSubMenuItemViewModel(children as T);
                }
            }
        }

        return leafsOfTopHeaderItem;
    }
    #endregion

    #region Contain Methods
    private bool AnyEqualsNodeViewModels(IEnumerable<T> ts, T t)
    {
        if (ts is null || (ts is not null && !ts.Any()) || t is null)
        {
            return false;
        }

        var isAny = ts.Any(v => IsEqualsNameOrTitle(v.Name, t.Name) || IsEqualsNameOrTitle(v.Title, t.Title));

        return isAny;
    }

    private bool AnyEqualsMenuItems(IEnumerable<MenuItem> menuItems, MenuItem menuItem)
    {
        var isAny = menuItems.Any(mi => IsEqualsNameOrTitle(mi.Name, menuItem.Name) || IsEqualsNameOrTitle(mi.Title, menuItem.Title));

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
    #endregion
}


