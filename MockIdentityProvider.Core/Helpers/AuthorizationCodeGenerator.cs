namespace MockIdentityProvider.Core.Helpers
{
    using System.Security.Cryptography;

    public static class AuthorizationCodeGenerator
    {
        public static string Generate()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);

            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
