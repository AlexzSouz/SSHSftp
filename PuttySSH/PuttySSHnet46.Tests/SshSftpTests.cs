using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuttySSHnet46.Enums;
using PuttySSHnet46.Providers;
using Moq;

namespace PuttySSHnet46.Tests
{
    [TestClass]
    public class SshSftpTests
    {
        [TestMethod]
        public void DownloadSingleDocument_ToEnvironmentLocalPath_Tests()
        {
            // Arrange
            string filePath = "/Folder/SubFolder/invoice-sftp.pdf";

            bool fileExists = false,
                isConnected = false,
                isDependencyOverriden = false;

            var sftpMock = new Mock<SshSftpBase>();

            sftpMock
                .Setup(a => a.Connect())
                .Returns(default(IDisposable))
                .Callback(() =>
                {
                    isConnected = true;
                });

            sftpMock
                .Setup(a => a.OverrideProvider<IIpAddressValidator, IPAddressValidatorMockProvider>())
                .Callback(() => {
                    isDependencyOverriden = true;
                });

            sftpMock
                .Setup(a => a.Exists(filePath, SftpPathType.FilePath))
                .Returns(true);

            var sftp = sftpMock.Object;

            // Act
            sftp.OverrideProvider<IIpAddressValidator, IPAddressValidatorMockProvider>();
            using (sftp.Connect())
            {
                if (fileExists = sftp.Exists(filePath, SftpPathType.FilePath))
                {
                    sftp.Download(filePath);
                }
            }

            // Assert
            Assert.IsTrue(fileExists);
            Assert.IsTrue(isConnected);
            Assert.IsTrue(isDependencyOverriden);
        }

        [TestMethod]
        public void DownloadSingleDocument_WithDependencyOverriden_ToEnvironmentLocalPath_Tests()
        {
            // Arrange
            string filePath = "/Folder/SubFolder/invoice-sftp.pdf";

            bool fileExists = false,
                isConnected = false,
                isDependencyOverriden = false,
                isDocDownloaded = false;
            
            var sshSftpMock = new Mock<SshSftpBase>();

            sshSftpMock
                .Setup((a) => a.Connect())
                .Returns(default(IDisposable))
                .Callback(() =>
                {
                    isConnected = true;
                });

            sshSftpMock
                .Setup(a => a.OverrideProvider<IIpAddressValidator, IPAddressValidatorMockProvider>())
                .Callback(() => {
                    isDependencyOverriden = true;
                });

            sshSftpMock
                .Setup(a => a.Exists(filePath, SftpPathType.FilePath))
                .Returns(true);
            
            sshSftpMock
                .Setup(a => a.Download(filePath))
                .Callback(() =>
                {
                    isDocDownloaded = true;
                });

            var sftp = sshSftpMock.Object;

            // Act
            sftp.OverrideProvider<IIpAddressValidator, IPAddressValidatorMockProvider>();
            using (sftp.Connect())
            {
                if (fileExists = sftp.Exists(filePath, SftpPathType.FilePath))
                {
                    sftp.Download(filePath);
                }
            }

            // Assert
            Assert.IsTrue(fileExists);
            Assert.IsTrue(isConnected);
            Assert.IsTrue(isDependencyOverriden);
            Assert.IsTrue(isDocDownloaded);
        }

        [TestMethod]
        public void DownloadSingleDocument_ToSpecifiedPathTests()
        {
            // Arrage
            string filePath = "/Folder/SubFolder/invoice-sftp.pdf";
            string localPath = @"C:\Folder\SubFolder";

            bool isDisposed = false,
                isLocalPathSet = false,
                isDocDownloaded = false;

            var sshSftpMock = new Mock<SshSftpBase>();
            sshSftpMock
                .Setup((a) => a.Connect())
                .Returns(default(IDisposable))
                .Callback(() =>
                {
                    isDisposed = true;
                });

            sshSftpMock
                .Setup(a => a.Exists(filePath, SftpPathType.FilePath))
                .Returns(true);

            sshSftpMock
                .Setup(a => a.SetLocalDirectory(localPath))
                .Callback(() =>
                {
                    isLocalPathSet = true;
                });

            sshSftpMock
                .Setup(a => a.Download(filePath))
                .Callback(() =>
                {
                    isDocDownloaded = true;
                });

            var sftp = sshSftpMock.Object;

            // Act
            using (sftp.Connect())
            {
                sftp.SetLocalDirectory(localPath);
                sftp.Download(filePath);
            }

            // Assert
            Assert.IsTrue(isDisposed);
            Assert.IsTrue(isLocalPathSet);
            Assert.IsTrue(isDocDownloaded);
        }

        [TestMethod]
        public void IsDirectoryPath_ValidatedTests()
        {
            // Arrage
            string dirPath = @"/Fold1/SubF\Chld";

            // Act
            string[] fodlers = dirPath.Split(new[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);

            // Assert
            Assert.IsTrue(fodlers.Length > 0);
        }

        [TestMethod]
        [Description("Integration Test")]
        public void DownloadSingleDocument_ToEnvironmentLocal_IntegrationTests()
        {
            // Arrange
            bool fileExists = false;
            var sftp = new SshSftp("192.168.0.221", "UserNn", "PassWd");
            sftp.OverrideProvider<IIpAddressValidator, IPAddressValidatorMockProvider>();

            // Act
            using (sftp.Connect())
            {
                sftp.LoadDirectory("/Folder/SubFolder/tests/");

                if (fileExists = sftp.Exists("1645d0ea-8374-4f5a-9949-bea70608a1f7.pdf", SftpPathType.FilePath))
                {
                    sftp.Download("1645d0ea-8374-4f5a-9949-bea70608a1f7.pdf");
                }
            }

            // Assert
            Assert.IsTrue(fileExists);
        }
    }
}
