using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.NPC.Schedule;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// fable_11 (CA-3 / CA-4) — a functional building door. Two binding behaviors:
    ///
    /// 1. <b>House door (teleport):</b> teleports the interactor (player) to a paired target position
    ///    in the SAME Unity scene (no scene transition / no Build Settings change). Interiors live in
    ///    an off-playfield band (y &gt; +40, city_rules.md Rule 3). The interior's return door pairs
    ///    back to the exterior. The camera follows because it tracks the player transform.
    ///
    /// 2. <b>Closed-shop door (blocked):</b> when wired to a shop NPC (<see cref="_linkedNpcId"/>) and
    ///    that NPC is currently unavailable by schedule, the door is BLOCKED — the player cannot enter
    ///    and an opening-hours notice is shown (decision v2 §6.4-A, city_rules.md Rule 4). No silent
    ///    fail, never walks in.
    ///
    /// The door is data-only configured by the scene generator via <see cref="Configure"/>; it holds
    /// no Unity asset references and performs no global scene search.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DoorInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _interactorTeleportTarget;
        [SerializeField] private Vector3 _teleportTargetPosition;
        [SerializeField] private bool _useExplicitPosition;
        [SerializeField] private string _doorLabel = "Entrar";
        [SerializeField] private string _linkedNpcId;
        [SerializeField] private bool _isShopDoor;

        /// <summary>Stable target position the interactor is moved to on use.</summary>
        public Vector3 TeleportTargetPosition =>
            _useExplicitPosition || _interactorTeleportTarget == null
                ? _teleportTargetPosition
                : _interactorTeleportTarget.position;

        public string LinkedNpcId => _linkedNpcId;
        public bool IsShopDoor => _isShopDoor;

        /// <summary>
        /// Generator/editor configuration. <paramref name="targetPosition"/> is where the interactor
        /// teleports to (interior entry or exterior return). When <paramref name="linkedNpcId"/> is a
        /// shop NPC, the door becomes a closed-shop gate using that NPC's schedule availability.
        /// </summary>
        public void Configure(Vector3 targetPosition, string doorLabel, string linkedNpcId, bool isShopDoor)
        {
            _teleportTargetPosition = targetPosition;
            _useExplicitPosition = true;
            _doorLabel = string.IsNullOrEmpty(doorLabel) ? "Entrar" : doorLabel;
            _linkedNpcId = linkedNpcId;
            _isShopDoor = isShopDoor;
        }

        public string InteractionPrompt
        {
            get
            {
                if (IsBlockedByShopHours())
                {
                    return ClosedNotice();
                }

                return _doorLabel;
            }
        }

        // Door is always selectable so the player can read the closed-hours notice on a closed shop.
        public bool CanInteract(GameObject interactor) => interactor != null;

        public void Interact(GameObject interactor)
        {
            if (interactor == null)
            {
                return;
            }

            if (IsBlockedByShopHours())
            {
                // Blocked: show the opening-hours notice and do NOT teleport (decision v2 §6.4-A).
                GameEventBus.Publish(new PlayerActionFeedbackEvent(ClosedNotice()));
                return;
            }

            // House/interior door: teleport the interactor to the paired position (same scene).
            interactor.transform.position = TeleportTargetPosition;
            GameEventBus.Publish(new PlayerActionFeedbackEvent(_doorLabel));
        }

        private bool IsBlockedByShopHours()
        {
            if (!_isShopDoor || string.IsNullOrEmpty(_linkedNpcId))
            {
                return false;
            }

            var service = NpcScheduleService.Instance;
            if (service == null)
            {
                return false; // fail-open: no schedule runtime → door behaves as a plain door
            }

            // Only block when the service actually tracks this NPC (avoids false closes pre-wiring).
            if (!service.TryGetRuntimeState(_linkedNpcId, out var state) || state == null)
            {
                return false;
            }

            return !state.IsAvailable;
        }

        private string ClosedNotice()
        {
            return "Loja fechada. Volte no horario de funcionamento.";
        }
    }
}
