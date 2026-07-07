using System;
using System.Collections.ObjectModel;
using System.Linq;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;

namespace Aksl.Modules.RadarMap.ViewModels
{
    public class RadarMapHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        #endregion

        #region Constructors
        public RadarMapHubViewModel(IUnityContainer container, IEventAggregator eventAggregator, IDialogViewService dialogViewService)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _dialogViewService = dialogViewService;

            RadarMapViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<RadarMapViewModel>();

            InitializeViewModel();
        }
        #endregion

        #region Initialize ViewModel Method
        private void InitializeViewModel()
        {
            RadarMapViewModel.Values = null;
            ObservableCollection<double> temp = new ObservableCollection<double>();
            for (int i = 0; i < RadarMapViewModel.Radials; i++)
            {
                temp.Add(i * 10 + new Random().Next(10, 30));
            }
            RadarMapViewModel.Values = temp;

            RadarMapViewModel.HeightLightValues = null;
            ObservableCollection<double> tempHeighLight = new ObservableCollection<double>();
            for (int i = 0; i < RadarMapViewModel.Radials; i++)
            {
                tempHeighLight.Add(temp.ToList()[new Random().Next(0, 3)]);
                tempHeighLight.Add(temp.ToList()[new Random().Next(3, RadarMapViewModel.Radials)]);
            }
            RadarMapViewModel.HeightLightValues = tempHeighLight;

            RadarMapViewModel.Titles = null;
            ObservableCollection<string> tempTitles = new ObservableCollection<string>();
            for (int i = 0; i < RadarMapViewModel.Radials; i++)
            {
                tempTitles.Add($"体质 {i}");
            }
            RadarMapViewModel.Titles = tempTitles;
        }
        #endregion

        #region Properties
        public RadarMapViewModel RadarMapViewModel { get; private set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
