using System;
using System.Collections.Generic;
using CindarsHope.Core.Events;

namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — tabela ESTÁTICA evento→categoria (CA-2). Fonte única de verdade
    /// do mapeamento; o <see cref="SfxEventBridge"/> usa-a para assinar e disparar.
    ///
    /// Classe pura (usa apenas tipos de evento, sem AudioSource) → testável: o teste
    /// confirma que TODO evento audível v1 tem categoria não-None.
    ///
    /// Nota sobre DamageBlockedEvent (listado no escopo da spec): esse evento NÃO
    /// existe no projeto (auditoria Fase 0). Como esta spec NÃO pode criar eventos de
    /// gameplay, ele não é assinado. A categoria <see cref="SfxCategory.Block"/>
    /// existe e fica RESERVADA: o consumidor futuro que publicar DamageBlockedEvent
    /// adiciona a linha aqui (1 linha) sem tocar no AudioManager. O feedback de block
    /// no v1 já é coberto por PlayerPerfectBlockEvent → PerfectBlock.
    /// </summary>
    public static class SfxEventMap
    {
        /// <summary>
        /// Eventos audíveis v1 (que EXISTEM no projeto) e suas categorias.
        /// A chave é o tipo do evento; o valor é a categoria de SFX.
        /// </summary>
        private static readonly IReadOnlyDictionary<Type, SfxCategory> Map = new Dictionary<Type, SfxCategory>
        {
            { typeof(PlayerChargedAttackEvent), SfxCategory.Charged },
            { typeof(EnemyPostureBrokenEvent), SfxCategory.PostureBreak },
            { typeof(PlayerPerfectBlockEvent), SfxCategory.PerfectBlock },
            { typeof(StatusEffectAppliedEvent), SfxCategory.Status },
            { typeof(DamageAppliedEvent), SfxCategory.Hit },
            { typeof(PlayerDamagedEvent), SfxCategory.Hit },
            { typeof(EnemyKilledEvent), SfxCategory.EnemyKilled },
            { typeof(NotificationToastRequestedEvent), SfxCategory.UiToast },
            { typeof(PlayerActionFeedbackEvent), SfxCategory.UiToast },
            { typeof(ItemPickedUpEvent), SfxCategory.Pickup },
            { typeof(ItemCraftedEvent), SfxCategory.Craft },
            { typeof(CropHarvestedEvent), SfxCategory.Harvest },
            { typeof(FishCaughtEvent), SfxCategory.Fish },
            { typeof(PlayerLevelChangedEvent), SfxCategory.LevelUp },
            { typeof(DayStartedEvent), SfxCategory.DayStart },
            { typeof(GameSavedEvent), SfxCategory.Save }
        };

        /// <summary>Número de eventos audíveis v1 mapeados.</summary>
        public static int Count => Map.Count;

        /// <summary>Todos os pares evento→categoria (para subscription e testes).</summary>
        public static IEnumerable<KeyValuePair<Type, SfxCategory>> Entries => Map;

        /// <summary>Categoria para um tipo de evento, ou None se não mapeado.</summary>
        public static SfxCategory CategoryFor(Type eventType)
        {
            if (eventType != null && Map.TryGetValue(eventType, out var category))
            {
                return category;
            }

            return SfxCategory.None;
        }

        /// <summary>Categoria para um tipo de evento (genérico).</summary>
        public static SfxCategory CategoryFor<TEvent>()
        {
            return CategoryFor(typeof(TEvent));
        }
    }
}
