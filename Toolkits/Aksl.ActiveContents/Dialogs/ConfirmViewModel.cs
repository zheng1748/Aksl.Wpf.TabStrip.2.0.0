using Aksl.Toolkit.Controls;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;

namespace Aksl.Dialogs.ViewModels
{
    public class ConfirmViewModel : DialogAware
    {
        #region Constructors
        public ConfirmViewModel() : base()
        {
        }
        #endregion

        #region Properties
        private bool _isConfirm;
        public bool IsConfirm
        {
            get => _isConfirm;
            set => SetProperty<bool>(ref _isConfirm, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        #endregion

        #region IDialogAware
        public override event Action<IDialogResult> RequestClose;

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            Title = parameters.GetValue<string>("Title") ?? "Notification";
            WindowCloseButtonVisibility = GetWindowCloseButtonVisibility(parameters.GetValue<string>("WindowCloseButtonVisibility"), Visibility.Visible);
            Width = parameters.GetValue<double?>("Width") ?? 650d;
            Height = parameters.GetValue<double?>("Height") ?? 300d;
            OkText = parameters.GetValue<string>("OkText") ?? "OK";
            OkIconKind = GetPackIconKind(parameters.GetValue<string>("OkIconKind"), PackIconKind.None);
            OkToolTip = parameters.GetValue<string>("OkToolTip") ?? "OK";
            CancelText = parameters.GetValue<string>("CancelText") ?? "Cancel";
          
            IsConfirm = parameters.GetValue<bool?>("IsConfirm") ?? true;
            Message = parameters.GetValue<string>("Message");
        }
        #endregion

        #region Ok Command
        protected override async Task ExecuteOkCommandAsync()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }
        #endregion

        #region Cancel Command
        protected override async Task ExecuteCancelCommandAsync()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }
        #endregion
    }
}
