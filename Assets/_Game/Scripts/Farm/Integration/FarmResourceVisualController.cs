using UnityEngine;

namespace CindarsHope.Farm.Integration
{
    public sealed class FarmResourceVisualController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _interactedColor = new Color(1f, 0.9f, 0.5f);
        [SerializeField] private Color _depletedColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        public void Apply(FarmResourceVisualState state)
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            switch (state)
            {
                case FarmResourceVisualState.Available:
                    _spriteRenderer.enabled = true;
                    _spriteRenderer.color = _availableColor;
                    break;
                case FarmResourceVisualState.Interacted:
                    _spriteRenderer.color = _interactedColor;
                    break;
                case FarmResourceVisualState.Depleted:
                    _spriteRenderer.color = _depletedColor;
                    break;
                case FarmResourceVisualState.Reset:
                    _spriteRenderer.enabled = true;
                    _spriteRenderer.color = _availableColor;
                    break;
            }
        }
    }
}
