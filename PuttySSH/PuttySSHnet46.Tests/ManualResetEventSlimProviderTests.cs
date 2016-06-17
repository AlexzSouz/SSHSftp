using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuttySSHnet46.Providers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PuttySSHnet46.Tests
{
    [TestClass]
    public class ManualResetEventSlimProviderTests
    {
        [TestMethod]
        public void ResetSlimTests()
        {
            // Arrange
            var ioc = new IoC();
            var eventLocker = (ManualResetEventSlimProvider)IoC.ResolveType<ThreadResetEventSlim>();

            bool wasEventLockerSet = false;

            Action waitResetClosure = () =>
            {
                while (eventLocker.IsSet)
                {
                    // NOOP
                }
            };

            // Act
            Task.Run(() =>
            {
                Thread.Sleep(2000);
                eventLocker.Set();
                wasEventLockerSet = true;

                Thread.Sleep(2000);
                eventLocker.Reset();
            });
            
            eventLocker.Wait();

            waitResetClosure();

            // Assert
            Assert.IsNotNull(eventLocker);
            Assert.IsTrue(wasEventLockerSet);
            Assert.IsFalse(eventLocker.IsSet);
        }

        [TestMethod]
        public void SetSlimTests()
        {
            // NOOP
        }

        [TestMethod]
        public void WaitParameterlessTest()
        {
            // NOOP
        }

        [TestMethod]
        public void WaitCancellationTokenTest()
        {
            // NOOP
        }

        [TestMethod]
        public void WaitTimeSpanTest()
        {
            // NOOP
        }

        [TestMethod]
        public void WaitMillisecondsTimeoutTest()
        {
            // NOOP
        }

        [TestMethod]
        public void WaitTimeSpanAndCancellationTokenTest()
        {
            // NOOP
        }

        [TestMethod]
        public void WaitMillisecondsAndCancellationTokenTest()
        {
            // NOOP
        }
    }
}
