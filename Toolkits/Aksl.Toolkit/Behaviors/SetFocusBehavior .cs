using Microsoft.Xaml.Behaviors;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;

//JulMar.Windows.Actions
namespace Aksl.Xaml.Behaviors
{
    public class SetFocusBehavior : Behavior<FrameworkElement>
    { 
        #region Delay Property
        /// <summary>
        /// Delay in milliseconds before setting focus. 0 means immediate.
        /// </summary>
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register(nameof(Delay), typeof(int), typeof(SetFocusBehavior), new PropertyMetadata(defaultValue:0));

        public int Delay
        {
            get => (int)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }
        #endregion

        #region OnAttached/OnDetaching Method
        protected override void OnAttached()
        {
            base.OnAttached();

            var element = AssociatedObject;
            if (element is not null)
            {
                element.IsVisibleChanged += OnIsVisibleChanged;
            }
        }

        protected override void OnDetaching()
        {
            var element = AssociatedObject;
            if (element is not null)
            {
                element.IsVisibleChanged -= OnIsVisibleChanged;
            }

            base.OnDetaching();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                _ = element.Dispatcher.InvokeAsync(async () =>
                {
                    await Task.Delay(Delay).ConfigureAwait(false);

                    element.Dispatcher.Invoke(() =>
                    {
                        if (!element.Focus())
                        {
                            var fs = FocusManager.GetFocusScope(element);
                            FocusManager.SetFocusedElement(fs, element);

                            // element.Focus();
                        }
                    });
                }, DispatcherPriority.Input);
            }
        }
        #endregion
    }
}

