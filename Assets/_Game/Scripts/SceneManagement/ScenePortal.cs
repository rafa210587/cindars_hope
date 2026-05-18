using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public sealed class ScenePortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _targetSceneName;
        [SerializeField] private string _targetScenePath;
        [SerializeField] private string _targetSpawnId;
        [SerializeField] private string _interactionPrompt = "Entrar";

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrWhiteSpace(_targetSceneName) || !string.IsNullOrWhiteSpace(_targetScenePath);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"{nameof(ScenePortal)} on '{name}' has no target scene configured.", this);
                return;
            }

            SceneTransitionState.SetPendingSpawn(_targetSpawnId);

            var sourceSceneName = SceneManager.GetActiveScene().name;
            var targetSceneName = string.IsNullOrWhiteSpace(_targetSceneName) ? _targetScenePath : _targetSceneName;
            GameEventBus.Publish(new SceneTransitionStartedEvent(sourceSceneName, targetSceneName, _targetSpawnId));

            LoadTargetScene();
        }

        private void LoadTargetScene()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(_targetScenePath))
            {
                EditorSceneManager.LoadSceneInPlayMode(_targetScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            if (!string.IsNullOrWhiteSpace(_targetSceneName))
            {
                SceneManager.LoadScene(_targetSceneName);
                return;
            }

            Debug.LogWarning($"{nameof(ScenePortal)} on '{name}' cannot load a scene without a scene name.", this);
        }
    }
}
