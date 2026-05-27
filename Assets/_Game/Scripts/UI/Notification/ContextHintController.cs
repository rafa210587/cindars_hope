using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.UI.Notification
{
    /// <summary>
    /// Displays a context hint when the player has an interactable in range.
    /// Subscribes to InteractionPromptChangedEvent and renders "[E] {prompt}"
    /// above the hotbar area. Replace OnGUI with Canvas label for polish.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ContextHintController : MonoBehaviour
    {
        private bool _hasCandidate;
        private string _prompt = string.Empty;

        private void OnEnable()
        {
            GameEventBus.Subscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
        }

        private void OnInteractionPromptChanged(InteractionPromptChangedEvent evt)
        {
            _hasCandidate = evt.HasCandidate;
            _prompt = evt.Prompt ?? string.Empty;
        }

        private void OnGUI()
        {
            if (!_hasCandidate || string.IsNullOrEmpty(_prompt)) return;

            var label = $"[E]  {_prompt}";
            var width = 260f;
            var height = 30f;
            var x = Screen.width * 0.5f - width * 0.5f;
            var y = Screen.height - 105f;
            GUI.Box(new Rect(x, y, width, height), label);
        }
    }
}
