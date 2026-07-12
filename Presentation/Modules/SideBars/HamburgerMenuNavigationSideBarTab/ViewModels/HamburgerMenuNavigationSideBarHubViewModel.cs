using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Mvvm;
using Aksl.Tabs.ViewModels;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Unity;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public class HamburgerMenuNavigationSideBarHubViewModel : HamburgerMenuViewModel, INavigationAware
    {
        #region Members 
        private readonly IRegionManager _regionManager;
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public HamburgerMenuNavigationSideBarHubViewModel()
        {
            _container = PrismIocExtensions.GetUnityContainer();
            _regionManager = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionManager>();
            _eventAggregator = _container.Resolve<IEventAggregator>();
            _dialogViewService = _container.Resolve<IDialogViewService>();
            _menuService = _container.Resolve<IMenuService>();

            IsPaneOpen = true;
            SelectedDisplayMode = SplitViewDisplayMode.CompactInline;
            SelectedPlacement = SplitViewPanePlacement.Left;

            CreateGroupedMenusViewModelAsync().Await();

            RegisterActiveContent();
            // RegisterPropertyChanged();
            RegisterHamburgerMenuBarPaneOpenEvent();
        }
        #endregion

        #region Properties
        public TabViewModel TabStripViewModel
        {
            get => field;
            set => SetProperty(ref field, value);
        }
        public GroupedMenusViewModel GroupedMenu { get; private set; }
         public MenuItemViewModel SelectedMenuItem { get;  set; }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;
        #endregion

        #region Register PropertyChanged Method
        //private void RegisterPropertyChanged()
        //{
        //    GroupedMenu.PropertyChanged += async (sender, e) =>
        //    {
        //        if (sender is GroupedMenusViewModel gmvm)
        //        {
        //            if (e.PropertyName == nameof(GroupedMenusViewModel.SelectedMenuItem)) 
        //            {
        //                if (gmvm.SelectedMenuItem is not null)
        //                {
        //                    //ActiveContentHelper.AddViewToContentAsync(gmvm.SelectedMenuItem.MenuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar).Await();

        //                    ActiveContentManagerExtensions.AddViewToContentAsync(gmvm.SelectedMenuItem.MenuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
        //                    {
        //                        System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
        //                        {
        //                            await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View");
        //                        });
        //                    });
        //                    //var result = await ActiveContentHelper.AddViewToContentAsync(gmvm.SelectedMenuItem.MenuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar);
        //                    //if (!result)
        //                    //{
        //                    //   // await _dialogViewService.AlertAsync(message: $"Unable to load view \"{gmvm.SelectedMenuItem.MenuItem.ViewName}\".", title: "Error: Load View");
        //                    //}
        //                }
        //            }

        //            if (e.PropertyName == nameof(GroupedMenusViewModel.SelectedNoGroupedMenuItem))
        //            {
        //                if (gmvm.SelectedNoGroupedMenuItem is not null)
        //                {
        //                    //ActiveContentHelper.AddViewToContentAsync(gmvm.SelectedNoGroupedMenuItem.MenuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar, _dialogViewService).Await();
        //                  //  ActiveContentHelper.AddViewToContentAsync(gmvm.SelectedNoGroupedMenuItem.MenuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar).Await();

        //                    ActiveContentManagerExtensions.AddViewToContentAsync(gmvm.SelectedNoGroupedMenuItem.MenuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
        //                    {
        //                        System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
        //                        {
        //                            await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View");
        //                        });
        //                    });
        //                }
        //            }
        //        }
        //    };
        //}
        #endregion

        #region HamburgerMenu Properties
        public override bool IsPaneOpen
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (GroupedMenu is not null)
                    {
                        GroupedMenu.IsPaneOpen = IsPaneOpen;
                    }

                    VisualState = GetVisualState();
                }
            }
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

        #region Register ActiveContents Method
        private void RegisterActiveContent()
        {
            RegisterRightTabStrip();
            void RegisterRightTabStrip()
            {
                _container.RegisterSingleton(from: typeof(TabViewModel), to: typeof(TabViewModel), name: ActiveContentNames.TabStripHamburgerMenuNavigationSideBar);
                var tabStripViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabViewModel>(name: ActiveContentNames.TabStripHamburgerMenuNavigationSideBar);

                TabStripViewModel = tabStripViewModel;
            }
        }
        #endregion

        #region Create GroupedMenus ViewModel Method
        private async Task CreateGroupedMenusViewModelAsync()
        {
            IsLoading = true;

            try
            {
                var rootMenuItem = await _menuService.GetMenuAsync("All");
                var subMenuItems = rootMenuItem.SubMenus;
                int index = 0;
                int groupIndex = 0;
                NodeResolver<MenuItemViewModel> nodeResolver = new();

                GroupedMenu = new();

                if (subMenuItems is not null && subMenuItems.Any())
                {
                    foreach (var smi in subMenuItems)
                    {
                        MenuItemViewModel virtualParent = new();
                        Func<MenuItem, MenuItemViewModel, MenuItemViewModel> childResolver = ((m, p) => { return new MenuItemViewModel(m, p); });
                        var topItem = await nodeResolver.GetTopItemByMenuItemAsync(smi, virtualParent, childResolver, false);
                        var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);

                        if (!IsCurrent())
                        {
                            GroupedMenuViewModel groupedMenuViewModel = new(groupIndex++, topItem, allTopItemLeafs);

                            GroupedMenu.GroupedMenus.Add(groupedMenuViewModel);
                            GroupedMenu.AllMenus.Add(groupedMenuViewModel);

                            GroupedMenu.AddGroupedMenuViewModelPropertyChanged(groupedMenuViewModel);
                        }
                        else
                        {
                            NoGroupedMenuViewModel noGroupedMenuViewModel = new(index++, allTopItemLeafs);

                            GroupedMenu.NoGroupedMenus.Add(noGroupedMenuViewModel);
                            GroupedMenu.AllMenus.Add(noGroupedMenuViewModel);

                            GroupedMenu.AddNoGroupedMenuViewModelPropertyChanged(noGroupedMenuViewModel);
                        }

                        bool IsCurrent()
                        {
                            var isCurrent = allTopItemLeafs.Count() == 1 && AnyEqualsMenuItemViewModels(allTopItemLeafs, topItem);

                            return isCurrent;
                        }
                    }
                }

                GroupedMenu.IsPaneOpen = IsPaneOpen;
                RaisePropertyChanged(nameof(GroupedMenu));
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Unable to create grouped menu : \"{ex.Message}\"", title: "Error: Create GroupedMenu");
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

        #region Contain Methods
        private bool AnyEqualsMenuItemViewModels(IEnumerable<MenuItemViewModel> menuItemViewModels, MenuItemViewModel menuItemViewModel)
        {
            var isAny = menuItemViewModels.Any(mi => IsEqualsNameOrTitle(mi.Name, menuItemViewModel.Name) || IsEqualsNameOrTitle(mi.Title, menuItemViewModel.Title));

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

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            if (parameters is not null)
            {
                if (parameters.Count == 0)
                {
                   // CreateGroupedMenusViewModelAsync().Await();
                }
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
