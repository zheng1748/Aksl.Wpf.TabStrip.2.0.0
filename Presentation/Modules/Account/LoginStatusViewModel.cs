using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Unity;

using Aksl.Dialogs.Services;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;

using Aksl.Modules.Account.Views;

namespace Aksl.Modules.Account.ViewModels
{
    public class LoginStatusViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        #endregion

        #region Constructors
        public LoginStatusViewModel()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();

            CreateSignInCommand();
            CreateSignOutCommand();

            RegisterSignInedEvent();

            RegisterPropertyChanged();

            IsSignIning = true;

            Title = "Please Login";
        }
        #endregion

        #region Properties
        public string Title
        {
            get => field;
            set => SetProperty<string>(ref field, value);
        }

        public string? UserName
        {
            get => field;
            set => SetProperty<string>(ref field, value);
        }

        public bool IsSignIning
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        }

        private bool _isAuthenticated = false;
        public bool IsAuthenticated
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        }
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            this.PropertyChanged += (sender, e) =>
            {
                if (sender is LoginStatusViewModel)
                {
                    if (e.PropertyName == nameof(IsSignIning))
                    {
                        (SignInCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                        (SignOutCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    }
                }
            };
        }
        #endregion

        #region Register SignIned Event
        private void RegisterSignInedEvent()
        {
            _eventAggregator.GetEvent<OnSignInedEvent>().Subscribe((siet) =>
            {
                IsSignIning = false;

                UserName = siet.UserName;
                IsAuthenticated = siet.IsSuccessful;

                Title = null;
                Title = IsAuthenticated ? $"{UserName} Login" : "Please Login";

            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region SignIn Command
        public ICommand SignInCommand { get; private set; }

        private void CreateSignInCommand()
        {
            SignInCommand = new DelegateCommand(async () =>
            {
                await ExecuteSignInCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecuteSignInCommand();
                return canExecute;
            });
        }

        private async Task ExecuteSignInCommandAsync()
        {
            IsSignIning = true;

            try
            {
                ShellActiveContentExtensions.RetsetActiveContentToLoginView();
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync($"{ex.Message}", "Sign In Failure:");
            }
        }

        private bool CanExecuteSignInCommand()
        {
            return !IsSignIning;
        }
        #endregion

        #region SignOut Command
        //AuthenticationService
        public ICommand SignOutCommand { get; private set; }

        private void CreateSignOutCommand()
        {
            SignOutCommand = new DelegateCommand(async () =>
            {
                await ExecuteSignOutCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecuteSignOutCommand();
                return canExecute;
            });
        }

        private async Task ExecuteSignOutCommandAsync()
        {
            IsSignIning = true;

            if (ServiceExtensions.GetWebApiProvider().IsAccessTokenExpired)
            {
                IsAuthenticated = false;
                UserName = null;
                RaisePropertyChanged(nameof(UserName));
                ServiceExtensions.GetLoginHandler().BindAccessTokenAction(null,null);
                ShellActiveContentExtensions.RetsetActiveContentToLoginView();
            }
            else
            {
                try
                {
                    var loginOutResponse = await ServiceExtensions.GetLoginHandler().ExecuteLoginOutAction(UserName);

                    IsAuthenticated = false;
                    UserName = null;
                    Title = "Please Login";
                    RaisePropertyChanged(nameof(UserName));
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync($"{ex.Message}", "Sign In Failure:");
                }
                finally
                {
                    ShellActiveContentExtensions.RetsetActiveContentToLoginView();
                    IsSignIning = false;
                }
            }
        }

        private bool CanExecuteSignOutCommand()
        {
            return !IsSignIning;
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
