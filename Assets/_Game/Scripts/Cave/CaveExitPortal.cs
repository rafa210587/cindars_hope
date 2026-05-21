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
        [SerializeField] private string _targetScenePath;
        [SerializeField] private string _targetSpawnId;
        [SerializeField] private string _interactionPrompt = "Sair da caverna";

        private CaveExitMode _mode;
        private CaveRunManager _caveRunManager;
        private CaveLevelRuntimeController _levelController;

        public string InteractionPrompt => _interactionPrompt;

        public void InitializeBackExit(CaveRunManager caveRunManager, CaveLevelRuntimeController levelController)
        {
            _mode = CaveExitMode.BackExit;
            _caveRunManager = caveRunManager;
            _levelController = levelController;
            _interactionPrompt = "Voltar / Sair";
        }

        public void InitializeForwardExit(CaveRunManager caveRunManager, CaveLevelRuntimeController levelController)
        {
            _mode = CaveExitMode.ForwardExit;
            _caveRunManager = caveRunManager;
            _levelController = levelController;
            _interactionPrompt = "Avançar para próximo nível";
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
                _targetScenePath = "Assets/_Game/Scenes/FarmScene.unity";
                _targetSpawnId = "farm_from_cave";
                Debug.Log($"CaveExitPortal: BackExit level 1. Loading FarmScene with spawn {_targetSpawnId}.", this);
                HandleSceneTransition();
            }
            else
            {
                if (_levelController == null)
                {
                    Debug.LogError("CaveExitPortal: CaveLevelRuntimeController not assigned for BackExit in level > 1. Regenerate CaveScene.", this);
                    return;
                }

                var currentLevel = _caveRunManager.CurrentCaveLevel;
                var previousLevel = currentLevel - 1;

                Debug.Log($"CaveExitPortal: BackExit interacted. Level {currentLevel} -> {previousLevel}.", this);

                _caveRunManager.EnterLevel(previousLevel);
                _levelController.SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.ForwardExit);

                var snapshot = _caveRunManager.State.VisitedLevelSnapshots.ContainsKey(previousLevel)
                    ? _caveRunManager.State.VisitedLevelSnapshots[previousLevel]
                    : null;

                if (snapshot != null && snapshot.IsValid())
                {
                    _levelController.RestoreFromSnapshot(snapshot);
                    Debug.Log($"CaveExitPortal: BackExit completed. Level {currentLevel} -> {previousLevel} restored from snapshot with ForwardExit spawn anchor.", this);
                }
                else
                {
                    _levelController.GenerateCurrentLevel();
                    Debug.Log($"CaveExitPortal: BackExit completed. Level {currentLevel} -> {previousLevel} generated fresh with ForwardExit spawn anchor.", this);
                }
            }
        }

        private void HandleForwardExit()
        {
            if (_caveRunManager == null)
            {
                Debug.LogWarning("CaveExitPortal: CaveRunManager not assigned for ForwardExit.", this);
                return;
            }

            if (_levelController == null)
            {
                Debug.LogError("CaveExitPortal: CaveLevelRuntimeController not assigned for ForwardExit. Regenerate CaveScene.", this);
                return;
            }

            var currentLevel = _caveRunManager.CurrentCaveLevel;
            var nextLevel = currentLevel + 1;

            if (!_caveRunManager.CanAdvanceToLevel(currentLevel, nextLevel))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Derrote o boss deste nível para avançar.", 3f));
                Debug.Log($"CaveExitPortal: ForwardExit blocked by boss gate. Current level {currentLevel}, target {nextLevel}.", this);
                return;
            }

            Debug.Log($"CaveExitPortal: ForwardExit interacted. Level {currentLevel} -> {nextLevel}.", this);

            _caveRunManager.EnterLevel(nextLevel);
            _levelController.SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.Entrance);
            _levelController.GenerateCurrentLevel();

            Debug.Log($"CaveExitPortal: ForwardExit completed. CurrentLevel={_caveRunManager.CurrentCaveLevel}.", this);
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
            if (!string.IsNullOrWhiteSpace(_targetScenePath))
            {
                EditorSceneManager.LoadSceneInPlayMode(_targetScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }

            EditorSceneManager.LoadSceneInPlayMode(_targetSceneName, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(_targetSceneName);
#endif
        }
    }
}
