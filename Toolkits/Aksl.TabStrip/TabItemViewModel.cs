using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace Aksl.TabStrip.ViewModels
{
    public class TabItemViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly TabInformation _tabInformation;
        #endregion

        #region Constructors
        public TabItemViewModel(TabInformation tabInformation)
        {
            _eventAggregator = PrismIocExtensions.GetUnityContainer().Resolve<IEventAggregator>();

            _tabInformation = tabInformation;

            CloseTabButtonVisibility = _tabInformation.CloseTabButtonVisibility;

            CreateExecuteCloseTabCommand();
        }
        #endregion

        #region Properties
        public TabInformation TabInformation => _tabInformation;
        public string Name => _tabInformation.Name;
        public string Title => _tabInformation.Title;
        public string ViewName => _tabInformation.ViewName;

        public Type ViewElementType
        {
            get
            {
                if (field is null)
                {
                    if (!string.IsNullOrEmpty(_tabInformation.ViewName))
                    {
                        string viewTypeAssemblyQualifiedName = _tabInformation.ViewName;
                        field = Type.GetType(viewTypeAssemblyQualifiedName);
                    }
                }

                return field;
            }
        }

        public DependencyObject? ViewElement
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
            get=> field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (field)
                    {
                        _eventAggregator.GetEvent<OnActiveTabItemEvent>().Publish(new() {  SelectedTabInfo = _tabInformation });
                    }
                }
            }
        }

        public Visibility CloseTabButtonVisibility
        {
            get => field;
            set => SetProperty<Visibility>(ref field, value);
        }
        #endregion

        #region CloseTab Command
        private event EventHandler _requestCloseEventHandler;
        public event EventHandler RequestClose
        {
            add { _requestCloseEventHandler += value; }
            remove { _requestCloseEventHandler -= value; }
        }

        public ICommand ExecuteCloseTabCommand { get; private set; }

        private void CreateExecuteCloseTabCommand()
        {
            ExecuteCloseTabCommand = new DelegateCommand(() =>
            {
                _requestCloseEventHandler?.Invoke(this, EventArgs.Empty);
            },
            () =>
            {
                return true;
            });
        }
        #endregion
    }
}
