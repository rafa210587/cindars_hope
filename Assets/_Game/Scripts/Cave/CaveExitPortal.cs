using CindarsHope.Cave.Runtime;
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
    public enum CaveExitMode
    {
        BackExit,
        ForwardExit
    }

    [DisallowMultipleComponent]
    public sealed class CaveExitPortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _targetSceneName = "FarmScene";
        [SerializeField] private string _targetSpawnId;
        [SerializeField] private string _interactionPrompt = "Sair da caverna";

        private CaveExitMode _mode;
        private CaveRunManager _caveRunManager;

        public string InteractionPrompt => _interactionPrompt;

        public void InitializeBackExit(CaveRunManager caveRunManager)
        {
            _mode = CaveExitMode.BackExit;
            _caveRunManager = caveRunManager;
            _interactionPrompt = "Voltar";
        }

        public void InitializeForwardExit(CaveRunManager caveRunManager)
        {
            _mode = CaveExitMode.ForwardExit;
            _caveRunManager = caveRunManager;
            _interactionPrompt = "Avançar";
        }

        public bool CanInteract(GameObject interactor)
        {
            if (_mode == CaveExitMode.BackExit)
            {
                return true;
            }
            if (_mode == CaveExitMode.ForwardExit)
            {
                return true;
            }
            return !string.IsNullOrWhiteSpace(_targetSceneName);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"CaveExitPortal on '{name}' cannot interact.", this);
                return;
            }

            if (_mode == CaveExitMode.BackExit)
            {
                HandleBackExit();
            }
            else if (_mode == CaveExitMode.ForwardExit)
            {
                HandleForwardExit();
            }
            else
            {
                HandleSceneTransition();
            }
        }

        private void HandleBackExit()
        {
            if (_caveRunManager == null)
            {
                Debug.LogWarning("CaveExitPortal: CaveRunManager not assigned for BackExit.", this);
                return;
            }

            if (_caveRunManager.CurrentCaveLevel == 1)
            {
                _targetSceneName = "FarmScene";
                _targetSpawnId = "cave_from_farm";
                HandleSceneTransition();
            }
            else
            {
                _caveRunManager.EnterLevel(_caveRunManager.CurrentCaveLevel - 1);
                Debug.Log($"CaveExitPortal: Entered level {_caveRunManager.CurrentCaveLevel}.", this);
            }
        }

        private void HandleForwardExit()
        {
            if (_caveRunManager == null)
            {
                Debug.LogWarning("CaveExitPortal: CaveRunManager not assigned for ForwardExit.", this);
                return;
            }

            _caveRunManager.EnterLevel(_caveRunManager.CurrentCaveLevel + 1);
            Debug.Log($"CaveExitPortal: Entered level {_caveRunManager.CurrentCaveLevel}.", this);
        }

        private void HandleSceneTransition()
        {
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
