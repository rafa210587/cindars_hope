using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using CindarsHope.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    public static class CreateMvpCaveScene
    {
        private const string ScenePath = "Assets/_Game/Scenes/CaveScene.unity";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string EnemySlimeDataPath = "Assets/_Game/Data/Combat/Enemy_Slime.asset";
        private const string BuiltinSpritePath = "UI/Skin/UISprite.psd";

        [MenuItem("CindarsHope/Scenes/Create MVP CaveScene")]
        public static void CreateSceneFromMenu()
        {
            CreateScene();
        }

        public static void CreateScene()
        {
            EnsureFolder("Assets/_Game", "Data");
            EnsureFolder("Assets/_Game/Data", "Combat");
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
            CreateGround();
            CreateBounds();
            CreateCavePortals();
            CreateEnemies(playerTransform);
            CreateEnemyDropSpawner(inventoryManager);
            CreateDebugHud(playerManager, inventoryManager, hungerManager, playerTransform.GetComponent<InteractionSystem>(), timeManager, saveManager);
            CreateSceneRuntimeInstaller(playerTransform);
            CreateMainCamera();

            ConfigureBootstrap(bootstrap, playerTransform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Selection.activeObject = sceneAsset;
            Debug.Log($"MVP CaveScene created at {ScenePath}.");
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
            bootstrapObject.AddComponent<FoodConsumer>();
            bootstrapObject.AddComponent<SaveInput>();
            bootstrapObject.AddComponent<EconomyManager>();

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
            SetReference(serializedBootstrap, "_economyManager", bootstrapObject.GetComponent<EconomyManager>());

            ConfigureSaveManager(
                bootstrapObject.GetComponent<SaveManager>(),
                bootstrapObject.GetComponent<PlayerManager>(),
                bootstrapObject.GetComponent<InventoryManager>(),
                bootstrapObject.GetComponent<HungerManager>(),
                bootstrapObject.GetComponent<TimeManager>(),
                playerTransform);
            ConfigureSaveInput(bootstrapObject.GetComponent<SaveInput>(), bootstrapObject.GetComponent<SaveManager>());
            ConfigureFoodConsumer(bootstrapObject.GetComponent<FoodConsumer>(), bootstrapObject.GetComponent<InventoryManager>(), bootstrapObject.GetComponent<HungerManager>());

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
            }
            else
            {
                Debug.LogWarning($"ItemDatabaseSO not found at {ItemDatabasePath}. Assign it manually on CaveScene GameBootstrap.");
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
            Transform playerTransform)
        {
            var serializedSave = new SerializedObject(saveManager);
            SetReference(serializedSave, "_playerManager", playerManager);
            SetReference(serializedSave, "_inventoryManager", inventoryManager);
            SetReference(serializedSave, "_hungerManager", hungerManager);
            SetReference(serializedSave, "_timeManager", timeManager);
            SetReference(serializedSave, "_playerTransform", playerTransform);
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
        }

        private static void ConfigurePlayerAttackController(PlayerAttackController playerAttackController)
        {
            var serializedAttack = new SerializedObject(playerAttackController);
            serializedAttack.FindProperty("_punchDamage").intValue = 1;
            serializedAttack.FindProperty("_punchRange").floatValue = 0.8f;
            serializedAttack.FindProperty("_attackCooldownSeconds").floatValue = 0.4f;
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
            rigidbody.isKinematic = true;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var playerController = player.AddComponent<PlayerController>();
            player.AddComponent<PlayerManager>();
            player.AddComponent<InteractionSystem>();
            var playerAttackController = player.AddComponent<PlayerAttackController>();
            var hitFlash = player.AddComponent<HitFlashController>();
            var knockback = player.AddComponent<KnockbackController>();

            ConfigurePlayerController(playerController);
            ConfigurePlayerAttackController(playerAttackController);
            ConfigureHitFlashController(hitFlash, spriteRenderer, new Color(1f, 1f, 0f));
            ConfigureKnockbackController(knockback, rigidbody);

            return player.transform;
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

        private static void CreateMainCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.5f;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        }

        private static void CreateEnemies(Transform playerTransform)
        {
            EnsureEnemySlimeData();
            CreateSlime(new Vector3(2f, 0f, 0f), playerTransform);
        }

        private static void EnsureEnemySlimeData()
        {
            var existingData = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(EnemySlimeDataPath);
            if (existingData != null)
            {
                return;
            }

            var slimeData = ScriptableObject.CreateInstance<EnemyDataSO>();
            slimeData.enemyId = "enemy_slime";
            slimeData.maxHp = 10;
            slimeData.contactDamage = 1;
            slimeData.contactDamageCooldownSeconds = 1f;
            slimeData.dropItemId = "item_wood";
            slimeData.dropAmount = 1;

            AssetDatabase.CreateAsset(slimeData, EnemySlimeDataPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created EnemyDataSO at {EnemySlimeDataPath}.");
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
            rigidbody.isKinematic = true;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var healthComponent = slimeObject.AddComponent<EnemyHealth>();
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

        private static void CreateSceneRuntimeInstaller(Transform playerTransform)
        {
            var runtimeRefObject = new GameObject("SceneRuntimeReferences");
            runtimeRefObject.transform.position = Vector3.zero;

            var installer = runtimeRefObject.AddComponent<CaveSceneRuntimeReferenceInstaller>();
            var serializedInstaller = new SerializedObject(installer);
            SetReference(serializedInstaller, "_playerTransform", playerTransform);
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
