using System.Security.Cryptography;

namespace MockIdentityProvider.Infrastructure.Security
{
    public static class RsaKeyProvider
    {
        private static readonly RSA _rsa;

        public const string KeyId = "dev-rsa-key";

        static RsaKeyProvider()
        {
            _rsa = RSA.Create(2048);
        }

        public static RSA Instance => _rsa;
    }
}
