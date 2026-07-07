using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
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

namespace Aksl.Modules.HamburgerMenuTreeSideBar.ViewModels
{
    public class HamburgerMenuTreeSideBarHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        //private object _currentView;
        private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public HamburgerMenuTreeSideBarHubViewModel()
        {
            _regionManager = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionManager>();
            _container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

            _menuService = _container.Resolve<IMenuService>();

            SelectedDisplayMode = SplitViewDisplayMode.Inline;
            SelectedPlacement = SplitViewPanePlacement.Left;

             CreateTreeSideBarViewModelAsync().Await();
            IsPaneOpen = true;

            //_workspaceViewEventName = "OnBuildHamburgerMenuTreeSideBarWorkspaceViewEvent";
            //WorkspaceRegionName = RegionNames.HamburgerMenuTreeSideBarWorkspaceRegion;

            RegisterActiveContent();

           // RegisterBuildWorkspaceViewEvents();
            RegisterHamburgerMenuBarPaneOpenEvent();
        }
        #endregion

        #region Properties
        public RandomActiveContentViewModel RightContentActiveContentViewModel { get; set; }
        public TreeSideBarViewModel TreeSideBar { get; private set; }

        private string _workspaceRegionName;
        public string WorkspaceRegionName
        {
            get => _workspaceRegionName;
            set => SetProperty<string>(ref _workspaceRegionName, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }
        #endregion

        #region HamburgerMenu Properties
        private Brush _paneBackground = new SolidColorBrush(Colors.White);
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

        private string _visualState;
        public string VisualState
        {
            get => _visualState;
            set => SetProperty<string>(ref _visualState, value);
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

        #region Register BuildWorkspaceView Event
        
        #endregion

        #region LoadView Method
        //private async Task LoadViewAsync(Infrastructure.MenuItem currentMenuItem,string regionName = RegionNames.HamburgerMenuTreeSideBarWorkspaceRegion)
        //{
        //    string viewTypeAssemblyQualifiedName = currentMenuItem.ViewName;
        //    Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
        //    if (viewType is not null)
        //    {
        //        IRegion region = _regionManager.Regions[regionName];
        //        var viewName = viewType.Name;

        //        //_currentView = region.GetView(viewTypeAssemblyQualifiedName);
        //        _currentView = region.Views.FirstOrDefault(v => v.GetType() == viewType);
        //        if (_currentView is null)
        //        {
        //            _currentView = region.GetView(viewType.FullName);
        //        }

        //        if (_currentView is not null)
        //        {
        //            if (currentMenuItem.IsCacheable)
        //            {
        //                region.Activate(_currentView);
        //            }
        //            else
        //            {
        //                region.Remove(_currentView);

        //                AddView();
        //            }
        //        }
        //        else
        //        {
        //            AddView();
        //        }

        //        void AddView()
        //        {
        //            if (CanAddView())
        //            {
        //                NavigationParameters navigationParameters = new()
        //                {
        //                   {"CurrentMenuItem", currentMenuItem }
        //                };

        //                _regionManager.RequestNavigate(regionName, viewName, navigationParameters);
        //            }
        //        }

        //        bool CanAddView() => !string.IsNullOrEmpty(currentMenuItem.ModuleName) && currentMenuItem.SubMenus.Count == 0;
        //    }
        //    else
        //    {
        //        //bhmsbwve.CallBack?.Invoke(false);

        //        await _dialogViewService.AlertAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type");
        //    }
        //}
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
            _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: ActiveContentNames.RightContentHamburgerMenuTreeSideBar);
            var rightContentActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<RandomActiveContentViewModel>(name: ActiveContentNames.RightContentHamburgerMenuTreeSideBar);

            RightContentActiveContentViewModel = rightContentActiveContentViewModel;
        }
        #endregion

        #region Create TreeSideBar ViewModel Method
        private async Task CreateTreeSideBarViewModelAsync()
        {
            IsLoading = true;

            try
            {
                TreeSideBar = new(_eventAggregator, _menuService);
                TreeSideBar.PropertyChanged += (sender, e) =>
                {
                    if (sender is TreeSideBarViewModel tvm)
                    {
                        if (e.PropertyName == nameof(TreeSideBarViewModel.IsLoading) && !tvm.IsLoading)
                        {
                            IsLoading = false;
                        }
                    }
                };

                await TreeSideBar.CreateTreeSideBarItemViewModelsAsync();
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
