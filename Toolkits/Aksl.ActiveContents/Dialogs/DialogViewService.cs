using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

using Prism.Services.Dialogs;

using Aksl.Dialogs.Views;

namespace Aksl.Dialogs.Services
{
    public interface IDialogViewService
    {
        Task AlertAsync(string message, string title = null, string windowCloseButtonVisibility = "Visible", double width = 650d, double height = 300d, string okText = "确 定", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null);

        Task ConfirmAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650d, double height = 300d, string okText = "确 定", string cancelText = "取 消", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null);

        Task<bool> ConfirmWhenAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650d, double height = 300d, string okText = "确 定", string cancelText = "取 消", string windowName = nameof(FixedSizeDialogWindow));

        Task<IDialogResult> ShowDialogAsync<T>(DialogParameters parameters = null);

        Task<IDialogResult> ShowDialogAsync(string contentName, IDialogParameters parameters = null);
    }

    public class DialogViewService : IDialogViewService
    {
        #region Members
        private readonly Aksl.Dialogs.Services.DialogService _dialogService;
        #endregion

        #region Constructors
        public DialogViewService(Aksl.Dialogs.Services.DialogService dialogService)
        {
            _dialogService = dialogService;
        }
        #endregion

        public Task AlertAsync(string message, string title = null, string windowCloseButtonVisibility = "Visible", double width = 650d, double height = 300d, string okText = "确 定", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null)
        {
            var parameters = new DialogParameters {{ "IsConfirm", false } ,{"Title", title }, { "WindowCloseButtonVisibility", windowCloseButtonVisibility },{ "Width", width },{ "Height", height },
                                                   { "OkText", okText },{"Message", message }};

            _dialogService.ShowDialog(dialogContentName:  nameof(Views.ConfirmView),parameters: parameters, windowName: windowName, callBack: callBack);

            //  _dialogService.Alert(message: message, title: title, width: width, height: height, okText: okText, callBack: callBack, windowName: nameof(FixedSizeDialogWindow));

            return Task.CompletedTask;
        }

        public Task ConfirmAsync(string message, string title = null, string windowCloseButtonVisibility = "Visible", double width = 650d, double height = 300d, string okText = "确 定", string cancelText = "取 消", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null)
        {
            var parameters = new DialogParameters {{"IsConfirm",true },  {"Title",  title },{ "WindowCloseButtonVisibility", windowCloseButtonVisibility}, { "Width", width }, { "Height", height },
                                                  {"OkText",  okText },{"CancelText",cancelText  },{ "Message", message } };

            _dialogService.ShowDialog(dialogContentName: nameof(Views.ConfirmView), parameters: parameters, windowName: windowName, callBack: callBack);

            // _dialogService.Confirm(message: message, title: title, width: width, height: height, okText: okText, cancelText: cancelText, callBack: callBack, windowName: nameof(FixedSizeDialogWindow));

            return Task.CompletedTask;
        }

        public Task<bool> ConfirmWhenAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650, double height = 300, string okText = "确 定", string cancelText = "取 消", string windowName = "FixedSizeDialogWindow")
        {
            var parameters = new DialogParameters {{ "IsConfirm",true },  {"Title",  title },{ "WindowCloseButtonVisibility", windowCloseButtonVisibility}, { "Width", width }, { "Height", height },
                                                    { "OkText",  okText },{"CancelText",cancelText  },  { "Message", message } };

            bool isOk = false;
            _dialogService.ShowDialog(dialogContentName: nameof(Views.ConfirmView), parameters: parameters, windowName: windowName, callBack: (result) =>
            {
                isOk = result.Result == ButtonResult.OK;
            });

            return Task.FromResult(isOk);
        }

        public Task<IDialogResult> ShowDialogAsync<T>(DialogParameters parameters = null)
        {
            var typeName = typeof(T).Name;

            return ShowDialogAsync(contentName: typeName, parameters: parameters);
        }

        public Task<IDialogResult> ShowDialogAsync(string contentName, IDialogParameters parameters = null)
        {
            IDialogResult dialogResult = null;

            _dialogService.ShowDialog(dialogContentName: contentName, parameters: parameters, callBack: (result) =>
            {
                dialogResult = result;
            });

            return Task.FromResult(dialogResult);
        }
    }

    public static class DialogExtensions
    {
        public static async Task AlertWhenAsync(this IDialogViewService dialogViewService, string message, string title)
        {
            if (!string.IsNullOrEmpty(message) || !string.IsNullOrWhiteSpace(message))
            {
                await dialogViewService.AlertAsync(message, title: title);
            }
        }

        public static async Task AlertWhenAsync(this IDialogViewService dialogViewService, string message, string title, string okText = "Ok")
        {
            if (!string.IsNullOrEmpty(message) || !string.IsNullOrWhiteSpace(message))
            {
                await dialogViewService.AlertAsync(message, title: title, okText: okText);
            }
        }
    }
}
