using System;
using System.Windows;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Aksl.ActiveContents.ViewModels            
{
    public class ActiveContentItemViewModel : BindableBase
    {
        #region Members
        private readonly ContentInformation _contentInformation;
        #endregion

        #region Constructors
        public ActiveContentItemViewModel(ContentInformation  contentInformation)
        {
            _contentInformation = contentInformation;
        }
        #endregion

        #region Properties
        public ContentInformation ContentInformation => _contentInformation;
        public string Name => _contentInformation.Name;
        public string Title => _contentInformation.Title;
        public string ViewName => _contentInformation.ViewName;

        private Type _viewElementType = default;
        public Type ViewElementType
        {
            get
            {
                if (_viewElementType is null)
                {
                    if (!string.IsNullOrEmpty(_contentInformation.ViewName))
                    {
                        string viewTypeAssemblyQualifiedName = _contentInformation.ViewName;
                        _viewElementType = Type.GetType(viewTypeAssemblyQualifiedName);
                    }
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
                        _viewElement = Activator.CreateInstance(ViewElementType) as DependencyObject;
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
                    if (ViewElement is not null)
                    {
                        (ViewElement as UIElement).Visibility = value;
                    }
                }
            }
        }
        #endregion
    }
}
