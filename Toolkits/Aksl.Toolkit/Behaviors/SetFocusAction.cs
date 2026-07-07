using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

//JulMar.Windows.Actions
namespace Aksl.Xaml.Behaviors
{
    public class SetFocusAction : Microsoft.Xaml.Behaviors.TriggerAction<FrameworkElement>
    {
        #region Target Property
        /// <summary>
        /// Dependency Property backing the Target property.
        /// </summary>
        public static readonly DependencyProperty TargetProperty =
                        DependencyProperty.Register(nameof(Target), typeof(FrameworkElement), typeof(SetFocusAction), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// This property allows you to set the focus target independent of where this
        /// action is applied - so you can apply the trigger/action to the Window and then
        /// push focus to a child element as an example.
        /// </summary>
        public FrameworkElement Target
        {
            get => (FrameworkElement)base.GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }
        #endregion

        #region UseKeyboardFocus Property
        public static readonly DependencyProperty UseKeyboardFocusProperty =
                        DependencyProperty.Register(nameof(UseKeyboardFocus), typeof(bool), typeof(SetFocusAction), new PropertyMetadata(true));

        public bool UseKeyboardFocus
        {
            get => (bool)GetValue(UseKeyboardFocusProperty);
            set => SetValue(UseKeyboardFocusProperty, value);
        }
        #endregion

        #region SelectAll Property
        public static readonly DependencyProperty SelectAllProperty =
                        DependencyProperty.Register(nameof(SelectAll), typeof(bool), typeof(SetFocusAction), new PropertyMetadata(false));

        public bool SelectAll
        {
            get => (bool)GetValue(SelectAllProperty);
            set => SetValue(SelectAllProperty, value);
        }
        #endregion

        #region Delay Property
        /// <summary>
        /// Delay in milliseconds before setting focus. 0 means immediate.
        /// </summary>
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register(nameof(Delay), typeof(int), typeof(SetFocusAction), new PropertyMetadata(0));

        public int Delay
        {
            get => (int)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }
        #endregion

        #region Invoke Method
        protected override void Invoke(object parameter)
        {
            var element = Target ?? AssociatedObject;

            if (element is null)
            {
                return;
            }

            if (Delay <= 0)
            {
                _ = element.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        element.Dispatcher.Invoke(() => SetFocusInternal(element));
                    }
                    catch
                    {
                    }
                }, DispatcherPriority.Input);

                return;
            }

            // schedule after delay without blocking UI thread
            _ = element.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await Task.Delay(Delay).ConfigureAwait(false);
                }
                catch {}

                // marshal back to UI thread
                element.Dispatcher.Invoke(() => SetFocusInternal(element));
            }, DispatcherPriority.Input);
        }

        private void SetFocusInternal(FrameworkElement element)
        {
            try
            {
                // If unable to directly set focus, then attempt to set *logical* focus
                // to our element so that when/if focus returns to this focus scope we will have focus.
                if (!element.Focus())
                {
                    var fs = FocusManager.GetFocusScope(element);
                    FocusManager.SetFocusedElement(fs, element);
                }

                if (UseKeyboardFocus)
                {
                    Keyboard.Focus(element);
                }

                if (SelectAll && element is TextBox tb &&  (string.IsNullOrEmpty (tb.Text)) && tb.Text.Length>0)
                {
                    tb.SelectAll();
                }
            }
            catch
            {
                // swallow exceptions to avoid breaking triggers; callers can add logging if desired
            }
        #endregion
        }
    }
}
