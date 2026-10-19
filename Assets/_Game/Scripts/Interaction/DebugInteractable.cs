using UnityEngine;

namespace CindarsHope.Interaction
{
    [DisallowMultipleComponent]
    public sealed class DebugInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _interactionPrompt = "Interagir";
        [SerializeField] private string _debugMessage = "Debug interaction executed.";

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public void Interact(GameObject interactor)
        {
            var interactorName = interactor != null ? interactor.name : "null";
            Debug.Log($"{_debugMessage} Object='{name}', Interactor='{interactorName}'.");
        }
    }
}
