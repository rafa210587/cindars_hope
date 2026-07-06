using System;
using CindarsHope.Gameplay.Narrative;

namespace CindarsHope.Narrative
{
    /// <summary>
    /// fable_63 — auto-oferta IDEMPOTENTE da quest-ponte mq_act1_00 no PRIMEIRO DayStarted do save.
    ///
    /// Logica pura/testavel: nao referencia UnityEngine nem o GameEventBus diretamente. Recebe um
    /// <see cref="NarrativeFlagStore"/> (familia de flags persistida) e um delegate de aceite
    /// (QuestService.AcceptQuest em runtime). Checa a flag persistida ANTES de ofertar: reload +
    /// novo DayStarted NUNCA duplica a oferta (o pior bug de quest possivel — evitado por flag).
    ///
    /// O MonoBehaviour adapter (MainQuestHookRuntimeBootstrap) assina DayStartedEvent e injeta as
    /// dependencias reais.
    /// </summary>
    public sealed class MainQuestHookService
    {
        private readonly NarrativeFlagStore _flags;
        private readonly Func<string, bool> _acceptQuest;

        public MainQuestHookService(NarrativeFlagStore flags, Func<string, bool> acceptQuest)
        {
            _flags = flags;
            _acceptQuest = acceptQuest;
        }

        /// <summary>
        /// Chamado no DayStarted. Oferta/aceita mq_act1_00 exatamente 1x por save.
        /// A flag persistida e a barreira UNICA: uma vez tentada no 1o DayStarted, sela —
        /// reload + novo DayStarted nunca reabre/duplica a quest.
        /// Retorna true SOMENTE na primeira chamada efetiva (flag estava ausente).
        /// </summary>
        public bool OnDayStarted()
        {
            if (_flags == null) return false;
            if (_flags.IsSet(NarrativeIds.FlagHookOffered)) return false;

            // Sela a flag ANTES/junto da oferta: garante exatamente UMA tentativa de oferta por save.
            // O aceite em si tambem e idempotente no QuestService (quest ja ativa/concluida => recusa).
            _flags.Set(NarrativeIds.FlagHookOffered);
            _acceptQuest?.Invoke(NarrativeIds.MainQuestHookId);
            return true;
        }

        /// <summary>True se a quest-ponte ja foi ofertada neste save (idempotencia exposta para testes/diag).</summary>
        public bool AlreadyOffered => _flags != null && _flags.IsSet(NarrativeIds.FlagHookOffered);
    }
}
