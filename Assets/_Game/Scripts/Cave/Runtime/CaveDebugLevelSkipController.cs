using CindarsHope.Cave.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveDebugLevelSkipController : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private CaveLevelRuntimeController _levelController;
        [SerializeField] private bool _enableDebugLevelSkip = true;
        [SerializeField] private KeyCode _nextLevelKey = KeyCode.P;
        [SerializeField] private bool _bypassBossGateForDebugSkip = true;

        private string _lastDebugAction = "none";

        private void Start()
        {
            Debug.Log($"CaveDebugLevelSkipController: enabled={_enableDebugLevelSkip}, key={_nextLevelKey}, bypassBossGate={_bypassBossGateForDebugSkip}.", this);
        }

        private void Update()
        {
            if (!_enableDebugLevelSkip || SceneManager.GetActiveScene().name != "CaveScene")
            {
                return;
            }

            if (Input.GetKeyDown(_nextLevelKey))
            {
                SkipToNextLevel();
            }
        }

        private void SkipToNextLevel()
        {
            if (_caveRunManager == null || _levelController == null)
            {
                Debug.LogError("CaveDebugLevelSkipController: CaveRunManager or LevelController not assigned.", this);
                return;
            }

            var currentLevel = _caveRunManager.CurrentCaveLevel;
            var nextLevel = currentLevel + 1;

            if (_bypassBossGateForDebugSkip)
            {
                Debug.LogWarning("DEBUG ONLY: bypassing boss gate for level skip.", this);
            }
            else
            {
                if (!_caveRunManager.CanAdvanceToLevel(currentLevel, nextLevel))
                {
                    Debug.LogWarning($"CaveDebugLevelSkipController: Cannot skip to level {nextLevel}. Boss gate blocks advancement.", this);
                    return;
                }
            }

            _caveRunManager.EnterLevel(nextLevel);
            _levelController.SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.Entrance);
            _levelController.GenerateCurrentLevel();

            _lastDebugAction = $"DEBUG: Level skip {currentLevel} -> {nextLevel}";
            Debug.Log($"CaveDebugLevelSkipController: {_lastDebugAction}", this);
        }

        public bool IsDebugSkipEnabled => _enableDebugLevelSkip;
        public string LastDebugAction => _lastDebugAction;
    }
}
