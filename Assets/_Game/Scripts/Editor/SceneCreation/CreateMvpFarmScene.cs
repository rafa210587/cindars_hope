using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Economy;
using CindarsHope.Enemy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Farm.Animals;
using CindarsHope.Farm.Integration;
// CindarsHope.Farm.Lots removido em 2026-06-26: geracao de lotes fable_41 na cena eliminada (spec_farm_scene_relayout_v4).
// FarmLotService / FarmLotsSectionProvider / FarmLotCatalog permanecem como sistemas de save/runtime.
using CindarsHope.Farm.Runtime;
using CindarsHope.Farm.Scene;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Player.Progression;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using CindarsHope.Skills;
using CindarsHope.UI;
using CindarsHope.UI.Crafting;
using CindarsHope.World;
using CindarsHope.World.Data;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CindarsHope.UI.Hotbar;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Routing;
using CindarsHope.Editor.Validation;
using CindarsHope.Editor.ScaleSystem;
using CindarsHope.Editor.Art;
using CindarsHope.NPC;

namespace CindarsHope.Editor.SceneCreation
{
    public static class CreateMvpFarmScene
    {
        private const string ScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string SeedDatabasePath = "Assets/_Game/Data/Registries/SeedDatabase.asset";
        private const string RecipeDatabasePath = "Assets/_Game/Data/Registries/RecipeDatabase.asset";
        private const string TreeDataPath = "Assets/_Game/Data/World/Trees/Tree_Basic.asset";
        private const string GameScaleConfigPath = "Assets/_Game/Data/Config/GameScaleConfig.asset";
        private const string BuiltinSpritePath = "UI/Skin/UISprite.psd";
        private const string WeaponDatabasePath = "Assets/_Game/Data/Combat/WeaponDatabase.asset";
        private const string SpellDatabasePath = "Assets/_Game/Data/Combat/SpellDatabase.asset";
        private const string StatusEffectDatabasePath = "Assets/_Game/Data/Combat/StatusEffectDatabase.asset";

        public static void CreateSceneFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Cannot create MVP FarmScene during Play Mode. Exit Play Mode and run this menu again.");
                return;
            }
            CreateScene();
        }

        public static void CreateScene()
        {
            EnsureFolder("Assets/_Game", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "FarmScene";

            var bootstrap = CreateBootstrap();
            var inventoryManager = bootstrap.GetComponent<InventoryManager>();
            var playerManager = bootstrap.GetComponent<PlayerManager>();
            var timeManager = bootstrap.GetComponent<TimeManager>();
            var hungerManager = bootstrap.GetComponent<HungerManager>();
            var saveManager = bootstrap.GetComponent<SaveManager>();
            var craftingRuntime = bootstrap.GetComponent<CraftingRuntime>();
            var modalManager = bootstrap.GetComponent<ModalManager>();
            var playerTransform = CreatePlayer();
            CreateFarmSpawnPoints(playerTransform);
            // Camera background is the uniform white playfield; do not create a giant
            // ground sprite because it reads as a horizon/central rectangle in MVP art.
            var farmPlotRegistry = CreateFarmPlots(inventoryManager, bootstrap.GetComponent<StaminaManager>());
            CreateRainIrrigation(farmPlotRegistry);
            // v5 (spec_farm_scene_relayout_v4 §15.3 item 2): casa WALK-IN reusando padrao da cidade.
            // Bed + BedLetter + FarmHouseChest ficam DENTRO do interior percorrivel.
            // CreateFarmBed / CreateBedLetter / CreateFarmHouseChest soltos REMOVIDOS — criados dentro da casa.
            CreateFarmWalkInHouse();
            CreateFonteAnya(bootstrap);
            var treeRegistry = CreateTrees(inventoryManager);
            var itemPickupRegistry = CreateItemPickups(inventoryManager);
            var craftingModal = CreateCraftingUi(craftingRuntime, modalManager);
            CreateCraftingStations(craftingRuntime, craftingModal);
            CreateFarmPortals();
            CreateFishingSpot(inventoryManager);
            CreateFarmSceneFoundationZones();
            CreateFarmGroundTexture();  // chao base texturizado (grama tiled) atras de tudo — antes so cinza
            CreateCaveEntrance();
            CreateZrixContractBoard(); // fable_51: Zrix's cave-contract board at the cave mouth
            CreateFarmZrixNpc();        // Fase 5: Zrix perambula no bosque NO (NpcController + NpcWanderer)
            CreateExpansionAreaPlaceholders(); // Fase 6: 3 areas de expansao reservadas (Exp_North/West/South)
            CreateFarmResourceInteractables(inventoryManager);
            CreateForagePoints();   // fable_54: forrageio sazonal real (substitui o smoke)
            CreateShippingBin();    // fable_54: caixa de envio overnight
            // fable_41 lotes substituidos pelo Quadro de Evolucoes (decisao 2026-06-26, spec_farm_scene_relayout_v4).
            CreateFarmEvolutionBoard();
            CreateAnimalHousings(inventoryManager);
            CreateProcessingAndGreenhouse(inventoryManager, bootstrap.GetComponent<StaminaManager>());
            CreateSellPoint(inventoryManager, playerManager);
            CreateGameplayInputRouter();
            CreateActiveSkillExecutionController();
            CreateDebugHud(
                playerManager,
                inventoryManager,
                hungerManager,
                playerTransform.GetComponent<InteractionSystem>(),
                timeManager,
                saveManager);
            CreateBounds();
            CreateMountainBarrier();    // spec_farm_scene_relayout_v4: montanha (colisao N) + backdrop
            CreateRiverAndBridge();     // spec_farm_scene_relayout_v4: rio (colisao) + ponte andavel
            CreateLockedOreNodes();     // spec_farm_scene_relayout_v4: 4 veios de minerio bloqueados
            CreateMainCamera(playerTransform);
            CreateFarmSceneRuntimeBootstrap(bootstrap.GetComponent<SaveManager>());
            CreateFarmTillingInputController(bootstrap.GetComponent<SaveManager>(), playerTransform, bootstrap.GetComponent<EquipmentManager>(),
                bootstrap.GetComponent<StaminaManager>(), bootstrap.GetComponent<TimeManager>());
            CreateSceneRuntimeInstaller(
                farmPlotRegistry,
                treeRegistry,
                itemPickupRegistry,
                playerTransform,
                bootstrap);

            ConfigureBootstrap(bootstrap, playerTransform, farmPlotRegistry, treeRegistry, itemPickupRegistry);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Selection.activeObject = sceneAsset;
            Debug.Log($"MVP FarmScene created at {ScenePath}.");
        }

        private static GameBootstrap CreateBootstrap()
        {
            var bootstrapObject = new GameObject("_Bootstrap");
            bootstrapObject.transform.position = Vector3.zero;

            var bootstrap = bootstrapObject.AddComponent<GameBootstrap>();
            bootstrapObject.AddComponent<PlayerManager>();
            bootstrapObject.AddComponent<InventoryManager>();
            bootstrapObject.AddComponent<TimeManager>();
            bootstrapObject.AddComponent<SaveManager>();
            bootstrapObject.AddComponent<DayAdvanceInput>();
            bootstrapObject.AddComponent<HungerManager>();
            bootstrapObject.AddComponent<StaminaManager>();
            bootstrapObject.AddComponent<GameTimeManager>();
            bootstrapObject.AddComponent<StatusEffectManager>();
            bootstrapObject.AddComponent<FoodConsumer>();
            bootstrapObject.AddComponent<SaveInput>();
            bootstrapObject.AddComponent<CraftingManager>();
            bootstrapObject.AddComponent<CraftingRuntime>();
            bootstrapObject.AddComponent<ModalManager>();
            bootstrapObject.AddComponent<EconomyManager>();
            bootstrapObject.AddComponent<ShopManager>();
            bootstrapObject.AddComponent<EquipmentManager>();
            bootstrapObject.AddComponent<PlayerProgressionManager>();
            bootstrapObject.AddComponent<SkillTreeManager>();
            bootstrapObject.AddComponent<BestiaryManager>();
            bootstrapObject.AddComponent<ManaManager>();
            bootstrapObject.AddComponent<HotbarDebugInput>();
            return bootstrap;
        }

        private static void ConfigureBootstrap(
            GameBootstrap bootstrap,
            Transform playerTransform,
            FarmPlotRegistry farmPlotRegistry,
            TreeRegistry treeRegistry,
            ItemPickupRegistry itemPickupRegistry)
        {
            var bootstrapObject = bootstrap.gameObject;
            var serializedBootstrap = new SerializedObject(bootstrap);

            SetReference(serializedBootstrap, "_playerManager", bootstrapObject.GetComponent<PlayerManager>());
            SetReference(serializedBootstrap, "_inventoryManager", bootstrapObject.GetComponent<InventoryManager>());
            SetReference(serializedBootstrap, "_timeManager", bootstrapObject.GetComponent<TimeManager>());
            SetReference(serializedBootstrap, "_saveManager", bootstrapObject.GetComponent<SaveManager>());
            SetReference(serializedBootstrap, "_hungerManager", bootstrapObject.GetComponent<HungerManager>());
            SetReference(serializedBootstrap, "_staminaManager", bootstrapObject.GetComponent<StaminaManager>());
            SetReference(serializedBootstrap, "_gameTimeManager", bootstrapObject.GetComponent<GameTimeManager>());
            SetReference(serializedBootstrap, "_statusEffectManager", bootstrapObject.GetComponent<StatusEffectManager>());
            SetReference(serializedBootstrap, "_craftingManager", bootstrapObject.GetComponent<CraftingManager>());
            SetReference(serializedBootstrap, "_modalManager", bootstrapObject.GetComponent<ModalManager>());
            SetReference(serializedBootstrap, "_economyManager", bootstrapObject.GetComponent<EconomyManager>());
            SetReference(serializedBootstrap, "_shopManager", bootstrapObject.GetComponent<ShopManager>());
            SetReference(serializedBootstrap, "_equipmentManager", bootstrapObject.GetComponent<EquipmentManager>());
            SetReference(serializedBootstrap, "_progressionManager", bootstrapObject.GetComponent<PlayerProgressionManager>());
            SetReference(serializedBootstrap, "_skillTreeManager", bootstrapObject.GetComponent<SkillTreeManager>());
            SetReference(serializedBootstrap, "_bestiaryManager", bootstrapObject.GetComponent<BestiaryManager>());
            PlayerNeedsDataInitializer.ConfigureRuntimeManagers(bootstrap, bootstrapObject.GetComponent<TimeManager>(), bootstrapObject.GetComponent<ModalManager>());
            ConfigureDayAdvanceInput(bootstrapObject.GetComponent<DayAdvanceInput>(), bootstrapObject.GetComponent<TimeManager>());
            ConfigureFoodConsumer(bootstrapObject.GetComponent<FoodConsumer>(), bootstrapObject.GetComponent<InventoryManager>(), bootstrapObject.GetComponent<HungerManager>());
            ConfigureSaveManager(
                bootstrapObject.GetComponent<SaveManager>(),
                bootstrapObject.GetComponent<PlayerManager>(),
                bootstrapObject.GetComponent<InventoryManager>(),
                bootstrapObject.GetComponent<HungerManager>(),
                bootstrapObject.GetComponent<TimeManager>(),
                farmPlotRegistry,
                treeRegistry,
                itemPickupRegistry,
                playerTransform,
                bootstrapObject.GetComponent<EquipmentManager>(),
                bootstrapObject.GetComponent<PlayerProgressionManager>(),
                bootstrapObject.GetComponent<CraftingRuntime>(),
                bootstrapObject.GetComponent<SkillTreeManager>(),
                bootstrapObject.GetComponent<ShopManager>(),
                bootstrapObject.GetComponent<BestiaryManager>());
            bootstrapObject.GetComponent<SaveManager>().RebindOptionalRuntimeManagers(
                bootstrapObject.GetComponent<EquipmentManager>(),
                bootstrapObject.GetComponent<PlayerProgressionManager>(),
                bootstrapObject.GetComponent<GameTimeManager>(),
                bootstrapObject.GetComponent<StaminaManager>(),
                bootstrapObject.GetComponent<StatusEffectManager>(),
                bootstrapObject.GetComponent<SkillTreeManager>(),
                bootstrapObject.GetComponent<ShopManager>(),
                bootstrapObject.GetComponent<BestiaryManager>());
            ConfigureSaveInput(bootstrapObject.GetComponent<SaveInput>(), bootstrapObject.GetComponent<SaveManager>());
            ConfigureHotbarDebugInput(
                bootstrapObject.GetComponent<HotbarDebugInput>(),
                bootstrapObject.GetComponent<SaveManager>());
            ConfigureCraftingManager(bootstrapObject.GetComponent<CraftingManager>(), bootstrapObject.GetComponent<InventoryManager>());
            ConfigureCraftingRuntime(bootstrapObject.GetComponent<CraftingRuntime>(), bootstrapObject.GetComponent<InventoryManager>());
            ConfigureEconomyManager(bootstrapObject.GetComponent<EconomyManager>(), bootstrapObject.GetComponent<InventoryManager>(), bootstrapObject.GetComponent<PlayerManager>());

            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData != null)
            {
                SetReference(serializedBootstrap, "_playerData", playerData);
                ConfigureHungerManager(bootstrapObject.GetComponent<HungerManager>(), playerData, bootstrapObject.GetComponent<PlayerManager>(), playerTransform);
            }
            else
            {
                Debug.LogWarning($"PlayerDataSO not found at {PlayerDataPath}. Assign it manually on GameBootstrap.");
                ConfigureHungerManager(bootstrapObject.GetComponent<HungerManager>(), null, bootstrapObject.GetComponent<PlayerManager>(), playerTransform);
            }

            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (itemDatabase != null)
            {
                SetReference(serializedBootstrap, "_itemDatabase", itemDatabase);
                var serializedShop = new SerializedObject(bootstrapObject.GetComponent<ShopManager>());
                SetReference(serializedShop, "_itemDatabase", itemDatabase);
                serializedShop.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bootstrapObject.GetComponent<ShopManager>());
            }
            else
            {
                Debug.LogWarning($"ItemDatabaseSO not found at {ItemDatabasePath}. Assign it manually on GameBootstrap.");
            }

            var weaponDatabase = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(WeaponDatabasePath);
            if (weaponDatabase != null)
            {
                SetReference(serializedBootstrap, "_weaponDatabase", weaponDatabase);
            }
            else
            {
                Debug.LogWarning($"WeaponDatabaseSO not found at {WeaponDatabasePath}. Assign it manually on GameBootstrap.");
            }

            var spellDatabase = AssetDatabase.LoadAssetAtPath<SpellDatabaseSO>(SpellDatabasePath);
            if (spellDatabase != null)
            {
                SetReference(serializedBootstrap, "_spellDatabase", spellDatabase);
            }
            else
            {
                Debug.LogWarning($"SpellDatabaseSO not found at {SpellDatabasePath}. Assign it manually on GameBootstrap.");
            }

            var statusEffectDatabase = AssetDatabase.LoadAssetAtPath<StatusEffectDatabaseSO>(StatusEffectDatabasePath);
            if (statusEffectDatabase != null)
            {
                SetReference(serializedBootstrap, "_statusEffectDatabase", statusEffectDatabase);
            }
            else
            {
                Debug.LogWarning($"StatusEffectDatabaseSO not found at {StatusEffectDatabasePath}. Create it or assign it manually on GameBootstrap.");
            }

            SetReference(serializedBootstrap, "_manaManager", bootstrapObject.GetComponent<ManaManager>());

            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
        }
        private static void ConfigureHotbarDebugInput(HotbarDebugInput hotbarDebugInput, SaveManager saveManager)
        {
            var serializedInput = new SerializedObject(hotbarDebugInput);
            SetReference(serializedInput, "_saveManager", saveManager);
            serializedInput.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hotbarDebugInput);
        }

        private static void ConfigureSaveManager(
            SaveManager saveManager,
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            HungerManager hungerManager,
            TimeManager timeManager,
            FarmPlotRegistry farmPlotRegistry,
            TreeRegistry treeRegistry,
            ItemPickupRegistry itemPickupRegistry,
            Transform playerTransform,
            EquipmentManager equipmentManager,
            PlayerProgressionManager progressionManager,
            CraftingRuntime craftingRuntime,
            SkillTreeManager skillTreeManager,
            ShopManager shopManager,
            BestiaryManager bestiaryManager)
        {
            var serializedSave = new SerializedObject(saveManager);
            SetReference(serializedSave, "_playerManager", playerManager);
            SetReference(serializedSave, "_inventoryManager", inventoryManager);
            SetReference(serializedSave, "_hungerManager", hungerManager);
            SetReference(serializedSave, "_timeManager", timeManager);
            SetReference(serializedSave, "_farmPlotRegistry", farmPlotRegistry);
            SetReference(serializedSave, "_treeRegistry", treeRegistry);
            SetReference(serializedSave, "_itemPickupRegistry", itemPickupRegistry);
            SetReference(serializedSave, "_playerTransform", playerTransform);
            SetReference(serializedSave, "_equipmentManager", equipmentManager);
            SetReference(serializedSave, "_progressionManager", progressionManager);
            SetReference(serializedSave, "_craftingRuntime", craftingRuntime);
            SetReference(serializedSave, "_skillTreeManager", skillTreeManager);
            SetReference(serializedSave, "_shopManager", shopManager);
            SetReference(serializedSave, "_bestiaryManager", bestiaryManager);
            serializedSave.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(saveManager);
        }

        private static void ConfigureSaveInput(SaveInput saveInput, SaveManager saveManager)
        {
            var serializedInput = new SerializedObject(saveInput);
            SetReference(serializedInput, "_saveManager", saveManager);
            serializedInput.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(saveInput);
        }

        private static void ConfigureHungerManager(HungerManager hungerManager, PlayerDataSO playerData, PlayerManager playerManager, Transform playerTransform)
        {
            var serializedHunger = new SerializedObject(hungerManager);
            SetReference(serializedHunger, "_playerData", playerData);
            SetReference(serializedHunger, "_playerManager", playerManager);
            SetReference(serializedHunger, "_playerTransform", playerTransform);
            serializedHunger.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hungerManager);
        }

        private static void ConfigureFoodConsumer(FoodConsumer foodConsumer, InventoryManager inventoryManager, HungerManager hungerManager)
        {
            var serializedConsumer = new SerializedObject(foodConsumer);
            SetReference(serializedConsumer, "_inventoryManager", inventoryManager);
            SetReference(serializedConsumer, "_hungerManager", hungerManager);
            SetReference(serializedConsumer, "_staminaManager", foodConsumer.GetComponent<StaminaManager>());
            SetReference(serializedConsumer, "_statusEffectManager", foodConsumer.GetComponent<StatusEffectManager>());
            serializedConsumer.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(foodConsumer);
        }

        private static void ConfigureDayAdvanceInput(DayAdvanceInput dayAdvanceInput, TimeManager timeManager)
        {
            var serializedInput = new SerializedObject(dayAdvanceInput);
            SetReference(serializedInput, "_timeManager", timeManager);
            serializedInput.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(dayAdvanceInput);
        }

        private static void ConfigureCraftingManager(CraftingManager craftingManager, InventoryManager inventoryManager)
        {
            var serializedCrafting = new SerializedObject(craftingManager);
            SetReference(serializedCrafting, "_inventoryManager", inventoryManager);

            var recipeDatabase = AssetDatabase.LoadAssetAtPath<RecipeDatabaseSO>(RecipeDatabasePath);
            if (recipeDatabase != null)
            {
                SetReference(serializedCrafting, "_recipeDatabase", recipeDatabase);
            }
            else
            {
                Debug.LogWarning($"RecipeDatabaseSO not found at {RecipeDatabasePath}. Assign it manually on CraftingManager.");
            }

            serializedCrafting.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(craftingManager);
        }

        private static void ConfigureCraftingRuntime(CraftingRuntime craftingRuntime, InventoryManager inventoryManager)
        {
            var serializedCrafting = new SerializedObject(craftingRuntime);
            SetReference(serializedCrafting, "_inventoryManager", inventoryManager);
            SetReference(serializedCrafting, "_recipeDatabase", AssetDatabase.LoadAssetAtPath<RecipeDatabaseSO>(RecipeDatabasePath));
            SetReference(serializedCrafting, "_staminaManager", craftingRuntime.GetComponent<StaminaManager>());
            serializedCrafting.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(craftingRuntime);
        }

        private static void ConfigureEconomyManager(EconomyManager economyManager, InventoryManager inventoryManager, PlayerManager playerManager)
        {
            var serializedEconomy = new SerializedObject(economyManager);
            SetReference(serializedEconomy, "_inventoryManager", inventoryManager);
            SetReference(serializedEconomy, "_playerManager", playerManager);
            serializedEconomy.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(economyManager);
        }

        private static Transform CreatePlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            // spec_codex_13: layer de gameplay para queries de combate (ContactFilter2D).
            CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                player, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.Player);
            // Size from the authored Player scale profile (VisualScale 2.0), not a hardcoded localScale —
            // keeps player/NPC/world sizing in one data source. Falls back if the profile asset is absent.
            if (!ScaleProfileLibrary.AttachApplicator(player, EntityScaleCategory.Player))
            {
                player.transform.localScale = new Vector3(1f, 1.5f, 1f);
            }

            // Escala visual do player com sprites pixel art a 128 PPU (~0.5625u/altura).
            // Valor afinavel: 1.3f resulta em ~0.73 unidade de altura na cena.
            const float PlayerVisualScale = 1.3f;

            var spriteRenderer = player.AddComponent<SpriteRenderer>();
            var walkDownFrame1 = Resources.Load<Sprite>("PlayerSprites/walk/down/walk_down_01");
            if (walkDownFrame1 != null)
            {
                spriteRenderer.sprite = walkDownFrame1;
            }
            else
            {
                spriteRenderer.sprite = GetBuiltinSprite();
                Debug.LogWarning("[PlayerWalkAnimator] Farm: Resources/PlayerSprites/walk/down/walk_down_01 nao encontrado. " +
                                 "Usando sprite builtin como fallback. Reimporte os sprites do player.");
            }
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(spriteRenderer, "World", 0);

            var rigidbody = player.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.gravityScale = 0f;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = player.AddComponent<BoxCollider2D>();
            FitBoxColliderToOpaqueSprite(collider, spriteRenderer);

            var playerController = player.AddComponent<PlayerController>();
            ConfigurePlayerController(playerController, rigidbody);

            player.AddComponent<CindarsHope.Player.PlayerWalkAnimator>();

            // Sobrescreve a escala definida pelo ScaleProfileLibrary para adequar o sprite pixel art.
            player.transform.localScale = new Vector3(PlayerVisualScale, PlayerVisualScale, 1f);

            var interactionSystem = player.AddComponent<InteractionSystem>();
            var interactionTrigger = CreateInteractionTrigger(player.transform, interactionSystem);
            ConfigureInteractionSystem(interactionSystem, interactionTrigger);

            // SPEC_04: PlayerAttackController is the authoritative attack handler and includes spell casting.
            // Legacy PlayerSpellCaster and FireballItemBridge are no longer added here.
            // They are preserved in-codebase for fallback but marked as QUARANTINED.
            var attackController = player.AddComponent<CindarsHope.Combat.PlayerAttackController>();
            var serializedAttack = new SerializedObject(attackController);
            SetReference(serializedAttack, "_playerController", playerController);
            serializedAttack.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(attackController);

            // WAVE_INTEGRATION_11: Add Dash (Space+direction) and Dodge (double-tap) controllers.
            // These are movement abilities; they do NOT occupy active skill slots.
            // Design: COMBAT_CORE_DIRECTION.md §13 (Dash: 3.5 tiles, 40 Stamina) and §14 (Dodge: 1.5 tiles, 40 Stamina).
            var dashControllerType = System.Type.GetType("CindarsHope.Player.Movement.PlayerDashController, Assembly-CSharp");
            if (dashControllerType != null)
            {
                var dashController = player.AddComponent(dashControllerType);
                var serializedDash = new SerializedObject(dashController);
                // PlayerDashController has no _rigidbody field; it self-resolves
                // _displacementResolver/_staminaManager in Start(). Only wire _playerController here.
                SetReference(serializedDash, "_playerController", playerController);
                serializedDash.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(dashController);
            }
            else
            {
                Debug.LogWarning("CreateMvpFarmScene: PlayerDashController type not found. Scene may need regeneration after reimport.");
            }

            var movAbilityType = System.Type.GetType("CindarsHope.Player.Movement.PlayerMovementAbilityController, Assembly-CSharp");
            if (movAbilityType != null)
            {
                var movementAbilityController = player.AddComponent(movAbilityType);
                var serializedMovAbility = new SerializedObject(movementAbilityController);
                SetReference(serializedMovAbility, "_rigidbody", rigidbody);
                SetReference(serializedMovAbility, "_playerController", playerController);
                serializedMovAbility.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(movementAbilityController);
            }
            else
            {
                Debug.LogWarning("CreateMvpFarmScene: PlayerMovementAbilityController type not found. Scene may need regeneration after reimport.");
            }

            return player.transform;
        }

        private static CircleCollider2D CreateInteractionTrigger(Transform parent, InteractionSystem interactionSystem)
        {
            var triggerObject = new GameObject("InteractionTrigger");
            triggerObject.transform.SetParent(parent);
            triggerObject.transform.localPosition = Vector3.zero;
            triggerObject.transform.localRotation = Quaternion.identity;
            triggerObject.transform.localScale = Vector3.one;

            var trigger = triggerObject.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.55f;

            var relay = triggerObject.AddComponent<InteractionTriggerRelay>();
            relay.Configure(interactionSystem);

            return trigger;
        }

        private static void ConfigurePlayerController(PlayerController playerController, Rigidbody2D rigidbody)
        {
            var serializedController = new SerializedObject(playerController);
            SetReference(serializedController, "_rigidbody", rigidbody);

            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData != null)
            {
                SetReference(serializedController, "_playerData", playerData);
            }
            else
            {
                Debug.LogWarning($"PlayerDataSO not found at {PlayerDataPath}. Assign it manually on PlayerController.");
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
            PlayerNeedsDataInitializer.ConfigurePlayerController(playerController);
            EditorUtility.SetDirty(playerController);
        }

        private static void ConfigureInteractionSystem(InteractionSystem interactionSystem, Collider2D interactionTrigger)
        {
            var serializedInteraction = new SerializedObject(interactionSystem);
            SetReference(serializedInteraction, "_interactionTrigger", interactionTrigger);
            serializedInteraction.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactionSystem);
        }

        private static void CreateFarmSpawnPoints(Transform playerTransform)
        {
            var parent = new GameObject("SpawnPoints");
            parent.transform.position = Vector3.zero;

            // v7 spawn anchors — IDs mantidos (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coords 64x44: farm_default (24,3) homestead, from_town (29,-1) junto portal, from_cave (-23,15).
            var defaultSpawn = CreateSceneSpawnPoint(parent.transform, "farm_default", new Vector3(24f, 3f, 0f));
            var fromTownSpawn = CreateSceneSpawnPoint(parent.transform, "farm_from_town", new Vector3(29f, -1f, 0f));
            var fromCaveSpawn = CreateSceneSpawnPoint(parent.transform, "farm_from_cave", new Vector3(-23f, 15f, 0f));

            var installer = parent.AddComponent<SceneSpawnInstaller>();
            var serializedInstaller = new SerializedObject(installer);
            SetReference(serializedInstaller, "_playerTransform", playerTransform);
            serializedInstaller.FindProperty("_spawnPoints").arraySize = 3;
            serializedInstaller.FindProperty("_spawnPoints").GetArrayElementAtIndex(0).objectReferenceValue = defaultSpawn;
            serializedInstaller.FindProperty("_spawnPoints").GetArrayElementAtIndex(1).objectReferenceValue = fromTownSpawn;
            serializedInstaller.FindProperty("_spawnPoints").GetArrayElementAtIndex(2).objectReferenceValue = fromCaveSpawn;
            serializedInstaller.FindProperty("_defaultSpawnId").stringValue = "farm_default";
            serializedInstaller.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
        }

        private static SceneSpawnPoint CreateSceneSpawnPoint(Transform parent, string spawnId, Vector3 position)
        {
            var spawnObject = new GameObject($"Spawn_{spawnId}");
            spawnObject.transform.SetParent(parent);
            spawnObject.transform.position = position;

            var spawnPoint = spawnObject.AddComponent<SceneSpawnPoint>();

            var serializedSpawn = new SerializedObject(spawnPoint);
            var spawnIdProperty = serializedSpawn.FindProperty("_spawnId");
            if (spawnIdProperty != null)
            {
                spawnIdProperty.stringValue = spawnId;
                serializedSpawn.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"CreateSceneSpawnPoint: Could not find _spawnId property on {spawnPoint.GetType().Name}. Using reflection fallback.", spawnPoint);
                var field = spawnPoint.GetType().GetField("_spawnId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(spawnPoint, spawnId);
                }
            }

            EditorUtility.SetDirty(spawnPoint);
            return spawnPoint;
        }

        private static void CreateFarmPortals()
        {
            var portals = new GameObject("Portals");
            portals.transform.position = Vector3.zero;

            // v7: portal na extrema direita, borda leste (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: (31, -1).
            CreateScenePortal(
                portals.transform,
                "Portal_Farm_To_Town",
                new Vector3(31f, -1f, 0f),
                new Color(0.29f, 0.43f, 0.67f),
                "TownScene",
                "Assets/_Game/Scenes/TownScene.unity",
                "town_from_farm",
                "Ir para Cindar's Hope");

            // Cave access moved to CaveEntranceInteractable (CreateCaveEntrance), which
            // routes through SceneTransitionRouter with run-state validation (WAVE16).
        }

        private static void CreateScenePortal(
            Transform parent,
            string name,
            Vector3 position,
            Color color,
            string targetSceneName,
            string targetScenePath,
            string targetSpawnId,
            string interactionPrompt)
        {
            var portalObject = new GameObject(name);
            portalObject.transform.SetParent(parent);
            portalObject.transform.position = position;
            if (!ScaleProfileLibrary.AttachApplicator(portalObject, EntityScaleCategory.CheckpointPortal))
            {
                portalObject.transform.localScale = new Vector3(1f, 1.35f, 1f);
            }

            var spriteRenderer = portalObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(spriteRenderer, "World", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"{name} placeholder SpriteRenderer was created without a sprite. Replace it with portal art in a future art PR.");
            }

            var collider = portalObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var portal = portalObject.AddComponent<ScenePortal>();
            var serializedPortal = new SerializedObject(portal);
            serializedPortal.FindProperty("_targetSceneName").stringValue = targetSceneName;
            serializedPortal.FindProperty("_targetScenePath").stringValue = targetScenePath;
            serializedPortal.FindProperty("_targetSpawnId").stringValue = targetSpawnId;
            serializedPortal.FindProperty("_interactionPrompt").stringValue = interactionPrompt;
            serializedPortal.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(portal);
        }

        private static void CreateDebugInteractable()
        {
            var debugInteractableObject = new GameObject("DebugInteractable");
            debugInteractableObject.transform.position = new Vector3(2f, 0f, 0f);
            debugInteractableObject.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

            var spriteRenderer = debugInteractableObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.95f, 0.82f, 0.22f);
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(spriteRenderer, "World", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("DebugInteractable placeholder SpriteRenderer was created without a sprite. Replace it with a future placeholder sprite if needed.");
            }

            var collider = debugInteractableObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            debugInteractableObject.AddComponent<DebugInteractable>();
        }

        private static void CreateSellPoint(InventoryManager inventoryManager, PlayerManager playerManager)
        {
            var sellPointObject = new GameObject("SellPoint");
            // v7: homestead leste (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: (24, 5).
            sellPointObject.transform.position = new Vector3(24f, 5f, 0f);
            if (!ScaleProfileLibrary.AttachApplicator(sellPointObject, EntityScaleCategory.FarmObject))
            {
                sellPointObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            }

            var spriteRenderer = sellPointObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.25f, 0.75f, 0.85f);
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(spriteRenderer, "World", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("SellPoint placeholder SpriteRenderer was created without a sprite. Replace it with economy art in a future art PR.");
            }

            var collider = sellPointObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var sellPoint = sellPointObject.AddComponent<SellPoint>();
            var serializedSellPoint = new SerializedObject(sellPoint);
            SetReference(serializedSellPoint, "_inventoryManager", inventoryManager);
            SetReference(serializedSellPoint, "_playerManager", playerManager);
            serializedSellPoint.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sellPoint);
        }

        private static void CreateSeedShopPoint(InventoryManager inventoryManager, PlayerManager playerManager)
        {
            var shopObject = new GameObject("SeedShopPoint");
            shopObject.transform.position = new Vector3(-4.75f, -3.25f, 0f);
            shopObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var spriteRenderer = shopObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.78f, 0.48f, 0.18f);
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(spriteRenderer, "World", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("SeedShopPoint placeholder SpriteRenderer was created without a sprite. Replace it with shop art in a future art PR.");
            }

            var collider = shopObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var seedShopPoint = shopObject.AddComponent<SeedShopPoint>();
            var serializedShop = new SerializedObject(seedShopPoint);
            SetReference(serializedShop, "_inventoryManager", inventoryManager);
            SetReference(serializedShop, "_playerManager", playerManager);
            serializedShop.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(seedShopPoint);
        }

        private static CraftingModal CreateCraftingUi(CraftingRuntime craftingRuntime, ModalManager modalManager)
        {
            var canvasObject = new GameObject("CraftingCanvas", typeof(Canvas));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var craftingModal = canvasObject.AddComponent<CraftingModal>();
            craftingModal.Configure(craftingRuntime, modalManager);
            return craftingModal;
        }

        private static void CreateCraftingStations(CraftingRuntime craftingRuntime, CraftingModal craftingModal)
        {
            // v7: nicho de craft FORA da casa, homestead leste (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coords 64x44: Workbench (25,1), Forge (27,1), CookingStation (29,1).
            CreateCraftingStation("Workbench", "farm_workbench_01", WorkshopType.Workbench, new Vector3(25f, 1f, 0f), new Color(0.58f, 0.36f, 0.18f), craftingRuntime, craftingModal);
            CreateCraftingStation("Forge", "farm_forge_01", WorkshopType.Forge, new Vector3(27f, 1f, 0f), new Color(0.58f, 0.23f, 0.16f), craftingRuntime, craftingModal);
            CreateCraftingStation("CookingStation", "farm_cooking_01", WorkshopType.CookingStation, new Vector3(29f, 1f, 0f), new Color(0.77f, 0.55f, 0.22f), craftingRuntime, craftingModal);
        }

        private static void CreateCraftingStation(string label, string stationId, WorkshopType stationType, Vector3 position, Color color, CraftingRuntime craftingRuntime, CraftingModal craftingModal)
        {
            var craftingObject = new GameObject($"CraftingStation_{label}");
            craftingObject.transform.position = position;
            var stationCategory = stationType switch
            {
                WorkshopType.Forge => EntityScaleCategory.Forge,
                WorkshopType.CookingStation => EntityScaleCategory.CookingStation,
                _ => EntityScaleCategory.Workbench,
            };
            if (!ScaleProfileLibrary.AttachApplicator(craftingObject, stationCategory))
            {
                craftingObject.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            }

            var spriteRenderer = craftingObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(spriteRenderer, "World", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"CraftingStation_{label} placeholder SpriteRenderer was created without a sprite. Replace it with workshop art in a future art PR.");
            }

            var collider = craftingObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var craftingPoint = craftingObject.AddComponent<CraftingPoint>();
            craftingPoint.Configure(stationId, stationType, craftingRuntime, craftingModal);
            EditorUtility.SetDirty(craftingPoint);
        }

        private static void CreateFishingSpot(InventoryManager inventoryManager)
        {
            var fishingObject = new GameObject("FishingSpot");
            // v7: margem oeste do lago SE (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: (8, -9).
            fishingObject.transform.position = new Vector3(8f, -9f, 0f);
            var lakeScaleConfig = AssetDatabase.LoadAssetAtPath<GameScaleConfigSO>(GameScaleConfigPath);
            var lakeScale = lakeScaleConfig != null ? lakeScaleConfig.LakeScale : 6f;
            fishingObject.transform.localScale = new Vector3(lakeScale, lakeScale, 1f);

            var spriteRenderer = fishingObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.18f, 0.42f, 0.85f);
            spriteRenderer.sortingOrder = 0;
            TrySetSortingLayer(spriteRenderer, "Ground", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("FishingSpot placeholder SpriteRenderer was created without a sprite. Replace it with water/fishing art in a future art PR.");
            }

            var blockingCollider = fishingObject.AddComponent<BoxCollider2D>();
            blockingCollider.isTrigger = false;
            FitLakeBlockingCollider(blockingCollider, spriteRenderer);

            var fishingSpot = fishingObject.AddComponent<FishingSpot>();
            var serializedFishing = new SerializedObject(fishingSpot);
            SetReference(serializedFishing, "_inventoryManager", inventoryManager);
            serializedFishing.FindProperty("_edgeInteractionOuterHalfExtents").vector2Value = new Vector2(0.16f, 0.16f);
            serializedFishing.FindProperty("_edgeInteractionInnerHalfExtents").vector2Value = new Vector2(0.055f, 0.055f);
            serializedFishing.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(fishingSpot);

            CreateLakeEdgeTrigger(fishingObject.transform, "LakeEdgeInteractionTrigger_Top", new Vector3(0f, 0.085f, 0f), new Vector2(0.18f, 0.025f));
            CreateLakeEdgeTrigger(fishingObject.transform, "LakeEdgeInteractionTrigger_Bottom", new Vector3(0f, -0.085f, 0f), new Vector2(0.18f, 0.025f));
            CreateLakeEdgeTrigger(fishingObject.transform, "LakeEdgeInteractionTrigger_Left", new Vector3(-0.085f, 0f, 0f), new Vector2(0.025f, 0.18f));
            CreateLakeEdgeTrigger(fishingObject.transform, "LakeEdgeInteractionTrigger_Right", new Vector3(0.085f, 0f, 0f), new Vector2(0.025f, 0.18f));
        }

        private static void CreateLakeEdgeTrigger(Transform parent, string name, Vector3 localPosition, Vector2 size)
        {
            var triggerObject = new GameObject(name);
            triggerObject.transform.SetParent(parent);
            triggerObject.transform.localPosition = localPosition;
            triggerObject.transform.localRotation = Quaternion.identity;
            triggerObject.transform.localScale = Vector3.one;

            var collider = triggerObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
        }

        private static void CreateFarmSceneFoundationZones()
        {
            var parent = new GameObject("FarmSceneFoundationZones");
            parent.transform.position = Vector3.zero;

            // v7 (spec_farm_scene_relayout_v4 §15.5 v7): disposicao 64x44 borda aberta.
            // Spawn padrao no homestead leste (24,3).
            CreateFarmSceneZone(parent.transform, "Zone_PlayerSpawn", FarmSceneZoneType.PlayerSpawn, "farm_zone_player_spawn", new Vector3(24f, 3f, 0f), new Vector2(1.4f, 1.4f), new Color(0.2f, 0.45f, 0.95f, 0.65f));
            // Solo aravel — CropField cobre o MIOLO central (x[-18,16] y[-15,16]).
            CreateFarmSceneZone(parent.transform, "Zone_CropField", FarmSceneZoneType.CropField, "farm_zone_crop_field", new Vector3(-1f, 0.5f, 0f), new Vector2(34f, 31f), new Color(0.35f, 0.22f, 0.12f, 0.25f));
            // Bosque DENSO borda oeste (x[-32,-22] y[0,17]).
            CreateFarmSceneZone(parent.transform, "Zone_ResourceTrees", FarmSceneZoneType.ResourceTrees, "farm_zone_resource_trees", new Vector3(-27f, 8.5f, 0f), new Vector2(10f, 17f), new Color(0.14f, 0.48f, 0.18f, 0.35f));
            CreateFarmSceneZone(parent.transform, "Zone_ResourceRocks", FarmSceneZoneType.ResourceRocks, "farm_zone_resource_rocks", new Vector3(-31f, 6f, 0f), new Vector2(2f, 2f), new Color(0.42f, 0.42f, 0.42f, 0.55f));
            // Forage cluster borda oeste (v7: 6 pontos em (-31,-4)(-29,-4)etc).
            CreateFarmSceneZone(parent.transform, "Zone_Forage", FarmSceneZoneType.Forage, "farm_zone_forage", new Vector3(-30f, -4f, 0f), new Vector2(4f, 6f), new Color(0.45f, 0.64f, 0.25f, 0.4f));
            // Lago SE v7 (x[5,31] y[-20,-6]).
            CreateFarmSceneZone(parent.transform, "Zone_LakeFishing", FarmSceneZoneType.LakeFishing, "farm_zone_lake_fishing", new Vector3(18f, -13f, 0f), new Vector2(26f, 14f), new Color(0.18f, 0.44f, 0.82f, 0.5f));
            // Envio + venda homestead leste (v7: ShippingBin 28,4 / SellPoint 24,5).
            CreateFarmSceneZone(parent.transform, "Zone_ShippingSellpoint", FarmSceneZoneType.ShippingSellpoint, "farm_zone_shipping_sellpoint", new Vector3(26f, 4.5f, 0f), new Vector2(8f, 3f), new Color(0.85f, 0.62f, 0.18f, 0.65f));
            // Construcoes BORDA SUL (v7: coop -22,-19 / barn -13,-19 / cheese -28,-19 / wine -8,-19).
            CreateFarmSceneZone(parent.transform, "Zone_Construction", FarmSceneZoneType.Construction, "farm_zone_construction", new Vector3(-18f, -19f, 0f), new Vector2(24f, 5f), new Color(0.58f, 0.45f, 0.32f, 0.45f));
            // Casa walk-in a leste (v7: centro 28,9, footprint ~7x6).
            CreateFarmSceneZone(parent.transform, "Zone_HouseEntrance", FarmSceneZoneType.HouseEntrance, "farm_zone_house_entrance", new Vector3(28f, 9f, 0f), new Vector2(7f, 6f), new Color(0.62f, 0.36f, 0.25f, 0.65f));
            // Portal da cidade na extrema direita (v7: 31, -1).
            CreateFarmSceneZone(parent.transform, "Zone_TownExit", FarmSceneZoneType.TownExit, "farm_zone_town_exit", new Vector3(31f, -1f, 0f), new Vector2(2f, 3f), new Color(0.82f, 0.82f, 0.25f, 0.55f));
            // Caverna canto NO da montanha (v7: -28, 18.5) — MAIOR ~5x4.
            CreateFarmSceneZone(parent.transform, "Zone_CaveEntrance", FarmSceneZoneType.CaveEntrance, "farm_zone_cave_entrance", new Vector3(-28f, 18.5f, 0f), new Vector2(5f, 4f), new Color(0.35f, 0.28f, 0.5f, 0.65f));
        }

        private static void CreateFarmSceneZone(
            Transform parent,
            string name,
            FarmSceneZoneType zoneType,
            string stableId,
            Vector3 position,
            Vector2 size,
            Color color)
        {
            var markerObject = new GameObject(name);
            markerObject.transform.SetParent(parent);
            markerObject.transform.position = position;
            // v5 (spec_farm_scene_relayout_v4 §15.3 item 1): zonas NAO renderizam em jogo.
            // SpriteRenderer removido — zona e visivel APENAS via OnDrawGizmos em FarmSceneZoneMarker.
            // O collider trigger permanece para logica (FarmForageRuntimeService, etc.).
            // A escala do transform nao importa mais para visual (collider usa size em space local).
            markerObject.transform.localScale = Vector3.one;

            var collider = markerObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size; // tamanho real da zona em world units (sem depender de localScale)

            var marker = markerObject.AddComponent<FarmSceneZoneMarker>();
            var serializedMarker = new SerializedObject(marker);
            serializedMarker.FindProperty("zoneType").enumValueIndex = (int)zoneType;
            serializedMarker.FindProperty("stableId").stringValue = stableId;
            serializedMarker.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(marker);
        }

        // Chao base texturizado. As zonas de fundacao NAO renderizam (gizmo-only), entao sem isto
        // o mundo aparece cinza. Adiciona um unico SpriteRenderer de grama em modo Tiled cobrindo o
        // footprint da fazenda (64x44 + margem), atras de tudo. Arte via WorldSpriteLibrary
        // (Art/Generated/World/tiles/ground_grass). Se ausente, loga wiring-error e nao cria (nao mascara).
        private static void CreateFarmGroundTexture()
        {
            var grass = WorldSpriteLibrary.Ground("ground_grass");
            if (grass == null)
            {
                Debug.LogWarning("CreateMvpFarmScene: 'ground_grass' ausente — chao texturizado NAO criado " +
                                 "(mundo permanece sem base). Rode art/world_gpt/_stage_world_to_assets.py e reimporte em Unity.");
                return;
            }

            // Chao via TILEMAP (best practice) — nao SpriteRenderer Tiled (estoura mesh) nem esticado.
            // Ver skill tilemap-world-rendering. A tile de grama e seamless, entao o Tilemap da campo
            // uniforme sem grade nem erro de 9-slice. Rule Tiles/variacao = passo futuro (2D Extras).
            var ground = new GameObject("FarmGround");
            ground.transform.position = Vector3.zero;
            // Grama COM VARIACAO (base + variantes esparsas) via Tilemap — quebra a repeticao.
            WorldTilemapGround.PaintGrass(ground.transform, "WorldGrid", 0, "Ground", new Vector2(-1f, -1f), new Vector2(72f, 52f));
        }

        private static ItemPickupRegistry CreateItemPickups(InventoryManager inventoryManager)
        {
            var parent = new GameObject("ItemPickups");
            parent.transform.position = Vector3.zero;
            var registry = parent.AddComponent<ItemPickupRegistry>();

            var pickups = new ItemPickup[1];
            // v7: perto do spawn homestead (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: (22, 1) — homestead leste, acessivel ao sair da casa.
            pickups[0] = CreateItemPickup(parent.transform, 0, "seed_carrot", 1, new Vector3(22f, 1f, 0f), inventoryManager);
            registry.Configure(pickups);
            EditorUtility.SetDirty(registry);
            return registry;
        }

        private static ItemPickup CreateItemPickup(
            Transform parent,
            int pickupIndex,
            string itemId,
            int amount,
            Vector3 position,
            InventoryManager inventoryManager)
        {
            var pickupObject = new GameObject("DebugCarrotSeedPickup");
            pickupObject.transform.SetParent(parent);
            pickupObject.transform.position = position;
            if (!ScaleProfileLibrary.AttachApplicator(pickupObject, EntityScaleCategory.Pickup))
            {
                pickupObject.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
            }

            var spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.95f, 0.5f, 0.22f);
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(spriteRenderer, "World", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("DebugCarrotSeedPickup placeholder SpriteRenderer was created without a sprite. Replace it with item art in a future art PR.");
            }

            var collider = pickupObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var pickup = pickupObject.AddComponent<ItemPickup>();
            var serializedPickup = new SerializedObject(pickup);
            serializedPickup.FindProperty("_pickupIndex").intValue = pickupIndex;
            serializedPickup.FindProperty("_itemId").stringValue = itemId;
            serializedPickup.FindProperty("_amount").intValue = amount;
            SetReference(serializedPickup, "_inventoryManager", inventoryManager);
            SetReference(serializedPickup, "_spriteRenderer", spriteRenderer);
            SetReference(serializedPickup, "_collider", collider);
            serializedPickup.ApplyModifiedPropertiesWithoutUndo();

            pickup.Configure(pickupIndex, itemId, amount, inventoryManager);
            EditorUtility.SetDirty(pickup);
            return pickup;
        }

        // WAVE_INTEGRATION_09: GameplayInputRouter provides centralized I/K/Esc routing.
        // InventoryPanelController and CharacterEquipmentPanelController auto-create via
        // [RuntimeInitializeOnLoadMethod] - they do not need explicit scene wiring.
        private static void CreateGameplayInputRouter()
        {
            var routerObject = new GameObject("GameplayInputRouter");
            routerObject.AddComponent<GameplayInputRouter>();
        }

        // WAVE_INTEGRATION_11: Creates the ActiveSkillExecutionController in the scene.
        // This controller bridges numeric keys 1-4 → active skill slots → SkillEffectRegistry → executor.
        // The RuntimeInitializeOnLoadMethod in ActiveSkillExecutionController creates it at runtime
        // if not present, but scene placement allows inspector wiring for future expansions.
        // Type resolved via string to avoid Editor build dependency on runtime assembly files
        // not yet tracked by the Unity-generated Assembly-CSharp-Editor.csproj.
        private static void CreateActiveSkillExecutionController()
        {
            var controllerType = System.Type.GetType("CindarsHope.Skills.Runtime.Effects.ActiveSkillExecutionController, Assembly-CSharp");
            if (controllerType == null)
            {
                Debug.LogWarning("CreateMvpFarmScene: ActiveSkillExecutionController type not found. RuntimeInitializeOnLoadMethod will create it at runtime. Regenerate scene after Unity reimport.");
                return;
            }
            var controllerObject = new GameObject("ActiveSkillExecutionController");
            var controller = controllerObject.AddComponent(controllerType);
            EditorUtility.SetDirty(controller);
        }

        private static void CreateDebugHud(
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            HungerManager hungerManager,
            InteractionSystem interactionSystem,
            TimeManager timeManager,
            SaveManager saveManager)
        {
            var hudObject = new GameObject("DebugHud");
            var debugHud = hudObject.AddComponent<DebugHud>();
            var serializedHud = new SerializedObject(debugHud);
            SetReference(serializedHud, "_playerManager", playerManager);
            SetReference(serializedHud, "_inventoryManager", inventoryManager);
            SetReference(serializedHud, "_hungerManager", hungerManager);
            SetReference(serializedHud, "_interactionSystem", interactionSystem);
            SetReference(serializedHud, "_timeManager", timeManager);
            SetReference(serializedHud, "_saveManager", saveManager);
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(debugHud);
        }

        // F17: Fonte de Anya física (bacia + água + pedestal) perto da casa, com interactable
        // e wiring do respawn point no GameBootstrap (_anyaFountain).
        private static void CreateFonteAnya(CindarsHope.Core.Bootstrap.GameBootstrap bootstrap)
        {
            var fonteRoot = new GameObject("FonteAnya");
            // v7: borda oeste, clareira do bosque — marco cenico + respawn (preservar wiring _anyaFountain).
            // Coord 64x44 (spec_farm_scene_relayout_v4 §15.5 v7): (-24, 4).
            fonteRoot.transform.position = new Vector3(-24f, 4f, 0f);

            var pedestal = CreateFontePart(fonteRoot.transform, "Pedestal", new Vector3(0f, -0.15f, 0f), new Vector3(1.6f, 0.5f, 1f), new Color(0.55f, 0.55f, 0.6f), 1);
            CreateFontePart(fonteRoot.transform, "Bacia", new Vector3(0f, 0.15f, 0f), new Vector3(1.3f, 0.55f, 1f), new Color(0.42f, 0.45f, 0.55f), 2);
            CreateFontePart(fonteRoot.transform, "Agua", new Vector3(0f, 0.22f, 0f), new Vector3(1.0f, 0.35f, 1f), new Color(0.35f, 0.65f, 0.95f), 3);

            var collider = fonteRoot.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.8f, 1.2f);

            var fountain = fonteRoot.AddComponent<CindarsHope.Locations.AnyaFountain>();
            fonteRoot.AddComponent<CindarsHope.Fonte.FonteInteractable>();

            if (bootstrap != null)
            {
                var serializedBootstrap = new SerializedObject(bootstrap);
                SetReference(serializedBootstrap, "_anyaFountain", fountain);
                serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bootstrap);
            }

            EditorUtility.SetDirty(fonteRoot);
        }

        private static GameObject CreateFontePart(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;

            var renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBuiltinSprite();
            renderer.color = color;
            // Desempate dentro do mesmo prop (mesma layer World, order 0), igual ao padrao da
            // CreateStatuePart da cidade: epsilon de Y decrescente por sortingOrder original.
            part.transform.localPosition -= new Vector3(0f, sortingOrder * 0.001f, 0f);
            renderer.sortingOrder = 0;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(renderer, "World", renderer.sortingOrder);
            return part;
        }

        // ── Constantes do padrao de casa percorrivel (replicam CreateMvpTownScene) ────────────────
        private const float FarmWallThickness = 0.22f;
        private const float FarmDoorGapWidth  = 1.0f;

        // v7 (spec_farm_scene_relayout_v4 §15.5 v7): Casa WALK-IN da fazenda.
        // Centro (28,9), footprint 7x6, porta ao sul (y negativo). Interior percorrivel.
        // Bed + BedLetter + FarmHouseChest criados DENTRO do interior.
        // RoofRevealController some ao entrar; HouseDoorInteractable [E] abre.
        // Padrao replicado de CreateMvpTownScene.CreateWalkInHouse (proibido editar aquele arquivo).
        private static void CreateFarmWalkInHouse()
        {
            const float cx = 28f;
            const float cy = 9f;
            const float w  = 7f;
            const float h  = 6f;
            float hw = w * 0.5f;
            float hh = h * 0.5f;
            // Porta ao sul: doorY = -hh.
            float doorY = -hh;

            var house = new GameObject("FarmHouse");
            house.transform.position = new Vector3(cx, cy, 0f);

            // Chao andavel (sem collider — jogador anda sobre ele).
            var floor = new GameObject("Floor");
            floor.transform.SetParent(house.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(w, h, 1f);
            var floorSr = floor.AddComponent<SpriteRenderer>();
            floorSr.sprite = GetBuiltinSprite();
            floorSr.color = new Color(0.78f, 0.72f, 0.62f); // piso madeira claro
            floorSr.sortingOrder = 5;
            TrySetSortingLayer(floorSr, "Ground", 5);

            // Paredes solidas (3 inteiras + vao da porta dividido em 2 laterais).
            CreateFarmInteriorWall(house.transform, "Wall_Left",  new Vector3(-hw, 0f, 0f), new Vector2(FarmWallThickness, h));
            CreateFarmInteriorWall(house.transform, "Wall_Right", new Vector3( hw, 0f, 0f), new Vector2(FarmWallThickness, h));
            // Parede oposta a porta (norte): inteira.
            CreateFarmInteriorWall(house.transform, "Wall_Top",   new Vector3(0f, -doorY, 0f), new Vector2(w, FarmWallThickness));
            // Parede do lado da porta (sul): dividida ao redor do vao.
            float sideWidth  = (w - FarmDoorGapWidth) * 0.5f;
            float sideCenter = (FarmDoorGapWidth + sideWidth) * 0.5f;
            CreateFarmInteriorWall(house.transform, "Wall_BottomL", new Vector3(-sideCenter, doorY, 0f), new Vector2(sideWidth, FarmWallThickness));
            CreateFarmInteriorWall(house.transform, "Wall_BottomR", new Vector3( sideCenter, doorY, 0f), new Vector2(sideWidth, FarmWallThickness));

            // Porta funcional no vao (HouseDoorInteractable — [E] abre/fecha).
            CreateFarmHouseDoor(house.transform, doorY);

            // Moveis DENTRO do interior (sem collider — andaveis).
            // Cama — canto NW do comodo (posicao relativa ao centro da casa em world).
            var bedObj = new GameObject("Bed");
            bedObj.transform.SetParent(house.transform);
            bedObj.transform.localPosition = new Vector3(-hw + 1.2f, hh - 1.1f, 0f);
            bedObj.transform.localScale = new Vector3(1.2f, 0.7f, 1f);
            var bedSr = bedObj.AddComponent<SpriteRenderer>();
            bedSr.sprite = GetBuiltinSprite();
            bedSr.color = new Color(0.55f, 0.30f, 0.45f);
            bedSr.sortingOrder = 0;
            bedSr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(bedSr, "World", 0);
            var bedCol = bedObj.AddComponent<BoxCollider2D>();
            bedCol.isTrigger = true;
            bedCol.size = Vector2.one;
            bedObj.AddComponent<CindarsHope.World.BedInteractable>();
            EditorUtility.SetDirty(bedObj);

            // BedLetter — ao lado da cama.
            var letterObj = new GameObject("BedLetter");
            letterObj.transform.SetParent(house.transform);
            letterObj.transform.localPosition = new Vector3(-hw + 2.6f, hh - 1.1f, 0f);
            letterObj.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
            var letterSr = letterObj.AddComponent<SpriteRenderer>();
            letterSr.sprite = GetBuiltinSprite();
            letterSr.color = new Color(0.95f, 0.92f, 0.78f);
            letterSr.sortingOrder = 0;
            letterSr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(letterSr, "World", 0);
            var letterCol = letterObj.AddComponent<BoxCollider2D>();
            letterCol.isTrigger = true;
            letterCol.size = Vector2.one;
            letterObj.AddComponent<CindarsHope.World.LetterInteractable>();
            EditorUtility.SetDirty(letterObj);

            // FarmHouseChest — canto NE do comodo.
            var chestObj = new GameObject("FarmHouseChest");
            chestObj.transform.SetParent(house.transform);
            chestObj.transform.localPosition = new Vector3(hw - 1.2f, hh - 1.1f, 0f);
            chestObj.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            var chestSr = chestObj.AddComponent<SpriteRenderer>();
            chestSr.sprite = GetBuiltinSprite();
            chestSr.color = new Color(0.52f, 0.38f, 0.18f);
            chestSr.sortingOrder = 0;
            chestSr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(chestSr, "World", 0);
            var chestCol = chestObj.AddComponent<BoxCollider2D>();
            chestCol.isTrigger = true;
            chestCol.size = Vector2.one;
            chestObj.AddComponent<CindarsHope.World.FarmHouseChestInteractable>();
            EditorUtility.SetDirty(chestObj);

            // Telhado: cobre todo o footprint + beiral; RoofRevealController some ao entrar.
            var roof = new GameObject("Roof");
            roof.transform.SetParent(house.transform);
            roof.transform.localPosition = Vector3.zero;
            roof.transform.localScale = new Vector3(w + 0.5f, h + 0.5f, 1f);
            var roofSr = roof.AddComponent<SpriteRenderer>();
            roofSr.sprite = GetBuiltinSprite();
            roofSr.color = new Color(0.42f, 0.20f, 0.14f); // vermelho-telha
            roofSr.sortingOrder = 20;
            TrySetSortingLayer(roofSr, "Roof", 20);

            // Cumeeira (estetica).
            var ridge = new GameObject("RoofRidge");
            ridge.transform.SetParent(house.transform);
            ridge.transform.localPosition = new Vector3(0f, hh * 0.45f, 0f);
            ridge.transform.localScale = new Vector3(w + 0.5f, 0.5f, 1f);
            var ridgeSr = ridge.AddComponent<SpriteRenderer>();
            ridgeSr.sprite = GetBuiltinSprite();
            ridgeSr.color = new Color(0.28f, 0.12f, 0.08f);
            ridgeSr.sortingOrder = 21;
            TrySetSortingLayer(ridgeSr, "Roof", 21);

            // Trigger de revelacao do telhado.
            var revealObj = new GameObject("RoofReveal");
            revealObj.transform.SetParent(house.transform);
            revealObj.transform.localPosition = Vector3.zero;
            var revealTrigger = revealObj.AddComponent<BoxCollider2D>();
            revealTrigger.isTrigger = true;
            revealTrigger.size = new Vector2(w, h);
            var reveal = revealObj.AddComponent<RoofRevealController>();
            reveal.Configure(new[] { roofSr, ridgeSr });
            EditorUtility.SetDirty(reveal);

            EditorUtility.SetDirty(house);
        }

        // Parede de interior (colisao solida) — replica CreateInteriorWall da cidade.
        private static void CreateFarmInteriorWall(Transform parent, string name, Vector3 localPos, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.localPosition = localPos;
            // spec_codex_13: parede solida entra no layer WorldSolid (obstacle avoidance de inimigo).
            CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                wall, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.WorldSolid);
            var col = wall.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = size;

            // Visual simples de pedra (nao tan elaborado quanto o da cidade — placeholder OK).
            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.50f, 0.46f, 0.40f); // cinza-pedra
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(sr, "World", 0);
            // Escala do visual = tamanho da parede.
            wall.transform.localScale = new Vector3(size.x, size.y, 1f);
            // O BoxCollider2D usa size local (size / localScale).
            col.size = Vector2.one; // normalizado — a escala do transform faz o resize
        }

        // Porta da casa de fazenda (HouseDoorInteractable — [E] abre). Layer World, order 0 — desempate
        // contra o shell de parede via epsilon de pivot em Y (nunca sortingOrder, ver contrato de
        // sorting da FASE 1 / CreateMvpTownScene.CreateHouseDoor).
        // doorY = posicao Y local da porta (negativo = sul, positivo = norte).
        private const float FarmDoorPivotEpsilon = 0.01f;

        private static void CreateFarmHouseDoor(Transform house, float doorY)
        {
            var door = new GameObject("Door");
            door.transform.SetParent(house);
            door.transform.localPosition = new Vector3(0f, doorY - FarmDoorPivotEpsilon, 0f);

            // Umbral escuro (abertura) — visivel quando a folha desliza.
            var threshold = new GameObject("Threshold");
            threshold.transform.SetParent(door.transform);
            threshold.transform.localPosition = Vector3.zero;
            threshold.transform.localScale = new Vector3(FarmDoorGapWidth, FarmWallThickness * 1.2f, 1f);
            var thrSr = threshold.AddComponent<SpriteRenderer>();
            thrSr.sprite = GetBuiltinSprite();
            thrSr.color = new Color(0.10f, 0.08f, 0.07f);
            thrSr.sortingOrder = 0;
            thrSr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(thrSr, "World", 0);

            // Folha de madeira (desliza ao abrir).
            var leaf = new GameObject("Leaf");
            leaf.transform.SetParent(door.transform);
            var closedLocalPos = Vector3.zero;
            var openLocalPos   = new Vector3(FarmDoorGapWidth * 0.92f, 0f, 0f);
            leaf.transform.localPosition = closedLocalPos;
            leaf.transform.localScale = new Vector3(FarmDoorGapWidth, FarmWallThickness * 2.0f, 1f);
            var leafSr = leaf.AddComponent<SpriteRenderer>();
            leafSr.sprite = GetBuiltinSprite();
            leafSr.color = new Color(0.38f, 0.24f, 0.14f); // madeira
            leafSr.sortingOrder = 0;
            leafSr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(leafSr, "World", 0);

            // Collider solido que tranca o vao (desligado quando aberta).
            var blocker = door.AddComponent<BoxCollider2D>();
            blocker.isTrigger = false;
            blocker.size = new Vector2(FarmDoorGapWidth, FarmWallThickness);

            // Trigger de interacao (sempre ligado).
            var interact = door.AddComponent<BoxCollider2D>();
            interact.isTrigger = true;
            interact.size = new Vector2(FarmDoorGapWidth + 0.6f, FarmWallThickness + 1.4f);

            var interactable = door.AddComponent<HouseDoorInteractable>();
            interactable.Configure(leaf.transform, blocker, closedLocalPos, openLocalPos);
            EditorUtility.SetDirty(interactable);
        }

        // F16: cama dentro da casa v4 — dormir voluntario com confirmacao.
        // NOTA: mantido como metodo legado (nao chamado na v5 — casa walk-in cria a cama internamente).
        private static void CreateFarmBed()
        {
            var bedObject = new GameObject("Bed");
            // v5: dentro da casa walk-in (centro 21,7 footprint 7x6 — spec_farm_scene_relayout_v4 §15.2 v5).
            // Posicao dentro do interior: canto NW do comodo.
            bedObject.transform.position = new Vector3(19f, 8.5f, 0f);
            bedObject.transform.localScale = new Vector3(1.2f, 0.7f, 1f);

            var spriteRenderer = bedObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.55f, 0.30f, 0.45f);
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Ground", spriteRenderer.sortingOrder);

            var collider = bedObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            bedObject.AddComponent<CindarsHope.World.BedInteractable>();
            EditorUtility.SetDirty(bedObject);
        }

        // fable_63: carta narrativa sobre a cama. Interactable padrao (LetterInteractable) que
        // mostra o texto da carta ("Procure Corvus na cidade") e persiste o estado de leitura.
        // v4: dentro da casa ao lado da cama.
        private static void CreateBedLetter()
        {
            var letterObject = new GameObject("BedLetter");
            // v5: dentro da casa walk-in ao lado da cama (spec_farm_scene_relayout_v4 §15.2 v5).
            letterObject.transform.position = new Vector3(20.5f, 8.5f, 0f);
            letterObject.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

            var spriteRenderer = letterObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.95f, 0.92f, 0.78f);
            spriteRenderer.sortingOrder = 3;
            TrySetSortingLayer(spriteRenderer, "Ground", spriteRenderer.sortingOrder);

            var collider = letterObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            letterObject.AddComponent<CindarsHope.World.LetterInteractable>();
            EditorUtility.SetDirty(letterObject);
        }

        // spec_farm_scene_relayout_v4: bau de armazenamento da casa. Dentro da casa walk-in (v5).
        // Posicao dentro do interior da casa (centro 21,7): canto leste do comodo.
        private static void CreateFarmHouseChest()
        {
            var chestObject = new GameObject("FarmHouseChest");
            chestObject.transform.position = new Vector3(23f, 8.5f, 0f);
            chestObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var spriteRenderer = chestObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.52f, 0.38f, 0.18f); // cor de bau (madeira escura)
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Ground", spriteRenderer.sortingOrder);

            var collider = chestObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            chestObject.AddComponent<CindarsHope.World.FarmHouseChestInteractable>();
            EditorUtility.SetDirty(chestObject);
        }

        // F15: liga a RainIrrigationIntegration (WAVE 02, antes órfã) ao registry real via runner.
        private static void CreateRainIrrigation(FarmPlotRegistry farmPlotRegistry)
        {
            var rainObject = new GameObject("RainIrrigation");
            var integration = rainObject.AddComponent<RainIrrigationIntegration>();
            var runner = rainObject.AddComponent<RainIrrigationRunner>();

            var serializedRunner = new SerializedObject(runner);
            SetReference(serializedRunner, "_integration", integration);
            SetReference(serializedRunner, "_plotRegistry", farmPlotRegistry);
            serializedRunner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(runner);
        }

        private static FarmPlotRegistry CreateFarmPlots(InventoryManager inventoryManager, StaminaManager staminaManager)
        {
            var seedDatabase = AssetDatabase.LoadAssetAtPath<SeedDatabaseSO>(SeedDatabasePath);
            if (seedDatabase == null)
            {
                Debug.LogWarning($"SeedDatabaseSO not found at {SeedDatabasePath}. Assign it manually on FarmPlot objects.");
            }

            var parent = new GameObject("FarmPlots");
            parent.transform.position = new Vector3(1f, -2.25f, 0f);
            var registry = parent.AddComponent<FarmPlotRegistry>();

            // spec_farm_scene_relayout_v4 (Desvio 1 corrigido 2026-06-26):
            // O campo aberto agora usa o tile system "arar qualquer terra" da Spec B.
            // Os 24 canteiros fixos (main 4x4 + east 2x4) foram removidos.
            // O registry existe vazio para que CreateRainIrrigation e FarmSectionProvider
            // possam referenciar o componente sem NullReferenceException.
            registry.Configure(new FarmPlot[0]);
            EditorUtility.SetDirty(registry);
            return registry;
        }

        private static FarmPlot CreateFarmPlot(Transform parent, int plotIndex, Vector3 position, InventoryManager inventoryManager, SeedDatabaseSO seedDatabase, StaminaManager staminaManager)
        {
            var plotObject = new GameObject($"FarmPlot_{plotIndex:00}");
            plotObject.transform.SetParent(parent);
            plotObject.transform.position = position;
            plotObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

            var spriteRenderer = plotObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.sortingOrder = 1;
            TrySetSortingLayer(spriteRenderer, "Ground", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"{plotObject.name} placeholder SpriteRenderer was created without a sprite. Replace it with plot art in a future art PR.");
            }

            var collider = plotObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var farmPlot = plotObject.AddComponent<FarmPlot>();
            var serializedPlot = new SerializedObject(farmPlot);
            SetReference(serializedPlot, "_spriteRenderer", spriteRenderer);
            SetReference(serializedPlot, "_inventoryManager", inventoryManager);
            SetReference(serializedPlot, "_seedDatabase", seedDatabase);
            SetReference(serializedPlot, "_staminaManager", staminaManager);
            serializedPlot.FindProperty("_temporarySequentialSliceMode").boolValue = plotIndex == 0;
            serializedPlot.FindProperty("_temporarySequentialSeedId").stringValue = "seed_carrot";
            serializedPlot.ApplyModifiedPropertiesWithoutUndo();

            farmPlot.Configure(plotIndex, inventoryManager, seedDatabase);
            farmPlot.ResetPlot();
            EditorUtility.SetDirty(farmPlot);
            return farmPlot;
        }

        private static TreeRegistry CreateTrees(InventoryManager inventoryManager)
        {
            var treeData = AssetDatabase.LoadAssetAtPath<TreeDataSO>(TreeDataPath);
            if (treeData == null)
            {
                Debug.LogWarning($"TreeDataSO not found at {TreeDataPath}. Assign it manually on TreeNode objects.");
            }

            var parent = new GameObject("Trees");
            parent.transform.position = Vector3.zero;
            var registry = parent.AddComponent<TreeRegistry>();

            // v7: bosque DENSO borda oeste (~40 arvores x[-32,-22] y[0,17]) + ~28 espalhadas no miolo = ~68 total.
            // (spec_farm_scene_relayout_v4 §15.5 v7 / §32). Sprites MAIORES via treeScaleBoost.
            // Bosque OESTE: regiao x in [-32,-22], y in [0,17]. Total bosque = 40 arvores.
            // Miolo espalhado: x in [-17,15], y in [-14,15], evitando lago/rio/construcoes. Total = 28.
            var trees = new TreeNode[68];
            // -- Bosque OESTE DENSO (40 arvores, grid irregular ~2 tiles de espaco) --
            // Faixa Y 0-2
            trees[0]  = CreateTree(parent.transform, 0,  new Vector3(-32f,  0.5f, 0f), treeData, inventoryManager);
            trees[1]  = CreateTree(parent.transform, 1,  new Vector3(-30f,  0.5f, 0f), treeData, inventoryManager);
            trees[2]  = CreateTree(parent.transform, 2,  new Vector3(-28f,  1.0f, 0f), treeData, inventoryManager);
            trees[3]  = CreateTree(parent.transform, 3,  new Vector3(-26f,  0.5f, 0f), treeData, inventoryManager);
            trees[4]  = CreateTree(parent.transform, 4,  new Vector3(-24f,  1.0f, 0f), treeData, inventoryManager);
            trees[5]  = CreateTree(parent.transform, 5,  new Vector3(-22f,  0.5f, 0f), treeData, inventoryManager);
            // Faixa Y 3-5
            trees[6]  = CreateTree(parent.transform, 6,  new Vector3(-32f,  3.5f, 0f), treeData, inventoryManager);
            trees[7]  = CreateTree(parent.transform, 7,  new Vector3(-30f,  3.0f, 0f), treeData, inventoryManager);
            trees[8]  = CreateTree(parent.transform, 8,  new Vector3(-28f,  3.5f, 0f), treeData, inventoryManager);
            trees[9]  = CreateTree(parent.transform, 9,  new Vector3(-26f,  3.0f, 0f), treeData, inventoryManager);
            trees[10] = CreateTree(parent.transform, 10, new Vector3(-24f,  3.5f, 0f), treeData, inventoryManager);
            trees[11] = CreateTree(parent.transform, 11, new Vector3(-22f,  3.0f, 0f), treeData, inventoryManager);
            // Faixa Y 6-8
            trees[12] = CreateTree(parent.transform, 12, new Vector3(-32f,  6.5f, 0f), treeData, inventoryManager);
            trees[13] = CreateTree(parent.transform, 13, new Vector3(-30f,  6.0f, 0f), treeData, inventoryManager);
            trees[14] = CreateTree(parent.transform, 14, new Vector3(-28f,  6.5f, 0f), treeData, inventoryManager);
            trees[15] = CreateTree(parent.transform, 15, new Vector3(-26f,  6.0f, 0f), treeData, inventoryManager);
            trees[16] = CreateTree(parent.transform, 16, new Vector3(-24f,  6.5f, 0f), treeData, inventoryManager);
            trees[17] = CreateTree(parent.transform, 17, new Vector3(-22f,  6.0f, 0f), treeData, inventoryManager);
            // Faixa Y 9-11
            trees[18] = CreateTree(parent.transform, 18, new Vector3(-32f,  9.5f, 0f), treeData, inventoryManager);
            trees[19] = CreateTree(parent.transform, 19, new Vector3(-30f,  9.0f, 0f), treeData, inventoryManager);
            trees[20] = CreateTree(parent.transform, 20, new Vector3(-28f,  9.5f, 0f), treeData, inventoryManager);
            trees[21] = CreateTree(parent.transform, 21, new Vector3(-26f,  9.0f, 0f), treeData, inventoryManager);
            trees[22] = CreateTree(parent.transform, 22, new Vector3(-24f,  9.5f, 0f), treeData, inventoryManager);
            trees[23] = CreateTree(parent.transform, 23, new Vector3(-22f,  9.0f, 0f), treeData, inventoryManager);
            // Faixa Y 12-14
            trees[24] = CreateTree(parent.transform, 24, new Vector3(-32f, 12.5f, 0f), treeData, inventoryManager);
            trees[25] = CreateTree(parent.transform, 25, new Vector3(-30f, 12.0f, 0f), treeData, inventoryManager);
            trees[26] = CreateTree(parent.transform, 26, new Vector3(-28f, 12.5f, 0f), treeData, inventoryManager);
            trees[27] = CreateTree(parent.transform, 27, new Vector3(-26f, 12.0f, 0f), treeData, inventoryManager);
            trees[28] = CreateTree(parent.transform, 28, new Vector3(-24f, 12.5f, 0f), treeData, inventoryManager);
            trees[29] = CreateTree(parent.transform, 29, new Vector3(-22f, 12.0f, 0f), treeData, inventoryManager);
            // Faixa Y 15-17
            trees[30] = CreateTree(parent.transform, 30, new Vector3(-32f, 15.5f, 0f), treeData, inventoryManager);
            trees[31] = CreateTree(parent.transform, 31, new Vector3(-30f, 15.0f, 0f), treeData, inventoryManager);
            trees[32] = CreateTree(parent.transform, 32, new Vector3(-28f, 15.5f, 0f), treeData, inventoryManager);
            trees[33] = CreateTree(parent.transform, 33, new Vector3(-26f, 15.0f, 0f), treeData, inventoryManager);
            trees[34] = CreateTree(parent.transform, 34, new Vector3(-24f, 15.5f, 0f), treeData, inventoryManager);
            trees[35] = CreateTree(parent.transform, 35, new Vector3(-22f, 15.0f, 0f), treeData, inventoryManager);
            // 4 extras para preencher o bosque (variacao de x/y para densificar)
            trees[36] = CreateTree(parent.transform, 36, new Vector3(-31f,  2.0f, 0f), treeData, inventoryManager);
            trees[37] = CreateTree(parent.transform, 37, new Vector3(-27f,  5.0f, 0f), treeData, inventoryManager);
            trees[38] = CreateTree(parent.transform, 38, new Vector3(-25f, 11.0f, 0f), treeData, inventoryManager);
            trees[39] = CreateTree(parent.transform, 39, new Vector3(-23f, 14.0f, 0f), treeData, inventoryManager);
            // -- Arvores ESPALHADAS no MIOLO (28 — limpáveis, harvestable) --
            // Miolo x in [-17,15], y in [-14,15]. Evitar: lago (x[5,31] y[-20,-6]), rio borda leste, montanha.
            trees[40] = CreateTree(parent.transform, 40, new Vector3(-17f,  1.0f, 0f), treeData, inventoryManager);
            trees[41] = CreateTree(parent.transform, 41, new Vector3(-14f,  3.0f, 0f), treeData, inventoryManager);
            trees[42] = CreateTree(parent.transform, 42, new Vector3(-11f,  6.0f, 0f), treeData, inventoryManager);
            trees[43] = CreateTree(parent.transform, 43, new Vector3( -8f,  2.0f, 0f), treeData, inventoryManager);
            trees[44] = CreateTree(parent.transform, 44, new Vector3( -5f,  8.0f, 0f), treeData, inventoryManager);
            trees[45] = CreateTree(parent.transform, 45, new Vector3( -2f,  4.0f, 0f), treeData, inventoryManager);
            trees[46] = CreateTree(parent.transform, 46, new Vector3(  1f, 11.0f, 0f), treeData, inventoryManager);
            trees[47] = CreateTree(parent.transform, 47, new Vector3(  5f,  6.0f, 0f), treeData, inventoryManager);
            trees[48] = CreateTree(parent.transform, 48, new Vector3(  9f, 13.0f, 0f), treeData, inventoryManager);
            trees[49] = CreateTree(parent.transform, 49, new Vector3( 13f,  8.0f, 0f), treeData, inventoryManager);
            trees[50] = CreateTree(parent.transform, 50, new Vector3(-15f, -1.0f, 0f), treeData, inventoryManager);
            trees[51] = CreateTree(parent.transform, 51, new Vector3(-12f, -4.0f, 0f), treeData, inventoryManager);
            trees[52] = CreateTree(parent.transform, 52, new Vector3( -9f, -7.0f, 0f), treeData, inventoryManager);
            trees[53] = CreateTree(parent.transform, 53, new Vector3( -6f,-11.0f, 0f), treeData, inventoryManager);
            trees[54] = CreateTree(parent.transform, 54, new Vector3( -3f, -3.0f, 0f), treeData, inventoryManager);
            trees[55] = CreateTree(parent.transform, 55, new Vector3(  0f, -8.0f, 0f), treeData, inventoryManager);
            trees[56] = CreateTree(parent.transform, 56, new Vector3(  4f,-13.0f, 0f), treeData, inventoryManager);
            trees[57] = CreateTree(parent.transform, 57, new Vector3( -7f, 14.0f, 0f), treeData, inventoryManager);
            trees[58] = CreateTree(parent.transform, 58, new Vector3( -4f, 10.0f, 0f), treeData, inventoryManager);
            trees[59] = CreateTree(parent.transform, 59, new Vector3( -1f,  7.0f, 0f), treeData, inventoryManager);
            trees[60] = CreateTree(parent.transform, 60, new Vector3(  3f,  2.0f, 0f), treeData, inventoryManager);
            trees[61] = CreateTree(parent.transform, 61, new Vector3(  7f, -5.0f, 0f), treeData, inventoryManager);
            trees[62] = CreateTree(parent.transform, 62, new Vector3(-13f,  9.0f, 0f), treeData, inventoryManager);
            trees[63] = CreateTree(parent.transform, 63, new Vector3(-16f, -6.0f, 0f), treeData, inventoryManager);
            trees[64] = CreateTree(parent.transform, 64, new Vector3( -6f,  0.0f, 0f), treeData, inventoryManager);
            trees[65] = CreateTree(parent.transform, 65, new Vector3(  2f, -5.0f, 0f), treeData, inventoryManager);
            trees[66] = CreateTree(parent.transform, 66, new Vector3( 10f,  5.0f, 0f), treeData, inventoryManager);
            trees[67] = CreateTree(parent.transform, 67, new Vector3( 15f, -2.0f, 0f), treeData, inventoryManager);

            registry.Configure(trees);
            EditorUtility.SetDirty(registry);

            // FASE 3 v7: debris no miolo — pedras e mato espalhados (limpáveis).
            // ~12 pedras (RockResource/node simples) e ~10 moitas/mato distribuidos no miolo central.
            // Nota: usam FarmResourceInteractable (Rock) e ForagePoint como placeholders visuais.
            CreateMioloPedras(parent, inventoryManager);
            CreateMioloMoitas(parent, inventoryManager);

            return registry;
        }

        // v7 FASE 3: ~12 pedras no miolo central (x[-17,15] y[-14,15]).
        // Usa FarmResourceInteractable tipo Rock como placeholder visual + coleta.
        private static void CreateMioloPedras(GameObject treesParent, InventoryManager inventoryManager)
        {
            var debrisParent = treesParent.transform.parent != null
                ? treesParent.transform.parent.gameObject
                : null;
            // Cria parent separado para pedras do miolo
            var pedraParent = new GameObject("MioloDebris_Pedras");
            pedraParent.transform.position = Vector3.zero;

            // 12 pedras no miolo (x[-17,15] y[-14,15]) — evitar lago/rio/construcoes/montanha
            var posPedras = new[]
            {
                new Vector3(-16f,  5.0f, 0f),
                new Vector3(-13f, -2.0f, 0f),
                new Vector3(-10f,  3.0f, 0f),
                new Vector3( -8f, 12.0f, 0f),
                new Vector3( -5f, -5.0f, 0f),
                new Vector3( -2f, -1.0f, 0f),
                new Vector3(  1f,  9.0f, 0f),
                new Vector3(  4f,  5.0f, 0f),
                new Vector3(  7f, -9.0f, 0f),
                new Vector3( 10f,  1.0f, 0f),
                new Vector3( 13f, -6.0f, 0f),
                new Vector3( -4f,  6.0f, 0f),
            };
            foreach (var pos in posPedras)
            {
                CreateRockResource(pedraParent.transform, inventoryManager, pos);
            }
        }

        // v7 FASE 3: ~10 moitas/mato no miolo central — placeholder visual usando ForagePoint.
        private static void CreateMioloMoitas(GameObject treesParent, InventoryManager inventoryManager)
        {
            var moitaParent = new GameObject("MioloDebris_Moitas");
            moitaParent.transform.position = Vector3.zero;

            // 10 moitas no miolo — IDs distintos dos forage da borda oeste (farm_forage_07..16)
            var posMoitas = new[]
            {
                new Vector3(-15f,  7.0f, 0f),
                new Vector3(-11f,  0.0f, 0f),
                new Vector3( -9f, -3.0f, 0f),
                new Vector3( -6f,  5.0f, 0f),
                new Vector3( -3f, 13.0f, 0f),
                new Vector3(  0f,  3.0f, 0f),
                new Vector3(  3f, -7.0f, 0f),
                new Vector3(  6f, 10.0f, 0f),
                new Vector3( 11f,  4.0f, 0f),
                new Vector3( 14f,-12.0f, 0f),
            };
            for (var i = 0; i < posMoitas.Length; i++)
            {
                // farm_forage_07..16 — IDs distintos dos 6 da borda oeste
                CreateForagePoint(moitaParent.transform, $"farm_forage_{(i + 7):00}", posMoitas[i]);
            }
        }

        // v5: boost visual das arvores do bosque NO (spec_farm_scene_relayout_v4 §15.3 item 5).
        // Multiplica o TreeScale por este fator para que o bosque fique visivelmente mais denso.
        private const float TreeScaleBoostV5 = 1.35f;

        private static TreeNode CreateTree(
            Transform parent,
            int treeIndex,
            Vector3 position,
            TreeDataSO treeData,
            InventoryManager inventoryManager)
        {
            var treeObject = new GameObject($"TreeNode_{treeIndex:00}");
            treeObject.transform.SetParent(parent);
            treeObject.transform.position = position;
            var treeScaleConfig = AssetDatabase.LoadAssetAtPath<GameScaleConfigSO>(GameScaleConfigPath);
            var baseTreeScale = treeScaleConfig != null ? treeScaleConfig.TreeScale : 3f;
            // v5: sprites de arvore MAIORES para o bosque denso NO (boost 1.35x sobre TreeScale).
            var treeScale = baseTreeScale * TreeScaleBoostV5;
            treeObject.transform.localScale = new Vector3(treeScale, treeScale, 1f);

            var spriteRenderer = treeObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.24f, 0.48f, 0.22f);
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(spriteRenderer, "World", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"{treeObject.name} placeholder SpriteRenderer was created without a sprite. Replace it with tree art in a future art PR.");
            }

            var collider = treeObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
            FitBoxColliderToOpaqueSprite(collider, spriteRenderer);

            var treeNode = treeObject.AddComponent<TreeNode>();
            var serializedTree = new SerializedObject(treeNode);
            SetReference(serializedTree, "_treeData", treeData);
            SetReference(serializedTree, "_inventoryManager", inventoryManager);
            SetReference(serializedTree, "_spriteRenderer", spriteRenderer);
            serializedTree.ApplyModifiedPropertiesWithoutUndo();

            treeNode.Configure(treeIndex, treeData, inventoryManager, spriteRenderer);
            EditorUtility.SetDirty(treeNode);
            return treeNode;
        }

        private static void FitBoxColliderToOpaqueSprite(BoxCollider2D collider, SpriteRenderer spriteRenderer)
        {
            if (collider == null || spriteRenderer == null || spriteRenderer.sprite == null)
            {
                Debug.LogWarning("FitBoxColliderToOpaqueSprite: cannot fit collider — collider, renderer, or sprite is null.");
                return;
            }

            if (!SpriteOpaqueBoundsUtility.TryGetOpaqueLocalBounds(spriteRenderer.sprite, 0.05f, out var localBounds))
            {
                Debug.LogWarning($"FitBoxColliderToOpaqueSprite: could not determine bounds for '{spriteRenderer.sprite.name}'.");
                return;
            }

            collider.size   = new Vector2(localBounds.size.x, localBounds.size.y);
            collider.offset = new Vector2(localBounds.center.x, localBounds.center.y);
        }

        private static void FitLakeBlockingCollider(BoxCollider2D collider, SpriteRenderer spriteRenderer)
        {
            const float lakeColliderScale = 0.92f;
            if (collider == null || spriteRenderer == null || spriteRenderer.sprite == null)
            {
                collider.size = new Vector2(0.92f, 0.92f);
                return;
            }

            var sprite = spriteRenderer.sprite;
            var spriteSize = sprite.rect.size / sprite.pixelsPerUnit;
            var fitSize = spriteSize * lakeColliderScale;
            collider.size = new Vector2(fitSize.x, fitSize.y);
        }

        private static void CreateBounds()
        {
            var bounds = new GameObject("Bounds");
            bounds.transform.position = Vector3.zero;

            // v6: Farm bounds 64x44 (origem centrada: x in [-32,32], y in [-22,22]).
            // A montanha ao norte (faixa y in [18,22]) ja bloqueia; estes bounds sao as bordas externas.
            CreateBound("Top", bounds.transform, new Vector2(0f, 22f), new Vector2(64f, 1f));
            CreateBound("Bottom", bounds.transform, new Vector2(0f, -22f), new Vector2(64f, 1f));
            CreateBound("Left", bounds.transform, new Vector2(-32f, 0f), new Vector2(1f, 44f));
            CreateBound("Right", bounds.transform, new Vector2(32f, 0f), new Vector2(1f, 44f));
        }

        private static void CreateBound(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var bound = new GameObject(name);
            bound.transform.SetParent(parent);
            bound.transform.position = position;

            var collider = bound.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static void CreateMainCamera(Transform playerTransform)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.5f; // calibrate in Play Mode with CameraScaleConfigSO
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.627451f, 0.5772549f, 0.4329412f);

            var cameraFollow = cameraObject.AddComponent<CindarsHope.Camera.CameraFollow2D>();
            var serializedFollow = new SerializedObject(cameraFollow);
            SetReference(serializedFollow, "_target", playerTransform);
            serializedFollow.FindProperty("_snapOnStart").boolValue = true;
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cameraFollow);
        }

        private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Serialized field '{propertyName}' was not found on '{serializedObject.targetObject.name}'.");
                return;
            }

            property.objectReferenceValue = value;
        }

        // fable_12 — abrigos de animais na Zone_Construction (-1.0, 9.0; 6.0×3.0). Coop_01 (galinhas,
        // cap. 4) e Barn_01 (cabra/vaca, cap. 4). Cada abrigo tem visual composto (corpo+telhado+porta)
        // e uma porta interativa (AnimalReleaseHandler) com cercado (bounds de wander). O registry é o
        // singleton de runtime (FarmAnimalRuntimeBootstrap) — o handler resolve via Instance; o
        // InventoryManager é wired por referência (sem GameObject.Find).
        private static void CreateAnimalHousings(InventoryManager inventoryManager)
        {
            var parent = new GameObject("FarmAnimalHousings");
            parent.transform.position = Vector3.zero;

            // v7: construcoes na BORDA SUL (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coords 64x44: Coop_01 (-22,-19) 6x3, Barn_01 (-13,-19) 7x4.
            CreateAnimalHousing(
                parent.transform, inventoryManager,
                "Coop_01", "coop_01", AnimalHousingBuildingType.Coop, 4,
                new Vector3(-22f, -19f, 0f), new Color(0.78f, 0.66f, 0.42f));

            CreateAnimalHousing(
                parent.transform, inventoryManager,
                "Barn_01", "barn_01", AnimalHousingBuildingType.Barn, 4,
                new Vector3(-13f, -19f, 0f), new Color(0.62f, 0.34f, 0.28f));
        }

        private static void CreateAnimalHousing(
            Transform parent,
            InventoryManager inventoryManager,
            string objectName,
            string housingId,
            AnimalHousingBuildingType housingType,
            int capacity,
            Vector3 position,
            Color bodyColor)
        {
            var root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            // Corpo.
            var body = new GameObject("Body");
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(2.0f, 1.4f, 1f);
            var bodySr = body.AddComponent<SpriteRenderer>();
            bodySr.sprite = GetBuiltinSprite();
            bodySr.color = bodyColor;
            bodySr.sortingOrder = 0;
            bodySr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(bodySr, "World", 0);

            // Telhado.
            var roof = new GameObject("Roof");
            roof.transform.SetParent(root.transform);
            roof.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            roof.transform.localScale = new Vector3(2.2f, 0.5f, 1f);
            var roofSr = roof.AddComponent<SpriteRenderer>();
            roofSr.sprite = GetBuiltinSprite();
            roofSr.color = new Color(bodyColor.r * 0.6f, bodyColor.g * 0.6f, bodyColor.b * 0.6f);
            roofSr.sortingOrder = 3;
            TrySetSortingLayer(roofSr, "Roof", 3);

            // Porta (interativa — solta animal).
            var door = new GameObject("Door");
            door.transform.SetParent(root.transform);
            door.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            door.transform.localScale = new Vector3(0.6f, 0.8f, 1f);
            var doorSr = door.AddComponent<SpriteRenderer>();
            doorSr.sprite = GetBuiltinSprite();
            doorSr.color = new Color(0.3f, 0.2f, 0.15f);
            doorSr.sortingOrder = 0;
            doorSr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(doorSr, "World", 0);

            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.2f, 1.8f);

            var releaseHandler = root.AddComponent<AnimalReleaseHandler>();
            releaseHandler.Configure(housingId, housingType, capacity, null, inventoryManager);
        }

        // fable_55: 2 estações de processamento físicas (queijaria/barril) + estufa mínima com 4
        // canteiros. Superfície de job única = FarmProcessingStationService (por dias). Refs
        // serializadas; sem GameObject.Find. Estufa = zona pequena sempre presente (decisão Fase 0).
        private static void CreateProcessingAndGreenhouse(InventoryManager inventoryManager, StaminaManager staminaManager)
        {
            var root = new GameObject("FarmProcessing");
            root.transform.position = Vector3.zero;

            // Serviço dono dos jobs (DontDestroyOnLoad via singleton em runtime).
            var serviceObject = new GameObject("FarmProcessingStationService");
            serviceObject.transform.SetParent(root.transform);
            var service = serviceObject.AddComponent<CindarsHope.Farm.Processing.FarmProcessingStationService>();
            var serializedService = new SerializedObject(service);
            serializedService.FindProperty("_inventoryManager").objectReferenceValue = inventoryManager;
            serializedService.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(service);

            // v7: Queijaria e Barril de Vinho na BORDA SUL (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coords 64x44: CheesePress (-28,-19), WineBarrel (-8,-19).
            CreateProcessingStation(
                root.transform, service,
                "Station_CheesePress", "Queijaria",
                CindarsHope.Farm.Processing.ProcessingRecipeCatalog.StationCheesePressId,
                new Vector3(-28f, -19f, 0f), new Color(0.92f, 0.86f, 0.55f));

            // Estacao 2 — Barril de Vinho.
            CreateProcessingStation(
                root.transform, service,
                "Station_WineBarrel", "Barril de Vinho",
                CindarsHope.Farm.Processing.ProcessingRecipeCatalog.StationWineBarrelId,
                new Vector3(-8f, -19f, 0f), new Color(0.55f, 0.18f, 0.22f));

            CreateGreenhouse(root.transform, inventoryManager, staminaManager);
        }

        private static void CreateProcessingStation(
            Transform parent,
            CindarsHope.Farm.Processing.FarmProcessingStationService service,
            string objectName,
            string displayName,
            string stationId,
            Vector3 position,
            Color bodyColor)
        {
            var obj = new GameObject(objectName);
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = bodyColor;
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(sr, "World", 0);

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.4f, 1.4f);

            var interactable = obj.AddComponent<CindarsHope.Farm.Processing.ProcessingStationInteractable>();
            var serializedInteractable = new SerializedObject(interactable);
            serializedInteractable.FindProperty("_stationId").stringValue = stationId;
            serializedInteractable.FindProperty("_service").objectReferenceValue = service;
            serializedInteractable.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactable);

            if (sr.sprite == null)
            {
                Debug.LogWarning($"{objectName} ({displayName}) placeholder SpriteRenderer created without sprite. Replace with station art in a future art PR.");
            }
        }

        // Estufa mínima: zona pequena sempre presente com 4 canteiros FarmPlot registrados no
        // GreenhouseRuntimeHost (instancia o GreenhouseContextProvider órfão). Os canteiros usam
        // índices 200+ (não colidem com os iniciais 0..23 nem com os lotes 100+).
        private static void CreateGreenhouse(Transform parent, InventoryManager inventoryManager, StaminaManager staminaManager)
        {
            var seedDatabase = AssetDatabase.LoadAssetAtPath<SeedDatabaseSO>(SeedDatabasePath);

            var greenhouseRoot = new GameObject("Greenhouse");
            greenhouseRoot.transform.SetParent(parent);
            // v7: estufa homestead leste (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: centro (24, 10).
            greenhouseRoot.transform.position = new Vector3(24f, 10f, 0f);

            // Piso/estrutura da estufa (visual placeholder translúcido).
            var floor = new GameObject("GreenhouseFloor");
            floor.transform.SetParent(greenhouseRoot.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(5f, 4f, 1f);
            var floorSr = floor.AddComponent<SpriteRenderer>();
            floorSr.sprite = GetBuiltinSprite();
            floorSr.color = new Color(0.70f, 0.90f, 0.80f, 0.45f);
            floorSr.sortingOrder = 0;
            TrySetSortingLayer(floorSr, "Ground", floorSr.sortingOrder);

            const int greenhouseBaseIndex = 200;
            var plotIds = new string[4];
            const float spacing = 1.2f;
            var origin = greenhouseRoot.transform.position + new Vector3(-0.9f, 0.8f, 0f);
            for (var i = 0; i < 4; i++)
            {
                var x = i % 2;
                var y = i / 2;
                var plotIndex = greenhouseBaseIndex + i;
                var pos = origin + new Vector3(x * spacing, -y * spacing, 0f);
                var plot = CreateFarmPlot(greenhouseRoot.transform, plotIndex, pos, inventoryManager, seedDatabase, staminaManager);
                plotIds[i] = plot.PlotId; // "plot_200".."plot_203"
            }

            // Host instancia o GreenhouseContextProvider e registra os 4 canteiros (refs do gerador).
            var hostObject = new GameObject("GreenhouseRuntimeHost");
            hostObject.transform.SetParent(greenhouseRoot.transform);
            var host = hostObject.AddComponent<CindarsHope.Farm.Watering.GreenhouseRuntimeHost>();
            var serializedHost = new SerializedObject(host);
            var idsProperty = serializedHost.FindProperty("_greenhousePlotIds");
            idsProperty.arraySize = plotIds.Length;
            for (var i = 0; i < plotIds.Length; i++)
            {
                idsProperty.GetArrayElementAtIndex(i).stringValue = plotIds[i];
            }
            serializedHost.FindProperty("_unlockedByDefault").boolValue = true;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
        }

        private static Sprite GetBuiltinSprite()
        {
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BuiltinSpritePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Builtin placeholder sprite not found at '{BuiltinSpritePath}'. Player and Ground will need a sprite assigned manually or by a future art PR.");
            }

            return sprite;
        }

        private static void TrySetSortingLayer(SpriteRenderer renderer, string layerName, int fallbackOrder)
            => SceneSortingLayerHelper.TrySetSortingLayer(renderer, layerName, fallbackOrder);

        private static void CreateSceneRuntimeInstaller(
            FarmPlotRegistry farmPlotRegistry,
            TreeRegistry treeRegistry,
            ItemPickupRegistry itemPickupRegistry,
            Transform playerTransform,
            GameBootstrap bootstrap)
        {
            var installerObject = new GameObject("SceneRuntimeReferences");
            var installer = installerObject.AddComponent<FarmSceneRuntimeReferenceInstaller>();

            var serializedInstaller = new SerializedObject(installer);

            var farmPlots = Object.FindObjectsByType<FarmPlot>();
            var farmPlotsProperty = serializedInstaller.FindProperty("_farmPlots");
            if (farmPlotsProperty != null)
            {
                farmPlotsProperty.arraySize = farmPlots.Length;
                for (int i = 0; i < farmPlots.Length; i++)
                {
                    farmPlotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = farmPlots[i];
                }
            }

            var treeNodes = Object.FindObjectsByType<TreeNode>();
            var treeNodesProperty = serializedInstaller.FindProperty("_treeNodes");
            if (treeNodesProperty != null)
            {
                treeNodesProperty.arraySize = treeNodes.Length;
                for (int i = 0; i < treeNodes.Length; i++)
                {
                    treeNodesProperty.GetArrayElementAtIndex(i).objectReferenceValue = treeNodes[i];
                }
            }

            var fishingSpots = Object.FindObjectsByType<FishingSpot>();
            var fishingSpot = fishingSpots.Length > 0 ? fishingSpots[0] : null;
            SetReference(serializedInstaller, "_fishingSpot", fishingSpot);

            SetReference(serializedInstaller, "_seedShopPoint", null);
            SetReference(serializedInstaller, "_sellAllPoint", null);

            var craftingPoints = Object.FindObjectsByType<CraftingPoint>();
            var craftingPoint = craftingPoints.Length > 0 ? craftingPoints[0] : null;
            SetReference(serializedInstaller, "_craftingPoint", craftingPoint);

            SetReference(serializedInstaller, "_farmPlotRegistry", farmPlotRegistry);
            SetReference(serializedInstaller, "_treeRegistry", treeRegistry);
            SetReference(serializedInstaller, "_itemPickupRegistry", itemPickupRegistry);
            SetReference(serializedInstaller, "_playerTransform", playerTransform);

            serializedInstaller.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
        }

        // WAVE_INTEGRATION_06: Farm resource interactables (Tree, Rock).
        // LakeFishing uses existing FishingSpot at (7.8, -2.8) which already implements IInteractable.
        // Tree/Rock adapters remain TODO_INTEGRATION_NOT_FINAL — for smoke validation only.
        // fable_54: Forage is now FINAL — CreateForagePoints wires ForagePointInteractable to
        // FarmForageRuntimeService (seasonal forage). Tree/Rock final wiring is still pending.
        // ─── Cave entrance (WAVE16 wiring, now automated by the generator) ───
        // Visible cave mouth on Zone_CaveEntrance with a CaveEntranceInteractable that
        // routes FarmScene → CaveScene via SceneTransitionRouter. Closes DEBT-SCENE
        // "CaveEntranceInteractable not placed" from the WAVE22 backlog.
        private static void CreateCaveEntrance()
        {
            var entrance = new GameObject("CaveEntrance");
            // v7: canto NO da montanha embutida — MAIOR ~5x4 (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: (-28, 18.5).
            entrance.transform.position = new Vector3(-28f, 18.5f, 0f);

            // Cave mouth visual: dark arch over a rock frame — MAIOR (5x4 footprint).
            var rockFrame = new GameObject("RockFrame");
            rockFrame.transform.SetParent(entrance.transform);
            rockFrame.transform.localPosition = Vector3.zero;
            rockFrame.transform.localScale = new Vector3(5f, 4f, 1f); // v6: boca proeminente ~5x4
            var rockRenderer = rockFrame.AddComponent<SpriteRenderer>();
            rockRenderer.sprite = GetBuiltinSprite();
            rockRenderer.color = new Color(0.36f, 0.33f, 0.3f);
            rockRenderer.sortingOrder = 0;
            rockRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(rockRenderer, "World", rockRenderer.sortingOrder);

            var mouth = new GameObject("CaveMouth");
            mouth.transform.SetParent(entrance.transform);
            mouth.transform.localPosition = new Vector3(0f, -0.3f - FarmDoorPivotEpsilon, 0f);
            mouth.transform.localScale = new Vector3(3.5f, 2.8f, 1f); // v6: interior escuro maior
            var mouthRenderer = mouth.AddComponent<SpriteRenderer>();
            mouthRenderer.sprite = GetBuiltinSprite();
            mouthRenderer.color = new Color(0.08f, 0.06f, 0.1f);
            mouthRenderer.sortingOrder = 0;
            mouthRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(mouthRenderer, "World", mouthRenderer.sortingOrder);

            var trigger = entrance.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(4f, 3.5f); // v6: trigger proporcional a boca maior

            var interactable = entrance.AddComponent<CaveEntranceInteractable>();
            var serializedInteractable = new SerializedObject(interactable);
            serializedInteractable.FindProperty("_targetSpawnId").stringValue = "cave_from_farm";
            serializedInteractable.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactable);
        }

        // fable_51: Zrix's cave-contract board, mounted at the cave mouth. It is ANOTHER ACCESS
        // POINT with the CaveContract source — NOT a second quest system. It reuses the existing
        // QuestBoardInteractable, posting the 8 canonical cc_* ids (6 milestones + 2 weeklies).
        // The 1x milestones are offered/rotated by CaveContractService (QuestRuntimeBootstrap);
        // this board just lets the player accept/turn them in via the existing F34 quest flow.
        private static void CreateZrixContractBoard()
        {
            var board = new GameObject("Board_Zrix");
            // v5: mural de contratos perto da caverna NO (spec_farm_scene_relayout_v4 §15.2 v5).
            // Coord 56x40: (-21, 15).
            // v7: mural de contratos perto da caverna NO (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: (-22, 16) — junto a entrada da caverna a (-28,18.5).
            board.transform.position = new Vector3(-22f, 16f, 0f);

            var sign = board.AddComponent<SpriteRenderer>();
            sign.sprite = GetBuiltinSprite();
            sign.color = new Color(0.30f, 0.22f, 0.42f); // draconato-purple board
            sign.sortingOrder = 2;
            TrySetSortingLayer(sign, "Roof", sign.sortingOrder);
            if (!ScaleProfileLibrary.AttachApplicator(board, EntityScaleCategory.ContractBoard))
            {
                board.transform.localScale = new Vector3(0.9f, 1.1f, 1f);
            }

            var trigger = board.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.2f, 1.6f);

            var postedIds = new List<string>();
            foreach (var depth in CindarsHope.Quests.CaveContracts.CaveContractCatalog.MilestoneDepths)
            {
                postedIds.Add(CindarsHope.Quests.CaveContracts.CaveContractCatalog.MilestoneId(depth));
            }
            // Week 0 weekly instance ids (the board re-resolves whatever is active at interact time).
            postedIds.Add(CindarsHope.Quests.CaveContracts.CaveContractCatalog.WeeklyInstanceId(
                CindarsHope.Quests.CaveContracts.CaveContractCatalog.BossRematchId, 0));
            postedIds.Add(CindarsHope.Quests.CaveContracts.CaveContractCatalog.WeeklyInstanceId(
                CindarsHope.Quests.CaveContracts.CaveContractCatalog.NoHitFloorId, 0));

            var interactable = board.AddComponent<CindarsHope.Quests.Runtime.QuestBoardInteractable>();
            var so = new SerializedObject(interactable);
            so.FindProperty("_boardId").stringValue = "board_zrix_cave_contracts";
            so.FindProperty("_interactionPrompt").stringValue =
                CindarsHope.Localization.LocalizationService.Get("interact.zrix_board.prompt");
            var arr = so.FindProperty("_postedQuestIds");
            arr.arraySize = postedIds.Count;
            for (int i = 0; i < postedIds.Count; i++)
            {
                arr.GetArrayElementAtIndex(i).stringValue = postedIds[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactable);
        }

        // ── Fase 5: Zrix perambulando no bosque NO ────────────────────────────────────────────────
        // Replica o minimo de CreateDialogueNpc da cidade: NpcController + Rigidbody2D + NpcWanderer
        // + BoxCollider2D trigger. Nao precisa de DialogueModal (o mural Board_Zrix ja e o canal
        // de contratos; Zrix so perambula na fazenda). Wiring: NpcDataSO Npc_Zrix.asset.
        // ConfigureMovement(1.0f, 4f, 2.5f, 5.5f, clampMin(-26,4), clampMax(-10,16)).
        private const string ZrixNpcDataPath = "Assets/_Game/Data/NPCs/Npc_Zrix.asset";

        private static void CreateFarmZrixNpc()
        {
            var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(ZrixNpcDataPath);
            if (npcData == null)
            {
                Debug.LogWarning($"[CreateMvpFarmScene] NpcDataSO de Zrix nao encontrado em {ZrixNpcDataPath}. " +
                                 "Zrix nao sera criado na FarmScene. Rode 'Inicializar Projeto' apos criar o asset.");
                return;
            }

            var npcObject = new GameObject("NPC_Zrix_Farm");
            // v7: borda oeste, bosque denso (spec_farm_scene_relayout_v4 §15.5 v7).
            npcObject.transform.position = new Vector3(-26f, 8f, 0f);
            if (!ScaleProfileLibrary.AttachApplicator(npcObject, EntityScaleCategory.NPC))
            {
                npcObject.transform.localScale = new Vector3(1f, 1.5f, 1f);
            }

            var renderer = npcObject.AddComponent<SpriteRenderer>();
            // Usa o BodySprite real do Zrix (mesmo padrao dos NPCs de dialogo da cidade); so cai no
            // placeholder azul-ardosia se AssignNpcBodySprites nao tiver setado o sprite. Sprite real sem tint.
            var hasBodySprite = npcData.BodySprite != null;
            renderer.sprite = hasBodySprite ? npcData.BodySprite : GetBuiltinSprite();
            renderer.color = hasBodySprite ? Color.white : new Color(0.43f, 0.52f, 0.68f);
            renderer.sortingOrder = 0;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(renderer, "World", renderer.sortingOrder);

            // Trigger de interacao (1x1) — sem DialogueModal na fazenda; interacao nao abre dialogo.
            var trigger = npcObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = Vector2.one;

            // Rigidbody2D necessario para o NpcWanderer se mover.
            var body = npcObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.linearDamping = 4f;

            // Collider solido nos pes (filho separado para nao colidir com o trigger 1x1).
            var solidBody = new GameObject("SolidBody");
            solidBody.transform.SetParent(npcObject.transform);
            solidBody.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            solidBody.transform.localScale = Vector3.one;
            var solidCol = solidBody.AddComponent<BoxCollider2D>();
            solidCol.isTrigger = false;
            solidCol.size = new Vector2(0.55f, 0.4f);

            // NpcController: wiring minimo (sem DialogueModal — Zrix so perambula na fazenda).
            var controller = npcObject.AddComponent<NpcController>();
            var serializedController = new SerializedObject(controller);
            SetReference(serializedController, "_npcData", npcData);
            // _dialogueModal e _modalManager ficam null — interacao logara aviso mas nao crashara.
            SetReference(serializedController, "_collider", trigger);
            SetReference(serializedController, "_spriteRenderer", renderer);

            // NpcWanderer: perambulacao lenta no bosque NO.
            var wanderer = npcObject.AddComponent<NpcWanderer>();
            var serializedWanderer = new SerializedObject(wanderer);
            SetReference(serializedWanderer, "_npcData", npcData);
            SetReference(serializedWanderer, "_rigidbody", body);
            serializedWanderer.ApplyModifiedPropertiesWithoutUndo();

            // NpcWalkAnimator: anima a caminhada (5x5) IGUAL aos NPCs da cidade. Sem ele o Zrix
            // perambula mas fica no frame parado (bug: CreateFarmZrixNpc replicava "o minimo da cidade"
            // mas esquecia justamente o animator). Le NpcDataSO.WalkAnimResourcesPath em runtime.
            var walkAnimator = npcObject.AddComponent<CindarsHope.NPC.NpcWalkAnimator>();
            var serializedWalkAnimator = new SerializedObject(walkAnimator);
            SetReference(serializedWalkAnimator, "_npcData", npcData);
            serializedWalkAnimator.ApplyModifiedPropertiesWithoutUndo();

            // Fade-in de aparição: o Zrix nasce longe (bosque NO) e "aparece" quando o player chega
            // perto. NpcAppearFade materializa suave (alpha 0→1) na 1a vez que entra na câmera.
            npcObject.AddComponent<CindarsHope.NPC.NpcAppearFade>();

            // ConfigureMovement(speed, radius, pauseMin, pauseMax, clampMin, clampMax)
            // Especificacao: speed=1.0, radius=4.0, pauseMin=2.5, pauseMax=5.5, clamp(-26,4)(-10,16)
            wanderer.ConfigureMovement(1.0f, 4f, 2.5f, 5.5f,
                new Vector2(-26f, 4f), new Vector2(-10f, 16f));

            SetReference(serializedController, "_wanderer", wanderer);
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(wanderer);
        }

        // ── Fase 6 (v7): 3 areas de expansao reservadas nas BORDAS ─────────────────────────────
        // Placeholder visual (borda escura + overlay semi-transparente) para as 3 regioes futuras.
        // v7 (spec_farm_scene_relayout_v4 §15.5): Exp_North, Exp_NE, Exp_West nas BORDAS — fora do miolo.
        // Nao-araveis (registradas no FarmSceneRuntimeBootstrap).
        private static void CreateExpansionAreaPlaceholders()
        {
            var parent = new GameObject("ExpansionAreaPlaceholders");
            parent.transform.position = Vector3.zero;

            // Exp_North: borda norte, centro (-2, 16.5), 8x4
            CreateExpansionPlaceholder(parent.transform, "Exp_North",
                new Vector3(-2f, 16.5f, 0f), new Vector2(8f, 4f));
            // Exp_NE: borda norte leste, centro (16, 16.5), 7x4
            CreateExpansionPlaceholder(parent.transform, "Exp_NE",
                new Vector3(16f, 16.5f, 0f), new Vector2(7f, 4f));
            // Exp_West: borda oeste sul, centro (-30, -10), 5x8
            CreateExpansionPlaceholder(parent.transform, "Exp_West",
                new Vector3(-30f, -10f, 0f), new Vector2(5f, 8f));
        }

        // Cria uma area de expansao reservada: overlay semi-transparente + borda escura.
        // Sem collider fisico (apenas visual de gizmo/placeholder em editor mode).
        private static void CreateExpansionPlaceholder(Transform parent, string name, Vector3 center, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = center;

            // Overlay semi-transparente (hachura) — cinza-roxo simbolizando area bloqueada/futura.
            var overlay = new GameObject("Overlay");
            overlay.transform.SetParent(obj.transform);
            overlay.transform.localPosition = Vector3.zero;
            overlay.transform.localScale = new Vector3(size.x, size.y, 1f);
            var overlaySr = overlay.AddComponent<SpriteRenderer>();
            overlaySr.sprite = GetBuiltinSprite();
            overlaySr.color = new Color(0.35f, 0.28f, 0.50f, 0.22f); // roxo-escuro semi-transparente
            overlaySr.sortingOrder = 0;
            TrySetSortingLayer(overlaySr, "Ground", 0);

            // Borda N (topo)
            CreateExpansionBorder(obj.transform, "Border_N",
                new Vector3(0f,  size.y * 0.5f, 0f), new Vector3(size.x, 0.15f, 1f));
            // Borda S (base)
            CreateExpansionBorder(obj.transform, "Border_S",
                new Vector3(0f, -size.y * 0.5f, 0f), new Vector3(size.x, 0.15f, 1f));
            // Borda L (leste)
            CreateExpansionBorder(obj.transform, "Border_E",
                new Vector3(size.x * 0.5f, 0f, 0f), new Vector3(0.15f, size.y, 1f));
            // Borda O (oeste)
            CreateExpansionBorder(obj.transform, "Border_W",
                new Vector3(-size.x * 0.5f, 0f, 0f), new Vector3(0.15f, size.y, 1f));

            // Trigger de deteccao (para futuro desbloqueio via Quadro de Evolucoes).
            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;
        }

        private static void CreateExpansionBorder(Transform parent, string name, Vector3 localPos, Vector3 localScale)
        {
            var border = new GameObject(name);
            border.transform.SetParent(parent);
            border.transform.localPosition = localPos;
            border.transform.localScale = localScale;
            var sr = border.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.20f, 0.14f, 0.35f, 0.75f); // roxo-escuro para borda
            sr.sortingOrder = 0;
            TrySetSortingLayer(sr, "Ground", 0);
        }

        private static void CreateFarmResourceInteractables(InventoryManager inventoryManager)
        {
            var parent = new GameObject("FarmResourceInteractables");
            parent.transform.position = Vector3.zero;

            // v7: RockResource_01 na borda oeste (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: (-31, 6).
            CreateRockResource(parent.transform, inventoryManager, new Vector3(-31f, 6f, 0f));
            // v7: TreeResource_01 absorvido pelo bosque oeste (posicionado dentro do cluster).
            CreateTreeResource(parent.transform, inventoryManager, new Vector3(-28f, 5f, 0f));
            // LakeFishing: FishingSpot at (7.8, -2.8) already implements IInteractable (prompt: "Pescar")
            // No additional FarmResourceInteractable needed for the lake.
        }

        // fable_54: pontos reais de forrageio sazonal na Zone_Forage (-8,-2). IDs estaveis
        // farm_forage_01..06; o FarmForageRuntimeService (bootstrap) seleciona deterministicamente
        // quais ficam ativos por dia/estacao e (re)spawna por politica. Refs serializadas; SEM Find.
        private static void CreateForagePoints()
        {
            var parent = new GameObject("FarmForagePoints");
            parent.transform.position = Vector3.zero;

            // v7: cluster borda oeste (spec_farm_scene_relayout_v4 §15.5 v7). IDs mantidos (farm_forage_01..06).
            // Coords 64x44: (-31,-4)(-29,-4)(-31,-6)(-29,-6)(-31,-2)(-29,-2).
            var positions = new[]
            {
                new Vector3(-31f, -4f, 0f),
                new Vector3(-29f, -4f, 0f),
                new Vector3(-31f, -6f, 0f),
                new Vector3(-29f, -6f, 0f),
                new Vector3(-31f, -2f, 0f),
                new Vector3(-29f, -2f, 0f),
            };

            for (var i = 0; i < positions.Length; i++)
            {
                CreateForagePoint(parent.transform, $"farm_forage_{(i + 1):00}", positions[i]);
            }
        }

        private static void CreateForagePoint(Transform parent, string spawnId, Vector3 position)
        {
            var obj = new GameObject($"ForagePoint_{spawnId}");
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                obj, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.Interactable);
            if (!ScaleProfileLibrary.AttachApplicator(obj, EntityScaleCategory.ForagePoint))
            {
                obj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.5f, 0.72f, 0.3f);
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(sr, "World", 0);

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            var vc = obj.AddComponent<FarmResourceVisualController>();
            var serializedVc = new SerializedObject(vc);
            serializedVc.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            serializedVc.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vc);

            var interactable = obj.AddComponent<CindarsHope.Farm.Forage.ForagePointInteractable>();
            var serializedI = new SerializedObject(interactable);
            serializedI.FindProperty("_spawnId").stringValue = spawnId;
            serializedI.FindProperty("_zoneId").stringValue = "farm_zone_forage";
            serializedI.FindProperty("_interactionPrompt").stringValue = "Coletar forrageio";
            serializedI.FindProperty("_visualController").objectReferenceValue = vc;
            serializedI.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactable);
        }

        // fable_54: caixa de envio FISICA na Zone_ShippingSellpoint (3.5,7.5). Interagir deposita os
        // itens vendaveis para venda overnight (ShippingBinRuntimeService). Canal ADICIONAL ao
        // SellPoint imediato (que permanece intacto). ID estavel farm_shipping_bin_01.
        private static void CreateShippingBin()
        {
            var obj = new GameObject("ShippingBin_farm_shipping_bin_01");
            // v7: homestead leste (spec_farm_scene_relayout_v4 §15.5 v7).
            // Coord 64x44: (28, 4).
            obj.transform.position = new Vector3(28f, 4f, 0f);
            CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                obj, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.Interactable);
            if (!ScaleProfileLibrary.AttachApplicator(obj, EntityScaleCategory.ShippingBin))
            {
                obj.transform.localScale = new Vector3(1.4f, 1.1f, 1f);
            }

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.78f, 0.58f, 0.22f);
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(sr, "World", 0);

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.3f, 1.1f);

            var interactable = obj.AddComponent<CindarsHope.Farm.Shipping.ShippingBinInteractable>();
            var serializedI = new SerializedObject(interactable);
            serializedI.FindProperty("_interactionPrompt").stringValue = "Depositar para envio";
            serializedI.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactable);
        }

        private static void CreateTreeResource(Transform parent, InventoryManager inventoryManager, Vector3 position)
        {
            // obj.transform.position/scale continuam sendo o que ScaleProfileLibrary/collider/layer
            // esperam (AttachApplicator baqueia a escala em target.transform, não num visualRoot — ver
            // ScaleProfileLibrary.AttachApplicator; passar um visualRoot causaria double-scale em Play
            // Mode). Só o SpriteRenderer visual muda para um child "Visual" (herda a escala do pai pela
            // hierarquia) — trees/ agora importa com pivot BottomCenter (Y-sort), então só o child
            // precisa de reposicionamento de base; obj/collider ficam intocados.
            var obj = new GameObject("TreeResource_01");
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                obj, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.Interactable);
            if (!ScaleProfileLibrary.AttachApplicator(obj, EntityScaleCategory.ResourceTree))
            {
                obj.transform.localScale = new Vector3(1.2f, 1.8f, 1f);
            }

            var visual = new GameObject("Visual");
            visual.transform.SetParent(obj.transform);
            var sr = visual.AddComponent<SpriteRenderer>();
            var treeSprite = WorldSpriteLibrary.Tree("tree_oak");
            if (treeSprite != null)
            {
                sr.sprite = treeSprite; sr.color = Color.white;
                float visualHeight = treeSprite.bounds.size.y; // child scale is 1; obj carries the scale
                visual.transform.localPosition = new Vector3(0f, WorldSpriteBasePlacement.BaseYForVisualCenter(0f, visualHeight), 0f);
            }
            else
            {
                visual.transform.localPosition = Vector3.zero;
                sr.sprite = GetBuiltinSprite(); sr.color = new Color(0.24f, 0.52f, 0.24f);
            }
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(sr, "World", 0);

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            var vc = obj.AddComponent<FarmResourceVisualController>();
            var serializedVc = new SerializedObject(vc);
            serializedVc.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            serializedVc.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vc);

            var interactable = obj.AddComponent<FarmResourceInteractable>();
            var serializedI = new SerializedObject(interactable);
            serializedI.FindProperty("_resourceType").enumValueIndex = (int)FarmResourceInteractableType.Tree;
            serializedI.FindProperty("_interactionPrompt").stringValue = "Cortar árvore";
            serializedI.FindProperty("_visualController").objectReferenceValue = vc;
            serializedI.FindProperty("_inventoryManager").objectReferenceValue = inventoryManager;
            var rewardProp = serializedI.FindProperty("_reward");
            rewardProp.FindPropertyRelative("_itemId").stringValue = "item_wood";
            rewardProp.FindPropertyRelative("_amount").intValue = 2;
            serializedI.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactable);
        }

        private static void CreateRockResource(Transform parent, InventoryManager inventoryManager, Vector3 position)
        {
            // Mesmo padrão de CreateTreeResource: obj/collider/AttachApplicator intocados; sprite visual
            // num child "Visual" reposicionado pela base (props/ agora importa com pivot BottomCenter).
            var obj = new GameObject("RockResource_01");
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                obj, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.Interactable);
            if (!ScaleProfileLibrary.AttachApplicator(obj, EntityScaleCategory.ResourceRock))
            {
                obj.transform.localScale = new Vector3(1.0f, 0.85f, 1f);
            }

            var visual = new GameObject("Visual");
            visual.transform.SetParent(obj.transform);
            var sr = visual.AddComponent<SpriteRenderer>();
            var rockSprite = WorldSpriteLibrary.Prop("rock_ore_0");
            if (rockSprite != null)
            {
                sr.sprite = rockSprite; sr.color = Color.white;
                float visualHeight = rockSprite.bounds.size.y; // child scale is 1; obj carries the scale
                visual.transform.localPosition = new Vector3(0f, WorldSpriteBasePlacement.BaseYForVisualCenter(0f, visualHeight), 0f);
            }
            else
            {
                visual.transform.localPosition = Vector3.zero;
                sr.sprite = GetBuiltinSprite(); sr.color = new Color(0.55f, 0.52f, 0.50f);
            }
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(sr, "World", 0);

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            var vc = obj.AddComponent<FarmResourceVisualController>();
            var serializedVc = new SerializedObject(vc);
            serializedVc.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            serializedVc.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vc);

            var interactable = obj.AddComponent<FarmResourceInteractable>();
            var serializedI = new SerializedObject(interactable);
            serializedI.FindProperty("_resourceType").enumValueIndex = (int)FarmResourceInteractableType.Rock;
            serializedI.FindProperty("_interactionPrompt").stringValue = "Minerar pedra";
            serializedI.FindProperty("_visualController").objectReferenceValue = vc;
            serializedI.FindProperty("_inventoryManager").objectReferenceValue = inventoryManager;
            var rewardProp = serializedI.FindProperty("_reward");
            rewardProp.FindPropertyRelative("_itemId").stringValue = "item_stone";
            rewardProp.FindPropertyRelative("_amount").intValue = 2;
            serializedI.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactable);
        }

        // fable_54: CreateForageResource (smoke ForageResource_01, reward fixo item_herbs) foi
        // REMOVIDO — substituido por CreateForagePoints + ForagePointInteractable +
        // FarmForageRuntimeService. Os dois caminhos NAO coexistem (anti-regressao).

        // fable_41 lotes de expansao (norte/leste/oeste) REMOVIDOS da cena em 2026-06-26
        // (spec_farm_scene_relayout_v4 Desvio 2 corrigido): o Quadro de Evolucoes
        // (FarmEvolutionBoardInteractable) e o mecanismo de substituicao.
        // FarmLotService / FarmLotsSectionProvider / FarmLotCatalog sao sistemas de save/runtime
        // independentes e permanecem; apenas a geracao de Lot_North/East/West na cena foi removida.

        // spec_farm_scene_relayout_v4: Quadro de Evolucoes — substitui lotes fable_41.
        // v5: Posicao 56x40 (spec_farm_scene_relayout_v4 §15.2 v5): (24, 2) a leste da casa.
        private static void CreateFarmEvolutionBoard()
        {
            var obj = new GameObject("FarmEvolutionBoard");
            // v7 Coord 64x44: (30, 4) — homestead leste (spec_farm_scene_relayout_v4 §15.5 v7).
            obj.transform.position = new Vector3(30f, 4f, 0f);
            obj.transform.localScale = new Vector3(1.1f, 1.4f, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.55f, 0.38f, 0.22f);
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(sr, "World", 0);

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 1.6f);

            obj.AddComponent<FarmEvolutionBoardInteractable>();
            EditorUtility.SetDirty(obj);
        }

        // spec_farm_scene_relayout_v4 v6: Faixa de montanha (N) — visual backdrop + colisao solida.
        // v6: Faixa y in [18, 22], largura total 64 (x in [-32, 32]). Colisao em y >= 18.
        private static void CreateMountainBarrier()
        {
            var root = new GameObject("MountainBarrier");
            root.transform.position = new Vector3(0f, 20f, 0f); // centro da faixa y[18,22]

            // Visual placeholder: backdrop castanho-cinza simulando montanha.
            var visual = new GameObject("MountainBackdrop");
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(64f, 4f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.38f, 0.34f, 0.30f);
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(sr, "World", 0);

            // Colisao solida na base da montanha (nao e trigger — bloqueia o jogador em y >= 18).
            var barrier = new GameObject("MountainCollider");
            barrier.transform.SetParent(root.transform);
            barrier.transform.localPosition = Vector3.zero;
            var col = barrier.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = new Vector2(64f, 4f);
        }

        // spec_farm_scene_relayout_v4 v7: Rio BORDA LESTE — nascente (22,18) → borda leste → lago SE.
        // v7 (spec_farm_scene_relayout_v4 §15.5): rio e moldura de BORDA LESTE (nunca cruza o miolo).
        //   Rota: acude (22,18)→(21,8)→(20,-2)→(19,-8)→DESAGUA na borda N do lago (~(19,-6)).
        //   Ponte v7 em (21,3); vao sem colisor. Lago centro (18,-13) ~26x14 spans x[5,31] y[-20,-6].
        //   Colisao de cada segmento alinhada com o rect visual.
        private static void CreateRiverAndBridge()
        {
            // Cor de agua v7 — azul suave tipo Stardew.
            var waterColor   = new Color(0.42f, 0.62f, 0.85f, 0.80f);
            var waterColorSr = new Color(0.42f, 0.62f, 0.85f, 0.72f); // um pouco mais translucido p/ visuais

            var root = new GameObject("RiverAndBridge");
            root.transform.position = Vector3.zero;

            // ── Acude / Nascente v7 — (22,18) na base da montanha norte ──────────────────────────
            CreateRiverSegment(root.transform, "Acude_Nascente",
                new Vector3(22f, 18f, 0f),
                new Vector2(4f, 3f),
                waterColor);

            // ── Segmentos do rio (rota v7 — borda leste, NUNCA cruza o miolo) ───────────────────
            // PONTE v7 em (21,3): vao sem colisor y∈(2,4) para o jogador atravessar a pe.
            // Segmentos com colisor PARAM acima/abaixo do vao.

            // Seg_N v7: (22,18)→(21,8). x≈21.5, y in [8,18]. COM colisor.
            CreateRiverSegment(root.transform, "RiverSeg_N",
                new Vector3(21.5f, 13f, 0f),
                new Vector2(1.8f, 10f),
                waterColor);

            // Seg_C_Upper v7: acima da ponte. x≈21, y in [4,8]. COM colisor.
            CreateRiverSegment(root.transform, "RiverSeg_C",
                new Vector3(21f, 6f, 0f),
                new Vector2(1.8f, 4f),
                waterColor);

            // Vao da ponte v7: agua VISUAL sob a ponte, SEM colisor (passagem livre). x≈21, y in [2,4].
            CreateRiverSegment(root.transform, "RiverCrossing_Bridge",
                new Vector3(21f, 3f, 0f),
                new Vector2(1.8f, 2f),
                waterColor,
                withCollider: false);

            // Seg_C_Lower v7: abaixo da ponte. x≈20, y in [-2,2]. COM colisor.
            CreateRiverSegment(root.transform, "RiverSeg_S",
                new Vector3(20f, 0f, 0f),
                new Vector2(1.8f, 4f),
                waterColor);

            // Seg_Lower2 v7: x≈19.5, y in [-8,-2]. COM colisor.
            CreateRiverSegment(root.transform, "RiverSeg_Lower2",
                new Vector3(19.5f, -5f, 0f),
                new Vector2(1.8f, 6f),
                waterColor);

            // Seg_Delta v7: foz (19,-8)→borda N do lago (~(19,-6)). x≈19, y in [-8,-6]. COM colisor.
            // Funde na borda N do lago — sem terminar no nada.
            CreateRiverSegment(root.transform, "RiverSeg_Delta",
                new Vector3(19f, -7f, 0f),
                new Vector2(1.8f, 2f),
                waterColor);

            // ── Ponte v7 ─────────────────────────────────────────────────────────────────────────
            // Centro v7: (21,3); footprint ~3×2; sortingOrder superior ao rio.
            var bridge = new GameObject("Bridge_01");
            bridge.transform.SetParent(root.transform);
            bridge.transform.position = new Vector3(21f, 3f, 0f);
            bridge.transform.localScale = new Vector3(3f, 2f, 1f);
            var bridgeSr = bridge.AddComponent<SpriteRenderer>();
            bridgeSr.sprite = GetBuiltinSprite();
            bridgeSr.color = new Color(0.68f, 0.52f, 0.32f); // madeira clara
            bridgeSr.sortingOrder = 0;
            bridgeSr.spriteSortPoint = SpriteSortPoint.Pivot;
            TrySetSortingLayer(bridgeSr, "World", 0);

            // ── Lago organico SE v7 (~26x14) ─────────────────────────────────────────────────────
            // Centro v7: (18,-13). spans x[5,31] y[-20,-6]; foz do rio funde na borda N (~19,-6).
            CreateLakeBody(root.transform, "LakeBody_Main",
                new Vector3(18f, -13f, 0f), new Vector3(20f, 10f, 1f), waterColorSr, -2);
            CreateLakeBody(root.transform, "LakeBody_NE",
                new Vector3(26f, -9f, 0f), new Vector3(10f, 6f, 1f), waterColorSr, -2); // Borda NE
            CreateLakeBody(root.transform, "LakeBody_N",
                new Vector3(18f, -7f, 0f), new Vector3(16f, 4f, 1f), waterColorSr, -2); // Borda N — funde com foz do rio
            CreateLakeBody(root.transform, "LakeBody_SE",
                new Vector3(24f, -17f, 0f), new Vector3(14f, 6f, 1f), waterColorSr, -2);
        }

        // Corpo de lago (visual sem collider — apenas backdrop estetico). "order" preservado na
        // assinatura por compatibilidade dos chamadores, mas a agua e sempre Ground/0 (contrato de
        // sorting da FASE 1 — profundidade decidida por Y-sort, nao por sortingOrder relativo).
        private static void CreateLakeBody(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, int order)
        {
            _ = order;
            var body = new GameObject(name);
            body.transform.SetParent(parent);
            body.transform.position = pos;
            body.transform.localScale = scale;
            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = color;
            sr.sortingOrder = 0;
            TrySetSortingLayer(sr, "Ground", 0);
        }

        // waterColor: opcional — se default(Color), usa azul v5 (0.42,0.62,0.85).
        // withCollider: false = água apenas VISUAL (sob a ponte) — não bloqueia o jogador.
        private static void CreateRiverSegment(Transform parent, string segmentName, Vector3 position, Vector2 colliderSize,
            Color waterColor = default, bool withCollider = true)
        {
            // Cor padrao v5: azul agua (0.42,0.62,0.85).
            if (waterColor == default)
                waterColor = new Color(0.42f, 0.62f, 0.85f, 0.80f);

            var seg = new GameObject(segmentName);
            seg.transform.SetParent(parent);
            seg.transform.position = position;
            seg.transform.localScale = new Vector3(colliderSize.x, colliderSize.y, 1f);

            // Visual azul v5 — cor passada ou default.
            var sr = seg.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = waterColor;
            sr.sortingOrder = 0;
            TrySetSortingLayer(sr, "Ground", 0);

            // Colisao solida (água bloqueia) — EXCETO no vão da ponte (withCollider:false).
            // BoxCollider2D usa size em espaco local — mantem (1,1) para herdar a escala do transform.
            if (withCollider)
            {
                var col = seg.AddComponent<BoxCollider2D>();
                col.isTrigger = false;
                col.size = Vector2.one;
            }
        }

        // spec_farm_scene_relayout_v4 v7: 4 veios de minerio bloqueados na base da montanha.
        // v7 Posicoes 64x44: (-18,18.5), (-9,18.5), (0,18.5), (9,18.5).
        private static void CreateLockedOreNodes()
        {
            var root = new GameObject("LockedOreNodes");
            root.transform.position = Vector3.zero;

            var positions = new[]
            {
                new Vector3(-18f, 18.5f, 0f),
                new Vector3( -9f, 18.5f, 0f),
                new Vector3(  0f, 18.5f, 0f),
                new Vector3(  9f, 18.5f, 0f),
            };

            var nodeIds = new[]
            {
                "ore_node_farm_01",
                "ore_node_farm_02",
                "ore_node_farm_03",
                "ore_node_farm_04",
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var obj = new GameObject($"OreNode_{nodeIds[i]}");
                obj.transform.SetParent(root.transform);
                obj.transform.position = positions[i];
                obj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = GetBuiltinSprite();
                sr.color = new Color(0.45f, 0.40f, 0.36f); // pedra escura com veia
                sr.sortingOrder = 0;
                sr.spriteSortPoint = SpriteSortPoint.Pivot;
                TrySetSortingLayer(sr, "World", 0);

                var col = obj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = Vector2.one;

                var interactable = obj.AddComponent<LockedOreNodeInteractable>();
                interactable.EditorSetNodeId(nodeIds[i]);
                EditorUtility.SetDirty(interactable);
            }
        }

        // spec_farm_scene_relayout_v4: FarmSceneRuntimeBootstrap — configura FarmTileGrid v4
        // e zonas nao-araveis no Start(). Ref ao SaveManager injetada aqui (sem FindObjectOfType).
        private static void CreateFarmSceneRuntimeBootstrap(SaveManager saveManager)
        {
            var obj = new GameObject("FarmSceneRuntimeBootstrap");
            var bootstrap = obj.AddComponent<FarmSceneRuntimeBootstrap>();
            bootstrap.EditorWire(saveManager);
            EditorUtility.SetDirty(bootstrap);
        }

        // spec_farm_scene_relayout_v4: FarmTillingInputController — input de aragem/rega pelo
        // jogador (tecla [F]). Fecha gap T006 da Spec B. Refs injetadas (sem FindObjectOfType).
        // Fase 8: tambem injeta EquipmentManager para checagem real de ferramenta.
        // spec_codex_03: tambem injeta StaminaManager/TimeManager para stamina e dia reais
        // (evita que o fallback permissivo mascare o gap apos regenerar a cena).
        private static void CreateFarmTillingInputController(SaveManager saveManager, Transform playerTransform, EquipmentManager equipmentManager,
            StaminaManager staminaManager = null, TimeManager timeManager = null)
        {
            var obj = new GameObject("FarmTillingInputController");
            var controller = obj.AddComponent<FarmTillingInputController>();
            controller.EditorWire(saveManager, playerTransform, equipmentManager, staminaManager, timeManager);
            EditorUtility.SetDirty(controller);
        }

        private static void EnsureFolder(string parentFolder, string childFolder)
        {
            var fullPath = $"{parentFolder}/{childFolder}";
            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }

            AssetDatabase.CreateFolder(parentFolder, childFolder);
        }
    }
}
