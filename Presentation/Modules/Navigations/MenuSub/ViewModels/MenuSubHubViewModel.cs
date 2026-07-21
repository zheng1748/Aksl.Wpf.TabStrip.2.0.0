using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
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
using System.Windows.Documents;
using Unity;

namespace Aksl.Modules.MenuSub.ViewModels
{
    public class MenuSubHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        //private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public MenuSubHubViewModel()
        {
            _container = PrismIocExtensions.GetUnityContainer();
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();
            _menuService = PrismUnityExtensions.GetMenuService();
        }
        #endregion

        #region Properties
        public string ActiveContentName { get; set; }
        public RandomActiveContentViewModel BottomActiveContentViewModel
        {
            get;
            set => SetProperty<RandomActiveContentViewModel>(ref field, value);
        }
        public HierarchicalMenusViewModel HierarchicalMenus { get; private set; }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;
        #endregion

        #region Register ActiveContent Method
        private void RegisterActiveContent()
        {
            _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: this.ActiveContentName);
            var bottomActiveContentViewModel = _container.Resolve<RandomActiveContentViewModel>(name: this.ActiveContentName);

            BottomActiveContentViewModel = bottomActiveContentViewModel;
        }
        #endregion

        #region Register BuildWorkspaceView Event
        //private void RegisterBuildWorkspaceViewEvents()
        //{
        //    var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(_workspaceViewEventName) as OnBuildWorkspaceViewEventbase;
        //    Debug.Assert(buildHWorkspaceViewEvent is not null);

        //    buildHWorkspaceViewEvent.Subscribe(async (bmve) =>
        //    {
        //        var currentMenuItem = bmve.CurrentMenuItem;

        //        try
        //        {
        //            await LoadViewAsync();

        //            #region LoadView Method
        //            async Task LoadViewAsync()
        //            {
        //                if (IsAddViewToBottomContent())
        //                {
        //                    AddViewToBottomContent();
        //                }

        //                if (IsNavigationToBottomContent())
        //                {
        //                    NavigationToBottomContent();
        //                }
        //            }

        //            void AddViewToBottomContent()
        //            {
        //                ActiveContentManagerExtensions.AddViewToRandomContentAsync(currentMenuItem, this.ActiveContentName).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
        //                {
        //                    System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
        //                    {
        //                        await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To BottomContent");
        //                    });
        //                });
        //            }

        //            void NavigationToBottomContent()
        //            {
        //                ActiveContentManagerExtensions.NavigationToRandomContentAsync(currentMenuItem, this.ActiveContentName, new() { { "CurrentMenuItem", currentMenuItem } }).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
        //                {
        //                    System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
        //                    {
        //                        await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To RightContent");
        //                    });
        //                });
        //            }

        //            bool IsAddViewToBottomContent() =>
        //                    !currentMenuItem.HasNextSubMenu() && currentMenuItem.HasViewName() && !currentMenuItem.IsNexApplication;

        //            bool IsNavigationToBottomContent() =>
        //                    currentMenuItem.HasNextSubMenu() && currentMenuItem.HasViewName() && currentMenuItem.IsNexApplication;
        //            #endregion
        //        }
        //        catch (Exception ex)
        //        {
        //            await _dialogViewService.AlertAsync(message: $"Unable to loading \"{currentMenuItem.ModuleName}\" module.: \"{ex.Message}\"", title: "Error: Load Module");
        //        }
        //    }, ThreadOption.UIThread, true);
        //}
        #endregion

        #region Create HierarchicalMenus ViewModel Method
        private Aksl.Infrastructure.MenuItem _rootMenuItem;
        private async Task CreateHierarchicalMenusViewModelAsync(MenuItem rootMenuItem)
        {
            try
            {
                IsLoading = true;
                _rootMenuItem = rootMenuItem;

                var parentMenuItem = await _menuService.GetMenuAsync(rootMenuItem.NavigationName);
                var subMenuItems = parentMenuItem.SubMenus;
                NodeResolver<HierarchicalMenuItemViewModel> nodeResolver = new();
                List<HierarchicalMenuItemViewModel> topHierarchicalMenuItems = new();

                foreach (var smi in subMenuItems)
                {
                    HierarchicalMenuItemViewModel virtualParent = new();
                    Func<Aksl.Infrastructure.MenuItem, HierarchicalMenuItemViewModel, HierarchicalMenuItemViewModel> childResolver = ((m, p) => { return new HierarchicalMenuItemViewModel(m, p); });
                    var topItem = await nodeResolver.GetTopItemByMenuItemAsync(smi, virtualParent, childResolver, false);
                    topHierarchicalMenuItems.Add(topItem);
                }

                // HierarchicalMenus = new(_eventAggregator, _menuService, currentMenuItem);
                HierarchicalMenus = new()
                {
                    TopHierarchicalMenuItems = new(topHierarchicalMenuItems)
                };
                //AddPropertyChanged();

                //void AddPropertyChanged()
                //{
                //    HierarchicalMenus.PropertyChanged += (sender, e) =>
                //    {
                //        if (sender is HierarchicalMenusViewModel hmvm)
                //        {
                //            if (e.PropertyName == nameof(HierarchicalMenusViewModel.IsLoading) && !hmvm.IsLoading)
                //            {
                //                IsLoading = false;
                //            }
                //        }
                //    };
                //}

                // await HierarchicalMenus.CreateHierarchicalMenuItemViewModelsAsync();
                HierarchicalMenus.SetActiveContentNameAndPropertyChanged(rootMenuItem.ActiveContentName);
                RaisePropertyChanged(nameof(HierarchicalMenus));
            }
            catch(Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Create Hierarchical Menus");
            }
            finally 
            {
                IsLoading = false;
            }
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            if (parameters.TryGetValue("CurrentMenuItem", out MenuItem currentMenuItem))
            {
                ActiveContentName = currentMenuItem.ActiveContentName;
                RegisterActiveContent();

                CreateHierarchicalMenusViewModelAsync(currentMenuItem).Await();
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
