using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Mvvm;
using Aksl.TabStrip.ViewModels;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Unity;

namespace Aksl.Modules.HamburgerMenuTreeSideBarTab.ViewModels
{
    public class HamburgerMenuTreeSideBarHubViewModel : HamburgerMenuViewModel, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        #endregion

        #region Constructors
        public HamburgerMenuTreeSideBarHubViewModel()
        {
            _container = PrismIocExtensions.GetUnityContainer();
            _eventAggregator = _container.Resolve<IEventAggregator>();
            _dialogViewService = _container.Resolve<IDialogViewService>();
            _menuService = _container.Resolve<IMenuService>();

            IsPaneOpen = true;
            SelectedDisplayMode = SplitViewDisplayMode.Inline;
            SelectedPlacement = SplitViewPanePlacement.Left;

            CreateTreeSideBarViewModelAsync().Await();

            RegisterActiveContent();

            RegisterHamburgerMenuBarPaneOpenEvent();
        }
        #endregion

        #region Properties
        public TabViewModel TabStripViewModel
        {
            get => field;
            set => SetProperty(ref field, value);
        }
        public TreeSideBarViewModel TreeSideBar { get; private set; }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;
        #endregion

        #region Register HamburgerMenuBarPaneOpen Event
        private void RegisterHamburgerMenuBarPaneOpenEvent()
        {
            _eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>().Subscribe(async (hmbpoe) =>
            {
                try
                {
                    IsPaneOpen = hmbpoe.IsPaneOpen;
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Subscribe PaneOpen Event Error.: \"{ex.Message}\"", title: "Error");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register ActiveContents Method
        private void RegisterActiveContent()
        {
            RegisterRightTabStrip();
            void RegisterRightTabStrip()
            {
                _container.RegisterSingleton(from: typeof(TabViewModel), to: typeof(TabViewModel), name: ActiveContentNames.TabStripHamburgerMenuTreeSideBarRightContent);
                var tabStripViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabViewModel>(name: ActiveContentNames.TabStripHamburgerMenuTreeSideBarRightContent);

                TabStripViewModel = tabStripViewModel;
            }
        }
        #endregion

        #region Create TreeSideBar ViewModel Method
        private async Task CreateTreeSideBarViewModelAsync()
        {
            try
            {
                IsLoading = true;

                var rootMenuItem = await _menuService.GetMenuAsync("All");
                var subMenuItems = rootMenuItem.SubMenus;
                NodeResolver<TreeSideBarItemViewModel> nodeResolver = new();
                List<TreeSideBarItemViewModel> topTreeSideBarItems = new();

                TreeSideBar = new();

                foreach (var smi in subMenuItems)
                {
                    TreeSideBarItemViewModel virtualParent = new();
                    Func<MenuItem, TreeSideBarItemViewModel, TreeSideBarItemViewModel> childResolver = ((m, p) => { return new TreeSideBarItemViewModel(m, p); });
                    var topItem = await nodeResolver.GetTopItemByMenuItemAsync(smi, virtualParent, childResolver, false);
                    TreeSideBar.TopTreeSideBarItems.Add(topItem);
                }

                TreeSideBar.SetPropertyChanged();
                RaisePropertyChanged(nameof(TreeSideBar));
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Unable to create tree bar : \"{ex.Message}\"", title: "Error: Create TreeSideBar");
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

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            if (parameters is not null)
            {
                if (parameters.Count == 0)
                {
                    // CreateTreeSideBarViewModelAsync().GetAwaiter().GetResult();
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
