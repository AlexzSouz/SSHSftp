using PuttySSHnet46.Enums;
using PuttySSHnet46.Exceptions;
using PuttySSHnet46.Providers;
using System;
using System.Diagnostics;

namespace PuttySSHnet46
{
    public abstract class SshSftpBase
    {
        #region Fields
        
        protected object _locker = new object();
        protected string _setDirectory = string.Empty;

        // TODO : Move this dependency from here?
        protected Process _processSession;

        protected IDirectoryManager _directoryManager;
        protected IIpAddressValidator _ipAddressValidator;

        protected readonly Func<ThreadResetEventSlim> GetThreadResetEventSlim =
            () => IoC.ResolveType<ThreadResetEventSlim>();

        #endregion

        #region Methods

        protected void processDataReceived(Action<string> callback)
        {
            _processSession.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                callback(e.Data);
            };
        }

        protected void download(string path, string pattern = null)
        {
            lock (_locker)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentNullException(nameof(path));
                }

                bool isPathNotFoundException = false;
                string exceptionMessage = string.Empty;

                _processSession.BeginOutputReadLine();
                var localResetEventSlim = GetThreadResetEventSlim();

                processDataReceived(data =>
                {
                    if (data.Contains("no such file or directory"))
                    {
                        isPathNotFoundException = true;
                        exceptionMessage = data;
                    }

                    localResetEventSlim.Set();
                });

                var command = (pattern == null) ? $"get {path}" : $"mget {path}/{pattern}";

                _processSession.StandardInput.WriteLine(command);
                localResetEventSlim.Wait();

                _processSession.CancelOutputRead();

                if (isPathNotFoundException)
                {
                    throw new SftpPathNotFoundException(exceptionMessage);
                }
            }
        }

        public abstract IDisposable Connect();

        public abstract void Close();

        public abstract void OverrideProvider<TBase, TProvider>()
            where TBase : class
            where TProvider : class, TBase;

        public abstract void SetLocalDirectory(string directoryPath);

        public abstract void LoadDirectory(string directoryPath, SftpPathType pathType = SftpPathType.DirectoryPath);

        public abstract bool Exists(string fileOrDirectoryPath, SftpPathType pathType);

        public abstract void Download(string filePath);

        public abstract void Download(string filePath, string localDirectory);

        public abstract void DownloadMany(string[] filesPath, string pattern = null);

        public abstract void DownloadMany(string[] filesPath, string localDirectory, string pattern = null);

        #endregion
    }
}
