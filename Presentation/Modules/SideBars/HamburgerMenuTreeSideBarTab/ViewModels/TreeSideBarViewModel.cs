using Aksl.Infrastructure;
using Aksl.Tabs;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Aksl.Modules.HamburgerMenuTreeSideBarTab.ViewModels
{
    public class TreeSideBarViewModel : BindableBase
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IMenuService _menuService;
        #endregion

        #region Constructors
        public TreeSideBarViewModel()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();

            TopTreeSideBarItems = new();

            RegisterActiveTabItemEvent();
            RegisterOnSelectedTabItemEmptyEvent();
        }
        #endregion

        #region Properties
        public ObservableCollection<TreeSideBarItemViewModel> TopTreeSideBarItems { get; set; }
        //public ObservableCollection<TreeSideBarItemViewModel> AllTreeSideBarItems { get; }

        public TreeSideBarItemViewModel SelectedTreeSideBarItem
        {
            get => field;
            set
            {
                SetProperty(ref field, value);
                //if (SetProperty(ref _selectedTreeSideBarItem, value))
                //{
                //    if (_selectedTreeSideBarItem is not null)
                //    {
                //        _selectedTreeSideBarItem.IsSelected = true;
                //    }
                //}
            }
        }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;
        #endregion

        #region Get Selected TreeSideBarItemViewModel Method
        private TreeSideBarItemViewModel GetSelectedTreeSideBarItemViewModel()
        {
            TreeSideBarItemViewModel findTreeSideBarItemViewModel = null;

            foreach (var tbi in TopTreeSideBarItems)
            {
                if (findTreeSideBarItemViewModel is null)
                {
                    RecursiveSubMenuItem(tbi);
                }
                else
                {
                    break;
                }
            }

            void RecursiveSubMenuItem(TreeSideBarItemViewModel curentTreeSideBarItemViewModel)
            {
                //if (curentTreeSideBarItemViewModel.IsLeaf && curentTreeSideBarItemViewModel.IsSelected)
                if (curentTreeSideBarItemViewModel.IsSelected)
                {
                    findTreeSideBarItemViewModel = curentTreeSideBarItemViewModel;
                    return;
                }

                if (curentTreeSideBarItemViewModel.HasChildren)
                {
                    foreach (var tbvm in curentTreeSideBarItemViewModel.Children)
                    {
                        RecursiveSubMenuItem(tbvm as TreeSideBarItemViewModel);
                    }
                }
            }

            return findTreeSideBarItemViewModel;
        }
        #endregion

        #region Findt TreeSideBarItemViewModel Method
        private TreeSideBarItemViewModel FindtTreeSideBarItemViewModel(TabInformation tabInformation)
        {
            TreeSideBarItemViewModel findTreeSideBarItemViewModel = null;

            foreach (var tbi in TopTreeSideBarItems)
            {
                if (findTreeSideBarItemViewModel is null)
                {
                    RecursiveSubMenuItem(tbi);
                }
                else
                {
                    break;
                }
            }

            void RecursiveSubMenuItem(TreeSideBarItemViewModel curentTreeSideBarItemViewModel)
            {
                if (IsEqualsNameOrTitle(curentTreeSideBarItemViewModel.Name, tabInformation.Name) || IsEqualsNameOrTitle(curentTreeSideBarItemViewModel.Title, tabInformation.Title))
                {
                    findTreeSideBarItemViewModel = curentTreeSideBarItemViewModel;
                    return;
                }

                if (curentTreeSideBarItemViewModel.HasChildren)
                {
                    foreach (var tbvm in curentTreeSideBarItemViewModel.Children)
                    {
                        RecursiveSubMenuItem(tbvm as TreeSideBarItemViewModel);
                    }
                }
            }

            return findTreeSideBarItemViewModel;
        }
        #endregion

        #region Register SelectedTabItem Empty Event
        private void RegisterOnSelectedTabItemEmptyEvent()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            _eventAggregator.GetEvent<OnSelectedTabItemEmptyEvent>().Subscribe(async (oatie) =>
            {
                try
                {
                    var selectedTreeSideBarItem = GetSelectedTreeSideBarItemViewModel();
                    Debug.Assert(selectedTreeSideBarItem == SelectedTreeSideBarItem);
                    if (selectedTreeSideBarItem is not null)
                    {
                        selectedTreeSideBarItem.IsSelected = false;
                        selectedTreeSideBarItem = null;

                        SelectedTreeSideBarItem = null;
                    }
                }
                catch (Exception ex)
                {
                    await dialogViewService.AlertAsync(message: $"Exception : \"{ex.Message}\"", title: "Error: Selected TabItem Is Empty");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register Active TabItem Event
        private void RegisterActiveTabItemEvent()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            _eventAggregator.GetEvent<OnActiveTabItemEvent>().Subscribe(async (oatie) =>
            {
                var currentTabInfo = oatie.SelectedTabInfo;

                try
                {
                    SetSelectedTreeSideBarItem();

                    #region Set Selected TreeSideBarItem Method
                    void SetSelectedTreeSideBarItem()
                    {
                        #region Method
                        //var treeSideBarItem = FindtTreeSideBarItemViewModel(currentTabInfo);

                        //var selectedTreeSideBarItem = GetSelectedTreeSideBarItemViewModel();
                        //Debug.Assert(selectedTreeSideBarItem == _selectedTreeSideBarItem);

                        //if (treeSideBarItem is not null)
                        //{
                        //    if (treeSideBarItem != selectedTreeSideBarItem)
                        //    {
                        //        treeSideBarItem.IsSelected = true;
                        //        treeSideBarItem.IsExpanded = true;

                        //        if (selectedTreeSideBarItem is not null)
                        //        {
                        //            selectedTreeSideBarItem.IsSelected = false;
                        //        }
                        //    }
                        //}
                        #endregion

                        var matchTreeSideBarItem = FindtTreeSideBarItemViewModel(currentTabInfo);

                        var selectedTreeSideBarItem = GetSelectedTreeSideBarItemViewModel();
                        Debug.Assert(selectedTreeSideBarItem == SelectedTreeSideBarItem);

                        if (matchTreeSideBarItem is not null)
                        {
                            if (matchTreeSideBarItem == selectedTreeSideBarItem)
                            {
                                return;
                            }

                            if (selectedTreeSideBarItem is not null)
                            {
                                selectedTreeSideBarItem.IsSelected = false;
                            }

                            matchTreeSideBarItem.IsSelected = true;
                            matchTreeSideBarItem.IsExpanded = true;
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    await dialogViewService.AlertAsync(message: $"Exception : \"{ex.Message}\"", title: "Error: Active TabItem");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Reset/Clear Selected TreeSideBarItem Method
        internal void ClearSelectedTreeSideBarItem()
        {
            if (SelectedTreeSideBarItem is not null)
            {
                SelectedTreeSideBarItem.IsSelected = false;
                SelectedTreeSideBarItem = null;
                //_previewSelectedTreeSideBarItem = null;
            }
        }
        #endregion

        #region Create TreeSideBarItem ViewModel Method
        internal async Task CreateTreeSideBarItemViewModelsAsync()
        {
            IsLoading = true;

            var rootMenuItem = await _menuService.GetMenuAsync("All");
            var subMenuItems = rootMenuItem.SubMenus;
            NodeResolver<TreeSideBarItemViewModel> nodeResolver = new();

            foreach (var smi in subMenuItems)
            {
                TreeSideBarItemViewModel virtualParent = new();
                Func<MenuItem, TreeSideBarItemViewModel, TreeSideBarItemViewModel> childResolver = ((m, p) => { return new TreeSideBarItemViewModel(m, p); });
                var topItem = await nodeResolver.GetTopItemByMenuItemAsync(smi, virtualParent, childResolver, false);
                // AllTreeSideBarItems.Add(topItem);
                TopTreeSideBarItems.Add(topItem);

                //TreeSideBarItemViewModel treeSideBarItemViewModel = new(_eventAggregator, smi);
                //TopTreeSideBarItems.Add(treeSideBarItemViewModel);

                //TreeSideBarItemViewModel parent = new(_eventAggregator, smi);
                //AllTreeSideBarItems.Add(parent);
                //List<MenuItem> allTravelMenuItems = new();
                //await GetAllTreeBarItemSubViewModelsAsync(smi, allTravelMenuItems, parent);

                //var treeSideBarItemViewModel = await GetAllTreeSideBarItemViewModelsByMenuItem(smi);
                //AllTreeSideBarItems.Add(treeSideBarItemViewModel);
            }

            SetPropertyChanged();

            void SetPropertyChanged()
            {
                foreach (var tbi in TopTreeSideBarItems)
                {
                    int level = 1;
                    tbi.Level = level;
                    RecursiveSubMenuItem(tbi, level);
                }

                void RecursiveSubMenuItem(TreeSideBarItemViewModel treeSideBarItemViewModel, int level)
                {
                    AddPropertyChanged(treeSideBarItemViewModel);

                    if (treeSideBarItemViewModel.HasChildren)
                    {
                        level++;
                        foreach (var smi in treeSideBarItemViewModel.Children)
                        {
                            smi.Level = level;
                        }

                        foreach (var smi in treeSideBarItemViewModel.Children)
                        {
                            RecursiveSubMenuItem(smi as TreeSideBarItemViewModel, level);
                        }
                    }
                }
            }

            void AddPropertyChanged(TreeSideBarItemViewModel treeSideBarItemViewModel)
            {
                treeSideBarItemViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is TreeSideBarItemViewModel tsbivm)
                    {
                        if (e.PropertyName == nameof(TreeSideBarItemViewModel.IsSelected))
                        {
                            if (SelectedTreeSideBarItem is null &&
                                       (tsbivm is not null && tsbivm.IsSelected && tsbivm != SelectedTreeSideBarItem))
                            {
                                SelectedTreeSideBarItem = tsbivm;
                            }

                            if (SelectedTreeSideBarItem is not null &&
                                      (tsbivm is not null && tsbivm.IsSelected && tsbivm != SelectedTreeSideBarItem))
                            {
                                //SelectedTreeSideBarItem.IsSelected = false;

                                SelectedTreeSideBarItem = tsbivm;
                            }
                        }
                    }
                };
            }

            //bool HasChild(TreeSideBarItemViewModel tsbivm) => (tsbivm is not null) && tsbivm.Children.Any();

            IsLoading = false;
        }
        #endregion

        #region  SetPropertyChanged Method
        public void SetPropertyChanged()
        {
            foreach (var tbi in TopTreeSideBarItems)
            {
                int level = 1;
                tbi.Level = level;
                RecursiveSubMenuItem(tbi, level);
            }

            void RecursiveSubMenuItem(TreeSideBarItemViewModel treeSideBarItemViewModel, int level)
            {
                AddPropertyChanged(treeSideBarItemViewModel);

                if (treeSideBarItemViewModel.HasChildren)
                {
                    level++;
                    foreach (var smi in treeSideBarItemViewModel.Children)
                    {
                        smi.Level = level;
                    }

                    foreach (var smi in treeSideBarItemViewModel.Children)
                    {
                        RecursiveSubMenuItem(smi as TreeSideBarItemViewModel, level);
                    }
                }
            }
        }

        private void AddPropertyChanged(TreeSideBarItemViewModel treeSideBarItemViewModel)
        {
            treeSideBarItemViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is TreeSideBarItemViewModel tsbivm)
                {
                    if (e.PropertyName == nameof(TreeSideBarItemViewModel.IsSelected))
                    {
                        if (SelectedTreeSideBarItem is null &&
                                   (tsbivm is not null && tsbivm.IsSelected && tsbivm != SelectedTreeSideBarItem))
                        {
                            SelectedTreeSideBarItem = tsbivm;
                        }

                        if (SelectedTreeSideBarItem is not null &&
                                  (tsbivm is not null && tsbivm.IsSelected && tsbivm != SelectedTreeSideBarItem))
                        {
                            //SelectedTreeSideBarItem.IsSelected = false;

                            SelectedTreeSideBarItem = tsbivm;
                        }
                    }
                }
            };
        }
        #endregion

        #region Contain Methods
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
    }
}
