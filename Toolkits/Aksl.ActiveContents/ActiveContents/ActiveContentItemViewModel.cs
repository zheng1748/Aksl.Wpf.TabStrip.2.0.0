using System;
using System.Windows;

using Prism.Mvvm;

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

        public Type ViewElementType
        {
            get
            {
                if (field is null)
                {
                    if (!string.IsNullOrEmpty(_contentInformation.ViewName))
                    {
                        string viewTypeAssemblyQualifiedName = _contentInformation.ViewName;
                        field = Type.GetType(viewTypeAssemblyQualifiedName);
                    }
                }

                return field;
            }
        }

        public DependencyObject ViewElement
        {
            get
            {
                if (field is null)
                {
                    if (ViewElementType is not null)
                    {
                        field = Activator.CreateInstance(ViewElementType) as DependencyObject;
                    }
                }

                return field;
            }
            set
            {
                SetProperty<DependencyObject>(ref field, value);
            }
        }

        public bool IsSelected
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (ViewElement is not null)
                    {
                        (ViewElement as UIElement).Visibility = field ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        public Visibility ViewElementVisibility
        {
            get => (ViewElement as UIElement).Visibility;
            set
            {
                if (SetProperty<Visibility>(ref field, value))
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
