using System.Collections.ObjectModel;
using System.Windows.Media;

using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Unity;

using Aksl.Dialogs.Services;

namespace Aksl.Modules.RadarMap.ViewModels
{
    public class RadarViewModel : BindableBase
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        #endregion

        #region Constructors
        public RadarViewModel(IUnityContainer container, IEventAggregator eventAggregator, IDialogViewService dialogViewService)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _dialogViewService = dialogViewService;

            InnerRadius = 100;

            Width = 300;
            Height = 300;

            CircelColors = new();
            RadarCircles = new();
            RadarLines = new();
            RadarItems = new();

            UseCirceCount = true;
            UnionColor = true;
        }
        #endregion

        #region Properties
        public ObservableCollection<RadarSize> RadarCircles { get; set; }

        public ObservableCollection<RadarItemViewModel> RadarItems { get; set; }

        private double _innerRadius = 0d;
        public double InnerRadius
        {
            get => _innerRadius;
            set
            {
                if (SetProperty<double>(ref _innerRadius, value))
                {
                    CirceCount =(int)((Width-2) / _innerRadius);
                }
            }
        }

        public bool UseCirceCount { get; set; }

        private int _circeCount ;
        public int CirceCount
        {
            get => _circeCount;
            set
            {
                if (SetProperty<int>(ref _circeCount, value))
                {
                    InnerRadius = Width / _circeCount - 2;
                }
            }
        }

        private double _width = 300d;
        public double Width
        {
            get => _width;
            set
            {
                if (SetProperty<double>(ref _width, value))
                {
                    Height = Width;

                    InnerRadius = Width / CirceCount - 2;
                }
            }
        }

        private double _height = 300d;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public bool UnionColor { get; set; }

        public ObservableCollection<Brush> CircelColors { get; set; }

        public double RadarFullWidth { get; set; }

        public double OutRadius { get; set; }

        public Brush RadarLineColor { get; set; }

        public double RadarLineThickness { get; set; }

        public int RadarLineCount { get; set; }

        public ObservableCollection<RadarLineSize> RadarLines { get; set; }

        public string ScanningPath { get; set; }

        private bool _play = false;
        public bool Play
        {
            get => _play;
            set => SetProperty<bool>(ref _play, value);
        }
        #endregion
    }
}
