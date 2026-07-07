using Aksl.Modules.RadarMap.ViewModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aksl.Modules.RadarMap.Views
{
    public partial class RadarView : UserControl
    {
        #region Constructors
        public RadarView()
        {
            // CircelColor = new ();
            //// RadarCircles = new ObservableCollection<RadarSize>();
            //RadarLine = new ObservableCollection<RadarLineSize>();
            //RadarItems = new ObservableCollection<RadarItemViewModel>();

            SetValue(CircelColorsProperty, new ObservableCollection<Brush>());
            SetValue(RadarCirclesProperty, new ObservableCollection<RadarSize>());
            SetValue(RadarLinesProperty, new ObservableCollection<RadarLineSize>());
            SetValue(RadarItemsProperty, new ObservableCollection<RadarItemViewModel>());

            CircelColors.Add(new SolidColorBrush(Colors.Red));

            InitializeComponent();
        }
        #endregion

        #region RadarItems Property
        public ObservableCollection<RadarItemViewModel> RadarItems
        {
            get => (ObservableCollection<RadarItemViewModel>)GetValue(RadarItemsProperty);
            set => SetValue(RadarItemsProperty, value);
        }

        public static readonly DependencyProperty RadarItemsProperty =
                  DependencyProperty.Register("RadarItems", typeof(ObservableCollection<RadarItemViewModel>), typeof(RadarView), new PropertyMetadata(defaultValue: null, propertyChangedCallback: null));

        private static void OnRadarItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((ObservableCollection<RadarItemViewModel>)e.NewValue != (ObservableCollection<RadarItemViewModel>)e.OldValue)
                {
                    radarView.InvalidateVisual();
                }
            }
        }
        #endregion

        #region InnerRadius Property
        /// <summary>
        /// 最内圈雷达图半径
        /// </summary>
        public double InnerRadius
        {
            get => (double)GetValue(InnerRadiusProperty);
            set => SetValue(InnerRadiusProperty, value);
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(double), typeof(RadarView), new PropertyMetadata(100.0, new PropertyChangedCallback(OnInnerRadiusChanged)));

        private static void OnInnerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((double)e.NewValue != (double)e.OldValue)
                {
                    radarView.RefreshRadar();
                }
            }
        }
        #endregion

        #region UseCirceCount Property
        /// <summary>
        /// 是否使用count作为圈数标记,和雷达子项冲突
        /// </summary>
        public bool UseCirceCount
        {
            get => (bool)GetValue(UseCirceCountProperty);
            set => SetValue(UseCirceCountProperty, value);
        }

        /// <summary>
        /// 是否使用count作为圈数标记,和雷达子项冲突
        /// </summary>
        public static readonly DependencyProperty UseCirceCountProperty =
            DependencyProperty.Register("UseCirceCount", typeof(bool), typeof(RadarView), new PropertyMetadata(defaultValue: true, propertyChangedCallback: new PropertyChangedCallback(OnUseCirceCountChanged)));

        private static void OnUseCirceCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((bool)e.NewValue != (bool)e.OldValue)
                {
                    radarView.RefreshRadar();
                }
            }
        }
        #endregion

        #region CirceCount Property
        /// <summary>
        /// 雷达的圈数,默认为5
        /// </summary>
        public int CirceCount
        {
            get => (int)GetValue(CirceCountProperty);
            set => SetValue(CirceCountProperty, value);
        }

        public static readonly DependencyProperty CirceCountProperty =
            DependencyProperty.Register("CirceCount", typeof(int), typeof(RadarView), new PropertyMetadata(defaultValue: 5, propertyChangedCallback: new PropertyChangedCallback(OnCirceCountChanged)));

        private static void OnCirceCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((int)e.NewValue != (int)e.OldValue)
                {
                    radarView.RefreshRadar();
                }
            }
        }
        #endregion

        #region UnionColor Property
        /// <summary>
        /// 是否统一颜色,当存在冲突时,使用默认颜色
        /// </summary>
        public bool UnionColor
        {
            get => (bool)GetValue(UnionColorProperty);
            set => SetValue(UnionColorProperty, value);
        }

        public static readonly DependencyProperty UnionColorProperty =
            DependencyProperty.Register("UnionColor", typeof(bool), typeof(RadarView), new PropertyMetadata(defaultValue: true, propertyChangedCallback: new PropertyChangedCallback(OnUnionColorChanged)));

        private static void OnUnionColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((bool)e.NewValue != (bool)e.OldValue)
                {
                    radarView.RefreshRadar();
                }
            }
        }
        #endregion

        #region CircelColors Property
        /// <summary>
        /// 每圈的颜色设置,可以设置一个,但是需要使用unioncolor.可以多个颜色配合每圈.
        /// </summ
        public ObservableCollection<System.Windows.Media.Brush> CircelColors
        {
            get => (ObservableCollection<Brush>)GetValue(CircelColorsProperty);
            set => SetValue(CircelColorsProperty, value);
        }

        /// <summary>
        /// 每圈的颜色设置,可以设置一个,但是需要使用unioncolor.可以多个颜色配合每圈.
        /// </summary>
        public static readonly DependencyProperty CircelColorsProperty =
            DependencyProperty.Register("CircelColors", typeof(ObservableCollection<Brush>), typeof(RadarView), new PropertyMetadata(defaultValue: null, propertyChangedCallback: new PropertyChangedCallback(OnCircelColorsChanged)));

        private static void OnCircelColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((ObservableCollection<Brush>)e.NewValue != (ObservableCollection<Brush>)e.OldValue)
                {
                    radarView.RefreshRadar();
                    //radarView.InvalidateVisual();
                }
            }
        }
        #endregion

        #region RadarCircles Property
        /// <summary>
        /// 每圈的大小
        /// </summary>
        public ObservableCollection<RadarSize> RadarCircles
        {
            get { return (ObservableCollection<RadarSize>)GetValue(RadarCirclesProperty); }
            set { SetValue(RadarCirclesProperty, value); }
        }

        public static readonly DependencyProperty RadarCirclesProperty =
            DependencyProperty.Register("RadarCircles", typeof(ObservableCollection<RadarSize>), typeof(RadarView), new PropertyMetadata(defaultValue: null, propertyChangedCallback: new PropertyChangedCallback(OnRadarCircleChanged)));

        private static void OnRadarCircleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((ObservableCollection<RadarSize>)e.NewValue != (ObservableCollection<RadarSize>)e.OldValue)
                {
                    radarView.RefreshRadar();
                    //radarView.InvalidateVisual();
                }
            }
        }
        #endregion

        #region RadarLineCount Property
        /// <summary>
        /// 雷达图的分割线,目前固定为6,可以自行修改
        /// </summary>
        public int RadarLineCount
        {
            get => (int)GetValue(RadarLineCountProperty);
            set => SetValue(RadarLineCountProperty, value);
        }

        public static readonly DependencyProperty RadarLineCountProperty =
            DependencyProperty.Register("RadarLineCount", typeof(int), typeof(RadarView), new PropertyMetadata(defaultValue: 6, propertyChangedCallback: OnRadarLineCountChanged));

        private static void OnRadarLineCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((int)e.NewValue != (int)e.OldValue)
                {
                    radarView.RefreshRadar();
                }
            }
        }
        #endregion

        #region RadarLineSizes Property
        /// <summary>
        /// 雷达图的分割线,目前固定为6,可以自行修改
        /// </summary>
        public ObservableCollection<RadarLineSize> RadarLines
        {
            get => (ObservableCollection<RadarLineSize>)GetValue(RadarLinesProperty);
            set => SetValue(RadarLinesProperty, value);
        }

        public static readonly DependencyProperty RadarLinesProperty =
            DependencyProperty.Register("RadarLines", typeof(ObservableCollection<RadarLineSize>), typeof(RadarView), new PropertyMetadata(defaultValue: null, propertyChangedCallback: null));
        #endregion

        #region RadarLineColor/ RadarLineThickness Property
        /// <summary>
        /// 雷达分割线的颜色
        /// </summary>
        public Brush RadarLineColor
        {
            get => (Brush)GetValue(RadarLineColorProperty);
            set => SetValue(RadarLineColorProperty, value);
        }

        public static readonly DependencyProperty RadarLineColorProperty =
            DependencyProperty.Register("RadarLineColor", typeof(Brush), typeof(RadarView), new PropertyMetadata(defaultValue: new SolidColorBrush(Colors.Black), propertyChangedCallback: new PropertyChangedCallback(OnRadarLineColorChanged)));

        private static void OnRadarLineColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((Brush)e.NewValue != (Brush)e.OldValue)
                {
                    radarView.RefreshRadar();
                }
            }
        }

        public double RadarLineThickness
        {
            get => (double)GetValue(RadarLineThicknessProperty);
            set => SetValue(RadarLineThicknessProperty, value);
        }

        public static readonly DependencyProperty RadarLineThicknessProperty =
            DependencyProperty.Register("RadarLineThickness", typeof(double), typeof(RadarView), new PropertyMetadata(defaultValue: 1.5, propertyChangedCallback: OnRadarLineThicknessChanged));

        private static void OnRadarLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarView radarView)
            {
                if ((double)e.NewValue != (double)e.OldValue)
                {
                    radarView.RefreshRadar();
                }
            }
        }
        #endregion

        #region ScanningColor Property
        /// <summary>
        /// 雷达扫描的颜色
        /// </summary>
        public Brush ScanningColor
        {
            get => (Brush)GetValue(ScanningColorProperty);
            set => SetValue(ScanningColorProperty, value);
        }

        public static readonly DependencyProperty ScanningColorProperty =
                  DependencyProperty.Register("ScanningColor", typeof(Brush), typeof(RadarView), new PropertyMetadata(defaultValue: Brushes.White, propertyChangedCallback: null));
        #endregion

        #region ScanningPath Property
        /// <summary>
        /// 雷达扫描的形状
        /// </summary>
        public string ScanningPath
        {
            get => (string)GetValue(ScanningPathProperty);
            set => SetValue(ScanningPathProperty, value);
        }

        public static readonly DependencyProperty ScanningPathProperty =
                  DependencyProperty.Register("ScanningPath", typeof(string), typeof(RadarView), new PropertyMetadata(defaultValue: "M 0.1,0.3 A 0.9,1 0 0 1 0.9 0.3 M 0.1 0.3  L 0.5 1 L 0.9 0.3", propertyChangedCallback: null));
        #endregion

        #region OutRadius Property
        /// <summary>
        /// 雷达半径
        /// </summary>
        public double OutRadius
        {
            get { return (Double)GetValue(OutRadiusProperty); }
            set { SetValue(OutRadiusProperty, value); }
        }

        public static readonly DependencyProperty OutRadiusProperty =
            DependencyProperty.Register("OutRadius", typeof(double), typeof(RadarView), new PropertyMetadata(defaultValue: 0.0, propertyChangedCallback: null));
        #endregion

        #region RadarFullWidth Property
        /// <summary>
        /// 雷达全尺寸
        /// </summary>
        public double RadarFullWidth
        {
            get { return (double)GetValue(RadarFullWidthProperty); }
            set { SetValue(RadarFullWidthProperty, value); }
        }

        public static readonly DependencyProperty RadarFullWidthProperty =
                DependencyProperty.Register("RadarFullWidth", typeof(double), typeof(RadarView), new PropertyMetadata(defaultValue: 0.0, propertyChangedCallback: null));
        #endregion

        #region Play Property
        /// <summary>
        /// 是否播放动画
        /// </summary>
        public bool Play
        {
            get { return (bool)GetValue(PlayProperty); }
            set { SetValue(PlayProperty, value); }
        }

        public static readonly DependencyProperty PlayProperty =
            DependencyProperty.Register("Play", typeof(bool), typeof(RadarView), new PropertyMetadata(defaultValue: false, propertyChangedCallback: null));
        #endregion

        #region ArrangeOverride Method
        //protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeBounds)
        //{
        //    var size = base.ArrangeOverride(arrangeBounds);

        //    //var h = size.Height / CirceCount;
        //    //var w = size.Width / CirceCount;
        //    //var circlesize = h > w ? w : h;
        //    ////是否强制圈数
        //    //if (UseCirceCount)
        //    //{
        //    //    RadarCircles.Clear();
        //    //    RadarLine.Clear();
        //    //    for (int i = 1; i <= CirceCount; i++)
        //    //    {
        //    //        RadarSize radar = new RadarSize();
        //    //        radar.Width = circlesize * i;
        //    //        radar.Height = circlesize * i;
        //    //        Brush brush = new SolidColorBrush(Colors.Red);
        //    //        //是否强制统一颜色
        //    //        if (UnionColor)
        //    //        {
        //    //            if (CircelColors.Count > 0)
        //    //            {
        //    //                brush = CircelColors[0];
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            if (i <= CircelColors.Count)
        //    //            {
        //    //                brush = CircelColors[i - 1];
        //    //            }
        //    //        }
        //    //        radar.Color = brush;
        //    //        brush.Freeze();
        //    //        RadarCircles.Add(radar);
        //    //    }
        //    //}
        //    ////绘制分割线
        //    //var angle = 180.0 / 6;
        //    //circlesize = size.Height > size.Width ? size.Width : size.Height;
        //    //RadarFillWidth = circlesize;
        //    //var midx = circlesize / 2.0;
        //    //var midy = circlesize / 2.0;
        //    //circlesize = circlesize / 2;
        //    //RadarRadius = circlesize;
        //    ////默认为6个
        //    //for (int i = 0; i < 6; i++)
        //    //{
        //    //    var baseangel = angle * i;
        //    //    var l1 = new System.Windows.Point(midx + circlesize * Math.Cos(Rad(baseangel)), midy - circlesize * Math.Sin(Rad(baseangel)));
        //    //    var half = baseangel + 180;
        //    //    var l2 = new System.Windows.Point(midx + circlesize * Math.Cos(Rad(half)), midy - circlesize * Math.Sin(Rad(half)));
        //    //    RadarLineSize radarLine = new RadarLineSize();
        //    //    radarLine.Start = l1;
        //    //    radarLine.End = l2;
        //    //    radarLine.Color = RadarLineColor;
        //    //    RadarLine.Add(radarLine);
        //    //}
        //    return size;
        //}
        #endregion

        #region Refresh Method
        private void RefreshRadar()
        {
            Initialize();

            //是否强制圈数
            if (UseCirceCount)
            {
                CreateCircles();
            }

            CreateLines();

            void Initialize()
            {
                if (RadarCircles is null)
                {
                    RadarCircles = new();
                }
                else
                {
                    RadarCircles.Clear();
                }

                if (RadarLines is null)
                {
                    RadarLines = new();
                }
                else
                {
                    RadarLines.Clear();
                }

                if (CircelColors is null)
                {
                    CircelColors = new();
                }
            }

            void CreateCircles()
            {
                for (int i = 1; i <= CirceCount; i++)
                {
                    RadarSize radar = new()
                    {
                        Width = InnerRadius * i,
                        Height = InnerRadius * i
                    };

                    Brush brush = new SolidColorBrush(Colors.Red);
                    //是否强制统一颜色
                    if (UnionColor)
                    {
                        if (CircelColors.Count > 0)
                        {
                            brush = CircelColors[0];
                        }
                    }
                    else
                    {
                        if (i <= CircelColors.Count)
                        {
                            brush = CircelColors[i - 1];
                        }
                    }
                    radar.Color = brush;

                    brush.Freeze();
                    RadarCircles.Add(radar);
                }
            }

            void CreateLines()
            {
                //绘制分割线
                var angle = 180.0 / RadarLineCount;

                var circleSize = InnerRadius * CirceCount;
                RadarFullWidth = circleSize;

                var midX = circleSize / 2.0;
                var midY = circleSize / 2.0;
                circleSize = circleSize / 2;
                OutRadius = circleSize;
                //默认为6个
                for (int i = 0; i < RadarLineCount; i++)
                {
                    var baseAngle = angle * i;
                    var startPoint = new System.Windows.Point(midX + circleSize * Math.Cos(Rad(baseAngle)), midY - circleSize * Math.Sin(Rad(baseAngle)));
                    var half = baseAngle + 180;
                    var endPoint = new System.Windows.Point(midX + circleSize * Math.Cos(Rad(half)), midY - circleSize * Math.Sin(Rad(half)));

                    RadarLineSize radarLine = new()
                    {
                        Start = startPoint,
                        End = endPoint,
                        Thickness = RadarLineThickness
                    };
                    radarLine.Color = RadarLineColor;
                    RadarLines.Add(radarLine);
                }
            }
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="val">角度</param>
        /// <returns>弧度制</returns>
        private double Rad(double val)
        {
            return val * Math.PI / 180;
        }
        #endregion
    }
}
