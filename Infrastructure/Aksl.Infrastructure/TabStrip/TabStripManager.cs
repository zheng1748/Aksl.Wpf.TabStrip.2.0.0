using Aksl.Tabs;
using Aksl.Tabs.ViewModels;
using Aksl.Tabs.Views;
using Prism;
using Prism.Common;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Unity;

namespace Aksl.Infrastructure;

public class TabStripManager
{
    #region Constructors
    public static TabStripManager Instance { get; }
    static TabStripManager()
    {
        Instance = new TabStripManager();
    }
    #endregion

    #region Create TabInformation Method

    #endregion

    #region Add View To Tab Content Method
    public async Task AddViewToRightTabContent(Infrastructure.MenuItem menuItem, TabViewModel topTabViewModel)
    {
        if (topTabViewModel.IsActiveTabItemByName(menuItem.Name))
        {
            return;
        }

        if (menuItem.HasNextSubMenu())
        {
            CreateTopTabView(menuItem, topTabViewModel);

            await AddSubTabViewAsync(menuItem, topTabViewModel);
        }
        else if (menuItem.HasViewName())
        {
            AddViewToTabContent(menuItem, topTabViewModel);
        }
    }

    private void AddViewToTabContent(MenuItem menuItem, TabViewModel topTabViewModel)
    {
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
        catch (Exception ex)
        {
            string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

            throw new Exception(msg);
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
                            try
                            {
                                var viewTypeName = lmi.GetViewTypeName();

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
                            catch (Exception ex)
                            {
                                string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

                                throw new Exception(msg);
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