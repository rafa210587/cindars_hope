using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.NPC;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// Porta de casa FÍSICA e funcional. Fica FECHADA por padrão: a folha bloqueia o vão da parede
    /// (collider sólido). Ao apertar E o jogador ABRE a porta — a folha desliza para o lado, o collider
    /// de bloqueio é desligado e ele entra a pé no interior (sem teleporte, sem cena separada). Apertar E
    /// de novo fecha. Um collider-trigger separado (sempre ligado) mantém a porta interagível aberta ou
    /// fechada, para que o jogador possa fechá-la por dentro.
    ///
    /// Segura só referências serializadas — nenhuma busca global de cena. O gerador pluga via Configure.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HouseDoorInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _leaf;          // folha visível da porta (desliza ao abrir)
        [SerializeField] private Collider2D _blocker;      // collider sólido que tranca o vão (off = aberto)
        [SerializeField] private Vector3 _closedLocalPos;  // posição da folha fechada
        [SerializeField] private Vector3 _openLocalPos;    // posição da folha aberta (deslizada p/ o lado)
        [SerializeField] private bool _isOpen;

        // Auto-abertura para NPCs: quando um morador (NpcDweller) entra no trigger da porta, ela abre
        // sozinha e fecha um instante depois que ele sai. O jogador continua abrindo/fechando com E.
        private int _npcNearby;
        private float _autoCloseTimer;
        private bool _autoOpened;
        private const float AutoCloseDelaySeconds = 1.2f;

        public bool IsOpen => _isOpen;

        public string InteractionPrompt => _isOpen ? "Fechar porta" : "Abrir porta";

        public bool CanInteract(GameObject interactor) => interactor != null;

        public void Interact(GameObject interactor)
        {
            if (interactor == null)
            {
                return;
            }

            SetOpen(!_isOpen);
            GameEventBus.Publish(new PlayerActionFeedbackEvent(_isOpen ? "Porta aberta" : "Porta fechada"));
        }

        private void SetOpen(bool open)
        {
            _isOpen = open;
            if (_blocker != null)
            {
                _blocker.enabled = !open; // aberta ⇒ sem bloqueio; fechada ⇒ tranca o vão
            }

            if (_leaf != null)
            {
                _leaf.localPosition = open ? _openLocalPos : _closedLocalPos;
            }
        }

        // ── Auto-abertura para NPCs moradores (não afeta o controle manual do jogador) ──────────────
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || other.GetComponentInParent<NpcDweller>() == null)
            {
                return;
            }

            _npcNearby++;
            if (!_isOpen)
            {
                SetOpen(true);
                _autoOpened = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == null || other.GetComponentInParent<NpcDweller>() == null)
            {
                return;
            }

            _npcNearby = Mathf.Max(0, _npcNearby - 1);
            if (_npcNearby == 0 && _autoOpened)
            {
                _autoCloseTimer = AutoCloseDelaySeconds; // fecha após o morador terminar de passar
            }
        }

        private void Update()
        {
            if (_autoCloseTimer <= 0f)
            {
                return;
            }

            _autoCloseTimer -= Time.deltaTime;
            if (_autoCloseTimer <= 0f && _npcNearby == 0)
            {
                if (_isOpen)
                {
                    SetOpen(false);
                }

                _autoOpened = false;
            }
        }

        /// <summary>Configuração pelo gerador: a folha, o collider de bloqueio e as posições aberta/fechada.</summary>
        public void Configure(Transform leaf, Collider2D blocker, Vector3 closedLocalPos, Vector3 openLocalPos)
        {
            _leaf = leaf;
            _blocker = blocker;
            _closedLocalPos = closedLocalPos;
            _openLocalPos = openLocalPos;
            SetOpen(false); // começa fechada
        }
    }
}
