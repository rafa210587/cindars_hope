using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.NPC
{
    [DisallowMultipleComponent]
    public sealed class NpcTalkPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _npcId = "npc_pip_miudinho";
        [SerializeField] private string _displayName = "Pip Miudinho";
        [SerializeField] private string _dialogueLine = "Bem-vindo a Cindar's Hope. Ainda estamos abrindo a cidade.";
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        public string InteractionPrompt => "Conversar com Pip";

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrWhiteSpace(_displayName) && !string.IsNullOrWhiteSpace(_dialogueLine);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"{nameof(NpcTalkPoint)} on '{name}' is missing dialogue data.", this);
                return;
            }

            Debug.Log($"[{_displayName}] {_dialogueLine}", this);
        }

        private void Reset()
        {
            EnsureComponents();
        }

        private void OnValidate()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }
    }
}
