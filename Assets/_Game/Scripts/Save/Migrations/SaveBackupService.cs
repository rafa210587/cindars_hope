using System;
using System.IO;
using UnityEngine;

namespace CindarsHope.Save.Migrations
{
    /// <summary>Owns save-file replacement and backup operations without interpreting the save schema.</summary>
    /// <remarks>Callers serialize and validate contents. IO failures propagate from writing; Try operations return error details.</remarks>
    public static class SaveBackupService
    {
        private const string BackupSuffix = ".backup";

        /// <summary>Writes through a temporary file and retains the previous contents as a backup.</summary>
        /// <remarks>Replacement is atomic where File.Replace is supported. The legacy fallback keeps a safety backup but is not crash-atomic.</remarks>
        public static void WriteTextSafely(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = $"{path}.tmp";
            File.WriteAllText(tempPath, contents);
            if (!File.Exists(tempPath))
            {
                throw new IOException($"Temporary save file was not written: {tempPath}");
            }

            if (!File.Exists(path))
            {
                File.Move(tempPath, path);
                return;
            }

            var backupPath = $"{path}{BackupSuffix}";
            try
            {
                File.Replace(tempPath, path, backupPath, true);
            }
            catch (PlatformNotSupportedException)
            {
                if (!TryCreateBackup(path, out _, out var backupError))
                {
                    throw new IOException($"Could not create safety backup before replacing save: {backupError}");
                }

                File.Delete(path);
                File.Move(tempPath, path);
            }
        }

        public static bool TryCreateBackup(string originalPath, out string backupPath, out string errorMessage)
        {
            backupPath = string.Empty;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(originalPath))
            {
                errorMessage = "Original save path is null or empty.";
                return false;
            }

            if (!File.Exists(originalPath))
            {
                errorMessage = $"Save file does not exist at {originalPath}.";
                return false;
            }

            try
            {
                backupPath = $"{originalPath}{BackupSuffix}";

                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                File.Copy(originalPath, backupPath, true);

                if (!File.Exists(backupPath))
                {
                    errorMessage = $"Backup file was not created at {backupPath}.";
                    return false;
                }

                Debug.Log($"Save backup created at {backupPath}.", null);
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to create backup: {exception.Message}";
                return false;
            }
        }

        public static bool TryRestoreBackup(string originalPath, string backupPath, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath))
            {
                errorMessage = "Backup file does not exist.";
                return false;
            }

            try
            {
                if (File.Exists(originalPath))
                {
                    File.Delete(originalPath);
                }

                File.Copy(backupPath, originalPath, true);

                Debug.Log($"Save restored from backup {backupPath}.", null);
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to restore backup: {exception.Message}";
                return false;
            }
        }

        public static bool TryDeleteBackup(string backupPath, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath))
            {
                return true;
            }

            try
            {
                File.Delete(backupPath);
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to delete backup: {exception.Message}";
                return false;
            }
        }
    }
}
