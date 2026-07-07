using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
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

public static class PrismUnityExtensions
{
    public static IRegionManager GetRegionManager()
    {
        return PrismIocExtensions.GetUnityContainer().Resolve<IRegionManager>();
    }

    public static IEventAggregator GetEventAggregator()
    {
        return PrismIocExtensions.GetUnityContainer().Resolve<IEventAggregator>();
    }

    public static IDialogViewService GetDialogViewService()
    {
        return PrismIocExtensions.GetUnityContainer().Resolve<IDialogViewService>();
    }

    public static IMenuService GetMenuService()
    {
        return PrismIocExtensions.GetUnityContainer().Resolve<IMenuService>();
    }
}

