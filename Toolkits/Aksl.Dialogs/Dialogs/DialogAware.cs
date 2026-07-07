using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using Aksl.Toolkit.Controls;

namespace Aksl.Dialogs
{
    public class DialogAware : BindableBase, IDialogAware
    {
        #region Constructors
        public DialogAware()
        {
            CreateOkCommand();
            CreateCancelCommand();
        }
        #endregion

        #region Properties
        private string _title = "Notification";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private double _width = 300d;
        public double Width
        {
            get => _width;
            set => SetProperty<double>(ref _width, value);
        }

        private double _height = 150d;
        public double Height
        {
            get => _height;
            set => SetProperty<double>(ref _height, value);
        }

        private Visibility _closeWindowCloseButtonVisibility = Visibility.Visible;
        public Visibility WindowCloseButtonVisibility
        {
            get => _closeWindowCloseButtonVisibility;
            set => SetProperty<Visibility>(ref _closeWindowCloseButtonVisibility, value);
        }

        private string _okText = "OK";
        public string OkText
        {
            get => _okText;
            set => SetProperty(ref _okText, value);
        }

        private PackIconKind _okIconKind;
        public PackIconKind OkIconKind
        {
            get => _okIconKind;
            set => SetProperty<PackIconKind>(ref _okIconKind, value);
        }

        private string _okToolTip = "Please Login";
        public string OkToolTip
        {
            get => _okToolTip;
            set => SetProperty(ref _okToolTip, value);
        }

        private string _cancelText = "Cancel";
        public string CancelText
        {
            get => _cancelText;
            set => SetProperty(ref _cancelText, value);
        }
        #endregion

        #region IDialogAware
        public virtual event Action<IDialogResult> RequestClose;

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {

        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            //Title = parameters.GetValue<string>("Title") ?? "登陆";
            //WindowCloseButtonVisibility = GetWindowCloseButtonVisibility(parameters.GetValue<string>("WindowCloseButtonVisibility"));
            //Width = GetDoubleValue(parameters.GetValue<string>("Width"), 650d);
            //Height = GetDoubleValue(parameters.GetValue<string>("Height"), 350d);
            //OkText = parameters.GetValue<string>("OkText") ?? "确定";
            //CancelText = parameters.GetValue<string>("CancelText") ?? "Cancel";
            //OkIconKind = GetPackIconKind(parameters.GetValue<string>("OkIconKind"));
            //OkToolTip = parameters.GetValue<string>("OkToolTip") ?? "登陆";
        }

        protected PackIconKind GetPackIconKind(string value, PackIconKind defaulValue= PackIconKind.No)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaulValue;
            }

          // PackIconKind kind = PackIconKind.No;

            _ = Enum.TryParse(value, out defaulValue);

            return defaulValue;
        }

        protected Visibility GetWindowCloseButtonVisibility(string value, Visibility defaulValue = Visibility.Visible)
        {
          //  Visibility defaulValue = Visibility.Visible;

            try
            {
                _ = Enum.TryParse<Visibility>(value, out defaulValue);
            }
            catch
            {
            }

            return defaulValue;
        }

        protected double GetDoubleValue(string value, double defaulValue)
        {
            try
            {
                return double.Parse(value);
            }
            catch
            {
                return defaulValue;
            }
        }
        #endregion

        #region Ok Command
        public ICommand OkCommand { get; set; }

        protected void CreateOkCommand()
        {
            OkCommand = new DelegateCommand(async () =>
            {
                await ExecuteOkCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecuteOkCommand();
                return canExecute;
            });
        }

        protected virtual async Task ExecuteOkCommandAsync()
        {
        }

        protected virtual bool CanExecuteOkCommand()
        {
            return true;
        }
        #endregion

        #region Cancel Command
        public ICommand CancelCommand { get; set; }

        protected void CreateCancelCommand()
        {
            CancelCommand = new DelegateCommand(async () =>
            {
                await ExecuteCancelCommandAsync();
            },
            () =>
            {
                return true;
            });
        }

        protected virtual async Task ExecuteCancelCommandAsync()
        {
        }
        #endregion
    }
}
