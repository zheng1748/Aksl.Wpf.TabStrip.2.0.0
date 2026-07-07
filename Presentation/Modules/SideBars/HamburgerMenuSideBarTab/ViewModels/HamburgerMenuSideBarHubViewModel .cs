using Aksl.ActiveContents.ViewModels;
using Aksl.ActiveContents.Views;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.HamburgerMenuSideBarTab.Views;
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
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Unity;

namespace Aksl.Modules.HamburgerMenuSideBarTab.ViewModels
{
    public class HamburgerMenuSideBarHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
       // private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarHubViewModel()
        {
            _container = PrismIocExtensions.GetUnityContainer();
            _eventAggregator = _container.Resolve<IEventAggregator>();
            _dialogViewService = _container.Resolve<IDialogViewService>();
            _menuService = _container.Resolve<IMenuService>();

            IsPaneOpen = true;
            SelectedDisplayMode = SplitViewDisplayMode.CompactInline;
            SelectedPlacement = SplitViewPanePlacement.Left;

            RegisterHamburgerMenuBarPaneOpenEvent();

            RegisterActiveContentAsync().Await();
        }
        #endregion

        #region Properties
        public SequenceActiveContentViewModel LeftPaneActiveContentViewModel { get; set; }
        public TabViewModel TabStripViewModel
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public HamburgerMenuSideBarViewModel TopHamburgerMenuSideBar { get; set; }

        public ActiveContentItemViewModel SelectedLeftPaneActiveContentItem
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public HamburgerMenuSideBarViewModel SelectedHamburgerMenuSideBar
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public HamburgerMenuSideBarItemViewModel SelectedHamburgerMenuSideBarItem
        {
            get;
            set => SetProperty(ref field, value);
        }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;

        public bool CanMove =>
                      LeftPaneActiveContentViewModel.CanMove;

        public Visibility MoveButtonVisibility
        {
            get
            {
                field = Visibility.Collapsed;
                return field;
            }
        } = Visibility.Collapsed;
        #endregion

        #region HamburgerMenu Properties
      //  private Brush _paneBackground = new SolidColorBrush(Colors.Transparent);
        private Brush _paneBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D3D3D3"));
        public Brush PaneBackground
        {
            get => _paneBackground;
            set => SetProperty<Brush>(ref _paneBackground, value);
        }

        public GridLength OpenPaneGridLength
        {
            get { return new GridLength(OpenPaneLength); }
        }

        private double _openPaneLength = 320d;
        public double OpenPaneLength
        {
            get => _openPaneLength;
            set => SetProperty<double>(ref _openPaneLength, value);
        }

        public GridLength CompactPaneGridLength
        {
            get { return new GridLength(CompactPaneLength); }
        }

        private double _compactPaneLength = 48d;
        public double CompactPaneLength
        {
            get => _compactPaneLength;
            set => SetProperty<double>(ref _compactPaneLength, value);
        }

        private bool _isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set
            {
                if (SetProperty<bool>(ref _isPaneOpen, value))
                {
                    if (TopHamburgerMenuSideBar is not null)
                    {
                        TopHamburgerMenuSideBar.IsPaneOpen = value;
                    }

                    VisualState = GetVisualState();
                }
            }
        }

        public List<SplitViewDisplayMode> DisplayModeList
        {
            get => Enum.GetValues(typeof(SplitViewDisplayMode)).Cast<SplitViewDisplayMode>().ToList();
        }

        private SplitViewDisplayMode _selectedDisplayMode = SplitViewDisplayMode.Overlay;
        public SplitViewDisplayMode SelectedDisplayMode
        {
            get => _selectedDisplayMode;
            set
            {
                if (SetProperty<SplitViewDisplayMode>(ref _selectedDisplayMode, value))
                {
                    VisualState = GetVisualState();
                }
            }
        }

        public List<SplitViewPanePlacement> PanePlacementList
        {
            get => Enum.GetValues(typeof(SplitViewPanePlacement)).Cast<SplitViewPanePlacement>().ToList();
        }

        private SplitViewPanePlacement _selectedPanePlacement = SplitViewPanePlacement.Left;
        public SplitViewPanePlacement SelectedPlacement
        {
            get => _selectedPanePlacement;
            set
            {
                if (SetProperty<SplitViewPanePlacement>(ref _selectedPanePlacement, value))
                {
                    VisualState = GetVisualState();
                }
            }
        }

        public string VisualState
        {
            get => field;
            set => SetProperty<string>(ref field, value);
        }
        #endregion

        #region Get HamburgerMenu State Method
        private bool IsCompact
        {
            get
            {
                return SelectedDisplayMode switch
                {
                    SplitViewDisplayMode.CompactInline or SplitViewDisplayMode.CompactOverlay => true,
                    _ => false,
                };
            }
        }

        private bool IsInline
        {
            get
            {
                return SelectedDisplayMode switch
                {
                    SplitViewDisplayMode.CompactInline or SplitViewDisplayMode.Inline => true,
                    _ => false
                };
            }
        }

        protected virtual string GetVisualState()
        {
            string state;

            if (IsPaneOpen)
            {
                state = "Open";
                state += IsInline ? "Inline" : SelectedDisplayMode.ToString();
            }
            else
            {
                state = "Closed";
                if (IsCompact)
                {
                    state += "Compact";
                }
                //else
                //{
                //    return state;
                //}
            }

            state += SelectedPlacement.ToString();

            return state;
        }
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

        #region Register ActiveContent Method
        private async Task RegisterActiveContentAsync()
        {
            RegisterRightTabStrip();
            void RegisterRightTabStrip()
            {
                _container.RegisterSingleton(from: typeof(TabViewModel), to: typeof(TabViewModel), name: ActiveContentNames.TabStripHamburgerMenuSideBar);
                var tabStripViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabViewModel>(name: ActiveContentNames.TabStripHamburgerMenuSideBar);

                TabStripViewModel = tabStripViewModel;
            }

            await RegisterLeftPaneActiveContentAsync();
            async Task RegisterLeftPaneActiveContentAsync()
            {
                _container.RegisterSingleton(from: typeof(SequenceActiveContentViewModel), to: typeof(SequenceActiveContentViewModel), name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);
                LeftPaneActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<SequenceActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);

                await CreateTopHamburgerMenuSideBarViewModelAsync();
                LeftPaneActiveContentViewModel.Add(new()
                {
                    Name = "Root",
                    Title = "Root",
                    ViewName = "Aksl.Modules.HamburgerMenuSideBar.Views.HamburgerMenuSideBarView,Aksl.Modules.HamburgerMenuSideBar",
                    ViewElement = new Views.HamburgerMenuSideBarView() { DataContext = TopHamburgerMenuSideBar }
                }, true);
            }
        }
        #endregion

        #region Create TopHamburgerMenuSideBar ViewModel Method
        private async Task CreateTopHamburgerMenuSideBarViewModelAsync()
        {
            IsLoading = true;

            try
            {

                var rootMenuItem = await _menuService.GetMenuAsync("All");
                var subMenuItems = rootMenuItem.SubMenus;
                // TopHamburgerMenuSideBar = await HamburgerMenuSideBarHelper.CreateTopHamburgerMenuSideBarViewModelAsync(subMenuItems);

                NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();
                TopHamburgerMenuSideBar = new();

                if (subMenuItems is not null && subMenuItems.Any())
                {
                    List<HamburgerMenuSideBarItemViewModel> allSideBarItemLeafs = new();

                    foreach (var mi in subMenuItems)
                    {
                        HamburgerMenuSideBarItemViewModel virtualParent = new();
                        Func<Infrastructure.MenuItem, HamburgerMenuSideBarItemViewModel, HamburgerMenuSideBarItemViewModel> childResolver = ((m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); });

                        var topItem = await nodeResolver.GetTopItemByMenuItemAsync(mi, virtualParent, childResolver, false);
                        var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);
                        allSideBarItemLeafs.AddRange(allTopItemLeafs);
                    }

                    TopHamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allSideBarItemLeafs);
                }

                TopHamburgerMenuSideBar.IsPaneOpen = IsPaneOpen;
                RaisePropertyChanged(nameof(TopHamburgerMenuSideBar));
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Unable to create top hamburger menu : \"{ex.Message}\"", title: "Error: Create Top HamburgerMenuSideBar");
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
