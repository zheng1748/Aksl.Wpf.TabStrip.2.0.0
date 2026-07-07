using Aksl.ActiveContents.ViewModels;
using Prism;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace Aksl.Infrastructure;

public static class ActiveContentManagerExtensions
{
    #region Add View To Random Content Method
    public static async Task AddViewToRandomContentAsync(Infrastructure.MenuItem menuItem, string activeContentName, NavigationParameters navigationParameters = null)
    {
        var randomActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<RandomActiveContentViewModel>(name: activeContentName);

        if (navigationParameters is null)
        {
            navigationParameters = new() { { "CurrentMenuItem", menuItem } };
        }

        try
        {
            await ActiveContentManager.Instance.AddViewToRandomContentAsync(menuItem, randomActiveContentViewModel, navigationParameters);
        }
        catch (Exception ex)
        {
            string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

            throw new Exception(msg);
        }
    }
    #endregion

    #region Navigation To Random Content Method
    public static async Task NavigationToRandomContentAsync(Infrastructure.MenuItem menuItem, string activeContentName, NavigationParameters navigationParameters = null)
    {
        await AddViewToRandomContentAsync(menuItem, activeContentName, navigationParameters);

        //var randomActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<RandomActiveContentViewModel>(name: activeContentName);

        //try
        //{
        //    await ActiveContentManager.Instance.AddViewToRandomContentAsync(menuItem, randomActiveContentViewModel, navigationParameters);
        //}
        //catch (Exception ex)
        //{
        //    string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

        //    throw new Exception(msg);
        //}
    }
    #endregion

    #region Add View To Sequence Content Method
    public static async Task AddViewToSequenceContentAsync(Infrastructure.MenuItem menuItem, string activeContentName, NavigationParameters navigationParameters = null)
    {
        var randomActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<SequenceActiveContentViewModel>(name: activeContentName);

        try
        {
            await ActiveContentManager.Instance.AddViewToSequenceContentAsync(menuItem, randomActiveContentViewModel, navigationParameters);
        }
        catch (Exception ex)
        {
            string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

            throw new Exception(msg);
        }
    }
    #endregion

    #region Navigation To Sequence Content Method
    public static async Task NavigationToSequenceContentAsync(Infrastructure.MenuItem menuItem, string activeContentName, NavigationParameters navigationParameters )
    {
        await AddViewToSequenceContentAsync(menuItem, activeContentName, navigationParameters);
    }
    #endregion
}
