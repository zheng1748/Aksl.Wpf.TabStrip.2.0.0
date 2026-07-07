using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aksl.ActiveContents.Views
{
    public partial class SequenceActiveContentView : UserControl
    {
        public SequenceActiveContentView()
        {
            InitializeComponent();
        }

        #region Background Property
        public static readonly DependencyProperty ActiveBackgroundProperty =
            DependencyProperty.Register(nameof(ActiveBackground), typeof(System.Windows.Media.Brush), typeof(SequenceActiveContentView), new PropertyMetadata(defaultValue: System.Windows.Media.Brushes.WhiteSmoke));

        public System.Windows.Media.Brush ActiveBackground
        {
            get => (System.Windows.Media.Brush)GetValue(ActiveBackgroundProperty);
            set => SetValue(ActiveBackgroundProperty, value);
        }
        #endregion
    }
}
