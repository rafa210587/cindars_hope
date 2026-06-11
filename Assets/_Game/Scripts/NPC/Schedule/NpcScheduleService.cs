using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// Runtime MonoBehaviour that bridges DayStartedEvent to NPC position resolution.
    /// TIME_BLOCK_DEBT: advances schedule only on DayStartedEvent (once per day).
    /// Intra-day period transitions (Morning/Midday/Evening/Night) deferred.
    /// Uses NpcDataSO.DefaultPosition as home anchor fallback.
    /// </summary>
    [DisallowMultipleComponent]
    public class NpcScheduleService : MonoBehaviour
    {
        private static NpcScheduleService s_instance;

        private readonly Dictionary<string, NpcScheduleAnchor> _anchors =
            new Dictionary<string, NpcScheduleAnchor>();

        private readonly Dictionary<string, NpcScheduleRuntimeState> _runtimeStates =
            new Dictionary<string, NpcScheduleRuntimeState>();

        // Registered NPC controllers (both dialogue and shop)
        private readonly List<NpcController> _npcControllers = new List<NpcController>();
        private readonly List<NpcShopController> _shopControllers = new List<NpcShopController>();

        public static NpcScheduleService Instance => s_instance;

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
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
        }

        private void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }

        /// <summary>
        /// Register a named anchor for schedule position resolution.
        /// </summary>
        public void RegisterAnchor(NpcScheduleAnchor anchor)
        {
            if (anchor == null || string.IsNullOrEmpty(anchor.AnchorId))
            {
                return;
            }

            _anchors[anchor.AnchorId] = anchor;
        }

        /// <summary>
        /// Register an NpcController for schedule management.
        /// </summary>
        public void RegisterNpcController(NpcController controller)
        {
            if (controller != null && !_npcControllers.Contains(controller))
            {
                _npcControllers.Add(controller);
            }
        }

        /// <summary>
        /// Register an NpcShopController for schedule management.
        /// </summary>
        public void RegisterShopController(NpcShopController controller)
        {
            if (controller != null && !_shopControllers.Contains(controller))
            {
                _shopControllers.Add(controller);
            }
        }

        /// <summary>
        /// Try to resolve a named anchor position.
        /// </summary>
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

        private void HandleDayStarted(DayStartedEvent evt)
        {
            // TIME_BLOCK_DEBT: resolve to DefaultPosition (home anchor) at day start
            // Full intra-day period schedule deferred until TimeBlockChangedEvent exists
            ResolveAllNpcsToHomePosition(evt.DayNumber);
        }

        private void ResolveAllNpcsToHomePosition(int dayNumber)
        {
            foreach (var controller in _npcControllers)
            {
                if (controller == null)
                {
                    continue;
                }

                ResolveNpcToDefaultPosition(controller.gameObject, controller.NpcData, dayNumber);
            }

            foreach (var controller in _shopControllers)
            {
                if (controller == null)
                {
                    continue;
                }

                ResolveNpcToDefaultPosition(controller.gameObject, controller.NpcData, dayNumber);
            }
        }

        private void ResolveNpcToDefaultPosition(GameObject npcGo, NpcDataSO npcData, int dayNumber)
        {
            if (npcGo == null || npcData == null)
            {
                return;
            }

            var npcId = npcData.NpcId;
            if (string.IsNullOrEmpty(npcId))
            {
                return;
            }

            // Try named anchor first
            var anchorId = $"npc_{npcId}_home";
            Vector3 targetPosition;
            if (!TryResolveAnchor(anchorId, out targetPosition))
            {
                // Fallback to NpcDataSO.DefaultPosition
                targetPosition = npcData.DefaultPosition;
            }

            // Only teleport if not currently interacting
            // Use NpcController.HasMet check only — IsInteracting not public
            npcGo.transform.position = targetPosition;

            // Update runtime state
            _runtimeStates[npcId] = new NpcScheduleRuntimeState
            {
                NpcId = npcId,
                CurrentScheduleId = $"schedule_{npcId}_default",
                CurrentScheduleBlock = NpcTimeBlock.Default.ToString(),
                CurrentAnchorId = anchorId,
                IsAvailable = true
            };

            Debug.Log($"[NpcScheduleService] Day {dayNumber}: moved {npcId} to home position {targetPosition}.");
        }

        /// <summary>
        /// Get the current runtime state of an NPC.
        /// </summary>
        public bool TryGetRuntimeState(string npcId, out NpcScheduleRuntimeState state)
        {
            return _runtimeStates.TryGetValue(npcId, out state);
        }
    }
}
