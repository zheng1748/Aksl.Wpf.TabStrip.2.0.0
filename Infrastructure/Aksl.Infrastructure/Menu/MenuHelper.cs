using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Prism;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace Aksl.Infrastructure;

public static class MenuHelper
{
    #region Get All LeafMenuItems Method
    public static async Task<IEnumerable<MenuItem>> GetLeafMenuItems(MenuItem menuItem)
    {
        var menuService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IMenuService>();
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
                currentMenuItem = await menuService.GetMenuAsync(currentMenuItem.NavigationName);
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

    #region Get All SubMenuSideBar ViewModels Method
    public static async Task<List<(string Path, MenuItem Menu)>> GetAllSubMenuSideBarViewModelsAsync(MenuItem menuItem)
    {
        List<(string Path, MenuItem MenuSideBar)> allMenuSideBars = new();

        await RecursiveSubMenuItemViewModel(menuItem);

        async Task RecursiveSubMenuItemViewModel(MenuItem currentMenuItem)
        {
            string topItemName = currentMenuItem.Name;

            var sublLeafMenuItems = await GetSubMenuAsync(currentMenuItem);

            if (sublLeafMenuItems is not null && sublLeafMenuItems.Any())
            {
                foreach (var sm in currentMenuItem.SubMenus)
                {

                    IEnumerable<MenuItem> allMenuItemLeafs = await GetLeafMenuItems(sm);

                    // allMenuSideBars.Add((topItemName, allMenuItemLeafs));

                    await RecursiveSubMenuItemViewModel(sm);
                }
            }
        }

        return allMenuSideBars;
    }
    #endregion

    #region Get SubMenu Method
    public static async Task<IEnumerable<Infrastructure.MenuItem>> GetSubMenuAsync(Infrastructure.MenuItem menuItem)
    {
        var menuService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IMenuService>();

        IEnumerable<Infrastructure.MenuItem> subMenuItems = default;

        if (!string.IsNullOrEmpty(menuItem.NavigationName))
        {
            var parentMenuItem = await menuService.GetMenuAsync(menuItem.NavigationName);
            subMenuItems = parentMenuItem.SubMenus;
        }

        if (string.IsNullOrEmpty(menuItem.NavigationName) && HasSubMenu(menuItem) && IsExistsViewInSubMenu(menuItem))
        {
            subMenuItems = menuItem.SubMenus.Where(sm => !string.IsNullOrEmpty(sm.ViewName)).ToList();
        }

        bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

        bool IsExistsViewInSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));

        return subMenuItems;
    }
    #endregion

    #region Contain Methods
    private static bool AnyEqualsMenuItems(IEnumerable<MenuItem> menuItems, MenuItem menuItem)
    {
        var isAny = menuItems.Any(mi => IsEqualsNameOrTitle(mi.Name, menuItem.Name) || IsEqualsNameOrTitle(mi.Title, menuItem.Title));

        return isAny;
    }

    private static bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
    {
        if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
        {
            return false;
        }

        var isAny = (!string.IsNullOrEmpty(nameOrTitle) && nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(otherNameOrTitle) && otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase));

        return isAny;
    }
    #endregion
}
