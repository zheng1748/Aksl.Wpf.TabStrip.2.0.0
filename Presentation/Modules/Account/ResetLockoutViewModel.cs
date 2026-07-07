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

namespace Aksl.Modules.Account.ViewModels
{
    public class ResetLockoutViewModel : BindableBase, IDataErrorInfo, INavigationAware
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly Dictionary<string, string> _errors;
        #endregion

        #region Constructors
        public ResetLockoutViewModel()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();

            _errors = new();

            CreateResetLockoutCommand();

            RegisterPropertyChanged();
        }
        #endregion

        #region Properties
        [Required(ErrorMessage = "用户名不能为空")]
        [RegularExpression("^[a-zA-Z]{1}([a-zA-Z0-9]){7,15}$", ErrorMessage = "用户名必须是8到16个字母或者数字,且以字母开头.")]
        public string UserName
        {
            get =>field;
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
                    if (e.PropertyName == nameof(IsLoading) || e.PropertyName == nameof(UserName))
                    {
                        (ResetLockoutCommand as DelegateCommand).RaiseCanExecuteChanged();
                    }
                }
            };
        }
        #endregion

        #region ResetLockoutCommand  Command
        public ICommand ResetLockoutCommand { get; private set; }

        private void CreateResetLockoutCommand()
        {
            ResetLockoutCommand = new DelegateCommand(async () =>
            {
                await ExecuteResetLockoutCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecuteResetLockoutCommand();
                return canExecute;
            });
        }

        private async Task ExecuteResetLockoutCommandAsync()
        {
            IsLoading = true;

            try
            {
                StatusMessage = "Reset Lockout.......";

                var resetLockoutResponse = await ServiceExtensions.GetLoginHandler().ExecuteResetLockoutAction(UserName);
                if (resetLockoutResponse.Succeeded)
                {
                    ResponseMessage = "Reset Lockout Succeeded";

                     await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                }
                else
                {
                    ResponseMessage = $"{resetLockoutResponse.ToString()}";
                }
            }
            catch (Exception ex)
            {
               // ServiceExtensions.GetLoginHandler().BindAccessTokenAction(null, null);

                ResponseMessage = $"{ex.Message}";

                _eventAggregator.GetEvent<OnAccessTokenExpiredEvent>().Publish(new() {IsExpired = true});

              // await _dialogViewService.AlertAsync($"{ex.Message}", "Login In Failure:");
            }

            IsLoading = false;
        }

        private bool CanExecuteResetLockoutCommand()
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
