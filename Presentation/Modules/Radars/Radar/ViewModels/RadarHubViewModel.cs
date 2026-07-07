using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;

namespace Aksl.Modules.RadarMap.ViewModels
{
    public class RadarHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private Random _random = new();
        #endregion

        #region Constructors
        public RadarHubViewModel(IUnityContainer container, IEventAggregator eventAggregator, IDialogViewService dialogViewService)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _dialogViewService = dialogViewService;

            RadarViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<RadarViewModel>();

            CreatePlayCommand();
            CreatePauseCommand();

            RegisterPropertyChanged();

            Initialize();
        }
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            RadarViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is RadarViewModel rvm)
                {
                    if (e.PropertyName == nameof(RadarViewModel.Play))
                    {
                        Playing = rvm.Play;
                        (PlayCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                        (PauseCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    }

                    if (e.PropertyName == nameof(RadarViewModel.RadarItems))
                    {
                        RaisePropertyChanged(nameof(Left));
                        RaisePropertyChanged(nameof(Top));
                    }
                }
            };
        }
        #endregion

        #region Initialize Method
        private void Initialize()
        {
            RadarViewModel.InnerRadius = 58;
            RadarViewModel.CirceCount = 5;
            Width = 300;
            Height = Width;

            RadarViewModel.UseCirceCount = true;
            RadarViewModel.UnionColor = true;
            RadarViewModel.RadarLineCount = 4;

            RadarViewModel.RadarLineColor = new SolidColorBrush(Colors.Black);
            RadarViewModel.RadarLineThickness = 1.5d;

            RadarViewModel.ScanningPath = "M 0.1,0.3 A 0.9,1 0 0 1 0.9 0.3 M 0.1 0.3  L 0.5 1 L 0.9 0.3";

            RadarViewModel.Play = false;

            RadarViewModel.CircelColors = new()
            {
                new SolidColorBrush (Colors.Red),
                new SolidColorBrush (Colors.Gold),
                new SolidColorBrush (Colors.Moccasin),
                new SolidColorBrush (Colors.MintCream),
                new SolidColorBrush (Colors.OliveDrab)
            };

            DoRadarItems();

            RaisePropertyChanged(nameof(RadarViewModel.RadarItems));
            RaisePropertyChanged(nameof(RadarViewModel.RadarCircles));
        }
        #endregion

        #region Initialize Method
        private  async void DoRadarItems()
        {
            while (Playing)
            {
                Application.Current.Dispatcher.Invoke(() => 
                {
                    RadarViewModel.RadarItems.Clear();

                    RadarItemViewModel radarItemViewModel = new()
                    {
                        Width = 15,
                        Height = 15,
                        Color = new SolidColorBrush(Colors.Red),
                        Left = _random.Next(0, (int)Width + 1),
                        Top = _random.Next(0, (int)Height + 1)
                    };

                    RadarViewModel.RadarItems.Add(radarItemViewModel);

                    RaisePropertyChanged(nameof(Left));
                    RaisePropertyChanged(nameof(Top));
                });

                await Task.Delay(TimeSpan.FromMilliseconds(2000)).ConfigureAwait(false);
            }
        }
        #endregion

        #region Properties
        public RadarViewModel RadarViewModel { get; set; }

        private double _width = 100d;
        public double Width
        {
            get => _width;
            set
            {
                if (SetProperty<double>(ref _width, value))
                {
                    Height = Width;

                    RadarViewModel.Width = value;
                    RadarViewModel.Height = value;
                }
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public double Top
        {
            get
            {
                if (RadarViewModel.RadarItems.Any())
                {
                    return RadarViewModel.RadarItems.FirstOrDefault().Top;
                }
               
               return 0;
            }
        }

        public double Left
        {
            get
            {
                if (RadarViewModel.RadarItems.Any())
                {
                    return RadarViewModel.RadarItems.FirstOrDefault().Left;
                }

                return 0;
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }

        public bool Playing { get; set; } = false;
        #endregion

        #region Play Command
        public ICommand PlayCommand { get; private set; }

        private void CreatePlayCommand()
        {
            PlayCommand = new DelegateCommand(async () =>
            {
                await ExecutePlayCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecutePlayCommand();
                return canExecute;
            });
        }

        private async Task ExecutePlayCommandAsync()
        {
            IsLoading = true;

            try
            {
                Playing = true;
                DoRadarItems();

                RadarViewModel.Play = true;

                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync($"{ex.Message}", "Play Failure:");
            }

            IsLoading = false;
        }

        private bool CanExecutePlayCommand()
        {
            return !Playing;
        }
        #endregion

        #region Pause Command
        public ICommand PauseCommand { get; private set; }

        private void CreatePauseCommand()
        {
            PauseCommand = new DelegateCommand(async () =>
            {
                await ExecutePauseCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecutePauseCommand();
                return canExecute;
            });
        }

        private async Task ExecutePauseCommandAsync()
        {
            IsLoading = true;

            try
            {
                Playing = false;
                DoRadarItems();

                RadarViewModel.Play = false;

                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync($"{ex.Message}", "Pause Failure:");
            }

            IsLoading = false;
        }

        private bool CanExecutePauseCommand()
        {
            return Playing;
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
