using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Crafting
{
    /// <summary>
    /// fable_49 — hook de first-kill: ao derrotar o boss de um gate (CaveBossDefeatedEvent), aprende a
    /// receita de tier alto mapeada para aquele gate (HighTierGearCanon.RecipeUnlockForGate). Idempotente:
    /// usa RecipeUnlockService.Unlock (re-kill = no-op) e só publica RecipeLearnedEvent na PRIMEIRA vez.
    /// O serviço persiste o unlock no save (aditivo) — sobrevive a save/load.
    ///
    /// Subscrição via RuntimeInitializeOnLoadMethod (uma vez por sessão, sem MonoBehaviour, sem busca
    /// global de cena). Se F06/F33 ainda não publicar CaveBossDefeatedEvent no gate certo, nenhum unlock
    /// automático ocorre — a regra permanece "nunca unlock silencioso"; o serviço continua expondo
    /// Unlock(id) para integração futura. A lógica de mapeamento é estática e pura (testável em EditMode).
    /// </summary>
    public static class RecipeFirstKillUnlockHook
    {
        private static bool _subscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (_subscribed) return;
            _subscribed = true;
            GameEventBus.Subscribe<CaveBossDefeatedEvent>(OnBossDefeated);
        }

        private static void OnBossDefeated(CaveBossDefeatedEvent evt)
        {
            TryApplyFirstKillUnlock(RecipeUnlockService.Active, evt.CaveLevel, publishEvent: true);
        }

        /// <summary>
        /// Núcleo PURO e testável: dado o serviço e o nível do gate, aprende a receita do gate (se houver)
        /// de forma idempotente. Retorna true só na PRIMEIRA vez (1× de evento). Serviço null ou gate sem
        /// receita de arma => no-op. Publica RecipeLearnedEvent apenas se publishEvent e foi a primeira vez.
        /// </summary>
        public static bool TryApplyFirstKillUnlock(RecipeUnlockService service, int gateLevel, bool publishEvent)
        {
            if (service == null) return false;

            string slug = HighTierGearCanon.RecipeUnlockForGate(gateLevel);
            if (string.IsNullOrWhiteSpace(slug)) return false;

            bool firstTime = service.Unlock(slug);
            if (firstTime && publishEvent)
            {
                GameEventBus.Publish(new RecipeLearnedEvent(slug));
            }
            return firstTime;
        }
    }
}
