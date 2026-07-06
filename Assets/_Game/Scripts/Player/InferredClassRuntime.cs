using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Skills;
using UnityEngine;

namespace CindarsHope.Player
{
    /// <summary>
    /// fable_39 — runtime recompute hook for the INFERRED player class. Reads the per-tree point
    /// counts the <see cref="SkillTreeManager"/> already owns, runs the pure
    /// <see cref="PlayerClassInference.GetProfile"/>, and exposes the result as the single named
    /// entry of the derived-stats provider (the project's static <c>Func</c> source-hook pattern,
    /// mirroring <see cref="PlayerVitalsApplier.CraftTimeReductionSource"/> and
    /// <c>PlayerDamageReceiver.ResistanceSource</c>). Recomputed on every
    /// <see cref="SkillDerivedStatsChangedEvent"/> (purchase / rank-up / respec / load) and at start.
    ///
    /// ZERO saved state: the profile is a pure function of the points the skill tree persists, so
    /// after load the first recompute reproduces the identical title and bonus. No GameObject.Find /
    /// FindObjectOfType at runtime (manager resolved via <see cref="GameBootstrap"/>); communication
    /// is event-bus only. The bonus enters as ONE named entry, replaced on each recompute — never
    /// double-counted across spread skill points.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InferredClassRuntime : MonoBehaviour
    {
        private static InferredClassRuntime _instance;

        private SkillTreeManager _skillTree;
        private bool _subscribed;

        public static InferredClassRuntime Instance => _instance;

        /// <summary>
        /// The current inferred-class profile (the single named provider entry). Defaults to
        /// <see cref="PlayerClassInference.Colono"/> until the first recompute, so callers never
        /// see a null/garbage value and a player with no investment reads as Colono (no bonus).
        /// </summary>
        public static PlayerClassInference.ClassProfile CurrentProfile { get; private set; }
            = PlayerClassInference.Colono;

        // ── Named identity-bonus source hooks (provider entry, read by the consumers) ─────────
        // Each returns the current magnitude on its axis (0 when Colono / not that class). Consumers
        // multiply by these (e.g. posture dealt, crit chance, mana cost, friendship, out-of-cave
        // stamina). The hooks are the single subscription point — replaced wholesale each recompute.

        /// <summary>+ posture dealt (Warrior). 0 when not applicable.</summary>
        public static System.Func<float> PostureDealtBonusSource = () =>
            CurrentProfile.BonusFor(PlayerClassInference.BonusAxis.PostureDealt);

        /// <summary>+ crit chance (Hunter). 0 when not applicable.</summary>
        public static System.Func<float> CritChanceBonusSource = () =>
            CurrentProfile.BonusFor(PlayerClassInference.BonusAxis.CritChance);

        /// <summary>- mana cost (Mystic). 0 when not applicable (caller treats as a reduction).</summary>
        public static System.Func<float> ManaCostReductionSource = () =>
            CurrentProfile.BonusFor(PlayerClassInference.BonusAxis.ManaCostReduction);

        /// <summary>+ stamina outside the cave (Farmer). 0 when not applicable.</summary>
        public static System.Func<float> StaminaOutOfCaveBonusSource = () =>
            CurrentProfile.BonusFor(PlayerClassInference.BonusAxis.StaminaOutOfCave);

        /// <summary>+ friendship gained (Bond). 0 when not applicable.</summary>
        public static System.Func<float> FriendshipBonusSource = () =>
            CurrentProfile.BonusFor(PlayerClassInference.BonusAxis.FriendshipGained);

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
        }

        private void Start()
        {
            var bootstrap = GameBootstrap.Instance;
            _skillTree = bootstrap != null ? bootstrap.SkillTreeManager : null;
            Recompute();
        }

        private void OnEnable()
        {
            if (!_subscribed)
            {
                GameEventBus.Subscribe<SkillDerivedStatsChangedEvent>(OnSkillStateChanged);
                _subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                GameEventBus.Unsubscribe<SkillDerivedStatsChangedEvent>(OnSkillStateChanged);
                _subscribed = false;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                // Reset the named entry so a torn-down runtime never leaves a stale bonus live.
                CurrentProfile = PlayerClassInference.Colono;
            }
        }

        private void OnSkillStateChanged(SkillDerivedStatsChangedEvent _)
        {
            Recompute();
        }

        /// <summary>
        /// Re-derive the profile from the live per-tree point counts. Resolves the manager lazily so
        /// it works even if it was wired after Start. Idempotent and side-effect free beyond
        /// replacing the single named entry (<see cref="CurrentProfile"/>).
        /// </summary>
        public void Recompute()
        {
            if (_skillTree == null)
            {
                var bootstrap = GameBootstrap.Instance;
                _skillTree = bootstrap != null ? bootstrap.SkillTreeManager : null;
            }

            if (_skillTree == null)
            {
                CurrentProfile = PlayerClassInference.Colono;
                return;
            }

            var points = new System.Collections.Generic.Dictionary<SkillTreeId, int>
            {
                { SkillTreeId.Melee, _skillTree.GetPointsSpentInTree("melee") },
                { SkillTreeId.Ranged, _skillTree.GetPointsSpentInTree("ranged") },
                { SkillTreeId.Magic, _skillTree.GetPointsSpentInTree("magic") },
                { SkillTreeId.Survival, _skillTree.GetPointsSpentInTree("survival") },
                { SkillTreeId.Crafting, _skillTree.GetPointsSpentInTree("crafting") },
            };

            CurrentProfile = PlayerClassInference.GetProfile(points);
        }
    }

    /// <summary>Ensures the inferred-class runtime exists (project bootstrap idiom).</summary>
    public static class InferredClassRuntimeBootstrap
    {
        public static InferredClassRuntime Install()
        {
            var existing = Object.FindAnyObjectByType<InferredClassRuntime>();
            if (existing != null)
            {
                return existing;
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return null;
            }

            var runtime = bootstrap.gameObject.AddComponent<InferredClassRuntime>();
            Debug.Log("[InferredClassRuntimeBootstrap] InferredClassRuntime instanciado via bootstrap.");
            return runtime;
        }
    }
}
