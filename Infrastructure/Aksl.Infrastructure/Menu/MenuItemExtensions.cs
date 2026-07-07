using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;

using Unity;

using Aksl.Toolkit.Controls;

namespace Aksl.Infrastructure
{
    public static class MenuItemExtensions
    {
        #region Is ExistsView In SubMenu Method
        public static bool IsExistsViewInSubMenu(this Infrastructure.MenuItem menuItem) =>
                                (menuItem is not null) && menuItem.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));
        #endregion

        #region Has ViewName Method
        public static bool HasViewName(this Infrastructure.MenuItem menuItem) =>
                                  (menuItem is not null) && !string.IsNullOrEmpty(menuItem.ViewName);
        #endregion

        #region Has Next SubMenu Method
        public static bool HasNextSubMenu(this Infrastructure.MenuItem menuItem)
        {
            //var hasNextSubMenu = (!string.IsNullOrEmpty(currentMenuItem.NavigationName)) || (string.IsNullOrEmpty(currentMenuItem.NavigationName) && HasSubMenu(currentMenuItem) && IsExistsViewInSubMenu(currentMenuItem));

            //bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            //bool IsExistsViewInSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));

            var hasNextSubMenu = (!string.IsNullOrEmpty(menuItem.NavigationName)) || (string.IsNullOrEmpty(menuItem.NavigationName) && menuItem.HasSubMenu() && menuItem.IsExistsViewInSubMenu());

            return hasNextSubMenu;
        }

        public static bool IsNexApplication(this Infrastructure.MenuItem menuItem) => 
                                       (menuItem is not null) && menuItem.IsNexApplication();
        #endregion

        #region Get ViewTypeName Method
        public static Type GetViewType(this Infrastructure.MenuItem menuItem)
        {
            string viewTypeAssemblyQualifiedName = menuItem.ViewName;
            Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);

            return viewType;
        }

        public static string GetViewTypeName(this Infrastructure.MenuItem menuItem)
        {
            string viewTypeAssemblyQualifiedName = menuItem.ViewName;
            Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);

            if (viewType is null)
            {
                throw new ArgumentException($"Missing Type {menuItem.ViewName}");
            }

            return viewType.Name;
        }
        #endregion

        #region Get Next SubMenu Method
        public static async Task<IEnumerable<Infrastructure.MenuItem>> GetNextSubMenuAsync(this Infrastructure.MenuItem menuItem)
        {
            var menuService = PrismIocExtensions.GetUnityContainer().Resolve<IMenuService>();

            IEnumerable<Infrastructure.MenuItem> subMenuItems = new List<Infrastructure.MenuItem>();

            if (!string.IsNullOrEmpty(menuItem.NavigationName))
            {
                var parentMenuItem = await menuService.GetMenuAsync(menuItem.NavigationName);
                subMenuItems = parentMenuItem.SubMenus;
            }

            // if (string.IsNullOrEmpty(currentMenuItem.NavigationName) && HasSubMenu(currentMenuItem) && IsExistsViewInSubMenu(currentMenuItem))
            if (string.IsNullOrEmpty(menuItem.NavigationName) && menuItem.HasSubMenu() && menuItem.IsExistsViewInSubMenu())
            {
                subMenuItems = menuItem.SubMenus.Where(sm => !string.IsNullOrEmpty(sm.ViewName)).ToList();
            }

            // bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            //bool IsExistsViewInSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));

            return subMenuItems;
        }
        #endregion

        #region Get IconKind Method
        //public static PackIconKind GetIconKind(this Infrastructure.MenuItem menuItem)
        //{
        //    PackIconKind kind = PackIconKind.None;

        //    _ = Enum.TryParse(menuItem.IconKind, out kind);

        //    return kind;
        //}
        #endregion

        #region Has SubMenu Method
        public static bool HasSubMenu(this Infrastructure.MenuItem menuItem) =>
                                  (menuItem is not null) && menuItem.SubMenus.Any();
        #endregion

        #region Is Leaf Method
        public static bool IsLeaf(this Infrastructure.MenuItem menuItem) =>
                                  (menuItem is not null) && menuItem.SubMenus.Count <= 0;
        #endregion

        #region Get LeafMenuItems Method
        public static async Task<IEnumerable<Infrastructure.MenuItem>> GetLeafMenuItems(this Infrastructure.MenuItem menuItem)
        {
            var menuService = PrismIocExtensions.GetUnityContainer().Resolve<IMenuService>();
            List<Infrastructure.MenuItem> leafMenuItems = new();

            await RecursiveSubMenuItem(menuItem);

            async Task RecursiveSubMenuItem(MenuItem currentMenuItem)
            {
                var isAddOnLeaf = currentMenuItem.IsLeaf() && (!HasNavigationName(currentMenuItem) || (HasNavigationName(currentMenuItem) && !IsNextNavigation(currentMenuItem)));
                var isAddOnNotLeaf = !currentMenuItem.IsLeaf() && !IsNexOnNotLeaf(currentMenuItem);

                if (!AnyEqualsMenuItems(leafMenuItems, currentMenuItem) && HasTitle(currentMenuItem) && (isAddOnLeaf || isAddOnNotLeaf))
                {
                    leafMenuItems.Add(currentMenuItem);
                }

                if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
                {
                    currentMenuItem = await menuService.GetMenuAsync(currentMenuItem.NavigationName);
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

            return leafMenuItems;
        }
        #endregion

        #region Is Current Method
        public static bool IsCurrent(this IEnumerable<MenuItem> leafMenuItems, Infrastructure.MenuItem menuItem)
        {
            var isCurrent = leafMenuItems.Count() == 1 && AnyEqualsMenuItems(leafMenuItems, menuItem);

            return isCurrent;
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
}
