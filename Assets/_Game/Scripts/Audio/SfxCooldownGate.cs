using System.Collections.Generic;

namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — gate anti-spam PURO por categoria. Em rajadas (ex.: DamageApplied
    /// multi-hit), evita que a mesma categoria toque mais de uma vez dentro da janela
    /// de cooldown (~50ms default), prevenindo ruído/clipping.
    ///
    /// Determinístico e testável: recebe o "agora" (segundos) por parâmetro — sem
    /// depender de Time.time. Não lança; categorias desconhecidas são tratadas.
    /// </summary>
    public sealed class SfxCooldownGate
    {
        /// <summary>Cooldown padrão por categoria, em segundos (~50ms).</summary>
        public const float DefaultCooldownSeconds = 0.05f;

        private readonly float _cooldownSeconds;
        private readonly Dictionary<SfxCategory, float> _lastPlayTime = new Dictionary<SfxCategory, float>();

        public SfxCooldownGate(float cooldownSeconds = DefaultCooldownSeconds)
        {
            _cooldownSeconds = cooldownSeconds < 0f ? 0f : cooldownSeconds;
        }

        public float CooldownSeconds => _cooldownSeconds;

        /// <summary>
        /// Retorna true se a categoria PODE tocar agora (e registra o disparo).
        /// Retorna false se ainda está dentro do cooldown (suprime o disparo).
        /// A categoria None nunca toca.
        /// </summary>
        public bool TryConsume(SfxCategory category, float nowSeconds)
        {
            if (category == SfxCategory.None)
            {
                return false;
            }

            if (_lastPlayTime.TryGetValue(category, out float last))
            {
                if (nowSeconds - last < _cooldownSeconds)
                {
                    return false;
                }
            }

            _lastPlayTime[category] = nowSeconds;
            return true;
        }

        /// <summary>Limpa o histórico (reset de sessão/teste).</summary>
        public void Reset()
        {
            _lastPlayTime.Clear();
        }
    }
}
