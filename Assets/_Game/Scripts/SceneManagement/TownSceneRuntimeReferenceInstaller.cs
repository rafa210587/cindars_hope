using CindarsHope.Core.Bootstrap;
using CindarsHope.Economy;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Save;
using CindarsHope.UI;
using UnityEngine;
using System.Collections;
using CindarsHope.Core;
using CindarsHope.Foundation;
using CindarsHope.Gameplay.Input;
using CindarsHope.NPC;
using CindarsHope.NPC.Schedule;
using CindarsHope.World.Scenes;
using UnityEngine.SceneManagement;

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public sealed class TownSceneRuntimeReferenceInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private bool _enableInteriorPilot;
        [SerializeField] private TownInteriorVisitController _interiorVisits;
        [SerializeField] private Collider2D _interiorPlayerBody;
        [SerializeField] private Transform _defaultTownSpawn;
        [SerializeField] private NpcShopController _renkoShop;
        private SaveManager _boundSave;
        private IPlayerRuntime _playerRuntime;
        private IModalRuntime _modalRuntime;
        private IPlayerFacingSource _facingSource;
        private bool _interiorReady;

        private void OnEnable()
        {
            if (_boundSave != null && _enableInteriorPilot)
                StartCoroutine(BindInteriorWhenReady());
        }

        private void OnDisable()
        {
            _interiorReady = false;
            StopAllCoroutines();
            if (_interiorVisits == null) return;
            SceneTransitionRouter.UnregisterTownInteriorVisitAdmission(_interiorVisits);
            if (_boundSave != null) _boundSave.UnbindTownInteriorRuntime(_interiorVisits);
        }

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

            // arch: quebra do par mutuo Core|Player (2026-07-15) — bootstrap.PlayerManager/
            // HungerManager/StaminaManager/PlayerProgressionManager/StatusEffectManager agora
            // retornam MonoBehaviour (Core nao nomeia mais CindarsHope.Player). arch: quebra do par
            // mutuo Player|SceneManagement (2026-07-15) — este installer (SceneManagement) NAO faz
            // cast para o tipo concreto (isso reintroduziria a aresta SceneManagement->Player, que
            // combinada com a aresta existente Player->SceneManagement via
            // AnyaFountainRespawnFlow/SceneTransitionRouterAdapter formaria um par mutuo novo);
            // repassa os MonoBehaviour direto para os Rebind* (SaveManager/DebugHud/etc.), que fazem
            // o cast internamente (esses modulos ja nomeiam CindarsHope.Player sem criar par novo).
            var playerManager = bootstrap.PlayerManager;
            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — bootstrap.InventoryManager agora
            // retorna a porta IInventoryRuntime; este installer (SceneManagement, fora do par cortado)
            // resolve o tipo concreto por cast local para os Rebind* cross-modulo (nao portaveis).
            var inventoryManager = bootstrap.InventoryManager as InventoryManager;
            var hungerManager = bootstrap.HungerManager;
            var staminaManager = bootstrap.StaminaManager;
            var progressionManager = bootstrap.PlayerProgressionManager;
            var statusEffectManager = bootstrap.StatusEffectManager;
            var timeManager = bootstrap.TimeManager;

            if (playerManager != null && inventoryManager != null && hungerManager != null && timeManager != null)
            {
                saveManager.RebindRuntimeManagers(playerManager, inventoryManager, hungerManager, timeManager);
                // arch: Core|Economy (spec_arch_core_economy_cycle_reduction_v33) — ShopManager
                // self-registra via static Instance; GameBootstrap nao segura mais essa ref.
                // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — idem para
                // EquipmentManager, via EquipmentManager.Instance.
                saveManager.RebindOptionalRuntimeManagers(CindarsHope.Equipment.EquipmentManager.Instance, progressionManager, bootstrap.GameTimeManager, staminaManager, statusEffectManager, CindarsHope.Skills.SkillTreeManager.Instance, ShopManager.Instance);
            }
            else
            {
                Debug.LogWarning("TownSceneRuntimeReferenceInstaller: one or more runtime managers are missing during SaveManager rebind.", this);
            }

            var interactionSystem = _playerTransform != null ? _playerTransform.GetComponent<InteractionSystem>() : null;
            DebugHud.RebindExisting(playerManager, inventoryManager, hungerManager, staminaManager, statusEffectManager, interactionSystem, timeManager, saveManager);

            Debug.Log("TownSceneRuntimeReferenceInstaller rebound runtime refs.", this);
            if (_enableInteriorPilot)
            {
                _boundSave = saveManager;
                _playerRuntime = playerManager as IPlayerRuntime;
                _modalRuntime = bootstrap.ModalManager;
                StartCoroutine(BindInteriorWhenReady());
            }
        }

        private IEnumerator BindInteriorWhenReady()
        {
            if (_interiorVisits == null || _playerTransform == null || _interiorPlayerBody == null ||
                _interiorPlayerBody.isTrigger || _interiorPlayerBody.attachedRigidbody == null ||
                _defaultTownSpawn == null || _defaultTownSpawn.gameObject.scene != gameObject.scene)
            {
                Debug.LogError("Town interior pilot requires explicit controller, Player body and safe Town spawn references.", this);
                yield break;
            }
            foreach (var component in _playerTransform.GetComponents<MonoBehaviour>())
                if (component is IPlayerFacingSource facing) { _facingSource = facing; break; }
            float timeout = _interiorVisits.TimeoutSeconds;
            if (_facingSource == null || _playerRuntime == null || _modalRuntime == null ||
                float.IsNaN(timeout) || float.IsInfinity(timeout) || timeout <= 0f)
            {
                Debug.LogError("Town interior pilot is missing Player/modal ports or valid timeout configuration.", this);
                yield break;
            }
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!AdaptersReady() && Time.realtimeSinceStartup < deadline) yield return null;
            if (!AdaptersReady())
            {
                Debug.LogError("Town interior pilot readiness timed out; entry remains disabled.", this);
                yield break;
            }
            _interiorVisits.BindRuntime(_playerTransform, _interiorPlayerBody,
                _ => ReadyToRestore() && _playerRuntime.CurrentHP > 0 &&
                    !_modalRuntime.HasActiveModal && !GameplayInputBlocker.IsBlocked,
                ReadyToRestore, _defaultTownSpawn,
                () => new Vector2(_facingSource.FacingX, _facingSource.FacingY));
            if (!SceneTransitionRouter.RegisterTownInteriorVisitAdmission(_interiorVisits))
            {
                Debug.LogError("Town interior pilot could not acquire the router admission slot.", this);
                yield break;
            }
            _boundSave.BindTownInteriorRuntime(_interiorVisits, gameObject.scene.handle);
            BindRenkoInteriorPilot();
            _interiorReady = true;
        }

        private void BindRenkoInteriorPilot()
        {
            if (_renkoShop == null) return; // S02 scene remains valid until the opt-in generator runs.
            if (_renkoShop.gameObject.scene != gameObject.scene ||
                _renkoShop.NpcData == null || _renkoShop.NpcData.NpcId != "npc_renko")
            {
                Debug.LogError("Renko interior pilot requires the existing Town npc_renko shop controller.", this);
                return;
            }
            var adapter = _renkoShop.GetComponent<NpcInteriorPresentationAdapter>();
            var schedule = NpcScheduleService.Instance;
            if (schedule == null)
            {
                Debug.LogError("Renko interior pilot requires the existing NPC schedule service.", this);
                return;
            }
            schedule.SetClock(GameBootstrap.Instance.GameTimeManager);
            schedule.RegisterShopController(_renkoShop);
            if (adapter == null || !adapter.BindPilot("npc_renko", "House_Residential_2",
                    "npc_renko_work", "npc_renko_home", ResolvePresentedRenkoAnchor))
            {
                Debug.LogError("Renko interior pilot has no adapter or could not bind it.", this);
                return;
            }
            schedule.RefreshSchedulesAfterInteriorBinding();
        }

        private Transform ResolvePresentedRenkoAnchor(string houseId, string anchorId)
        {
            if (_interiorVisits != null &&
                _interiorVisits.TryGetPresentedInterior(houseId, out var instance) &&
                instance != null && instance.TryGetNpcAnchor("npc_renko", anchorId, out var anchor))
                return anchor;
            return null;
        }

        private bool AdaptersReady() => _boundSave != null && _boundSave.IsInitialized &&
            _interiorVisits != null && _interiorVisits.isActiveAndEnabled &&
            SceneManager.GetActiveScene() == gameObject.scene &&
            GameEventBus.HasSubscribers<TownInteriorBlackoutRequestedEvent>() &&
            GameEventBus.HasSubscribers<TownInteriorPlayerPlacementRequestedEvent>();

        private bool ReadyToRestore() => _interiorReady && AdaptersReady();
    }
}
