using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public class HamburgerMenuNavigationSideBarHubViewModel : BindableBase, INavigationAware
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

            SelectedDisplayMode = SplitViewDisplayMode.CompactInline;
            SelectedPlacement = SplitViewPanePlacement.Left;

            CreateGroupedMenusViewModelAsync().Await();
            IsPaneOpen = true;

            RegisterActiveContent();
            // RegisterPropertyChanged();
            RegisterHamburgerMenuBarPaneOpenEvent();
        }
        #endregion

        #region Properties
        public RandomActiveContentViewModel RightContentActiveContentViewModel { get; set; }

        public GroupedMenusViewModel GroupedMenu { get; private set; }
        //public ObservableCollection<NoGroupedMenuViewModel> NoGroupedMenus { get; }
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
        public Brush PaneBackground
        {
            get;
            set => SetProperty<Brush>(ref field, value);
        } = new SolidColorBrush(Colors.White);

        public GridLength OpenPaneGridLength
        {
            get { return new GridLength(OpenPaneLength); }
        }

        public double OpenPaneLength
        {
            get;
            set => SetProperty<double>(ref field, value);
        }= 320d;

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
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (GroupedMenu is not null)
                    {
                        GroupedMenu.IsPaneOpen = value;
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
            get;
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
                IsPaneOpen = hmbpoe.IsPaneOpen;
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register ActiveContents Method
        private void RegisterActiveContent()
        {
            _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: ActiveContentNames.RightContentHamburgerMenuNavigationSideBar);
            var rightContentActiveContentViewModel = PrismIocExtensions.GetUnityContainer().
                                              Resolve<RandomActiveContentViewModel>(name: ActiveContentNames.RightContentHamburgerMenuNavigationSideBar);

            RightContentActiveContentViewModel = rightContentActiveContentViewModel;
        }
        #endregion

        #region Create GroupedMenus ViewModel Method
        private async Task CreateGroupedMenusViewModelAsync()
        {
            IsLoading = true;

            try
            {
                GroupedMenu = new();

                AddPropertyChanged();
                void AddPropertyChanged()
                {
                    GroupedMenu.PropertyChanged += (sender, e) =>
                    {
                        if (sender is GroupedMenusViewModel gmvm)
                        {
                            if (e.PropertyName == nameof(GroupedMenusViewModel.IsLoading) && !gmvm.IsLoading)
                            {
                                IsLoading = false;
                            }
                        }
                    };
                }

                //GroupedMenu.WorkspaceViewEventName = _workspaceViewEventName;
                await GroupedMenu.CreateGroupedMenuViewModelsAsync();
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
