using System.Security.Cryptography;
using System.Text;

namespace Assignment.Services
{
    public class EncryptionService
    {
        // In a real production app, this key should be stored in a secure vault or environment variable.
        // For this assignment, we use a hardcoded 32-byte key (AES-256).
        private static string _key;

        public static void Initialize(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 32)
            {
                throw new ArgumentException("Encryption key must be at least 32 characters long.");
            }
            _key = key;
        }

        private static string Key 
        {
            get
            {
                if (string.IsNullOrEmpty(_key))
                {
                    throw new InvalidOperationException("EncryptionService has not been initialized. Call Initialize() at startup.");
                }
                return _key;
            }
        } 

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(Key);
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // If decryption fails (e.g. invalid format), return original text or empty
                return cipherText; 
            }
        }
    }
}
