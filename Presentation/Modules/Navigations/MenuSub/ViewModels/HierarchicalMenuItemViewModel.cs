using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.TabStrip;
using Aksl.TabStrip.ViewModels;
using Aksl.Toolkit.Controls;
using Prism;
using Prism.Commands;
using Prism.Common;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace Aksl.Modules.MenuSub.ViewModels
{
    public class HierarchicalMenuItemViewModel : Mvvm.NodeViewModel
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly Aksl.Infrastructure.MenuItem _menuItem;
        #endregion

        #region Constructors
        public HierarchicalMenuItemViewModel() : base()
        {
            _menuItem = null;
        }

        public HierarchicalMenuItemViewModel(Aksl.Infrastructure.MenuItem menuItem) : base(menuItem.Name, menuItem.Title,null)
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();

            _menuItem = menuItem;
        }

        public HierarchicalMenuItemViewModel(Aksl.Infrastructure.MenuItem menuItem, HierarchicalMenuItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _menuItem = menuItem;

            CreateExecuteClickCommand();
        }
        #endregion

        #region Properties
        public Aksl.Infrastructure.MenuItem MenuItem => _menuItem;
        public int Id => _menuItem.Id; 
        //public string WorkspaceViewEventName { get; set; }
        public string ActiveContentName { get; set; }
        public string NavigationNam => _menuItem.NavigationName;
        public bool IsSeparator => _menuItem.IsSeparator;
        public bool IsSelectedOnInitialize => _menuItem.IsSelectedOnInitialize;
        public bool IsTopLevel => IsTopLevelItem || IsTopLevelHeader;
        public bool IsSubmenu => IsSubmenuItem || IsSubmenuHeader;

        public bool IsTopLevelSelected
        {
            get => field;
            set => SetProperty(ref field, value);
        } = false;

        public bool IsChecked
        {
            get => field;
            set => SetProperty(ref field, value);
        } = false;

        public bool DenyPublishWhenIsSelected
        {
            get => field;
            set => SetProperty(ref field, value);
        } = false;

       public bool IsAddViewToBottomContent =>
                          !_menuItem.HasNextSubMenu() && _menuItem.HasViewName() && !_menuItem.IsNexApplication;

        public bool IsNavigationToBottomContent =>
                          _menuItem.HasNextSubMenu() && _menuItem.HasViewName() && _menuItem.IsNexApplication;

        public bool IsSelected
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    //if (!DenyPublishWhenIsSelected && IsLeaf && field)
                    //{
                    //    var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(WorkspaceViewEventName) as OnBuildWorkspaceViewEventbase;
                    //    buildHWorkspaceViewEvent.Publish(new() { CurrentMenuItem = _menuItem });
                    //}

                    //if (!DenyPublishWhenIsSelected && (IsTopLevelItem || IsSubmenuItem) && field)
                    //{
                    //    _eventAggregator.GetEvent<OnTopMenuSubSelectedEvent>().Publish(new OnTopMenuSubSelectedEvent { SelectedMenuItem = _menuItem });
                    //}

                    if (IsSubmenu)
                    {
                        IsChecked = field;
                    }

                    if (IsLeaf && field && IsAddViewToBottomContent)
                    {
                        //AddViewToBottomContent();
                        AddViewToRightTabContentAsync(_menuItem).Await();
                    }

                    if (IsLeaf && field && IsNavigationToBottomContent)
                    {
                        NavigationToBottomContent();
                    }
                }
            }
        }

        public PackIconKind IconKind =>
               _menuItem.IconKind.ToPackIconKind();

        public bool IsEnabled
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    foreach (var children in this.Children)
                    {
                        (children as HierarchicalMenuItemViewModel).IsEnabled = field;
                    }
                }
            }
        } = true;
        #endregion

        #region Click Command
        public ICommand ExecuteClickCommand { get; private set; }

        private void CreateExecuteClickCommand()
        {
            ExecuteClickCommand = new DelegateCommand(() =>
            {
                if (DenyPublishWhenIsSelected)
                {
                    DenyPublishWhenIsSelected = false;

                    IsSelected = true;
                    //var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(WorkspaceViewEventName) as OnBuildWorkspaceViewEventbase;
                    //buildHWorkspaceViewEvent.Publish(new() { CurrentMenuItem = _menuItem });
                }
                else
                {
                    IsSelected = true;
                }
            },
            () =>
            {
                var canExecute = true;
                return canExecute;
            });
        }
        #endregion

        #region Add View To Right TabContent Method
        private async Task AddViewToRightTabContentAsync(MenuItem menuItem)
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            try
            {
                var randomActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<RandomActiveContentViewModel>(name: this.ActiveContentName);

               // NavigationParameters navigationParameters = new() { { "CurrentMenuItem", menuItem } };

                ContentInformation contentInformation = new()
                {
                    Name = menuItem.Name,
                    Title = menuItem.Title,
                    ViewName = menuItem.ViewName
                };

                var currentView = randomActiveContentViewModel.GetStoreViewElementByName(menuItem.Name);
                if (currentView is not null)
                {
                    if (menuItem.IsCacheable)
                    {
                        // activeContentViewModel.SetContentItem(contentInformation);
                        randomActiveContentViewModel.SetActiveContentItemByName(menuItem.Name);
                    }
                    else
                    {
                      //  ActiveContents.ContentInformation contentInformation = CreateContentInformation(menuItem, navigationParameters);
                        randomActiveContentViewModel.RetsetContentItem(contentInformation);
                    }
                }
                else
                {
                   // ActiveContents.ContentInformation contentInformation = CreateContentInformation(menuItem, navigationParameters);
                    randomActiveContentViewModel.Add(contentInformation);
                }
            }
            catch (Exception ex) when (!string.IsNullOrEmpty(ex.InnerException?.Message))
            {
                await dialogViewService.AlertAsync(message: $"{ex.InnerException.Message}", title: $"Error:Add View In MenuSub");
            }
            catch (Exception ex)
            {
                await dialogViewService.AlertAsync(message: $"{ex.Message}", title: $"Error:Add View In MenuSub");
            }
        }
        #endregion

        #region Create ContentInformation Methods
        public ContentInformation CreateContentInformation(string name, string title, string viewTypeAssemblyQualifiedName, NavigationParameters navigationParameters = null)
        {
            Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
            if (viewType is null)
            {
                throw new ArgumentException($"Missing Type {viewTypeAssemblyQualifiedName}");
            }
            var viewName = viewType.Name;

            var unityContainer = PrismIocExtensions.GetUnityContainer();
            var regionNavigationService = unityContainer.Resolve<IRegionNavigationService>();

            ContentInformation contentInformation = new()
            {
                Name = name,
                Title = title,
                ViewName = viewTypeAssemblyQualifiedName
            };

            var registeredView = unityContainer.Resolve<object>(viewName);
            if (registeredView is FrameworkElement frameworkElement)
            {
                MvvmHelpers.AutowireViewModel(registeredView);

                NavigationContext navigationContext = new(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute));
                navigationContext.Parameters = navigationParameters;

                Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
                MvvmHelpers.ViewAndViewModelAction<INavigationAware>(registeredView, action);

                contentInformation.ViewName = null;
                contentInformation.ViewElement = frameworkElement;
            }

            return contentInformation;
        }

        public ContentInformation CreateContentInformation(Infrastructure.MenuItem menuItem, NavigationParameters navigationParameters = null)
        {
            //  CreateContentInformation(menuItem.Name, menuItem.Title, menuItem.ViewName, navigationParameters);

            var viewName = menuItem.GetViewTypeName();

            var unityContainer = PrismIocExtensions.GetUnityContainer();
            var regionNavigationService = unityContainer.Resolve<IRegionNavigationService>();

            ContentInformation contentInformation = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                ViewName = menuItem.ViewName
            };

            var registeredView = unityContainer.Resolve<object>(viewName);
            if (registeredView is FrameworkElement frameworkElement)
            {
                MvvmHelpers.AutowireViewModel(registeredView);

                //if (navigationParameters is null)
                //{
                //    navigationParameters = new() { { "CurrentMenuItem", menuItem } };
                //}

                NavigationContext navigationContext = new(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute));
                navigationContext.Parameters = navigationParameters;

                Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
                MvvmHelpers.ViewAndViewModelAction<INavigationAware>(registeredView, action);

                contentInformation.ViewName = null;
                contentInformation.ViewElement = frameworkElement;
            }

            return contentInformation;
        }
        #endregion

        #region Add View To BottomContent Method
        public void AddViewToBottomContent()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            ActiveContentManagerExtensions.AddViewToRandomContentAsync(_menuItem, this.ActiveContentName).Await(completedCallback: null, configureAwait:false, errorCallback: (ex) =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                {
                    await dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To BottomContent");
                });
            });
        }
        #endregion

        #region Navigation To BottomContent Method
        public void NavigationToBottomContent()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            ActiveContentManagerExtensions.NavigationToRandomContentAsync(_menuItem, this.ActiveContentName, new() { { "CurrentMenuItem", _menuItem } }).Await(completedCallback: null, configureAwait: false, errorCallback: (ex) =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                {
                    await dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Navigation To BottomContent");
                });
            });
        }
        #endregion
    }
}