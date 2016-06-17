using System.Net;

namespace PuttySSHnet46.Providers
{
    public class IPAddressValidatorProvider : IIpAddressValidator
    {
        public bool IsValid(string ipAddress)
        {
            IPAddress ipAddressCast;
            IPAddress.TryParse(ipAddress, out ipAddressCast);

            return ipAddressCast != null;
        }
    }

    public class IPAddressValidatorMockProvider : IIpAddressValidator
    {
        public bool IsValid(string ipAddress)
        {
            return true;
        }
    }
    
    public interface IIpAddressValidator
    {
        bool IsValid(string ipAddress);
    }
}