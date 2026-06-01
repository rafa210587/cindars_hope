using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Cave;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Resources;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Enemy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Player.Progression;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using CindarsHope.Skills;
using CindarsHope.UI;
using CindarsHope.UI.Modal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using CindarsHope.UI.Hotbar;

namespace CindarsHope.Editor.SceneCreation
{
    public static class CreateMvpCaveScene
    {
        private const string ScenePath = "Assets/_Game/Scenes/CaveScene.unity";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string EnemySlimeDataPath = "Assets/_Game/Data/Combat/Enemy_Slime.asset";
        // SPEC 14A-FIX7: canonical roster lives under Data/Enemies/. The stale duplicate at
        // Data/Combat/Enemy_Meteor_Ooze_King.asset was deleted to fix "Duplicate data Id" in EnemyDatabase.
        private const string EnemyMeteorOozeKingDataPath = "Assets/_Game/Data/Enemies/enemy_meteor_ooze_king.asset";
        private const string CaveGenerationConfigPath = "Assets/_Game/Data/Cave/CaveGenerationConfig_Default.asset";
        private const string ResourceNodeStonePath = "Assets/_Game/Data/Cave/ResourceNode_Stone.asset";
        private const string ResourceNodeCopperPath = "Assets/_Game/Data/Cave/ResourceNode_Copper.asset";
        private const string ResourceNodeCaveRootTreePath = "Assets/_Game/Data/Cave/ResourceNode_CaveRootTree.asset";
        private const string ItemStonePath = "Assets/_Game/Data/Items/Item_Material_Stone.asset";
        private const string ItemCopperOrePath = "Assets/_Game/Data/Items/Item_Ore_Copper.asset";
        private const string CaveBossGateRegistryPath = "Assets/_Game/Data/Cave/CaveBossGateRegistry.asset";
        private const string CaveBossGateLevel15Path = "Assets/_Game/Data/Cave/BossGate_Level15.asset";
        private const string WeaponDatabasePath = "Assets/_Game/Data/Combat/WeaponDatabase.asset";
        private const string SpellDatabasePath = "Assets/_Game/Data/Combat/SpellDatabase.asset";
        private const string StatusEffectDatabasePath = "Assets/_Game/Data/Combat/StatusEffectDatabase.asset";
        private const string BuiltinSpritePath = "UI/Skin/UISprite.psd";

        [MenuItem("CindarsHope/Advanced/Legacy/Scenes/Create MVP CaveScene")]
        public static void CreateSceneFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Cannot create MVP CaveScene during Play Mode. Exit Play Mode and run this menu again.");
                return;
            }

            CreateScene();
        }

        public static void CreateScene()
        {
            EnsureFolder("Assets/_Game", "Data");
            EnsureFolder("Assets/_Game/Data", "Combat");
            EnsureFolder("Assets/_Game/Data", "Cave");
            EnsureFolder("Assets/_Game", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "CaveScene";

            var bootstrap = CreateBootstrap();
            var playerManager = bootstrap.GetComponent<PlayerManager>();
            var inventoryManager = bootstrap.GetComponent<InventoryManager>();
            var timeManager = bootstrap.GetComponent<TimeManager>();
            var hungerManager = bootstrap.GetComponent<HungerManager>();
            var saveManager = bootstrap.GetComponent<SaveManager>();
            var economyManager = bootstrap.GetComponent<EconomyManager>();

            var playerTransform = CreatePlayer();
            CreateCaveSpawnPoints(playerTransform);
            // FIXED: Removing static cave elements to enable procedural generation
            // CreateGround();
            // CreateBounds();
            // CreateCavePortals();
            var equipmentManager = bootstrap.GetComponent<EquipmentManager>();
            var caveRuntime = CreateCaveRuntime(playerTransform, inventoryManager, equipmentManager);
            var debugSkipController = playerTransform.gameObject.GetComponent<CaveDebugLevelSkipController>();
            if (debugSkipController == null)
            {
                debugSkipController = caveRuntime.controller.gameObject.GetComponent<CaveDebugLevelSkipController>();
            }
            // CreateResourceNodes(inventoryManager, equipmentManager, caveRuntime.runManager);
            // CreateEnemies(playerTransform);
            CreateEnemyDropSpawner(inventoryManager);
            CreateDebugHud(playerManager, inventoryManager, hungerManager, playerTransform.GetComponent<InteractionSystem>(), timeManager, saveManager);
            CreateSceneRuntimeInstaller(playerTransform, caveRuntime.runManager, caveRuntime.controller, debugSkipController);
            CreateMainCamera(playerTransform);
            ConfigureBootstrap(bootstrap, playerTransform);
            
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Selection.activeObject = sceneAsset;
            Debug.Log($"MVP CaveScene created at {ScenePath}.");
        }

        private static void ConfigureHotbarDebugInput(HotbarDebugInput hotbarDebugInput, SaveManager saveManager)
        {
            var serializedInput = new SerializedObject(hotbarDebugInput);
            SetReference(serializedInput, "_saveManager", saveManager);
            serializedInput.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hotbarDebugInput);
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
            bootstrapObject.AddComponent<HungerManager>();
            bootstrapObject.AddComponent<StaminaManager>();
            bootstrapObject.AddComponent<GameTimeManager>();
            bootstrapObject.AddComponent<CindarsHope.Player.StatusEffectManager>();
            bootstrapObject.AddComponent<FoodConsumer>();
            bootstrapObject.AddComponent<SaveInput>();
            bootstrapObject.AddComponent<EconomyManager>();
            bootstrapObject.AddComponent<ShopManager>();
            bootstrapObject.AddComponent<EquipmentManager>();
            bootstrapObject.AddComponent<PlayerProgressionManager>();
            bootstrapObject.AddComponent<SkillTreeManager>();
            bootstrapObject.AddComponent<HotbarDebugInput>();
            bootstrapObject.AddComponent<ModalManager>();
            bootstrapObject.AddComponent<BestiaryManager>();
            bootstrapObject.AddComponent<ManaManager>();

            return bootstrap;
        }

        private static void ConfigureBootstrap(GameBootstrap bootstrap, Transform playerTransform)
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
            SetReference(serializedBootstrap, "_statusEffectManager", bootstrapObject.GetComponent<CindarsHope.Player.StatusEffectManager>());
            SetReference(serializedBootstrap, "_modalManager", bootstrapObject.GetComponent<ModalManager>());
            SetReference(serializedBootstrap, "_economyManager", bootstrapObject.GetComponent<EconomyManager>());
            SetReference(serializedBootstrap, "_shopManager", bootstrapObject.GetComponent<ShopManager>());
            SetReference(serializedBootstrap, "_equipmentManager", bootstrapObject.GetComponent<EquipmentManager>());
            SetReference(serializedBootstrap, "_progressionManager", bootstrapObject.GetComponent<PlayerProgressionManager>());
            SetReference(serializedBootstrap, "_skillTreeManager", bootstrapObject.GetComponent<SkillTreeManager>());
            SetReference(serializedBootstrap, "_bestiaryManager", bootstrapObject.GetComponent<BestiaryManager>());
            SetReference(serializedBootstrap, "_manaManager", bootstrapObject.GetComponent<ManaManager>());
            PlayerNeedsDataInitializer.ConfigureRuntimeManagers(bootstrap, bootstrapObject.GetComponent<TimeManager>(), bootstrapObject.GetComponent<ModalManager>());

            ConfigureSaveManager(
                bootstrapObject.GetComponent<SaveManager>(),
                bootstrapObject.GetComponent<PlayerManager>(),
                bootstrapObject.GetComponent<InventoryManager>(),
                bootstrapObject.GetComponent<HungerManager>(),
                bootstrapObject.GetComponent<TimeManager>(),
                playerTransform,
                bootstrapObject.GetComponent<SkillTreeManager>(),
                bootstrapObject.GetComponent<ShopManager>());
            ConfigureSaveInput(bootstrapObject.GetComponent<SaveInput>(), bootstrapObject.GetComponent<SaveManager>());
            ConfigureHotbarDebugInput(
                bootstrapObject.GetComponent<HotbarDebugInput>(),
                bootstrapObject.GetComponent<SaveManager>());
            ConfigureFoodConsumer(bootstrapObject.GetComponent<FoodConsumer>(), bootstrapObject.GetComponent<InventoryManager>(), bootstrapObject.GetComponent<HungerManager>());
                bootstrapObject.GetComponent<SaveManager>().RebindOptionalRuntimeManagers(
                bootstrapObject.GetComponent<EquipmentManager>(),
                bootstrapObject.GetComponent<PlayerProgressionManager>(),
                bootstrapObject.GetComponent<GameTimeManager>(),
                bootstrapObject.GetComponent<StaminaManager>(),
                bootstrapObject.GetComponent<CindarsHope.Player.StatusEffectManager>(),
                bootstrapObject.GetComponent<SkillTreeManager>(),
                bootstrapObject.GetComponent<ShopManager>(),
                bootstrapObject.GetComponent<BestiaryManager>());

            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData != null)
            {
                SetReference(serializedBootstrap, "_playerData", playerData);
                ConfigureHungerManager(bootstrapObject.GetComponent<HungerManager>(), playerData, bootstrapObject.GetComponent<PlayerManager>(), playerTransform);
            }
            else
            {
                Debug.LogWarning($"PlayerDataSO not found at {PlayerDataPath}. Assign it manually on CaveScene GameBootstrap.");
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
                Debug.LogWarning($"ItemDatabaseSO not found at {ItemDatabasePath}. Assign it manually on CaveScene GameBootstrap.");
            }

            var weaponDatabase = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(WeaponDatabasePath);
            if (weaponDatabase != null)
            {
                SetReference(serializedBootstrap, "_weaponDatabase", weaponDatabase);
            }
            else
            {
                Debug.LogWarning($"WeaponDatabaseSO not found at {WeaponDatabasePath}. Assign it manually on CaveScene GameBootstrap.");
            }

            var spellDatabase = AssetDatabase.LoadAssetAtPath<SpellDatabaseSO>(SpellDatabasePath);
            if (spellDatabase != null)
            {
                SetReference(serializedBootstrap, "_spellDatabase", spellDatabase);
            }
            else
            {
                Debug.LogWarning($"SpellDatabaseSO not found at {SpellDatabasePath}. Assign it manually on CaveScene GameBootstrap.");
            }

            var statusEffectDatabase = AssetDatabase.LoadAssetAtPath<StatusEffectDatabaseSO>(StatusEffectDatabasePath);
            if (statusEffectDatabase != null)
            {
                SetReference(serializedBootstrap, "_statusEffectDatabase", statusEffectDatabase);
            }
            else
            {
                Debug.LogWarning($"StatusEffectDatabaseSO not found at {StatusEffectDatabasePath}. Create it or assign it manually on CaveScene GameBootstrap.");
            }

            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
        }

        private static void ConfigureSaveManager(
            SaveManager saveManager,
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            HungerManager hungerManager,
            TimeManager timeManager,
            Transform playerTransform,
            SkillTreeManager skillTreeManager,
            ShopManager shopManager)
        {
            var serializedSave = new SerializedObject(saveManager);
            SetReference(serializedSave, "_playerManager", playerManager);
            SetReference(serializedSave, "_inventoryManager", inventoryManager);
            SetReference(serializedSave, "_hungerManager", hungerManager);
            SetReference(serializedSave, "_timeManager", timeManager);
            SetReference(serializedSave, "_playerTransform", playerTransform);
            SetReference(serializedSave, "_skillTreeManager", skillTreeManager);
            SetReference(serializedSave, "_shopManager", shopManager);
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
            SetReference(serializedConsumer, "_statusEffectManager", foodConsumer.GetComponent<CindarsHope.Player.StatusEffectManager>());
            serializedConsumer.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(foodConsumer);
        }

        private static void ConfigurePlayerController(PlayerController playerController)
        {
            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData != null)
            {
                var serializedController = new SerializedObject(playerController);
                SetReference(serializedController, "_playerData", playerData);
                serializedController.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(playerController);
            }
            else
            {
                Debug.LogWarning($"PlayerDataSO not found at {PlayerDataPath}. Assign it manually on CaveScene Player PlayerController.");
            }

            PlayerNeedsDataInitializer.ConfigurePlayerController(playerController);
        }

        private static void ConfigurePlayerAttackController(PlayerAttackController playerAttackController, PlayerController playerController)
        {
            var serializedAttack = new SerializedObject(playerAttackController);
            SetReference(serializedAttack, "_playerController", playerController);
            serializedAttack.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(playerAttackController);
        }

        private static void ConfigureHitFlashController(HitFlashController hitFlash, SpriteRenderer spriteRenderer, Color flashColor)
        {
            var serializedFlash = new SerializedObject(hitFlash);
            SetReference(serializedFlash, "_spriteRenderer", spriteRenderer);
            serializedFlash.FindProperty("_flashColor").colorValue = flashColor;
            serializedFlash.FindProperty("_flashDuration").floatValue = 0.12f;
            serializedFlash.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hitFlash);
        }

        private static void ConfigureKnockbackController(KnockbackController knockback, Rigidbody2D rigidbody)
        {
            var serializedKnockback = new SerializedObject(knockback);
            SetReference(serializedKnockback, "_rigidbody", rigidbody);
            serializedKnockback.FindProperty("_duration").floatValue = 0.15f;
            serializedKnockback.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(knockback);
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
            TrySetSortingLayer(spriteRenderer, "Player", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("Player placeholder SpriteRenderer was created without a sprite. Replace it with player art in a future art PR.");
            }

            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.6f, 1f);

            var rigidbody = player.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var playerController = player.AddComponent<PlayerController>();
            player.AddComponent<PlayerManager>();

            var interactionSystem = player.AddComponent<InteractionSystem>();
            var interactionTrigger = CreateInteractionTrigger(player.transform, interactionSystem);
            ConfigureInteractionSystem(interactionSystem, interactionTrigger);

            var playerAttackController = player.AddComponent<PlayerAttackController>();
            var hitFlash = player.AddComponent<HitFlashController>();
            var knockback = player.AddComponent<KnockbackController>();

            ConfigurePlayerController(playerController);
            ConfigurePlayerAttackController(playerAttackController, playerController);
            ConfigureHitFlashController(hitFlash, spriteRenderer, new Color(1f, 1f, 0f));
            ConfigureKnockbackController(knockback, rigidbody);

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
            trigger.radius = 0.45f;

            var relay = triggerObject.AddComponent<InteractionTriggerRelay>();
            relay.Configure(interactionSystem);

            return trigger;
        }

        private static void ConfigureInteractionSystem(InteractionSystem interactionSystem, Collider2D interactionTrigger)
        {
            var serializedInteraction = new SerializedObject(interactionSystem);
            SetReference(serializedInteraction, "_interactionTrigger", interactionTrigger);
            serializedInteraction.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactionSystem);
        }

        private static void CreateCaveSpawnPoints(Transform playerTransform)
        {
            var parent = new GameObject("SpawnPoints");
            parent.transform.position = Vector3.zero;

            CreateSpawnPoint(parent.transform, "cave_default", Vector3.zero);
            CreateSpawnPoint(parent.transform, "cave_from_farm", new Vector3(-3f, 0f, 0f));
        }

        private static void CreateSpawnPoint(Transform parent, string spawnId, Vector3 position)
        {
            var spawn = new GameObject(spawnId);
            spawn.transform.SetParent(parent);
            spawn.transform.position = position;
            spawn.SetActive(false);
        }

        private static void CreateGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = Vector3.zero;

            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.15f, 0.15f, 0.15f);
            spriteRenderer.sortingOrder = -1;
            TrySetSortingLayer(spriteRenderer, "Ground", spriteRenderer.sortingOrder);

            var collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(20f, 15f);
            collider.offset = Vector2.zero;

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("Cave Ground placeholder SpriteRenderer was created without a sprite. Replace it with cave ground tilemap in a future art PR.");
            }
        }

        private static void CreateBounds()
        {
            var bounds = new GameObject("Bounds");
            bounds.transform.position = Vector3.zero;

            CreateBound("Top", bounds.transform, new Vector2(0f, 7.5f), new Vector2(18f, 1f));
            CreateBound("Bottom", bounds.transform, new Vector2(0f, -7.5f), new Vector2(18f, 1f));
            CreateBound("Left", bounds.transform, new Vector2(-9.5f, 0f), new Vector2(1f, 14f));
            CreateBound("Right", bounds.transform, new Vector2(9.5f, 0f), new Vector2(1f, 14f));
        }

        private static void CreateBound(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var bound = new GameObject(name);
            bound.transform.SetParent(parent);
            bound.transform.position = position;

            var collider = bound.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static void CreateCavePortals()
        {
            CreatePortal("Portal_Cave_To_Farm", new Vector3(3.5f, 0f, 0f), new Color(0.95f, 0.75f, 0.35f),
                "FarmScene", "Assets/_Game/Scenes/FarmScene.unity", "farm_from_cave", "Ir para a Fazenda");
        }

        private static void CreatePortal(string name, Vector3 position, Color color, string targetSceneName, string targetScenePath, string targetSpawnId, string interactionPrompt)
        {
            var portalObject = new GameObject(name);
            portalObject.transform.position = position;
            portalObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

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

        private static void CreateMainCamera(Transform playerTransform)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.5741177f, 0.5741177f, 0.5741177f);

            var cameraFollow = cameraObject.AddComponent<CindarsHope.Camera.CameraFollow2D>();
            var serializedFollow = new SerializedObject(cameraFollow);
            SetReference(serializedFollow, "_target", playerTransform);
            serializedFollow.FindProperty("_snapOnStart").boolValue = true;
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cameraFollow);
        }

        private static void CreateEnemies(Transform playerTransform)
        {
            EnsureEnemySlimeData();
            CreateSlime(new Vector3(2f, 0f, 0f), playerTransform);
        }

        private static void CreateSlime(Vector3 position, Transform playerTransform)
        {
            var slimeObject = new GameObject("Slime");
            slimeObject.transform.position = position;

            var spriteRenderer = slimeObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.85f, 0.23f, 0.23f);
            spriteRenderer.sortingOrder = 0;
            TrySetSortingLayer(spriteRenderer, "Enemies", spriteRenderer.sortingOrder);

            var collider = slimeObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = false;

            var triggerCollider = new GameObject("TriggerCollider");
            triggerCollider.transform.SetParent(slimeObject.transform);
            triggerCollider.transform.localPosition = Vector3.zero;
            var triggerCollider2D = triggerCollider.AddComponent<CircleCollider2D>();
            triggerCollider2D.radius = 0.6f;
            triggerCollider2D.isTrigger = true;

            var rigidbody = slimeObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var healthComponent = slimeObject.AddComponent<CindarsHope.Combat.EnemyHealth>();
            var serializedHealth = new SerializedObject(healthComponent);

            var enemyData = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(EnemySlimeDataPath);
            if (enemyData != null)
            {
                SetReference(serializedHealth, "_enemyData", enemyData);
            }
            else
            {
                Debug.LogWarning($"EnemyDataSO not found at {EnemySlimeDataPath}. Assign it manually on Slime EnemyHealth.");
            }
            serializedHealth.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(healthComponent);

            var contactDamage = triggerCollider.AddComponent<EnemyContactDamage>();
            var serializedDamage = new SerializedObject(contactDamage);
            SetReference(serializedDamage, "_collider", triggerCollider2D);
            if (enemyData != null)
            {
                SetReference(serializedDamage, "_enemyData", enemyData);
            }
            serializedDamage.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(contactDamage);

            var chaseController = slimeObject.AddComponent<EnemyChaseController>();
            var serializedChase = new SerializedObject(chaseController);
            SetReference(serializedChase, "_target", playerTransform);
            SetReference(serializedChase, "_rigidbody", rigidbody);
            if (enemyData != null)
            {
                serializedChase.FindProperty("_moveSpeed").floatValue = enemyData.moveSpeed;
                serializedChase.FindProperty("_detectionRadius").floatValue = enemyData.detectionRadius;
                serializedChase.FindProperty("_stopDistance").floatValue = enemyData.stopDistance;
            }
            else
            {
                serializedChase.FindProperty("_moveSpeed").floatValue = 1.2f;
                serializedChase.FindProperty("_detectionRadius").floatValue = 5f;
                serializedChase.FindProperty("_stopDistance").floatValue = 0.55f;
            }
            serializedChase.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(chaseController);

            var slimeHitFlash = slimeObject.AddComponent<HitFlashController>();
            if (enemyData != null)
            {
                ConfigureHitFlashController(slimeHitFlash, spriteRenderer, enemyData.hitFlashColor);
            }
            else
            {
                ConfigureHitFlashController(slimeHitFlash, spriteRenderer, new Color(1f, 0.5f, 0f));
            }

            var slimeKnockback = slimeObject.AddComponent<KnockbackController>();
            ConfigureKnockbackController(slimeKnockback, rigidbody);
        }

        private static void CreateEnemyDropSpawner(InventoryManager inventoryManager)
        {
            var dropSpawnerObject = new GameObject("EnemyDropSpawner");
            dropSpawnerObject.transform.position = Vector3.zero;

            var dropSpawner = dropSpawnerObject.AddComponent<EnemyDropSpawner>();
            var serializedDropSpawner = new SerializedObject(dropSpawner);
            SetReference(serializedDropSpawner, "_inventoryManager", inventoryManager);
            serializedDropSpawner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(dropSpawner);
        }

        private static (CaveRunManager runManager, CaveLevelRuntimeController controller) CreateCaveRuntime(
            Transform playerTransform,
            InventoryManager inventoryManager,
            EquipmentManager equipmentManager)
        {
            var runtimeObject = new GameObject("CaveRuntime");
            runtimeObject.transform.position = Vector3.zero;

            var runManager = runtimeObject.AddComponent<CaveRunManager>();
            var checkpointService = runtimeObject.AddComponent<CaveCheckpointService>();
            var materializer = runtimeObject.AddComponent<CaveRuntimeMaterializer>();
            var enemySpawner = runtimeObject.AddComponent<CaveEnemySpawner>();
            var controller = runtimeObject.AddComponent<CaveLevelRuntimeController>();
            var config = EnsureCaveGenerationConfig();

            var bossGateRegistry = EnsureCaveBossGateRegistry();
            var meteorOozeKingData = EnsureMeteorOozeKingEnemyData();

            var serializedRunManager = new SerializedObject(runManager);
            serializedRunManager.FindProperty("_defaultWorldSeed").stringValue = "cindars_world_seed_001";
            serializedRunManager.FindProperty("_currentCaveLevel").intValue = 1;
            serializedRunManager.FindProperty("_deepestLayerReached").intValue = 1;
            if (bossGateRegistry != null)
            {
                SetReference(serializedRunManager, "_bossGateRegistry", bossGateRegistry);
            }
            serializedRunManager.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(runManager);

            var serializedCheckpointService = new SerializedObject(checkpointService);
            SetReference(serializedCheckpointService, "_runManager", runManager);
            serializedCheckpointService.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(checkpointService);

            var resourceNodeDatabase = EnsureResourceNodeDatabase();
            var enemyDatabase = EnsureEnemyDatabase();
            var spawnProfiles = LoadAssets<EnemySpawnProfileSO>("Assets/_Game/Data/EnemySpawn/Profiles");
            var spawnPacks = LoadAssets<EnemySpawnPackSO>("Assets/_Game/Data/EnemySpawn/Packs");
            var factionLocks = LoadAssets<EnemyFactionLockSO>("Assets/_Game/Data/EnemySpawn/FactionLocks");
            if (spawnProfiles.Length == 0 || spawnPacks.Length == 0 || factionLocks.Length == 0)
            {
                Debug.LogWarning(
                    "CreateMvpCaveScene: SPEC 13G spawn assets missing or incomplete. " +
                    $"Profiles={spawnProfiles.Length}, Packs={spawnPacks.Length}, FactionLocks={factionLocks.Length}. " +
                    "Run CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets before Play Mode.");
            }
            var serializedMaterializer = new SerializedObject(materializer);
            SetReference(serializedMaterializer, "_caveRunManager", runManager);
            SetReference(serializedMaterializer, "_inventoryManager", inventoryManager);
            SetReference(serializedMaterializer, "_equipmentManager", equipmentManager);
            if (enemyDatabase != null)
            {
                SetReference(serializedMaterializer, "_enemyDatabase", enemyDatabase);
            }
            SetObjectArray(serializedMaterializer, "_enemySpawnProfiles", spawnProfiles);
            SetObjectArray(serializedMaterializer, "_enemySpawnPacks", spawnPacks);
            SetObjectArray(serializedMaterializer, "_enemyFactionLocks", factionLocks);
            SetReference(serializedMaterializer, "_resourceNodeDatabase", resourceNodeDatabase);
            SetReference(serializedMaterializer, "_playerTransform", playerTransform);
            SetReference(serializedMaterializer, "_levelController", controller);
            serializedMaterializer.FindProperty("_resourceSpawnChance").floatValue = 0.28f;
            serializedMaterializer.FindProperty("_minResourceNodes").intValue = 1;
            serializedMaterializer.FindProperty("_maxResourceNodes").intValue = 4;
            serializedMaterializer.FindProperty("_maxEnemiesPerLevel").intValue = 12;
            serializedMaterializer.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(materializer);

            var fallbackSlimeData = EnsureEnemySlimeData();

            var serializedSpawner = new SerializedObject(enemySpawner);
            if (enemyDatabase != null)
            {
                SetReference(serializedSpawner, "_enemyDatabase", enemyDatabase);
            }
            if (fallbackSlimeData != null)
            {
                SetReference(serializedSpawner, "_fallbackEnemyData", fallbackSlimeData);
            }
            SetReference(serializedSpawner, "_caveRunManager", runManager);
            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(enemySpawner);

            // Create Boss Spawner
            var bossSpawner = runtimeObject.AddComponent<CaveBossSpawner>();
            var serializedBossSpawner = new SerializedObject(bossSpawner);
            if (bossGateRegistry != null)
            {
                SetReference(serializedBossSpawner, "_bossGateRegistry", bossGateRegistry);
            }
            SetReference(serializedBossSpawner, "_caveRunManager", runManager);
            if (enemyDatabase != null)
            {
                SetReference(serializedBossSpawner, "_enemyDatabase", enemyDatabase);
            }
            if (meteorOozeKingData != null)
            {
                SetReference(serializedBossSpawner, "_fallbackEnemyData", meteorOozeKingData);
            }
            serializedBossSpawner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bossSpawner);
            Debug.Log($"CreateMvpCaveScene: CaveBossSpawner configured with registry, boss gate level 15, and enemy data.");

            var serializedController = new SerializedObject(controller);
            SetReference(serializedController, "_runManager", runManager);
            SetReference(serializedController, "_materializer", materializer);
            SetReference(serializedController, "_enemySpawner", enemySpawner);
            SetReference(serializedController, "_bossSpawner", bossSpawner);
            SetReference(serializedController, "_generationConfig", config);
            SetReference(serializedController, "_playerTransform", playerTransform);
            serializedController.FindProperty("_defaultBiomeId").stringValue = "biome_cave_earth";
            serializedController.FindProperty("_logGeneratedLayout").boolValue = true;
            serializedController.FindProperty("_materializeAfterGeneration").boolValue = true;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);

            // Create Debug Level Skip Controller
            var debugSkipController = runtimeObject.AddComponent<CaveDebugLevelSkipController>();
            var serializedDebugSkip = new SerializedObject(debugSkipController);
            SetReference(serializedDebugSkip, "_caveRunManager", runManager);
            SetReference(serializedDebugSkip, "_levelController", controller);
            serializedDebugSkip.FindProperty("_enableDebugLevelSkip").boolValue = true;
            serializedDebugSkip.FindProperty("_nextLevelKey").intValue = (int)KeyCode.P;
            serializedDebugSkip.FindProperty("_alternateNextLevelKey").intValue = (int)KeyCode.F2;
            serializedDebugSkip.FindProperty("_bypassBossGateForDebugSkip").boolValue = true;
            serializedDebugSkip.FindProperty("_showDebugSkipButton").boolValue = true;
            serializedDebugSkip.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(debugSkipController);
            Debug.Log($"CreateMvpCaveScene: CaveDebugLevelSkipController configured on {runtimeObject.name}. Keys=P/F2, Button=enabled, enableDebugLevelSkip=true, bypassBossGate=true.");

            // Create Player Path Confinement
            var pathConfinement = playerTransform.gameObject.AddComponent<CavePlayerPathConfinement>();
            var serializedConfinement = new SerializedObject(pathConfinement);
            SetReference(serializedConfinement, "_playerTransform", playerTransform);
            SetReference(serializedConfinement, "_levelController", controller);
            serializedConfinement.FindProperty("_enableConfinement").boolValue = true;
            serializedConfinement.FindProperty("_horizontalHalfWidth").floatValue = 0.005f;
            serializedConfinement.FindProperty("_verticalHalfHeight").floatValue = 0.08f;
            serializedConfinement.FindProperty("_useLateralSamples").boolValue = false;
            serializedConfinement.FindProperty("_useVerticalSamples").boolValue = true;
            serializedConfinement.FindProperty("_useDiagonalSamples").boolValue = false;
            serializedConfinement.FindProperty("_logFailedSample").boolValue = false;
            serializedConfinement.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pathConfinement);
            Debug.Log($"CreateMvpCaveScene: CavePlayerPathConfinement configured on {playerTransform.gameObject.name}. horizontalHalfWidth=0.005, verticalHalfHeight=0.08, useLateral=false, useVertical=true, useDiagonals=false.");

            return (runManager, controller);
        }

        private static CaveGenerationConfigSO EnsureCaveGenerationConfig()
        {
            var existingConfig = AssetDatabase.LoadAssetAtPath<CaveGenerationConfigSO>(CaveGenerationConfigPath);
            if (existingConfig != null)
            {
                return existingConfig;
            }

            var config = ScriptableObject.CreateInstance<CaveGenerationConfigSO>();
            config.Id = "cave_generation_default";
            config.TargetWidth = 80;
            config.TargetHeight = 48;
            config.MinRooms = 8;
            config.MaxRooms = 14;
            config.MinRoomWidth = 6;
            config.MaxRoomWidth = 14;
            config.MinRoomHeight = 4;
            config.MaxRoomHeight = 10;
            config.ExtraConnectionChancePercent = 20;
            config.EnemyPointCount = 6;
            config.ResourcePointCount = 8;

            AssetDatabase.CreateAsset(config, CaveGenerationConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created CaveGenerationConfigSO at {CaveGenerationConfigPath}.");
            return config;
        }

        private static ResourceNodeDatabaseSO EnsureResourceNodeDatabase()
        {
            const string databasePath = "Assets/_Game/Data/Cave/ResourceNodeDatabase.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ResourceNodeDatabaseSO>(databasePath);
            if (existing != null && existing.All.Count > 0)
            {
                return existing;
            }

            // Ensure resource node data exists
            EnsureCaveResourceData();

            var database = existing ?? ScriptableObject.CreateInstance<ResourceNodeDatabaseSO>();
            database.name = "ResourceNodeDatabase";

            // Load resource node data
            var stoneNode = AssetDatabase.LoadAssetAtPath<ResourceNodeDataSO>(ResourceNodeStonePath);
            var copperNode = AssetDatabase.LoadAssetAtPath<ResourceNodeDataSO>(ResourceNodeCopperPath);
            var caveRootTreeNode = AssetDatabase.LoadAssetAtPath<ResourceNodeDataSO>(ResourceNodeCaveRootTreePath);

            // Add to database if not already present
            var serializedDatabase = new SerializedObject(database);
            var nodesProperty = serializedDatabase.FindProperty("_items");

            if (nodesProperty == null)
            {
                Debug.LogWarning("ResourceNodeDatabaseSO does not have an '_items' property. Skipping resource node population.");
                return database;
            }

            if (stoneNode != null && !ContainsNode(nodesProperty, stoneNode))
            {
                nodesProperty.InsertArrayElementAtIndex(nodesProperty.arraySize);
                nodesProperty.GetArrayElementAtIndex(nodesProperty.arraySize - 1).objectReferenceValue = stoneNode;
            }

            if (copperNode != null && !ContainsNode(nodesProperty, copperNode))
            {
                nodesProperty.InsertArrayElementAtIndex(nodesProperty.arraySize);
                nodesProperty.GetArrayElementAtIndex(nodesProperty.arraySize - 1).objectReferenceValue = copperNode;
            }

            if (caveRootTreeNode != null && !ContainsNode(nodesProperty, caveRootTreeNode))
            {
                nodesProperty.InsertArrayElementAtIndex(nodesProperty.arraySize);
                nodesProperty.GetArrayElementAtIndex(nodesProperty.arraySize - 1).objectReferenceValue = caveRootTreeNode;
            }

            serializedDatabase.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(database, databasePath);
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(database);
            Debug.Log($"ResourceNodeDatabaseSO at {databasePath} now contains {nodesProperty.arraySize} nodes.");
            return database;
        }

        private static bool ContainsNode(SerializedProperty nodesProperty, ResourceNodeDataSO node)
        {
            for (var i = 0; i < nodesProperty.arraySize; i++)
            {
                if (nodesProperty.GetArrayElementAtIndex(i).objectReferenceValue == node)
                {
                    return true;
                }
            }
            return false;
        }

        private static EnemyDatabaseSO EnsureEnemyDatabase()
        {
            const string databasePath = "Assets/_Game/Data/Combat/EnemyDatabase.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDatabaseSO>(databasePath);

            // Ensure enemy slime data exists
            var slimeData = EnsureEnemySlimeData();
            var rosterEnemies = LoadAssets<EnemyDataSO>("Assets/_Game/Data/Enemies/Roster");

            var database = existing ?? ScriptableObject.CreateInstance<EnemyDatabaseSO>();
            if (database == null)
            {
                Debug.LogError("Failed to create or load EnemyDatabase.");
                return null;
            }
            database.name = "EnemyDatabase";

            if (slimeData != null || rosterEnemies.Length > 0)
            {
                var serializedDatabase = new SerializedObject(database);
                var enemiesProperty = serializedDatabase.FindProperty("_items");

                if (enemiesProperty == null)
                {
                    Debug.LogWarning("EnemyDatabase does not have an '_items' property. Skipping enemy population.");
                    return database;
                }

                if (slimeData != null && !ContainsEnemy(enemiesProperty, slimeData))
                {
                    enemiesProperty.InsertArrayElementAtIndex(enemiesProperty.arraySize);
                    enemiesProperty.GetArrayElementAtIndex(enemiesProperty.arraySize - 1).objectReferenceValue = slimeData;
                }

                foreach (var enemy in rosterEnemies)
                {
                    if (enemy != null && !ContainsEnemy(enemiesProperty, enemy))
                    {
                        enemiesProperty.InsertArrayElementAtIndex(enemiesProperty.arraySize);
                        enemiesProperty.GetArrayElementAtIndex(enemiesProperty.arraySize - 1).objectReferenceValue = enemy;
                    }
                }

                serializedDatabase.ApplyModifiedPropertiesWithoutUndo();

                if (existing == null)
                {
                    AssetDatabase.CreateAsset(database, databasePath);
                }

                AssetDatabase.SaveAssets();
                EditorUtility.SetDirty(database);
                Debug.Log($"EnemyDatabase at {databasePath} now contains {enemiesProperty.arraySize} enemies.");
            }
            else
            {
                Debug.LogWarning($"EnemyDatabase could not load Slime data. Cave enemy spawning may not work.");
            }

            return database;
        }

        private static bool ContainsEnemy(SerializedProperty enemiesProperty, EnemyDataSO enemy)
        {
            for (var i = 0; i < enemiesProperty.arraySize; i++)
            {
                if (enemiesProperty.GetArrayElementAtIndex(i).objectReferenceValue == enemy)
                {
                    return true;
                }
            }
            return false;
        }

        private static EnemyDataSO EnsureEnemySlimeData()
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(EnemySlimeDataPath);
            if (existing != null)
            {
                return existing;
            }

            var slimeData = ScriptableObject.CreateInstance<EnemyDataSO>();
            slimeData.enemyId = "enemy_slime_basic";
            slimeData.DisplayName = "Slime";
            slimeData.maxHp = 10;
            slimeData.contactDamage = 1;
            slimeData.contactDamageCooldownSeconds = 1f;
            slimeData.moveSpeed = 1.2f;
            slimeData.detectionRadius = 5f;
            slimeData.stopDistance = 0.55f;
            slimeData.hitFlashColor = Color.red;
            slimeData.hitFlashDuration = 0.12f;
            slimeData.dropItemId = "item_wood";
            slimeData.dropAmount = 1;
            slimeData.enemyLevel = 1;
            slimeData.baseDifficulty = EnemyDifficulty.Easy;

            AssetDatabase.CreateAsset(slimeData, EnemySlimeDataPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created default Slime enemy data at {EnemySlimeDataPath}");
            return slimeData;
        }

        private static void CreateResourceNodes(InventoryManager inventoryManager, EquipmentManager equipmentManager, CaveRunManager runManager)
        {
            EnsureCaveResourceData();

            var parent = new GameObject("ResourceNodes");
            parent.transform.position = Vector3.zero;

            CreateResourceNode(parent.transform, "StoneNode_00", new Vector3(-4f, 1f, 0f), ResourceNodeStonePath, new Color(0.45f, 0.45f, 0.5f), inventoryManager, equipmentManager, runManager);
            CreateResourceNode(parent.transform, "StoneNode_01", new Vector3(-5.5f, -1f, 0f), ResourceNodeStonePath, new Color(0.45f, 0.45f, 0.5f), inventoryManager, equipmentManager, runManager);
            CreateResourceNode(parent.transform, "CopperNode_00", new Vector3(-2.5f, -2f, 0f), ResourceNodeCopperPath, new Color(0.72f, 0.38f, 0.18f), inventoryManager, equipmentManager, runManager);
            CreateResourceNode(parent.transform, "CaveRootTree_00", new Vector3(5f, 1.5f, 0f), ResourceNodeCaveRootTreePath, new Color(0.36f, 0.25f, 0.15f), inventoryManager, equipmentManager, runManager);
        }

        private static void CreateResourceNode(
            Transform parent,
            string name,
            Vector3 position,
            string nodeDataPath,
            Color color,
            InventoryManager inventoryManager,
            EquipmentManager equipmentManager,
            CaveRunManager runManager)
        {
            var nodeObject = new GameObject(name);
            nodeObject.transform.SetParent(parent);
            nodeObject.transform.position = position;

            var spriteRenderer = nodeObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 1;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

            var collider = nodeObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var node = nodeObject.AddComponent<ResourceNode>();
            var nodeData = AssetDatabase.LoadAssetAtPath<ResourceNodeDataSO>(nodeDataPath);
            node.Configure(name, nodeData, inventoryManager, equipmentManager, runManager, spriteRenderer);
            EditorUtility.SetDirty(node);
        }

        private static void EnsureCaveResourceData()
        {
            var stoneItem = EnsureItemData(ItemStonePath, "item_material_stone", "Stone", ItemCategory.Material, 99);
            var copperItem = EnsureItemData(ItemCopperOrePath, "ore_copper", "Copper Ore", ItemCategory.Material, 99);
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (itemDatabase != null)
            {
                EnsureItemInDatabase(itemDatabase, stoneItem);
                EnsureItemInDatabase(itemDatabase, copperItem);
            }

            EnsureResourceNodeData(ResourceNodeStonePath, "resource_stone", "Stone", Tools.ToolType.Pickaxe, Tools.ToolTier.Basic, "item_material_stone", 1, "item_material_stone", 1, false);
            EnsureResourceNodeData(ResourceNodeCopperPath, "resource_copper_ore", "Copper Ore", Tools.ToolType.Pickaxe, Tools.ToolTier.Basic, "ore_copper", 1, "item_material_stone", 1, false);
            EnsureResourceNodeData(ResourceNodeCaveRootTreePath, "resource_cave_root_tree", "Cave Root Tree", Tools.ToolType.Axe, Tools.ToolTier.Basic, "item_wood", 1, string.Empty, 0, false);
        }

        private static ItemDataSO EnsureItemData(string path, string id, string displayName, ItemCategory category, int maxStack)
        {
            var existingItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (existingItem != null)
            {
                return existingItem;
            }

            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.DisplayName = displayName;
            item.Description = $"{displayName} cave debug item.";
            item.Category = category;
            item.MaxStack = maxStack;
            item.BaseValue = 1;
            AssetDatabase.CreateAsset(item, path);
            AssetDatabase.SaveAssets();
            return item;
        }

        private static void EnsureItemInDatabase(ItemDatabaseSO itemDatabase, ItemDataSO item)
        {
            if (itemDatabase == null || item == null)
            {
                return;
            }

            var serializedDatabase = new SerializedObject(itemDatabase);
            var itemsProperty = serializedDatabase.FindProperty("_items");
            for (var i = 0; i < itemsProperty.arraySize; i++)
            {
                if (itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue == item)
                {
                    return;
                }
            }

            itemsProperty.InsertArrayElementAtIndex(itemsProperty.arraySize);
            itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1).objectReferenceValue = item;
            serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureResourceNodeData(
            string path,
            string id,
            string displayName,
            Tools.ToolType requiredTool,
            Tools.ToolTier requiredTier,
            string primaryDropItemId,
            int primaryDropAmount,
            string fallbackItemId,
            int fallbackAmount,
            bool fallbackDepletesNode)
        {
            var existingNode = AssetDatabase.LoadAssetAtPath<ResourceNodeDataSO>(path);
            if (existingNode != null)
            {
                return;
            }

            var node = ScriptableObject.CreateInstance<ResourceNodeDataSO>();
            node.Id = id;
            node.DisplayName = displayName;
            node.RequiredToolType = requiredTool;
            node.RequiredToolTier = requiredTier;
            node.StaminaCost = 1;
            node.HitsRequired = 2;
            node.PrimaryDropItemId = primaryDropItemId;
            node.PrimaryDropAmount = primaryDropAmount;
            node.RespawnsDaily = false;
            node.FallbackItemId = fallbackItemId;
            node.FallbackAmount = fallbackAmount;
            node.FallbackDepletesNode = fallbackDepletesNode;
            AssetDatabase.CreateAsset(node, path);
            AssetDatabase.SaveAssets();
        }

        private static void CreateSceneRuntimeInstaller(Transform playerTransform, CaveRunManager runManager, CaveLevelRuntimeController controller, CaveDebugLevelSkipController debugSkipController = null)
        {
            var runtimeRefObject = new GameObject("SceneRuntimeReferences");
            runtimeRefObject.transform.position = Vector3.zero;

            var installer = runtimeRefObject.AddComponent<CaveSceneRuntimeReferenceInstaller>();
            var serializedInstaller = new SerializedObject(installer);
            SetReference(serializedInstaller, "_playerTransform", playerTransform);
            SetReference(serializedInstaller, "_caveRunManager", runManager);
            SetReference(serializedInstaller, "_caveLevelRuntimeController", controller);
            if (debugSkipController != null)
            {
                SetReference(serializedInstaller, "_caveDebugLevelSkipController", debugSkipController);
            }
            serializedInstaller.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
        }

        private static void CreateDebugHud(PlayerManager playerManager, InventoryManager inventoryManager, HungerManager hungerManager, InteractionSystem interactionSystem, TimeManager timeManager, SaveManager saveManager)
        {
            var hudObject = new GameObject("DebugHud");
            hudObject.transform.position = Vector3.zero;

            var hud = hudObject.AddComponent<DebugHud>();
            var serializedHud = new SerializedObject(hud);
            SetReference(serializedHud, "_playerManager", playerManager);
            SetReference(serializedHud, "_inventoryManager", inventoryManager);
            SetReference(serializedHud, "_hungerManager", hungerManager);
            SetReference(serializedHud, "_interactionSystem", interactionSystem);
            SetReference(serializedHud, "_timeManager", timeManager);
            SetReference(serializedHud, "_saveManager", saveManager);
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hud);
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

        private static void SetObjectArray<T>(SerializedObject serializedObject, string propertyName, T[] values)
            where T : Object
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                Debug.LogWarning($"Property '{propertyName}' not found or not array on {serializedObject.targetObject.name}.");
                return;
            }

            property.arraySize = values?.Length ?? 0;
            for (var i = 0; i < property.arraySize; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static T[] LoadAssets<T>(string folder) where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .OrderBy(a => a.name)
                .ToArray();
        }

        private static Sprite GetBuiltinSprite()
        {
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BuiltinSpritePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Builtin placeholder sprite not found at '{BuiltinSpritePath}'. CaveScene placeholders will need sprites assigned manually or by a future art PR.");
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

        private static CaveBossGateRegistrySO EnsureCaveBossGateRegistry()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CaveBossGateRegistrySO>(CaveBossGateRegistryPath);
            if (existing != null && existing.Gates.Count > 0)
            {
                return existing;
            }

            var registry = ScriptableObject.CreateInstance<CaveBossGateRegistrySO>();
            var level15Gate = EnsureCaveBossGateLevel15();

            if (level15Gate != null)
            {
                var gatesProperty = new SerializedObject(registry).FindProperty("_gates");
                if (gatesProperty != null)
                {
                    if (gatesProperty.arraySize == 0)
                    {
                        gatesProperty.InsertArrayElementAtIndex(0);
                    }
                    gatesProperty.GetArrayElementAtIndex(0).objectReferenceValue = level15Gate;
                    new SerializedObject(registry).ApplyModifiedPropertiesWithoutUndo();
                }
            }

            if (existing == null)
            {
                AssetDatabase.CreateAsset(registry, CaveBossGateRegistryPath);
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(registry);
            Debug.Log($"CaveBossGateRegistry ensured at {CaveBossGateRegistryPath}");
            return registry;
        }

        private static CaveBossGateDataSO EnsureCaveBossGateLevel15()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CaveBossGateDataSO>(CaveBossGateLevel15Path);
            if (existing != null)
            {
                return existing;
            }

            var gate = ScriptableObject.CreateInstance<CaveBossGateDataSO>();
            gate.Id = "boss_gate_level_15";
            gate.CaveLevel = 15;
            gate.BiomeId = "biome_cave_earth";
            gate.BossEnemyId = "enemy_meteor_ooze_king";
            gate.CheckpointUnlockedOnDefeat = 15;

            AssetDatabase.CreateAsset(gate, CaveBossGateLevel15Path);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(gate);
            Debug.Log($"CaveBossGateData ensured at {CaveBossGateLevel15Path}");
            return gate;
        }

        private static EnemyDataSO EnsureMeteorOozeKingEnemyData()
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(EnemyMeteorOozeKingDataPath);
            if (existing != null)
            {
                return existing;
            }

            var bossData = ScriptableObject.CreateInstance<EnemyDataSO>();
            bossData.enemyId = "enemy_meteor_ooze_king";
            bossData.DisplayName = "Meteor Ooze King";
            bossData.maxHp = 30;
            bossData.contactDamage = 3;
            bossData.contactDamageCooldownSeconds = 1f;
            bossData.moveSpeed = 0.8f;
            bossData.detectionRadius = 8f;
            bossData.stopDistance = 1f;
            bossData.hitFlashColor = new Color(1f, 0.5f, 0f);
            bossData.hitFlashDuration = 0.15f;
            bossData.dropItemId = "item_wood";
            bossData.dropAmount = 3;
            bossData.enemyLevel = 15;
            bossData.baseDifficulty = EnemyDifficulty.Hard;

            AssetDatabase.CreateAsset(bossData, EnemyMeteorOozeKingDataPath);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(bossData);
            Debug.Log($"Created Meteor Ooze King enemy data at {EnemyMeteorOozeKingDataPath}");
            return bossData;
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
