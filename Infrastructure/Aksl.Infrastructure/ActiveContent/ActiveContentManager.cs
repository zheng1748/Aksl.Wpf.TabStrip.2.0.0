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

    #region Create ContentInformation Methods
    public ContentInformation CreateContentInformation(string name, string title, string viewTypeAssemblyQualifiedName, NavigationParameters navigationParameters = null)
    {
        Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
        if (viewType is null)
        {
            throw new ArgumentException($"Missing Type {viewTypeAssemblyQualifiedName}");
        }
        var viewName = viewType.Name;

        var unityContainer = PrismIocExtensions.GetUnityContainer();
        var regionNavigationService = unityContainer.Resolve<IRegionNavigationService>();

        ContentInformation contentInformation = new()
        {
            Name = name,
            Title = title,
            ViewName = viewTypeAssemblyQualifiedName
        };

        var registeredView = unityContainer.Resolve<object>(viewName);
        if (registeredView is FrameworkElement frameworkElement)
        {
            MvvmHelpers.AutowireViewModel(registeredView);

            NavigationContext navigationContext = new(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute));
            navigationContext.Parameters = navigationParameters;

            Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
            MvvmHelpers.ViewAndViewModelAction<INavigationAware>(registeredView, action);

            contentInformation.ViewName = null;
            contentInformation.ViewElement = frameworkElement;
        }

        return contentInformation;
    }

    public ContentInformation CreateContentInformation(Infrastructure.MenuItem menuItem, NavigationParameters navigationParameters = null)
    {
        //  CreateContentInformation(menuItem.Name, menuItem.Title, menuItem.ViewName, navigationParameters);

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

            //if (navigationParameters is null)
            //{
            //    navigationParameters = new() { { "CurrentMenuItem", menuItem } };
            //}

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

    #region Register Navigation For RandomContentt Method
    public void RegisterNavigationForRandomContentt(string name, string title, string viewTypeAssemblyQualifiedName, RandomActiveContentViewModel randomActiveContentViewModel, NavigationParameters navigationParameters, bool isActive = true)
    {
        var currentView = randomActiveContentViewModel.GetStoreViewElementByName(name);
        if (currentView is null)
        {
            ActiveContents.ContentInformation contentInformation = CreateContentInformation(name, title, viewTypeAssemblyQualifiedName, navigationParameters);
            randomActiveContentViewModel.Add(contentInformation, isActive);
        }
    }
    #endregion

    #region Add View To Random Content Method
    public async Task AddViewToRandomContentAsync(Infrastructure.MenuItem menuItem, RandomActiveContentViewModel randomActiveContentViewModel, NavigationParameters navigationParameters = null, bool isActive = true)
    {
        //var viewName = menuItem.GetViewTypeName();

        //ContentInformation contentInformation = new()
        //{
        //    Name = menuItem.Name,
        //    Title = menuItem.Title,
        //    ViewName = menuItem.ViewName
        //};
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
               ActiveContents.ContentInformation contentInformation = CreateContentInformation(menuItem, navigationParameters);
                randomActiveContentViewModel.RetsetContentItem(contentInformation);
            }
        }
        else
        {
            ActiveContents.ContentInformation contentInformation = CreateContentInformation(menuItem, navigationParameters);
            randomActiveContentViewModel.Add(contentInformation, isActive);
        }
    }
    #endregion

    #region  NavigationTo To Random Content Method
    public async Task NavigationToRandomContentAsync(Infrastructure.MenuItem menuItem, RandomActiveContentViewModel randomActiveContentViewModel, NavigationParameters navigationParameters, bool isActive = true)
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
                ActiveContents.ContentInformation contentInformation = CreateContentInformation(menuItem, navigationParameters);
                randomActiveContentViewModel.RetsetContentItem(contentInformation);
            }
        }
        else
        {
            ActiveContents.ContentInformation contentInformation = CreateContentInformation(menuItem, navigationParameters);
            randomActiveContentViewModel.Add(contentInformation, isActive);
        }
    }
    #endregion

    #region Add View To Sequence Content Method
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
                ActiveContents.ContentInformation contentInformation = CreateContentInformation(menuItem, navigationParameters);
                sequenceActiveContentViewModel.RetsetContentItem(contentInformation);
            }
        }
        else
        {
            ActiveContents.ContentInformation contentInformation = CreateContentInformation(menuItem, navigationParameters);
            sequenceActiveContentViewModel.Add(contentInformation);
        }
    }
    #endregion
}