using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuttySSHnet46.Providers
{
    public class ManualResetEventSlimProvider : ThreadResetEventSlim
    {
        #region Fields & Properties

        private readonly ManualResetEventSlim _manualResetEventSlim;

        public bool IsSet {
            get {
                if (_manualResetEventSlim != null)
                {
                    return _manualResetEventSlim.IsSet;
                }

                return default(bool);
            }
        }

        public int SpinCount {
            get
            {
                if (_manualResetEventSlim != null)
                {
                    return _manualResetEventSlim.SpinCount;
                }

                return default(int);
            }
        }

        public WaitHandle WaitHandle {
            get
            {
                if (_manualResetEventSlim != null)
                {
                    return _manualResetEventSlim.WaitHandle;
                }

                return null;
            }
        }

        #endregion

        #region Constructors

        public ManualResetEventSlimProvider()
        {
            _manualResetEventSlim = new ManualResetEventSlim();
        }

        #endregion

        #region Methods

        public override void Reset()
        {
            _manualResetEventSlim.Reset();
        }

        public override void Set()
        {
            _manualResetEventSlim.Set();
        }

        public override void Wait()
        {
            _manualResetEventSlim.Wait();
        }

        public override void Wait(object cancellationToken)
        {
            _manualResetEventSlim.Wait((CancellationToken)cancellationToken);
        }

        public override void Wait(TimeSpan timeout)
        {
            _manualResetEventSlim.Wait(timeout);
        }

        public override bool Wait(int millisecondsTimeout)
        {
            return _manualResetEventSlim.Wait(millisecondsTimeout);
        }

        public override bool Wait(TimeSpan timeout, object cancellationToken)
        {
            return _manualResetEventSlim.Wait(timeout, (CancellationToken)cancellationToken);
        }

        public override bool Wait(int millisecondsTimeout, object cancellationToken)
        {
            return _manualResetEventSlim.Wait(millisecondsTimeout, (CancellationToken)cancellationToken);
        }
        
        #endregion
    }
}
