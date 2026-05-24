using System;
using System.IO;

namespace CindarsHope.Save.Migrations
{
    public static class SaveBackupService
    {
        private const string BackupDirectoryName = "backups";

        public static bool TryCreateBackup(string saveFilePath, out string backupFilePath, out string errorMessage)
        {
            backupFilePath = string.Empty;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(saveFilePath) || !File.Exists(saveFilePath))
            {
                errorMessage = $"Save file not found for backup: {saveFilePath}";
                return false;
            }

            try
            {
                var saveDirectory = Path.GetDirectoryName(saveFilePath);
                var backupDirectory = Path.Combine(saveDirectory, BackupDirectoryName);
                Directory.CreateDirectory(backupDirectory);

                var saveName = Path.GetFileNameWithoutExtension(saveFilePath);
                var extension = Path.GetExtension(saveFilePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                backupFilePath = Path.Combine(backupDirectory, $"{saveName}_{timestamp}{extension}");
                File.Copy(saveFilePath, backupFilePath, false);
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }
    }
}
