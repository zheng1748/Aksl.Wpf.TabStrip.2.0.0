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
    public class RadarLineSize : BindableBase
    {
        #region Properties
        private System.Windows.Point _start;
        public System.Windows.Point Start
        {
            get => _start;
            set => SetProperty<System.Windows.Point>(ref _start, value);
        }

        private System.Windows.Point _end;
        public System.Windows.Point End
        {
            get => _end;
            set => SetProperty<System.Windows.Point>(ref _end, value);
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

        #region CreateInstanceCore Method
        //protected override Freezable CreateInstanceCore()
        //{
        //    return (Freezable)Activator.CreateInstance(this.GetType());
        //}
        #endregion
    }
}
