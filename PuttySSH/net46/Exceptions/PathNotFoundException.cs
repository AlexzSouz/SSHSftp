using System;
using System.IO;
using System.Runtime.Serialization;

namespace PuttySSHnet46.Exceptions
{
    public class SftpPathNotFoundException : IOException
    {
        public SftpPathNotFoundException()
            : base ()
        {
            // NOOP
        }

        public SftpPathNotFoundException(string message)
            : base (message)
        {
            // NOOP
        }
        
        public SftpPathNotFoundException(string message, int hresult)
            : base(message, hresult)
        {
            // NOOP
        }

        public SftpPathNotFoundException(string message, Exception exception)
            : base (message, exception)
        {
            // NOOP
        }
        
        public SftpPathNotFoundException(SerializationInfo sInfo, StreamingContext context)
            : base(sInfo, context)
        {
            // NOOP
        }
    }
}
