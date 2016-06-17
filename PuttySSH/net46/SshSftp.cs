using PuttySSHnet46.Exceptions;
using System;
using System.Diagnostics;
using System.Net;
using PuttySSHnet46.Providers;
using PuttySSHnet46.Enums;

namespace PuttySSHnet46
{
    public sealed class SshSftp : SshSftpBase, IDisposable
    {
        #region Fields

        private Process _processSession;

        private string _ipAddress;
        private string _username;
        private string _password;

        /// <summary>
        /// Validate IP address received for connection
        /// </summary>
        internal Func<string, bool> IsValidIPAddress = (ipAddress) =>
        {
            return IoC.ResolveType<IIpAddressValidator>().IsValid(ipAddress);
        };

        #endregion

        #region Constructors

        public SshSftp()
        {
            // Manage provided Dependencies in here
            _directoryManager = IoC.ResolveType<IDirectoryManager>();
        }

        public SshSftp(string ipAddress, string username, string password)
            : this()
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            _ipAddress = ipAddress;
            _username = username;
            _password = password;
        }

        public SshSftp(string ipAddress, string username, string password, string localPath)
            : this(ipAddress, username, password)
        {
            if (string.IsNullOrWhiteSpace(localPath))
            {
                throw new ArgumentNullException(nameof(localPath));
            }

            SetLocalDirectory(localPath);
        }
        
        ~SshSftp()
        {
            this.Dispose();
        }

        #endregion

        #region Methods

        private void processDataReceived(Action<string> callback)
        {
            _processSession.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                callback(e.Data);
            };
        }
        
        private void download(string path, string pattern = null)
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

        /// <summary>
        /// IDisposable method to initialize a SSH connection
        /// SSH Connection [Simple]: PSFTP.EXE -l {username} -pw {password} {192.168.0.1}
        /// </summary>
        /// <returns></returns>
        public IDisposable Connect()
        {
            // TODO : Move it from here
            if (!IsValidIPAddress(_ipAddress))
            {
                throw new InvalidIPAddressException(_ipAddress);
            }

            // TODO : Decouple ProcessStartInfo and Process

            // Executes a SSH OPEN
            var startInfo = new ProcessStartInfo
            {
                FileName = "PSFTP.EXE",
                Arguments = $@"-l {_username} -pw {_password} {_ipAddress}",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _processSession = Process.Start(startInfo);

            if (_processSession == null)
            {
                throw new NullReferenceException("The session is null");
            }

            return this;
        }
        
        /// <summary>
        /// Thread-Sage method to close SSH connection
        /// </summary>
        public void Close()
        {
            lock (_locker)
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// Method to override default Dependencies used by SshSftp
        /// </summary>
        /// <typeparam name="TBase"></typeparam>
        /// <typeparam name="TProvider"></typeparam>
        public void OverrideProvider<TBase, TProvider>()
            where TBase : class
            where TProvider : class, TBase
        {
            IoC.RegisterType<TBase, TProvider>();
        }

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
        /// Thread-Safe method to Download a file to LOCAL Environment Directory
        /// </summary>
        /// <param name="filePath"></param>
        public void Download(string filePath)
        {
            lock (_locker)
            {
                // Protective programming (Fail-Fast) not required in here
                // as it's a single file and private method download handles it

                download(filePath);
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
                download(filePath);
            }
        }
        
        /// <summary>
        /// Thread-Safe method to Download several file, 
        /// It either download files respecting the pattern or not based on the file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="localDirectory"></param>
        public void DownloadMany(string[] filesPath, string pattern = null)
        {
            lock (_locker)
            {
                if(filesPath == null)
                {
                    throw new ArgumentNullException(nameof(filesPath));
                }
                
                foreach (var path in filesPath)
                {
                    download(path, pattern);
                }
            }
        }

        /// <summary>
        /// Thread-Safe method to Download several file, 
        /// It either download files respecting the pattern or not based on the file path
        /// Download files to a specified LOCAL Directory
        /// </summary>
        /// <param name="filesPath"></param>
        /// <param name="localDirectory"></param>
        /// <param name="pattern"></param>
        public void DownloadMany(string[] filesPath, string localDirectory, string pattern = null)
        {
            lock (_locker)
            {
                if (filesPath == null)
                {
                    throw new ArgumentNullException(nameof(filesPath));
                }

                SetLocalDirectory(localDirectory);

                foreach (var path in filesPath)
                {
                    download(path, pattern);
                }
            }
        }

        /// <summary>
        /// Method to Dispose object from memory
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_processSession != null && !_processSession.HasExited)
                {
                    _processSession.StandardInput.WriteLine("quit");
                    
                    _processSession.Close();
                    _processSession.Dispose();
                }
            }
            catch (Exception)
            {
                // NOOP : No process is associated with this object anymore.
            }
        }

        #endregion
    }
}