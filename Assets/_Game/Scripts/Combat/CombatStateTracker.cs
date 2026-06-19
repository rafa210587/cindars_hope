using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// fable_69 — regras PURAS (sem Unity) da janela "em combate".
    /// O player é considerado em combate enquanto o tempo decorrido desde o último
    /// dano DADO ou RECEBIDO for menor que <see cref="CombatWindowSeconds"/>.
    /// Mantida testável (EditMode) para a F69 sprint, evitando depender de Time.time em teste.
    /// </summary>
    public static class CombatWindowRules
    {
        /// <summary>Janela canônica F69: 4s sem dano dado/recebido → sai de combate.</summary>
        public const float CombatWindowSeconds = 4f;

        /// <summary>
        /// True se ainda dentro da janela de combate. <paramref name="lastCombatActivityTime"/>
        /// negativo (sentinela) significa "nunca houve combate" → fora de combate.
        /// </summary>
        public static bool IsInCombat(float lastCombatActivityTime, float now)
        {
            if (lastCombatActivityTime < 0f)
            {
                return false;
            }

            return now - lastCombatActivityTime < CombatWindowSeconds;
        }
    }

    /// <summary>
    /// fable_69 — tracker LEVE de "em combate" (Fase 0 confirmou ausência de qualquer
    /// CombatStateTracker/InCombat no projeto). Consome eventos EXISTENTES
    /// (<see cref="DamageAppliedEvent"/> = dano dado, <see cref="PlayerDamagedEvent"/> = dano
    /// recebido) e expõe <see cref="IsInCombat"/> via janela de 4s. Não cria comunicação nova
    /// nem segundo sistema de combate; serve a F69 (sprint) como sinal de habilitação.
    ///
    /// Auto-registro estático (sem global search) seguindo o padrão de PlayerBlockController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatStateTracker : MonoBehaviour
    {
        private static CombatStateTracker _activeInstance;

        // Sentinela: nunca houve atividade de combate (fora de combate).
        private float _lastCombatActivityTime = -1000f;

        /// <summary>Instância ativa (auto-registro) para consumidores sem FindObjectOfType.</summary>
        public static CombatStateTracker ActiveInstance => _activeInstance;

        /// <summary>True enquanto dentro da janela de 4s desde o último dano dado/recebido.</summary>
        public bool IsInCombat => CombatWindowRules.IsInCombat(_lastCombatActivityTime, Time.time);

        /// <summary>Último instante (Time.time) de atividade de combate registrada.</summary>
        public float LastCombatActivityTime => _lastCombatActivityTime;

        private void Awake()
        {
            if (_activeInstance != null && _activeInstance != this)
            {
                Destroy(this);
                return;
            }

            _activeInstance = this;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DamageAppliedEvent>(HandleDamageApplied);
            GameEventBus.Subscribe<PlayerDamagedEvent>(HandlePlayerDamaged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DamageAppliedEvent>(HandleDamageApplied);
            GameEventBus.Unsubscribe<PlayerDamagedEvent>(HandlePlayerDamaged);
        }

        private void OnDestroy()
        {
            if (_activeInstance == this)
            {
                _activeInstance = null;
            }
        }

        private void HandleDamageApplied(DamageAppliedEvent evt)
        {
            // Dano DADO pelo player (qualquer DamageResult resolvido) reabre a janela de combate.
            _lastCombatActivityTime = Time.time;
        }

        private void HandlePlayerDamaged(PlayerDamagedEvent evt)
        {
            // Dano RECEBIDO pelo player reabre a janela de combate.
            _lastCombatActivityTime = Time.time;
        }
    }

    /// <summary>Garante o tracker na cena (padrão bootstrap do projeto, sem global search recorrente).</summary>
    public static class CombatStateTrackerBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (CombatStateTracker.ActiveInstance != null)
            {
                return;
            }

            var go = new GameObject("CombatStateTracker");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<CombatStateTracker>();
            Debug.Log("[CombatStateTrackerBootstrap] CombatStateTracker instanciado via bootstrap (fable_69).");
        }
    }
}
