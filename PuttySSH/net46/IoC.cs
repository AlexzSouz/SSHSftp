using PuttySSHnet46.Providers;
using Microsoft.Practices.Unity;
using ConsoleApp;
using System;

namespace PuttySSHnet46
{
    public class IoC
    {
        #region Fields

        private static readonly IUnityContainer _container;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor used to register dependencies
        /// </summary>
        static IoC()
        {
            _container = new UnityContainer();

            // Register
            _container.RegisterType<ThreadResetEventSlim, ManualResetEventSlimProvider>();
            _container.RegisterType<IDirectoryManager, DirectoryManager>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resolve a dependency by <T> type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ResolveType<T>()
        {
            return _container.Resolve<T>();
        }

        /// <summary>
        /// Resolve a dependency overriden <T> type with <TDependency>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDependency"></typeparam>
        /// <returns></returns>
        public static TDependency ResolveTypeOverride<T, TDependency>() where TDependency : new()
        {
            var type = new TDependency();

            // TODO : Validate if <TDependency> is of type <T>, if it implements <T>

            return type;
        }

        #endregion
    }
}
