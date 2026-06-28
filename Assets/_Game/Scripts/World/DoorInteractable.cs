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
    /// 2. <b>Closed-shop door (notice):</b> when wired to a shop NPC (<see cref="_linkedNpcId"/>) and
    ///    that NPC is currently unavailable by schedule, o prompt avisa que a loja está fechada — mas a
    ///    ENTRADA no prédio continua liberada (o jogador pode entrar e encontrar o morador em casa à
    ///    noite). A trava de COMÉRCIO noturno (city_rules Rule 4) é aplicada no <c>NpcShopController</c>,
    ///    que recusa a venda quando o vendedor está indisponível — não na porta. Assim a porta é só
    ///    acesso ao prédio e o NPC permanece acessível dentro de casa no bloco "home".
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
                if (IsShopClosedNow())
                {
                    // Avisa que a loja está fechada, mas o jogador ainda pode entrar no prédio.
                    return $"{_doorLabel} ({ClosedNotice()})";
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

            // House/interior door: teleport the interactor to the paired position (same scene).
            // A entrada nunca é bloqueada — a loja fechada só recusa COMÉRCIO (no NpcShopController),
            // permitindo visitar o morador dentro de casa no horário "home".
            interactor.transform.position = TeleportTargetPosition;

            // Corta a câmera para o destino no mesmo frame, senão ela faria um pan suave pelo mapa
            // inteiro até o interior (y > +40) — a sensação de "ir para longe e voltar".
            GameEventBus.Publish(new CameraSnapRequestedEvent());

            var feedback = IsShopClosedNow() ? $"{_doorLabel} ({ClosedNotice()})" : _doorLabel;
            GameEventBus.Publish(new PlayerActionFeedbackEvent(feedback));
        }

        // Apenas INFORMATIVO: a loja vinculada está fora do horário agora? Não bloqueia a entrada;
        // só alimenta o aviso no prompt/feedback. A recusa de venda mora no NpcShopController.
        private bool IsShopClosedNow()
        {
            if (!_isShopDoor || string.IsNullOrEmpty(_linkedNpcId))
            {
                return false;
            }

            var service = NpcScheduleService.Instance;
            if (service == null)
            {
                return false; // fail-open: no schedule runtime → trata como porta comum
            }

            // Só avisa quando o serviço realmente rastreia este NPC (evita falso "fechado" pré-wiring).
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
