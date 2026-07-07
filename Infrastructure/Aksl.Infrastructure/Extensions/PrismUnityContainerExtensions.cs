using Prism;
using Prism.Ioc;
using Prism.Unity;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace Aksl.Infrastructure;

public static class PrismUnityContainerExtensions
{
    //private static UnityContainerExtension _unityContainerExtension;
    //public static UnityContainerExtension GetUnityContainerExtension()
    //{
    //    _unityContainerExtension ??= (PrismApplication.Current as PrismApplicationBase).Container.Resolve<UnityContainerExtension>("CurrentContainer");

    //    return _unityContainerExtension;
    //}

    //private static IUnityContainer _unityContainer;
    //public static IUnityContainer GetContainer()
    //{
    //    if (_unityContainer is null)
    //    {
    //        _unityContainer = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<UnityContainerExtension>("CurrentContainer").Instance;
    //    }

    //    return _unityContainer;
    //}
}

