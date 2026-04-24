namespace EliteAcademy.Application.Interfaces
{
    public interface IConfigEncryptor
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
