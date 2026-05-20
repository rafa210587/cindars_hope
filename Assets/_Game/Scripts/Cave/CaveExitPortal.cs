using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace CindarsHope.Cave
{
    [DisallowMultipleComponent]
    public sealed class CaveExitPortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _targetSceneName = "FarmScene";
        [SerializeField] private string _targetSpawnId;
        [SerializeField] private string _interactionPrompt = "Sair da caverna";

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrWhiteSpace(_targetSceneName);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"CaveExitPortal on '{name}' has no target scene configured.", this);
                return;
            }

            SceneTransitionState.SetPendingSpawn(_targetSpawnId);

            var sourceSceneName = SceneManager.GetActiveScene().name;
            GameEventBus.Publish(new SceneTransitionStartedEvent(sourceSceneName, _targetSceneName, _targetSpawnId));

            LoadTargetScene();
        }

        private void LoadTargetScene()
        {
#if UNITY_EDITOR
            EditorSceneManager.LoadSceneInPlayMode(_targetSceneName, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(_targetSceneName);
#endif
        }
    }
}
