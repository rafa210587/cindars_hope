using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveDebugLevelSkipController : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private CaveLevelRuntimeController _levelController;
        [SerializeField] private bool _enableDebugLevelSkip = false;
        [SerializeField] private KeyCode _nextGateKey = KeyCode.P;
        [SerializeField] private KeyCode _alternateNextGateKey = KeyCode.F2;
        [SerializeField] private int _maxDebugGateSearchLevel = 100;
        [SerializeField] private bool _showDebugSkipButton = true;

        // Backward-compatible serialized fields used by older scene generators.
        // Keep these hidden to avoid breaking CreateMvpCaveScene while the generator is updated.
        [SerializeField, HideInInspector] private KeyCode _nextLevelKey = KeyCode.P;
        [SerializeField, HideInInspector] private KeyCode _alternateNextLevelKey = KeyCode.F2;
        [SerializeField, HideInInspector] private bool _bypassBossGateForDebugSkip = true;

        private string _lastDebugAction = "none";
        private bool _disabledLogged = false;
        private int _noMoreGatesWarnedAtLevel = -1;

        private void OnValidate()
        {
            SyncLegacySerializedFields();
        }

        private void Start()
        {
            SyncLegacySerializedFields();
            var hasRunManager = _caveRunManager != null;
            var hasLevelController = _levelController != null;
            Debug.Log($"CaveDebugLevelSkipController: enabled={_enableDebugLevelSkip}, nextGateKey={_nextGateKey}, altKey={_alternateNextGateKey}, maxGateSearchLevel={_maxDebugGateSearchLevel}, hasRunManager={hasRunManager}, hasLevelController={hasLevelController}.", this);
        }

        private void Update()
        {
            SyncLegacySerializedFields();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Build guard: skip de nivel e ferramenta de debug; dupla protecao com o default
            // seguro (_enableDebugLevelSkip = false) garante que nao roda em build de producao
            // mesmo que o campo tenha sido setado manualmente como true em algum asset/prefab.
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

            if (Input.GetKeyDown(_nextGateKey))
            {
                Debug.Log($"CaveDebugLevelSkipController: debug next gate key pressed. Key={_nextGateKey}.", this);
                SkipToNextBossGateLevel();
            }

            if (Input.GetKeyDown(_alternateNextGateKey))
            {
                Debug.Log($"CaveDebugLevelSkipController: debug next gate key pressed. Key={_alternateNextGateKey}.", this);
                SkipToNextBossGateLevel();
            }
#endif
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!_enableDebugLevelSkip || !_showDebugSkipButton || SceneManager.GetActiveScene().name != "CaveScene")
            {
                return;
            }

            var rect = new Rect(20f, 20f, 260f, 32f);
            if (GUI.Button(rect, "DEBUG: Next Boss Gate Level (P/F2)"))
            {
                Debug.Log("CaveDebugLevelSkipController: debug next gate button clicked.", this);
                SkipToNextBossGateLevel();
            }
#endif
        }

        private void SyncLegacySerializedFields()
        {
            if (_nextLevelKey != _nextGateKey)
            {
                _nextGateKey = _nextLevelKey;
            }

            if (_alternateNextLevelKey != _alternateNextGateKey)
            {
                _alternateNextGateKey = _alternateNextLevelKey;
            }

            // Legacy serialized field intentionally read to preserve backward compatibility
            // with older scene generators without changing current boss gate behavior.
            _ = _bypassBossGateForDebugSkip;
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

        private void SkipToNextBossGateLevel()
        {
            if (!TryRebindLocalReferences())
            {
                var hasRunManager = _caveRunManager != null;
                var hasLevelController = _levelController != null;
                Debug.LogError($"CaveDebugLevelSkipController: Failed to bind references. hasRunManager={hasRunManager}, hasLevelController={hasLevelController}.", this);
                return;
            }

            var currentLevel = _caveRunManager.CurrentCaveLevel;
            if (!TryFindNextBossGateLevel(currentLevel, out var nextGateLevel, out var gateId))
            {
                _lastDebugAction = $"DEBUG: No further boss gates configured after level {currentLevel}.";
                if (_noMoreGatesWarnedAtLevel != currentLevel)
                {
                    _noMoreGatesWarnedAtLevel = currentLevel;
                    Debug.Log($"CaveDebugLevelSkipController: {_lastDebugAction}", this);
                }
                return;
            }

            _caveRunManager.EnterLevel(nextGateLevel);
            _levelController.SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.Entrance);
            _levelController.GenerateCurrentLevel();

            _lastDebugAction = $"DEBUG: Next gate skip {currentLevel} -> {nextGateLevel} ({gateId})";
            Debug.Log($"CaveDebugLevelSkipController: {_lastDebugAction}.", this);
            Debug.Log($"VALIDATION READ: DebugHud > Cave Summary > CaveLevel should be {nextGateLevel}; Current Level Boss Gate should show '{gateId}' as active/blocked until defeated; Can advance should be false before boss defeat and true after MarkBossAsDefeated/debug boss flow.", this);
            Debug.Log($"VALIDATION ACTION: Try using the forward cave exit from level {nextGateLevel}. It should be blocked before the boss gate is defeated. Then mark/defeat the boss, retry forward exit, and confirm checkpoint unlock + boss defeat history in DebugHud.", this);
        }

        private bool TryFindNextBossGateLevel(int currentLevel, out int nextGateLevel, out string gateId)
        {
            nextGateLevel = 0;
            gateId = string.Empty;
            var maxLevel = Mathf.Max(currentLevel + 1, _maxDebugGateSearchLevel);

            for (var level = currentLevel + 1; level <= maxLevel; level++)
            {
                if (_caveRunManager.TryGetBossGateForLevel(level, out var gate) && gate != null)
                {
                    nextGateLevel = gate.CaveLevel;
                    gateId = gate.Id;
                    return true;
                }
            }

            return false;
        }

        public bool IsDebugSkipEnabled => _enableDebugLevelSkip;
        public string LastDebugAction => _lastDebugAction;
    }
}