using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Prism;
using Prism.Ioc;
using System;
using System.Threading.Tasks;

using Prism.Regions;
using Prism.Unity;
using Unity;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels;

public static class HamburgerMenuNavigationSideBarHelper
{
    #region Add View To Content Method
    //public static async Task AddViewToContentAsync(Infrastructure.MenuItem menuItem)
    //{
    //    var dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();
    //    var rightContentActiveContentViewModel = (System.Windows.Application.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: ActiveContentNames.RightContentHamburgerMenuNavigationSideBar);

    //    ActiveContentManager activeContentManager = new();

    //    Action<string> exceptionHandler = (message) =>
    //    {
    //        dialogViewService.AlertAsync(message: $"\"{message}\".", title: $"Error:Add View");
    //    };
    //    NavigationParameters navigationParameters = new() { { "CurrentMenuItem", menuItem } };
    // //   activeContentManager.AddViewToContentAsync(menuItem, rightContentActiveContentViewModel, navigationParameters,null).Await();
    //}
    #endregion
}
