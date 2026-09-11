using System;
using System.IO;
using System.Reflection;
using CindarsHope.Save;
using CindarsHope.Save.Migrations;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Save
{
    /// <summary>Tests safe file replacement directly and backup recovery through the SaveManager adapter.</summary>
    /// <remarks>Every file is isolated in a temporary directory; only the private recovery adapter uses reflection.</remarks>
    public class SaveAtomicWriteTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "cindars_hope_save_atomic_write_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        private static void InvokeWriteTextSafely(string path, string contents)
        {
            SaveBackupService.WriteTextSafely(path, contents);
        }

        private static bool InvokeTryRecoverFromBackupIfNeeded(string path)
        {
            var method = typeof(SaveManager).GetMethod(
                "TryRecoverFromBackupIfNeeded",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "TryRecoverFromBackupIfNeeded method not found via reflection.");

            try
            {
                return (bool)method.Invoke(null, new object[] { path });
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }

        private static string BuildMinimalSaveJson()
        {
            // GameSaveData exige Player != null para ser considerado válido por ValidateAndNormalizeSave
            // (indiretamente via TryDeserializeSave, que s� checa parse, n�o valida��o completa).
            // Para os testes desta spec usamos um JSON minimamente parseável por JsonUtility.
            return "{\"SchemaVersion\":5,\"CurrentDay\":1}";
        }

        // ── Escrita segura ──────────────────────────────────────────────────────────────────

        [Test]
        public void WriteTextSafely_NoExistingFile_CreatesFileDirectly()
        {
            var path = Path.Combine(_tempDir, "slot_1.json");
            var contents = BuildMinimalSaveJson();

            InvokeWriteTextSafely(path, contents);

            Assert.IsTrue(File.Exists(path));
            Assert.AreEqual(contents, File.ReadAllText(path));
            Assert.IsFalse(File.Exists(path + ".tmp"), "Temp file should not remain after successful write.");
        }

        [Test]
        public void WriteTextSafely_ExistingFile_ReplacesContentAndCreatesBackupOfOldContent()
        {
            var path = Path.Combine(_tempDir, "slot_1.json");
            var oldContents = "{\"SchemaVersion\":5,\"CurrentDay\":1}";
            var newContents = "{\"SchemaVersion\":5,\"CurrentDay\":2}";

            File.WriteAllText(path, oldContents);

            InvokeWriteTextSafely(path, newContents);

            Assert.IsTrue(File.Exists(path), "Final save file must exist after write.");
            Assert.AreEqual(newContents, File.ReadAllText(path));

            var backupPath = path + ".backup";
            Assert.IsTrue(File.Exists(backupPath), "Backup of previous content must exist after overwrite (File.Replace semantics).");
            Assert.AreEqual(oldContents, File.ReadAllText(backupPath));

            Assert.IsFalse(File.Exists(path + ".tmp"), "Temp file should not remain after successful write.");
        }

        [Test]
        public void WriteTextSafely_ExistingFile_NeverLeavesPathMissing()
        {
            // Verifica��o estrutural: depois de qualquer WriteTextSafely bem-sucedido sobre um
            // arquivo existente, o path final est� sempre presente � n�o h� passo intermedi�rio
            // observável (do ponto de vista do chamador) em que o arquivo esteja ausente.
            var path = Path.Combine(_tempDir, "slot_1.json");
            File.WriteAllText(path, "{\"SchemaVersion\":5,\"CurrentDay\":1}");

            InvokeWriteTextSafely(path, "{\"SchemaVersion\":5,\"CurrentDay\":3}");

            Assert.IsTrue(File.Exists(path));
        }

        [Test]
        public void WriteTextSafely_CreatesParentAndPreservesUnicodeContents()
        {
            var path = Path.Combine(_tempDir, "nested", "slot_1.json");
            const string contents = "{\"name\":\"Cindar — esperança\"}";
            InvokeWriteTextSafely(path, contents);
            Assert.That(File.ReadAllText(path), Is.EqualTo(contents));
            Assert.That(File.Exists(path + ".tmp"), Is.False);
        }

        [Test]
        public void WriteTextSafely_RepeatedReplacementBacksUpImmediatelyPreviousContent()
        {
            var path = Path.Combine(_tempDir, "slot_1.json");
            InvokeWriteTextSafely(path, "first");
            InvokeWriteTextSafely(path, "second");
            InvokeWriteTextSafely(path, "third");
            Assert.That(File.ReadAllText(path), Is.EqualTo("third"));
            Assert.That(File.ReadAllText(path + ".backup"), Is.EqualTo("second"));
        }

        [Test]
        public void WriteTextSafely_TemporaryWriteFailurePreservesMainAndExistingBackup()
        {
            var path = Path.Combine(_tempDir, "slot_1.json");
            File.WriteAllText(path, "original");
            File.WriteAllText(path + ".backup", "previous");
            Directory.CreateDirectory(path + ".tmp");

            Assert.Catch<Exception>(() => InvokeWriteTextSafely(path, "replacement"));
            Assert.That(File.ReadAllText(path), Is.EqualTo("original"));
            Assert.That(File.ReadAllText(path + ".backup"), Is.EqualTo("previous"));
        }

        // -- Recupera��o de backup no load ---------------------------------------------------

        [Test]
        public void RecoverFromBackup_MainMissing_BackupValid_RestoresFromBackup()
        {
            var path = Path.Combine(_tempDir, "slot_1.json");
            var backupPath = path + ".backup";
            var backupContents = BuildMinimalSaveJson();
            File.WriteAllText(backupPath, backupContents);

            var recovered = InvokeTryRecoverFromBackupIfNeeded(path);

            Assert.IsTrue(recovered);
            Assert.IsTrue(File.Exists(path));
            Assert.AreEqual(backupContents, File.ReadAllText(path));
        }

        [Test]
        public void RecoverFromBackup_MainCorrupted_BackupValid_RestoresFromBackup()
        {
            var path = Path.Combine(_tempDir, "slot_1.json");
            var backupPath = path + ".backup";
            File.WriteAllText(path, "{not valid json!!");
            var backupContents = BuildMinimalSaveJson();
            File.WriteAllText(backupPath, backupContents);

            var recovered = InvokeTryRecoverFromBackupIfNeeded(path);

            Assert.IsTrue(recovered);
            Assert.AreEqual(backupContents, File.ReadAllText(path));
        }

        [Test]
        public void RecoverFromBackup_MainMissing_NoBackup_ReturnsFalse_NoRegression()
        {
            // Sem path nem .backup: comportamento existente ("sem save = novo jogo") n�o deve
            // mudar. Nenhum arquivo deve ser criado por este método.
            var path = Path.Combine(_tempDir, "slot_1.json");

            var recovered = InvokeTryRecoverFromBackupIfNeeded(path);

            Assert.IsFalse(recovered);
            Assert.IsFalse(File.Exists(path));
        }

        [Test]
        public void RecoverFromBackup_MainValid_DoesNotTouchBackupOrOverwrite()
        {
            var path = Path.Combine(_tempDir, "slot_1.json");
            var mainContents = BuildMinimalSaveJson();
            File.WriteAllText(path, mainContents);

            var recovered = InvokeTryRecoverFromBackupIfNeeded(path);

            Assert.IsFalse(recovered, "Main save is valid; recovery path must not trigger.");
            Assert.AreEqual(mainContents, File.ReadAllText(path));
        }

        [Test]
        public void RecoverFromBackup_MainMissing_BackupAlsoCorrupted_ReturnsFalse()
        {
            var path = Path.Combine(_tempDir, "slot_1.json");
            var backupPath = path + ".backup";
            File.WriteAllText(backupPath, "{also not valid json!!");

            var recovered = InvokeTryRecoverFromBackupIfNeeded(path);

            Assert.IsFalse(recovered);
            Assert.IsFalse(File.Exists(path));
        }
    }
}
