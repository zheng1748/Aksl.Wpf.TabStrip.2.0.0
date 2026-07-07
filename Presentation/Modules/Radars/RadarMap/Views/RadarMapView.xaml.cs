using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Prism.Events;

using Aksl.Dialogs.Services;

namespace Aksl.Modules.RadarMap.Views
{
    public partial class RadarMapView : UserControl
    {
        #region Constructors
        public RadarMapView()
        {
            SolidColorBrush polygonFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7653a7"));

            //绘制图层
            List<Color> colors = GetSingleColorList(OutColor, InnerColor, Layers);
            for (int i = 0; i < Layers; i++)
            {
                RadarMapLayersPolygon.Add(new Polygon() { Fill = new SolidColorBrush(colors[i]), Stroke = LayerStroke, StrokeThickness = LayerStrokeThickness });
            }

            //绘制射线以及线上值
            for (int i = 0; i < Radials; i++)
            {
                RadarMapRadialsPolyline.Add(new Polyline() { Stroke = RadialBrush, StrokeThickness = RadialThickness });
                RadarMapRadialsValuesPolygons.Add(new Polygon() { Fill = ValueBrush, StrokeThickness = 1 });
            }

            //雷达图值组成的区域
            RadarMapRadialsValuesPolygon = new Polygon() { Fill = ValuesAreaFill, Stroke = ValuesAreaStroke, Opacity = 0.2 };

            InitializeComponent();
        }
        #endregion

        #region RadarMapUserSizeChanged Event
        private void RadarMapUserSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshRadarMap();
        }
        #endregion

        #region Members
        /// <summary>
        /// 每个扇区的角度
        /// </summary>
        private double Angle { set; get; }

        /// <summary>
        /// 用于绘制雷达图的层数的多边形
        /// </summary>
        private List<Polygon> RadarMapLayersPolygon = new();

        /// <summary>
        /// 用于绘制雷达图的射线
        /// </summary>
        private List<Polyline> RadarMapRadialsPolyline = new();

        /// <summary>
        /// 用于绘制雷达图射线上实际值的圆点，使用多边形绘制，以实际值为圆心扩展多变形
        /// </summary>
        private List<Polygon> RadarMapRadialsValuesPolygons = new();

        /// <summary>
        /// 所有的雷达图的多变形
        /// </summary>
        private Polygon RadarMapRadialsValuesPolygon = new();
        #endregion

        #region Layers Property
        /// <summary>
        /// 雷达图的层数
        /// </summary>
        public int Layers
        {
            get => (int)GetValue(LayersProperty);
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                SetValue(LayersProperty, value);
            }
        }
        // Using a DependencyProperty as the backing store for Layers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersProperty =
                   DependencyProperty.Register("Layers", typeof(int), typeof(RadarMapView), new PropertyMetadata(defaultValue: 4, propertyChangedCallback: new PropertyChangedCallback(OnLayersChanged)));

        private static void OnLayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((int)e.NewValue != (int)e.OldValue)
                {
                    bool isRefresh = false;

                    if (radarMapView.RadarMapLayersPolygon.Count > radarMapView.Layers)
                    {
                        int i = radarMapView.RadarMapLayersPolygon.Count - radarMapView.Layers;
                        radarMapView.RadarMapLayersPolygon.RemoveRange(radarMapView.RadarMapLayersPolygon.Count - 1 - i, i);
                        isRefresh = true;
                    }
                    else if (radarMapView.RadarMapLayersPolygon.Count < radarMapView.Layers)
                    {
                        int layerCount = radarMapView.Layers - radarMapView.RadarMapLayersPolygon.Count;
                        for (int i = 0; i < layerCount; i++)
                        {
                            radarMapView.RadarMapLayersPolygon.Add(new() { Stroke = radarMapView.LayerStroke, StrokeThickness = 1 });
                        }

                        isRefresh = true;
                    }

                    if (isRefresh)
                    {
                        radarMapView.RefreshRadarMap();
                    }
                }
            }
        }
        #endregion

        #region LayersPercentList Property
        /// <summary>
        /// 雷达图分层的规则,这里使用0-1之间的数据标识，主要是用比例来表示
        /// 在使用者未指定的情况下，则根据Layers的层数来均分
        /// 设置举例：雷达图分4层，均分每层面积，则LayersPercentList设置为：
        /// LayersPercentList[0] = 0.25;
        /// LayersPercentList[1] = 0.5;
        /// LayersPercentList[2] = 0.75;
        /// LayersPercentList[3] = 1;
        /// </summary>
        public IEnumerable<double> LayersPercentList
        {
            get => (IEnumerable<double>)GetValue(LayersPercentListProperty);
            set => SetValue(LayersPercentListProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersPercentListProperty =
                    DependencyProperty.Register("LayersPercentList", typeof(IEnumerable<double>), typeof(RadarMapView), new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: new PropertyChangedCallback(OnLayersPercentListChanged)));

        private static void OnLayersPercentListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((IEnumerable<double>)e.NewValue != (IEnumerable<double>)e.OldValue)
                {
                    radarMapView.RefreshRadarMap();
                }
            }
        }
        #endregion

        #region Layer StrokeThickness/Stroke/Color Properties
        /// <summary>
        /// 每层边框的粗细
        /// </summary>
        public double LayerStrokeThickness
        {
            get => (double)GetValue(LayerStrokeThicknessProperty);
            set => SetValue(LayerStrokeThicknessProperty, value);
        }

        // Using a DependencyProperty as the backing store for LayerStrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerStrokeThicknessProperty =
            DependencyProperty.Register("LayerStrokeThickness", typeof(double), typeof(RadarMapView), new PropertyMetadata(defaultValue: 1.0, propertyChangedCallback: new PropertyChangedCallback(OnLayerStrokeThicknessChanged)));

        private static void OnLayerStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((double)e.NewValue != (double)e.OldValue)
                {
                    radarMapView.RefreshLayersFillBrushAndThickness();
                }
            }
        }

        /// <summary>
        /// 每层的边框颜色
        /// </summary>
        public SolidColorBrush LayerStroke
        {
            get => (SolidColorBrush)GetValue(LayerStrokeProperty);
            set => SetValue(LayerStrokeProperty, value);
        }

        // Using a DependencyProperty as the backing store for LayerStroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerStrokeProperty =
            DependencyProperty.Register("LayerStroke", typeof(SolidColorBrush), typeof(RadarMapView), new PropertyMetadata(defaultValue: Brushes.White, propertyChangedCallback: new PropertyChangedCallback(OnLayerStrokeChanged)));

        private static void OnLayerStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((SolidColorBrush)e.NewValue != (SolidColorBrush)e.OldValue)
                {
                    radarMapView.RefreshLayersFillBrushAndThickness();
                }
            }
        }

        /// <summary>
        /// 雷达图从内到外渐变色，内部颜色
        /// </summary>
        public Color InnerColor
        {
            get => (Color)GetValue(InnerColorProperty);
            set => SetValue(InnerColorProperty, value);
        }

        // Using a DependencyProperty as the backing store for InnerColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerColorProperty =
            DependencyProperty.Register("InnerColor", typeof(Color), typeof(RadarMapView), new PropertyMetadata(defaultValue: Colors.White, propertyChangedCallback: new PropertyChangedCallback(OnInnerColorChanged)));
        private static void OnInnerColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((Color)e.NewValue != (Color)e.OldValue)
                {
                    radarMapView.RefreshLayersFillBrushAndThickness();
                }
            }
        }

        /// <summary>
        /// 雷达图从内到外渐变色，外部颜色
        /// </summary>
        public Color OutColor
        {
            get => (Color)GetValue(OutColorProperty);
            set => SetValue(OutColorProperty, value);
        }

        // Using a DependencyProperty as the backing store for OutColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutColorProperty =
            DependencyProperty.Register("OutColor", typeof(Color), typeof(RadarMapView), new PropertyMetadata(defaultValue: Colors.Purple, propertyChangedCallback: new PropertyChangedCallback(OnOutColorChanged)));

        private static void OnOutColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((Color)e.NewValue != (Color)e.OldValue)
                {
                    radarMapView.RefreshLayersFillBrushAndThickness();
                }
            }
        }

        private static void OnLayersBrushAndStockThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                radarMapView.RefreshLayersFillBrushAndThickness();
            }
        }
        #endregion

        #region Radials Property
        /// <summary>
        /// 雷达图的射线数
        /// </summary>
        public int Radials
        {
            get => (int)GetValue(RadialsProperty);
            set => SetValue(RadialsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Radials.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadialsProperty =
                     DependencyProperty.Register("Radials", typeof(int), typeof(RadarMapView), new PropertyMetadata(defaultValue: 9, propertyChangedCallback: new PropertyChangedCallback(OnRadialsChanged)));

        private static void OnRadialsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((int)e.NewValue != (int)e.OldValue)
                {
                    bool isRefresh = false;
                    if (radarMapView.RadarMapRadialsPolyline.Count > radarMapView.Radials)
                    {
                        int lineCount = radarMapView.RadarMapRadialsPolyline.Count - radarMapView.Radials;
                        radarMapView.RadarMapRadialsPolyline.RemoveRange(radarMapView.RadarMapRadialsPolyline.Count - 1 - lineCount, lineCount);
                        isRefresh = true;
                    }
                    else if (radarMapView.RadarMapRadialsPolyline.Count < radarMapView.Radials)
                    {
                        int lineCount = radarMapView.Radials - radarMapView.RadarMapRadialsPolyline.Count;
                        for (int i = 0; i < lineCount; i++)
                        {
                            radarMapView.RadarMapRadialsPolyline.Add(new Polyline() { Stroke = radarMapView.RadialBrush, StrokeThickness = 2 });
                        }
                        isRefresh = true;
                    }

                    if (radarMapView.RadarMapRadialsValuesPolygons.Count > radarMapView.Radials)
                    {
                        int polygonsCount = radarMapView.RadarMapRadialsValuesPolygons.Count - radarMapView.Radials;
                        radarMapView.RadarMapRadialsValuesPolygons.RemoveRange(radarMapView.RadarMapRadialsValuesPolygons.Count - 1 - polygonsCount, polygonsCount);
                        isRefresh = true;
                    }
                    else if (radarMapView.RadarMapRadialsValuesPolygons.Count < radarMapView.Radials)
                    {
                        int polygonsCount = radarMapView.Radials - radarMapView.RadarMapRadialsValuesPolygons.Count;
                        for (int i = 0; i < polygonsCount; i++)
                        {
                            radarMapView.RadarMapRadialsValuesPolygons.Add(new Polygon() { Stroke = radarMapView.LayerStroke, StrokeThickness = 1 });
                        }
                        isRefresh = true;
                    }

                    if (isRefresh)
                    {
                        radarMapView.RefreshRadarMap();
                    }
                }
            }
        }

        /// <summary>
        /// 雷达图半径,决定雷达图的半径
        /// </summary>
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(RadarMapView), new PropertyMetadata(100.0, new PropertyChangedCallback(OnRadiusChanged)));

        private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((double)e.NewValue != (double)e.OldValue)
                {
                    radarMapView.RefreshRadarMap();
                }
            }
        }

        /// <summary>
        /// 射线颜色
        /// </summary>
        public SolidColorBrush RadialBrush
        {
            get => (SolidColorBrush)GetValue(RadialBrushProperty);
            set => SetValue(RadialBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for RadialBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadialBrushProperty =
               DependencyProperty.Register("RadialBrush", typeof(SolidColorBrush), typeof(RadarMapView), new PropertyMetadata(Brushes.White, new PropertyChangedCallback(OnRadialBrushChanged)));

        private static void OnRadialBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((SolidColorBrush)e.NewValue != (SolidColorBrush)e.OldValue)
                {
                    radarMapView.RefreshRadialBrushAndThinkness();
                }
            }
        }

        /// <summary>
        /// 射线粗细
        /// </summary>
        public double RadialThickness
        {
            get => (double)GetValue(RadialThicknessProperty);
            set => SetValue(RadialThicknessProperty, value);
        }

        // Using a DependencyProperty as the backing store for RadialThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadialThicknessProperty =
            DependencyProperty.Register("RadialThickness", typeof(double), typeof(RadarMapView), new PropertyMetadata(1.5, new PropertyChangedCallback(OnRadialThicknessChanged)));

        private static void OnRadialThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((double)e.NewValue != (double)e.OldValue)
                {
                    radarMapView.RefreshRadialBrushAndThinkness();
                }
            }
        }

        private static void OnRadialBrushAndThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                radarMapView.RefreshRadialBrushAndThinkness();
            }
        }
        #endregion

        #region ValueRadius Property
        /// <summary>
        /// 射线上的所有值点
        /// 1. 注意在使用绑定时，要先将Binding对象设置为null，然后将数据整合好的ObservableCollection再赋值给绑定对象，否则不更新
        /// </summary>
        public IEnumerable<double> Values
        {
            get => (IEnumerable<double>)GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }

        // Using a DependencyProperty as the backing store for Values.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register("Values", typeof(IEnumerable<double>), typeof(RadarMapView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValuesAndHeightLightValuesChanged)));

        private static void OnValuesAndHeightLightValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((IEnumerable<double>)e.NewValue != (IEnumerable<double>)e.OldValue)
                {
                    radarMapView.DrawRadarMapRadialsValues();
                }
            }
        }

        /// <summary>
        /// 普通值点的绘制半径
        /// </summary>
        public double ValueRadius
        {
            get => (double)GetValue(ValueRadiusProperty);
            set => SetValue(ValueRadiusProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValueRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueRadiusProperty =
            DependencyProperty.Register("ValueRadius", typeof(double), typeof(RadarMapView), new PropertyMetadata(4.0, new PropertyChangedCallback(OnValueRadiusAndHeighLightRadiusChanged)));

        private static void OnValueRadiusAndHeighLightRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((double)e.NewValue != (double)e.OldValue)
                {
                    radarMapView.RefreshValuesRadiusAndBrush();
                }
            }
        }

        /// <summary>
        /// 普通值点的颜色
        /// </summary>
        public SolidColorBrush ValueBrush
        {
            get => (SolidColorBrush)GetValue(ValueBrushProperty);
            set => SetValue(ValueBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValueBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueBrushProperty =
            DependencyProperty.Register("ValueBrush", typeof(SolidColorBrush), typeof(RadarMapView), new PropertyMetadata(Brushes.Red, new PropertyChangedCallback(OnValueBrushAndHeighLightBrushChanged)));

        private static void OnValueBrushAndHeighLightBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((SolidColorBrush)e.NewValue != (SolidColorBrush)e.OldValue)
                {
                    radarMapView.RefreshValuesRadiusAndBrush();
                }
            }
        }

        /// <summary>
        /// 需要高亮点的索引
        /// 1. 注意在使用绑定时，要先将Binding对象设置为null，然后将数据整合好的ObservableCollection再赋值给绑定对象，否则不更新
        /// </summary>
        public IEnumerable<double> HeightLightValues
        {
            get => (IEnumerable<double>)GetValue(HeightLightValuesProperty);
            set => SetValue(HeightLightValuesProperty, value);
        }

        // Using a DependencyProperty as the backing store for HeightLightPoints.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightLightValuesProperty =
            DependencyProperty.Register("HeightLightValues", typeof(IEnumerable<double>), typeof(RadarMapView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValuesAndHeightLightValuesChanged)));

        /// <summary>
        /// 光亮点的半径
        /// </summary>
        public double HeighLightRadius
        {
            get => (double)GetValue(HeighLightRadiusProperty);
            set => SetValue(HeighLightRadiusProperty, value);
        }

        // Using a DependencyProperty as the backing store for HeighLightValueRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeighLightRadiusProperty =
            DependencyProperty.Register("HeighLightRadius", typeof(double), typeof(RadarMapView), new PropertyMetadata(6.0, new PropertyChangedCallback(OnValueRadiusAndHeighLightRadiusChanged)));

        /// <summary>
        /// 高亮点的颜色
        /// </summary>
        public SolidColorBrush HeighLightBrush
        {
            get => (SolidColorBrush)GetValue(HeighLightBrushProperty);
            set => SetValue(HeighLightBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeighLightBrushProperty =
            DependencyProperty.Register("HeighLightBrush", typeof(SolidColorBrush), typeof(RadarMapView), new PropertyMetadata(Brushes.Yellow, new PropertyChangedCallback(OnValueBrushAndHeighLightBrushChanged)));

        //private static void OnChangedToRefreshValueRadiusAndBrush(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is RadarMapView radarMapView)
        //    {
        //        radarMapView.RefreshValuesRadiusAndBrush();
        //    }
        //}
        #endregion

        #region Area Property
        /// <summary>
        /// 雷达图值区域填充色
        /// </summary>
        public SolidColorBrush ValuesAreaFill
        {
            get => (SolidColorBrush)GetValue(ValuesAreaFillProperty);
            set => SetValue(ValuesAreaFillProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValuesAreaFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValuesAreaFillProperty =
            DependencyProperty.Register("ValuesAreaFill", typeof(SolidColorBrush), typeof(RadarMapView), new PropertyMetadata(Brushes.Red, new PropertyChangedCallback(OnValuesAreaFillAndStrokeBrushChanged)));

        /// <summary>
        /// 雷达图值区域边框色
        /// </summary>
        public SolidColorBrush ValuesAreaStroke
        {
            get => (SolidColorBrush)GetValue(ValuesAreaStrokeProperty);
            set => SetValue(ValuesAreaStrokeProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValuesAreaStroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValuesAreaStrokeProperty =
            DependencyProperty.Register("ValuesAreaStroke", typeof(SolidColorBrush), typeof(RadarMapView), new PropertyMetadata(Brushes.Gray, new PropertyChangedCallback(OnValuesAreaFillAndStrokeBrushChanged)));

        private static void OnValuesAreaStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((SolidColorBrush)e.NewValue != (SolidColorBrush)e.OldValue)
                {
                    radarMapView.RefreshValuesAreaBrushAndStroke();
                }
            }
        }

        private static void OnValuesAreaFillAndStrokeBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((SolidColorBrush)e.NewValue != (SolidColorBrush)e.OldValue)
                {
                    radarMapView.RefreshValuesAreaBrushAndStroke();
                }
            }
        }

        private static void OnChangedToRefreshValuesAreaFillAndStrokeBrush(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                radarMapView.RefreshValuesAreaBrushAndStroke();
            }
        }
        #endregion

        #region Title Property
        /// <summary>
        /// 是否显示Title
        /// </summary>
        public bool ShowTitle
        {
            get => (bool)GetValue(ShowTitleProperty);
            set => SetValue(ShowTitleProperty, value);
        }

        // Using a DependencyProperty as the backing store for ShowTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowTitleProperty =
            DependencyProperty.Register("ShowTitle", typeof(bool), typeof(RadarMapView), new PropertyMetadata(false, new PropertyChangedCallback(OnShowTitleChanged)));

        private static void OnShowTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((bool)e.NewValue != (bool)e.OldValue)
                {
                    radarMapView.RefreshRadarMap();
                }
            }
        }

        /// <summary>
        /// 文字的前景色
        /// </summary>
        public SolidColorBrush TitleForground
        {
            get => (SolidColorBrush)GetValue(TitleForgroundProperty);
            set => SetValue(TitleForgroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for TitleForground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleForgroundProperty =
            DependencyProperty.Register("TitleForground", typeof(SolidColorBrush), typeof(RadarMapView), new PropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnTitleForgroundChanged)));

        private static void OnTitleForgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((SolidColorBrush)e.NewValue != (SolidColorBrush)e.OldValue)
                {
                    radarMapView.RefreshRadarMap();
                }
            }
        }

        /// <summary>
        /// 文字的字号
        /// </summary>
        public int TitleFontSize
        {
            get => (int)GetValue(TitleFontSizeProperty);
            set => SetValue(TitleFontSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for TitleFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleFontSizeProperty =
            DependencyProperty.Register("TitleFontSize", typeof(int), typeof(RadarMapView), new PropertyMetadata(14, new PropertyChangedCallback(OnTitleFontSizeChanged)));

        private static void OnTitleFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((int)e.NewValue != (int)e.OldValue)
                {
                    radarMapView.RefreshRadarMap();
                }
            }
        }

        /// <summary>
        /// FontWeight
        /// </summary>
        public FontWeight TitleFontWeight
        {
            get => (FontWeight)GetValue(TitleFontWeightProperty);
            set => SetValue(TitleFontWeightProperty, value);
        }

        // Using a DependencyProperty as the backing store for TitleFontWeights.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleFontWeightProperty =
            DependencyProperty.Register("TitleFontWeights", typeof(FontWeight), typeof(RadarMapView), new PropertyMetadata(FontWeights.Normal, new PropertyChangedCallback(OnTitleFontWeightChanged)));

        private static void OnTitleFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((FontWeight)e.NewValue != (FontWeight)e.OldValue)
                {
                    radarMapView.RefreshRadarMap();
                }
            }
        }

        /// <summary>
        /// Title要显示的文字
        /// </summary>
        public IEnumerable<string> Titles
        {
            get => (IEnumerable<string>)GetValue(TitlesProperty);
            set => SetValue(TitlesProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitlesProperty =
            DependencyProperty.Register("Titles", typeof(IEnumerable<string>), typeof(RadarMapView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTitlesChanged)));

        private static void OnTitlesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                if ((IEnumerable<string>)e.NewValue != (IEnumerable<string>)e.OldValue)
                {
                    radarMapView.RefreshRadarMap();
                }
            }
        }

        private static void OnChangedToRefreshTitles(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadarMapView radarMapView)
            {
                radarMapView.RefreshRadarMap();
            }
        }
        #endregion

        #region RefreshRadarMap Method
        /// <summary>
        /// 刷新雷达图
        /// </summary>
        private void RefreshRadarMap()
        {
            GridRadarMap.Children.Clear();

            //首先清除一下polygon里存储的数据
            for (int i = 0; i < Layers; i++)
            {
                RadarMapLayersPolygon[i]?.Points?.Clear();
            }

            for (int i = 0; i < Radials; i++)
            {
                RadarMapRadialsPolyline[i]?.Points?.Clear();
            }

            //如果设置了LayersPercentList，并且LayersPercentList的元素个数与层数相同则按照LayersPercentList画每层的占比，否则均分每层占比
            List<double> layersPercents = new List<double>();
            if (LayersPercentList != null && LayersPercentList.Count() == Layers && LayersPercentList.Max() < 1)
            {
                foreach (var item in LayersPercentList)
                {
                    layersPercents.Add(item);
                }
            }
            else
            {
                double gap = 1.0 / Layers;
                for (int i = 0; i < Layers; i++)
                {
                    layersPercents.Add(gap * i + gap); //计算每层的默认占比
                }
            }

            //计算每个扇区的角度
            Angle = 360 / Radials;

            //计算并添加雷达图的区域线和射线上的点
            for (int i = 0; i < Radials; i++)
            {
                //射线上每层的点,从外到内
                List<Point> points = new List<Point>();
                for (int j = 0; j < Layers; j++)
                {
                    var a = (Radius * layersPercents[Layers - j - 1]);
                    var n = Math.Cos((Angle * i - 90) * Math.PI / 180);
                    Point p = new Point(Radius + (Radius * layersPercents[Layers - j - 1]) * Math.Cos((Angle * i - 90) * Math.PI / 180),
                                        Radius + (Radius * layersPercents[Layers - j - 1]) * Math.Sin((Angle * i - 90) * Math.PI / 180));

                    points.Add(p);

                    //添加到区域线中
                    RadarMapLayersPolygon[j].Points.Add(p);
                }

                //添加到射线中
                foreach (var item in points)
                {
                    RadarMapRadialsPolyline[i].Points.Add(item);
                }

                //计算原点并添加到射线中
                Point originPoint = new Point(Radius + Radius * 0 * Math.Cos((Angle * i - 90) * Math.PI / 180),
                                              Radius + Radius * 0 * Math.Sin((Angle * i - 90) * Math.PI / 180));

                RadarMapRadialsPolyline[i].Points.Add(originPoint);
            }

            //绘制区域层
            foreach (var polygon in RadarMapLayersPolygon)
            {
                if (!GridRadarMap.Children.Contains(polygon))
                {
                    GridRadarMap.Children.Add(polygon);
                }
            }

            //绘制雷达图射线
            foreach (var polyline in RadarMapRadialsPolyline)
            {
                if (!GridRadarMap.Children.Contains(polyline))
                {
                    GridRadarMap.Children.Add(polyline);
                }
            }

            //绘制雷达图上的文字
            if (ShowTitle && Titles != null && Titles.Count() == Radials)
            {
                List<string> titleList = Titles.ToList();
                for (int i = 0; i < Radials; i++)
                {
                    Point point = RadarMapLayersPolygon[0].Points[i];
                    string title = titleList[i];
                    TextBlock textBlock = RefreshRadiusTitles(point, title);

                    if (!GridRadarMap.Children.Contains(textBlock))
                        GridRadarMap.Children.Add(textBlock);
                }
            }

            DrawRadarMapRadialsValues();
        }
        #endregion

        #region Refres Methods
        /// <summary>
        /// 刷新雷达图的层的填充色和层线粗细
        /// </summary>
        private void RefreshLayersFillBrushAndThickness()
        {
            //绘制雷达图层的多边形
            List<Color> colors = GetSingleColorList(OutColor, InnerColor, Layers);
            for (int i = 0; i < Layers; i++)
            {
                RadarMapLayersPolygon[i].Fill = new SolidColorBrush(colors[i]);
                RadarMapLayersPolygon[i].Stroke = LayerStroke;
                RadarMapLayersPolygon[i].StrokeThickness = LayerStrokeThickness;
            }
        }

        /// <summary>
        /// 刷新射线的颜色和粗细
        /// </summary>
        private void RefreshRadialBrushAndThinkness()
        {
            foreach (var item in RadarMapRadialsPolyline)
            {
                item.Stroke = RadialBrush;
                item.StrokeThickness = RadialThickness;
            }
        }

        /// <summary>
        /// 刷新雷达图上值的点的半径和填充色，以及高亮点的半径和填充色
        /// </summary>
        private void RefreshValuesRadiusAndBrush()
        {
            if (Values == null)
                return;

            bool drawHeight = false;
            if (HeightLightValues != null && HeightLightValues.Count() > 0)
                drawHeight = true;

            List<double> values = Values.ToList();
            for (int i = 0; i < RadarMapRadialsValuesPolygon.Points.Count; i++)
            {
                RadarMapRadialsValuesPolygons[i].Points.Clear();
                RadarMapRadialsValuesPolygons[i].Fill = ValueBrush;

                double radius = ValueRadius;

                if (drawHeight)
                {
                    if (HeightLightValues.Contains(values[i]))
                    {
                        radius = HeighLightRadius;
                        RadarMapRadialsValuesPolygons[i].Fill = HeighLightBrush;

                        if (ShowTitle && Titles != null && Titles.Count() > i)
                        {
                            List<string> titleList = Titles.ToList();
                            string heightTitle = titleList[i];
                            foreach (var item in GridRadarMap.Children)
                            {
                                if (item is TextBlock)
                                {
                                    TextBlock textBlock = (TextBlock)item;
                                    if (textBlock.Text == heightTitle)
                                    {
                                        textBlock.Foreground = HeighLightBrush;
                                    }
                                }
                            }
                        }
                    }
                }

                Point valuePoint = RadarMapRadialsValuesPolygon.Points[i];
                Point[] calc_points = GetEllipsePoints(valuePoint, radius);

                foreach (var p in calc_points)
                {
                    RadarMapRadialsValuesPolygons[i].Points.Add(p);
                }

                if (!GridRadarMap.Children.Contains(RadarMapRadialsValuesPolygons[i]))
                    GridRadarMap.Children.Add(RadarMapRadialsValuesPolygons[i]);
            }
        }

        /// <summary>
        /// 刷新雷达图值区域的填充色和边框色
        /// </summary>
        private void RefreshValuesAreaBrushAndStroke()
        {
            RadarMapRadialsValuesPolygon.Fill = ValuesAreaFill;
            RadarMapRadialsValuesPolygon.Stroke = ValuesAreaStroke;
        }

        /// <summary>
        /// 刷新射线上的文字标题
        /// </summary>
        /// <param name="point">图层最外层的点</param>
        /// <returns></returns>
        private TextBlock RefreshRadiusTitles(Point point, string title)
        {
            TextBlock textBlock = new TextBlock();

            textBlock.FontSize = 20;
            textBlock.Text = title;
            textBlock.Foreground = TitleForground;
            textBlock.FontWeight = FontWeights.Normal;
            textBlock.FontSize = TitleFontSize;

            //计算文字的实际像素值
            Rect rect1 = new Rect();
            textBlock.Arrange(rect1);
            double textLength = textBlock.ActualWidth;

            Thickness thickness = new Thickness(point.X + 10, point.Y - 10, 0, 0);
            if (point.X == Radius && point.Y < Radius)
            {
                thickness = new Thickness(point.X - textLength / 2, point.Y - 30, 0, 0);
            }
            else if (point.X == Radius && point.Y >= Radius)
            {
                thickness = new Thickness(point.X - textLength / 2, point.Y + 10, 0, 0);
            }
            else if (point.X < Radius)
            {
                thickness = new Thickness(point.X - 20 - textLength, point.Y - 10, 0, 0);
            }
            else
            {
                thickness = new Thickness(point.X + 10, point.Y - 10, 0, 0);
            }

            textBlock.Margin = thickness;

            return textBlock;
        }
        #endregion

        #region DrawRadarMapRadialsValues Method
        /// <summary>
        /// 绘制雷达图上的点
        /// </summary>
        /// <param name="Values"></param>
        /// <param name="mainType"></param>
        /// <param name="secondType"></param>
        public void DrawRadarMapRadialsValues()
        {
            if (Values == null || Values.Count() != Radials)
            {
                return;
            }

            int fullScore = 100;

            RadarMapRadialsValuesPolygon.Points.Clear();
            for (int i = 0; i < Radials; i++)
            {
                double temp = Values.ToList()[i];

                if (temp <= 0)
                {
                    continue;
                }

                Point value = new Point(Radius + Radius * (temp * 1.0 / fullScore) * Math.Cos((Angle * i - 90) * Math.PI / 180),
                                        Radius + Radius * (temp * 1.0 / fullScore) * Math.Sin((Angle * i - 90) * Math.PI / 180));

                RadarMapRadialsValuesPolygon.Points.Add(value);
            }

            if (!GridRadarMap.Children.Contains(RadarMapRadialsValuesPolygon))
            {
                GridRadarMap.Children.Add(RadarMapRadialsValuesPolygon);
            }

            RefreshValuesRadiusAndBrush();
        }
        #endregion

        #region GetEllipsePoints Methods

        /// <summary>
        /// 根据圆心，扩展绘制圆
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private Point[] GetEllipsePoints(Point origin, double radius)
        {
            int count = 10;
            Point[] points = new Point[count];

            double angle = 360 / count;

            for (int i = 0; i < count; i++)
            {
                Point p1 = new Point(origin.X + radius * Math.Cos((angle * i - 90) * Math.PI / 180),
                                     origin.Y + radius * Math.Sin((angle * i - 90) * Math.PI / 180));

                points[i] = p1;
            }

            return points;
        }

        /// <summary>
        /// 获得某一颜色区间的颜色集合
        /// </summary>
        /// <param name="sourceColor">起始颜色</param>
        /// <param name="destColor">终止颜色</param>
        /// <param name="count">分度数</param>
        /// <returns>返回颜色集合</returns>
        private List<Color> GetSingleColorList(Color srcColor, Color desColor, int count)
        {
            List<Color> colorFactorList = new ();
            int redSpan = desColor.R - srcColor.R;
            int greenSpan = desColor.G - srcColor.G;
            int blueSpan = desColor.B - srcColor.B;
            for (int i = 0; i < count; i++)
            {
                Color color = Color.FromRgb(
                    (byte)(srcColor.R + (int)((double)i / count * redSpan)),
                    (byte)(srcColor.G + (int)((double)i / count * greenSpan)),
                    (byte)(srcColor.B + (int)((double)i / count * blueSpan))
                );
                colorFactorList.Add(color);
            }
            return colorFactorList;
        }

        #endregion
    }
}
