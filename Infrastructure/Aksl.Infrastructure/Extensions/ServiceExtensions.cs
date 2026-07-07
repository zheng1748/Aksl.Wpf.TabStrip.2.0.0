using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Unity;

namespace Aksl.Infrastructure;

public static class ServiceExtensions
{
    public static LoginHandler GetLoginHandler()
    {
        var loginHandler = PrismIocExtensions.GetUnityContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<LoginHandler>();

        return loginHandler;
    }

    public static IOptions<WebApiAddressSettings> GetWebApiAddressSettings()
    {
        var webApiAddressSettings = PrismIocExtensions.GetUnityContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<IOptions<WebApiAddressSettings>>();

        return webApiAddressSettings;
    }

    public static IDistributedCache GetMemoryDistributedCache()
    {
        var distributedCache = PrismIocExtensions.GetUnityContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<IDistributedCache>();

        return distributedCache;
    }

    public static WebApiProvider GetWebApiProvider()
    {
        var webApiProvider = PrismIocExtensions.GetUnityContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<WebApiProvider>();

        return webApiProvider;
    }
}

