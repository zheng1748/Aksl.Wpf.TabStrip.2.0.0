using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.TabStrip;
using Aksl.TabStrip.ViewModels;
using Aksl.TabStrip.Views;
using Aksl.Toolkit.Controls;
using Prism.Common;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Unity;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace Aksl.Modules.HamburgerMenuSideBarTab.ViewModels;

public class HamburgerMenuSideBarItemViewModel : Mvvm.NodeViewModel
{
    #region Members
    protected readonly IEventAggregator _eventAggregator;
    private readonly IDialogViewService _dialogViewService;
    private readonly IMenuService _menuService;
    private readonly Aksl.Infrastructure.MenuItem _menuItem;
    #endregion

    #region Constructors
    public HamburgerMenuSideBarItemViewModel() : base()
    {
        _eventAggregator = PrismUnityExtensions.GetEventAggregator();
        _dialogViewService = PrismUnityExtensions.GetDialogViewService();
        _menuService = PrismUnityExtensions.GetMenuService();

        _menuItem = null;
    }

    public HamburgerMenuSideBarItemViewModel(Aksl.Infrastructure.MenuItem menuItem, HamburgerMenuSideBarItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
    {
        _eventAggregator = PrismUnityExtensions.GetEventAggregator();
        _dialogViewService = PrismUnityExtensions.GetDialogViewService();
        _menuService = PrismUnityExtensions.GetMenuService();

        _menuItem = menuItem;
    }
    #endregion

    #region Properties
    public Aksl.Infrastructure.MenuItem MenuItem => _menuItem;
    public string NavigationName => _menuItem.NavigationName;
    public bool IsSelectedOnInitialize => _menuItem.IsSelectedOnInitialize;
    public PackIconKind IconKind =>
                      _menuItem.IconKind.ToPackIconKind();
    public bool HasSubMenu =>
                       _menuItem.HasNextSubMenu();
    public bool HasViewName =>
                       _menuItem.HasViewName();
    public bool IsShowArrow =>
              IsNavigationToRightTabContent || IsAddViewsToRightTabContent;
    public bool IsNavigationToRightTabContent =>
                            IsLeaf && _menuItem.HasNextSubMenu() && _menuItem.HasViewName() && _menuItem.IsNexApplication;
    public bool IsAddViewsToRightTabContent =>
                         IsLeaf && _menuItem.HasNextSubMenu() && !_menuItem.HasViewName() && !_menuItem.IsNexApplication;
    public bool IsAddViewToRightTabContent =>
                         IsLeaf && !_menuItem.HasNextSubMenu() && _menuItem.HasViewName() && !_menuItem.IsNexApplication;

    public bool IsSelected
    {
        get;
        set
        {
            if (SetProperty<bool>(ref field, value))
            {
                if (field && IsAddViewToRightTabContent)
                {
                    AddViewToRightTabContentAsync(_menuItem).Await();
                }

                if (field && IsAddViewsToRightTabContent)
                {
                    AddViewsToRightTabContentAsync(_menuItem).Await();
                }

                if (field && IsNavigationToRightTabContent)
                {
                    NavigationToRightTabContentAsync(_menuItem).Await();
                }

                //if (field && IsLeaf)
                //{
                //    AddViewToRightTabContent().Await();
                //}
            }
        }
    } = false;

    public bool IsPaneOpen
    {
        get => field;
        set => SetProperty<bool>(ref field, value);
    } = true;

    public bool IsEnabled
    {
        get => field;

        set => SetProperty<bool>(ref field, value);
    } = true;
    #endregion

    #region Add View To Right TabContent Method
    private async Task AddViewToRightTabContentAsync(MenuItem menuItem, string activeContentName = ActiveContentNames.TabStripHamburgerMenuSideBarRightContent)
    {
        try
        {
            var topTabViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabViewModel>(name: activeContentName);
            if (topTabViewModel.IsActiveTabItemByName(menuItem.Name))
            {
                return;
            }

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
        catch (Exception ex) when (!string.IsNullOrEmpty(ex.InnerException?.Message))
        {
            await _dialogViewService.AlertAsync(message: $"{ex.InnerException.Message}", title: $"Error:Add Tab View");
        }
        catch (Exception ex)
        {
            await _dialogViewService.AlertAsync(message: $"{ex.Message}", title: $"Error:Add Tab View");
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

            _dialogViewService.AlertAsync(message: $"Unable to find \"{msg}\".", title: $"Error:Missing Type").Await();
        }
    }
    #endregion

    #region Add Views To RightTab Method
    public async Task AddViewsToRightTabContentAsync(MenuItem menuItem,string activeContentName= ActiveContentNames.TabStripHamburgerMenuSideBarRightContent)
    {
        try
        {
            var topTabViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabViewModel>(name: activeContentName);
            if (topTabViewModel.IsActiveTabItemByName(menuItem.Name))
            {
                return;
            }

            var topTabView = topTabViewModel.GetStoreViewElementByName(menuItem.Name) as TabView;
            if (topTabView is null)
            {
                CreateTopTabView(menuItem, topTabViewModel);
                await AddSubTabViewAsync(menuItem, topTabViewModel);
            }
            else
            {
                CreateTopTabView(menuItem, topTabViewModel);
                await AddSubTabViewAsync(menuItem, topTabViewModel);
            }
        }
        catch (Exception ex) when (!string.IsNullOrEmpty(ex.InnerException?.Message))
        {
            await _dialogViewService.AlertAsync(message: $"{ex.InnerException.Message}", title: $"Error:Add Tab Views");
        }
        catch (Exception ex)
        {
            await _dialogViewService.AlertAsync(message: $"{ex.Message}", title: $"Error:Add Tab Views");
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

    #region Navigation To RightTabContent Method
    public async Task NavigationToRightTabContentAsync(MenuItem menuItem, string activeContentName = ActiveContentNames.TabStripHamburgerMenuSideBarRightContent)
    {
        try
        {
            var topTabViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabViewModel>(name: activeContentName);
            if (topTabViewModel.IsActiveTabItemByName(menuItem.Name))
            {
                return;
            }

            TabInformation tabInfo = CreateTabInformation(menuItem.Name, menuItem.Title, menuItem.ViewName, new() { { "CurrentMenuItem", _menuItem } });

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
        catch (Exception ex) when (!string.IsNullOrEmpty(ex.InnerException?.Message))
        {
            await _dialogViewService.AlertAsync(message: $"{ex.InnerException.Message}", title: $"Error: Navigation To TabContent");
        }
        catch (Exception ex)
        {
            await _dialogViewService.AlertAsync(message: $"{ex.Message}", title: $"Error:Navigation To TabContent");
        }
    }
    #endregion

    #region Create TabHeaderedContentInformation Method
    public TabInformation CreateTabInformation(string name, string title, string viewTypeAssemblyQualifiedName, NavigationParameters navigationParameters)
    {
        Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
        if (viewType is null)
        {
            throw new ArgumentException($"Missing Type {viewTypeAssemblyQualifiedName}");
        }
        var viewName = viewType.Name;

        var unityContainer = PrismIocExtensions.GetUnityContainer();
        var regionNavigationService = unityContainer.Resolve<IRegionNavigationService>();

        TabInformation tabInfo = new()
        {
            Name = name,
            Title = title,
            ViewName = viewTypeAssemblyQualifiedName
        };

        var registeredView = unityContainer.Resolve<object>(viewName);
        if (registeredView is FrameworkElement frameworkElement)
        {
            MvvmHelpers.AutowireViewModel(registeredView);

            NavigationContext navigationContext = new(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute))
            {
                Parameters = navigationParameters
            };

            Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
            MvvmHelpers.ViewAndViewModelAction<INavigationAware>(registeredView, action);

            tabInfo.ViewName = null;
            tabInfo.ViewElement = frameworkElement;
        }

        return tabInfo;
    }
    #endregion
}

