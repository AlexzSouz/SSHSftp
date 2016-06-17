using System;
using System.IO;
using System.Runtime.Serialization;

namespace PuttySSHnet46.Exceptions
{
    public class LocalDirectoryNotFoundException : IOException
    {
        public LocalDirectoryNotFoundException()
            : base ()
        {
            // NOOP
        }

        public LocalDirectoryNotFoundException(string message)
            : base (message)
        {
            // NOOP
        }

        public LocalDirectoryNotFoundException(string message, int hresult)
            : base(message, hresult)
        {
            // NOOP
        }

        public LocalDirectoryNotFoundException(string message, Exception exception)
            : base (message, exception)
        {
            // NOOP
        }

        public LocalDirectoryNotFoundException(SerializationInfo sInfo, StreamingContext context)
            : base(sInfo, context)
        {
            // NOOP
        }
    }
}
