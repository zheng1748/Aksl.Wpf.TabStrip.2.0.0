using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Aksl.TabStripPlus.ViewModels
{
    public class TabContentViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Constructors
        public TabContentViewModel()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

            //TabContentItems=new();

            ActiveTabContentItems= new();
            StoreTabContentItems = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<TabContentItemViewModel> ActiveTabContentItems { get; }
        public List<TabContentItemViewModel> StoreTabContentItems { get; }

        private TabContentItemViewModel _selectedTabContentItem;
        public TabContentItemViewModel SelectedTabContentItem
        {
            get => _selectedTabContentItem;
            set => SetProperty<TabContentItemViewModel>(ref _selectedTabContentItem, value);
        }
        #endregion

        #region Methods
        public void Add(TabPlusInformation tabPlusInformation)
        {
            TabContentItemViewModel newTabContentItemViewModel = new(tabPlusInformation);

            if (tabPlusInformation.ViewElement is not null)
            {
                newTabContentItemViewModel.ViewElement = tabPlusInformation.ViewElement;
            }

           AddCore(newTabContentItemViewModel);
        }

        private void AddCore(TabContentItemViewModel newTabContentItemViewModel)
        {
            if (!IsExistsActivTabContentItems(newTabContentItemViewModel.Name, newTabContentItemViewModel.Title))
            {
                ActiveTabContentItems.Add(newTabContentItemViewModel);
            }

           // AddPropertyChanged();
            void AddPropertyChanged()
            {
                newTabContentItemViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is TabContentItemViewModel tcivm)
                    {
                        if (e.PropertyName == nameof(TabContentItemViewModel.IsSelected))
                        {
                            if (SelectedTabContentItem is null && (tcivm is not null && tcivm.IsSelected))
                            {
                                SelectedTabContentItem = tcivm;
                            }

                            if (SelectedTabContentItem is not null && (tcivm is not null && tcivm.IsSelected && tcivm != SelectedTabContentItem))
                            {
                                SelectedTabContentItem.IsSelected = false;

                                SelectedTabContentItem = tcivm;
                            }
                        }
                    }
                };
            }

            StoreTabContentItem(newTabContentItemViewModel);

            SetActiveTabContentItem(newTabContentItemViewModel);

            RaisePropertyChanged(nameof(ActiveTabContentItems));
        }

        private void StoreTabContentItem(TabContentItemViewModel tabContentItemViewModel)
        {
            if (!IsExistsStoreTabContentItems(tabContentItemViewModel.Name, tabContentItemViewModel.Title))
            {
                StoreTabContentItems.Add(tabContentItemViewModel);
            }
        }

        public void SetActiveTabContentItem(TabContentItemViewModel tabContentItemViewModel)
        {
            if (tabContentItemViewModel is not null && !IsEqualsTabContentItemViewModel(tabContentItemViewModel, SelectedTabContentItem))
            {
                if (SelectedTabContentItem is null)
                {
                    SelectedTabContentItem = tabContentItemViewModel; 
                    SelectedTabContentItem.IsSelected = true;
                }

                if (SelectedTabContentItem is not null && tabContentItemViewModel != SelectedTabContentItem)
                {
                    SelectedTabContentItem.IsSelected = false;
                   // SelectedTabContentItem.ViewElementVisibility = Visibility.Collapsed;

                    SelectedTabContentItem = tabContentItemViewModel;
                    SelectedTabContentItem.IsSelected = true;
                  //  SelectedTabContentItem.ViewElementVisibility = Visibility.Visible;
                }
            }
        }

        public void SetTabContentItem(TabPlusInformation tabInformation)
        {
            var activeTabContentItem= GetActiveTabContentItemByInfo(tabInformation);
            if (activeTabContentItem is not null)
            {
                SetActiveTabContentItem(activeTabContentItem);
            }
            else
            {
                var storeTabContentItem = GetStoreTabContentItemViewModelByInfo(tabInformation);
                if (storeTabContentItem is not null)
                {
                    AddCore(storeTabContentItem);
                }
            }
        }

        public void RetsetTabItem(TabPlusInformation tabPlusInformation)
        {
            var activeTabContentItem = GetActiveTabContentItemByInfo(tabPlusInformation);
            if (activeTabContentItem is not null)
            {
                activeTabContentItem.ViewElement = null;
                if (tabPlusInformation.ViewElement is not null)
                {
                    activeTabContentItem.ViewElement = tabPlusInformation.ViewElement;
                }

                SetActiveTabContentItem(activeTabContentItem);
            }
            else
            {
                var storeContentTabItem = GetStoreTabContentItemViewModelByInfo(tabPlusInformation);
                if (storeContentTabItem is not null)
                {
                    storeContentTabItem.ViewElement = null;
                    if (tabPlusInformation.ViewElement is not null)
                    {
                        storeContentTabItem.ViewElement = tabPlusInformation.ViewElement;
                    }

                    ActiveTabContentItems.Add(storeContentTabItem);

                    SetActiveTabContentItem(storeContentTabItem);
                }
            }
        }

        public void SetTabItemOnClose(TabPlusInformation tabPlusInformation)
        {
            var activeTabContentItem = GetActiveTabContentItemByInfo(tabPlusInformation);
            if (activeTabContentItem is not null)
            {
                Remove(activeTabContentItem);
            }
        }

        public void Remove(TabContentItemViewModel tabContentItemViewModel)
        {
            if (tabContentItemViewModel is not null)
            {
                if (IsExistsActivTabContentItems(tabContentItemViewModel.Name, tabContentItemViewModel.Title))
                {
                    if (SelectedTabContentItem == tabContentItemViewModel ||  tabContentItemViewModel.IsSelected)
                    {
                        tabContentItemViewModel.IsSelected = false;
                        SelectedTabContentItem = null;
                    }

                    ActiveTabContentItems.Remove(tabContentItemViewModel);
                }

                if (!ActiveTabContentItems.Any())
                {
                    SelectedTabContentItem = null;
                }

                RaisePropertyChanged(nameof(ActiveTabContentItems));
            }
        }

        public void SetTabItemOnSelected(TabPlusInformation tabPlusInformation)
        {
            var activeTabContentItem = GetActiveTabContentItemByInfo(tabPlusInformation);
            if (activeTabContentItem is not null)
            {
                SetActiveTabContentItem(activeTabContentItem);
            }
        }

        private TabContentItemViewModel GetActiveTabContentItemByInfo(TabPlusInformation tabPlusInformation)
        {
            var activeTabContentItemViewModel = ActiveTabContentItems.FirstOrDefault(tc => IsEqualsNameOrTitle(tc.Name, tabPlusInformation.Name) || IsEqualsNameOrTitle(tc.Title, tabPlusInformation.Title));

            return activeTabContentItemViewModel;
        }

        public TabContentItemViewModel GetStoreTabContentItemViewModelByInfo(TabPlusInformation tabPlusInformation)
        {
            var storeTabContentItem = StoreTabContentItems.FirstOrDefault(stc => IsEqualsNameOrTitle(stc.Name, tabPlusInformation.Name) || IsEqualsNameOrTitle(stc.Title, tabPlusInformation.Title));

            return storeTabContentItem;
        }

        public System.Windows.DependencyObject GetStoreViewElementByType(Type viewType)
        {
            var tabContentItem = StoreTabContentItems.FirstOrDefault(stc => stc.ViewElementType == viewType);

            return tabContentItem?.ViewElement;
        }
        #endregion

        #region Contain Methods
        private bool IsExistsActivTabContentItems(string name, string title)
        {
            var isAny = ActiveTabContentItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsExistsStoreTabContentItems(string name, string title)
        {
            var isAny = StoreTabContentItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsEqualsTabContentItemViewModel(TabContentItemViewModel tabContentItemViewModel, TabContentItemViewModel otherTabContentItemViewModel)
        {
            if (tabContentItemViewModel is null || otherTabContentItemViewModel is null)
            {
                return false;
            }

            var isEquals = (IsEqualsNameOrTitle(tabContentItemViewModel?.Name, otherTabContentItemViewModel?.Name) ||
                            IsEqualsNameOrTitle(tabContentItemViewModel?.Title, otherTabContentItemViewModel?.Title));

            return isEquals;
        }

        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isEquals = (!string.IsNullOrEmpty(nameOrTitle) && nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase)) ||
                           (!string.IsNullOrEmpty(otherNameOrTitle) && otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase));

            return isEquals;
        }
        #endregion
    }
}
