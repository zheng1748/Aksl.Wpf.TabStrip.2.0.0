using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using Prism.Mvvm;

namespace Aksl.Modules.RadarMap
{
    public class RadarSize :  BindableBase
    {
        #region Constructors
        public RadarSize()
        {

        }
        #endregion

        #region Properties
        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty<double>(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => SetProperty<double>(ref _height, value);
        }

        private Brush _color;
        public Brush Color
        {
            get => _color;
            set => SetProperty<Brush>(ref _color, value);
        }

        private double _thickness;
        public double Thickness
        {
            get => _thickness;
            set => SetProperty<double>(ref _thickness, value);
        }
        #endregion
    }
}
