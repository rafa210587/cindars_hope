using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.DebugTools;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// F02 — postura do inimigo. Quebra → stagger (Stunned 1.2s) + CoreExposed via janela
    /// de vulnerabilidade. Recupera sozinha fora de combate. Transiente (sem save).
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyPostureState : MonoBehaviour
    {
        public const float StaggerSeconds = 1.2f;
        public const float CoreExposedSeconds = 1.5f;
        public const float CoreExposedMultiplier = 1.3f;
        private const float RecoverPerSecond = 8f;
        private const float RecoverDelaySeconds = 2.5f;
        private const float BreakCooldownSeconds = 6f;

        private float _maxPosture = 50f;
        private float _currentPosture;
        private float _lastDamageTime;
        private float _nextBreakAllowedTime;
        // arch: quebra do par mutuo Combat|Enemy — portas Foundation em vez dos tipos concretos
        // CindarsHope.Enemy.EnemyBrain / EnemyVulnerabilityState.
        private IEnemyBrainController _brain;
        private IEnemyVulnerabilityWindow _vulnerability;
        private CindarsHope.Combat.EnemyHealth _health;

        public float MaxPosture => _maxPosture;
        public float CurrentPosture => _currentPosture;
        public bool IsBroken { get; private set; }

        /// <summary>Tabela de posture por dificuldade (documentada na F02).</summary>
        public static float MaxPostureFor(EnemyDifficulty difficulty)
        {
            switch (difficulty)
            {
                case EnemyDifficulty.VeryEasy: return 25f;
                case EnemyDifficulty.Easy: return 40f;
                case EnemyDifficulty.Normal: return 60f;
                case EnemyDifficulty.Hard: return 90f;
                case EnemyDifficulty.Elite: return 120f;
                case EnemyDifficulty.MiniBoss: return 160f;
                case EnemyDifficulty.Boss: return 220f;
                default: return 60f;
            }
        }

        public void Configure(EnemyDifficulty difficulty)
        {
            _maxPosture = MaxPostureFor(difficulty);
            _currentPosture = _maxPosture;
        }

        /// <summary>Aplica dano de posture. Retorna true se quebrou neste hit.</summary>
        public bool ApplyPostureDamage(float amount)
        {
            if (amount <= 0f || IsBroken)
            {
                return false;
            }

            _currentPosture = Mathf.Max(0f, _currentPosture - amount);
            _lastDamageTime = Time.time;

            if (_currentPosture > 0f || Time.time < _nextBreakAllowedTime)
            {
                return false;
            }

            Break();
            return true;
        }

        private void Break()
        {
            IsBroken = true;
            _nextBreakAllowedTime = Time.time + BreakCooldownSeconds;

            var enemyId = _health != null ? _health.EnemyId : name;
            CombatLog.Log($"CombatLog: EnemyPostureBroken. EnemyId={enemyId}, MaxPosture={_maxPosture}", this);
            GameEventBus.Publish(new EnemyPostureBrokenEvent(enemyId));

            if (_brain != null)
            {
                _brain.ApplyStun(StaggerSeconds);
            }

            // CoreExposed: crítico garantido + bônus (janela de vulnerabilidade canônica).
            if (_vulnerability != null)
            {
                _vulnerability.OpenWindow(StaggerSeconds + CoreExposedSeconds, CoreExposedMultiplier, BreakCooldownSeconds);
            }

            Invoke(nameof(Recover), StaggerSeconds);
        }

        private void Recover()
        {
            IsBroken = false;
            _currentPosture = _maxPosture * 0.5f; // meia barra pós-stagger
        }

        private void Awake()
        {
            _brain = GetComponent<IEnemyBrainController>();
            _vulnerability = GetComponent<IEnemyVulnerabilityWindow>();
            _health = GetComponent<CindarsHope.Combat.EnemyHealth>();
            _currentPosture = _maxPosture;
        }

        private void Update()
        {
            if (IsBroken || _currentPosture >= _maxPosture)
            {
                return;
            }

            if (Time.time - _lastDamageTime >= RecoverDelaySeconds)
            {
                _currentPosture = Mathf.Min(_maxPosture, _currentPosture + RecoverPerSecond * Time.deltaTime);
            }
        }
    }
}
