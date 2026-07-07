using System.Collections.Generic;

using Prism.Events;
using Prism.Mvvm;
using Unity;

using Aksl.Dialogs.Services;

namespace Aksl.Modules.RadarMap.ViewModels
{
    public class RadarMapViewModel : BindableBase
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        #endregion

        #region Constructors
        public RadarMapViewModel(IUnityContainer container, IEventAggregator eventAggregator, IDialogViewService dialogViewService)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _dialogViewService = dialogViewService;

            LayersPercentList = new List<double>();
        }
        #endregion

        #region Layers Property
        private int _layers=4;
        public int Layers
        {
            get => _layers;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

               SetProperty<int>(ref _layers, value);
            }
        }

        private IEnumerable<double> _layersPercentList;
        public IEnumerable<double> LayersPercentList
        {
            get => _layersPercentList;
            set => SetProperty<IEnumerable<double>>(ref _layersPercentList, value);
        }
        #endregion

        #region Radials Property
        private int _radials =6;
        public int Radials
        {
            get => _radials;
            set => SetProperty<int>(ref _radials, value);
        }
        #endregion

        #region ValueRadius Property
        private IEnumerable<double> _values;
        public IEnumerable<double> Values
        {
            get => _values;
            set => SetProperty<IEnumerable<double>>(ref _values, value);
        }

        private IEnumerable<double> _heightLightValues;
        public IEnumerable<double> HeightLightValues
        {
            get => _heightLightValues;
            set => SetProperty<IEnumerable<double>>(ref _heightLightValues, value);
        }
        #endregion

        #region Title Property
        private IEnumerable<string> _titles;
        public IEnumerable<string> Titles
        {
            get => _titles;
            set => SetProperty(ref _titles, value);
        }
        #endregion

        #region Property
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }
        #endregion
    }
}
