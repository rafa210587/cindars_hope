using CindarsHope.Core.Bootstrap;
using CindarsHope.Economy;
using CindarsHope.Interaction;
using CindarsHope.Save;
using CindarsHope.UI;
using UnityEngine;

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public sealed class TownSceneRuntimeReferenceInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;

        private void Start()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogWarning("TownSceneRuntimeReferenceInstaller: GameBootstrap.Instance is null, cannot rebind references.", this);
                return;
            }

            // arch: quebra do par mutuo Core|Save (2026-07-15) — bootstrap.SaveManager agora retorna
            // a porta ISaveRuntime; este installer (SceneManagement, fora do par cortado) resolve o
            // tipo concreto por cast local para os Rebind* cross-modulo (nao portaveis).
            var saveManager = bootstrap.SaveManager as SaveManager;
            if (saveManager == null)
            {
                Debug.LogWarning("TownSceneRuntimeReferenceInstaller: SaveManager is null, cannot rebind player transform.", this);
                return;
            }

            saveManager.RebindPlayerTransform(_playerTransform);

            var playerManager = bootstrap.PlayerManager;
            var inventoryManager = bootstrap.InventoryManager;
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
                Debug.LogWarning("TownSceneRuntimeReferenceInstaller: one or more runtime managers are missing during SaveManager rebind.", this);
            }

            var interactionSystem = _playerTransform != null ? _playerTransform.GetComponent<InteractionSystem>() : null;
            DebugHud.RebindExisting(playerManager, inventoryManager, hungerManager, staminaManager, bootstrap.StatusEffectManager, interactionSystem, timeManager, saveManager);

            Debug.Log("TownSceneRuntimeReferenceInstaller rebound runtime refs.", this);
        }
    }
}
