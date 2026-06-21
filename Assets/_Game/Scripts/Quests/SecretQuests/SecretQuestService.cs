using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Runtime;

namespace CindarsHope.Quests.SecretQuests
{
    /// <summary>
    /// fable_52 — runtime orchestrator for the 8 cave-secret quests (scq_*) and their world effects.
    ///
    /// REUSE, not a second system: every offer goes through the single fable_34 channel
    /// (<see cref="QuestService.OfferSecretQuest"/> for discovery + <see cref="QuestService.AcceptDynamicInstance"/>
    /// for the live instance), rewards ride the single reward applicator, and flags live in the existing
    /// <see cref="QuestFlagService"/>. It owns no quest registry, no second reward formula, no second
    /// flag store. The merchant-list sequence (1×/run each, stable per seed) is the only stateful bit and
    /// is a deterministic index over <see cref="SecretQuestCatalog.MerchantListSequence"/>.
    ///
    /// Pure-testable surface (EditMode): the Offer*/Register* methods, the duel + incubator trackers, and
    /// the world-effects queries. The GameEventBus subscriptions (<see cref="Subscribe"/>/<see cref="Unsubscribe"/>)
    /// are a thin adapter over that surface.
    /// </summary>
    public sealed class SecretQuestService
    {
        private readonly QuestService _questService;
        private readonly SecretQuestWorldEffects _worldEffects;
        private readonly Func<int> _playerLevelProvider;

        private readonly PackDuelTracker _duel = new PackDuelTracker();
        private readonly DragonEggIncubator _incubator = new DragonEggIncubator();

        // Which merchant lists have already been offered this run (index into MerchantListSequence).
        private int _merchantListsOffered;
        private string _currentRunSeed;
        private bool _subscribed;

        public SecretQuestService(
            QuestService questService,
            SecretQuestWorldEffects worldEffects,
            Func<int> playerLevelProvider = null)
        {
            _questService = questService;
            _worldEffects = worldEffects;
            _playerLevelProvider = playerLevelProvider ?? (() => 1);
        }

        public PackDuelTracker Duel => _duel;
        public DragonEggIncubator Incubator => _incubator;
        public SecretQuestWorldEffects WorldEffects => _worldEffects;
        public int MerchantListsOffered => _merchantListsOffered;

        // ─── Discovery + offer (CA-1) ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Offers a single canonical scq_* through the fable_34 channel: marks it discovered (so it
        /// appears in the Secrets tab) and accepts the live instance so progress/turn-in resolve.
        /// Returns the concrete quest id, or null when the id is not canonical / already accepted.
        /// </summary>
        public string Offer(string questId, int caveLevel)
        {
            if (_questService == null || !SecretQuestCatalog.IsCanonical(questId)) return null;

            // Discovery (idempotent): opens the Secrets tab entry and publishes SecretQuestDiscoveredEvent.
            _questService.OfferSecretQuest(questId);

            // Already active/completed? Do not re-accept (idempotent).
            var existing = _questService.GetQuestState(questId);
            if (existing != null) return questId;

            var instance = SecretQuestCatalog.Build(questId, caveLevel, _playerLevelProvider());
            if (instance == null) return null;

            if (!_questService.AcceptDynamicInstance(instance)) return null;
            return questId;
        }

        /// <summary>
        /// The wandering merchant offers the NEXT merchant list in the fixed sequence (1×/run each,
        /// stable per seed). Returns the offered quest id, or null when all 3 have been offered this run.
        /// The run seed resets the sequence (a new run re-offers from list 1).
        /// </summary>
        public string OfferNextMerchantList(string runSeed, int caveLevel)
        {
            if (runSeed != _currentRunSeed)
            {
                _currentRunSeed = runSeed;
                _merchantListsOffered = 0;
            }
            if (_merchantListsOffered >= SecretQuestCatalog.MerchantListSequence.Count) return null;

            var questId = SecretQuestCatalog.MerchantListSequence[_merchantListsOffered];
            var offered = Offer(questId, caveLevel);
            if (offered != null) _merchantListsOffered++;
            return offered;
        }

        /// <summary>A peaceful-creature interactable (F33) offers its bound secret (Scrounger/Warden/Warchief).</summary>
        public string OfferFromPeacefulCreature(string creatureId, int caveLevel)
        {
            var questId = SecretForCreature(creatureId);
            return questId == null ? null : Offer(questId, caveLevel);
        }

        /// <summary>Maps a F33 non-aggressive creature id to the secret it offers (null = none).</summary>
        public static string SecretForCreature(string creatureId)
        {
            switch (creatureId)
            {
                case "enemy_scrounger_king":
                case "scrounger_king": return SecretQuestCatalog.ScroungerBargainId;
                case "enemy_silence_warden":
                case "silence_warden": return SecretQuestCatalog.WardenOfferingId;
                case "enemy_goblin_warchief":
                case "goblin_warchief": return SecretQuestCatalog.GoblinTruceId;
                default: return null;
            }
        }

        // ─── Pack duel (CA-4) ───────────────────────────────────────────────────────────────────────

        /// <summary>Begins the Warchief duel window (scopes pack-death tracking to this encounter).</summary>
        public void BeginWarchiefDuel(string encounterId, IEnumerable<string> packIds)
        {
            _duel.Begin(encounterId, packIds);
        }

        /// <summary>An enemy died — dirties the duel if it is a watched pack member inside the window.</summary>
        public void OnEnemyDied(string enemyId) => _duel.OnEnemyDied(enemyId);

        /// <summary>
        /// The Warchief was defeated. If the pack survived, completes scq_goblin_truce through the
        /// existing flow; otherwise the duel is dirty and the truce is simply re-offerable next run
        /// (no softlock). Closes the duel window.
        /// </summary>
        public bool OnWarchiefDefeated(string warchiefId)
        {
            _duel.OnWarchiefDefeated(warchiefId);
            bool earned = _duel.IsTruceEarned();
            if (earned) CompleteObjective(SecretQuestCatalog.GoblinTruceId);
            _duel.End();
            return earned;
        }

        // ─── Dragon egg incubator (CA-1/§dragon_egg) ─────────────────────────────────────────────────

        /// <summary>Places the Ashwing egg in Nimble's incubator (starts the cosmetic day timer).</summary>
        public void StartEggIncubation(int currentDay) => _incubator.StartIncubation(currentDay);

        /// <summary>Advances the incubator; on hatch, completes scq_dragon_egg through the existing flow.</summary>
        public bool TickEggIncubation(int currentDay)
        {
            bool hatched = _incubator.Tick(currentDay);
            if (hatched) CompleteObjective(SecretQuestCatalog.DragonEggId);
            return hatched;
        }

        // ─── Run lifecycle (CA-3) ───────────────────────────────────────────────────────────────────

        /// <summary>A new run begins: reset the merchant-list sequence and expire the per-run goblin band.</summary>
        public void OnNewRunStarted(string newRunSeed)
        {
            _currentRunSeed = newRunSeed;
            _merchantListsOffered = 0;
            _duel.End();
            _worldEffects?.OnNewRunStarted();
        }

        /// <summary>CA-3 — re-applies persisted permanent world flags after a save load.</summary>
        public void RehydrateWorldEffects(IEnumerable<string> grantedFlagIds)
        {
            _worldEffects?.RehydrateFromGrantedFlags(grantedFlagIds);
        }

        // ─── GameEventBus adapter (thin) ─────────────────────────────────────────────────────────────

        public void Subscribe()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<EnemyKilledEvent>(HandleEnemyKilled);
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
            _subscribed = true;
        }

        public void Unsubscribe()
        {
            if (!_subscribed) return;
            GameEventBus.Unsubscribe<EnemyKilledEvent>(HandleEnemyKilled);
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
            _subscribed = false;
        }

        private void HandleEnemyKilled(EnemyKilledEvent evt)
        {
            OnEnemyDied(evt.EnemyId);
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            TickEggIncubation(evt.DayNumber);
        }

        // ─── Internals ────────────────────────────────────────────────────────────────────────────

        private void CompleteObjective(string questId)
        {
            var record = _questService.GetQuestState(questId);
            if (record == null) return; // not accepted yet — nothing to complete (no softlock)
            _questService.MarkObjectiveComplete(questId, "obj_" + questId);
        }
    }
}
