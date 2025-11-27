using BCrypt.Net;

namespace Assignment.Services
{
    /// <summary>
    /// Service for hashing and verifying passwords using the BCrypt algorithm.
    /// BCrypt is a secure password hashing algorithm that automatically handles salting
    /// and is resistant to brute-force attacks. Never store plain text passwords!
    /// </summary>
    public class PasswordService
    {
        /// <summary>
        /// Hashes a plain text password using BCrypt with a work factor of 12.
        /// The work factor determines how computationally expensive the hashing is,
        /// making it harder for attackers to brute-force passwords.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <returns>A BCrypt hash string that includes the salt and can be used for verification.</returns>
        public static string HashPassword(string password)
        {
            // Generate a salt with work factor 12 and hash the password
            // Work factor 12 means 2^12 = 4096 iterations, providing good security
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// Verifies that a plain text password matches a BCrypt hash.
        /// Used during login to check if the user entered the correct password.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <param name="hash">The BCrypt hash to compare against.</param>
        /// <returns>True if the password matches the hash, false otherwise.</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                // BCrypt automatically extracts the salt from the hash and verifies the password
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                // If verification fails (e.g., invalid hash format), return false
                return false;
            }
        }
    }
}

