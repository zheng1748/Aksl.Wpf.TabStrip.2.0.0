using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Prism;
using Prism.Common;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using Unity;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels;

//public static class ActiveContentHelper
//{
//    #region Create ContentInformation Method
//    //public static async Task<ContentInformation> CreateContentInformationAsync(Infrastructure.MenuItem menuItem)
//    //{
//    //    var container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
//    //    var regionNavigationService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionNavigationService>();
//    //    var dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

//    //    string viewTypeAssemblyQualifiedName = menuItem.ViewName;
//    //    Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
//    //    if (viewType is not null)
//    //    {
//    //        ContentInformation contentInformation = new()
//    //        {
//    //            Name = menuItem.Name,
//    //            Title = menuItem.Title,
//    //            ViewName = menuItem.ViewName
//    //        };

//    //        var viewName = viewType.Name;

//    //        var currentView = container.Resolve<object>(viewName);
//    //        if (currentView is FrameworkElement frameworkElement)
//    //        {
//    //            MvvmHelpers.AutowireViewModel(currentView);

//    //            NavigationParameters navigationParameters = new() { { "CurrentMenuItem", menuItem } };

//    //            var navigationContext = new NavigationContext(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute), navigationParameters);

//    //            Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
//    //            MvvmHelpers.ViewAndViewModelAction(currentView, action);

//    //            contentInformation.ViewName = null;
//    //            contentInformation.ViewElement = frameworkElement;
//    //        }

//    //        return contentInformation;
//    //    }
//    //    else
//    //    {
//    //        throw new ArgumentException(menuItem.ViewName);
//    //    }
//    //}
//    #endregion

//    #region Add View To Content Method
//    //public static async Task AddViewToContentAsync(Infrastructure.MenuItem currentMenuItem,string activeContentName, IDialogViewService dialogViewService)
//    //{
//    //    var rightContentActiveContent = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContents.ViewModels.ActiveContentViewModel>(name: activeContentName);

//    //    string viewTypeAssemblyQualifiedName = currentMenuItem.ViewName;
//    //    Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
//    //    if (viewType is not null)
//    //    {
//    //        var viewName = viewType.Name;

//    //        ActiveContents.ContentInformation contentInformation = new()
//    //        {
//    //            Name = currentMenuItem.Name,
//    //            Title = currentMenuItem.Title,
//    //            ViewName = currentMenuItem.ViewName
//    //        };

//    //        var currentView = rightContentActiveContent.GetStoreViewElementByName(currentMenuItem.Name);

//    //        if (currentView is not null)
//    //        {
//    //            if (currentMenuItem.IsCacheable)
//    //            {
//    //                rightContentActiveContent.SetContentItem(contentInformation);
//    //            }
//    //            else
//    //            {
//    //                rightContentActiveContent.RetsetContentItem(contentInformation);
//    //            }
//    //        }
//    //        else
//    //        {
//    //            rightContentActiveContent.Add(contentInformation);
//    //        }
//    //    }
//    //    else
//    //    {
//    //        var result = await dialogViewService.ConfirmWhenAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type");
//    //    }
//    //}
//    #endregion
//}
