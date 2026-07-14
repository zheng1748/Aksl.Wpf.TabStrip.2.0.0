using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Unity;

namespace Aksl.TabStrip.ViewModels
{
    public class TabViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Constructors
        public TabViewModel()
        {
            _eventAggregator = PrismIocExtensions.GetUnityContainer().Resolve<IEventAggregator>();

            ActiveTabItems = new();
            //AddCollectionChangedEventHandler();
            StoreTabItems = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<TabItemViewModel> ActiveTabItems { get; }

        public TabItemViewModel? SelectedTabItem
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public List<TabItemViewModel> StoreTabItems { get; }

        public Dock TabStripPlacement
        {
            get => field;
            set => SetProperty<Dock>(ref field, value);
        }= Dock.Top;
        #endregion

        #region Methods
        //public void AddCollectionChangedEventHandler()
        //{
        //    ActiveTabItems.CollectionChanged += (sender, e) =>
        //    {
        //        if (e.NewItems is not null && e.NewItems.Count != 0)
        //        {
        //            foreach (TabItemViewModel tabItemViewModel in e.NewItems)
        //            {
        //                tabItemViewModel.RequestClose += this.OnTabItemRequestClose;
        //            }
        //        }

        //        if (e.OldItems is not null && e.OldItems.Count != 0)
        //        {
        //            foreach (TabItemViewModel tabItemViewModel in e.OldItems)
        //            {
        //                tabItemViewModel.RequestClose -= this.OnTabItemRequestClose;
        //            }
        //        }
        //    };
        //}

        private void OnTabItemRequestClose(object sender, EventArgs e)
        {
            if (sender is TabItemViewModel tabItemViewModel)
            {
                Remove(tabItemViewModel);
            }
        }

        public void Add(TabInformation tabInformation, bool isActive = true)
        {
            TabItemViewModel newTabItemViewModel = new(tabInformation);

            if (tabInformation.ViewElement is not null)
            {
                newTabItemViewModel.ViewElement = tabInformation.ViewElement;
            }

            AddCore(newTabItemViewModel, isActive);

            //TabItemViewModel newTabItemViewModel=new (menuItem);

            //TabItems.Add(newTabItemViewModel);

            //SetActiveTabItem(newTabItemViewModel);

            //RaisePropertyChanged(nameof(TabItems));
        }

        private void AddCore(TabItemViewModel newTabItemViewModel, bool isActive = true)
        {
            if (!IsExistsActivTabItems(newTabItemViewModel.Name, newTabItemViewModel.Title))
            {
                ActiveTabItems.Add(newTabItemViewModel);

                newTabItemViewModel.RequestClose += this.OnTabItemRequestClose;
            }

            StoreTabItem(newTabItemViewModel);

            if (isActive)
            {
                SetActiveTabItem(newTabItemViewModel);
            }

            RaisePropertyChanged(nameof(ActiveTabItems));
        }

        private void StoreTabItem(TabItemViewModel newTabItemViewModel)
        {
            if (!IsExistsStoreTabItems(newTabItemViewModel.Name, newTabItemViewModel.Title))
            {
                StoreTabItems.Add(newTabItemViewModel);
            }
        }

        public void Remove(TabItemViewModel tabItemViewModel)
        {
            if (tabItemViewModel is not null)
            {
                if (IsExistsActivTabItems(tabItemViewModel.Name, tabItemViewModel.Title))
                {
                    if (SelectedTabItem == tabItemViewModel || tabItemViewModel.IsSelected)
                    {
                        tabItemViewModel.IsSelected = false;
                    }

                    ActiveTabItems.Remove(tabItemViewModel);

                    tabItemViewModel.RequestClose -= this.OnTabItemRequestClose;
                }

                if (!ActiveTabItems.Any())
                {
                    SelectedTabItem = null;

                    _eventAggregator.GetEvent<OnSelectedTabItemEmptyEvent>().Publish(new());
                }

                RaisePropertyChanged(nameof(ActiveTabItems));
            }
        }

        public void SetFirstActiveTabItem()
        {
            if (ActiveTabItems.Any())
            {
                var activeTabItem = ActiveTabItems.First();
                SelectedTabItem = activeTabItem;
            }
            else if (StoreTabItems.Any())
            {
                var storeTabItem = StoreTabItems.First();
                SelectedTabItem = storeTabItem;
            }
        }

        private void SetActiveTabItem(TabItemViewModel tabItemViewModel)
        {
            if (tabItemViewModel is not null && !IsEqualsTabItemViewModel(tabItemViewModel, SelectedTabItem))
            {
                if (SelectedTabItem is null)
                {
                    SelectedTabItem = tabItemViewModel;
                }

                if (SelectedTabItem is not null && SelectedTabItem != tabItemViewModel)
                {
                    SelectedTabItem = tabItemViewModel;
                }
            }

            //if (tabItemViewModel is not null)
            //{
            //    ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.TabItems);
            //    if (collectionView is not null)
            //    {
            //        collectionView.MoveCurrentTo(tabItemViewModel);
            //    }

            //    collectionView.Refresh();
            //}
        }

        public void SetTabItem(TabInformation tabInformation)
        {
            var activeTabItem = GetActiveTabItemViewModelByInfo(tabInformation);
            if (activeTabItem is not null)
            {
                SetActiveTabItem(activeTabItem);
            }
            else
            {
                var storeTabItem = GetStoreTabItemViewModelByInfo(tabInformation);
                if (storeTabItem is not null)
                {
                    AddCore(storeTabItem);
                }
            }
        }

        public void SetActiveTabItemByName(string name)
        {
            var activeTabItem = GetActiveTabItemViewModelByName(name);
            if (activeTabItem is not null)
            {
                SetActiveTabItem(activeTabItem);
            }
            else
            {
                var storeTabItem = GetStoreTabItemViewModelByName(name);
                if (storeTabItem is not null)
                {
                    AddCore(storeTabItem);
                }
            }
        }

        public void RetsetTabItem(TabInformation tabInformation)
        {
            var oldActiveTabItem = GetActiveTabItemViewModelByInfo(tabInformation);
            if (oldActiveTabItem is not null)
            {
                oldActiveTabItem.ViewElement = null;

                if (tabInformation.ViewElement is not null)
                {
                    oldActiveTabItem.ViewElement = tabInformation.ViewElement;
                }

                SetActiveTabItem(oldActiveTabItem);
            }
            else
            {
                var oldStoreTabItem = GetStoreTabItemViewModelByInfo(tabInformation);
                if (oldStoreTabItem is not null)
                {
                    oldStoreTabItem.ViewElement = null;

                    if (tabInformation.ViewElement is not null)
                    {
                        oldStoreTabItem.ViewElement = tabInformation.ViewElement;
                    }

                    ActiveTabItems.Add(oldStoreTabItem);
                    oldStoreTabItem.RequestClose += this.OnTabItemRequestClose;

                    SetActiveTabItem(oldStoreTabItem);
                }
            }
        }

        public void RetsetTabItemNoActive(TabInformation tabInformation)
        {
            var oldActiveTabItem = GetActiveTabItemViewModelByInfo(tabInformation);
            if (oldActiveTabItem is not null)
            {
                oldActiveTabItem.ViewElement = null;

                if (tabInformation.ViewElement is not null)
                {
                    oldActiveTabItem.ViewElement = tabInformation.ViewElement;
                }
            }
            else
            {
                var oldStoreTabItem = GetStoreTabItemViewModelByInfo(tabInformation);
                if (oldStoreTabItem is not null)
                {
                    oldStoreTabItem.ViewElement = null;

                    if (tabInformation.ViewElement is not null)
                    {
                        oldStoreTabItem.ViewElement = tabInformation.ViewElement;
                    }

                    ActiveTabItems.Add(oldStoreTabItem);
                    oldStoreTabItem.RequestClose += this.OnTabItemRequestClose;
                }
            }
        }

        private TabItemViewModel? GetActiveTabItemViewModelByInfo(TabInformation tabInformation)
        {
            var activeTabItem = ActiveTabItems.FirstOrDefault(ti => IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title));

            return activeTabItem;
        }

        private TabItemViewModel GetActiveTabItemViewModelByName(string name)
        {
            var activeTabItem = ActiveTabItems.FirstOrDefault(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, name));

            return activeTabItem;
        }

        public TabItemViewModel? GetStoreTabItemViewModelByInfo(TabInformation tabInformation)
        {
            var storeTabItem = StoreTabItems.FirstOrDefault(sti => IsEqualsNameOrTitle(sti.Name, tabInformation.Name) || IsEqualsNameOrTitle(sti.Title, tabInformation.Title));

            return storeTabItem;
        }

        public TabItemViewModel GetStoreTabItemViewModelByName(string name)
        {
            var storeTabItem = StoreTabItems.FirstOrDefault(sti => IsEqualsNameOrTitle(sti.Name, name) || IsEqualsNameOrTitle(sti.Title, name));

            return storeTabItem;
        }

        public System.Windows.DependencyObject? GetStoreViewElementByName(string name)
        {
            var storeTabItem= StoreTabItems.FirstOrDefault(sti => IsEqualsNameOrTitle(sti.Name, name) || IsEqualsNameOrTitle(sti.Title, name));

            return storeTabItem?.ViewElement;
        }

        public System.Windows.DependencyObject? GetStoreViewElementByType(Type viewType)
        {
            var storeTabItem = StoreTabItems.FirstOrDefault(sti => sti.ViewElementType == viewType);

            return storeTabItem?.ViewElement;
        }

        public TabInformation? GetStoreTabInformationByName(string name)
        {
            var storeTabItem = StoreTabItems.FirstOrDefault(sti => IsEqualsNameOrTitle(sti.Name, name) || IsEqualsNameOrTitle(sti.Title, name));

            return storeTabItem?.TabInformation;
        }

        public TabItemViewModel? GetStoreTabItemByName<T>(string name)
        {
            Type viewType = typeof(T);

            var storeTabItem = StoreTabItems.FirstOrDefault(sti => (sti.ViewElementType is not null && sti.ViewElementType == viewType) && IsEqualsNameOrTitle(sti.Name, name));

            return storeTabItem;
        }

        public bool IsActiveTabItem(TabInformation tabInformation)
        {
            // var _isExists = ActiveTabItems.Any(ti => ti.IsSelected && (IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title)));

            //  var activeTabItemViewModel = ActiveTabItems.FirstOrDefault(ti => ti.IsSelected);
            //  var isExists = IsEqualsNameOrTitle(activeTabItemViewModel?.Name, tabInformation.Name) || IsEqualsNameOrTitle(activeTabItemViewModel?.Title, tabInformation.Title);

            var isAny = ActiveTabItems.Any(ati => ati.IsSelected && (IsEqualsNameOrTitle(ati.Name, tabInformation.Name) || IsEqualsNameOrTitle(ati.Title, tabInformation.Title)));

            return isAny;
        }

        public bool IsActiveTabItemByName(string name)
        {
            var isAny = ActiveTabItems.Any(ti => ti.IsSelected && (IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, name)));

            return isAny;
        }
        #endregion

        #region Contain Methods
        private bool IsExistsActivTabItems(string name, string title)
        {
            var isAny = ActiveTabItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsExistsStoreTabItems(string name, string title)
        {
            var isAny = StoreTabItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsEqualsTabItemViewModel(TabItemViewModel tabItemViewModel, TabItemViewModel otherTabItemViewModel)
        {
            if (tabItemViewModel is null || otherTabItemViewModel is null)
            {
                return false;
            }

            var isEquals = IsEqualsNameOrTitle(tabItemViewModel.Name, otherTabItemViewModel.Name) ||
                           IsEqualsNameOrTitle(tabItemViewModel.Title, otherTabItemViewModel.Title);

            return isEquals;
        }

        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isEquals = nameOrTitle.Equals(otherNameOrTitle, StringComparison.OrdinalIgnoreCase) ||
                           otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase);

            return isEquals;
        }
        #endregion
    }
}
