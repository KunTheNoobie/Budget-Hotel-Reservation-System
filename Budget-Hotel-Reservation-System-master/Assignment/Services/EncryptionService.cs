using System.Security.Cryptography;
using System.Text;

namespace Assignment.Services
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data using AES (Advanced Encryption Standard) algorithm.
    /// Used to protect sensitive information such as phone numbers stored in the database.
    /// Uses AES-256 encryption with a configurable encryption key.
    /// </summary>
    public class EncryptionService
    {
        /// <summary>
        /// Static encryption key used for AES encryption/decryption.
        /// In a real production app, this key should be stored in a secure vault or environment variable.
        /// For this assignment, we use a key from appsettings.json (minimum 32 characters for AES-256).
        /// </summary>
        private static string _key;

        /// <summary>
        /// Initializes the encryption service with the provided encryption key.
        /// Must be called at application startup (typically in Program.cs).
        /// </summary>
        /// <param name="key">The encryption key (must be at least 32 characters long for AES-256).</param>
        /// <exception cref="ArgumentException">Thrown if the key is null, empty, or less than 32 characters.</exception>
        public static void Initialize(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 32)
            {
                throw new ArgumentException("Encryption key must be at least 32 characters long.");
            }
            _key = key;
        }

        /// <summary>
        /// Gets the encryption key, ensuring it has been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if Initialize() has not been called.</exception>
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

        /// <summary>
        /// Encrypts a plain text string using AES encryption.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <returns>Base64-encoded encrypted string. Returns the original string if input is null or empty.</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            // Initialize IV (Initialization Vector) - using zero IV for simplicity
            // In production, use a random IV and prepend it to the ciphertext
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                // Set encryption key and IV
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = iv;

                // Create encryptor
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Encrypt the plain text
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

            // Return base64-encoded encrypted data
            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Decrypts an encrypted string using AES decryption.
        /// </summary>
        /// <param name="cipherText">The base64-encoded encrypted text to decrypt.</param>
        /// <returns>Decrypted plain text. Returns the original string if decryption fails or input is null/empty.</returns>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                // Initialize IV (must match the IV used during encryption)
                byte[] iv = new byte[16];
                // Decode base64-encoded ciphertext
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    // Set decryption key and IV
                    aes.Key = Encoding.UTF8.GetBytes(Key);
                    aes.IV = iv;
                    // Create decryptor
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    // Decrypt the ciphertext
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
                // If decryption fails (e.g., invalid format, wrong key), return original text
                // This allows the system to handle legacy data or corrupted entries gracefully
                return cipherText; 
            }
        }
    }
}
