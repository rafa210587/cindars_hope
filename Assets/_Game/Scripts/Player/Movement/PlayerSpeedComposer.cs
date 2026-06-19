using System.Collections.Generic;

namespace CindarsHope.Player.Movement
{
    /// <summary>
    /// fable_47 — compositor puro (sem Unity) da velocidade do player.
    /// Cada sistema escreve seu fator nomeado via <see cref="SetFactor"/> e o remove via
    /// <see cref="ClearFactor"/>. <see cref="Value"/> é o PRODUTO de todos os fatores ativos,
    /// clampado em [0, +inf). Remover um fator nunca corrompe os demais — elimina a corrida
    /// do antigo padrão "snapshot + restore" sobre o mutável compartilhado.
    ///
    /// Regra F01 preservada: o fator <see cref="SpeedFactorKind.Status"/>, quando reduz a
    /// velocidade sem ser parada total (Root/Stun = 0), é pisado em <see cref="MinSpeedFloor"/>
    /// — um slow nunca derruba o fator de status abaixo de 0.5, mas Root/Stun continuam zerando.
    /// </summary>
    public sealed class PlayerSpeedComposer
    {
        /// <summary>Floor do fator de status (F01): slow não-letal nunca cai abaixo disto.</summary>
        public const float MinSpeedFloor = 0.5f;

        private readonly Dictionary<SpeedFactorKind, float> _factors = new Dictionary<SpeedFactorKind, float>();

        /// <summary>Velocidade efetiva = produto dos fatores ativos (1.0 quando nenhum fator está setado).</summary>
        public float Value
        {
            get
            {
                var product = 1f;
                foreach (var factor in _factors.Values)
                {
                    product *= factor;
                }

                return product < 0f ? 0f : product;
            }
        }

        /// <summary>Define (ou substitui) o fator do sistema dado. Status é pisado pela regra F01.</summary>
        public void SetFactor(SpeedFactorKind kind, float value)
        {
            _factors[kind] = NormalizeFactor(kind, value);
        }

        /// <summary>Remove o fator do sistema (volta a não contribuir = neutro 1.0).</summary>
        public void ClearFactor(SpeedFactorKind kind)
        {
            _factors.Remove(kind);
        }

        /// <summary>True se o sistema tem um fator ativo.</summary>
        public bool HasFactor(SpeedFactorKind kind)
        {
            return _factors.ContainsKey(kind);
        }

        /// <summary>Fator atual do sistema (1.0 se não setado — neutro).</summary>
        public float GetFactor(SpeedFactorKind kind)
        {
            return _factors.TryGetValue(kind, out var value) ? value : 1f;
        }

        /// <summary>Remove todos os fatores (teardown/respawn — evita fator órfão).</summary>
        public void ClearAll()
        {
            _factors.Clear();
        }

        /// <summary>Normalização pura (testável): clamp >= 0; floor F01 no fator de status não-letal.</summary>
        public static float NormalizeFactor(SpeedFactorKind kind, float value)
        {
            if (value < 0f)
            {
                value = 0f;
            }

            if (kind == SpeedFactorKind.Status && value > 0f && value < MinSpeedFloor)
            {
                return MinSpeedFloor;
            }

            return value;
        }
    }
}
