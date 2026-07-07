using System;
using System.Windows.Media;

using Prism.Events;
using Prism.Mvvm;

using Aksl.Infrastructure;

namespace Aksl.Modules.RadarMap.ViewModels
{
    public class RadarItemViewModel : BindableBase
    {
        #region Members
        #endregion

        #region Constructors
        public RadarItemViewModel()
        {
        }
        #endregion

        #region Properties
        private double _top=0d;
        public double Top
        {
            get => _top;
            set => SetProperty<double>(ref _top, value);
        }

        private double _left = 0d;
        public double Left
        {
            get => _left;
            set => SetProperty<double>(ref _left, value);
        }

        private double _width = 15d;
        public double Width
        {
            get => _width;
            set => SetProperty<double>(ref _width, value);
        }

        private double _height = 15d;
        public double Height
        {
            get => _height;
            set => SetProperty<double>(ref _height, value);
        }

        public Brush _color=new SolidColorBrush(Colors.Red);
        public Brush Color
        {
            get => _color;
            set => SetProperty<Brush>(ref _color, value);
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                }
            }
        }
        #endregion
    }
}
