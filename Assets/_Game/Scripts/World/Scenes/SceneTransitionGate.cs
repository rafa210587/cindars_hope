using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.World.Scenes
{
    /// <summary>
    /// MonoBehaviour that acts as an interactable scene transition gate.
    /// When the player interacts with this GameObject, it creates a
    /// SceneTransitionRequest and delegates to SceneTransitionRouter.
    ///
    /// This is the thin WAVE_INTEGRATION_13 gate that wraps the same logic
    /// as the legacy ScenePortal, but works with stable SceneId constants
    /// and the SceneTransitionRouter service.
    ///
    /// FADE_LOADING_DEFERRED_WITH_REASON: no fade system exists in current wave.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneTransitionGate : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _transitionId;
        [SerializeField] private string _fromSceneId;
        [SerializeField] private string _toSceneId;
        [SerializeField] private string _targetSpawnAnchorId;

        [Tooltip("Text shown to the player when approaching this gate.")]
        [SerializeField] private string _interactionPrompt = "Entrar";

        public string InteractionPrompt => _interactionPrompt;
        public string TransitionId => _transitionId;
        public string FromSceneId => _fromSceneId;
        public string ToSceneId => _toSceneId;
        public string TargetSpawnAnchorId => _targetSpawnAnchorId;

        public bool CanInteract(GameObject interactor)
        {
            if (string.IsNullOrWhiteSpace(_toSceneId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_targetSpawnAnchorId))
            {
                return false;
            }

            return true;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning(
                    $"[SceneTransitionGate] '{name}' cannot interact: toSceneId or targetSpawnAnchorId is empty.",
                    this);
                return;
            }

            var request = new SceneTransitionRequest(
                sourceSceneId: _fromSceneId,
                destinationSceneId: _toSceneId,
                targetSpawnAnchorId: _targetSpawnAnchorId,
                initiatingGateId: _transitionId);

            SceneTransitionRouter.Execute(request);
        }

        private void OnValidate()
        {
            if (!string.IsNullOrWhiteSpace(_transitionId))
                _transitionId = _transitionId.Trim();
            if (!string.IsNullOrWhiteSpace(_fromSceneId))
                _fromSceneId = _fromSceneId.Trim();
            if (!string.IsNullOrWhiteSpace(_toSceneId))
                _toSceneId = _toSceneId.Trim();
            if (!string.IsNullOrWhiteSpace(_targetSpawnAnchorId))
                _targetSpawnAnchorId = _targetSpawnAnchorId.Trim();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);
#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(_transitionId))
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, _transitionId);
            }
#endif
        }
    }
}
