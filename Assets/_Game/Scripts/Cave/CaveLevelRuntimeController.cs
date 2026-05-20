using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Cave
{
    [DisallowMultipleComponent]
    public sealed class CaveLevelRuntimeController : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _runManager;
        [SerializeField] private CaveGenerationConfigSO _generationConfig;
        [SerializeField] private string _defaultBiomeId = "biome_cave_earth";
        [SerializeField] private bool _logGeneratedLayout = true;

        private readonly CaveProceduralGenerator _generator = new CaveProceduralGenerator();

        public CaveGeneratedLevel CurrentGeneratedLevel { get; private set; }
        public int RoomCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.Rooms.Count : 0;
        public int EnemyPointCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.EnemySpawnPoints.Count : 0;
        public int ResourcePointCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.ResourceSpawnPoints.Count : 0;
        public CaveRunManager RunManager => _runManager;

        private void Awake()
        {
            if (_runManager == null)
            {
                _runManager = GetComponent<CaveRunManager>();
            }
        }

        private void Start()
        {
            GenerateCurrentLevel();
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "CaveScene")
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
            {
                RegenerateCurrentRunDebug();
            }
        }

        public void GenerateCurrentLevel()
        {
            EnsureRuntimeReferences();
            _runManager.InitializeIfNeeded();

            CurrentGeneratedLevel = _generator.Generate(
                _generationConfig,
                _runManager.CurrentCaveLevel,
                _runManager.CaveWorldSeed,
                _runManager.CaveRunSeed,
                _defaultBiomeId);

            Debug.Log(
                $"Cave generated.\nLevel: {_runManager.CurrentCaveLevel}\nWorldSeed: {_runManager.CaveWorldSeed}\nRunSeed: {_runManager.CaveRunSeed}\nRooms: {RoomCount}\nEnemyPoints: {EnemyPointCount}\nResourcePoints: {ResourcePointCount}",
                this);

            if (_logGeneratedLayout)
            {
                Debug.Log(CaveGenerationDebugPrinter.ToAscii(CurrentGeneratedLevel), this);
            }

            GameEventBus.Publish(new CaveLevelEnteredEvent(
                _runManager.CurrentCaveLevel,
                _defaultBiomeId,
                _runManager.CaveRunSeed));
        }

        public void RegenerateCurrentRunDebug()
        {
            EnsureRuntimeReferences();
            _runManager.GenerateNewRunSeed("debug_regeneration");
            GenerateCurrentLevel();
        }

        private void EnsureRuntimeReferences()
        {
            if (_runManager == null)
            {
                _runManager = GetComponent<CaveRunManager>();
            }

            if (_runManager == null)
            {
                _runManager = gameObject.AddComponent<CaveRunManager>();
            }

            if (_generationConfig == null)
            {
                _generationConfig = ScriptableObject.CreateInstance<CaveGenerationConfigSO>();
                _generationConfig.Id = "runtime_default_cave_generation";
            }
        }
    }
}
