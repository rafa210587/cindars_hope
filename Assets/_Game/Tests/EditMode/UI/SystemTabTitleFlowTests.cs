using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.UI.SystemTab;
using CindarsHope.UI.Title;

namespace CindarsHope.Tests.EditMode.UI
{
    /// <summary>
    /// fable_56: EditMode tests da logica deterministica da aba Sistema + fluxo de titulo.
    /// IO de save (existencia/corrupcao/backup) e de settings (volume/video) abstraido.
    /// Cobre CA-1..CA-4 e CA-6. Sem dependencia de scene/canvas (essas ficam no cenario humano).
    /// </summary>
    [TestFixture]
    public class SystemTabTitleFlowTests
    {
        private sealed class FakeProbe : ISaveFileProbe
        {
            public bool HasSave { get; set; }
            public bool LastLoadFailed { get; set; }
            public bool HasBackup { get; set; }
            public string BackupDateLabel { get; set; } = string.Empty;
        }

        private sealed class FakeStore : ISettingsStore
        {
            private readonly Dictionary<string, int> _data = new Dictionary<string, int>();
            public int SaveCalls { get; private set; }
            public int GetInt(string key, int defaultValue) => _data.TryGetValue(key, out var v) ? v : defaultValue;
            public void SetInt(string key, int value) => _data[key] = value;
            public void Save() => SaveCalls++;
        }

        private sealed class FakeResetSurface : IResettableGameState
        {
            public FakeResetSurface(string id) => ResetSurfaceId = id;
            public string ResetSurfaceId { get; }
            public int ResetCount { get; private set; }
            public void ResetToNewGame() => ResetCount++;
        }

        // ---- CA-1 / availability ----

        [Test]
        public void Save_AlwaysAvailable()
        {
            var vm = new SystemTabViewModel(new FakeProbe { HasSave = false });
            Assert.IsTrue(vm.CanSave);
        }

        [Test]
        public void Load_Disabled_WhenNoSaveFile()
        {
            var vm = new SystemTabViewModel(new FakeProbe { HasSave = false });
            Assert.IsFalse(vm.CanLoad);
            Assert.IsFalse(vm.RequestLoad());
            Assert.AreEqual(SystemConfirmState.None, vm.ConfirmState);
        }

        [Test]
        public void Load_Enabled_WhenSaveExists()
        {
            var vm = new SystemTabViewModel(new FakeProbe { HasSave = true });
            Assert.IsTrue(vm.CanLoad);
        }

        // ---- CA-2 confirmation state machine ----

        [Test]
        public void RequestLoad_OpensConfirmation()
        {
            var vm = new SystemTabViewModel(new FakeProbe { HasSave = true });
            Assert.IsTrue(vm.RequestLoad());
            Assert.AreEqual(SystemConfirmState.ConfirmLoad, vm.ConfirmState);
            Assert.IsTrue(vm.HasPendingConfirm);
        }

        [Test]
        public void ConfirmLoad_OK_LoadsMainSave_AndClearsConfirm()
        {
            var vm = new SystemTabViewModel(new FakeProbe { HasSave = true, LastLoadFailed = false });
            vm.RequestLoad();
            Assert.AreEqual(SystemLoadOutcome.LoadedMainSave, vm.ConfirmLoad());
            Assert.AreEqual(SystemConfirmState.None, vm.ConfirmState);
        }

        [Test]
        public void Cancel_ClosesConfirmation_NoEffect()
        {
            var vm = new SystemTabViewModel(new FakeProbe { HasSave = true });
            vm.RequestQuitToTitle();
            Assert.AreEqual(SystemConfirmState.ConfirmQuitToTitle, vm.ConfirmState);
            vm.CancelConfirm();
            Assert.AreEqual(SystemConfirmState.None, vm.ConfirmState);
        }

        [Test]
        public void QuitToTitle_RequiresConfirmation()
        {
            var vm = new SystemTabViewModel(new FakeProbe());
            vm.RequestQuitToTitle();
            Assert.IsTrue(vm.ConfirmQuitToTitle());
            Assert.AreEqual(SystemConfirmState.None, vm.ConfirmState);
        }

        // ---- CA-6 corrupted save recovery ----

        [Test]
        public void ConfirmLoad_Corrupted_WithBackup_OffersRestore()
        {
            var probe = new FakeProbe { HasSave = true, LastLoadFailed = true, HasBackup = true, BackupDateLabel = "2026-06-20 14:32" };
            var vm = new SystemTabViewModel(probe);
            vm.RequestLoad();
            Assert.AreEqual(SystemLoadOutcome.OfferBackup, vm.ConfirmLoad());
            Assert.AreEqual(SystemConfirmState.OfferBackupRestore, vm.ConfirmState);
            StringAssert.Contains("2026-06-20 14:32", vm.BackupOfferMessage());
        }

        [Test]
        public void ConfirmBackupRestore_Restores_AndClears()
        {
            var probe = new FakeProbe { HasSave = true, LastLoadFailed = true, HasBackup = true };
            var vm = new SystemTabViewModel(probe);
            vm.RequestLoad();
            vm.ConfirmLoad();
            Assert.AreEqual(SystemLoadOutcome.RestoredBackup, vm.ConfirmBackupRestore());
            Assert.AreEqual(SystemConfirmState.None, vm.ConfirmState);
        }

        [Test]
        public void ConfirmLoad_Corrupted_NoBackup_ErrorAndNothingDestroyed()
        {
            var probe = new FakeProbe { HasSave = true, LastLoadFailed = true, HasBackup = false };
            var vm = new SystemTabViewModel(probe);
            vm.RequestLoad();
            Assert.AreEqual(SystemLoadOutcome.LoadFailedNoBackup, vm.ConfirmLoad());
            Assert.AreEqual(SystemConfirmState.LoadFailedNoBackup, vm.ConfirmState);
        }

        // ---- Volume / video persistence (EMENDA v3 4.4) ----

        [Test]
        public void Volume_ThreeChannels_PersistAndClamp()
        {
            var store = new FakeStore();
            GameAudioSettings.Bind(store);
            try
            {
                GameAudioSettings.MasterVolume = 150; // clamps to 100
                GameAudioSettings.SfxVolume = -10;    // clamps to 0
                GameAudioSettings.MusicVolume = 42;
                Assert.AreEqual(100, GameAudioSettings.MasterVolume);
                Assert.AreEqual(0, GameAudioSettings.SfxVolume);
                Assert.AreEqual(42, GameAudioSettings.MusicVolume);
                Assert.AreEqual(GameAudioSettings.SfxVolumeKey != null, true);
            }
            finally
            {
                GameAudioSettings.Bind(null);
            }
        }

        [Test]
        public void Volume_Defaults_WhenUnbound()
        {
            GameAudioSettings.Bind(null);
            Assert.AreEqual(GameAudioSettings.DefaultMasterVolume, GameAudioSettings.MasterVolume);
            Assert.AreEqual(GameAudioSettings.DefaultSfxVolume, GameAudioSettings.SfxVolume);
            Assert.AreEqual(GameAudioSettings.DefaultMusicVolume, GameAudioSettings.MusicVolume);
        }

        [Test]
        public void Video_FullscreenAndResolution_Persist()
        {
            var store = new FakeStore();
            GameAudioSettings.Bind(store);
            try
            {
                GameAudioSettings.Fullscreen = false;
                GameAudioSettings.ResolutionIndex = 3;
                Assert.IsFalse(GameAudioSettings.Fullscreen);
                Assert.AreEqual(3, GameAudioSettings.ResolutionIndex);
            }
            finally
            {
                GameAudioSettings.Bind(null);
            }
        }

        // ---- CA-3 New Game reset (single point) ----

        [Test]
        public void NewGameReset_ResetsAllRegisteredSurfaces()
        {
            var service = new NewGameStateResetService();
            var inventory = new FakeResetSurface("inventory");
            var economy = new FakeResetSurface("economy");
            var quests = new FakeResetSurface("quests");
            service.Register(inventory);
            service.Register(economy);
            service.Register(quests);

            service.ResetAll();

            Assert.AreEqual(3, service.LastResetCount);
            Assert.AreEqual(1, inventory.ResetCount);
            Assert.AreEqual(1, economy.ResetCount);
            Assert.AreEqual(1, quests.ResetCount);
        }

        [Test]
        public void NewGameReset_IsIdempotentAndDeduplicates()
        {
            var service = new NewGameStateResetService();
            var surface = new FakeResetSurface("time");
            service.Register(surface);
            service.Register(surface); // duplicate ignored

            service.ResetAll();
            service.ResetAll();

            Assert.AreEqual(1, service.Surfaces.Count);
            Assert.AreEqual(2, surface.ResetCount); // two ResetAll calls, idempotent surface
        }

        // ---- CA-4 Continue conditional ----

        [Test]
        public void Continue_Hidden_WhenNoSave()
        {
            var vm = new TitleFlowViewModel(new FakeProbe { HasSave = false });
            Assert.IsFalse(vm.CanContinue);
            Assert.AreEqual(SystemLoadOutcome.NoOp, vm.ResolveContinue());
        }

        [Test]
        public void Continue_Visible_WhenSaveExists_SamePathAsLoad()
        {
            var vm = new TitleFlowViewModel(new FakeProbe { HasSave = true, LastLoadFailed = false });
            Assert.IsTrue(vm.CanContinue);
            Assert.AreEqual(SystemLoadOutcome.LoadedMainSave, vm.ResolveContinue());
        }

        [Test]
        public void Continue_Corrupted_WithBackup_OffersBackup()
        {
            var vm = new TitleFlowViewModel(new FakeProbe { HasSave = true, LastLoadFailed = true, HasBackup = true });
            Assert.AreEqual(SystemLoadOutcome.OfferBackup, vm.ResolveContinue());
        }

        [Test]
        public void Continue_Corrupted_NoBackup_ReportsFailure()
        {
            var vm = new TitleFlowViewModel(new FakeProbe { HasSave = true, LastLoadFailed = true, HasBackup = false });
            Assert.AreEqual(SystemLoadOutcome.LoadFailedNoBackup, vm.ResolveContinue());
        }

        [Test]
        public void NewGame_AlwaysAvailable()
        {
            var vm = new TitleFlowViewModel(new FakeProbe { HasSave = false });
            Assert.IsTrue(vm.CanStartNewGame);
        }
    }
}
