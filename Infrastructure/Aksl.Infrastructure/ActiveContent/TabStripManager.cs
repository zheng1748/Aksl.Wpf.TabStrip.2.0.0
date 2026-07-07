using Aksl.Tabs;
using Prism;
using Prism.Common;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace Aksl.Infrastructure;

public class TabStripManager
{
    #region Constructors
    public static TabStripManager Instance { get; }
    static TabStripManager()
    {
        Instance = new TabStripManager();
    }
    #endregion

    #region Create ContentInformation Method
    public async Task<TabInformation> CreateContentInformationAsync(Infrastructure.MenuItem menuItem, NavigationParameters navigationParameters = null)
    {
        var viewName = menuItem.GetViewTypeName();

        var unityContainer = PrismIocExtensions.GetUnityContainer();
        var regionNavigationService = unityContainer.Resolve<IRegionNavigationService>();

        TabInformation tabInformation = new()
        {
            Name = menuItem.Name,
            Title = menuItem.Title,
            ViewName = menuItem.ViewName,
        };

        var registeredView = unityContainer.Resolve<object>(viewName);

        if (registeredView is FrameworkElement frameworkElement)
        {
            MvvmHelpers.AutowireViewModel(registeredView);

            if (navigationParameters is null)
            {
                navigationParameters = new() { { "CurrentMenuItem", menuItem } };
            }

            NavigationContext navigationContext = new(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute));
            navigationContext.Parameters = navigationParameters;

            Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
            MvvmHelpers.ViewAndViewModelAction<INavigationAware>(registeredView, action);

            tabInformation.ViewName = null;
            tabInformation.ViewElement = frameworkElement;
        }

        return tabInformation;
    }
    #endregion

    #region Add View To Random Content Method
   
    #endregion

    #region Add View To Content Method
  
    #endregion
}