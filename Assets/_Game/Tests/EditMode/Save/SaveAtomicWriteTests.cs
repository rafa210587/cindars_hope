using System;
using System.IO;
using System.Reflection;
using CindarsHope.Save;
using NUnit.Framework;

/// <summary>
/// Cobre spec_codex_10_save_atomic_write: WriteTextSafely n�o pode deixar uma janela em que o
/// arquivo final está ausente, e o load deve recuperar automaticamente de um `.backup` quando o
/// arquivo principal está ausente ou corrompido. WriteTextSafely e TryRecoverFromBackupIfNeeded
/// s�o `private static` em SaveManager (partial class) � chamados via reflection para n�o abrir
/// API p�blica nova apenas para teste (YAGNI). Todos os arquivos usados s�o criados em um
/// diretório temporário isolado (Path.GetTempPath()), nunca tocando saves reais do usuário.
/// </summary>
namespace CindarsHope.Tests.EditMode.Save
{
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
            var method = typeof(SaveManager).GetMethod(
                "WriteTextSafely",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "WriteTextSafely method not found via reflection.");

            try
            {
                method.Invoke(null, new object[] { path, contents });
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
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
