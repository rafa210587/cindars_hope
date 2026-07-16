using CindarsHope.Core.Bootstrap;
using CindarsHope.Cave;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Runtime;
using CindarsHope.Economy;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Save;
using CindarsHope.UI;
using UnityEngine;

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public sealed class CaveSceneRuntimeReferenceInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private CaveLevelRuntimeController _caveLevelRuntimeController;
        [SerializeField] private CaveDebugLevelSkipController _caveDebugLevelSkipController;
        [SerializeField] private CaveBossGateRegistrySO _bossGateRegistry;

        private void Start()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogWarning("CaveSceneRuntimeReferenceInstaller: GameBootstrap.Instance is null, cannot rebind references.", this);
                return;
            }

            // arch: quebra do par mutuo Core|Save (2026-07-15) — bootstrap.SaveManager agora retorna
            // a porta ISaveRuntime; este installer (SceneManagement, fora do par cortado) resolve o
            // tipo concreto por cast local para os Rebind* cross-modulo (nao portaveis).
            var saveManager = bootstrap.SaveManager as SaveManager;
            if (saveManager == null)
            {
                Debug.LogWarning("CaveSceneRuntimeReferenceInstaller: SaveManager is null, cannot rebind player transform.", this);
                return;
            }

            saveManager.RebindPlayerTransform(_playerTransform);
            saveManager.RebindCaveRuntime(_caveRunManager);

            var playerManager = bootstrap.PlayerManager;
            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — bootstrap.InventoryManager agora
            // retorna a porta IInventoryRuntime; este installer (SceneManagement, fora do par cortado)
            // resolve o tipo concreto por cast local para os Rebind* cross-modulo (nao portaveis).
            var inventoryManager = bootstrap.InventoryManager as InventoryManager;
            var hungerManager = bootstrap.HungerManager;
            var staminaManager = bootstrap.StaminaManager;
            var timeManager = bootstrap.TimeManager;

            if (playerManager != null && inventoryManager != null && hungerManager != null && timeManager != null)
            {
                saveManager.RebindRuntimeManagers(playerManager, inventoryManager, hungerManager, timeManager);
                // arch: Core|Economy (spec_arch_core_economy_cycle_reduction_v33) — ShopManager
                // self-registra via static Instance; GameBootstrap nao segura mais essa ref.
                // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — idem para
                // EquipmentManager, via EquipmentManager.Instance.
                saveManager.RebindOptionalRuntimeManagers(CindarsHope.Equipment.EquipmentManager.Instance, bootstrap.PlayerProgressionManager, bootstrap.GameTimeManager, staminaManager, bootstrap.StatusEffectManager, CindarsHope.Skills.SkillTreeManager.Instance, ShopManager.Instance);
            }
            else
            {
                Debug.LogWarning("CaveSceneRuntimeReferenceInstaller: one or more runtime managers are missing during SaveManager rebind.", this);
            }

            // Try to rebind CaveDebugLevelSkipController locally if null
            if (_caveDebugLevelSkipController == null && _caveLevelRuntimeController != null)
            {
                _caveDebugLevelSkipController = _caveLevelRuntimeController.GetComponent<CaveDebugLevelSkipController>();
            }

            // Try to rebind CaveBossGateRegistry if null
            if (_bossGateRegistry == null)
            {
                _bossGateRegistry = Resources.Load<CaveBossGateRegistrySO>("CaveBossGateRegistry");
                if (_bossGateRegistry == null)
                {
                    Debug.LogWarning("CaveSceneRuntimeReferenceInstaller: CaveBossGateRegistry not found in Resources or as Inspector reference.", this);
                }
            }

            var attackController = _playerTransform != null ? _playerTransform.GetComponent<CindarsHope.Combat.PlayerAttackController>() : null;
            if (attackController != null)
            {
                attackController.RebindStaminaManager(staminaManager);
            }

            // SPEC 14A-FIX10: rebind combat databases on every cave-scene load from the single
            // Resources-loaded registry, so wiring can't drift when the scene gets re-saved.
            var combatRegistry = Resources.Load<CindarsHope.Core.Data.CombatRuntimeDatabasesRegistrySO>("CombatRuntimeDatabasesRegistry");
            if (combatRegistry == null)
            {
                Debug.LogError("CaveSceneRuntimeReferenceInstaller: CombatRuntimeDatabasesRegistry not found at Resources/CombatRuntimeDatabasesRegistry. Enemy profiles and player weapon resolution will rely solely on inspector wiring.", this);
            }
            else
            {
                if (_caveRunManager != null)
                {
                    var materializer = _caveRunManager.GetComponent<CaveRuntimeMaterializer>();
                    if (materializer != null) materializer.RebindCombatDatabases(combatRegistry);
                }
                if (attackController != null)
                {
                    attackController.RebindCombatData(combatRegistry.ItemDatabase, combatRegistry.WeaponDatabase, combatRegistry.SpellDatabase);
                }
            }

            // SPEC 14A-FIX7: bootstrap floating damage numbers if the scene didn't include the component.
            // Self-creating instance auto-builds its world-space Canvas in OnEnable.
            CindarsHope.Combat.FloatingDamageNumberDisplayer.EnsureExists(transform);

            // SPEC 14A-FIX10: ensure the player has a DamagePopupAnchor so EnemyContactDamage
            // can show numbers above the player's head instead of guessing.
            if (_playerTransform != null && _playerTransform.GetComponent<CindarsHope.Combat.DamagePopupAnchor>() == null)
            {
                _playerTransform.gameObject.AddComponent<CindarsHope.Combat.DamagePopupAnchor>();
            }

            var interactionSystem = _playerTransform != null ? _playerTransform.GetComponent<InteractionSystem>() : null;
            DebugHud.RebindExisting(playerManager, inventoryManager, hungerManager, staminaManager, bootstrap.StatusEffectManager, interactionSystem, timeManager, saveManager);
            DebugHud.RebindExistingCaveRuntime(_caveRunManager, _caveLevelRuntimeController, _caveDebugLevelSkipController);

            Debug.Log("CaveSceneRuntimeReferenceInstaller rebound runtime refs.", this);
        }
    }
}
