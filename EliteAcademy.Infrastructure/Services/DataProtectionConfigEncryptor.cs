using EliteAcademy.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace EliteAcademy.Infrastructure.Services
{
    public class DataProtectionConfigEncryptor : IConfigEncryptor
    {
        private readonly IDataProtector _protector;

        public DataProtectionConfigEncryptor(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("EliteAcademy.GatewayConfig");
        }

        public string Encrypt(string plainText) => _protector.Protect(plainText);
        public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
    }
}
