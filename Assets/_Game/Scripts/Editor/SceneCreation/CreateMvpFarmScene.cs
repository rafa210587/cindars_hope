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
using CindarsHope.Farm.Integration;
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
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using CindarsHope.UI.Hotbar;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Routing;
using CindarsHope.Editor.Validation;

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

        [MenuItem("CindarsHope/Create Scenes/Farm Scene", priority = 20)]
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
            var treeRegistry = CreateTrees(inventoryManager);
            var itemPickupRegistry = CreateItemPickups(inventoryManager);
            var craftingModal = CreateCraftingUi(craftingRuntime, modalManager);
            CreateCraftingStations(craftingRuntime, craftingModal);
            CreateFarmPortals();
            CreateFishingSpot(inventoryManager);
            CreateFarmSceneFoundationZones();
            CreateFarmResourceInteractables(inventoryManager);
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
            CreateMainCamera(playerTransform);
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
            player.transform.localScale = new Vector3(1f, 1.5f, 1f);

            var spriteRenderer = player.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.23f, 0.48f, 0.84f);
            spriteRenderer.sortingOrder = 0;
            TrySetSortingLayer(spriteRenderer, "Characters", 0);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("Player placeholder SpriteRenderer was created without a sprite. Replace it with the final placeholder sprite in a future art PR.");
            }

            var rigidbody = player.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.gravityScale = 0f;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = player.AddComponent<BoxCollider2D>();
            FitBoxColliderToOpaqueSprite(collider, spriteRenderer);

            var playerController = player.AddComponent<PlayerController>();
            ConfigurePlayerController(playerController, rigidbody);

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
                SetReference(serializedDash, "_rigidbody", rigidbody);
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

            var defaultSpawn = CreateSceneSpawnPoint(parent.transform, "farm_default", Vector3.zero);
            var fromTownSpawn = CreateSceneSpawnPoint(parent.transform, "farm_from_town", new Vector3(-7.25f, -4.75f, 0f));
            var fromCaveSpawn = CreateSceneSpawnPoint(parent.transform, "farm_from_cave", new Vector3(-5f, 0f, 0f));

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

            CreateScenePortal(
                portals.transform,
                "Portal_Farm_To_Town",
                new Vector3(-8.25f, -4.75f, 0f),
                new Color(0.29f, 0.43f, 0.67f),
                "TownScene",
                "Assets/_Game/Scenes/TownScene.unity",
                "town_from_farm",
                "Ir para Cindar's Hope");

            CreateScenePortal(
                portals.transform,
                "Portal_Farm_To_Cave",
                new Vector3(-5.5f, 0f, 0f),
                new Color(0.5f, 0.25f, 0.6f),
                "CaveScene",
                "Assets/_Game/Scenes/CaveScene.unity",
                "cave_from_farm",
                "Entrar na Caverna");
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
            portalObject.transform.localScale = new Vector3(1f, 1.35f, 1f);

            var spriteRenderer = portalObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

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
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

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
            sellPointObject.transform.position = new Vector3(3.5f, 7.5f, 0f);
            sellPointObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var spriteRenderer = sellPointObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.25f, 0.75f, 0.85f);
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

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
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

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
            CreateCraftingStation("Workbench", "farm_workbench_01", WorkshopType.Workbench, new Vector3(-3.25f, -4.6f, 0f), new Color(0.58f, 0.36f, 0.18f), craftingRuntime, craftingModal);
            CreateCraftingStation("Forge", "farm_forge_01", WorkshopType.Forge, new Vector3(-1.9f, -4.6f, 0f), new Color(0.58f, 0.23f, 0.16f), craftingRuntime, craftingModal);
            CreateCraftingStation("CookingStation", "farm_cooking_01", WorkshopType.CookingStation, new Vector3(-0.55f, -4.6f, 0f), new Color(0.77f, 0.55f, 0.22f), craftingRuntime, craftingModal);
        }

        private static void CreateCraftingStation(string label, string stationId, WorkshopType stationType, Vector3 position, Color color, CraftingRuntime craftingRuntime, CraftingModal craftingModal)
        {
            var craftingObject = new GameObject($"CraftingStation_{label}");
            craftingObject.transform.position = position;
            craftingObject.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            var spriteRenderer = craftingObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

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
            fishingObject.transform.position = new Vector3(7.8f, -2.8f, 0f);
            var lakeScaleConfig = AssetDatabase.LoadAssetAtPath<GameScaleConfigSO>(GameScaleConfigPath);
            var lakeScale = lakeScaleConfig != null ? lakeScaleConfig.LakeScale : 6f;
            fishingObject.transform.localScale = new Vector3(lakeScale, lakeScale, 1f);

            var spriteRenderer = fishingObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.18f, 0.42f, 0.85f);
            spriteRenderer.sortingOrder = 1;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

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

            // Reconciled layout v2 (2026-06-08): trees consolidated east, crops moved from portal area,
            // house entrance moved from bounds edge, bounds reduced to 28x22.
            CreateFarmSceneZone(parent.transform, "Zone_PlayerSpawn", FarmSceneZoneType.PlayerSpawn, "farm_zone_player_spawn", new Vector3(0f, 0f, 0f), new Vector2(1.4f, 1.4f), new Color(0.2f, 0.45f, 0.95f, 0.65f));
            CreateFarmSceneZone(parent.transform, "Zone_CropField", FarmSceneZoneType.CropField, "farm_zone_crop_field", new Vector3(1f, -2.25f, 0f), new Vector2(5.5f, 5.5f), new Color(0.35f, 0.22f, 0.12f, 0.45f));
            CreateFarmSceneZone(parent.transform, "Zone_ResourceTrees", FarmSceneZoneType.ResourceTrees, "farm_zone_resource_trees", new Vector3(9.5f, 1.0f, 0f), new Vector2(10.0f, 13.0f), new Color(0.14f, 0.48f, 0.18f, 0.35f));
            CreateFarmSceneZone(parent.transform, "Zone_ResourceRocks", FarmSceneZoneType.ResourceRocks, "farm_zone_resource_rocks", new Vector3(-9.0f, 5.0f, 0f), new Vector2(4.5f, 3.5f), new Color(0.42f, 0.42f, 0.42f, 0.55f));
            CreateFarmSceneZone(parent.transform, "Zone_Forage", FarmSceneZoneType.Forage, "farm_zone_forage", new Vector3(-8.0f, -2.0f, 0f), new Vector2(4.5f, 4.5f), new Color(0.45f, 0.64f, 0.25f, 0.4f));
            CreateFarmSceneZone(parent.transform, "Zone_LakeFishing", FarmSceneZoneType.LakeFishing, "farm_zone_lake_fishing", new Vector3(7.8f, -2.8f, 0f), new Vector2(5.8f, 4.4f), new Color(0.18f, 0.44f, 0.82f, 0.5f));
            CreateFarmSceneZone(parent.transform, "Zone_ShippingSellpoint", FarmSceneZoneType.ShippingSellpoint, "farm_zone_shipping_sellpoint", new Vector3(3.5f, 7.5f, 0f), new Vector2(3.5f, 2.5f), new Color(0.85f, 0.62f, 0.18f, 0.65f));
            CreateFarmSceneZone(parent.transform, "Zone_Construction", FarmSceneZoneType.Construction, "farm_zone_construction", new Vector3(-1.0f, 9.0f, 0f), new Vector2(6.0f, 3.0f), new Color(0.58f, 0.45f, 0.32f, 0.45f));
            CreateFarmSceneZone(parent.transform, "Zone_HouseEntrance", FarmSceneZoneType.HouseEntrance, "farm_zone_house_entrance", new Vector3(-5.0f, -7.0f, 0f), new Vector2(2.5f, 2.0f), new Color(0.62f, 0.36f, 0.25f, 0.65f));
            CreateFarmSceneZone(parent.transform, "Zone_TownExit", FarmSceneZoneType.TownExit, "farm_zone_town_exit", new Vector3(-8.25f, -4.75f, 0f), new Vector2(1.6f, 2.4f), new Color(0.82f, 0.82f, 0.25f, 0.55f));
            CreateFarmSceneZone(parent.transform, "Zone_CaveEntrance", FarmSceneZoneType.CaveEntrance, "farm_zone_cave_entrance", new Vector3(-5.5f, 0f, 0f), new Vector2(1.8f, 2.4f), new Color(0.35f, 0.28f, 0.5f, 0.65f));
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
            markerObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            var spriteRenderer = markerObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = -5;
            TrySetSortingLayer(spriteRenderer, "Ground", spriteRenderer.sortingOrder);

            var collider = markerObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var marker = markerObject.AddComponent<FarmSceneZoneMarker>();
            var serializedMarker = new SerializedObject(marker);
            serializedMarker.FindProperty("zoneType").enumValueIndex = (int)zoneType;
            serializedMarker.FindProperty("stableId").stringValue = stableId;
            serializedMarker.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(marker);
        }

        private static ItemPickupRegistry CreateItemPickups(InventoryManager inventoryManager)
        {
            var parent = new GameObject("ItemPickups");
            parent.transform.position = Vector3.zero;
            var registry = parent.AddComponent<ItemPickupRegistry>();

            var pickups = new ItemPickup[1];
            pickups[0] = CreateItemPickup(parent.transform, 0, "seed_carrot", 1, new Vector3(3.75f, -1.5f, 0f), inventoryManager);
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
            pickupObject.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            var spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.95f, 0.5f, 0.22f);
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

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

        private static FarmPlotRegistry CreateFarmPlots(InventoryManager inventoryManager, StaminaManager staminaManager)
        {
            var seedDatabase = AssetDatabase.LoadAssetAtPath<SeedDatabaseSO>(SeedDatabasePath);
            if (seedDatabase == null)
            {
                Debug.LogWarning($"SeedDatabaseSO not found at {SeedDatabasePath}. Assign it manually on FarmPlot objects.");
            }

            var parent = new GameObject("FarmPlots");
            parent.transform.position = new Vector3(1f, -1.5f, 0f);
            var registry = parent.AddComponent<FarmPlotRegistry>();

            const int gridSize = 3;
            const float spacing = 1.35f;
            var startPosition = new Vector3(-spacing, spacing, 0f);
            var plots = new FarmPlot[gridSize * gridSize];

            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var plotIndex = y * gridSize + x;
                    plots[plotIndex] = CreateFarmPlot(parent.transform, plotIndex, startPosition + new Vector3(x * spacing, -y * spacing, 0f), inventoryManager, seedDatabase, staminaManager);
                }
            }

            registry.Configure(plots);
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

            // All 19 trees consolidated to east cluster (x: 6-13, y: -5 to +5.5)
            // Trees 3-6 previously on west side (-8 to -6) moved here to align with Zone_ResourceTrees
            var trees = new TreeNode[19];
            trees[0]  = CreateTree(parent.transform, 0,  new Vector3(6.5f,  5.0f, 0f), treeData, inventoryManager);
            trees[1]  = CreateTree(parent.transform, 1,  new Vector3(8.0f,  5.5f, 0f), treeData, inventoryManager);
            trees[2]  = CreateTree(parent.transform, 2,  new Vector3(10.0f, 5.5f, 0f), treeData, inventoryManager);
            trees[3]  = CreateTree(parent.transform, 3,  new Vector3(12.0f, 5.0f, 0f), treeData, inventoryManager);
            trees[4]  = CreateTree(parent.transform, 4,  new Vector3(6.5f,  3.0f, 0f), treeData, inventoryManager);
            trees[5]  = CreateTree(parent.transform, 5,  new Vector3(8.5f,  3.5f, 0f), treeData, inventoryManager);
            trees[6]  = CreateTree(parent.transform, 6,  new Vector3(10.5f, 3.5f, 0f), treeData, inventoryManager);
            trees[7]  = CreateTree(parent.transform, 7,  new Vector3(12.5f, 3.0f, 0f), treeData, inventoryManager);
            trees[8]  = CreateTree(parent.transform, 8,  new Vector3(7.0f,  1.5f, 0f), treeData, inventoryManager);
            trees[9]  = CreateTree(parent.transform, 9,  new Vector3(9.0f,  1.5f, 0f), treeData, inventoryManager);
            trees[10] = CreateTree(parent.transform, 10, new Vector3(11.0f, 1.5f, 0f), treeData, inventoryManager);
            trees[11] = CreateTree(parent.transform, 11, new Vector3(7.5f, -0.5f, 0f), treeData, inventoryManager);
            trees[12] = CreateTree(parent.transform, 12, new Vector3(9.5f, -0.5f, 0f), treeData, inventoryManager);
            trees[13] = CreateTree(parent.transform, 13, new Vector3(11.5f,-0.5f, 0f), treeData, inventoryManager);
            trees[14] = CreateTree(parent.transform, 14, new Vector3(6.5f, -2.0f, 0f), treeData, inventoryManager);
            trees[15] = CreateTree(parent.transform, 15, new Vector3(8.5f, -2.5f, 0f), treeData, inventoryManager);
            trees[16] = CreateTree(parent.transform, 16, new Vector3(10.5f,-2.5f, 0f), treeData, inventoryManager);
            trees[17] = CreateTree(parent.transform, 17, new Vector3(6.5f, -4.5f, 0f), treeData, inventoryManager);
            trees[18] = CreateTree(parent.transform, 18, new Vector3(9.0f, -5.0f, 0f), treeData, inventoryManager);

            registry.Configure(trees);
            EditorUtility.SetDirty(registry);
            return registry;
        }

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
            var treeScale = treeScaleConfig != null ? treeScaleConfig.TreeScale : 3f;
            treeObject.transform.localScale = new Vector3(treeScale, treeScale, 1f);

            var spriteRenderer = treeObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.24f, 0.48f, 0.22f);
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

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

            // Farm bounds: 28x22 (reconciled v2 — reduced from 40x34 to reduce empty space)
            CreateBound("Top", bounds.transform, new Vector2(0f, 11f), new Vector2(28f, 1f));
            CreateBound("Bottom", bounds.transform, new Vector2(0f, -11f), new Vector2(28f, 1f));
            CreateBound("Left", bounds.transform, new Vector2(-14.5f, 0f), new Vector2(1f, 22f));
            CreateBound("Right", bounds.transform, new Vector2(14.5f, 0f), new Vector2(1f, 22f));
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
        {
            foreach (var layer in SortingLayer.layers)
            {
                if (layer.name == layerName)
                {
                    renderer.sortingLayerName = layerName;
                    return;
                }
            }

            renderer.sortingOrder = fallbackOrder;
        }

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

        // WAVE_INTEGRATION_06: Farm resource interactables (Tree, Rock, Forage).
        // LakeFishing uses existing FishingSpot at (7.8, -2.8) which already implements IInteractable.
        // These adapters are TODO_INTEGRATION_NOT_FINAL — for smoke validation only.
        // Final wiring must connect to TreeChopService / RockMiningService / FarmForageSpawnService.
        private static void CreateFarmResourceInteractables(InventoryManager inventoryManager)
        {
            var parent = new GameObject("FarmResourceInteractables");
            parent.transform.position = Vector3.zero;

            // Zone_ResourceTrees: (9.5, 1.0) — place one smoke resource node near center of tree zone
            CreateRockResource(parent.transform, inventoryManager, new Vector3(-9.0f, 4.5f, 0f));
            CreateForageResource(parent.transform, inventoryManager, new Vector3(-8.0f, -2.5f, 0f));
            CreateTreeResource(parent.transform, inventoryManager, new Vector3(7.5f, 3.5f, 0f));
            // LakeFishing: FishingSpot at (7.8, -2.8) already implements IInteractable (prompt: "Pescar")
            // No additional FarmResourceInteractable needed for the lake.
        }

        private static void CreateTreeResource(Transform parent, InventoryManager inventoryManager, Vector3 position)
        {
            var obj = new GameObject("TreeResource_01");
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(1.2f, 1.8f, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.24f, 0.52f, 0.24f);
            sr.sortingOrder = 2;
            TrySetSortingLayer(sr, "Items", 2);

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
            var obj = new GameObject("RockResource_01");
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(1.0f, 0.85f, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.55f, 0.52f, 0.50f);
            sr.sortingOrder = 2;
            TrySetSortingLayer(sr, "Items", 2);

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

        private static void CreateForageResource(Transform parent, InventoryManager inventoryManager, Vector3 position)
        {
            var obj = new GameObject("ForageResource_01");
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = new Color(0.52f, 0.68f, 0.25f);
            sr.sortingOrder = 2;
            TrySetSortingLayer(sr, "Items", 2);

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
            serializedI.FindProperty("_resourceType").enumValueIndex = (int)FarmResourceInteractableType.Forage;
            serializedI.FindProperty("_interactionPrompt").stringValue = "Coletar ervas";
            serializedI.FindProperty("_visualController").objectReferenceValue = vc;
            serializedI.FindProperty("_inventoryManager").objectReferenceValue = inventoryManager;
            var rewardProp = serializedI.FindProperty("_reward");
            rewardProp.FindPropertyRelative("_itemId").stringValue = "item_herbs";
            rewardProp.FindPropertyRelative("_amount").intValue = 1;
            serializedI.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactable);
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
