using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class TabHubViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Constructors
        public TabHubViewModel()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

            TabHeaderViewModel = new();
            TabContentViewModel = new();

            RegisterPropertyChanged();
        }
        #endregion

        #region Properties
        public TabHeaderViewModel TabHeaderViewModel { get; set; }

        public TabContentViewModel TabContentViewModel { get; set; }

        public bool HasContent
        {
            get
            { 
                return TabContentViewModel.ActiveTabContentItems is not null &&TabContentViewModel.ActiveTabContentItems.Any();
            }
        }
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            TabHeaderViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is TabHeaderViewModel thvm)
                {
                    if (e.PropertyName == nameof(TabHeaderViewModel.SelectedTabHeaderItem))
                    {
                        if (thvm.SelectedTabHeaderItem is not null &&  thvm.SelectedTabHeaderItem.IsSelected)
                        {
                            TabContentViewModel.SetTabItemOnSelected(thvm.SelectedTabHeaderItem.TabInformation);
                        }
                    }
                }
            };

            TabContentViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is TabContentViewModel tvvm)
                {
                    if (e.PropertyName == nameof(TabContentViewModel.ActiveTabContentItems))
                    {
                        RaisePropertyChanged(nameof(HasContent));
                    }
                }
            };
        }
        #endregion

        #region Methods
        public void Add(TabInformation tabInformation)
        {
            TabHeaderViewModel.Add(tabInformation);

            TabHeaderViewModel.RequestClose += (sender, e) =>
            {
                if (sender is TabHeaderItemViewModel tabHeaderItemViewModel)
                {
                    TabContentViewModel.SetTabItemOnClose(tabHeaderItemViewModel.TabInformation);
                }
            };

            TabContentViewModel.Add(tabInformation);
        }
       
        public void SetTabItem(TabInformation tabInformation)
        {
            TabHeaderViewModel.SetTabHeaderItem(tabInformation);

            TabContentViewModel.SetTabContentItem(tabInformation);
        }

        public void RetsetTabItem(TabInformation tabInformation)
        {
            TabHeaderViewModel.RetsetTabItem(tabInformation);

            TabContentViewModel.RetsetTabItem(tabInformation);
        }

        public System.Windows.DependencyObject GetStoreViewElementByType(Type viewType)
        {
            var viewElement = TabContentViewModel.GetStoreViewElementByType(viewType);

            return viewElement;
        }

        public bool IsActiveTabItem(TabInformation tabInformation)
        {
            var isActive = TabHeaderViewModel.IsActiveTabItem(tabInformation);

            return isActive;
        }
        #endregion

        #region Contain Methods
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
