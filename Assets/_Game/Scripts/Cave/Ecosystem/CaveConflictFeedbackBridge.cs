using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 (SLICE 4) — feedback de HUD OBRIGATÓRIO do conflito inter-monstro (seção 14.6/16.5).
    /// Assina <see cref="CaveEcosystemConflictStartedEvent"/> e publica um toast no HUD existente via
    /// <see cref="PlayerActionFeedbackEvent"/> (consumido por GameplayFeedbackService/DebugHud). Sem isto
    /// a feature lê como bug (rule game-feel-checklist).
    ///
    /// Self-wiring por RuntimeInitializeOnLoadMethod (idioma do projeto p/ bridges de HUD — ex.: bridges
    /// de WAVE_INTEGRATION_23) — não exige edição de cena/prefab. Singleton DontDestroyOnLoad com guard de
    /// duplicata. Unsubscribe em OnDisable/OnDestroy (rule de unsubscribe obrigatório p/ subscriber novo).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CaveConflictFeedbackBridge : MonoBehaviour
    {
        private const float ToastDurationSeconds = 3f;

        private static CaveConflictFeedbackBridge _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject(nameof(CaveConflictFeedbackBridge));
            _instance = go.AddComponent<CaveConflictFeedbackBridge>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<CaveEcosystemConflictStartedEvent>(OnConflictStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CaveEcosystemConflictStartedEvent>(OnConflictStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnConflictStarted(CaveEcosystemConflictStartedEvent evt)
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Criaturas em conflito!", ToastDurationSeconds));
        }
    }
}
