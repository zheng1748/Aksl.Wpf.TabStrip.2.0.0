using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Toolkit.Controls;

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

       public bool IsAddViewToBottomContent() =>
                          !_menuItem.HasNextSubMenu() && _menuItem.HasViewName() && !_menuItem.IsNexApplication;

        public bool IsNavigationToBottomContent() =>
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

                    if (IsLeaf && field && IsAddViewToBottomContent())
                    {
                        AddViewToBottomContent();
                    }

                    if (IsLeaf && field && IsNavigationToBottomContent())
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

        #region Add View To BottomContent Method
        public void AddViewToBottomContent()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            ActiveContentManagerExtensions.AddViewToRandomContentAsync(_menuItem, this.ActiveContentName).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
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

            ActiveContentManagerExtensions.NavigationToRandomContentAsync(_menuItem, this.ActiveContentName, new() { { "CurrentMenuItem", _menuItem } }).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
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