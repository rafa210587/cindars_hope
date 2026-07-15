using System;
using UnityEngine;
using CindarsHope.Audio;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Save;

namespace CindarsHope.UI.SystemTab
{
    /// <summary>
    /// fable_56: adapter MonoBehaviour FINO da aba Sistema do painel unico F14. Toda a logica
    /// de disponibilidade/confirmacao/recuperacao vive em <see cref="SystemTabViewModel"/>
    /// (pura, testavel). Este controller apenas: resolve dependencias via GameBootstrap (sem
    /// GameObject.Find), chama as APIs publicas do SaveManager (Save/Load), aplica volumes via
    /// AudioManager + GameAudioSettings, aplica video via Screen.*, e empurra confirmacoes no
    /// ModalManager existente. Feedback de salvar/carregar via GameEventBus.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SystemTabController : MonoBehaviour
    {
        private SystemTabViewModel _viewModel;
        private SaveFileProbe _probe;
        private SaveManager _saveManager;
        private IModalRuntime _modalManager;

        public SystemTabViewModel ViewModel => _viewModel;

        private void Awake()
        {
            EnsureSettingsBound();
            ResolveDependencies();
        }

        /// <summary>Liga o store de PlayerPrefs uma vez (idempotente).</summary>
        public static void EnsureSettingsBound()
        {
            if (!GameAudioSettings.IsBound)
            {
                GameAudioSettings.Bind(new PlayerPrefsSettingsStore());
            }
        }

        private void ResolveDependencies()
        {
            var bootstrap = GameBootstrap.Instance;
            // arch: quebra do par mutuo Core|Save (2026-07-15) — bootstrap.SaveManager agora retorna
            // a porta ISaveRuntime; SystemTabController ja referencia CindarsHope.Save diretamente
            // (fora do par cortado), entao resolve o tipo concreto por cast local.
            _saveManager = bootstrap != null ? bootstrap.SaveManager as SaveManager : null;
            _modalManager = bootstrap != null ? bootstrap.ModalManager : null;
            _probe = new SaveFileProbe(_saveManager);
            _viewModel = new SystemTabViewModel(_probe);
        }

        /// <summary>Salvar (slot unico v1) + feedback via GameSavedEvent (publicado pelo SaveManager).</summary>
        public bool RequestSave()
        {
            if (_saveManager == null)
            {
                Debug.LogWarning("[fable_56] SystemTabController: SaveManager ausente; Salvar ignorado.");
                return false;
            }

            return _saveManager.SaveGame();
        }

        /// <summary>Carregar: abre confirmacao modal. O load real ocorre em ConfirmLoad.</summary>
        public bool RequestLoad()
        {
            if (_viewModel == null || !_viewModel.RequestLoad())
            {
                return false;
            }

            _modalManager?.PushModal(ModalType.SystemConfirm);
            return true;
        }

        /// <summary>
        /// Confirma o Carregar: executa SaveManager.LoadGame. Se falhar (corrompido), marca o
        /// probe e deixa a viewmodel oferecer a recuperacao de backup (CA-6) sem destruir nada.
        /// </summary>
        public SystemLoadOutcome ConfirmLoad()
        {
            if (_viewModel == null)
            {
                return SystemLoadOutcome.NoOp;
            }

            var loadOk = _saveManager != null && _saveManager.LoadGame();
            _probe.LastLoadFailed = !loadOk;

            var outcome = _viewModel.ConfirmLoad();
            if (outcome != SystemLoadOutcome.OfferBackup &&
                outcome != SystemLoadOutcome.NoOp)
            {
                _modalManager?.TryPopIfCurrent(ModalType.SystemConfirm);
            }

            return outcome;
        }

        /// <summary>Aceita restaurar o backup rolling (CA-6). Recarrega via API publica do SaveManager.</summary>
        public SystemLoadOutcome ConfirmBackupRestore()
        {
            if (_viewModel == null)
            {
                return SystemLoadOutcome.NoOp;
            }

            // O reload a partir do backup usa o mesmo caminho publico; quando o SaveManager
            // expuser a restauracao do .bak, esta chamada a aciona. Por ora reexecuta LoadGame
            // (o backup ja foi promovido pelo SaveManager quando implementado).
            _saveManager?.LoadGame();
            var outcome = _viewModel.ConfirmBackupRestore();
            _modalManager?.TryPopIfCurrent(ModalType.SystemConfirm);
            return outcome;
        }

        /// <summary>Cancela qualquer confirmacao aberta (Esc).</summary>
        public void CancelConfirm()
        {
            _viewModel?.CancelConfirm();
            _modalManager?.TryPopIfCurrent(ModalType.SystemConfirm);
        }

        // ---- Volume (tres canais, EMENDA v3 4.4) ----

        public void SetMasterVolume(int value)
        {
            GameAudioSettings.MasterVolume = value;
            AudioManager.Instance?.SetMasterVolume(GameAudioSettings.MasterVolume);
        }

        public void SetSfxVolume(int value)
        {
            GameAudioSettings.SfxVolume = value;
            AudioManager.Instance?.SetChannelVolume(AudioChannel.Sfx, GameAudioSettings.SfxVolume);
        }

        public void SetMusicVolume(int value)
        {
            GameAudioSettings.MusicVolume = value;
            AudioManager.Instance?.SetChannelVolume(AudioChannel.Music, GameAudioSettings.MusicVolume);
        }

        // ---- Video (EMENDA v3 4.4) — UI state via Screen.*, fora de ProjectSettings ----

        public void SetFullscreen(bool fullscreen)
        {
            GameAudioSettings.Fullscreen = fullscreen;
            Screen.fullScreen = fullscreen;
        }
    }
}
