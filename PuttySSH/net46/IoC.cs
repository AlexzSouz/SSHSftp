using PuttySSHnet46.Providers;
using Microsoft.Practices.Unity;
using System.Net;

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

            _container.RegisterType<IDirectoryManager, DirectoryManagerProvider>();
            _container.RegisterType<IIpAddressValidator, IPAddressValidatorProvider>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to register a new type for a existing one
        /// It does not accept Structs (struct), only Classes (class)
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        public static void RegisterType<TFrom, TTo>() 
            where TFrom : class
            where TTo : class, TFrom
        {
            _container.RegisterType<TFrom, TTo>(new TransientLifetimeManager());
        }

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
