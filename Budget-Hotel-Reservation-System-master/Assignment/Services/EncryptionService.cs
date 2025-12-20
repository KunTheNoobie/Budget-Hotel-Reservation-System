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
        /// Used to encrypt sensitive data before storing in database (e.g., phone numbers).
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <returns>Base64-encoded encrypted string. Returns the original string if input is null or empty.</returns>
        public static string Encrypt(string plainText)
        {
            // ========== INPUT VALIDATION ==========
            // If input is null or empty, return as-is (no encryption needed)
            if (string.IsNullOrEmpty(plainText)) return plainText;

            // ========== INITIALIZE ENCRYPTION ==========
            // Initialize IV (Initialization Vector) - using zero IV for simplicity
            // In production, use a random IV and prepend it to the ciphertext for better security
            // IV ensures same plaintext produces different ciphertext each time
            byte[] iv = new byte[16];  // 16 bytes = 128 bits (AES block size)
            byte[] array;               // Will store the encrypted bytes

            // ========== AES ENCRYPTION PROCESS ==========
            // Create AES encryption algorithm instance
            using (Aes aes = Aes.Create())
            {
                // ========== SET ENCRYPTION PARAMETERS ==========
                // Set encryption key (converted from string to bytes)
                // Key length must match AES key size (256 bits = 32 bytes)
                aes.Key = Encoding.UTF8.GetBytes(Key);
                
                // Set IV (Initialization Vector) for encryption
                // IV adds randomness to encryption (same plaintext → different ciphertext)
                aes.IV = iv;

                // ========== CREATE ENCRYPTOR ==========
                // Create encryptor transform using the key and IV
                // This transform will perform the actual encryption
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // ========== PERFORM ENCRYPTION ==========
                // Encrypt the plain text using streams
                // MemoryStream: Stores encrypted bytes in memory
                // CryptoStream: Performs encryption as data flows through
                // StreamWriter: Writes plain text to the encryption stream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            // Write plain text to encryption stream
                            // CryptoStream automatically encrypts the data
                            streamWriter.Write(plainText);
                        }

                        // Get encrypted bytes from memory stream
                        array = memoryStream.ToArray();
                    }
                }
            }

            // ========== RETURN BASE64-ENCODED RESULT ==========
            // Convert encrypted bytes to Base64 string for storage
            // Base64 encoding makes the encrypted data safe for storage in database (text format)
            // Example: Encrypted phone number stored as "aBc123XyZ..." instead of binary data
            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Decrypts an encrypted string using AES decryption.
        /// Used to decrypt sensitive data retrieved from database (e.g., phone numbers for display).
        /// </summary>
        /// <param name="cipherText">The base64-encoded encrypted text to decrypt.</param>
        /// <returns>Decrypted plain text. Returns the original string if decryption fails or input is null/empty.</returns>
        public static string Decrypt(string cipherText)
        {
            // ========== INPUT VALIDATION ==========
            // If input is null or empty, return as-is (nothing to decrypt)
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                // ========== INITIALIZE DECRYPTION ==========
                // Initialize IV (must match the IV used during encryption)
                // Using zero IV to match encryption (for simplicity)
                byte[] iv = new byte[16];  // 16 bytes = 128 bits (AES block size)
                
                // ========== DECODE BASE64 ==========
                // Decode base64-encoded ciphertext back to bytes
                // Base64 was used for storage, now we need bytes for decryption
                byte[] buffer = Convert.FromBase64String(cipherText);

                // ========== AES DECRYPTION PROCESS ==========
                // Create AES decryption algorithm instance
                using (Aes aes = Aes.Create())
                {
                    // ========== SET DECRYPTION PARAMETERS ==========
                    // Set decryption key (must match encryption key)
                    // Key length must match AES key size (256 bits = 32 bytes)
                    aes.Key = Encoding.UTF8.GetBytes(Key);
                    
                    // Set IV (must match the IV used during encryption)
                    aes.IV = iv;
                    
                    // ========== CREATE DECRYPTOR ==========
                    // Create decryptor transform using the key and IV
                    // This transform will perform the actual decryption
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    // ========== PERFORM DECRYPTION ==========
                    // Decrypt the ciphertext using streams
                    // MemoryStream: Reads encrypted bytes from buffer
                    // CryptoStream: Performs decryption as data flows through
                    // StreamReader: Reads decrypted plain text from the decryption stream
                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                // Read decrypted plain text from decryption stream
                                // CryptoStream automatically decrypts the data
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // ========== ERROR HANDLING ==========
                // If decryption fails (e.g., invalid format, wrong key, corrupted data), return original text
                // This allows the system to handle legacy data or corrupted entries gracefully
                // In production, you might want to log this error for investigation
                return cipherText; 
            }
        }
    }
}
