using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Prism;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;

using Aksl.Modules.HamburgerMenuSideBarTab.ViewModels;
using Aksl.Modules.HamburgerMenuSideBarTab.Views;

namespace Aksl.Modules.HamburgerMenuSideBarTab;

public static class HamburgerMenuSideBarHelper
{
    #region Create Top HamburgerMenuSideBarViewModel Method
    public static async Task<HamburgerMenuSideBarViewModel> CreateTopHamburgerMenuSideBarViewModelAsync(IEnumerable<Infrastructure.MenuItem> menuItems)
    {
        NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();
        HamburgerMenuSideBarViewModel hamburgerMenuSideBar = new();

        if (menuItems is not null && menuItems.Any())
        {
            List<HamburgerMenuSideBarItemViewModel> allSideBarItemLeafs = new();

            foreach (var mi in menuItems)
            {
                HamburgerMenuSideBarItemViewModel virtualParent = new();
                Func<MenuItem, HamburgerMenuSideBarItemViewModel, HamburgerMenuSideBarItemViewModel> childResolver = ((m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); });

                var topItem = await nodeResolver.GetTopItemByMenuItemAsync(mi, virtualParent, childResolver, false);
                var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);
                allSideBarItemLeafs.AddRange(allTopItemLeafs);
            }

            hamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allSideBarItemLeafs);
        }

        return hamburgerMenuSideBar;
    }
    #endregion

    #region Add Views To LeftPane Method
    public static async Task AddViewsToLeftPaneAsync(HamburgerMenuSideBarItemViewModel topSideBarItem)
    {
        var leftPaneActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<SequenceActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);
        NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();

        // var sublLeafMenuItems = await topuSideBarItem.GetSubMenuAsync();
        var sublLeafMenuItems = await topSideBarItem.MenuItem.GetNextSubMenuAsync();

        if (sublLeafMenuItems is not null && sublLeafMenuItems.Any())
        {
            List<HamburgerMenuSideBarItemViewModel> allBarItemLeafs = new();

            string topItemName = default;
            foreach (var smi in sublLeafMenuItems)
            {
                var topItem = await nodeResolver.GetTopItemByMenuItemAsync(menuItem: smi, parent: topSideBarItem, childResolver: (m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); }, isKeepParent: true);
                topItemName = topItem.Path;
            }

            foreach (var topItem in topSideBarItem.Children)
            {
                var topItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem as HamburgerMenuSideBarItemViewModel);
                allBarItemLeafs.AddRange(topItemLeafs);
            }

            var subHamburgerMenuSideBar = new HamburgerMenuSideBarViewModel
            {
                AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allBarItemLeafs)
            };

            ContentInformation contentInfo = new()
            {
                Name = topItemName,
                Title = topItemName,
                ViewName = "Aksl.Modules.HamburgerMenuSideBar.Views.HamburgerMenuSideBarView,Aksl.Modules.HamburgerMenuSideBar",
                ViewElement = new HamburgerMenuSideBarView() { DataContext = subHamburgerMenuSideBar }
            };

            leftPaneActiveContentViewModel.Add(contentInfo);
        }
    }
    #endregion

    #region Get All SubMenuSideBar ViewModels Method

    #endregion

    #region Create HamburgerMenuSideBarViewModel Method

    #endregion

    #region Get SubMenu Method

    #endregion

    #region Add View To RightContent Method

    #endregion
}
