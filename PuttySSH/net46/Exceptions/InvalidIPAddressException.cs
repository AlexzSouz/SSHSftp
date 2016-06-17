using System;

namespace PuttySSHnet46.Exceptions
{
    public class InvalidIPAddressException : Exception
    {
        public InvalidIPAddressException()
            : base ()
        {
        }

        public InvalidIPAddressException(string message)
            : base (message)
        {
        }

        public InvalidIPAddressException(string message, Exception exception)
            : base (message, exception)
        {
        }
    }
}
