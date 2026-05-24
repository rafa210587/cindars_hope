using System;
using System.IO;
using UnityEngine;

namespace CindarsHope.Save.Migrations
{
    public static class SaveBackupService
    {
        private const string BackupSuffix = ".backup";

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
