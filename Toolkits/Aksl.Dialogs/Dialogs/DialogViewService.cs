using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

using Prism.Services.Dialogs;

using Aksl.Dialogs.Views;

namespace Aksl.Dialogs.Services;

public interface IDialogViewService
{
    Task AlertAsync(string message, string title = null, string windowCloseButtonVisibility = "Visible", double width = 650d, double height = 300d, string okText = "确 定", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null);

    Task ConfirmAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650d, double height = 300d, string okText = "确 定", string cancelText = "取 消", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null);

    Task<bool> ConfirmWhenAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650d, double height = 300d, string okText = "确 定", string cancelText = "取 消", string windowName = nameof(FixedSizeDialogWindow));

    Task<IDialogResult> ShowAsync<T>(DialogParameters parameters = null, bool isModal = true, CancellationToken cancellationToken = default);
    Task<IDialogResult> ShowAsync(string contentName, IDialogParameters parameters = null, bool isModal = true, CancellationToken cancellationToken = default);
    Task<IDialogResult> ShowAsync(FrameworkElement dialogContent, IDialogParameters parameters = null, bool isModal = true, CancellationToken cancellationToken = default);

    Task<IDialogResult> ShowDialogAsync<T>(DialogParameters parameters = null, CancellationToken cancellationToken = default);
    Task<IDialogResult> ShowDialogAsync(string contentName, IDialogParameters parameters = null, CancellationToken cancellationToken = default);
    Task<IDialogResult> ShowDialogAsync(FrameworkElement dialogContent, DialogParameters parameters = null, CancellationToken cancellationToken = default);
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
        var parameters = new DialogParameters {{"IsConfirm", false} ,{"Title",title }, {"WindowCloseButtonVisibility", windowCloseButtonVisibility},{"Width", width},{"Height", height},
                                               {"OkText", okText },{"Message",message }};

        _dialogService.Show(contentName: nameof(Views.ConfirmView), parameters: parameters, windowName: windowName, callBack: callBack, isModal: true);

        return Task.CompletedTask;
    }

    public Task ConfirmAsync(string message, string title = null, string windowCloseButtonVisibility = "Visible", double width = 650d, double height = 300d, string okText = "确 定", string cancelText = "取 消", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null)
    {
        var parameters = new DialogParameters {{"IsConfirm",true},{"Title",title },{"WindowCloseButtonVisibility",windowCloseButtonVisibility},{"Width", width }, {"Height", height },
                                               {"OkText",okText },{"CancelText",cancelText},{"Message", message } };

        _dialogService.Show(contentName: nameof(Views.ConfirmView), parameters: parameters, windowName: windowName, callBack: callBack, isModal: true);

        return Task.CompletedTask;
    }

    public Task<bool> ConfirmWhenAsync(string message, string title = null, string windowCloseButtonVisibility = "Visible", double width = 650, double height = 300, string okText = "确 定", string cancelText = "取 消", string windowName = "FixedSizeDialogWindow")
    {
        var parameters = new DialogParameters {{ "IsConfirm",true },  {"Title",  title },{ "WindowCloseButtonVisibility", windowCloseButtonVisibility}, { "Width", width }, { "Height", height },
                                               { "OkText",  okText },{"CancelText",cancelText  },{"Message", message } };

        bool isOk = false;
        _dialogService.Show(contentName: nameof(Views.ConfirmView), parameters: parameters, windowName: windowName, callBack: (result) =>
        {
            isOk = result.Result == ButtonResult.OK;
        }, isModal: true);

        return Task.FromResult(isOk);
    }

    public Task<IDialogResult> ShowAsync<T>(DialogParameters parameters = null, bool isModal = true, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<IDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        CancellationTokenRegistration? ctr = null;
        if (cancellationToken.CanBeCanceled)
        {
            ctr = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
        }

        try
        {
            _dialogService.Show<T>(parameters: parameters, callBack: (result) =>
            {
                tcs.TrySetResult(result);
            }, isModal: isModal);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        // Ensure cancellation registration is disposed when the task completes
        if (ctr.HasValue)
        {
            tcs.Task.ContinueWith(_ => ctr?.Dispose(), TaskScheduler.Default);
        }

        return tcs.Task;
    }

    public Task<IDialogResult> ShowAsync(string contentName, IDialogParameters parameters = null, bool isModal = true, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<IDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        CancellationTokenRegistration? ctr = null;
        if (cancellationToken.CanBeCanceled)
        {
            ctr = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
        }

        try
        {
            _dialogService.Show(contentName: contentName, parameters: parameters, callBack: (result) =>
            {
                tcs.TrySetResult(result);
            }, isModal: isModal);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        if (ctr.HasValue)
        {
            tcs.Task.ContinueWith(_ => ctr?.Dispose(), TaskScheduler.Default);
        }

        return tcs.Task;
    }

    public Task<IDialogResult> ShowAsync(FrameworkElement dialogContent, IDialogParameters parameters = null, bool isModal = true, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<IDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        CancellationTokenRegistration? ctr = null;
        if (cancellationToken.CanBeCanceled)
        {
            ctr = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
        }

        try
        {
            _dialogService.Show(dialogContent: dialogContent, parameters: parameters, callBack: (result) =>
            {
                tcs.TrySetResult(result);
            }, isModal: isModal);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        if (ctr.HasValue)
        {
            tcs.Task.ContinueWith(_ => ctr?.Dispose(), TaskScheduler.Default);
        }

        return tcs.Task;
    }

    public async Task<IDialogResult> ShowDialogAsync<T>(DialogParameters parameters = null, CancellationToken cancellationToken = default)
    {
        IDialogResult dialogResult = await ShowAsync<T>(parameters: parameters, isModal: true, cancellationToken: cancellationToken);

        return dialogResult;
    }

    public async Task<IDialogResult> ShowDialogAsync(string contentName, IDialogParameters parameters = null, CancellationToken cancellationToken = default)
    {
        IDialogResult dialogResult = await ShowAsync(contentName: contentName, parameters: parameters, isModal: true, cancellationToken: cancellationToken);

        return dialogResult;
    }

    public async Task<IDialogResult> ShowDialogAsync(FrameworkElement dialogContent, DialogParameters parameters = null, CancellationToken cancellationToken = default)
    {
        IDialogResult dialogResult = await ShowAsync(dialogContent: dialogContent, parameters: parameters, isModal: true, cancellationToken: cancellationToken);

        return dialogResult;
    }
}

//public static class DialogExtensions
//{
//    public static async Task AlertWhenAsync(this IDialogViewService dialogViewService, string message, string title)
//    {
//        if (!string.IsNullOrEmpty(message) || !string.IsNullOrWhiteSpace(message))
//        {
//            await dialogViewService.AlertAsync(message, title: title);
//        }
//    }

//    public static async Task AlertWhenAsync(this IDialogViewService dialogViewService, string message, string title, string okText = "Ok")
//    {
//        if (!string.IsNullOrEmpty(message) || !string.IsNullOrWhiteSpace(message))
//        {
//            await dialogViewService.AlertAsync(message, title: title, okText: okText);
//        }
//    }
//}

