using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Aksl.TabBits.ViewModels
{
    public class TabHeaderViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Constructors
        public TabHeaderViewModel()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

            //TabHeaderItems = new();

            ActiveTabHeaderItems = new();
            StoreTabHeaderItems = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<TabHeaderItemViewModel> ActiveTabHeaderItems { get; }
        public List<TabHeaderItemViewModel> StoreTabHeaderItems { get; }

        private TabHeaderItemViewModel _selectedTabHeaderItem;
        public TabHeaderItemViewModel SelectedTabHeaderItem
        {
            get => _selectedTabHeaderItem;
            set => SetProperty<TabHeaderItemViewModel>(ref _selectedTabHeaderItem, value);
        }
        #endregion

        #region Event Handler
        public event EventHandler RequestClose;
        private void OnTabHeaderItemRequestClose(object sender, EventArgs e)
        {
            if (sender is TabHeaderItemViewModel tabHeaderItemViewModel)
            {
                TabHeaderItemViewModel nextTabHeaderItemViewModel = default;
                if (ActiveTabHeaderItems.Any())
                {
                    nextTabHeaderItemViewModel = GetNextActiveTabHeaderItemByInfo(tabHeaderItemViewModel.TabInformation);
                }

                //if (SelectedTabHeaderItem == tabHeaderItemViewModel)
                //{
                //    SelectedTabHeaderItem.IsSelected = false;
                //}

                Remove(tabHeaderItemViewModel);

                RequestClose?.Invoke(sender, EventArgs.Empty);

                if (nextTabHeaderItemViewModel is not null)
                {
                    if (SelectedTabHeaderItem is null)
                    {
                        //SelectedTabHeaderItem.IsSelected = false;
                        nextTabHeaderItemViewModel.IsSelected = true;
                    }

                    //if (SelectedTabHeaderItem is not null && SelectedTabHeaderItem == nextTabHeaderItemViewModel)
                    //{
                    //    SelectedTabHeaderItem.IsSelected = false;
                    //}

                    //nextTabHeaderItemViewModel.IsSelected = true;
                    // nextTabHeaderItemViewModel.IsSelected = true;
                    // SelectedTabHeaderItem = nextTabHeaderItemViewModel;
                    // SelectedTabHeaderItem.IsSelected = true;
                }
            }
        }
        #endregion

        #region Methods
        public void Add(TabInformation tabInformation)
        {
            TabHeaderItemViewModel newTabHeaderItemViewModel = new(tabInformation);

            AddCore(newTabHeaderItemViewModel);
        }

        private void AddCore(TabHeaderItemViewModel newTabHeaderItemViewModel)
        {
            if (!IsExistsActivTabHeaderItems(newTabHeaderItemViewModel.Name, newTabHeaderItemViewModel.Title))
            {
                ActiveTabHeaderItems.Add(newTabHeaderItemViewModel);

                newTabHeaderItemViewModel.RequestClose += this.OnTabHeaderItemRequestClose;
            }

           //AddPropertyChanged();
            void AddPropertyChanged()
            {
                newTabHeaderItemViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is TabHeaderItemViewModel tbhivm)
                    {
                        if (e.PropertyName == nameof(TabHeaderItemViewModel.IsSelected))
                        {
                            if (SelectedTabHeaderItem is null)
                            {
                                SelectedTabHeaderItem = tbhivm;
                            }

                            if (SelectedTabHeaderItem is not null && tbhivm.IsSelected && tbhivm != SelectedTabHeaderItem)
                            {
                                //SelectedTabHeaderItem.IsSelected = false;

                                SelectedTabHeaderItem = tbhivm;
                            }
                        }
                    }
                };
            }

            StoreTabHeaderItem(newTabHeaderItemViewModel);

            SetActiveTabHeaderItem(newTabHeaderItemViewModel);

            RaisePropertyChanged(nameof(ActiveTabHeaderItems));
        }

        private void StoreTabHeaderItem(TabHeaderItemViewModel tabHeaderItemViewModel)
        {
            if (!IsExistsStoreTabHeaderItems(tabHeaderItemViewModel.Name, tabHeaderItemViewModel.Title))
            {
                StoreTabHeaderItems.Add(tabHeaderItemViewModel);
            }
        }

        private void SetActiveTabHeaderItem(TabHeaderItemViewModel tabHeaderItemViewModel)
        {
            if (tabHeaderItemViewModel is not null && !IsEqualsTabHeaderItemViewModel(tabHeaderItemViewModel, SelectedTabHeaderItem))
            {
                if (SelectedTabHeaderItem is null)
                {
                    //tabHeaderItemViewModel.IsSelected = true;

                    SelectedTabHeaderItem = tabHeaderItemViewModel;
                    //SelectedTabHeaderItem.IsSelected = true;
                }

                if (SelectedTabHeaderItem is not null && tabHeaderItemViewModel != SelectedTabHeaderItem)
                {
                    //tabHeaderItemViewModel.IsSelected = true;

                    //SelectedTabHeaderItem.IsSelected = false;

                    SelectedTabHeaderItem = tabHeaderItemViewModel;
                  //  SelectedTabHeaderItem.IsSelected = true;

                    //tabHeaderItemViewModel.IsSelected = true;
                    //SelectedTabHeaderItem = tabHeaderItemViewModel;
                }
            }
        }

        public void Remove(TabHeaderItemViewModel tabHeaderItemViewModel)
        {
            if (tabHeaderItemViewModel is not null)
            {
                if (IsExistsActivTabHeaderItems(tabHeaderItemViewModel.Name, tabHeaderItemViewModel.Title))
                {
                    if (SelectedTabHeaderItem == tabHeaderItemViewModel || tabHeaderItemViewModel.IsSelected)
                    {
                        tabHeaderItemViewModel.IsSelected = false;

                        var isIsSelectedToActive= ActiveTabHeaderItems.Any(ti => ti.IsSelected && (IsEqualsNameOrTitle(ti.Name, tabHeaderItemViewModel.Name) || IsEqualsNameOrTitle(ti.Title, tabHeaderItemViewModel.Title)));
                        var isIsSelectedToStore = StoreTabHeaderItems.Any(ti => ti.IsSelected && (IsEqualsNameOrTitle(ti.Name, tabHeaderItemViewModel.Name) || IsEqualsNameOrTitle(ti.Title, tabHeaderItemViewModel.Title)));

                        Debug.Assert(isIsSelectedToActive is false);
                        Debug.Assert(isIsSelectedToStore is false);
                    }

                    ActiveTabHeaderItems.Remove(tabHeaderItemViewModel);

                    tabHeaderItemViewModel.RequestClose -= this.OnTabHeaderItemRequestClose;
                }

                if (!ActiveTabHeaderItems.Any())
                {
                    SelectedTabHeaderItem = null;

                    _eventAggregator.GetEvent<OnSelectedTabHeaderItemEmptyEvent>().Publish(new());
                }

                RaisePropertyChanged(nameof(ActiveTabHeaderItems));
            }
        }

        public void SetTabHeaderItem(TabInformation tabInformation)
        {
            var activeTabHeaderItem = GetActiveTabHeaderItemByInfo(tabInformation);
            if (activeTabHeaderItem is not null)
            {
                SetActiveTabHeaderItem(activeTabHeaderItem);
            }
            else
            {
                var storeTabHeaderItem = GetStoreTabHeaderItemViewModelByInfo(tabInformation);
                if (storeTabHeaderItem is not null)
                {
                    AddCore(storeTabHeaderItem);
                }
            }
        }

        public void RetsetTabItem(TabInformation tabInformation)
        {
            var activeTabHeaderItem = GetActiveTabHeaderItemByInfo(tabInformation);
            if (activeTabHeaderItem is not null)
            {
                SetActiveTabHeaderItem(activeTabHeaderItem);
            }
            else
            {
                var storeTabHeaderItem = GetStoreTabHeaderItemViewModelByInfo(tabInformation);
                if (storeTabHeaderItem is not null)
                {
                    ActiveTabHeaderItems.Add(storeTabHeaderItem);
                    storeTabHeaderItem.RequestClose += this.OnTabHeaderItemRequestClose;

                    SetActiveTabHeaderItem(storeTabHeaderItem);
                }
            }
        }

        private TabHeaderItemViewModel GetActiveTabHeaderItemByInfo(TabInformation tabInformation)
        {
            var activeTabHeaderItem = ActiveTabHeaderItems.FirstOrDefault(th => IsEqualsNameOrTitle(th.Name, tabInformation.Name) || IsEqualsNameOrTitle(th.Title, tabInformation.Title));

            return activeTabHeaderItem;
        }

        public TabHeaderItemViewModel GetStoreTabHeaderItemViewModelByInfo(TabInformation tabInformation)
        {
            var storeTabHeaderItem = StoreTabHeaderItems.FirstOrDefault(sth => IsEqualsNameOrTitle(sth.Name, tabInformation.Name) || IsEqualsNameOrTitle(sth.Title, tabInformation.Title));

            return storeTabHeaderItem;
        }

        private TabHeaderItemViewModel GetNextActiveTabHeaderItemByInfo(TabInformation tabInformation)
        {
            TabHeaderItemViewModel nextTabHeaderItemViewModel = default;

            var index = ActiveTabHeaderItems.ToList().FindIndex(th => IsEqualsNameOrTitle(th.Name, tabInformation.Name) || IsEqualsNameOrTitle(th.Title, tabInformation.Title));

            if ((index + 1) <= (ActiveTabHeaderItems.Count - 1))
            {
                nextTabHeaderItemViewModel = ActiveTabHeaderItems[index + 1];
            }
            else if ((index - 1) >= 0)
            {
                nextTabHeaderItemViewModel = ActiveTabHeaderItems[index - 1];
            }

            //if (index == (TabHeaderItems.Count - 1))
            //{
            //    if ((index - 1) >= 0)
            //    {
            //        nextTabHeaderItemViewModel = TabHeaderItems[index - 1];
            //    }
            //}
            //else if (index < (TabHeaderItems.Count - 1))
            //{
            //    if ((index + 1) <= (TabHeaderItems.Count - 1))
            //    {
            //        nextTabHeaderItemViewModel = TabHeaderItems[index + 1];
            //    }
            //}

            return nextTabHeaderItemViewModel;
        }

        public bool IsActiveTabItem(TabInformation tabInformation)
        {
            var isAny = ActiveTabHeaderItems.Any(th => th.IsSelected && (IsEqualsNameOrTitle(th.Name, tabInformation.Name) || IsEqualsNameOrTitle(th.Title, tabInformation.Title)));

            return isAny;
        }
        #endregion

        #region Contain Methods
        private bool IsExistsActivTabHeaderItems(string name, string title)
        {
            var isAny = ActiveTabHeaderItems.Any(th => IsEqualsNameOrTitle(th.Name, name) || IsEqualsNameOrTitle(th.Title, title));

            return isAny;
        }

        private bool IsExistsStoreTabHeaderItems(string name, string title)
        {
            var isAny = StoreTabHeaderItems.Any(sth => IsEqualsNameOrTitle(sth.Name, name) || IsEqualsNameOrTitle(sth.Title, title));

            return isAny;
        }

        private bool IsEqualsTabHeaderItemViewModel(TabHeaderItemViewModel tabHeaderItemViewModel, TabHeaderItemViewModel otherTabHeaderItemViewModel)
        {
            if (tabHeaderItemViewModel is null || otherTabHeaderItemViewModel is null)
            {
                return false;
            }

            var isEquals = (IsEqualsNameOrTitle(tabHeaderItemViewModel?.Name, otherTabHeaderItemViewModel?.Name) ||
                            IsEqualsNameOrTitle(tabHeaderItemViewModel?.Title, otherTabHeaderItemViewModel?.Title));

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
