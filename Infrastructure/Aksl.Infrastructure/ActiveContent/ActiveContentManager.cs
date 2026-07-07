using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;

using Prism;
using Prism.Common;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using Unity;

using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;

namespace Aksl.Infrastructure;

public class ActiveContentManager
{
    #region Constructors
    public static ActiveContentManager Instance { get; }
    static ActiveContentManager()
    {
        Instance = new ActiveContentManager();
    }
    #endregion

    #region Create ContentInformation Method
    public async Task<ContentInformation> CreateContentInformationAsync(Infrastructure.MenuItem menuItem, NavigationParameters navigationParameters = null)
    {
        var viewName = menuItem.GetViewTypeName();

        var unityContainer = PrismIocExtensions.GetUnityContainer();
        var regionNavigationService = unityContainer.Resolve<IRegionNavigationService>();

        ContentInformation contentInformation = new()
        {
            Name = menuItem.Name,
            Title = menuItem.Title,
            ViewName = menuItem.ViewName
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

            contentInformation.ViewName = null;
            contentInformation.ViewElement = frameworkElement;
        }

        return contentInformation;
    }
    #endregion

    #region Add View To Random Content Method
    public async Task AddViewToRandomContentAsync(Infrastructure.MenuItem menuItem, RandomActiveContentViewModel  randomActiveContentViewModel, NavigationParameters navigationParameters = null)
    {
        var viewName = menuItem.GetViewTypeName();

        var currentView = randomActiveContentViewModel.GetStoreViewElementByName(menuItem.Name);
        if (currentView is not null)
        {
            if (menuItem.IsCacheable)
            {
                // activeContentViewModel.SetContentItem(contentInformation);
                randomActiveContentViewModel.SetActiveContentItemByName(menuItem.Name);
            }
            else
            {
                ActiveContents.ContentInformation contentInformation = await CreateContentInformationAsync(menuItem, navigationParameters);
                randomActiveContentViewModel.RetsetContentItem(contentInformation);
            }
        }
        else
        {
            ActiveContents.ContentInformation contentInformation = await CreateContentInformationAsync(menuItem, navigationParameters);
            randomActiveContentViewModel.Add(contentInformation);
        }
    }
    #endregion

    #region Add View To Content Method
    public async Task AddViewToSequenceContentAsync(Infrastructure.MenuItem menuItem, SequenceActiveContentViewModel sequenceActiveContentViewModel, NavigationParameters navigationParameters = null)
    {
        var viewName = menuItem.GetViewTypeName();

        var currentView = sequenceActiveContentViewModel.GetStoreViewElementByName(menuItem.Name);
        if (currentView is not null)
        {
            if (menuItem.IsCacheable)
            {
                sequenceActiveContentViewModel.SetContentItemByName(menuItem.Name);
            }
            else
            {
                ActiveContents.ContentInformation contentInformation = await CreateContentInformationAsync(menuItem, navigationParameters);
                sequenceActiveContentViewModel.RetsetContentItem(contentInformation);
            }
        }
        else
        {
            ActiveContents.ContentInformation contentInformation = await CreateContentInformationAsync(menuItem, navigationParameters);
            sequenceActiveContentViewModel.Add(contentInformation);
        }
    }
    #endregion
}