using UnityEngine;

namespace CindarsHope.Combat.Magic
{
    /// <summary>
    /// fable_08 — barreira arcana temporária do player. Absorve até <c>RemainingAbsorb</c> de dano
    /// e expira após <c>ExpiresAtTime</c>. NÃO é um receptor de dano paralelo: o
    /// <see cref="CindarsHope.Combat.PlayerDamageReceiver"/> consulta esta barreira ANTES de block,
    /// defesa e resistência (ordem documentada — risco de duplicar mitigação mitigado por teste).
    ///
    /// A lógica de absorção (<see cref="AbsorbLogic"/>) é PURA/estática para EditMode tests sem cena.
    /// O <see cref="ActiveInstance"/> é um hook fino setado pelo SpellCastService quando a barreira é
    /// conjurada; transitório (não persistido — ver Save contracts da spec: N/A).
    /// </summary>
    public sealed class PlayerBarrierState
    {
        /// <summary>Instância ativa (null = sem barreira). Setada pelo cast de Barrier.</summary>
        public static PlayerBarrierState ActiveInstance { get; private set; }

        public int RemainingAbsorb { get; private set; }
        public float ExpiresAtTime { get; private set; }
        public string SourceSpellId { get; private set; }

        private PlayerBarrierState(int absorb, float expiresAtTime, string sourceSpellId)
        {
            RemainingAbsorb = Mathf.Max(0, absorb);
            ExpiresAtTime = expiresAtTime;
            SourceSpellId = sourceSpellId ?? string.Empty;
        }

        /// <summary>True se ainda há absorção e não expirou no instante <paramref name="now"/>.</summary>
        public bool IsActive(float now) => RemainingAbsorb > 0 && now < ExpiresAtTime;

        /// <summary>
        /// Conjura (ou substitui) a barreira ativa. absorb &lt;= 0 ou seconds &lt;= 0 limpa a barreira
        /// (sem barreira útil). Substituição NÃO acumula (regra canônica anti-stack, igual a status).
        /// </summary>
        public static PlayerBarrierState Cast(int absorb, float seconds, float now, string sourceSpellId)
        {
            if (absorb <= 0 || seconds <= 0f)
            {
                Clear();
                return null;
            }

            ActiveInstance = new PlayerBarrierState(absorb, now + seconds, sourceSpellId);
            Debug.Log($"CombatLog: SpellBarrierRaised. Spell={ActiveInstance.SourceSpellId}, Absorb={absorb}, Seconds={seconds:F2}");
            return ActiveInstance;
        }

        /// <summary>Remove a barreira ativa (expiração natural ou cancelamento).</summary>
        public static void Clear()
        {
            ActiveInstance = null;
        }

        /// <summary>
        /// Aplica a barreira ativa ao dano recebido, consumindo absorção. Retorna o dano que
        /// passa para os próximos redutores (block/defesa). Expira/limpa a instância quando esgota
        /// ou quando o tempo acabou. Null-safe via <see cref="ActiveInstance"/>.
        /// </summary>
        public static int AbsorbIncoming(int rawDamage, float now)
        {
            var barrier = ActiveInstance;
            if (barrier == null)
            {
                return rawDamage;
            }

            if (!barrier.IsActive(now))
            {
                Clear();
                return rawDamage;
            }

            int passthrough = barrier.ConsumeAbsorb(rawDamage, out int absorbed);
            Debug.Log($"CombatLog: SpellBarrierAbsorbed. Spell={barrier.SourceSpellId}, Absorbed={absorbed}, Passthrough={passthrough}, Remaining={barrier.RemainingAbsorb}");

            if (barrier.RemainingAbsorb <= 0)
            {
                Debug.Log($"CombatLog: SpellBarrierExpired. Spell={barrier.SourceSpellId}, Reason=Depleted");
                Clear();
            }

            return passthrough;
        }

        /// <summary>
        /// Lógica pura de consumo: absorve min(raw, RemainingAbsorb), reduz o restante e devolve o
        /// dano que passa adiante. Mutável (instância) mas isolada para teste direto.
        /// </summary>
        public int ConsumeAbsorb(int rawDamage, out int absorbed)
        {
            absorbed = AbsorbLogic(rawDamage, RemainingAbsorb, out int newRemaining);
            RemainingAbsorb = newRemaining;
            return rawDamage - absorbed;
        }

        /// <summary>
        /// Função PURA: dado o dano cru e a absorção restante, devolve quanto foi absorvido e a nova
        /// absorção restante (via out). Sem efeitos colaterais — base dos EditMode tests (CA-3).
        /// </summary>
        public static int AbsorbLogic(int rawDamage, int remainingAbsorb, out int newRemaining)
        {
            if (rawDamage <= 0 || remainingAbsorb <= 0)
            {
                newRemaining = Mathf.Max(0, remainingAbsorb);
                return 0;
            }

            int absorbed = Mathf.Min(rawDamage, remainingAbsorb);
            newRemaining = remainingAbsorb - absorbed;
            return absorbed;
        }
    }
}
