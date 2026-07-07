using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Aksl.TabBits.ViewModels
{
    public class TabHeaderItemViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly TabInformation _tabInformation;
        #endregion

        #region Constructors
        public TabHeaderItemViewModel(TabInformation tabInformation)
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

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

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                    if (_isSelected)
                    {
                        _eventAggregator.GetEvent<OnActiveTabHeaderItemEvent>().Publish(new() { SelectedTabInfo= _tabInformation });
                    }
                }
            }
        }

        private Visibility _closeTabButtonVisibility = Visibility.Visible;
        public Visibility CloseTabButtonVisibility
        {
            get => _closeTabButtonVisibility;
            set => SetProperty<Visibility>(ref _closeTabButtonVisibility, value);
        }
        #endregion

        #region MouseLeftButton Down
        public void ExecuteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is UserControl uc) 
            {
                IsSelected=true; 
            }
        }
        #endregion

        #region CloseTab Command
        public event EventHandler RequestClose;
        public ICommand ExecuteCloseTabCommand { get; private set; }

        private void CreateExecuteCloseTabCommand()
        {
            ExecuteCloseTabCommand = new DelegateCommand(() =>
            {
                RequestClose?.Invoke(this, EventArgs.Empty);
            },
            () =>
            {
                return true;
            });
        }
        #endregion
    }
}
