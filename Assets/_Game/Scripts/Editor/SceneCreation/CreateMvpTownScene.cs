using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Economy;
using CindarsHope.Enemy;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.NPC;
using CindarsHope.NPC.Schedule;
using CindarsHope.World;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using CindarsHope.Skills;
using CindarsHope.UI;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Shop;
using CindarsHope.NPC.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CindarsHope.UI.Hotbar;
using CindarsHope.Equipment;
using CindarsHope.Player.Progression;
using CindarsHope.Editor.Validation;

namespace CindarsHope.Editor.SceneCreation
{
    public static class CreateMvpTownScene
    {
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string FarmScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string RecipeDatabasePath = "Assets/_Game/Data/Registries/RecipeDatabase.asset";
        private const string WeaponDatabasePath = "Assets/_Game/Data/Combat/WeaponDatabase.asset";
        private const string SpellDatabasePath = "Assets/_Game/Data/Combat/SpellDatabase.asset";
        private const string StatusEffectDatabasePath = "Assets/_Game/Data/Combat/StatusEffectDatabase.asset";
        private const string BuiltinSpritePath = "UI/Skin/UISprite.psd";

        [MenuItem("CindarsHope/Create Scenes/Town Scene", priority = 21)]
        public static void CreateSceneFromMenu()
        {

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Cannot create MVP TownScene during Play Mode. Exit Play Mode and run this menu again.");
                return;
            }
            CreateScene();
        }

        public static void CreateScene()
        {
            EnsureFolder("Assets/_Game", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "TownScene";

            var bootstrap = CreateBootstrap();
            var playerManager = bootstrap.GetComponent<PlayerManager>();
            var inventoryManager = bootstrap.GetComponent<InventoryManager>();
            var timeManager = bootstrap.GetComponent<TimeManager>();
            var saveManager = bootstrap.GetComponent<SaveManager>();
            var hungerManager = bootstrap.GetComponent<HungerManager>();
            var craftingManager = bootstrap.GetComponent<CraftingManager>();
            var economyManager = bootstrap.GetComponent<EconomyManager>();
            var shopManager = bootstrap.GetComponent<ShopManager>();
            var modalManager = bootstrap.GetComponent<ModalManager>();
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);

            var playerTransform = CreatePlayer();
            var interactionSystem = playerTransform.GetComponent<InteractionSystem>();
            var shopUi = CreateShopUi(modalManager);

            // Camera background is a flat color playfield; do not create a giant
            // ground sprite because it reads as a horizon/central rectangle in MVP art.
            CreateBounds();
            CreateMainCamera(playerTransform);
            CreateSpawnPoints(playerTransform);
            CreatePortals();
            var npcManager = CreateNpcs(playerTransform, playerManager, inventoryManager, itemDatabase, shopManager, modalManager, shopUi);
            CreateCentralPlaza();
            CreateMarketStalls();
            CreateHouses();
            // fable_11: schedule anchors (work/social/home per NPC), minimal interiors (y>+40) and
            // functional house doors. Must run after NPCs + houses exist.
            CreateNpcScheduleAnchors();
            CreateHouseInteriorsAndDoors();
            CreateTownDecorations();
            CreateTownTrees();
            CreateDebugHud(playerManager, inventoryManager, hungerManager, interactionSystem, timeManager, saveManager);
            CreateSceneRuntimeInstaller(playerTransform);

            ConfigureBootstrap(
                bootstrap,
                playerTransform,
                playerManager,
                inventoryManager,
                timeManager,
                saveManager,
                hungerManager,
                craftingManager,
                economyManager,
                shopManager,
                modalManager,
                npcManager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            // fable_40 (CA-1): element-count audit — before (canonical baseline) = after (in scene).
            LogRelayoutElementCountAudit(scene);

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Selection.activeObject = sceneAsset;
            Debug.Log($"MVP TownScene created at {ScenePath} (48x42 relayout — fable_40).");
        }

        // fable_40 (CA-1): proves the 48×42 relayout dropped no element. The "before" baseline is
        // the canonical spec count (the relayout repositions, never removes); the "after" is what
        // the saved scene actually contains. Any mismatch is logged as an error (relayout regression).
        private static void LogRelayoutElementCountAudit(UnityEngine.SceneManagement.Scene scene)
        {
            int npcExpected = RefinedCanonicalTownNpcSpecs.Length + 1; // +1 wanderer
            int houseExpected = TownHouseSpecs.Length;
            int treeExpected = TownTreePositions.Length;
            int shopExpected = 0;
            foreach (var s in RefinedCanonicalTownNpcSpecs)
            {
                if (!string.IsNullOrWhiteSpace(s.ShopDataPath)) shopExpected++;
            }

            int npcActual = 0, houseActual = 0, treeActual = 0, stallActual = 0,
                anchorActual = 0, spawnActual = 0;
            bool lakeFound = false, hallFound = false, muralFound = false, boardFound = false;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    string n = t.gameObject.name;
                    if (n.StartsWith("NPC_")) npcActual++;
                    else if (n.StartsWith("House_")) houseActual++;
                    else if (n.StartsWith("TownTree_")) treeActual++;
                    else if (n.StartsWith("Stall_")) stallActual++;
                    else if (n.StartsWith("Anchor_npc_")) anchorActual++;
                    else if (n.StartsWith("Spawn_")) spawnActual++;
                    if (n == "LakeWater") lakeFound = true;
                    if (n == "TownHallBuilding") hallFound = true;
                    if (n == "TownHallMural") muralFound = true;
                    if (n == "WarriorStatue") boardFound = true;
                }
            }

            Debug.Log(
                "[fable_40] Town 48x42 relayout element-count audit (before=canonical | after=scene):\n" +
                $"  NPCs:    before={npcExpected} after={npcActual}\n" +
                $"  Houses:  before={houseExpected} after={houseActual}\n" +
                $"  Trees:   before={treeExpected} after={treeActual}\n" +
                $"  Stalls:  before={shopExpected} after={stallActual}\n" +
                $"  Anchors: before={npcExpected * 3} after={anchorActual} (work/social/home)\n" +
                $"  Spawns:  before={TownDistrictLayout.StableSpawnIds.Length} after={spawnActual}\n" +
                $"  New districts: lake/park={lakeFound} townHall={hallFound} mural={muralFound}\n" +
                $"  Landmark statue present={boardFound}\n" +
                $"  Footprint: {TownDistrictLayout.WidthTiles}x{TownDistrictLayout.HeightTiles} " +
                $"(bounds +/-{TownDistrictLayout.HalfWidth}/+/-{TownDistrictLayout.HalfHeight})");

            bool ok = npcActual == npcExpected && houseActual == houseExpected &&
                      treeActual == treeExpected && stallActual == shopExpected &&
                      anchorActual == npcExpected * 3 &&
                      spawnActual == TownDistrictLayout.StableSpawnIds.Length &&
                      lakeFound && hallFound && muralFound && boardFound;
            if (!ok)
            {
                Debug.LogError("[fable_40] RELAYOUT REGRESSION: element count before != after, " +
                               "or a required new district/landmark is missing. See the audit above.");
            }
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
            bootstrapObject.AddComponent<EconomyManager>();
            bootstrapObject.AddComponent<ShopManager>();
            bootstrapObject.AddComponent<ModalManager>();
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
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            TimeManager timeManager,
            SaveManager saveManager,
            HungerManager hungerManager,
            CraftingManager craftingManager,
            EconomyManager economyManager,
            ShopManager shopManager,
            ModalManager modalManager,
            NpcManager npcManager)
        {
            var serializedBootstrap = new SerializedObject(bootstrap);
            SetReference(serializedBootstrap, "_playerManager", playerManager);
            SetReference(serializedBootstrap, "_inventoryManager", inventoryManager);
            SetReference(serializedBootstrap, "_timeManager", timeManager);
            SetReference(serializedBootstrap, "_saveManager", saveManager);
            SetReference(serializedBootstrap, "_hungerManager", hungerManager);
            SetReference(serializedBootstrap, "_staminaManager", bootstrap.GetComponent<StaminaManager>());
            SetReference(serializedBootstrap, "_gameTimeManager", bootstrap.GetComponent<GameTimeManager>());
            SetReference(serializedBootstrap, "_statusEffectManager", bootstrap.GetComponent<StatusEffectManager>());
            SetReference(serializedBootstrap, "_craftingManager", craftingManager);
            SetReference(serializedBootstrap, "_economyManager", economyManager);
            SetReference(serializedBootstrap, "_shopManager", shopManager);
            SetReference(serializedBootstrap, "_modalManager", modalManager);
            SetReference(serializedBootstrap, "_equipmentManager", bootstrap.GetComponent<EquipmentManager>());
            SetReference(serializedBootstrap, "_progressionManager", bootstrap.GetComponent<PlayerProgressionManager>());
            SetReference(serializedBootstrap, "_skillTreeManager", bootstrap.GetComponent<SkillTreeManager>());
            SetReference(serializedBootstrap, "_bestiaryManager", bootstrap.GetComponent<BestiaryManager>());
            PlayerNeedsDataInitializer.ConfigureRuntimeManagers(bootstrap, timeManager, modalManager);

            ConfigureDayAdvanceInput(bootstrap.GetComponent<DayAdvanceInput>(), timeManager);
            ConfigureFoodConsumer(bootstrap.GetComponent<FoodConsumer>(), inventoryManager, hungerManager);
            ConfigureSaveInput(bootstrap.GetComponent<SaveInput>(), saveManager);
            ConfigureHotbarDebugInput(
                bootstrap.GetComponent<HotbarDebugInput>(),
                saveManager);
            ConfigureSaveManager(saveManager, playerManager, inventoryManager, hungerManager, timeManager, playerTransform, npcManager);
            ConfigureCraftingManager(craftingManager, inventoryManager);
            ConfigureEconomyManager(economyManager, inventoryManager, playerManager);

            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData != null)
            {
                SetReference(serializedBootstrap, "_playerData", playerData);
                ConfigureHungerManager(hungerManager, playerData, playerManager, playerTransform);
            }
            else
            {
                Debug.LogWarning($"PlayerDataSO not found at {PlayerDataPath}. Assign it manually on TownScene GameBootstrap.");
                ConfigureHungerManager(hungerManager, null, playerManager, playerTransform);
            }

            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (itemDatabase != null)
            {
                SetReference(serializedBootstrap, "_itemDatabase", itemDatabase);
                var serializedShop = new SerializedObject(shopManager);
                SetReference(serializedShop, "_itemDatabase", itemDatabase);
                serializedShop.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(shopManager);
            }
            else
            {
                Debug.LogWarning($"ItemDatabaseSO not found at {ItemDatabasePath}. Assign it manually on TownScene GameBootstrap.");
            }

            var weaponDatabase = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(WeaponDatabasePath);
            if (weaponDatabase != null)
            {
                SetReference(serializedBootstrap, "_weaponDatabase", weaponDatabase);
            }
            else
            {
                Debug.LogWarning($"WeaponDatabaseSO not found at {WeaponDatabasePath}. Assign it manually on TownScene GameBootstrap.");
            }

            var spellDatabase = AssetDatabase.LoadAssetAtPath<SpellDatabaseSO>(SpellDatabasePath);
            if (spellDatabase != null)
            {
                SetReference(serializedBootstrap, "_spellDatabase", spellDatabase);
            }
            else
            {
                Debug.LogWarning($"SpellDatabaseSO not found at {SpellDatabasePath}. Assign it manually on TownScene GameBootstrap.");
            }

            var statusEffectDatabase = AssetDatabase.LoadAssetAtPath<StatusEffectDatabaseSO>(StatusEffectDatabasePath);
            if (statusEffectDatabase != null)
            {
                SetReference(serializedBootstrap, "_statusEffectDatabase", statusEffectDatabase);
            }
            else
            {
                Debug.LogWarning($"StatusEffectDatabaseSO not found at {StatusEffectDatabasePath}. Create it or assign it manually on TownScene GameBootstrap.");
            }

            SetReference(serializedBootstrap, "_manaManager", bootstrap.GetComponent<ManaManager>());

            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
            var serializedSave = new SerializedObject(saveManager);
            SetReference(serializedSave, "_shopManager", shopManager);
            SetReference(serializedSave, "_skillTreeManager", bootstrap.GetComponent<SkillTreeManager>());
            SetReference(serializedSave, "_bestiaryManager", bootstrap.GetComponent<BestiaryManager>());
            serializedSave.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(saveManager);
            saveManager.RebindOptionalRuntimeManagers(
                bootstrap.GetComponent<EquipmentManager>(),
                bootstrap.GetComponent<PlayerProgressionManager>(),
                bootstrap.GetComponent<GameTimeManager>(),
                bootstrap.GetComponent<StaminaManager>(),
                bootstrap.GetComponent<StatusEffectManager>(),
                bootstrap.GetComponent<SkillTreeManager>(),
                shopManager,
                bootstrap.GetComponent<BestiaryManager>());
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
            Transform playerTransform,
            NpcManager npcManager)
        {
            var serializedSave = new SerializedObject(saveManager);
            SetReference(serializedSave, "_playerManager", playerManager);
            SetReference(serializedSave, "_inventoryManager", inventoryManager);
            SetReference(serializedSave, "_hungerManager", hungerManager);
            SetReference(serializedSave, "_timeManager", timeManager);
            SetReference(serializedSave, "_playerTransform", playerTransform);
            SetReference(serializedSave, "_npcManager", npcManager);
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
                Debug.LogWarning($"RecipeDatabaseSO not found at {RecipeDatabasePath}. Assign it manually on TownScene CraftingManager.");
            }

            serializedCrafting.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(craftingManager);
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
            TrySetSortingLayer(spriteRenderer, "Characters", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("Town Player placeholder SpriteRenderer was created without a sprite. Replace it with the final placeholder sprite in a future art PR.");
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
                Debug.LogWarning($"PlayerDataSO not found at {PlayerDataPath}. Assign it manually on TownScene PlayerController.");
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

        private static void CreateBounds()
        {
            var bounds = new GameObject("Bounds");
            bounds.transform.position = Vector3.zero;

            // fable_40: canonical 48×42 footprint (bounds −24..24 / −21..21, city_rules Rule 1).
            // Border colliders sit half a tile outside the playfield so the perimeter is
            // intransponível (CA-4); each spans the full canonical width/height + overlap.
            float w = TownDistrictLayout.WidthTiles;   // 48
            float h = TownDistrictLayout.HeightTiles;  // 42
            float halfW = TownDistrictLayout.HalfWidth;  // 24
            float halfH = TownDistrictLayout.HalfHeight; // 21
            CreateBound("Top", bounds.transform, new Vector2(0f, halfH + 0.5f), new Vector2(w + 1f, 1f));
            CreateBound("Bottom", bounds.transform, new Vector2(0f, -(halfH + 0.5f)), new Vector2(w + 1f, 1f));
            CreateBound("Left", bounds.transform, new Vector2(-(halfW + 0.5f), 0f), new Vector2(1f, h + 1f));
            CreateBound("Right", bounds.transform, new Vector2(halfW + 0.5f, 0f), new Vector2(1f, h + 1f));
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
            camera.orthographicSize = 8f; // calibrate in Play Mode with CameraScaleConfigSO
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.6211765f, 0.6086274f, 0.5584314f);

            var cameraFollow = cameraObject.AddComponent<CindarsHope.Camera.CameraFollow2D>();
            var serializedFollow = new SerializedObject(cameraFollow);
            SetReference(serializedFollow, "_target", playerTransform);
            serializedFollow.FindProperty("_snapOnStart").boolValue = true;
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cameraFollow);
        }

        private static void CreateSpawnPoints(Transform playerTransform)
        {
            var parent = new GameObject("SpawnPoints");
            parent.transform.position = Vector3.zero;

            // fable_40: spawn IDs are frozen (CA-4); only positions move into the 48×42 footprint.
            var defaultSpawn = CreateSpawnPoint(parent.transform, "town_default", TownDistrictLayout.Reposition(new Vector3(0f, -5f, 0f)));
            var fromFarmSpawn = CreateSpawnPoint(parent.transform, "town_from_farm", TownDistrictLayout.Reposition(new Vector3(0f, -11.5f, 0f)));

            var installer = parent.AddComponent<SceneSpawnInstaller>();
            var serializedInstaller = new SerializedObject(installer);
            SetReference(serializedInstaller, "_playerTransform", playerTransform);
            serializedInstaller.FindProperty("_spawnPoints").arraySize = 2;
            serializedInstaller.FindProperty("_spawnPoints").GetArrayElementAtIndex(0).objectReferenceValue = defaultSpawn;
            serializedInstaller.FindProperty("_spawnPoints").GetArrayElementAtIndex(1).objectReferenceValue = fromFarmSpawn;
            serializedInstaller.FindProperty("_defaultSpawnId").stringValue = "town_default";
            serializedInstaller.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
        }

        private static SceneSpawnPoint CreateSpawnPoint(Transform parent, string spawnId, Vector3 position)
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
                Debug.LogWarning($"CreateSpawnPoint: Could not find _spawnId property on {spawnPoint.GetType().Name}. Using reflection fallback.", spawnPoint);
                var field = spawnPoint.GetType().GetField("_spawnId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(spawnPoint, spawnId);
                }
            }

            EditorUtility.SetDirty(spawnPoint);
            return spawnPoint;
        }

        private static void CreatePortals()
        {
            var portals = new GameObject("Portals");
            portals.transform.position = Vector3.zero;

            // South gate: the road back to the farm leaves the town at the bottom wall.
            // fable_40: portal moved to the new south perimeter; target scene/spawn IDs frozen.
            CreateScenePortal(
                portals.transform,
                "Portal_Town_To_Farm",
                new Vector3(0f, -(TownDistrictLayout.HalfHeight - 2f), 0f),
                new Color(0.78f, 0.62f, 0.24f),
                "FarmScene",
                FarmScenePath,
                "farm_from_town",
                "Voltar para a Fazenda");
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

        // ─── Central plaza with the warrior statue ───────────────────────────
        // Composite placeholder statue: pedestal + warrior body + bastard sword + round
        // shield + plaque. All shapes are builtin-sprite placeholders to be replaced by art.
        private static void CreateCentralPlaza()
        {
            var plaza = new GameObject("CentralPlaza");
            plaza.transform.position = Vector3.zero;

            // Plaza floor (large light slab under the statue, sorting below everything else)
            var floor = new GameObject("PlazaFloor");
            floor.transform.SetParent(plaza.transform);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(9f, 9f, 1f);
            var floorRenderer = floor.AddComponent<SpriteRenderer>();
            floorRenderer.sprite = GetBuiltinSprite();
            floorRenderer.color = new Color(0.69f, 0.66f, 0.6f);
            floorRenderer.sortingOrder = 0;
            TrySetSortingLayer(floorRenderer, "Ground", floorRenderer.sortingOrder);

            var statue = new GameObject("WarriorStatue");
            statue.transform.SetParent(plaza.transform);
            statue.transform.position = Vector3.zero;

            // Pedestal (blocks movement)
            var pedestal = CreateStatuePart(statue.transform, "Pedestal", new Vector3(0f, -0.6f, 0f), new Vector3(2.2f, 0.9f, 1f), new Color(0.45f, 0.45f, 0.5f), 2);
            var pedestalCollider = pedestal.AddComponent<BoxCollider2D>();
            pedestalCollider.isTrigger = false;
            pedestalCollider.size = Vector2.one;

            // Warrior body and head (weathered bronze)
            CreateStatuePart(statue.transform, "WarriorBody", new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 1.6f, 1f), new Color(0.42f, 0.5f, 0.46f), 3);
            CreateStatuePart(statue.transform, "WarriorHead", new Vector3(0f, 1.55f, 0f), new Vector3(0.5f, 0.5f, 1f), new Color(0.46f, 0.54f, 0.5f), 3);

            // Bastard sword raised in the right hand (long thin blade + crossguard)
            var sword = CreateStatuePart(statue.transform, "BastardSwordBlade", new Vector3(0.75f, 1.25f, 0f), new Vector3(0.14f, 2.3f, 1f), new Color(0.72f, 0.76f, 0.82f), 4);
            sword.transform.rotation = Quaternion.Euler(0f, 0f, -12f);
            var crossguard = CreateStatuePart(statue.transform, "BastardSwordCrossguard", new Vector3(0.66f, 0.45f, 0f), new Vector3(0.55f, 0.12f, 1f), new Color(0.55f, 0.5f, 0.35f), 4);
            crossguard.transform.rotation = Quaternion.Euler(0f, 0f, -12f);

            // Round shield resting on the left arm
            CreateStatuePart(statue.transform, "Shield", new Vector3(-0.7f, 0.5f, 0f), new Vector3(0.85f, 0.95f, 1f), new Color(0.5f, 0.38f, 0.28f), 4);
            CreateStatuePart(statue.transform, "ShieldBoss", new Vector3(-0.7f, 0.5f, 0f), new Vector3(0.3f, 0.35f, 1f), new Color(0.7f, 0.66f, 0.5f), 5);

            // Plaque honoring the founder of Cindar's Hope
            CreateStatuePart(statue.transform, "Plaque", new Vector3(0f, -1.15f, 0f), new Vector3(1.1f, 0.3f, 1f), new Color(0.75f, 0.68f, 0.4f), 3);

            // Plaza benches and corner planters
            CreateDecoration(plaza.transform, "PlazaBench_N", new Vector3(0f, 3.4f, 0f), new Vector3(1.6f, 0.45f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaBench_S", new Vector3(0f, -3.4f, 0f), new Vector3(1.6f, 0.45f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaBench_E", new Vector3(3.4f, 0f, 0f), new Vector3(0.45f, 1.6f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaBench_W", new Vector3(-3.4f, 0f, 0f), new Vector3(0.45f, 1.6f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaPlanter_NE", new Vector3(3.8f, 3.8f, 0f), new Vector3(0.8f, 0.8f, 1f), new Color(0.3f, 0.52f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaPlanter_NW", new Vector3(-3.8f, 3.8f, 0f), new Vector3(0.8f, 0.8f, 1f), new Color(0.3f, 0.52f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaPlanter_SE", new Vector3(3.8f, -3.8f, 0f), new Vector3(0.8f, 0.8f, 1f), new Color(0.3f, 0.52f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaPlanter_SW", new Vector3(-3.8f, -3.8f, 0f), new Vector3(0.8f, 0.8f, 1f), new Color(0.3f, 0.52f, 0.26f));

            // fable_37 — âncora das barracas de festival (ponto de extensão da praça). Empty marcado com
            // FestivalStallAnchor: o WorldEventService monta/desmonta aqui as 3 barracas temporárias no
            // dia de festival. Sem geometria — apenas o ponto de spawn (ADITIVO; não altera o layout).
            var festivalAnchor = new GameObject("FestivalStallAnchor");
            festivalAnchor.transform.SetParent(plaza.transform);
            festivalAnchor.transform.position = new Vector3(0f, 2.6f, 0f);
            festivalAnchor.AddComponent<CindarsHope.World.Events.FestivalStallAnchor>();
        }

        private static GameObject CreateStatuePart(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color, int sortingOrder)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;

            var renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBuiltinSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            TrySetSortingLayer(renderer, "Items", renderer.sortingOrder);
            return part;
        }

        // ─── Market stalls: one per shop NPC, placed just behind the vendor ──
        private static void CreateMarketStalls()
        {
            var parent = new GameObject("MarketStalls");
            parent.transform.position = Vector3.zero;

            foreach (var spec in RefinedCanonicalTownNpcSpecs)
            {
                if (string.IsNullOrWhiteSpace(spec.ShopDataPath))
                {
                    continue;
                }

                var stall = new GameObject($"Stall_{spec.NpcId}");
                stall.transform.SetParent(parent.transform);
                stall.transform.position = spec.LayoutPosition + new Vector3(0f, 1.15f, 0f);

                // Counter (walkable in front, blocks behind)
                var counter = new GameObject("Counter");
                counter.transform.SetParent(stall.transform);
                counter.transform.localPosition = Vector3.zero;
                counter.transform.localScale = new Vector3(2.1f, 0.6f, 1f);
                var counterRenderer = counter.AddComponent<SpriteRenderer>();
                counterRenderer.sprite = GetBuiltinSprite();
                counterRenderer.color = new Color(0.46f, 0.34f, 0.22f);
                counterRenderer.sortingOrder = 1;
                TrySetSortingLayer(counterRenderer, "Items", counterRenderer.sortingOrder);
                var counterCollider = counter.AddComponent<BoxCollider2D>();
                counterCollider.isTrigger = false;
                counterCollider.size = Vector2.one;

                // Awning tinted with the vendor color so each market is identifiable
                var awning = new GameObject("Awning");
                awning.transform.SetParent(stall.transform);
                awning.transform.localPosition = new Vector3(0f, 0.75f, 0f);
                awning.transform.localScale = new Vector3(2.4f, 0.5f, 1f);
                var awningRenderer = awning.AddComponent<SpriteRenderer>();
                awningRenderer.sprite = GetBuiltinSprite();
                awningRenderer.color = Color.Lerp(spec.Color, Color.white, 0.25f);
                awningRenderer.sortingOrder = 4;
                TrySetSortingLayer(awningRenderer, "Items", awningRenderer.sortingOrder);
            }
        }

        // ─── Houses: simple base+roof+door composites per resident district ──
        private static readonly (string name, Vector3 position, Color baseColor)[] TownHouseSpecs =
        {
            ("House_Temple", new Vector3(-12f, 11.5f, 0f), new Color(0.78f, 0.74f, 0.62f)),
            ("House_Registry", new Vector3(-8.5f, 11.5f, 0f), new Color(0.55f, 0.58f, 0.66f)),
            ("House_MarketRow_A", new Vector3(-4.5f, 9.6f, 0f), new Color(0.55f, 0.42f, 0.3f)),
            ("House_MarketRow_B", new Vector3(0f, 9.6f, 0f), new Color(0.6f, 0.46f, 0.32f)),
            ("House_MarketRow_C", new Vector3(4.5f, 9.6f, 0f), new Color(0.52f, 0.4f, 0.3f)),
            ("House_Inn", new Vector3(8.5f, 9.6f, 0f), new Color(0.62f, 0.5f, 0.36f)),
            ("House_Blacksmith", new Vector3(-14.5f, 4.5f, 0f), new Color(0.4f, 0.34f, 0.3f)),
            ("House_Archive", new Vector3(-11.5f, -5.5f, 0f), new Color(0.48f, 0.42f, 0.6f)),
            ("House_Workshop", new Vector3(9.5f, -6f, 0f), new Color(0.56f, 0.46f, 0.3f)),
            ("House_AnimalYard", new Vector3(14.5f, 10.5f, 0f), new Color(0.46f, 0.56f, 0.4f)),
            ("House_AlchemyLab", new Vector3(14f, 1f, 0f), new Color(0.36f, 0.55f, 0.58f)),
            ("House_GateKeeper", new Vector3(-7.5f, -11f, 0f), new Color(0.42f, 0.46f, 0.56f)),
        };

        private static void CreateHouses()
        {
            var parent = new GameObject("TownHouses");
            parent.transform.position = Vector3.zero;

            // fable_40: every house repositioned into the canonical 48×42 footprint.
            foreach (var (name, position, baseColor) in TownHouseSpecs)
            {
                CreateHouse(parent.transform, name, TownDistrictLayout.Reposition(position), baseColor);
            }
        }

        private static void CreateHouse(Transform parent, string name, Vector3 position, Color baseColor)
        {
            var house = new GameObject(name);
            house.transform.SetParent(parent);
            house.transform.position = position;

            // Walls (blocking)
            var body = new GameObject("Body");
            body.transform.SetParent(house.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(2.6f, 1.8f, 1f);
            var bodyRenderer = body.AddComponent<SpriteRenderer>();
            bodyRenderer.sprite = GetBuiltinSprite();
            bodyRenderer.color = baseColor;
            bodyRenderer.sortingOrder = 2;
            TrySetSortingLayer(bodyRenderer, "Items", bodyRenderer.sortingOrder);
            var bodyCollider = body.AddComponent<BoxCollider2D>();
            bodyCollider.isTrigger = false;
            bodyCollider.size = Vector2.one;

            // Roof
            var roof = new GameObject("Roof");
            roof.transform.SetParent(house.transform);
            roof.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            roof.transform.localScale = new Vector3(3f, 0.7f, 1f);
            var roofRenderer = roof.AddComponent<SpriteRenderer>();
            roofRenderer.sprite = GetBuiltinSprite();
            roofRenderer.color = Color.Lerp(baseColor, new Color(0.5f, 0.18f, 0.12f), 0.6f);
            roofRenderer.sortingOrder = 3;
            TrySetSortingLayer(roofRenderer, "Items", roofRenderer.sortingOrder);

            // Door
            var door = new GameObject("Door");
            door.transform.SetParent(house.transform);
            door.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            door.transform.localScale = new Vector3(0.5f, 0.75f, 1f);
            var doorRenderer = door.AddComponent<SpriteRenderer>();
            doorRenderer.sprite = GetBuiltinSprite();
            doorRenderer.color = new Color(0.28f, 0.2f, 0.14f);
            doorRenderer.sortingOrder = 3;
            TrySetSortingLayer(doorRenderer, "Items", doorRenderer.sortingOrder);
        }

        // ─── fable_11: schedule anchors (work/social/home per NPC) ───────────────────────────────
        // Canonical social hubs by archetype: tavern (Gruta's corner), plaza center, night market.
        // Legacy authoring coordinates — repositioned into the 48×42 footprint at use (fable_40).
        private static readonly Vector3 TavernSocialAnchorLegacy = new Vector3(9.5f, 5.2f, 0f);
        private static readonly Vector3 PlazaSocialAnchorLegacy = new Vector3(0f, -1.5f, 0f);
        private static readonly Vector3 NightMarketAnchorLegacy = new Vector3(11f, -9.5f, 0f);

        private static void CreateNpcScheduleAnchors()
        {
            var parent = new GameObject("NpcScheduleAnchors");
            parent.transform.position = Vector3.zero;

            var houseDoorPositions = HouseDoorPositions();
            int houseCount = houseDoorPositions.Count;
            int index = 0;

            foreach (var spec in RefinedCanonicalTownNpcSpecs)
            {
                var archetype = NpcScheduleBlockResolver.ArchetypeFromMovementProfile(
                    spec.MovementProfile, !string.IsNullOrWhiteSpace(spec.ShopDataPath));

                // Work = current stall/post position (canonical 48×42 footprint).
                CreateScheduleAnchor(parent.transform, spec.NpcId,
                    NpcScheduleBlockResolver.WorkAnchorSuffix, spec.LayoutPosition);

                // Social = archetype-appropriate hub (repositioned into the 48×42 footprint).
                // The archetype branch keys off the legacy authoring grid (classification only).
                Vector3 socialLegacy = archetype == NpcScheduleArchetype.Night ? NightMarketAnchorLegacy
                    : (spec.Position.y > 3f ? TavernSocialAnchorLegacy : PlazaSocialAnchorLegacy);
                CreateScheduleAnchor(parent.transform, spec.NpcId,
                    NpcScheduleBlockResolver.SocialAnchorSuffix, TownDistrictLayout.Reposition(socialLegacy));

                // Home = a house door (cycled across the 12 houses), nudged just below the door.
                // houseDoorPositions are already in the 48×42 footprint (HouseDoorPositions).
                var home = houseCount > 0
                    ? houseDoorPositions[index % houseCount] + new Vector3(0f, -0.9f, 0f)
                    : spec.LayoutPosition;
                CreateScheduleAnchor(parent.transform, spec.NpcId,
                    NpcScheduleBlockResolver.HomeAnchorSuffix, home);

                index++;
            }

            Debug.Log($"[fable_11] Created schedule anchors for {index} NPC(s) " +
                      $"({index * 3} anchors: work/social/home).");
        }

        private static void CreateScheduleAnchor(Transform parent, string npcId, string suffix, Vector3 position)
        {
            var anchorId = $"npc_{npcId}_{suffix}";
            var go = new GameObject($"Anchor_{anchorId}");
            go.transform.SetParent(parent);
            go.transform.position = position;

            var anchor = go.AddComponent<NpcScheduleAnchor>();
            var serialized = new SerializedObject(anchor);
            SetSerializedString(serialized, "_anchorId", anchorId);
            SetSerializedString(serialized, "_npcId", npcId);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(anchor);
        }

        // ─── fable_11: minimal interiors (off-playfield band y > +40) + functional house doors ────
        private const float InteriorBandBaseY = 44f;
        private const float InteriorSpacingX = 12f;
        private const float InteriorSpacingY = 9f;
        private const int InteriorsPerRow = 4;

        private static void CreateHouseInteriorsAndDoors()
        {
            var interiorsParent = new GameObject("HouseInteriors");
            interiorsParent.transform.position = Vector3.zero;
            var doorsParent = new GameObject("HouseDoors");
            doorsParent.transform.position = Vector3.zero;

            // Map shop NPCs to their nearest house so a closed-shop door blocks at that house.
            int doorCount = 0;
            for (int i = 0; i < TownHouseSpecs.Length; i++)
            {
                var (houseName, legacyHousePosition, _) = TownHouseSpecs[i];
                // fable_40: the exterior of the house moved into the 48×42 footprint; the
                // interior band (y>+40, fable_11) is preserved unchanged.
                var housePosition = TownDistrictLayout.Reposition(legacyHousePosition);

                // Interior center for this house in the off-playfield band (preserved offsets).
                int row = i / InteriorsPerRow;
                int col = i % InteriorsPerRow;
                var interiorCenter = new Vector3(
                    (col - (InteriorsPerRow - 1) * 0.5f) * InteriorSpacingX,
                    InteriorBandBaseY + row * InteriorSpacingY,
                    0f);

                var exteriorDoorPos = housePosition + new Vector3(0f, -0.55f, 0f);
                var interiorExitPos = interiorCenter + new Vector3(0f, -2.0f, 0f);
                var interiorEntryPos = interiorCenter + new Vector3(0f, -1.4f, 0f);
                var exteriorReturnPos = housePosition + new Vector3(0f, -1.4f, 0f);

                BuildMinimalInterior(interiorsParent.transform, houseName, interiorCenter);

                // Exterior door on the house → teleports into the interior.
                var linkedNpcId = NpcIdNearestHouse(housePosition);
                var isShopDoor = linkedNpcId != null;
                CreateDoor(doorsParent.transform, $"Door_{houseName}_Exterior",
                    exteriorDoorPos, interiorEntryPos, "Entrar", linkedNpcId, isShopDoor);
                doorCount++;

                // Interior return door → teleports back outside.
                CreateDoor(doorsParent.transform, $"Door_{houseName}_Interior",
                    interiorExitPos, exteriorReturnPos, "Sair", null, false);
                doorCount++;
            }

            Debug.Log($"[fable_11] Created {TownHouseSpecs.Length} interiors and {doorCount} paired doors.");
        }

        private static void BuildMinimalInterior(Transform parent, string houseName, Vector3 center)
        {
            var interior = new GameObject($"Interior_{houseName}");
            interior.transform.SetParent(parent);
            interior.transform.position = center;

            // Floor (6x5 tiles) — decorative.
            var floor = new GameObject("Floor");
            floor.transform.SetParent(interior.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(6f, 5f, 1f);
            var floorRenderer = floor.AddComponent<SpriteRenderer>();
            floorRenderer.sprite = GetBuiltinSprite();
            floorRenderer.color = new Color(0.34f, 0.28f, 0.22f);
            floorRenderer.sortingOrder = 0;
            TrySetSortingLayer(floorRenderer, "Items", floorRenderer.sortingOrder);

            // Interior bounds (own colliders so the playfield camera/bounds are unaffected).
            CreateInteriorWall(interior.transform, "Wall_Top", new Vector3(0f, 2.6f, 0f), new Vector2(6.4f, 0.4f));
            CreateInteriorWall(interior.transform, "Wall_Bottom", new Vector3(0f, -2.6f, 0f), new Vector2(6.4f, 0.4f));
            CreateInteriorWall(interior.transform, "Wall_Left", new Vector3(-3.2f, 0f, 0f), new Vector2(0.4f, 5.2f));
            CreateInteriorWall(interior.transform, "Wall_Right", new Vector3(3.2f, 0f, 0f), new Vector2(0.4f, 5.2f));

            // Bed + table placeholders.
            CreateInteriorProp(interior.transform, "Bed", new Vector3(-2f, 1.4f, 0f), new Vector3(1.6f, 1f, 1f), new Color(0.5f, 0.36f, 0.5f));
            CreateInteriorProp(interior.transform, "Table", new Vector3(1.6f, -0.4f, 0f), new Vector3(1.2f, 0.8f, 1f), new Color(0.46f, 0.34f, 0.22f));
        }

        private static void CreateInteriorWall(Transform parent, string name, Vector3 localPos, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.localPosition = localPos;
            var collider = wall.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
            collider.size = size;
        }

        private static void CreateInteriorProp(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
        {
            var prop = new GameObject(name);
            prop.transform.SetParent(parent);
            prop.transform.localPosition = localPos;
            prop.transform.localScale = scale;
            var renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBuiltinSprite();
            renderer.color = color;
            renderer.sortingOrder = 1;
            TrySetSortingLayer(renderer, "Items", renderer.sortingOrder);
        }

        private static void CreateDoor(
            Transform parent,
            string name,
            Vector3 doorPosition,
            Vector3 teleportTarget,
            string label,
            string linkedNpcId,
            bool isShopDoor)
        {
            var door = new GameObject(name);
            door.transform.SetParent(parent);
            door.transform.position = doorPosition;

            var trigger = door.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.8f, 0.9f);

            var interactable = door.AddComponent<DoorInteractable>();
            interactable.Configure(teleportTarget, label, linkedNpcId, isShopDoor);
            EditorUtility.SetDirty(interactable);
        }

        // Door anchor (just below each house body) used as the home schedule anchor target.
        // fable_40: positions are in the canonical 48×42 footprint (repositioned).
        private static List<Vector3> HouseDoorPositions()
        {
            var list = new List<Vector3>(TownHouseSpecs.Length);
            foreach (var (_, position, _) in TownHouseSpecs)
            {
                list.Add(TownDistrictLayout.Reposition(position));
            }
            return list;
        }

        // Nearest shop NPC to a house (within ~4 units) so its door gates by that vendor's hours.
        // fable_40: both sides compared in the relayout footprint; the gate radius scales with
        // the relayout so vendors stay matched to their nearest house after repositioning.
        private static string NpcIdNearestHouse(Vector3 layoutHousePosition)
        {
            string nearestId = null;
            float gate = 4f * TownDistrictLayout.RelayoutScale;
            float bestSqr = gate * gate;
            foreach (var spec in RefinedCanonicalTownNpcSpecs)
            {
                if (string.IsNullOrWhiteSpace(spec.ShopDataPath))
                {
                    continue;
                }

                float sqr = (spec.LayoutPosition - layoutHousePosition).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearestId = spec.NpcId;
                }
            }

            return nearestId;
        }

        private static void SetSerializedString(SerializedObject serialized, string propertyName, string value)
        {
            var prop = serialized.FindProperty(propertyName);
            if (prop != null)
            {
                prop.stringValue = value;
            }
        }

        private static void CreateTownDecorations()
        {
            var decorations = new GameObject("TownDecorations");
            decorations.transform.position = Vector3.zero;

            // fable_40: all town-wide props repositioned into the 48×42 footprint via R().
            Vector3 R(float x, float y) => TownDistrictLayout.Reposition(new Vector3(x, y, 0f));

            CreateDecoration(decorations.transform, "TownWell", R(-5.5f, 4.5f), new Vector3(1.2f, 1.2f, 1f), new Color(0.32f, 0.38f, 0.44f));

            // Street lamps along the main roads (plaza → gates and market row)
            CreateDecoration(decorations.transform, "TownLamp_PlazaN", R(1.2f, 4.6f), new Vector3(0.4f, 1.3f, 1f), new Color(0.83f, 0.66f, 0.31f));
            CreateDecoration(decorations.transform, "TownLamp_PlazaS", R(-1.2f, -4.6f), new Vector3(0.4f, 1.3f, 1f), new Color(0.83f, 0.66f, 0.31f));
            CreateDecoration(decorations.transform, "TownLamp_MarketW", R(-7f, 5.5f), new Vector3(0.4f, 1.3f, 1f), new Color(0.83f, 0.66f, 0.31f));
            CreateDecoration(decorations.transform, "TownLamp_MarketE", R(7f, 5.5f), new Vector3(0.4f, 1.3f, 1f), new Color(0.83f, 0.66f, 0.31f));
            CreateDecoration(decorations.transform, "TownLamp_GateS", R(1.5f, -12f), new Vector3(0.4f, 1.3f, 1f), new Color(0.83f, 0.66f, 0.31f));
            CreateDecoration(decorations.transform, "TownLamp_CaveRoad", R(10.5f, -1f), new Vector3(0.4f, 1.3f, 1f), new Color(0.83f, 0.66f, 0.31f));
            CreateDecoration(decorations.transform, "TownLamp_NightMarket", R(7.5f, -9.5f), new Vector3(0.4f, 1.3f, 1f), new Color(0.6f, 0.45f, 0.75f));

            // Night market tents (Yael's corner)
            CreateDecoration(decorations.transform, "NightMarketTent_A", R(11.5f, -10.5f), new Vector3(1.8f, 1.1f, 1f), new Color(0.3f, 0.26f, 0.5f));
            CreateDecoration(decorations.transform, "NightMarketTent_B", R(13f, -8.5f), new Vector3(1.6f, 1f, 1f), new Color(0.36f, 0.3f, 0.55f));

            // Quarry and construction props
            CreateDecoration(decorations.transform, "QuarryRocks", R(-14.5f, -3.5f), new Vector3(1.6f, 1.1f, 1f), new Color(0.5f, 0.48f, 0.46f));
            CreateDecoration(decorations.transform, "ConstructionPile", R(5.5f, -8f), new Vector3(1.5f, 0.8f, 1f), new Color(0.6f, 0.5f, 0.34f));

            // Animal pen fence (Eiran's yard)
            CreateDecoration(decorations.transform, "AnimalPenFence_N", R(12f, 10f), new Vector3(4f, 0.25f, 1f), new Color(0.52f, 0.4f, 0.26f));
            CreateDecoration(decorations.transform, "AnimalPenFence_S", R(12f, 7f), new Vector3(4f, 0.25f, 1f), new Color(0.52f, 0.4f, 0.26f));

            // ── fable_40 new districts (CA-3): lake/park SW + town hall NE with mural ──
            CreateLakeParkDistrict(decorations.transform);
            CreateTownHallDistrict(decorations.transform);
        }

        // ── fable_40: lake / park district (SW) — water body + park benches (new) ──
        private static void CreateLakeParkDistrict(Transform parent)
        {
            var district = new GameObject("District_LakePark_SW");
            district.transform.SetParent(parent);
            district.transform.position = Vector3.zero;

            // Water body (decorative, blue, large slab under the park).
            var lake = new GameObject("LakeWater");
            lake.transform.SetParent(district.transform);
            lake.transform.position = TownDistrictLayout.LakeCenter;
            lake.transform.localScale = new Vector3(6f, 4.5f, 1f);
            var lakeRenderer = lake.AddComponent<SpriteRenderer>();
            lakeRenderer.sprite = GetBuiltinSprite();
            lakeRenderer.color = new Color(0.27f, 0.45f, 0.62f);
            lakeRenderer.sortingOrder = 0;
            TrySetSortingLayer(lakeRenderer, "Ground", lakeRenderer.sortingOrder);

            CreateDecoration(district.transform, "ParkBench_W", TownDistrictLayout.LakeBenchWest, new Vector3(1.4f, 0.4f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(district.transform, "ParkBench_E", TownDistrictLayout.LakeBenchEast, new Vector3(1.4f, 0.4f, 1f), new Color(0.5f, 0.38f, 0.26f));
        }

        // ── fable_40: town hall district (NE) — building + mural on the wall (new) ──
        // The public board/calendar stays in the plaza (CA-3); the mural moves here.
        private static void CreateTownHallDistrict(Transform parent)
        {
            var district = new GameObject("District_TownHall_NE");
            district.transform.SetParent(parent);
            district.transform.position = Vector3.zero;

            // Town hall building body (blocking).
            var hall = new GameObject("TownHallBuilding");
            hall.transform.SetParent(district.transform);
            hall.transform.position = TownDistrictLayout.TownHallCenter;
            hall.transform.localScale = new Vector3(4.5f, 3.2f, 1f);
            var hallRenderer = hall.AddComponent<SpriteRenderer>();
            hallRenderer.sprite = GetBuiltinSprite();
            hallRenderer.color = new Color(0.6f, 0.58f, 0.52f);
            hallRenderer.sortingOrder = 2;
            TrySetSortingLayer(hallRenderer, "Items", hallRenderer.sortingOrder);
            var hallCollider = hall.AddComponent<BoxCollider2D>();
            hallCollider.isTrigger = false;
            hallCollider.size = Vector2.one;

            // Mural on the south wall of the town hall (F34 anchor — interactable wired later).
            CreateDecoration(district.transform, "TownHallMural", TownDistrictLayout.TownHallMural, new Vector3(3.2f, 0.9f, 1f), new Color(0.7f, 0.55f, 0.4f));
        }

        // Tree clusters along the perimeter, road edges, and district borders.
        private static readonly Vector3[] TownTreePositions =
        {
            // West perimeter
            new Vector3(-16.5f, 11f, 0f), new Vector3(-16f, 6.5f, 0f), new Vector3(-16.5f, -0.5f, 0f),
            new Vector3(-16f, -6f, 0f), new Vector3(-16.5f, -11f, 0f),
            // East perimeter
            new Vector3(16.5f, 11.5f, 0f), new Vector3(16f, 5.5f, 0f), new Vector3(16.5f, -0.5f, 0f),
            new Vector3(16f, -5.5f, 0f), new Vector3(16.5f, -11f, 0f),
            // North band between districts
            new Vector3(-6f, 12.5f, 0f), new Vector3(-1.5f, 12.8f, 0f), new Vector3(2.5f, 12.5f, 0f), new Vector3(6.5f, 12.8f, 0f),
            // South band near the gate road
            new Vector3(-10f, -12.5f, 0f), new Vector3(-3.5f, -12.8f, 0f), new Vector3(4f, -12.5f, 0f), new Vector3(13f, -12.5f, 0f),
            // Inner garden clusters (statue garden + temple path)
            new Vector3(-5.5f, -7.5f, 0f), new Vector3(5.5f, 3.5f, 0f), new Vector3(-5.5f, 2.5f, 0f),
            new Vector3(4.8f, -3.6f, 0f), new Vector3(-10.5f, 5.8f, 0f), new Vector3(9.5f, 0.5f, 0f),
        };

        private static void CreateTownTrees()
        {
            var parent = new GameObject("TownTrees");
            parent.transform.position = Vector3.zero;

            for (int i = 0; i < TownTreePositions.Length; i++)
            {
                CreateTownTree(parent.transform, i, TownTreePositions[i]);
            }
        }

        private static void CreateTownTree(Transform parent, int treeIndex, Vector3 position)
        {
            var treeObject = new GameObject($"TownTree_{treeIndex:00}");
            treeObject.transform.SetParent(parent);
            // fable_40: tree clusters repositioned into the 48×42 footprint.
            treeObject.transform.position = TownDistrictLayout.Reposition(position);
            treeObject.transform.localScale = new Vector3(3f, 3f, 1f);

            var spriteRenderer = treeObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.24f, 0.48f, 0.22f);
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

            var collider = treeObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
            FitBoxColliderToOpaqueSprite(collider, spriteRenderer);
        }

        private sealed class ShopUiReferences
        {
            public DialogueModal DialogueModal;
            public ShopMenuModal ShopMenuModal;
            public BuyPanel BuyPanel;
            public SellPanel SellPanel;
        }

        private static ShopUiReferences CreateShopUi(ModalManager modalManager)
        {
            var canvasObject = new GameObject("ShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);

            new GameObject("ShopEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var dialoguePanel = CreatePanel(canvasObject.transform, "DialogueModal", new Vector2(0f, -195f), new Vector2(1040f, 300f));
            var dialogueText = CreateText(dialoguePanel.transform, "DialogueText", new Vector2(0f, 105f), new Vector2(900f, 56f), string.Empty);
            var continueButton = CreateButton(dialoguePanel.transform, "ContinueButton", new Vector2(0f, -105f), new Vector2(160f, 42f), "Continuar");
            var choicesContainer = CreateContainer(dialoguePanel.transform, "Choices", new Vector2(0f, -5f), new Vector2(760f, 145f));
            var choiceTemplate = CreateButton(choicesContainer, "ChoiceTemplate", Vector2.zero, new Vector2(760f, 30f), string.Empty);
            choiceTemplate.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;
            choiceTemplate.gameObject.SetActive(false);
            var dialogue = dialoguePanel.AddComponent<DialogueModal>();
            var serializedDialogue = new SerializedObject(dialogue);
            SetReference(serializedDialogue, "_canvasGroup", dialoguePanel.GetComponent<CanvasGroup>());
            SetReference(serializedDialogue, "_dialogueText", dialogueText);
            SetReference(serializedDialogue, "_continueButton", continueButton);
            SetReference(serializedDialogue, "_choicesContainer", choicesContainer);
            SetReference(serializedDialogue, "_choiceButtonPrefab", choiceTemplate.gameObject);
            serializedDialogue.ApplyModifiedPropertiesWithoutUndo();

            var menuPanel = CreatePanel(canvasObject.transform, "ShopMenuModal", Vector2.zero, new Vector2(320f, 300f));
            var buyButton = CreateButton(menuPanel.transform, "BuyButton", new Vector2(0f, 75f), new Vector2(240f, 48f), "Comprar");
            var sellButton = CreateButton(menuPanel.transform, "SellButton", new Vector2(0f, 10f), new Vector2(240f, 48f), "Vender");
            var exitButton = CreateButton(menuPanel.transform, "ExitButton", new Vector2(0f, -55f), new Vector2(240f, 48f), "Sair");
            var menu = menuPanel.AddComponent<ShopMenuModal>();
            var serializedMenu = new SerializedObject(menu);
            SetReference(serializedMenu, "_canvasGroup", menuPanel.GetComponent<CanvasGroup>());
            SetReference(serializedMenu, "_buyButton", buyButton);
            SetReference(serializedMenu, "_sellButton", sellButton);
            SetReference(serializedMenu, "_exitButton", exitButton);
            serializedMenu.ApplyModifiedPropertiesWithoutUndo();

            var buyPanel = CreatePanel(canvasObject.transform, "BuyPanel", Vector2.zero, new Vector2(620f, 560f));
            var buyGold = CreateText(buyPanel.transform, "Gold", new Vector2(-205f, 258f), new Vector2(190f, 34f), "Ouro:");
            var buyContainer = CreateScrollableContainer(buyPanel.transform, "ItemsScroll", new Vector2(0f, 72f), new Vector2(574f, 250f));
            var buyDetails = CreateText(buyPanel.transform, "Details", new Vector2(0f, -100f), new Vector2(570f, 105f), "Selecione um item para ver detalhes.");
            buyDetails.fontSize = 15;
            var buyFeedback = CreateText(buyPanel.transform, "Feedback", new Vector2(-72f, -218f), new Vector2(420f, 36f), string.Empty);
            var buyBack = CreateButton(buyPanel.transform, "BackButton", new Vector2(225f, -228f), new Vector2(120f, 40f), "Voltar");
            var buyTemplate = CreateBuyItemTemplate(buyContainer);
            var buy = buyPanel.AddComponent<BuyPanel>();
            var serializedBuy = new SerializedObject(buy);
            SetReference(serializedBuy, "_canvasGroup", buyPanel.GetComponent<CanvasGroup>());
            SetReference(serializedBuy, "_itemsContainer", buyContainer);
            SetReference(serializedBuy, "_itemPrefab", buyTemplate);
            SetReference(serializedBuy, "_goldDisplay", buyGold);
            SetReference(serializedBuy, "_feedbackText", buyFeedback);
            SetReference(serializedBuy, "_detailsText", buyDetails);
            SetReference(serializedBuy, "_backButton", buyBack);
            serializedBuy.ApplyModifiedPropertiesWithoutUndo();

            var sellPanel = CreatePanel(canvasObject.transform, "SellPanel", Vector2.zero, new Vector2(620f, 560f));
            var sellGold = CreateText(sellPanel.transform, "Gold", new Vector2(-205f, 258f), new Vector2(190f, 34f), "Ouro:");
            var sellContainer = CreateScrollableContainer(sellPanel.transform, "ItemsScroll", new Vector2(0f, 72f), new Vector2(574f, 250f));
            var sellDetails = CreateText(sellPanel.transform, "Details", new Vector2(0f, -100f), new Vector2(570f, 105f), "Selecione um item para ver detalhes.");
            sellDetails.fontSize = 15;
            var sellFeedback = CreateText(sellPanel.transform, "Feedback", new Vector2(-72f, -218f), new Vector2(420f, 36f), string.Empty);
            var sellBack = CreateButton(sellPanel.transform, "BackButton", new Vector2(225f, -228f), new Vector2(120f, 40f), "Voltar");
            var sellTemplate = CreateSellItemTemplate(sellContainer);
            var sell = sellPanel.AddComponent<SellPanel>();
            var serializedSell = new SerializedObject(sell);
            SetReference(serializedSell, "_canvasGroup", sellPanel.GetComponent<CanvasGroup>());
            SetReference(serializedSell, "_itemsContainer", sellContainer);
            SetReference(serializedSell, "_itemPrefab", sellTemplate);
            SetReference(serializedSell, "_goldDisplay", sellGold);
            SetReference(serializedSell, "_feedbackText", sellFeedback);
            SetReference(serializedSell, "_detailsText", sellDetails);
            SetReference(serializedSell, "_backButton", sellBack);
            serializedSell.ApplyModifiedPropertiesWithoutUndo();

            modalManager.Initialize();
            return new ShopUiReferences
            {
                DialogueModal = dialogue,
                ShopMenuModal = menu,
                BuyPanel = buy,
                SellPanel = sell
            };
        }

        // District layout v2: NPCs spread across the full playfield (~±16 x ±12) so the town
        // reads as neighborhoods — temple NW, market row N, plaza center, industry W,
        // workshop SE, night market S, cave road E, animal yard NE, gate S.
        private static readonly TownNpcSpec[] RefinedCanonicalTownNpcSpecs =
        {
            // Temple district (NW)
            new("npc_corvus", "NPC_Corvus_Temple", "Assets/_Game/Data/NPCs/Npc_Corvus.asset", "Assets/_Game/Data/Economy/Shop_Corvus.asset", new Vector3(-12f, 9f, 0f), new Color(0.82f, 0.76f, 0.58f), "Stationary/TemplePatrol", false, 2f),
            new("npc_mara", "NPC_Mara_Registry", "Assets/_Game/Data/NPCs/Npc_Mara.asset", "Assets/_Game/Data/Economy/Shop_Mara.asset", new Vector3(-9f, 9f, 0f), new Color(0.44f, 0.56f, 0.72f), "Stationary/RegistryDesk", false, 2f),
            // Market row (N)
            new("npc_tovin", "NPC_Tovin_Registry", "Assets/_Game/Data/NPCs/Npc_Tovin.asset", "Assets/_Game/Data/Economy/Shop_Tovin.asset", new Vector3(-6.5f, 7f, 0f), new Color(0.58f, 0.64f, 0.72f), "Stationary/PermitDesk", false, 2f),
            new("npc_sylveth", "NPC_Sylveth_SeedVendor", "Assets/_Game/Data/NPCs/Npc_Sylveth.asset", "Assets/_Game/Data/Economy/Shop_Sylveth.asset", new Vector3(-3.5f, 7f, 0f), new Color(0.42f, 0.72f, 0.34f), "ShopKeeperFixed/FarmVisit", false, 2f),
            new("npc_renko", "NPC_Renko_GeneralMerchant", "Assets/_Game/Data/NPCs/Npc_Renko.asset", "Assets/_Game/Data/Economy/Shop_Renko.asset", new Vector3(0f, 7f, 0f), new Color(0.86f, 0.72f, 0.34f), "ShopKeeperFixed", false, 2f),
            new("npc_mirela", "NPC_Mirela_Tailor", "Assets/_Game/Data/NPCs/Npc_Mirela.asset", "Assets/_Game/Data/Economy/Shop_Mirela.asset", new Vector3(3.5f, 7f, 0f), new Color(0.82f, 0.48f, 0.62f), "ShopKeeperFixed", false, 2f),
            new("npc_orlan", "NPC_Orlan_Inn", "Assets/_Game/Data/NPCs/Npc_Orlan.asset", "Assets/_Game/Data/Economy/Shop_Orlan.asset", new Vector3(6.5f, 7f, 0f), new Color(0.66f, 0.56f, 0.42f), "ShopKeeperFixed", false, 2f),
            new("npc_gruta", "NPC_Gruta_Tavern", "Assets/_Game/Data/NPCs/Npc_Gruta.asset", "Assets/_Game/Data/Economy/Shop_Gruta.asset", new Vector3(9.5f, 6.5f, 0f), new Color(0.75f, 0.42f, 0.28f), "ShopKeeperFixed/TavernStage", false, 2.5f),
            // Industry / blacksmith / quarry (W)
            new("npc_brumdar", "NPC_Brumdar_Blacksmith", "Assets/_Game/Data/NPCs/Npc_Brumdar.asset", "Assets/_Game/Data/Economy/Shop_Brumdar.asset", new Vector3(-12f, 2.5f, 0f), new Color(0.64f, 0.45f, 0.3f), "ShopKeeperFixed", false, 2f),
            new("npc_dagna", "NPC_Dagna_Quarry", "Assets/_Game/Data/NPCs/Npc_Dagna.asset", "Assets/_Game/Data/Economy/Shop_Dagna.asset", new Vector3(-12.5f, -2.5f, 0f), new Color(0.54f, 0.46f, 0.4f), "Patrol/QuarryRoad", true, 3f),
            new("npc_hund", "NPC_Hund_GuardRoute", "Assets/_Game/Data/NPCs/Npc_Hund.asset", "Assets/_Game/Data/Economy/Shop_Hund.asset", new Vector3(-8f, 0f, 0f), new Color(0.38f, 0.48f, 0.58f), "Patrol/TownRoad", true, 5f),
            new("npc_thalindra", "NPC_Thalindra_Archive", "Assets/_Game/Data/NPCs/Npc_Thalindra.asset", "Assets/_Game/Data/Economy/Shop_Thalindra.asset", new Vector3(-9f, -5f, 0f), new Color(0.5f, 0.42f, 0.77f), "Stationary/ArchiveDesk", false, 2f),
            // South gate
            new("npc_alaric", "NPC_Alaric_GuardPost", "Assets/_Game/Data/NPCs/Npc_Alaric.asset", string.Empty, new Vector3(-5f, -10f, 0f), new Color(0.36f, 0.46f, 0.72f), "Patrol/TownGate", true, 3.5f),
            new("npc_pip", "NPC_Pip_TownEntrance", "Assets/_Game/Data/NPCs/Npc_Pip_Miudinho.asset", "Assets/_Game/Data/Economy/Shop_Pip.asset", new Vector3(-2.5f, -9f, 0f), new Color(0.38f, 0.72f, 0.86f), "WanderWithinZone", true, 3f),
            // Workshop / construction (SE)
            new("npc_nimble", "NPC_Nimble_Workshop", "Assets/_Game/Data/NPCs/Npc_Nimble.asset", "Assets/_Game/Data/Economy/Shop_Nimble.asset", new Vector3(7f, -5f, 0f), new Color(0.72f, 0.58f, 0.32f), "Patrol/WorkshopDesk", true, 2.5f),
            new("npc_gurd", "NPC_Gurd_ConstructionYard", "Assets/_Game/Data/NPCs/Npc_Gurd.asset", "Assets/_Game/Data/Economy/Shop_Gurd.asset", new Vector3(4f, -6.5f, 0f), new Color(0.62f, 0.36f, 0.32f), "Patrol/HeavyWorkZone", true, 3f),
            // Night market (S)
            new("npc_yael", "NPC_Yael_NightMarket", "Assets/_Game/Data/NPCs/Npc_Yael.asset", "Assets/_Game/Data/Economy/Shop_Yael.asset", new Vector3(10f, -9f, 0f), new Color(0.28f, 0.24f, 0.62f), "NightOnly/WanderHidden", true, 3f),
            new("npc_maelor", "NPC_Maelor_NightRoute", "Assets/_Game/Data/NPCs/Npc_Maelor.asset", string.Empty, new Vector3(0f, -11.5f, 0f), new Color(0.22f, 0.24f, 0.32f), "NightOnly/WanderHidden", true, 4f),
            // Cave road / forest gate / alchemy (E)
            new("npc_zrix", "NPC_Zrix_CaveRoad", "Assets/_Game/Data/NPCs/Npc_Zrix.asset", "Assets/_Game/Data/Economy/Shop_Zrix.asset", new Vector3(12f, -2f, 0f), new Color(0.43f, 0.52f, 0.68f), "Patrol/CaveRoad", true, 3.5f),
            new("npc_savra", "NPC_Savra_ForestGate", "Assets/_Game/Data/NPCs/Npc_Savra.asset", "Assets/_Game/Data/Economy/Shop_Savra.asset", new Vector3(13.5f, 4.5f, 0f), new Color(0.34f, 0.62f, 0.38f), "Patrol/HerbRoute", true, 3f),
            new("npc_ozzra", "NPC_Ozzra_AlchemyLab", "Assets/_Game/Data/NPCs/Npc_Ozzra.asset", "Assets/_Game/Data/Economy/Shop_Ozzra.asset", new Vector3(11f, 2.5f, 0f), new Color(0.32f, 0.7f, 0.75f), "WanderWithinZone/Lab", true, 2.5f),
            // Animal yard (NE)
            new("npc_eiran", "NPC_Eiran_AnimalYard", "Assets/_Game/Data/NPCs/Npc_Eiran.asset", "Assets/_Game/Data/Economy/Shop_Eiran.asset", new Vector3(12f, 8.5f, 0f), new Color(0.44f, 0.68f, 0.42f), "WanderWithinZone/AnimalArea", true, 3f),
            // Statue garden (center)
            new("npc_liora", "NPC_Liora_StatueGarden", "Assets/_Game/Data/NPCs/Npc_Liora.asset", string.Empty, new Vector3(2.5f, -1.5f, 0f), new Color(0.68f, 0.62f, 0.9f), "WanderWithinZone/EveningStage", true, 3f),
        };

        private readonly struct TownNpcSpec
        {
            public TownNpcSpec(string npcId, string objectName, string npcDataPath, string shopDataPath, Vector3 position, Color color, string movementProfile, bool canWander, float wanderRadius)
            {
                NpcId = npcId;
                ObjectName = objectName;
                NpcDataPath = npcDataPath;
                ShopDataPath = shopDataPath;
                Position = position;
                Color = color;
                MovementProfile = movementProfile;
                CanWander = canWander;
                WanderRadius = wanderRadius;
            }

            public string NpcId { get; }
            public string ObjectName { get; }
            public string NpcDataPath { get; }
            public string ShopDataPath { get; }

            /// <summary>Legacy authoring position (36×30 grid). Use <see cref="LayoutPosition"/>
            /// for placement so the element lands in the canonical 48×42 footprint (fable_40).</summary>
            public Vector3 Position { get; }

            /// <summary>Canonical 48×42 position (legacy position repositioned by the district
            /// layout). Single source of truth so no spec reader misses the relayout.</summary>
            public Vector3 LayoutPosition => TownDistrictLayout.Reposition(Position);

            public Color Color { get; }
            public string MovementProfile { get; }
            public bool CanWander { get; }
            public float WanderRadius { get; }
        }

        private static NpcManager CreateNpcs(
            Transform playerTransform,
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            ItemDatabaseSO itemDatabase,
            ShopManager shopManager,
            ModalManager modalManager,
            ShopUiReferences shopUi)
        {
            var parent = new GameObject("NPCs");
            parent.transform.position = Vector3.zero;

            var dialogueNpcs = new List<NpcController>();
            var shopNpcs = new List<NpcShopController>();

            foreach (var spec in RefinedCanonicalTownNpcSpecs)
            {
                GameObject npcObject;
                if (!string.IsNullOrWhiteSpace(spec.ShopDataPath))
                {
                    npcObject = CreateShopNpc(
                        parent.transform,
                        spec.ObjectName,
                        spec.LayoutPosition,
                        spec.Color,
                        spec.NpcDataPath,
                        spec.ShopDataPath,
                        spec.MovementProfile,
                        playerManager,
                        inventoryManager,
                        itemDatabase,
                        shopManager,
                        modalManager,
                        shopUi);

                    var shopController = npcObject.GetComponent<NpcShopController>();
                    if (shopController != null)
                    {
                        shopNpcs.Add(shopController);
                    }
                }
                else
                {
                    npcObject = CreateDialogueNpc(
                        parent.transform,
                        spec.ObjectName,
                        spec.LayoutPosition,
                        spec.Color,
                        spec.NpcDataPath,
                        modalManager,
                        shopUi.DialogueModal,
                        spec.CanWander,
                        spec.MovementProfile,
                        spec.WanderRadius);

                    var dialogueController = npcObject.GetComponent<NpcController>();
                    if (dialogueController != null)
                    {
                        dialogueNpcs.Add(dialogueController);
                    }
                }

                if (spec.NpcId == "npc_pip")
                {
                    var reception = npcObject.AddComponent<PipReceptionController>();
                    var serializedReception = new SerializedObject(reception);
                    SetReference(serializedReception, "_playerTransform", playerTransform);
                    serializedReception.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            var wanderer = CreateDialogueNpc(
                parent.transform,
                "NPC_Vaalara_Wanderer_01",
                TownDistrictLayout.Reposition(new Vector3(-1.5f, 1.5f, 0f)),
                new Color(0.62f, 0.56f, 0.82f),
                "Assets/_Game/Data/NPCs/Npc_Vaalara_Wanderer_01.asset",
                modalManager,
                shopUi.DialogueModal,
                true,
                "WanderWithinZone",
                6f);
            var wandererController = wanderer.GetComponent<NpcController>();
            if (wandererController != null)
            {
                dialogueNpcs.Add(wandererController);
            }

            var manager = parent.AddComponent<NpcManager>();
            var serializedManager = new SerializedObject(manager);
            SetReference(serializedManager, "_dialogueModal", shopUi.DialogueModal);
            SetReference(serializedManager, "_modalManager", modalManager);
            SetReferences(serializedManager, "_npcs", dialogueNpcs.ToArray());
            SetReferences(serializedManager, "_shopNpcs", shopNpcs.ToArray());
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            return manager;
        }

        private static GameObject CreateShopNpc(
            Transform parent,
            string objectName,
            Vector3 position,
            Color color,
            string npcDataPath,
            string shopDataPath,
            string movementProfile,
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            ItemDatabaseSO itemDatabase,
            ShopManager shopManager,
            ModalManager modalManager,
            ShopUiReferences shopUi)
        {
            var npcObject = new GameObject(objectName);
            npcObject.transform.SetParent(parent);
            npcObject.transform.position = position;
            npcObject.transform.localScale = new Vector3(1f, 1.5f, 1f);
            var renderer = npcObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBuiltinSprite();
            renderer.color = color;
            renderer.sortingOrder = 2;
            TrySetSortingLayer(renderer, "Characters", renderer.sortingOrder);
            var collider = npcObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var controller = npcObject.AddComponent<NpcShopController>();
            var serialized = new SerializedObject(controller);
            var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(npcDataPath);
            SetReference(serialized, "_npcData", npcData);
            if (!string.IsNullOrWhiteSpace(shopDataPath))
            {
                SetReference(serialized, "_shopData", AssetDatabase.LoadAssetAtPath<ShopDataSO>(shopDataPath));
            }
            SetReference(serialized, "_playerManager", playerManager);
            SetReference(serialized, "_inventoryManager", inventoryManager);
            SetReference(serialized, "_itemDatabase", itemDatabase);
            SetReference(serialized, "_shopManager", shopManager);
            SetReference(serialized, "_dialogueModal", shopUi.DialogueModal);
            SetReference(serialized, "_shopMenuModal", shopUi.ShopMenuModal);
            SetReference(serialized, "_buyPanel", shopUi.BuyPanel);
            SetReference(serialized, "_sellPanel", shopUi.SellPanel);
            SetReference(serialized, "_modalManager", modalManager);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AddPlacementMarker(npcObject, npcData, movementProfile);
            return npcObject;
        }

        private static GameObject CreateDialogueNpc(
            Transform parent,
            string objectName,
            Vector3 position,
            Color color,
            string npcDataPath,
            ModalManager modalManager,
            DialogueModal dialogueModal,
            bool canWander,
            string movementProfile,
            float wanderRadius = 3f)
        {
            var npcObject = new GameObject(objectName);
            npcObject.transform.SetParent(parent);
            npcObject.transform.position = position;
            npcObject.transform.localScale = new Vector3(1f, 1.5f, 1f);
            var renderer = npcObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBuiltinSprite();
            renderer.color = color;
            renderer.sortingOrder = 2;
            TrySetSortingLayer(renderer, "Characters", renderer.sortingOrder);
            var collider = npcObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var controller = npcObject.AddComponent<NpcController>();
            var serializedController = new SerializedObject(controller);
            var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(npcDataPath);
            SetReference(serializedController, "_npcData", npcData);
            SetReference(serializedController, "_dialogueModal", dialogueModal);
            SetReference(serializedController, "_modalManager", modalManager);
            SetReference(serializedController, "_collider", collider);
            SetReference(serializedController, "_spriteRenderer", renderer);

            if (canWander)
            {
                var body = npcObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                var wanderer = npcObject.AddComponent<NpcWanderer>();
                var serializedWanderer = new SerializedObject(wanderer);
                SetReference(serializedWanderer, "_npcData", npcData);
                SetReference(serializedWanderer, "_rigidbody", body);
                // District-relative wander bounds: each NPC roams around its own home spot
                // instead of one shared central rectangle, clamped to the town playfield.
                // fable_40: clamp to the canonical 48×42 interior (1 tile inside the perimeter).
                float radius = Mathf.Max(1f, wanderRadius);
                float clampX = TownDistrictLayout.HalfWidth - 1f;   // 23
                float clampY = TownDistrictLayout.HalfHeight - 1f;  // 20
                var boundsMin = new Vector2(
                    Mathf.Max(-clampX, position.x - radius),
                    Mathf.Max(-clampY, position.y - radius));
                var boundsMax = new Vector2(
                    Mathf.Min(clampX, position.x + radius),
                    Mathf.Min(clampY, position.y + radius));
                serializedWanderer.FindProperty("_wanderBoundsMin").vector2Value = boundsMin;
                serializedWanderer.FindProperty("_wanderBoundsMax").vector2Value = boundsMax;
                serializedWanderer.ApplyModifiedPropertiesWithoutUndo();
                SetReference(serializedController, "_wanderer", wanderer);
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
            AddPlacementMarker(npcObject, npcData, movementProfile);
            return npcObject;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            panel.GetComponent<Image>().color = new Color(0.09f, 0.1f, 0.13f, 0.96f);
            return panel;
        }

        private static Text CreateText(Transform parent, string name, Vector2 position, Vector2 size, string text)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var label = textObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 18;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.white;
            label.text = text;
            return label;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 position, Vector2 size, string text)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            buttonObject.GetComponent<Image>().color = new Color(0.26f, 0.29f, 0.36f, 1f);
            var label = CreateText(buttonObject.transform, "Label", Vector2.zero, size, text);
            label.alignment = TextAnchor.MiddleCenter;
            return buttonObject.GetComponent<Button>();
        }

        private static Transform CreateContainer(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var containerObject = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            containerObject.transform.SetParent(parent, false);
            var rect = containerObject.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var layout = containerObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            return containerObject.transform;
        }

        private static Transform CreateScrollableContainer(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var viewportObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewportObject.transform.SetParent(parent, false);
            var viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchoredPosition = position;
            viewportRect.sizeDelta = size;
            viewportObject.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.09f, 0.75f);

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);
            var contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 0f);
            var layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewportObject.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;
            return contentObject.transform;
        }

        private static BuyPanelItem CreateBuyItemTemplate(Transform parent)
        {
            var item = CreatePanel(parent, "BuyItemTemplate", Vector2.zero, new Vector2(560f, 48f));
            item.AddComponent<LayoutElement>().preferredHeight = 48f;
            var component = item.AddComponent<BuyPanelItem>();
            var serialized = new SerializedObject(component);
            SetReference(serialized, "_itemNameText", CreateText(item.transform, "Name", new Vector2(-183f, 0f), new Vector2(185f, 40f), string.Empty));
            SetReference(serialized, "_priceText", CreateText(item.transform, "Price", new Vector2(-40f, 0f), new Vector2(62f, 40f), string.Empty));
            SetReference(serialized, "_stockText", CreateText(item.transform, "Stock", new Vector2(50f, 0f), new Vector2(112f, 40f), string.Empty));
            SetReference(serialized, "_amountInput", CreateInputField(item.transform, "Amount", new Vector2(145f, 0f)));
            SetReference(serialized, "_buyButton", CreateButton(item.transform, "Buy", new Vector2(214f, 0f), new Vector2(82f, 36f), "Comprar"));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            item.SetActive(false);
            return component;
        }

        private static SellPanelItem CreateSellItemTemplate(Transform parent)
        {
            var item = CreatePanel(parent, "SellItemTemplate", Vector2.zero, new Vector2(560f, 48f));
            item.AddComponent<LayoutElement>().preferredHeight = 48f;
            var component = item.AddComponent<SellPanelItem>();
            var serialized = new SerializedObject(component);
            SetReference(serialized, "_itemNameText", CreateText(item.transform, "Name", new Vector2(-183f, 0f), new Vector2(185f, 40f), string.Empty));
            SetReference(serialized, "_priceText", CreateText(item.transform, "Price", new Vector2(-40f, 0f), new Vector2(62f, 40f), string.Empty));
            SetReference(serialized, "_amountText", CreateText(item.transform, "AmountOwned", new Vector2(50f, 0f), new Vector2(112f, 40f), string.Empty));
            SetReference(serialized, "_amountInput", CreateInputField(item.transform, "Amount", new Vector2(145f, 0f)));
            SetReference(serialized, "_sellButton", CreateButton(item.transform, "Sell", new Vector2(214f, 0f), new Vector2(82f, 36f), "Vender"));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            item.SetActive(false);
            return component;
        }

        private static InputField CreateInputField(Transform parent, string name, Vector2 position)
        {
            var fieldObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
            fieldObject.transform.SetParent(parent, false);
            var rect = fieldObject.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(55f, 36f);
            fieldObject.GetComponent<Image>().color = Color.white;
            var value = CreateText(fieldObject.transform, "Value", Vector2.zero, new Vector2(48f, 32f), "1");
            value.color = Color.black;
            value.alignment = TextAnchor.MiddleCenter;
            var field = fieldObject.GetComponent<InputField>();
            field.textComponent = value;
            field.text = "1";
            field.contentType = InputField.ContentType.IntegerNumber;
            return field;
        }

        private static void CreateBuyItemPoint(
            Transform parent,
            string name,
            Vector3 position,
            Color color,
            string sourceId,
            string itemId,
            int amount,
            int totalCost,
            string interactionPrompt)
        {
            var pointObject = CreateCommerceObject(parent, name, position, color);
            var point = pointObject.AddComponent<BuyItemPoint>();
            var serializedPoint = new SerializedObject(point);
            serializedPoint.FindProperty("_sourceId").stringValue = sourceId;
            serializedPoint.FindProperty("_itemId").stringValue = itemId;
            serializedPoint.FindProperty("_amount").intValue = amount;
            serializedPoint.FindProperty("_totalCost").intValue = totalCost;
            serializedPoint.FindProperty("_interactionPrompt").stringValue = interactionPrompt;
            SetReference(serializedPoint, "_spriteRenderer", pointObject.GetComponent<SpriteRenderer>());
            SetReference(serializedPoint, "_collider", pointObject.GetComponent<Collider2D>());
            serializedPoint.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(point);
        }

        private static void CreateSellAllPoint(
            Transform parent,
            string name,
            Vector3 position,
            Color color,
            string sourceId,
            string interactionPrompt)
        {
            var pointObject = CreateCommerceObject(parent, name, position, color);
            var point = pointObject.AddComponent<SellAllPoint>();
            var serializedPoint = new SerializedObject(point);
            serializedPoint.FindProperty("_sourceId").stringValue = sourceId;
            serializedPoint.FindProperty("_interactionPrompt").stringValue = interactionPrompt;
            SetReference(serializedPoint, "_spriteRenderer", pointObject.GetComponent<SpriteRenderer>());
            SetReference(serializedPoint, "_collider", pointObject.GetComponent<Collider2D>());
            serializedPoint.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(point);
        }

        private static GameObject CreateCommerceObject(Transform parent, string name, Vector3 position, Color color)
        {
            var pointObject = new GameObject(name);
            pointObject.transform.SetParent(parent);
            pointObject.transform.position = position;
            pointObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var spriteRenderer = pointObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"{name} placeholder SpriteRenderer was created without a sprite. Replace it with shop art in a future art PR.");
            }

            var collider = pointObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            return pointObject;
        }

        private static void AddPlacementMarker(GameObject npcObject, NpcDataSO npcData, string movementProfile)
        {
            if (npcObject == null || npcData == null)
            {
                return;
            }

            var marker = npcObject.AddComponent<NpcScenePlacementMarker>();
            marker.Configure(
                npcData.NpcId,
                string.IsNullOrWhiteSpace(npcData.DefaultSceneId) ? "TownScene" : npcData.DefaultSceneId,
                npcData.DefaultPositionId,
                movementProfile,
                true);
            EditorUtility.SetDirty(marker);
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

        private static void CreateDecoration(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var decoration = new GameObject(name);
            decoration.transform.SetParent(parent);
            decoration.transform.position = position;
            decoration.transform.localScale = scale;

            var spriteRenderer = decoration.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 1;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);
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

        private static void SetReferences(SerializedObject serializedObject, string propertyName, params Object[] values)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Serialized field '{propertyName}' was not found on '{serializedObject.targetObject.name}'.");
                return;
            }

            property.arraySize = values.Length;
            for (var index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
        }

        private static Sprite GetBuiltinSprite()
        {
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BuiltinSpritePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Builtin placeholder sprite not found at '{BuiltinSpritePath}'. TownScene placeholders will need sprites assigned manually or by a future art PR.");
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

        private static void CreateSceneRuntimeInstaller(Transform playerTransform)
        {
            var runtimeRefObject = new GameObject("SceneRuntimeReferences");
            runtimeRefObject.transform.position = Vector3.zero;

            var installer = runtimeRefObject.AddComponent<TownSceneRuntimeReferenceInstaller>();
            var serializedInstaller = new SerializedObject(installer);
            SetReference(serializedInstaller, "_playerTransform", playerTransform);
            serializedInstaller.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
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
