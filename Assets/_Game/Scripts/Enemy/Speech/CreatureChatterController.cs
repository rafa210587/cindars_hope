using System;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Enemy.Speech
{
    /// <summary>
    /// fala ambiente de criatura da caverna. Anexado pelo CaveRuntimeMaterializer a cada inimigo
    /// (logo, cave-only por construção). Cerca de 1 em 10 criaturas vira "falante"; uma falante
    /// emite uma fala curta a cada 30-60s (intervalo aleatório), publicando
    /// <see cref="CreatureSpokeEvent"/>. As outras 9 em 10 desligam o Update (custo zero por frame).
    ///
    /// É puramente cosmético: não toca em combate, loot, nem no contrato de stable-run da caverna
    /// (chatter/timing não são conteúdo estável). Mesmo assim a seleção "é falante?" usa um seed
    /// determinístico do EnemyInstanceId, então a mesma criatura é (ou não) falante de forma estável.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CreatureChatterController : MonoBehaviour
    {
        // 1 em N criaturas fala (pedido: "1 em cada 10").
        private const int TalkerChanceDenominator = 10;

        // Janela do intervalo entre falas (pedido: "30s a 1min, aleatoriamente").
        private const float MinIntervalSeconds = 30f;
        private const float MaxIntervalSeconds = 60f;

        // Pequeno atraso inicial aleatório para as falantes não dispararem todas juntas ao entrar no nível.
        private const float MinFirstDelaySeconds = 4f;
        private const float MaxFirstDelaySeconds = MaxIntervalSeconds;

        // Sobe um tico acima da cabeça (acima de onde sai o número de dano).
        private const float BubbleExtraHeadOffsetY = 0.35f;

        private string _enemyId;
        private int _band;
        private System.Random _rng;
        private DamagePopupAnchor _anchor;
        private bool _isTalker;
        private float _timer;
        private float _nextInterval;

        /// <summary>
        /// Configura a criatura. <paramref name="instanceId"/> precisa ser estável por inimigo
        /// (EnemyInstanceId) para a decisão "é falante?" ser determinística. Se não for falante,
        /// o componente se desativa para não custar Update.
        /// </summary>
        public void Configure(string enemyId, int band, string instanceId, DamagePopupAnchor anchor)
        {
            _enemyId = enemyId ?? string.Empty;
            _band = band;
            _anchor = anchor != null ? anchor : GetComponent<DamagePopupAnchor>();
            _rng = new System.Random(StableSeed(instanceId));

            // 1 em TalkerChanceDenominator. Next(N) == 0 dá exatamente 1/N.
            _isTalker = _rng.Next(TalkerChanceDenominator) == 0;
            if (!_isTalker)
            {
                enabled = false; // sem Update para as não-falantes
                return;
            }

            _timer = 0f;
            _nextInterval = RandomInRange(MinFirstDelaySeconds, MaxFirstDelaySeconds);
        }

        private void Update()
        {
            if (!_isTalker)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < _nextInterval)
            {
                return;
            }

            _timer = 0f;
            _nextInterval = RandomInRange(MinIntervalSeconds, MaxIntervalSeconds);
            Speak();
        }

        private void Speak()
        {
            if (!CreatureSpeechLinePool.TryGetLine(_enemyId, _band, _rng, out var line))
            {
                return;
            }

            Vector3 position = _anchor != null
                ? _anchor.GetPopupWorldPosition()
                : transform.position + Vector3.up * 0.6f;
            position += Vector3.up * BubbleExtraHeadOffsetY;

            GameEventBus.Publish(new CreatureSpokeEvent(_enemyId, line, position));
        }

        private float RandomInRange(float min, float max)
        {
            return min + (float)_rng.NextDouble() * (max - min);
        }

        // Hash FNV-1a estável (não usa string.GetHashCode, que varia por plataforma/runtime).
        private static int StableSeed(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 17;
            }

            unchecked
            {
                const uint fnvOffset = 2166136261;
                const uint fnvPrime = 16777619;
                uint hash = fnvOffset;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= fnvPrime;
                }

                return (int)hash;
            }
        }
    }
}
