using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Craft
{
    [DisallowMultipleComponent]
    public class CraftingPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private CraftingManager _craftingManager;
        [SerializeField] private string _recipeId = "recipe_processed_wood";
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        public string InteractionPrompt => "Craftar";

        public bool CanInteract(GameObject interactor)
        {
            return _craftingManager != null && !string.IsNullOrWhiteSpace(_recipeId);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"{nameof(CraftingPoint)} on '{name}' cannot craft because it is not configured.", this);
                return;
            }

            if (_craftingManager.TryCraft(_recipeId))
            {
                Debug.Log($"{nameof(CraftingPoint)} crafted recipe '{_recipeId}'.", this);
                return;
            }

            Debug.Log($"{nameof(CraftingPoint)} could not craft recipe '{_recipeId}'. Check ingredients, output capacity, and recipe data.", this);
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
