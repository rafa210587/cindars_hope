using CindarsHope.City.Services;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.NPC.Events;
using CindarsHope.Quests.NpcChains;
using UnityEngine;

namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_46 — host do romance por cima da amizade (F26). Envolve o núcleo puro
    /// <see cref="RomanceState"/>, injeta as probes de gate reais (amizade via FriendshipService;
    /// cadeia F35 via done-flag do passo final no QuestFlagService; flag de ato F36) e publica
    /// <see cref="RomanceStageChangedEvent"/>/<see cref="RomanceConfessionRejectedEvent"/> no
    /// GameEventBus. Hospedado no MESMO GameObject do FriendshipService (sem novo singleton de
    /// gameplay; sem GameObject.Find runtime — segue o padrão dos *RuntimeBootstrap do projeto).
    ///
    /// Persistência: o romance é ADITIVO na seção de amizade (F26). Este serviço escreve/lê os campos
    /// extras da FriendshipSaveData no momento do Capture/Restore do FriendshipService (ver hooks).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RomanceService : MonoBehaviour
    {
        private readonly RomanceState _state = new RomanceState();
        private int _currentDay = 1;

        private static RomanceService _instance;
        public static RomanceService Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>Setter de dia (normalmente espelha o FriendshipService, via DayStartedEvent).</summary>
        public void SetCurrentDay(int day) => _currentDay = day;

        // ── API pública (consumida por diálogo F28 e pelo HUD) ───────────────────────────────────

        public RomanceStage GetStage(string npcId) => _state.GetStage(npcId);
        public bool IsPartner(string npcId) => _state.IsPartner(npcId);
        public System.Collections.Generic.IReadOnlyList<string> Partners() => _state.Partners();
        public int PartnerCount() => _state.PartnerCount();

        public bool CanConfess(string npcId) => _state.CanConfess(npcId, BuildProbe());
        public RomanceConfessionRejection EvaluateConfession(string npcId)
            => _state.EvaluateConfession(npcId, BuildProbe());

        /// <summary>
        /// Tenta confessar. Sucesso ⇒ Interesse + RomanceStageChangedEvent. Recusa ⇒
        /// RomanceConfessionRejectedEvent com o motivo (limite de 2, gate, já romanceado).
        /// </summary>
        public bool Confess(string npcId)
        {
            var result = _state.Confess(npcId, BuildProbe(), _currentDay);
            if (result.Accepted)
            {
                GameEventBus.Publish(new RomanceStageChangedEvent(npcId, result.NewStage, RomanceStage.None));
                return true;
            }
            GameEventBus.Publish(new RomanceConfessionRejectedEvent(npcId, result.Rejection));
            return false;
        }

        /// <summary>Registra uma interação de parceiro e publica o evento se houver avanço de estágio.</summary>
        public void RegisterPartnerInteraction(string npcId)
        {
            PublishIfAdvanced(npcId, _state.RegisterPartnerInteraction(npcId));
        }

        // ── Hook de presente (chamado pelo FriendshipService no ponto único de presente) ─────────

        /// <summary>
        /// Multiplicador de pontos de presente para este NPC: parceiro (Namoro+) rende +50% (x1.5);
        /// caso contrário 1.0. Aplicado pelo FriendshipService APÓS o cap diário do F26 (anti-exploit).
        /// </summary>
        public float GiftPointMultiplierFor(string npcId)
        {
            return _state.IsPartner(npcId) ? RomanceEligibilityTable.PartnerGiftPointMultiplier : 1f;
        }

        /// <summary>
        /// Notifica que um presente foi ACEITO por este NPC (após o cap). Conta como marco de presente
        /// se há romance ativo e publica avanço de estágio quando os marcos do estágio são batidos.
        /// </summary>
        public void NotifyGiftAccepted(string npcId)
        {
            PublishIfAdvanced(npcId, _state.RegisterPartnerGift(npcId));
        }

        // ── Save aditivo (espelhado pelo FriendshipService Capture/Restore) ──────────────────────

        public void WriteSaveFields(FriendshipSaveData data) => _state.WriteInto(data);
        public void RestoreSaveFields(FriendshipSaveData data) => _state.RestoreFrom(data);

        // ── Probes de gate (leitura pura de outros sistemas; sem GameObject.Find) ─────────────────

        private RomanceState.GateProbe BuildProbe()
        {
            return new RomanceState.GateProbe(
                friendshipAtLeast: FriendshipAtLeast,
                chainComplete: IsChainComplete,
                flagSet: IsFlagSet);
        }

        private static bool FriendshipAtLeast(string npcId, int level)
        {
            var fs = FriendshipService.Instance;
            return fs != null && fs.IsAtLeast(npcId, level);
        }

        /// <summary>Cadeia pessoal F35 do NPC concluída = done-flag do passo final setada no QuestFlagService.</summary>
        private static bool IsChainComplete(string npcId)
        {
            var steps = NpcQuestChainCatalog.ForNpc(npcId);
            if (steps == null || steps.Count == 0)
            {
                // NPC sem cadeia F35 ⇒ esse gate não se aplica (não bloqueia a confissão).
                return true;
            }
            var finalStep = steps[steps.Count - 1];
            return IsFlagSet(NpcQuestChainCatalog.DoneFlag(finalStep.QuestId));
        }

        private static bool IsFlagSet(string flagId)
        {
            if (string.IsNullOrEmpty(flagId)) return false;
            var flagService = CityServiceRuntimeBootstrap.FlagService;
            return flagService != null && flagService.IsSet(flagId);
        }

        // ── Publicação de evento ─────────────────────────────────────────────────────────────────

        private void PublishIfAdvanced(string npcId, RomanceState.AdvanceResult result)
        {
            if (!result.Changed) return;
            GameEventBus.Publish(new RomanceStageChangedEvent(npcId, result.NewStage, result.PreviousStage));
        }
    }
}
