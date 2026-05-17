using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Economy;
using CindarsHope.Farm;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Save;
using CindarsHope.UI;
using CindarsHope.World;
using CindarsHope.World.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    public static class CreateMvpFarmScene
    {
        private const string ScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string SeedDatabasePath = "Assets/_Game/Data/Registries/SeedDatabase.asset";
        private const string TreeDataPath = "Assets/_Game/Data/World/Trees/Tree_Basic.asset";
        private const string BuiltinSpritePath = "UI/Skin/UISprite.psd";

        [MenuItem("CindarsHope/Scenes/Create MVP FarmScene")]
        public static void CreateSceneFromMenu()
        {
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
            var playerTransform = CreatePlayer();
            CreateGround();
            var farmPlotRegistry = CreateFarmPlots(inventoryManager);
            var treeRegistry = CreateTrees(inventoryManager);
            CreateDebugWoodPickup(inventoryManager);
            CreateSellPoint(inventoryManager, playerManager);
            CreateSeedShopPoint(inventoryManager, playerManager);
            CreateFishingSpot(inventoryManager);
            CreateDebugHud(
                playerManager,
                inventoryManager,
                hungerManager,
                playerTransform.GetComponent<InteractionSystem>(),
                timeManager,
                saveManager);
            CreateBounds();
            CreateMainCamera();

            ConfigureBootstrap(bootstrap, playerTransform, farmPlotRegistry, treeRegistry);

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
            bootstrapObject.AddComponent<FoodConsumer>();
            bootstrapObject.AddComponent<SaveInput>();

            return bootstrap;
        }

        private static void ConfigureBootstrap(
            GameBootstrap bootstrap,
            Transform playerTransform,
            FarmPlotRegistry farmPlotRegistry,
            TreeRegistry treeRegistry)
        {
            var bootstrapObject = bootstrap.gameObject;
            var serializedBootstrap = new SerializedObject(bootstrap);

            SetReference(serializedBootstrap, "_playerManager", bootstrapObject.GetComponent<PlayerManager>());
            SetReference(serializedBootstrap, "_inventoryManager", bootstrapObject.GetComponent<InventoryManager>());
            SetReference(serializedBootstrap, "_timeManager", bootstrapObject.GetComponent<TimeManager>());
            SetReference(serializedBootstrap, "_saveManager", bootstrapObject.GetComponent<SaveManager>());
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
                playerTransform);
            ConfigureSaveInput(bootstrapObject.GetComponent<SaveInput>(), bootstrapObject.GetComponent<SaveManager>());

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
            }
            else
            {
                Debug.LogWarning($"ItemDatabaseSO not found at {ItemDatabasePath}. Assign it manually on GameBootstrap.");
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
            FarmPlotRegistry farmPlotRegistry,
            TreeRegistry treeRegistry,
            Transform playerTransform)
        {
            var serializedSave = new SerializedObject(saveManager);
            SetReference(serializedSave, "_playerManager", playerManager);
            SetReference(serializedSave, "_inventoryManager", inventoryManager);
            SetReference(serializedSave, "_hungerManager", hungerManager);
            SetReference(serializedSave, "_timeManager", timeManager);
            SetReference(serializedSave, "_farmPlotRegistry", farmPlotRegistry);
            SetReference(serializedSave, "_treeRegistry", treeRegistry);
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

        private static void ConfigureDayAdvanceInput(DayAdvanceInput dayAdvanceInput, TimeManager timeManager)
        {
            var serializedInput = new SerializedObject(dayAdvanceInput);
            SetReference(serializedInput, "_timeManager", timeManager);
            serializedInput.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(dayAdvanceInput);
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
            SetSortingLayerIfExists(spriteRenderer, "Characters");

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
            collider.size = new Vector2(0.85f, 0.85f);

            var playerController = player.AddComponent<PlayerController>();
            ConfigurePlayerController(playerController, rigidbody);

            var interactionTrigger = CreateInteractionTrigger(player.transform);
            var interactionSystem = player.AddComponent<InteractionSystem>();
            ConfigureInteractionSystem(interactionSystem, interactionTrigger);

            return player.transform;
        }

        private static CircleCollider2D CreateInteractionTrigger(Transform parent)
        {
            var triggerObject = new GameObject("InteractionTrigger");
            triggerObject.transform.SetParent(parent);
            triggerObject.transform.localPosition = Vector3.zero;
            triggerObject.transform.localRotation = Quaternion.identity;
            triggerObject.transform.localScale = Vector3.one;

            var trigger = triggerObject.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 1.25f;

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
            EditorUtility.SetDirty(playerController);
        }

        private static void ConfigureInteractionSystem(InteractionSystem interactionSystem, Collider2D interactionTrigger)
        {
            var serializedInteraction = new SerializedObject(interactionSystem);
            SetReference(serializedInteraction, "_interactionTrigger", interactionTrigger);
            serializedInteraction.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(interactionSystem);
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
            SetSortingLayerIfExists(spriteRenderer, "Items");

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
            sellPointObject.transform.position = new Vector3(-4.75f, -1.75f, 0f);
            sellPointObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var spriteRenderer = sellPointObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.25f, 0.75f, 0.85f);
            spriteRenderer.sortingOrder = 2;
            SetSortingLayerIfExists(spriteRenderer, "Items");

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
            SetSortingLayerIfExists(spriteRenderer, "Items");

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

        private static void CreateFishingSpot(InventoryManager inventoryManager)
        {
            var fishingObject = new GameObject("FishingSpot");
            fishingObject.transform.position = new Vector3(5.5f, -3f, 0f);
            fishingObject.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

            var spriteRenderer = fishingObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.18f, 0.42f, 0.85f);
            spriteRenderer.sortingOrder = 1;
            SetSortingLayerIfExists(spriteRenderer, "Items");

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("FishingSpot placeholder SpriteRenderer was created without a sprite. Replace it with water/fishing art in a future art PR.");
            }

            var collider = fishingObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var fishingSpot = fishingObject.AddComponent<FishingSpot>();
            var serializedFishing = new SerializedObject(fishingSpot);
            SetReference(serializedFishing, "_inventoryManager", inventoryManager);
            serializedFishing.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(fishingSpot);
        }

        private static void CreateDebugWoodPickup(InventoryManager inventoryManager)
        {
            var pickupObject = new GameObject("DebugWoodPickup");
            pickupObject.transform.position = new Vector3(3.75f, -1.5f, 0f);
            pickupObject.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            var spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.72f, 0.45f, 0.2f);
            spriteRenderer.sortingOrder = 2;
            SetSortingLayerIfExists(spriteRenderer, "Items");

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("DebugWoodPickup placeholder SpriteRenderer was created without a sprite. Replace it with item art in a future art PR.");
            }

            var collider = pickupObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var pickup = pickupObject.AddComponent<ItemPickup>();
            var serializedPickup = new SerializedObject(pickup);
            serializedPickup.FindProperty("_itemId").stringValue = "item_wood";
            serializedPickup.FindProperty("_amount").intValue = 1;
            SetReference(serializedPickup, "_inventoryManager", inventoryManager);
            serializedPickup.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pickup);
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

        private static FarmPlotRegistry CreateFarmPlots(InventoryManager inventoryManager)
        {
            var seedDatabase = AssetDatabase.LoadAssetAtPath<SeedDatabaseSO>(SeedDatabasePath);
            if (seedDatabase == null)
            {
                Debug.LogWarning($"SeedDatabaseSO not found at {SeedDatabasePath}. Assign it manually on FarmPlot objects.");
            }

            var parent = new GameObject("FarmPlots");
            parent.transform.position = Vector3.zero;
            var registry = parent.AddComponent<FarmPlotRegistry>();

            const int gridSize = 3;
            const float spacing = 1.35f;
            var startPosition = new Vector3(-spacing, 2.25f, 0f);
            var plots = new FarmPlot[gridSize * gridSize];

            for (var y = 0; y < gridSize; y++)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    var plotIndex = y * gridSize + x;
                    plots[plotIndex] = CreateFarmPlot(parent.transform, plotIndex, startPosition + new Vector3(x * spacing, -y * spacing, 0f), inventoryManager, seedDatabase);
                }
            }

            registry.Configure(plots);
            EditorUtility.SetDirty(registry);
            return registry;
        }

        private static FarmPlot CreateFarmPlot(Transform parent, int plotIndex, Vector3 position, InventoryManager inventoryManager, SeedDatabaseSO seedDatabase)
        {
            var plotObject = new GameObject($"FarmPlot_{plotIndex:00}");
            plotObject.transform.SetParent(parent);
            plotObject.transform.position = position;
            plotObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

            var spriteRenderer = plotObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.sortingOrder = 1;
            SetSortingLayerIfExists(spriteRenderer, "Ground");

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

            var trees = new TreeNode[3];
            trees[0] = CreateTree(parent.transform, 0, new Vector3(6.5f, 3.5f, 0f), treeData, inventoryManager);
            trees[1] = CreateTree(parent.transform, 1, new Vector3(7.5f, 1.5f, 0f), treeData, inventoryManager);
            trees[2] = CreateTree(parent.transform, 2, new Vector3(6.25f, -0.75f, 0f), treeData, inventoryManager);

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
            treeObject.transform.localScale = new Vector3(1.15f, 1.65f, 1f);

            var spriteRenderer = treeObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.24f, 0.48f, 0.22f);
            spriteRenderer.sortingOrder = 2;
            SetSortingLayerIfExists(spriteRenderer, "Items");

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"{treeObject.name} placeholder SpriteRenderer was created without a sprite. Replace it with tree art in a future art PR.");
            }

            var collider = treeObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

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

        private static void CreateGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, 0f, 1f);
            ground.transform.localScale = new Vector3(20f, 16f, 1f);

            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.78f, 0.64f, 0.39f);
            spriteRenderer.sortingOrder = -10;
            SetSortingLayerIfExists(spriteRenderer, "Ground");

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("Ground placeholder SpriteRenderer was created without a sprite. Replace it with tilemap art in a future scene/art PR.");
            }
        }

        private static void CreateBounds()
        {
            var bounds = new GameObject("Bounds");
            bounds.transform.position = Vector3.zero;

            CreateBound("Top", bounds.transform, new Vector2(0f, 8.5f), new Vector2(20f, 1f));
            CreateBound("Bottom", bounds.transform, new Vector2(0f, -8.5f), new Vector2(20f, 1f));
            CreateBound("Left", bounds.transform, new Vector2(-10.5f, 0f), new Vector2(1f, 16f));
            CreateBound("Right", bounds.transform, new Vector2(10.5f, 0f), new Vector2(1f, 16f));
        }

        private static void CreateBound(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var bound = new GameObject(name);
            bound.transform.SetParent(parent);
            bound.transform.position = position;

            var collider = bound.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static void CreateMainCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            camera.backgroundColor = new Color(0.11f, 0.13f, 0.14f);
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

        private static void SetSortingLayerIfExists(SpriteRenderer spriteRenderer, string sortingLayerName)
        {
            foreach (var layer in SortingLayer.layers)
            {
                if (layer.name != sortingLayerName)
                {
                    continue;
                }

                spriteRenderer.sortingLayerName = sortingLayerName;
                return;
            }

            Debug.LogWarning($"Sorting Layer '{sortingLayerName}' was not found. '{spriteRenderer.gameObject.name}' will use the default sorting layer.");
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
