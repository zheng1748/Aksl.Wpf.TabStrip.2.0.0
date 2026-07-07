using System;
using System.Windows;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;

namespace Aksl.TabBits.ViewModels
{
    public class TabContentItemViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly TabInformation _tabInformation;
        #endregion

        #region Constructors
        public TabContentItemViewModel(TabInformation tabInformation)
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

            _tabInformation = tabInformation;
        }
        #endregion

        #region Properties
        public TabInformation TabInformation => _tabInformation;
        public string Name => _tabInformation.Name;
        public string Title => _tabInformation.Title;
        public string ViewName => _tabInformation.ViewName;

        private Type _viewElementType = default;
        public Type ViewElementType
        {
            get
            {
                if (_viewElementType is null)
                {
                    string viewTypeAssemblyQualifiedName = _tabInformation.ViewName;
                    _viewElementType = Type.GetType(viewTypeAssemblyQualifiedName);
                }

                return _viewElementType;
            }
        }

        private DependencyObject _viewElement = default;
        public DependencyObject ViewElement
        {
            get
            {
                if (_viewElement is null)
                {
                    if (ViewElementType is not null)
                    {
                        // viewElemen = Activator.CreateInstance(viewType) as DependencyObject;
                        _viewElement = (PrismApplication.Current as PrismApplicationBase).Container.Resolve(ViewElementType) as DependencyObject;
                    }
                }

                return _viewElement;
            }
            set
            {
                SetProperty<DependencyObject>(ref _viewElement, value);
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                    //if (ViewElement is not null)
                    //{
                    //    (ViewElement as UIElement).Visibility = _isSelected ? Visibility.Visible : Visibility.Collapsed;
                    //}
                    //if (_isSelected)
                    //{
                        if (ViewElement is not null)
                        {
                            (ViewElement as UIElement).Visibility = _isSelected ? Visibility.Visible : Visibility.Collapsed;
                        }
                    //}
                    //else
                    //{
                    //    if (ViewElement is not null)
                    //    {
                    //        ViewElement = null;
                    //    }
                    //}
                }
            }
        }

        private Visibility _viewElementVisibility = Visibility.Visible;
        public Visibility ViewElementVisibility
        {
            get => (ViewElement as UIElement).Visibility;
            set
            {
                if (SetProperty<Visibility>(ref _viewElementVisibility, value))
                {
                    (ViewElement as UIElement).Visibility=value;
                }
            }
        }
        #endregion
    }
}
