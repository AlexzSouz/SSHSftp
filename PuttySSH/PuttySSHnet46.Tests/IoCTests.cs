using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuttySSHnet46.Providers;
using Moq;

namespace PuttySSHnet46.Tests
{
    [TestClass]
    public class IoCTests
    {
        [TestMethod]
        public void ResolveTypeTests()
        {
            // Arrange
            var ioc = new IoC();

            // Act
            var type = IoC.ResolveType<ThreadResetEventSlim>();

            // Assert
            Assert.AreEqual(typeof(ManualResetEventSlimProvider), type.GetType());
        }

        [TestMethod]
        public void ResolveTypeOverrideTests()
        {
            // Arrange
            var ioc = new IoC();
            
            // Act
            var type = IoC.ResolveTypeOverride<ThreadResetEventSlim, Mock<ThreadResetEventSlim>>();

            // Assert
            Assert.AreEqual(typeof(Mock<ThreadResetEventSlim>), type.GetType());
        }
    }
}
