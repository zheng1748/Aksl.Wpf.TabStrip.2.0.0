using Aksl.Infrastructure;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aksl.Modules.MenuSub.ViewModels
{
     public class HierarchicalMenusViewModel : BindableBase
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IMenuService _menuService;
        private Aksl.Infrastructure.MenuItem _rootMenuItem; 
        private Aksl.Infrastructure.MenuItem _parentMenuItem;
        //private HierarchicalMenuItemViewModel _selectedMenuItem;
        #endregion

        #region Constructors
        public HierarchicalMenusViewModel()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();

            TopHierarchicalMenuItems = new();

            RegisterOnTopMenuSelectedEvent();
        }

        public HierarchicalMenusViewModel(IEventAggregator eventAggregator,IMenuService menuService, Aksl.Infrastructure.MenuItem rootMenuItem)
        {
            _eventAggregator = eventAggregator;
            _menuService = menuService;

            _rootMenuItem = rootMenuItem;

            TopHierarchicalMenuItems = new();

            RegisterOnTopMenuSelectedEvent();
        }
        #endregion

        #region Properties
        public ObservableCollection<HierarchicalMenuItemViewModel> TopHierarchicalMenuItems { get; set; }

        public HierarchicalMenuItemViewModel SelectedHierarchicalMenuItem { get; set; }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;
        #endregion

        #region Register TopMenu Selected Event
        private void RegisterOnTopMenuSelectedEvent()
        {
            _eventAggregator.GetEvent<OnTopMenuSubSelectedEvent>().Subscribe((tmsse) =>
            {
                ClearTopSelectedHierarchicalMenuItemViewModels();

                SetTopLevelSelected(tmsse.SelectedMenuItem);

                //var nowTopHierarchicalMenuItem = OnlyFindTopHierarchicalMenuItemViewModelByMenuItem(tmsse.SelectedMenuItem);

                //if (nowTopHierarchicalMenuItem.Top is not null && nowTopHierarchicalMenuItem.Sub is not null && nowTopHierarchicalMenuItem.Top == nowTopHierarchicalMenuItem.Sub)
                //{
                //    if (_selectedMenuItem is not null && _selectedMenuItem.IsSubmenuItem)
                //    {
                //        nowTopHierarchicalMenuItem.Top.IsTopLevelSelected = true;
                //    }

                //    if (_selectedMenuItem is not null && _selectedMenuItem.IsTopLevelItem && _selectedMenuItem == nowTopHierarchicalMenuItem.Top)
                //    {
                //        nowTopHierarchicalMenuItem.Top.IsTopLevelSelected = true;
                //    }
                //}

                //if (nowTopHierarchicalMenuItem.Top is not null && nowTopHierarchicalMenuItem.Sub is not null && nowTopHierarchicalMenuItem.Top != nowTopHierarchicalMenuItem.Sub)
                //{
                //    if (_selectedMenuItem is not null && _selectedMenuItem.IsSubmenuItem && _selectedMenuItem == nowTopHierarchicalMenuItem.Sub)
                //    {
                //        nowTopHierarchicalMenuItem.Top.IsTopLevelSelected = true;
                //    }
                //}
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Set TopLevelSelected Method
        private void SetTopLevelSelected(Aksl.Infrastructure.MenuItem menuItem)
        {
            var nowTopHierarchicalMenuItem = OnlyFindTopHierarchicalMenuItemViewModelByMenuItem(menuItem);

            if (nowTopHierarchicalMenuItem.Top is not null && nowTopHierarchicalMenuItem.Sub is not null && nowTopHierarchicalMenuItem.Top == nowTopHierarchicalMenuItem.Sub)
            {
                if (SelectedHierarchicalMenuItem is not null && SelectedHierarchicalMenuItem.IsSubmenuItem)
                {
                    nowTopHierarchicalMenuItem.Top.IsTopLevelSelected = true;
                }

                if (SelectedHierarchicalMenuItem is not null && SelectedHierarchicalMenuItem.IsTopLevelItem && SelectedHierarchicalMenuItem == nowTopHierarchicalMenuItem.Top)
                {
                    nowTopHierarchicalMenuItem.Top.IsTopLevelSelected = true;
                }
            }

            if (nowTopHierarchicalMenuItem.Top is not null && nowTopHierarchicalMenuItem.Sub is not null && nowTopHierarchicalMenuItem.Top != nowTopHierarchicalMenuItem.Sub)
            {
                if (SelectedHierarchicalMenuItem is not null && SelectedHierarchicalMenuItem.IsSubmenuItem && SelectedHierarchicalMenuItem == nowTopHierarchicalMenuItem.Sub)
                {
                    nowTopHierarchicalMenuItem.Top.IsTopLevelSelected = true;
                }
            }
        }
        #endregion

        #region Find Top HierarchicalMenuItemViewModel Method
        private (HierarchicalMenuItemViewModel Top, HierarchicalMenuItemViewModel Sub) OnlyFindTopHierarchicalMenuItemViewModelByMenuItem(Aksl.Infrastructure.MenuItem menuItem)
        {
            HierarchicalMenuItemViewModel topHierarchicalMenuItemViewModel = null;
            HierarchicalMenuItemViewModel findHierarchicalMenuItemViewModel = null;

            int i = 0;
            for (i = 0; i < TopHierarchicalMenuItems.Count; i++)
            {
                if (findHierarchicalMenuItemViewModel is null)
                {
                    RecursiveSubMenuItemViewModel(TopHierarchicalMenuItems[i]);
                }
                else
                {
                    topHierarchicalMenuItemViewModel = TopHierarchicalMenuItems[i - 1];
                    break;
                }
            }

            void RecursiveSubMenuItemViewModel(HierarchicalMenuItemViewModel parent)
            {
                if (IsEqualsNameOrTitle(parent.Name, menuItem.Name) || IsEqualsNameOrTitle(parent.Title, menuItem.Title))
                {
                    findHierarchicalMenuItemViewModel = parent;
                    return;
                }

                if (parent.HasChildren)
                {
                    foreach (var children in parent.Children)
                    {
                        RecursiveSubMenuItemViewModel(children as HierarchicalMenuItemViewModel);
                    }
                }
            }

            if (findHierarchicalMenuItemViewModel is not null && i == TopHierarchicalMenuItems.Count)
            {
                topHierarchicalMenuItemViewModel = TopHierarchicalMenuItems[i - 1];
            }

            return (Top: topHierarchicalMenuItemViewModel, Sub: findHierarchicalMenuItemViewModel);
        }
        #endregion

        #region Create HierarchicalMenuItem ViewModels Method
        internal async Task CreateHierarchicalMenuItemViewModelsAsync()
        {
            IsLoading = true;

            var parentMenuItem = await _menuService.GetMenuAsync(_rootMenuItem.NavigationName);
            _parentMenuItem = parentMenuItem;
            var subMenuItems = _parentMenuItem.SubMenus;
            NodeResolver<HierarchicalMenuItemViewModel> nodeResolver = new();

            foreach (var smi in subMenuItems)
            {
                HierarchicalMenuItemViewModel virtualParent = new();
                Func<Aksl.Infrastructure.MenuItem, HierarchicalMenuItemViewModel, HierarchicalMenuItemViewModel> childResolver = ((m, p) => { return new HierarchicalMenuItemViewModel(m, p); });
                var topItem = await nodeResolver.GetTopItemByMenuItemAsync(smi, virtualParent, childResolver, false);
                TopHierarchicalMenuItems.Add(topItem);
            }

            SetActiveContentNameAndPropertyChanged();

            void SetActiveContentNameAndPropertyChanged()
            {
                foreach (var tmi in TopHierarchicalMenuItems)
                {
                    RecursiveSubMenuItem(tmi);
                }

                void RecursiveSubMenuItem(HierarchicalMenuItemViewModel hierarchicalMenuItemViewModel)
                {
                    AddPropertyChanged(hierarchicalMenuItemViewModel);

                    if (hierarchicalMenuItemViewModel.IsLeaf)
                    {
                       // hierarchicalMenuItemViewModel.WorkspaceViewEventName = _rootMenuItem.WorkspaceViewEventName;
                        hierarchicalMenuItemViewModel.ActiveContentName = _rootMenuItem.ActiveContentName;
                    }

                    if (hierarchicalMenuItemViewModel.HasChildren)
                    {
                        foreach (var smi in hierarchicalMenuItemViewModel.Children)
                        {
                            RecursiveSubMenuItem(smi as HierarchicalMenuItemViewModel);
                        }
                    }
                }
            }

            IsLoading = false;
        }

        #endregion

        #region Set ActiveContentName Method

        public void SetActiveContentNameAndPropertyChanged(string activeContentName)
        {
            foreach (var tmi in TopHierarchicalMenuItems)
            {
                RecursiveSubMenuItem(tmi);
            }

            void RecursiveSubMenuItem(HierarchicalMenuItemViewModel hierarchicalMenuItemViewModel)
            {
                AddPropertyChanged(hierarchicalMenuItemViewModel);

                if (hierarchicalMenuItemViewModel.IsLeaf)
                {
                    hierarchicalMenuItemViewModel.ActiveContentName = activeContentName;
                }

                if (hierarchicalMenuItemViewModel.HasChildren)
                {
                    foreach (var smi in hierarchicalMenuItemViewModel.Children)
                    {
                        RecursiveSubMenuItem(smi as HierarchicalMenuItemViewModel);
                    }
                }
            }
        }
        #endregion

        #region RegisterPropertyChanged Method
        private void AddPropertyChanged(HierarchicalMenuItemViewModel curentHierarchicalMenuItemViewModel)
        {
            curentHierarchicalMenuItemViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is HierarchicalMenuItemViewModel hmvm)
                {
                    if (e.PropertyName == nameof(HierarchicalMenuItemViewModel.IsSelected))
                    {
                        ClearTopSelectedHierarchicalMenuItemViewModels();

                        var selectedMenuItemCount = GetSelectedHierarchicalMenuItemViewModels().Count();
                        //if (_selectedMenuItem is null && hmvm.IsSelected)
                        if (SelectedHierarchicalMenuItem is null && hmvm.IsSelected)
                        {
                            SelectedHierarchicalMenuItem = hmvm;
                            //_selectedMenuItem.IsSelected = true;
                        }
                        //else if ((_selectedMenuItem is not null) && !IsEqualsNameOrTitle(_selectedMenuItem?.Name, hmvm?.Name))
                        else if ((SelectedHierarchicalMenuItem is not null) && hmvm.IsSelected && SelectedHierarchicalMenuItem != hmvm )
                        {
                            //_selectedMenuItem.IsSelected = false;
                            //_selectedMenuItem = hmvm;
                            SelectedHierarchicalMenuItem.IsSelected = false;
                            SelectedHierarchicalMenuItem = hmvm;
                        }
                        selectedMenuItemCount = GetSelectedHierarchicalMenuItemViewModels().Count();

                        if (!hmvm.DenyPublishWhenIsSelected && (hmvm.IsTopLevelItem || hmvm.IsSubmenuItem) && hmvm.IsSelected)
                        {
                            SetTopLevelSelected(hmvm.MenuItem);
                        }
                    }
                }
            };
        }
        #endregion

        #region Clear TopSelected HierarchicalMenuItemViewModels Method
        private void ClearTopSelectedHierarchicalMenuItemViewModels()
        {
            var topLevelMenuItemViewModels = GetTopSelectedHierarchicalMenuItemViewModels();
            foreach (var tmivm in topLevelMenuItemViewModels)
            {
                if (tmivm is not null && tmivm.IsTopLevelSelected)
                {
                    tmivm.IsTopLevelSelected = false;
                }
            }
        }
        #endregion

        #region Get TopSelected HierarchicalMenuItemViewModels Method
        private List<HierarchicalMenuItemViewModel> GetTopSelectedHierarchicalMenuItemViewModels()
        {
            List<HierarchicalMenuItemViewModel> hieMenuItemViewModels = new();

            foreach (var hmivm in TopHierarchicalMenuItems)
            {
                RecursiveSubMenuItemViewModel(hmivm);
            }

            void RecursiveSubMenuItemViewModel(HierarchicalMenuItemViewModel parent)
            {
                if (parent.IsTopLevelSelected)
                {
                    hieMenuItemViewModels.Add(parent);
                }

                if (parent.HasChildren)
                {
                    foreach (var children in parent.Children)
                    {
                        RecursiveSubMenuItemViewModel(children as HierarchicalMenuItemViewModel);
                    }
                }
            }

            return hieMenuItemViewModels;
        }
        #endregion

        #region Get Selected HierarchicalMenuItemViewModels Method
        private List<HierarchicalMenuItemViewModel> GetSelectedHierarchicalMenuItemViewModels()
        {
            List<HierarchicalMenuItemViewModel> findHierarchicalMenuItemViewModels = new();

            foreach (var hmivm in TopHierarchicalMenuItems)
            {
                RecursiveSubMenuItemViewModel(hmivm);
            }

            void RecursiveSubMenuItemViewModel(HierarchicalMenuItemViewModel parent)
            {
                if (parent.IsSelected)
                {
                    findHierarchicalMenuItemViewModels.Add(parent);
                }

                if (parent.HasChildren)
                {
                    foreach (var children in parent.Children)
                    {
                        RecursiveSubMenuItemViewModel(children as HierarchicalMenuItemViewModel);
                    }
                }
            }

            return findHierarchicalMenuItemViewModels;
        }
        #endregion

        #region Contain Methods
        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isEquals =  nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase) ||
                            otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase);

            return isEquals;
        }
        #endregion
    }
}
