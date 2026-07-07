using Unity;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

using Aksl.Infrastructure;
using Aksl.Modules.Account.Views;

namespace Aksl.Modules.Shell
{
    public class ShellModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public ShellModule(IUnityContainer container)
        {
            _container = container;
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
        #endregion
    }
}
