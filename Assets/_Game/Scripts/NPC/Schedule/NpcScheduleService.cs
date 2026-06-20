using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// Runtime MonoBehaviour that resolves each town NPC's Work/Social/Home/Night block from the
    /// current hour and moves the NPC toward the matching anchor.
    ///
    /// fable_11 closes the two WAVE25 debts:
    /// - TIME_BLOCK_DEBT: blocks are now resolved per hour (via <see cref="NpcScheduleBlockResolver"/>),
    ///   not once per day. The service subscribes to <see cref="GameTimeTickEvent"/> and re-resolves
    ///   on every tick, publishing <see cref="NpcScheduleBlockChangedEvent"/> on block transitions.
    /// - SCENE_WIRING_DEBT: the generator now emits 3 anchors per NPC (work/social/home), registered
    ///   here at bootstrap.
    ///
    /// Movement uses <c>NpcWanderer.SetDestination</c>; an NPC blocked en route teleports to the anchor
    /// after a fallback timeout. NPCs mid-interaction keep their schedule paused (handled by the
    /// controllers + wanderer's existing SetInteractionPaused).
    /// </summary>
    [DisallowMultipleComponent]
    public class NpcScheduleService : MonoBehaviour
    {
        private const float StuckTeleportSeconds = 5f;
        private const float ArriveRadius = 0.3f;

        private static NpcScheduleService s_instance;

        private readonly Dictionary<string, NpcScheduleAnchor> _anchors =
            new Dictionary<string, NpcScheduleAnchor>();

        private readonly Dictionary<string, NpcScheduleRuntimeState> _runtimeStates =
            new Dictionary<string, NpcScheduleRuntimeState>();

        private readonly Dictionary<string, NpcScheduleProfile> _profiles =
            new Dictionary<string, NpcScheduleProfile>();

        // Registered NPC controllers (both dialogue and shop)
        private readonly List<NpcController> _npcControllers = new List<NpcController>();
        private readonly List<NpcShopController> _shopControllers = new List<NpcShopController>();

        // Tracks per-NPC movement (target + stuck timer) keyed by npcId.
        private readonly Dictionary<string, MoveOrder> _moveOrders = new Dictionary<string, MoveOrder>();

        private GameTimeManager _timeManager;
        private int _lastResolvedHour = -1;

        public static NpcScheduleService Instance => s_instance;

        private struct MoveOrder
        {
            public Transform Npc;
            public NpcWanderer Wanderer;
            public Vector3 Target;
            public float ElapsedBlocked;
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
            GameEventBus.Subscribe<GameTimeTickEvent>(HandleTimeTick);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
            GameEventBus.Unsubscribe<GameTimeTickEvent>(HandleTimeTick);
        }

        private void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }

        /// <summary>Inject the time source (bootstrap). Optional — the service degrades to day-start only.</summary>
        public void SetTimeManager(GameTimeManager timeManager)
        {
            _timeManager = timeManager;
        }

        /// <summary>Register a named anchor for schedule position resolution.</summary>
        public void RegisterAnchor(NpcScheduleAnchor anchor)
        {
            if (anchor == null || string.IsNullOrEmpty(anchor.AnchorId))
            {
                return;
            }

            _anchors[anchor.AnchorId] = anchor;
        }

        /// <summary>Register an NpcController for schedule management and seed its profile.</summary>
        public void RegisterNpcController(NpcController controller)
        {
            if (controller != null && !_npcControllers.Contains(controller))
            {
                _npcControllers.Add(controller);
                EnsureProfile(controller.NpcData, hasShop: false);
            }
        }

        /// <summary>Register an NpcShopController for schedule management and seed its profile.</summary>
        public void RegisterShopController(NpcShopController controller)
        {
            if (controller != null && !_shopControllers.Contains(controller))
            {
                _shopControllers.Add(controller);
                EnsureProfile(controller.NpcData, hasShop: true);
            }
        }

        /// <summary>
        /// Register (or replace) the schedule profile for an NPC. Used by the generator/bootstrap to
        /// assign the canonical archetype (e.g. Yael/Maelor = Night). Idempotent.
        /// </summary>
        public void RegisterProfile(NpcScheduleProfile profile)
        {
            if (profile == null || string.IsNullOrEmpty(profile.NpcId))
            {
                return;
            }

            _profiles[profile.NpcId] = profile;
        }

        private void EnsureProfile(NpcDataSO npcData, bool hasShop)
        {
            if (npcData == null || string.IsNullOrEmpty(npcData.NpcId))
            {
                return;
            }

            if (_profiles.ContainsKey(npcData.NpcId))
            {
                return;
            }

            // Default archetype inferred from roster movement profile when available, else shop/wander.
            var archetype = NpcScheduleArchetype.Shopkeeper;
            if (!hasShop)
            {
                archetype = NpcScheduleArchetype.Wanderer;
            }

            _profiles[npcData.NpcId] = NpcScheduleProfile.CreateForArchetype(npcData.NpcId, archetype);
        }

        /// <summary>Try to resolve a named anchor position.</summary>
        public bool TryResolveAnchor(string anchorId, out Vector3 position)
        {
            position = Vector3.zero;
            if (string.IsNullOrEmpty(anchorId))
            {
                return false;
            }

            if (_anchors.TryGetValue(anchorId, out var anchor) && anchor != null)
            {
                position = anchor.GetPosition();
                return true;
            }

            return false;
        }

        /// <summary>
        /// fable_11 (CA-1) — current Work/Social/Home/Night block for an NPC at the given hour.
        /// Pure passthrough to the resolver using the NPC's registered archetype.
        /// </summary>
        public NpcRuntimeBlock GetCurrentBlock(string npcId, int hour)
        {
            var archetype = ArchetypeFor(npcId);
            return NpcScheduleBlockResolver.ResolveBlock(archetype, hour);
        }

        /// <summary>Current block using the live time source (falls back to hour 12 if no time manager).</summary>
        public NpcRuntimeBlock GetCurrentBlock(string npcId)
        {
            return GetCurrentBlock(npcId, CurrentHour());
        }

        /// <summary>
        /// fable_11 (CA-4) — is the NPC available for shop/dialogue right now? An NPC at Home/Night is
        /// unavailable; night-types invert (available only at night). Uses the live time source.
        /// </summary>
        public bool IsAvailable(string npcId)
        {
            return IsAvailable(npcId, CurrentHour());
        }

        /// <summary>Availability at an explicit hour (testable).</summary>
        public bool IsAvailable(string npcId, int hour)
        {
            var archetype = ArchetypeFor(npcId);
            return NpcScheduleBlockResolver.IsAvailable(archetype, hour);
        }

        private NpcScheduleArchetype ArchetypeFor(string npcId)
        {
            if (!string.IsNullOrEmpty(npcId) && _profiles.TryGetValue(npcId, out var profile) && profile != null)
            {
                return profile.Archetype;
            }

            return NpcScheduleArchetype.Shopkeeper;
        }

        private int CurrentHour()
        {
            return _timeManager != null ? _timeManager.CurrentHourOfDay : 12;
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            // Force a re-resolution at the start of each day.
            _lastResolvedHour = -1;
            ResolveAllNpcs(CurrentHour());
        }

        private void HandleTimeTick(GameTimeTickEvent evt)
        {
            var hour = CurrentHour();
            if (hour == _lastResolvedHour)
            {
                return;
            }

            ResolveAllNpcs(hour);
        }

        private void ResolveAllNpcs(int hour)
        {
            _lastResolvedHour = hour;

            foreach (var controller in _npcControllers)
            {
                if (controller == null)
                {
                    continue;
                }

                ResolveNpc(controller.gameObject, controller.NpcData, hour,
                    controller.GetComponent<NpcWanderer>());
            }

            foreach (var controller in _shopControllers)
            {
                if (controller == null)
                {
                    continue;
                }

                ResolveNpc(controller.gameObject, controller.NpcData, hour,
                    controller.GetComponent<NpcWanderer>());
            }
        }

        private void ResolveNpc(GameObject npcGo, NpcDataSO npcData, int hour, NpcWanderer wanderer)
        {
            if (npcGo == null || npcData == null || string.IsNullOrEmpty(npcData.NpcId))
            {
                return;
            }

            var npcId = npcData.NpcId;
            var archetype = ArchetypeFor(npcId);
            var block = NpcScheduleBlockResolver.ResolveBlock(archetype, hour);
            var available = block == NpcRuntimeBlock.Work || block == NpcRuntimeBlock.Social;
            var suffix = NpcScheduleBlockResolver.AnchorSuffixForBlock(block);
            var anchorId = $"npc_{npcId}_{suffix}";

            // Resolve target position: named anchor first, else NpcDataSO.DefaultPosition.
            if (!TryResolveAnchor(anchorId, out var targetPosition))
            {
                targetPosition = npcData.DefaultPosition;
            }

            // Update runtime state and publish the block transition (only on change).
            var changed = !_runtimeStates.TryGetValue(npcId, out var prev)
                          || prev == null
                          || prev.CurrentScheduleBlock != block.ToString();

            _runtimeStates[npcId] = new NpcScheduleRuntimeState
            {
                NpcId = npcId,
                CurrentScheduleId = $"schedule_{npcId}",
                CurrentScheduleBlock = block.ToString(),
                CurrentAnchorId = anchorId,
                IsAvailable = available
            };

            if (changed)
            {
                GameEventBus.Publish(new NpcScheduleBlockChangedEvent(npcId, block.ToString(), available));
            }

            // Order movement toward the target (smooth via wanderer; teleport fallback if stuck).
            _moveOrders[npcId] = new MoveOrder
            {
                Npc = npcGo.transform,
                Wanderer = wanderer,
                Target = targetPosition,
                ElapsedBlocked = 0f
            };

            if (wanderer != null)
            {
                wanderer.SetDestination(targetPosition, ArriveRadius);
            }
        }

        private void Update()
        {
            if (_moveOrders.Count == 0)
            {
                return;
            }

            // Iterate over a snapshot of keys so we can mutate the dictionary.
            _stuckScratch.Clear();
            foreach (var kvp in _moveOrders)
            {
                _stuckScratch.Add(kvp.Key);
            }

            for (var i = 0; i < _stuckScratch.Count; i++)
            {
                var npcId = _stuckScratch[i];
                if (!_moveOrders.TryGetValue(npcId, out var order) || order.Npc == null)
                {
                    _moveOrders.Remove(npcId);
                    continue;
                }

                var distance = Vector2.Distance(order.Npc.position, order.Target);
                if (distance <= ArriveRadius)
                {
                    _moveOrders.Remove(npcId);
                    continue;
                }

                order.ElapsedBlocked += Time.deltaTime;
                if (order.ElapsedBlocked >= StuckTeleportSeconds)
                {
                    // Teleport fallback: NPC stuck behind a collider on the way to its anchor.
                    order.Npc.position = order.Target;
                    _moveOrders.Remove(npcId);
                    continue;
                }

                _moveOrders[npcId] = order;
            }
        }

        private readonly List<string> _stuckScratch = new List<string>();

        /// <summary>Get the current runtime state of an NPC.</summary>
        public bool TryGetRuntimeState(string npcId, out NpcScheduleRuntimeState state)
        {
            return _runtimeStates.TryGetValue(npcId, out state);
        }
    }
}
