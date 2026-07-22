using Aksl.ActiveContents.ViewModels;
using Aksl.Infrastructure;
using Aksl.Modules.Account.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Unity;

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
            var shellActiveContentViewModel = containerProvider.Resolve<RandomActiveContentViewModel>(name: ActiveContentNames.ShellContent);

            ActiveContentManager.Instance.RegisterNavigationForRandomContentt(name: "HamburgerMenuSideBarHubView",
                                                      title: "HamburgerMenuSideBarHubView",
                                                      viewTypeAssemblyQualifiedName: "Aksl.Modules.HamburgerMenuSideBarTab.Views.HamburgerMenuSideBarHubView,Aksl.Modules.HamburgerMenuSideBarTab",
                                                      randomActiveContentViewModel: shellActiveContentViewModel,
                                                      navigationParameters: new() { { "ActiveContentName", "HamburgerMenuNavigationSideBarTab.RightContent" } },
                                                      isActive: false);

            ActiveContentManager.Instance.RegisterNavigationForRandomContentt(name: "HamburgerMenuNavigationSideBarHubView",
                                                      title: "HamburgerMenuNavigationSideBarHubView",
                                                       viewTypeAssemblyQualifiedName: "Aksl.Modules.HamburgerMenuNavigationSideBarTab.Views.HamburgerMenuNavigationSideBarHubView,Aksl.Modules.HamburgerMenuNavigationSideBarTab",
                                                                              randomActiveContentViewModel: shellActiveContentViewModel,
                                                                              navigationParameters: new() { { "ActiveContentName", "HamburgerMenuNavigationSideBarTab.RightContent" } },
                                                                              isActive: false);

            ActiveContentManager.Instance.RegisterNavigationForRandomContentt(name: "HamburgerMenuTreeSideBarHubView",
                                                    title: "HamburgerMenuTreeSideBarHubView",
                                                    viewTypeAssemblyQualifiedName: "Aksl.Modules.HamburgerMenuTreeSideBarTab.Views.HamburgerMenuTreeSideBarHubView,Aksl.Modules.HamburgerMenuTreeSideBarTab",
                                                    randomActiveContentViewModel: shellActiveContentViewModel,
                                                    navigationParameters: new() { { "ActiveContentName", "HamburgerMenuTreeSideBarTab.RightContent" } },
                                                    isActive: false);
        }
        #endregion
    }
}
