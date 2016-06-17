using System;

namespace PuttySSHnet46.Providers
{
    public class DirectoryManagerProvider : IDirectoryManager
    {
        #region Methods

        public string[] SplitPath(string path)
        {
            return path.Split(new[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    }
}
