using Prism.Ioc;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace Prism.Unity
{
    /// <summary>
    /// Extensions help get the underlying <see cref="IUnityContainer" />
    /// </summary>
    public static class PrismIocExtensions
    {
        /// <summary>
        /// Gets the <see cref="IUnityContainer" /> from the <see cref="IContainerProvider" />
        /// </summary>
        /// <param name="containerProvider">The current <see cref="IContainerProvider" /></param>
        /// <returns>The underlying <see cref="IUnityContainer" /></returns>
        public static IUnityContainer GetContainer(this IContainerProvider containerProvider)
        {
            return ((IContainerExtension<IUnityContainer>)containerProvider).Instance;
        }

        /// <summary>
        /// Gets the <see cref="IUnityContainer" /> from the <see cref="IContainerProvider" />
        /// </summary>
        /// <param name="containerRegistry">The current <see cref="IContainerRegistry" /></param>
        /// <returns>The underlying <see cref="IUnityContainer" /></returns>
        public static IUnityContainer GetContainer(this IContainerRegistry containerRegistry)
        {
            return ((IContainerExtension<IUnityContainer>)containerRegistry).Instance;
        }

        public static UnityContainerExtension GetUnityContainerExtension()
        {
            var unityContainerExtension = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<UnityContainerExtension>("CurrentContainer");
            return unityContainerExtension;
        }

        //public static IUnityContainer GetContainer()
        //{
        //    var unityContainerExtension = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<UnityContainerExtension>("CurrentContainer");
        //    return unityContainerExtension.Instance;
        //}

        private static IUnityContainer _unityContainer;
        public static IUnityContainer GetUnityContainer()
        {
            if (_unityContainer is null)
            {
                _unityContainer = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<UnityContainerExtension>("CurrentContainer").Instance;
            }

            return _unityContainer;
        }
    }
}
