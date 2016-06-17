using PuttySSHnet46.Providers;
using System;

namespace PuttySSHnet46
{
    public abstract class SshSftpBase
    {
        #region Fields

        protected object _locker = new object();
        protected string _setDirectory = string.Empty;

        protected IDirectoryManager _directoryManager;
        protected IIpAddressValidator _ipAddressValidator;

        protected readonly Func<ThreadResetEventSlim> GetThreadResetEventSlim =
            () => IoC.ResolveType<ThreadResetEventSlim>();
        
        #endregion
    }
}
