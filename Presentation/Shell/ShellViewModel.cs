using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.Account.ViewModels;
using Aksl.Modules.Account.Views;
using Aksl.Modules.HamburgerMenuSideBarTab.ViewModels;
using Aksl.Modules.HamburgerMenuSideBarTab.Views;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Unity;

namespace Aksl.Modules.Shell.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        #endregion

        #region Constructors
        public ShellViewModel()
        {
            _container = PrismIocExtensions.GetUnityContainer();
            _eventAggregator = _container.Resolve<IEventAggregator>();
            _dialogViewService = _container.Resolve<IDialogViewService>();

            RegisterSignInedEvent();
            RegisterAccessTokenExpiredEvent();

            RegisterActiveContents().Await(configureAwait:true);
        }
        #endregion

        #region Properties
        public RandomActiveContentViewModel ShellContentActiveContentViewModel
        {
            get;
            set => SetProperty<RandomActiveContentViewModel>(ref field, value);
        }

        public RandomActiveContentViewModel LoginActiveContentViewModel
        {
            get;
            set => SetProperty<RandomActiveContentViewModel>(ref field, value);
        }

        public bool IsPaneOpen
        {
            get;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    _eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>().Publish(new() { IsPaneOpen = field });
                }
            }
        }= true;
        #endregion

        #region Register SignIned Event
        private void RegisterSignInedEvent()
        {
            _eventAggregator.GetEvent<OnSignInedEvent>().Subscribe((siet) =>
            {
                if (siet.IsSuccessful)
                {
                    //var hamburgerMenuSideBarHubView = ShellContentActiveContentViewModel.GetStoreViewElementByName("HamburgerMenuSideBarHubView") as HamburgerMenuSideBarHubView;
                    //var hamburgerMenuSideBarHubViewModel = hamburgerMenuSideBarHubView.DataContext as HamburgerMenuSideBarHubViewModel;

                    ShellContentActiveContentViewModel.SetActiveContentItemByName("HamburgerMenuSideBarHubView");
                   // ShellContentActiveContentViewModel.SetActiveContentItemByName("HamburgerMenuNavigationSideBarHubView");
                   // ShellContentActiveContentViewModel.SetActiveContentItemByName("HamburgerMenuTreeSideBarHubView");
                }
                else
                {
                    //ShellContentActiveContentViewModel.RetsetContentItemByName("LoginView");
                    ShellContentActiveContentViewModel.ClearSelectedItem();
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register AccessTokenExpired Event
        private void RegisterAccessTokenExpiredEvent()
        {
            _eventAggregator.GetEvent<OnAccessTokenExpiredEvent>().Subscribe((atee) =>
            {
                if (atee.IsExpired)
                {
                    ShellContentActiveContentViewModel.RetsetContentItemByName("LoginView");

                    var loginStatusView = LoginActiveContentViewModel.GetStoreViewElementByName("LoginStatusView") as LoginStatusView;
                    var loginStatusViewModel = loginStatusView.DataContext as LoginStatusViewModel;
                    loginStatusViewModel.IsSignIning = true;
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register ActiveContent Method
        private async Task RegisterActiveContents()
        {
            try
            {
                _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: ActiveContentNames.ShellContent);
                ShellContentActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<RandomActiveContentViewModel>(name: ActiveContentNames.ShellContent);

                RegisterShellContentActiveContent();
                void RegisterShellContentActiveContent()
                {
                    ShellContentActiveContentViewModel.Add(new()
                    {
                        Name = "LoginView",
                        Title = "LoginView",
                        ViewName = "Aksl.Modules.Account.Views.LoginView,Aksl.Modules.Account",
                        //ViewElement = new LoginView(),
                    }, true);

                    ShellContentActiveContentViewModel.Add(new()
                    {
                        Name = "HamburgerMenuSideBarHubView",
                        Title = "HamburgerMenuSideBarHubView",
                        ViewName = "Aksl.Modules.HamburgerMenuSideBarTab.Views.HamburgerMenuSideBarHubView,Aksl.Modules.HamburgerMenuSideBarTab",
                        //ViewElement = new HamburgerMenuSideBarHubView()
                    }, false);

                    //ShellContentActiveContentViewModel.Add(new()
                    //{
                    //    Name = "HamburgerMenuNavigationSideBarHubView",
                    //    Title = "HamburgerMenuNavigationSideBarHubView",
                    //    ViewName = "Aksl.Modules.HamburgerMenuNavigationSideBar.Views.HamburgerMenuNavigationSideBarHubView,Aksl.Modules.HamburgerMenuNavigationSideBar",
                    //    //ViewElement = new HamburgerMenuSideBarHubView()
                    //}, false);

                    //ShellContentActiveContentViewModel.Add(new()
                    //{
                    //    Name = "HamburgerMenuTreeSideBarHubView",
                    //    Title = "HamburgerMenuTreeSideBarHubView",
                    //    ViewName = "Aksl.Modules.HamburgerMenuTreeSideBar.Views.HamburgerMenuTreeSideBarHubView,Aksl.Modules.HamburgerMenuTreeSideBar"
                    //}, false);
                }

                RegisterLoginActiveContent();
                void RegisterLoginActiveContent()
                {
                    _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: ActiveContentNames.LoginContent);
                    LoginActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<RandomActiveContentViewModel>(name: ActiveContentNames.LoginContent);
                 
                    LoginActiveContentViewModel.Add(new()
                    {
                        Name = "LoginStatusView",
                        Title = "LoginStatusView",
                        ViewName = "Aksl.Modules.Account.Views.LoginStatusView,Aksl.Modules.Account",
                        // ViewElement = new LoginStatusView(),
                    },true);
                }
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Registering Message \"{ex.Message}\"", title: "Error:Register ActiveContents");
            }
        }
        #endregion
    }
}
