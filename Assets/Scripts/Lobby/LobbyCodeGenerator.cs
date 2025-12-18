using UnityEngine;

namespace Lobby
{
    /// <summary>
    /// Generates unique lobby codes for multiplayer sessions
    /// </summary>
    public static class LobbyCodeGenerator
    {
        // Exclude confusing characters: 0, O, 1, I, L
        private const string ALLOWED_CHARS = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
        private const int CODE_LENGTH = 6;

        /// <summary>
        /// Generate a random lobby code
        /// </summary>
        /// <returns>6-character lobby code</returns>
        public static string GenerateCode()
        {
            char[] code = new char[CODE_LENGTH];
            
            for (int i = 0; i < CODE_LENGTH; i++)
            {
                int randomIndex = Random.Range(0, ALLOWED_CHARS.Length);
                code[i] = ALLOWED_CHARS[randomIndex];
            }
            
            return new string(code);
        }

        /// <summary>
        /// Validates if a lobby code is in the correct format
        /// </summary>
        /// <param name="code">Code to validate</param>
        /// <returns>True if code is valid format</returns>
        public static bool ValidateCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            if (code.Length < 4 || code.Length > 6)
                return false;

            // Check all characters are allowed
            foreach (char c in code.ToUpper())
            {
                if (!ALLOWED_CHARS.Contains(c.ToString()))
                    return false;
            }

            return true;
        }
    }
}
