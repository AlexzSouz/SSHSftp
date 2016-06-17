using PuttySSHnet46.Exceptions;
using System;
using System.Diagnostics;
using System.Net;
using PuttySSHnet46.Providers;
using PuttySSHnet46.Enums;

namespace PuttySSHnet46
{
    public class SshSftp
    {
        #region Fields

        object _locker = new object();
        string _setDirectory = string.Empty;

        private readonly Process _processSession;
        private readonly IDirectoryManager _directoryManager;

        protected readonly Func<ThreadResetEventSlim> GetThreadResetEventSlim =
            () => IoC.ResolveType<ThreadResetEventSlim>();

        internal Func<string, bool> IsIPAddressValid = (ipAddress) =>
        {
            IPAddress validIpAddress;
            IPAddress.TryParse(ipAddress, out validIpAddress);

            return validIpAddress != null;
        };

        public Process ProccessSession => _processSession;

        #endregion

        #region Constructors

        public SshSftp()
        {
            // Manage dependencies in here
            _directoryManager = IoC.ResolveType<IDirectoryManager>();
        }
        
        public SshSftp(string ipAddress, string username, string password)
            : this()
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (!IsIPAddressValid(ipAddress))
            {
                throw new InvalidIPAddressException(ipAddress);
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // Executes a SSH OPEN
            var startInfo = new ProcessStartInfo
            {
                FileName = "PSFTP.EXE",
                Arguments = $@"-l {username} -pw {password} {ipAddress}",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _processSession = Process.Start(startInfo);

            if (_processSession == null)
            {
                throw new NullReferenceException("The session is null");
            }
        }
        
        public SshSftp(string ipAddress, string username, string password, string localPath)
            : this (ipAddress, username, password)
        {
            if (string.IsNullOrWhiteSpace(localPath))
            {
                throw new ArgumentNullException(nameof(localPath));
            }

            SetLocalDirectory(localPath);
        }

        ~SshSftp()
        {
            //_processSession.StandardInput.WriteLine("quit");
            _processSession.WaitForExit();
            _processSession.Close();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Thread-Safe method to set the LOCAL directory path
        /// </summary>
        /// <param name="directoryPath"></param>
        public void SetLocalDirectory(string directoryPath)
        {
            lock (_locker)
            {
                bool isPathNotFoundException = false;
                string exceptionMessage = string.Empty;

                _processSession.BeginOutputReadLine();

                var localResetEventSlim = GetThreadResetEventSlim();
                processDataReceived(data =>
                {
                    if (data.Contains("unable to change directory:"))
                    {
                        isPathNotFoundException = true;
                        exceptionMessage = data;
                    }

                    localResetEventSlim.Set();
                });

                _processSession.StandardInput.WriteLine($"lcd {directoryPath}");
                localResetEventSlim.Wait();
                
                _processSession.CancelOutputRead();

                if (isPathNotFoundException)
                {
                    throw new LocalDirectoryNotFoundException(exceptionMessage);
                }
            }
        }
        
        /// <summary>
        /// Thread-Safe method to load a Directory or a File (Explicit) as directory
        /// </summary>
        /// <param name="directoryPath">Path to load</param>
        public void LoadDirectory(string directoryPath, SftpPathType pathType = SftpPathType.DirectoryPath)
        {
            lock (_locker)
            {
                if (pathType == SftpPathType.DirectoryPath)
                {
                    _setDirectory = directoryPath;
                }

                string[] pathsVet = _directoryManager.SplitPath(directoryPath);

                foreach (string path in pathsVet)
                {
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

                    _processSession.StandardInput.WriteLine($"cd {path}");
                    localResetEventSlim.Wait();

                    if (pathType == SftpPathType.FilePath)
                    {
                        _processSession.StandardInput.WriteLine($"cd {_setDirectory}");
                        localResetEventSlim.Wait();
                    }

                    _processSession.CancelOutputRead();

                    if (isPathNotFoundException)
                    {
                        throw new SftpPathNotFoundException(exceptionMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Thread-Safe method to verify is Directory or File exists
        /// </summary>
        /// <param name="fileOrDirectoryPath">File or Directory path for validation</param>
        /// <returns></returns>
        public bool Exists(string fileOrDirectoryPath, SftpPathType pathType)
        {
            lock (_locker)
            {
                if (string.IsNullOrWhiteSpace(fileOrDirectoryPath))
                {
                    throw new ArgumentNullException(nameof(fileOrDirectoryPath));
                }

                try
                {
                    LoadDirectory(fileOrDirectoryPath, pathType);
                    return true;
                }
                catch (Exception ex)
                {
                    // TODO : Add logging in here using a logging provider
                    return false;
                } 
            }
        }

        /// <summary>
        /// Thread-Sage method to close SSH connection
        /// </summary>
        public void Close()
        {
            lock (_locker)
            {
                if (_processSession.HasExited)
                {
                    return;
                }

                _processSession.StandardInput.WriteLine("quit"); 
            }
        }

        /// <summary>
        /// Thread-Safe method to Download a file to LOCAL Environment Directory
        /// </summary>
        /// <param name="filePath"></param>
        public void Download(string filePath)
        {
            lock (_locker)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentNullException(nameof(filePath));
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
                
                _processSession.StandardInput.WriteLine($"get {filePath}");
                localResetEventSlim.Wait();

                _processSession.CancelOutputRead();

                if (isPathNotFoundException)
                {
                    throw new SftpPathNotFoundException(exceptionMessage);
                }
            }
        }

        /// <summary>
        /// Thread-Safe method to Download a file to a specified LOCAL Directory
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="localDirectory"></param>
        public void Download(string filePath, string localDirectory)
        {
            lock (_locker)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentNullException(nameof(filePath));
                }

                if (string.IsNullOrWhiteSpace(localDirectory))
                {
                    throw new ArgumentNullException(nameof(localDirectory));
                }

                SetLocalDirectory(localDirectory);
                Download(filePath);
            }
        }

        public void DownloadMany(string filePath, string pattern = null)
        {
            // MGET
        }

        public void DownloadMany(string[] filesPath, string pattern = null)
        {
            // MGET
        }

        private void processDataReceived(Action<string> callback)
        {
            _processSession.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                callback(e.Data);
            };
        }
        
        #endregion
    }
}