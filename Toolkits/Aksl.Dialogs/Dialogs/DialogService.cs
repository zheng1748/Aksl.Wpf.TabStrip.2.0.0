using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

using Prism;
using Prism.Common;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Views;

namespace Aksl.Dialogs.Services
{
    public interface IDialogService
    {
        void Show<T>(IDialogParameters parameters = null, string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null, bool isModal = true);
        void Show(string contentName, IDialogParameters parameters = null, string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null, bool isModal = true);

        void Show(FrameworkElement dialogContent, IDialogParameters parameters = null, string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null, bool isModal = true);
    }

    public class DialogService : Aksl.Dialogs.Services.IDialogService
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public DialogService()
        {
            _container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
        }
        #endregion

        #region Show Methods
        public void Show<T>(IDialogParameters parameters = null, string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null, bool isModal = true)
        {
            //var typeName = typeof(T).Name;
            //var containerExtension = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IContainerExtension>();
            //var content = containerExtension.Resolve<object>(typeName); 

            var content = _container.Resolve<T>();
            if (!(content is FrameworkElement dialogContent))
            {
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");
            }

            ShowDialogInternal(dialogContent: dialogContent, parameters: parameters, windowName: windowName, callBack: callBack, isModal: isModal);
        }

        public void Show(string contentName, IDialogParameters parameters = null, string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null, bool isModal = true)
        {
            var content = _container.Resolve<object>(contentName);
            if (!(content is FrameworkElement dialogContent))
            {
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");
            }

            ShowDialogInternal(dialogContent: dialogContent, parameters: parameters, windowName: windowName, callBack: callBack, isModal: isModal);
        }

        public void Show(FrameworkElement dialogContent, IDialogParameters parameters = null, string windowName = "FixedSizeDialogWindow", Action<IDialogResult> callBack = null, bool isModal = true)
        {
            ShowDialogInternal(dialogContent: dialogContent, parameters: parameters, windowName: windowName, callBack: callBack, isModal: isModal);
        }

        private void ShowDialogInternal(FrameworkElement dialogContent, IDialogParameters parameters, string windowName, Action<IDialogResult> callBack, bool isModal)
        {
            if (parameters is null)
            {
                parameters = new DialogParameters();
            }

            IDialogWindow dialogWindow = CreateDialogWindow(windowName);
            ConfigureDialogWindowEvents(dialogWindow, callBack);
            ConfigureDialogWindowContent(dialogContent, dialogWindow, parameters);

            ShowDialogWindow(dialogWindow, isModal);
        }

        protected virtual void ShowDialogWindow(IDialogWindow dialogWindow, bool isModal)
        {
            if (isModal)
            {
                dialogWindow.ShowDialog();
            }
            else
            {
                dialogWindow.Show();
            }
        }

        protected virtual IDialogWindow CreateDialogWindow(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return _container.Resolve<IDialogWindow>();
            }
            else
            {
                return _container.Resolve<IDialogWindow>(name);
            }
        }

        protected virtual void ConfigureDialogWindowContent(FrameworkElement dialogContent, IDialogWindow window, IDialogParameters parameters)
        {
            MvvmHelpers.AutowireViewModel(dialogContent);

            if (dialogContent.DataContext is not IDialogAware viewModel)
            {
                throw new NullReferenceException("A dialog's ViewModel must implement the IDialogAware interface");
            }

            ConfigureDialogWindowProperties(window, dialogContent, viewModel);

            MvvmHelpers.ViewAndViewModelAction<IDialogAware>(viewModel, d => d.OnDialogOpened(parameters));
        }

        protected virtual void ConfigureDialogWindowEvents(IDialogWindow dialogWindow, Action<IDialogResult> callBack)
        {
            Action<IDialogResult> requestCloseHandler = null;
            requestCloseHandler = (o) =>
            {
                dialogWindow.Result = o;
                dialogWindow.Close();
            };

            RoutedEventHandler loadedHandler = null;
            loadedHandler = (o, e) =>
            {
                dialogWindow.Loaded -= loadedHandler;
                dialogWindow.GetDialogViewModel().RequestClose += requestCloseHandler;
            };
            dialogWindow.Loaded += loadedHandler;

            CancelEventHandler closingHandler = null;
            closingHandler = (o, e) =>
            {
                if (!dialogWindow.GetDialogViewModel().CanCloseDialog())
                    e.Cancel = true;
            };
            dialogWindow.Closing += closingHandler;

            EventHandler closedHandler = null;
            closedHandler = (o, e) =>
            {
                dialogWindow.Closed -= closedHandler;
                dialogWindow.Closing -= closingHandler;
                dialogWindow.GetDialogViewModel().RequestClose -= requestCloseHandler;

                dialogWindow.GetDialogViewModel().OnDialogClosed();

                if (dialogWindow.Result == null)
                    dialogWindow.Result = new DialogResult();

                callBack?.Invoke(dialogWindow.Result);

                dialogWindow.DataContext = null;
                dialogWindow.Content = null;
            };
            dialogWindow.Closed += closedHandler;
        }

        protected virtual void ConfigureDialogWindowProperties(IDialogWindow window, FrameworkElement dialogContent, IDialogAware viewModel)
        {
            var windowStyle = Dialog.GetWindowStyle(dialogContent);
            if (windowStyle is not null)
            {
                window.Style = windowStyle;
            }

            window.Content = dialogContent;
            window.DataContext = viewModel; //we want the host window and the dialog to share the same data context

            window.Owner ??= Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        }
        #endregion
    }

    internal static class IDialogWindowExtensions
    {
        /// <summary>
        /// Get the <see cref="IDialogAware"/> ViewModel from a <see cref="IDialogWindow"/>.
        /// </summary>
        /// <param name="dialogWindow"><see cref="IDialogWindow"/> to get ViewModel from.</param>
        /// <returns>ViewModel as a <see cref="IDialogAware"/>.</returns>
        internal static IDialogAware GetDialogViewModel(this IDialogWindow dialogWindow)
        {
            return (IDialogAware)dialogWindow.DataContext;
        }
    }
}
