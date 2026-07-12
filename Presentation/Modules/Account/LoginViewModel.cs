using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Toolkit.UI;

using Aksl.Infrastructure.Events;
using Aksl.Modules.Account.Views;
using Aksl.Toolkit;

namespace Aksl.Modules.Account.ViewModels
{
    public class LoginViewModel : BindableBase, IDataErrorInfo, INavigationAware
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly Dictionary<string, string> _errors;
        #endregion

        #region Constructors
        public LoginViewModel()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();

            _errors = new();

            CreateLoginCommand();
            CreateCloseCommand();

            RegisterPropertyChanged();
        }
        #endregion

        #region Properties
        public string Title { get; private set; } = "Sign In";

        [Required(ErrorMessage = "用户名不能为空")]
        [RegularExpression("^[a-zA-Z]{1}([a-zA-Z0-9]){7,15}$", ErrorMessage = "用户名必须是8到16个字母或者数字,且以字母开头.")]
        public string UserName
        {
            get => field;
            set => SetProperty<string>(ref field, value);
        }="zhengming";

        [Required(ErrorMessage = "密码不能为空")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[$@$!%#?&])[a-zA-Z\d$@$!%#?&]{8,}$", ErrorMessage = "密码至少8个字符,必须包含一个字母,一个数字,一个特殊字符.")]
        public string Password
        {
            get;
            set => SetProperty<string>(ref field, value);
        } = "zheng@0616";

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;

        public string StatusMessage
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public bool IsSuccessful
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public bool HasErrors => _errors.Count > 0;
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            this.PropertyChanged += (sender, e) =>
            {
                if (sender is LoginViewModel lvm)
                {
                    if (e.PropertyName == nameof(IsLoading) || e.PropertyName == nameof(UserName) || e.PropertyName == nameof(Password))
                    {
                        (LoginCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    }
                }
            };
        }
        #endregion

        #region Login Command
        public ICommand LoginCommand { get; private set; }

        private void CreateLoginCommand()
        {
            LoginCommand = new DelegateCommand(async () =>
            {
                await ExecuteLoginCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecuteLoginCommand();
                return canExecute;
            });
        }

        private async Task ExecuteLoginCommandAsync()
        {
            IsLoading = true;

            try
            {
                StatusMessage = "Logining.......";

                if (ServiceExtensions.GetWebApiProvider().IsAccessTokenExpired)
                {
                    ServiceExtensions.GetLoginHandler().BindAccessTokenAction(null, null);
                }

                var loginResponse = await ServiceExtensions.GetLoginHandler().ExecuteLoginAction(UserName, Password);
                if (loginResponse.Succeeded)
                {
                    //var loginResponse2 = await ServiceExtensions.GetLoginHandler().ExecuteLoginAction(UserName, Password);

                    IsSuccessful = true;

                    _eventAggregator.GetEvent<OnSignInedEvent>().Publish(new OnSignInedEvent { UserName = this.UserName, IsSuccessful = true });

                    await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                }
                else
                {
                    await _dialogViewService.AlertAsync($"{loginResponse.ToString()}", "Login In Failure:");
                }
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync($"{ex.Message}", "Login In Failure:");
            }

            StatusMessage = null;
            IsLoading = false;
        }

        private bool CanExecuteLoginCommand()
        {
            return !IsLoading && !HasErrors;
        }
        #endregion

        #region Close Method
        public async void ExecuteClose(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                VisualTreeFinder visualTreeFinder = new();
                var shellWindow = visualTreeFinder.FindVisualParent<Window>(button);
                var parents = visualTreeFinder.FindVisualParents<FrameworkElement>(button);
                var loginView = parents.FirstOrDefault(e => e is UserControl) as LoginView;
                var childs = visualTreeFinder.FindVisualChilds<FrameworkElement>(loginView);

                IsLoading = true;

                try
                {
                    StatusMessage = "Closing.......";

                    //RetsetShellActiveItem();

                    _eventAggregator.GetEvent<OnSignInedEvent>().Publish(new OnSignInedEvent { UserName = "", IsSuccessful = false });

                    await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync($"{ex.Message}", "Close Failure:");
                }

                IsLoading = false;
            }
        }
        #endregion

        #region Close Command
        public ICommand CloseCommand { get; private set; }

        private void CreateCloseCommand()
        {
            CloseCommand = new DelegateCommand(async () =>
            {
                await ExecuteCloseCommandAsync();
            },
            () =>
            {
                return true;
            });
        }

        private async Task ExecuteCloseCommandAsync()
        {
            IsLoading = true;

            try
            {
                IsSuccessful = false;

                StatusMessage = "Closing.......";

                //  RemoveLoginView();

                ShellActiveContentExtensions.RetsetActiveContentToLoginView();

                _eventAggregator.GetEvent<OnSignInedEvent>().Publish(new OnSignInedEvent { UserName = "", IsSuccessful = false });

                // await Task.Delay(TimeSpan.FromMilliseconds(1000)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync($"Please userName and  password", "Login Failure:");
            }

            IsLoading = false;
        }
        #endregion

        #region Retset Shell ActiveItem Method
        public void RetsetShellActiveItem()
        {
            var shellContentActiveContentViewModel = PrismIocExtensions.GetUnityContainer().
                                              Resolve<ActiveContents.ViewModels.RandomActiveContentViewModel>(name: ActiveContentNames.ShellContent);
            //shellContentActiveContentViewModel.SetSelectedItemByName(ActiveContentNames.HamburgerMenuSideBarName);
            shellContentActiveContentViewModel.RetsetContentItemByName(nameof(LoginView));
        }
        #endregion

        #region IDataErrorInfo Interface
        public string this[string columnName]
        {
            get
            {
                ValidationContext validationContext = new(this, null, null)
                {
                    MemberName = columnName
                };
                List<System.ComponentModel.DataAnnotations.ValidationResult> validationResults = new();
                var isValidate = Validator.TryValidateProperty(this.GetType().GetProperty(columnName).GetValue(this, null), validationContext, validationResults);
                if (!isValidate && validationResults.Any())
                {
                    if (!_errors.ContainsKey(columnName))
                    {
                        _errors.Add(columnName, "");
                        // return _errors[columnName];
                    }

                    return string.Join(Environment.NewLine, validationResults.Select(r => r.ErrorMessage).ToArray());
                }
                else
                {
                    _errors.Remove(columnName);
                }

                //RaisePropertyChanged(nameof(HasErrors));

                return null;
            }
            set
            {
                _errors[columnName] = value;
            }
        }

        public string Error => null;
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
