using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.Account.Views;
using Aksl.Toolkit.UI;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity;

namespace Aksl.Modules.Account.ViewModels
{
    public class RefreshTokenViewModel : BindableBase, IDataErrorInfo, INavigationAware
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly WebApiProvider _webApiProvider;
        private readonly Dictionary<string, string> _errors;
        #endregion

        #region Constructors
        public RefreshTokenViewModel()
        {
             StatusMessage = "RefreshToken Module Initializeing.......";

            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();

            _errors = new();

            _webApiProvider = ServiceExtensions.GetWebApiProvider();
            AccessToken= _webApiProvider.AccessToken;
            RefreshToken= _webApiProvider.RefreshToken;

            CreateRefreshTokenCommand();

            RegisterPropertyChanged();

            StatusMessage = "";
        }
        #endregion

        #region Properties
        [Required(ErrorMessage = "AccessToken不能为空")]
        public string AccessToken
        {
            get =>field;
            set => SetProperty<string>(ref field, value);
        }

        [Required(ErrorMessage = "RefreshToken不能为空")]
        public string RefreshToken
        {
            get => field;
            set => SetProperty<string>(ref field, value);
        }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;

        public string ResponseMessage
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public string StatusMessage
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
                if (sender is ResetLockoutViewModel)
                {
                    if (e.PropertyName == nameof(IsLoading) || e.PropertyName == nameof(AccessToken) || e.PropertyName == nameof(RefreshToken))
                    {
                        (RefreshTokenCommand as DelegateCommand).RaiseCanExecuteChanged();
                    }
                }
            };
        }
        #endregion

        #region RefreshTokenCommand  Command
        public ICommand RefreshTokenCommand { get; private set; }

        private void CreateRefreshTokenCommand()
        {
            RefreshTokenCommand = new DelegateCommand(async () =>
            {
                await ExecuteRefreshTokenCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecuteRefreshTokenCommand();
                return canExecute;
            });
        }

        private async Task ExecuteRefreshTokenCommandAsync()
        {
            try
            {
                if (!_webApiProvider.IsAccessTokenExpired && !_webApiProvider.IsRefreshTokenExpire)
                {
                    ResponseMessage = $"Token has not yet expired.";

                    return;
                }

                if (_webApiProvider.IsAccessTokenExpired && _webApiProvider.IsRefreshTokenExpire)
                {
                    //AccessToken = null;
                    //RefreshToken = null;

                    //var dialogResult = await _dialogViewService.ConfirmWhenAsync(message: $"accessToken {this.AccessToken} is expired,yes will re-login", title: "Refresh Token");
                    //if (dialogResult)
                    //{
                    //    ShellActiveContentExtensions.RetsetActiveContentToLoginView();
                    //}

                    ResponseMessage = $"Refresh Token has expired, user needs to re-login.";

                    return;
                }

                IsLoading = true;
                StatusMessage = "Refreshing Token.......";

                var refreshTokenResponse =await ServiceExtensions.GetLoginHandler().
                                              ExecuteRefreshTokenAction(_webApiProvider.AccessToken, _webApiProvider.RefreshToken);

                if (refreshTokenResponse.Succeeded)
                {
                    ResponseMessage = "Refresh Token Succeeded";

                    AccessToken = refreshTokenResponse.AccessToken;
                    RefreshToken = refreshTokenResponse.RefreshToken;

                    await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                }
                else
                {
                    if (refreshTokenResponse.ToString().Contains("Refresh Token has expired, user needs to re-login.") ||
                        refreshTokenResponse.ToString().Contains("Token has expired please re-login") ||
                        refreshTokenResponse.ToString().Contains("Something went wrong."))
                    {
                        _eventAggregator.GetEvent<OnAccessTokenExpiredEvent>().Publish(new() { IsExpired = true });
                    }
                    
                    ResponseMessage = $"{refreshTokenResponse.ToString()}";
                }
            }
            catch (Exception ex)
            {
               // var webApiProvider = ServiceExtensions.GetWebApiProvider();

                ServiceExtensions.GetLoginHandler().BindAccessTokenAction(null, null);

                ResponseMessage = $"{ex.Message}";

                _eventAggregator.GetEvent<OnAccessTokenExpiredEvent>().Publish(new() {IsExpired = true});

              // await _dialogViewService.AlertAsync($"{ex.Message}", "Login In Failure:");
            }
            finally
            {
                StatusMessage = "";
                IsLoading = false;
            }
        }

        private bool CanExecuteRefreshTokenCommand()
        {
            return !IsLoading && !HasErrors;
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
                    }

                    return string.Join(Environment.NewLine, validationResults.Select(r => r.ErrorMessage).ToArray());
                }
                else
                {
                    _errors.Remove(columnName);
                }

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
