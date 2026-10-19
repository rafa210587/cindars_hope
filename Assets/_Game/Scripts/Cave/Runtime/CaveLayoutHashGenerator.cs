using System.Text;
using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public class CaveLayoutHashGenerator
    {
        public static string GenerateHash(CaveGeneratedLevel generatedLevel)
        {
            if (generatedLevel == null)
            {
                return "null_level";
            }

            // CaveGeneratedLevel already computes its own layout hash
            if (!string.IsNullOrWhiteSpace(generatedLevel.LayoutHash))
            {
                return generatedLevel.LayoutHash;
            }

            // Fallback: compute from available data
            var sb = new StringBuilder();
            sb.Append("level_").Append(generatedLevel.CaveLevel).Append("_");

            if (generatedLevel.Rooms != null && generatedLevel.Rooms.Count > 0)
            {
                sb.Append("rooms_").Append(generatedLevel.Rooms.Count).Append("_");

                // Hash room dimensions
                int totalCells = 0;
                foreach (var room in generatedLevel.Rooms)
                {
                    totalCells += room.Width * room.Height;
                }
                sb.Append("cells_").Append(totalCells).Append("_");
            }

            string hashInput = sb.ToString();
            return ComputeHash(hashInput);
        }

        public static bool VerifyLayoutHash(CaveGeneratedLevel generatedLevel, string expectedHash)
        {
            if (generatedLevel == null || string.IsNullOrWhiteSpace(expectedHash))
            {
                return false;
            }

            string computedHash = GenerateHash(generatedLevel);
            bool matches = computedHash == expectedHash;

            if (!matches)
            {
                Debug.LogWarning(
                    $"CaveLayoutHashGenerator: Layout hash mismatch! " +
                    $"Expected: {expectedHash}, Got: {computedHash}");
            }

            return matches;
        }

        private static string ComputeHash(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "empty";
            }

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString().Substring(0, 16);
            }
        }
    }
}
