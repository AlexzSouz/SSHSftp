using System;

namespace PuttySSHnet46.Providers
{
    public abstract class ThreadResetEventSlim
    {
        #region Methods

        public abstract void Reset();
        public abstract void Set();
        public abstract void Wait();
        public abstract void Wait(object cancellationToken);
        public abstract void Wait(TimeSpan timeout);
        public abstract bool Wait(int millisecondsTimeout);
        public abstract bool Wait(TimeSpan timeout, object cancellationToken);
        public abstract bool Wait(int millisecondsTimeout, object cancellationToken);

        #endregion
    }
}
