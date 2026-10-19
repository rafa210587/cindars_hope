using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_21 — discovery-knowledge core. Pure C# (no Unity refs, no scene access) so every
    /// threshold, the spoiler gate, the milestone accounting and the save round-trip are
    /// EditMode-testable. Hosted by the existing <see cref="BestiaryManager"/> (the only manager) —
    /// this class is NOT a MonoBehaviour and does NOT register on the bootstrap.
    ///
    /// Responsibilities (direction + FABLE_DECISOES §C1 thresholds + EMENDAS B/D):
    /// - record sightings / kills / effective hits per element / resisted hits / actions seen;
    /// - unlock categories by threshold and publish <see cref="BestiaryKnowledgeUnlockedEvent"/> once;
    /// - GrantKnowledge external source (Thalindra analysis, books) — idempotent;
    /// - IsVisible spoiler gate (SpoilerTier 3+ gated behind quest flags via an injected source);
    /// - boss creatures reveal their full ficha AFTER defeat (EMENDA-D);
    /// - FullyDocumented mechanical reward: small outgoing-damage bonus vs. the documented creature;
    /// - +1 skill point per 10 "Studied" creatures, max 5 — idempotent across save/load (EMENDA-B).
    ///
    /// The bestiary only REVEALS data — vulnerability/drop authoring stays in F06.
    /// </summary>
    public class EnemyKnowledgeService
    {
        // ── C1 thresholds (canonical) ────────────────────────────────────────────────────────────
        public const int SightingsForIdentity = 1;
        public const int ActionRepeatsForBehavior = 3;          // see same action 3x …
        public const int EffectiveHitsForVulnerability = 3;     // 3 effective hits on an axis
        public const int KillsForCommonDrops = 5;
        public const int ResistedHitsForResistance = 3;         // 3 "resisted" events

        // ── EMENDA-B: skill-point milestones ─────────────────────────────────────────────────────
        public const int StudiedPerMilestone = 10;
        public const int MaxMilestones = 5;

        // ── EMENDA-D: FullyDocumented mechanical reward ──────────────────────────────────────────
        // +3% outgoing damage vs. a creature whose ficha is FullyDocumented (K4 + all core categories).
        public const float FullyDocumentedDamageBonus = 0.03f;

        private readonly Dictionary<string, EnemyKnowledgeState> _states =
            new Dictionary<string, EnemyKnowledgeState>();

        // Number of "10 Studied" milestones already granted (persisted for idempotency).
        private int _milestonesGranted;

        /// <summary>
        /// Injected source for a creature's SpoilerTier (0..4). Defaults to 0 (no gate) when unset.
        /// Wired by the host to <c>CanonicalBestiaryCatalog</c>; tests inject synthetic tiers.
        /// </summary>
        public Func<string, int> SpoilerTierSource;

        /// <summary>
        /// Injected predicate: returns true when the given quest flag id is set. Defaults to
        /// "not set" when unset, so a SpoilerTier 3+ category stays hidden until the flag exists.
        /// Wired by the host to the quest flag service (F09); tests inject synthetic flag state.
        /// </summary>
        public Func<string, bool> QuestFlagSource;

        /// <summary>
        /// Injected mapping SpoilerTier → required quest flag id (for tiers that are gated). Returns
        /// null/empty when a tier needs no flag. Defaults to a built-in convention when unset.
        /// </summary>
        public Func<int, string> SpoilerTierFlagSource;

        /// <summary>
        /// Injected sink to grant skill points (the F34/F42 grant hook). Returns true on success.
        /// Defaults to a no-op; the host wires it to the skill tree / event publish. Independent of
        /// the published <see cref="BestiaryMilestoneReachedEvent"/> (which is the canonical bus path).
        /// </summary>
        public Func<int, bool> SkillPointGrantSink;

        /// <summary>Marker creatures that reveal the whole ficha after a single defeat (EMENDA-D).</summary>
        public Func<string, bool> IsBossSource;

        public int StatesCount => _states.Count;
        public int MilestonesGranted => _milestonesGranted;

        // ── Recording ────────────────────────────────────────────────────────────────────────────

        public void RecordSighting(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId)) return;
            var state = GetOrCreate(enemyId);
            state.Sightings++;
            EvaluateThresholds(state);
        }

        public void RecordKill(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId)) return;
            var state = GetOrCreate(enemyId);
            state.Sightings = Math.Max(state.Sightings, 1);
            state.Kills++;

            // Bosses reveal everything after defeat (EMENDA-D): unlock all categories at/below the
            // creature's SpoilerTier-appropriate set, gate still applies to VISIBILITY (IsVisible).
            if (IsBoss(enemyId))
            {
                RevealFullFicha(state, KnowledgeSource.BossDefeat);
            }

            EvaluateThresholds(state);
        }

        public void RecordEffectiveHit(string enemyId, string elementAxis)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || string.IsNullOrWhiteSpace(elementAxis)) return;
            var state = GetOrCreate(enemyId);
            state.Sightings = Math.Max(state.Sightings, 1);
            state.EffectiveHits.TryGetValue(elementAxis, out var current);
            state.EffectiveHits[elementAxis] = current + 1;
            EvaluateThresholds(state);
        }

        public void RecordResistedHit(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId)) return;
            var state = GetOrCreate(enemyId);
            state.Sightings = Math.Max(state.Sightings, 1);
            state.ResistedHits++;
            EvaluateThresholds(state);
        }

        /// <summary>Records an enemy action being observed. <paramref name="suffered"/> = the action
        /// hit the player (reveals behavior immediately).</summary>
        public void RecordActionSeen(string enemyId, string actionId, bool suffered = false)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || string.IsNullOrWhiteSpace(actionId)) return;
            var state = GetOrCreate(enemyId);
            state.Sightings = Math.Max(state.Sightings, 1);
            state.ActionsSeen.TryGetValue(actionId, out var current);
            state.ActionsSeen[actionId] = current + 1;
            if (suffered)
            {
                state.SufferedAction = true;
            }

            EvaluateThresholds(state);
        }

        // ── External grant (idempotent) ──────────────────────────────────────────────────────────

        /// <summary>
        /// External knowledge source (Thalindra analysis, Yael books). Unlocks the category without
        /// grind. Idempotent: a repeat grant does not duplicate state nor re-publish the event.
        /// Returns true only when this call actually unlocked something new.
        /// </summary>
        public bool GrantKnowledge(string enemyId, BestiaryKnowledgeCategory category, string source)
        {
            if (string.IsNullOrWhiteSpace(enemyId)) return false;
            var state = GetOrCreate(enemyId);
            bool unlocked = Unlock(state, category);
            if (unlocked)
            {
                // External grant of identity should also count as having seen the creature.
                state.Sightings = Math.Max(state.Sightings, 1);
                CheckMilestones();
            }

            return unlocked;
        }

        // ── Queries ──────────────────────────────────────────────────────────────────────────────

        public bool IsUnlocked(string enemyId, BestiaryKnowledgeCategory category)
        {
            return TryGet(enemyId, out var state) && state.IsUnlocked(category);
        }

        public EnemyKnowledgeLevel GetLevel(string enemyId)
        {
            return TryGet(enemyId, out var state) ? state.GetLevel() : EnemyKnowledgeLevel.Unknown;
        }

        public EnemyKnowledgeState GetState(string enemyId)
        {
            return TryGet(enemyId, out var state) ? state : null;
        }

        /// <summary>
        /// The single spoiler-gate decision point (regra de não duplicação). A category is visible
        /// only when (a) it is unlocked for the creature AND (b) the creature's SpoilerTier is not
        /// gated, or its gating quest flag is set. SpoilerTier 0-2 are never flag-gated; tier 3+
        /// require their flag. <paramref name="tier"/> overrides the SpoilerTierSource when >= 0.
        /// </summary>
        public bool IsVisible(string enemyId, BestiaryKnowledgeCategory category, int tier = -1)
        {
            if (!IsUnlocked(enemyId, category))
            {
                return false;
            }

            int spoilerTier = tier >= 0 ? tier : ResolveSpoilerTier(enemyId);
            if (spoilerTier < 3)
            {
                return true;
            }

            string flagId = ResolveSpoilerFlag(spoilerTier);
            if (string.IsNullOrWhiteSpace(flagId))
            {
                // Tier 3+ with no configured flag stays hidden (fail-safe: never leak a spoiler).
                return false;
            }

            return QuestFlagSource != null && QuestFlagSource(flagId);
        }

        /// <summary>
        /// True when the creature's ficha is FullyDocumented (K4 Studied). EMENDA-D mechanical reward
        /// consumers query this to apply the small damage bonus.
        /// </summary>
        public bool IsFullyDocumented(string enemyId)
        {
            return GetLevel(enemyId) == EnemyKnowledgeLevel.Studied;
        }

        /// <summary>
        /// Outgoing-damage multiplier vs. a creature (EMENDA-D): 1 + 3% when FullyDocumented, else 1.
        /// The damage pipeline (F02) multiplies by this; the bestiary does not author damage itself.
        /// </summary>
        public float GetDamageMultiplierVs(string enemyId)
        {
            return IsFullyDocumented(enemyId) ? 1f + FullyDocumentedDamageBonus : 1f;
        }

        public int StudiedCount()
        {
            int count = 0;
            foreach (var state in _states.Values)
            {
                if (state.GetLevel() == EnemyKnowledgeLevel.Studied)
                {
                    count++;
                }
            }

            return count;
        }

        // ── Save round-trip ──────────────────────────────────────────────────────────────────────

        public List<BestiaryKnowledgeEntrySaveData> CaptureKnowledge()
        {
            var list = new List<BestiaryKnowledgeEntrySaveData>(_states.Count);
            foreach (var state in _states.Values)
            {
                if (state != null && !string.IsNullOrWhiteSpace(state.EnemyId))
                {
                    list.Add(new BestiaryKnowledgeEntrySaveData(state));
                }
            }

            return list;
        }

        public int CaptureMilestonesGranted() => _milestonesGranted;

        public void RestoreKnowledge(List<BestiaryKnowledgeEntrySaveData> entries, int milestonesGranted)
        {
            _states.Clear();
            _milestonesGranted = Math.Min(MaxMilestones, Math.Max(0, milestonesGranted));

            if (entries == null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EnemyId))
                {
                    continue;
                }

                var state = entry.ToState();
                _states[state.EnemyId] = state;
            }
        }

        // ── Threshold evaluation ─────────────────────────────────────────────────────────────────

        private void EvaluateThresholds(EnemyKnowledgeState state)
        {
            bool anyUnlocked = false;

            if (state.Sightings >= SightingsForIdentity)
            {
                anyUnlocked |= Unlock(state, BestiaryKnowledgeCategory.Identity);
            }

            // Behavior: same action seen 3x OR any action suffered once.
            if (state.SufferedAction || HasActionRepeated(state, ActionRepeatsForBehavior))
            {
                anyUnlocked |= Unlock(state, BestiaryKnowledgeCategory.BehaviorSummary);
            }

            // Element vulnerability: 3 effective hits on any single axis.
            if (HasEffectiveAxis(state, EffectiveHitsForVulnerability))
            {
                anyUnlocked |= Unlock(state, BestiaryKnowledgeCategory.ElementVulnerability);
            }

            // Common drops: 5 kills.
            if (state.Kills >= KillsForCommonDrops)
            {
                anyUnlocked |= Unlock(state, BestiaryKnowledgeCategory.DropsCommon);
            }

            // Resistances: 3 "resisted" events (external grant is the other path, via GrantKnowledge).
            if (state.ResistedHits >= ResistedHitsForResistance)
            {
                anyUnlocked |= Unlock(state, BestiaryKnowledgeCategory.ResistanceTags);
            }

            if (anyUnlocked)
            {
                CheckMilestones();
            }
        }

        private static bool HasActionRepeated(EnemyKnowledgeState state, int threshold)
        {
            foreach (var count in state.ActionsSeen.Values)
            {
                if (count >= threshold) return true;
            }

            return false;
        }

        private static bool HasEffectiveAxis(EnemyKnowledgeState state, int threshold)
        {
            foreach (var count in state.EffectiveHits.Values)
            {
                if (count >= threshold) return true;
            }

            return false;
        }

        // ── Unlock + event publish (single source of truth) ──────────────────────────────────────

        private bool Unlock(EnemyKnowledgeState state, BestiaryKnowledgeCategory category)
        {
            string name = BestiaryKnowledgeCategories.ToName(category);
            if (state.UnlockedCategories.Contains(name))
            {
                return false; // idempotent — no duplicate state, no re-publish
            }

            state.UnlockedCategories.Add(name);
            GameEventBus.Publish(new BestiaryKnowledgeUnlockedEvent(
                state.EnemyId, name, ResolveSpoilerTier(state.EnemyId)));
            return true;
        }

        private void RevealFullFicha(EnemyKnowledgeState state, KnowledgeSource source)
        {
            // Unlock the four core categories (the set that defines K4 Studied) plus resistance tags.
            Unlock(state, BestiaryKnowledgeCategory.Identity);
            Unlock(state, BestiaryKnowledgeCategory.BehaviorSummary);
            Unlock(state, BestiaryKnowledgeCategory.ElementVulnerability);
            Unlock(state, BestiaryKnowledgeCategory.DropsCommon);
            Unlock(state, BestiaryKnowledgeCategory.ResistanceTags);
        }

        // ── Milestones (EMENDA-B) ────────────────────────────────────────────────────────────────

        private void CheckMilestones()
        {
            int studied = StudiedCount();
            int eligible = Math.Min(MaxMilestones, studied / StudiedPerMilestone);

            while (_milestonesGranted < eligible)
            {
                _milestonesGranted++;
                SkillPointGrantSink?.Invoke(1);
                GameEventBus.Publish(new BestiaryMilestoneReachedEvent(
                    _milestonesGranted, 1, studied));
            }
        }

        // ── Spoiler tier / flag resolution ───────────────────────────────────────────────────────

        private int ResolveSpoilerTier(string enemyId)
        {
            if (SpoilerTierSource == null || string.IsNullOrWhiteSpace(enemyId))
            {
                return 0;
            }

            int tier = SpoilerTierSource(enemyId);
            return tier < 0 ? 0 : tier;
        }

        private string ResolveSpoilerFlag(int tier)
        {
            if (SpoilerTierFlagSource != null)
            {
                return SpoilerTierFlagSource(tier);
            }

            // Default convention until a tier→flag map is wired (F10/F36 main-quest gates):
            // tier 3 = gate-boss progress flag; tier 4 = the Four (level 101) progress flag.
            switch (tier)
            {
                case 3: return "flag_bestiary_gate_boss_revealed";
                case 4: return "flag_bestiary_final_four_revealed";
                default: return null;
            }
        }

        private bool IsBoss(string enemyId)
        {
            return IsBossSource != null && !string.IsNullOrWhiteSpace(enemyId) && IsBossSource(enemyId);
        }

        // ── Internal helpers ─────────────────────────────────────────────────────────────────────

        private EnemyKnowledgeState GetOrCreate(string enemyId)
        {
            if (!_states.TryGetValue(enemyId, out var state))
            {
                state = new EnemyKnowledgeState { EnemyId = enemyId };
                _states[enemyId] = state;
            }

            return state;
        }

        private bool TryGet(string enemyId, out EnemyKnowledgeState state)
        {
            if (!string.IsNullOrWhiteSpace(enemyId) && _states.TryGetValue(enemyId, out state))
            {
                return true;
            }

            state = null;
            return false;
        }

        public enum KnowledgeSource
        {
            Combat,
            External,
            BossDefeat
        }
    }
}
