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
using System.Windows.Threading;

namespace Aksl.Modules.Account.ViewModels
{
    public class RefreshTokenViewModel : BindableBase, IDataErrorInfo, INavigationAware
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly WebApiProvider _webApiProvider;
        private readonly Dictionary<string, string> _errors;

        // timer for updating AccessTokenExpireLeft
        private readonly DispatcherTimer _expireTimer;
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
            AccessTokenExpire= _webApiProvider.AccessTokenExpire ?? _webApiProvider.AccessTokenExpire.Value;
            AccessTokenExpireString = $"{_webApiProvider.AccessTokenExpire??_webApiProvider.AccessTokenExpire.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

            RefreshToken = _webApiProvider.RefreshToken;
            RefreshTokenExpire = _webApiProvider.RefreshTokenExpire ?? _webApiProvider.RefreshTokenExpire.Value;
            RefreshTokenExpireString = $"{_webApiProvider.RefreshTokenExpire ?? _webApiProvider.RefreshTokenExpire.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

            CreateRefreshTokenCommand();

            RegisterPropertyChanged();

            // create and start dispatcher timer to update AccessTokenExpireLeft every second
            _expireTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _expireTimer.Tick += (s, e) => 
            { 
                UpdateAccessTokenExpire(); 
                UpdateRefreshTokenExpire();
            };
            UpdateAccessTokenExpire();
            _expireTimer.Start();

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

        public DateTime AccessTokenExpire { get; set; }
        public bool IsAccessTokenExpired
        {
            get
            {
                return DateTime.UtcNow > AccessTokenExpire;
            }
        }

        // AccessTokenExpireLeft shows the live countdown as a formatted string
        private string _accessTokenExpireLeft;
        public string AccessTokenExpireLeft
        {
            get => _accessTokenExpireLeft;
            set => SetProperty(ref _accessTokenExpireLeft, value);
        }

        public string AccessTokenExpireString 
         {
            get => field;
            set => SetProperty<string>(ref field, value);
        }

        [Required(ErrorMessage = "RefreshToken不能为空")]
        public string RefreshToken
        {
            get => field;
            set => SetProperty<string>(ref field, value);
        }

        public DateTime RefreshTokenExpire { get; set; }
        public bool IsRefreshTokenExpire
        {
            get
            {
                return DateTime.UtcNow > RefreshTokenExpire;
            }
        }
        public string RefreshTokenExpireLeft
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public string RefreshTokenExpireString
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

        #region UpdateAccessTokenExpire Method
        private void UpdateAccessTokenExpire()
        {
            try
            {
                var remainingAccessToken = AccessTokenExpire - DateTime.UtcNow;

                if (remainingAccessToken.TotalSeconds <= 0)
                {
                    //AccessTokenExpireLeft = "00:00:00";
                    //AccessTokenExpireLeft = remainingAccessToken.ToString("hh:mm:ss");
                    AccessTokenExpireLeft = remainingAccessToken.ToString();
                }
                else
                {
                    if (remainingAccessToken.Days > 0)
                    {
                        AccessTokenExpireLeft = $"{remainingAccessToken.Days}d {remainingAccessToken:hh:mm:ss}";
                    }
                    else
                    {
                        AccessTokenExpireLeft = remainingAccessToken.ToString();
                    }
                }

                RaisePropertyChanged(nameof(IsAccessTokenExpired));
            }
            catch
            {
                AccessTokenExpireLeft = string.Empty;
            }
        }

        private void UpdateRefreshTokenExpire()
        {
            try
            {
                var remainingRefreshToken = RefreshTokenExpire - DateTime.UtcNow;

                if (remainingRefreshToken.TotalSeconds <= 0)
                {
                    //RefreshTokenExpireLeft = remainingRefreshToken.ToString("hh:mm:ss");
                    RefreshTokenExpireLeft = remainingRefreshToken.ToString();
                }
                else
                {
                    if (remainingRefreshToken.Days > 0)
                    {
                        RefreshTokenExpireLeft = $"{remainingRefreshToken.Days}d {remainingRefreshToken:hh:mm:ss}";
                    }
                    else
                    {
                        RefreshTokenExpireLeft = remainingRefreshToken.ToString();
                    }
                }

                RaisePropertyChanged(nameof(IsRefreshTokenExpire));
            }
            catch
            {
                RefreshTokenExpireLeft = string.Empty;
            }
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
                //if (!_webApiProvider.IsAccessTokenExpired && !_webApiProvider.IsRefreshTokenExpire)
                //{
                //    ResponseMessage = $"AccessToken has not yet expired.";

                //    return;
                //}

                //if (_webApiProvider.IsAccessTokenExpired && _webApiProvider.IsRefreshTokenExpire)
                //{
                //    ResponseMessage = $"Refresh Token has expired, user needs to re-login.";

                //    return;
                //}

                IsLoading = true;
                StatusMessage = "Refreshing Token.......";

                var refreshTokenResponse =await ServiceExtensions.GetLoginHandler().
                                              ExecuteRefreshTokenAction(_webApiProvider.AccessToken, _webApiProvider.RefreshToken);

                if (refreshTokenResponse.Succeeded)
                {
                    ResponseMessage = "Refresh Token Succeeded";

                    AccessToken = _webApiProvider.AccessToken;
                    AccessTokenExpire = _webApiProvider.AccessTokenExpire.Value;
                    AccessTokenExpireString = $"{_webApiProvider.AccessTokenExpire.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

                    RefreshToken = _webApiProvider.RefreshToken;
                    RefreshTokenExpire = _webApiProvider.RefreshTokenExpire.Value;
                    RefreshTokenExpireString = $"{_webApiProvider.RefreshTokenExpire.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

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
            catch (Exception ex) when (ex is System.Text.Json.JsonException)
            {
                _webApiProvider.ClearHeader();

                // var webApiProvider = ServiceExtensions.GetWebApiProvider();

                //ServiceExtensions.GetLoginHandler().BindAccessTokenAction(null, null);

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
