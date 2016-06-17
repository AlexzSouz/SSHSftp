using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuttySSHnet46.Enums
{
    public enum SftpPathType
    {
        FilePath = 1,
        DirectoryPath
    }

    public enum SftpDownloadType
    {
        [Command("get")]
        Single = 1,
        [Command("mget")]
        Multi = 2
    }

    class Command : Attribute
    {
        #region Fields

        private readonly string _cmd;

        #endregion

        #region Constructor

        public Command(string cmd)
        {
            _cmd = cmd;
        }

        #endregion
    }
}
