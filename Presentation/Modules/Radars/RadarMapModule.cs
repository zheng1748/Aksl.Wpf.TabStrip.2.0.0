using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;

using Unity;

using Aksl.Modules.RadarMap.ViewModels;
using Aksl.Modules.RadarMap.Views;

namespace Aksl.Modules.RadarMap
{
    public class RadarMapModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public RadarMapModule(IUnityContainer container)
        {
            this._container = container;
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterForNavigation<RadarMapHubView>();
            //containerRegistry.RegisterForNavigation<RadarHubView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(typeof(RadarMapHubView).ToString(),
                                               () => this._container.Resolve<RadarMapHubViewModel>());
            ViewModelLocationProvider.Register(typeof(RadarHubView).ToString(),
                                              () => this._container.Resolve<RadarHubViewModel>());
        }
        #endregion
    }
}
