using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.Account;
using Aksl.Modules.Account.ViewModels;
using Aksl.Modules.Account.Views;
using Aksl.Modules.AirCompresser;
using Aksl.Modules.CoolingTower;
using Aksl.Modules.HamburgerMenuSideBarTab;
using Aksl.Modules.HamburgerMenuNavigationSideBarTab;
using Aksl.Modules.HamburgerMenuTreeSideBarTab;
using Aksl.Modules.Home;
using Aksl.Modules.Others;
using Aksl.Modules.Pipeline;
using Aksl.Modules.Shell.Configuration;
using Aksl.Modules.Shell.ViewModels;
using Aksl.Modules.Shell.Views;
using Aksl.Modules.Thermometer;
using Aksl.Toolkit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Unity;
using Aksl.Modules.MenuSub;

namespace Aksl.Modules.Shell
{
    public partial class App
    {
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.Register(typeof(ShellView).ToString(), () => Container.Resolve<ShellViewModel>());
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            #region Initialize
            var services = new ServiceCollection();
            services.AddOptions();
          
            string basePath = Directory.GetCurrentDirectory();
            string configPath = Path.Combine(basePath, "Configuration");
            string appSettingsPath = Path.Combine(configPath, "appsettings.json");
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(basePath)
                                                                                   .AddJsonFile(path: appSettingsPath, optional: true, reloadOnChange: false);

            var configurationRoot = configurationBuilder.Build();
           // services.AddSingleton(configuration);

            //services.AddHttpClient<HttpClient>("WebApi", client =>
            //{
            //    client.BaseAddress = new Uri($"{configuration["WebApi:BaseAddress"]}");
            //    // client.BaseAddress = new Uri("http://localhost:5249/api/account/");
            //});
            #endregion

            #region Logging
            services.AddLogging(builder =>
            {
                var loggingSection = configurationRoot.GetSection("Logging");
                var consoleIncludeScopes = loggingSection.GetValue<bool>("Console:IncludeScopes");

                builder.AddConfiguration(loggingSection);
   
                builder.AddSimpleConsole(simpleConsoleFormatterOptions =>
                {
                    simpleConsoleFormatterOptions.IncludeScopes = consoleIncludeScopes;
                    simpleConsoleFormatterOptions.SingleLine = true;
                    simpleConsoleFormatterOptions.TimestampFormat = "hh:mm:ss ";
                });

                //builder.AddFilter("System.Net.Http.HttpClient", LogLevel.Debug)
                //       .AddFilter("Aksl.Infrastructure.LoginHandler", LogLevel.Debug);

                builder.AddFilter((provider, category, logLevel) =>
                {
                    return provider.Contains("ConsoleLoggerProvider") &&
                           (category.Contains("System.Net.Http.HttpClient") || category.Contains("Aksl.Infrastructure.LoginHandler")) &&
                           logLevel >= LogLevel.Information;
                });

                builder.AddDebug();
            });
            #endregion

            #region MemoryCache
            //services.AddDistributedMemoryCache(option =>
            //{
            //    option.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
            //});
            #endregion

            #region Account
            services.Configure<WebApiAddressSettings>(address =>
            {
                address.BaseAddress =$"{configurationRoot["WebApi:BaseAddress"]}";
                address.LoginUrl = $"{address.BaseAddress}/login";
                address.LoginOutUrl = $"{address.BaseAddress}/loginout";
                address.RefreshTokenUrl = $"{address.BaseAddress}/refreshtoken";
                address.ResetLockoutUrl = $"{address.BaseAddress}/resetlockout";
                address.CreateUserUrl = $"{address.BaseAddress}/register";
                address.GetEmailConfirmationTokenUrl = $"{address.BaseAddress}/getemailconfirmationtoken";
                address.ConfirmEmailTokenUrl = $"{address.BaseAddress}/confirmemail";
            });

            services.AddLoginHandler();
            #endregion

            var serviceProvider = services.BuildServiceProvider();
            containerRegistry.RegisterInstance<IServiceProvider>(serviceProvider);

            containerRegistry.RegisterDialogWindow<Dialogs.Views.FixedSizeDialogWindow>(name: nameof(Dialogs.Views.FixedSizeDialogWindow));
            containerRegistry.RegisterSingleton(typeof(Aksl.Dialogs.Services.IDialogViewService), typeof(Aksl.Dialogs.Services.DialogViewService));
            containerRegistry.RegisterDialog<Dialogs.Views.ConfirmView, Dialogs.ViewModels.ConfirmViewModel>();
            //containerRegistry.RegisterDialog<LoginPopupView, LoginPopupViewModel>();

            RegisterMenuFactoryAsync(containerRegistry).Await();

            RegisterBuildWorkspaceViewEventAsync().Await();
        }

        protected async Task RegisterMenuFactoryAsync(IContainerRegistry containerRegistry)
        {
            var dialogViewService = Container.Resolve<IDialogViewService>();

            try
            {
                MenuService menuService = new(new List<string> {"pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/AllMenus.xml",
                                                                "pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/Industry.xml",
                                                                "pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/Pipelines.xml",
                                                                "pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/Thermometers.xml",
                                                                "pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/CoolingTowers.xml",
                                                                "pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/AirCompressers.xml",
                                                                "pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/Others.xml",
                                                                "pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/Radars.xml",
                                                                "pack://application:,,,/Aksl.Wpf.TabStrip;Component/Data/Accounts.xml"
                                                                 });

                await menuService.CreateMenusAsync();

                containerRegistry.RegisterInstance<IMenuService>(menuService);
            }
            catch (Exception ex)
            {
                //Debug.Print(ex.Message);
                dialogViewService.AlertAsync(message: ex.Message, title: "Register Menu", okText: "确定").Await();
            }
        }

        protected Task RegisterBuildWorkspaceViewEventAsync()
        {
            try
            {
                var eventAggregator = Container.Resolve<IEventAggregator>();

                _ = eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>();
                _ = eventAggregator.GetEvent<OnSignInedEvent>();

                //SideBar
                //_ = eventAggregator.GetEvent<OnBuildHamburgerMenuSideBarWorkspaceViewEvent>();
                //_ = eventAggregator.GetEvent<OnBuildHamburgerMenuNavigationSideBarWorkspaceViewEvent>();
                //_ = eventAggregator.GetEvent<OnBuildHamburgerMenuTreeSideBarWorkspaceViewEvent>();

                //_ = eventAggregator.GetEvent<OnBuildHamburgerMenuPopupSideBarWorkspaceViewEvent>();

                //_ = eventAggregator.GetEvent<OnBuildRadarsManagerMenuSubWorkspaceViewEvent>();
                //_ = eventAggregator.GetEvent<OnBuildPipelinesMenuSubWorkspaceViewEvent>();

            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            return Task.CompletedTask;
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            _ = moduleCatalog.AddModule(nameof(AccountModule), typeof(AccountModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(typeof(ShellModule).Name, typeof(ShellModule).AssemblyQualifiedName, InitializationMode.WhenAvailable,
                                        dependsOn: [typeof(HamburgerMenuSideBarModule).Name, typeof(HamburgerMenuNavigationSideBarModule).Name, typeof(HamburgerMenuTreeSideBarViewModule).Name]);

            _ = moduleCatalog.AddModule(typeof(HamburgerMenuSideBarModule).Name, typeof(HamburgerMenuSideBarModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(typeof(HamburgerMenuNavigationSideBarModule).Name, typeof(HamburgerMenuNavigationSideBarModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(typeof(HamburgerMenuTreeSideBarViewModule).Name, typeof(HamburgerMenuTreeSideBarViewModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            //_ = moduleCatalog.AddModule(nameof(HamburgerMenuPopupSideBarModule), typeof(HamburgerMenuPopupSideBarModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            //_ = moduleCatalog.AddModule(nameof(ExpandHamburgerMenuModule), typeof(ExpandHamburgerMenuModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            //_ = moduleCatalog.AddModule(nameof(ExpandHamburgerMenuNavigationBarModule), typeof(ExpandHamburgerMenuNavigationBarModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            //_ = moduleCatalog.AddModule(nameof(ExpandHamburgerMenuTreeBarModule), typeof(ExpandHamburgerMenuTreeBarModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            //_ = moduleCatalog.AddModule(nameof(ExpandHamburgerMenuTabModule), typeof(ExpandHamburgerMenuTabModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(MenuSubModule), typeof(MenuSubModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            //_ = moduleCatalog.AddModule(nameof(TabBarModule), typeof(TabBarModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(nameof(HomeModule), typeof(HomeModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(PipelineModule), typeof(PipelineModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(ThermometerModule), typeof(ThermometerModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(CoolingTowerModule), typeof(CoolingTowerModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(AirCompresserModule), typeof(AirCompresserModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(nameof(OthersModule), typeof(OthersModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(nameof(Aksl.Modules.RadarMap.RadarMapModule), typeof(Aksl.Modules.RadarMap.RadarMapModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<ShellView>();
        }

        protected override void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);
        }

        protected override async void OnInitialized()
        {
            base.OnInitialized();

           var eventAggregator = PrismUnityExtensions.GetEventAggregator();

            #region ILoggerFactory
            var loggerFactory = PrismIocExtensions.GetUnityContainer().Resolve<IServiceProvider>()
                                         ?.GetRequiredService<ILoggerFactory>();
            #endregion
            //var distributedCache = ServiceExtensions.GetMemoryDistributedCache();
            var webApiAddressSettings = ServiceExtensions.GetWebApiAddressSettings().Value;

            var webApiProvider = ServiceExtensions.GetWebApiProvider();
          //  var lwtTokenProvider = ServiceExtensions.GetJwtTokenProvider();
            var loginHandler = ServiceExtensions.GetLoginHandler();

            var refreshTokenDateTime = DateTime.UtcNow.AddHours(1);
            var refreshTokenExpirationTicks = refreshTokenDateTime.ConvertToLong();
            var refreshTokenDateTime1 = refreshTokenExpirationTicks.ConvertToDateTime();
            //Debug.Assert((refreshTokenDateTime==refreshTokenDateTime1));

           // await PrismUnityExtensions.GetDialogViewService().ShowLoginDialogAsync();
        }
    }
}
