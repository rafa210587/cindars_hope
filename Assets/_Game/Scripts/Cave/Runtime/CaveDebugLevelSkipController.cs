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
        [SerializeField] private KeyCode _alternateNextLevelKey = KeyCode.F2;
        [SerializeField] private bool _bypassBossGateForDebugSkip = true;
        [SerializeField] private bool _showDebugSkipButton = true;

        private string _lastDebugAction = "none";
        private bool _disabledLogged = false;

        private void Start()
        {
            var hasRunManager = _caveRunManager != null;
            var hasLevelController = _levelController != null;
            Debug.Log($"CaveDebugLevelSkipController: enabled={_enableDebugLevelSkip}, key={_nextLevelKey}, altKey={_alternateNextLevelKey}, bypassBossGate={_bypassBossGateForDebugSkip}, hasRunManager={hasRunManager}, hasLevelController={hasLevelController}.", this);
        }

        private void Update()
        {
            if (!_enableDebugLevelSkip)
            {
                if (!_disabledLogged)
                {
                    Debug.Log("CaveDebugLevelSkipController: disabled by toggle.", this);
                    _disabledLogged = true;
                }
                return;
            }

            if (SceneManager.GetActiveScene().name != "CaveScene")
            {
                return;
            }

            if (Input.GetKeyDown(_nextLevelKey))
            {
                Debug.Log($"CaveDebugLevelSkipController: debug skip key pressed. Key={_nextLevelKey}.", this);
                SkipToNextLevel();
            }

            if (Input.GetKeyDown(_alternateNextLevelKey))
            {
                Debug.Log($"CaveDebugLevelSkipController: debug skip key pressed. Key={_alternateNextLevelKey}.", this);
                SkipToNextLevel();
            }
        }

        private void OnGUI()
        {
            if (!_enableDebugLevelSkip || !_showDebugSkipButton || SceneManager.GetActiveScene().name != "CaveScene")
            {
                return;
            }

            var rect = new Rect(20f, 20f, 220f, 32f);
            if (GUI.Button(rect, "DEBUG: Next Cave Level (P/F2)"))
            {
                Debug.Log("CaveDebugLevelSkipController: debug skip button clicked.", this);
                SkipToNextLevel();
            }
        }

        private bool TryRebindLocalReferences()
        {
            if (_caveRunManager == null)
            {
                _caveRunManager = GetComponent<CaveRunManager>();
            }

            if (_levelController == null)
            {
                _levelController = GetComponent<CaveLevelRuntimeController>();
            }

            return _caveRunManager != null && _levelController != null;
        }

        private void SkipToNextLevel()
        {
            if (!TryRebindLocalReferences())
            {
                var hasRunManager = _caveRunManager != null;
                var hasLevelController = _levelController != null;
                Debug.LogError($"CaveDebugLevelSkipController: Failed to bind references. hasRunManager={hasRunManager}, hasLevelController={hasLevelController}.", this);
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
