using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.NPC;
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
    public static class CreateMvpTownScene
    {
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string FarmScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string RecipeDatabasePath = "Assets/_Game/Data/Registries/RecipeDatabase.asset";
        private const string BuiltinSpritePath = "UI/Skin/UISprite.psd";

        [MenuItem("CindarsHope/Scenes/Create MVP TownScene")]
        public static void CreateSceneFromMenu()
        {
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

            var playerTransform = CreatePlayer();
            var interactionSystem = playerTransform.GetComponent<InteractionSystem>();

            CreateGround();
            CreateBounds();
            CreateMainCamera();
            CreateSpawnPoints(playerTransform);
            CreatePortals();
            CreateNpcs();
            CreateTownCommerce();
            CreateTownDecorations();
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
                economyManager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Selection.activeObject = sceneAsset;
            Debug.Log($"MVP TownScene created at {ScenePath}.");
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
            bootstrapObject.AddComponent<CraftingManager>();
            bootstrapObject.AddComponent<EconomyManager>();

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
            EconomyManager economyManager)
        {
            var serializedBootstrap = new SerializedObject(bootstrap);
            SetReference(serializedBootstrap, "_playerManager", playerManager);
            SetReference(serializedBootstrap, "_inventoryManager", inventoryManager);
            SetReference(serializedBootstrap, "_timeManager", timeManager);
            SetReference(serializedBootstrap, "_saveManager", saveManager);
            SetReference(serializedBootstrap, "_hungerManager", hungerManager);
            SetReference(serializedBootstrap, "_craftingManager", craftingManager);
            SetReference(serializedBootstrap, "_economyManager", economyManager);

            ConfigureDayAdvanceInput(bootstrap.GetComponent<DayAdvanceInput>(), timeManager);
            ConfigureFoodConsumer(bootstrap.GetComponent<FoodConsumer>(), inventoryManager, hungerManager);
            ConfigureSaveInput(bootstrap.GetComponent<SaveInput>(), saveManager);
            ConfigureSaveManager(saveManager, playerManager, inventoryManager, hungerManager, timeManager, playerTransform);
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
            }
            else
            {
                Debug.LogWarning($"ItemDatabaseSO not found at {ItemDatabasePath}. Assign it manually on TownScene GameBootstrap.");
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
            trigger.radius = 0.0005f;

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

        private static void CreateGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, 0f, 1f);
            ground.transform.localScale = new Vector3(18f, 14f, 1f);

            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.48f, 0.54f, 0.6f);
            spriteRenderer.sortingOrder = -10;
            TrySetSortingLayer(spriteRenderer, "Ground", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("Town Ground placeholder SpriteRenderer was created without a sprite. Replace it with tilemap art in a future scene/art PR.");
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

        private static void CreateMainCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.5f;
            camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);
        }

        private static void CreateSpawnPoints(Transform playerTransform)
        {
            var parent = new GameObject("SpawnPoints");
            parent.transform.position = Vector3.zero;

            var defaultSpawn = CreateSpawnPoint(parent.transform, "town_default", new Vector3(0f, -2f, 0f));
            var fromFarmSpawn = CreateSpawnPoint(parent.transform, "town_from_farm", new Vector3(0f, -5f, 0f));

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
            serializedSpawn.FindProperty("_spawnId").stringValue = spawnId;
            serializedSpawn.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawnPoint);
            return spawnPoint;
        }

        private static void CreatePortals()
        {
            var portals = new GameObject("Portals");
            portals.transform.position = Vector3.zero;

            CreateScenePortal(
                portals.transform,
                "Portal_Town_To_Farm",
                new Vector3(0f, -6f, 0f),
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

        private static void CreateTownDecorations()
        {
            var decorations = new GameObject("TownDecorations");
            decorations.transform.position = Vector3.zero;

            CreateDecoration(decorations.transform, "TownWell_Placeholder", new Vector3(-3.5f, 1f, 0f), new Vector3(1.2f, 1.2f, 1f), new Color(0.32f, 0.38f, 0.44f));
            CreateDecoration(decorations.transform, "TownHouse_Placeholder", new Vector3(4f, 2f, 0f), new Vector3(2.2f, 1.6f, 1f), new Color(0.36f, 0.28f, 0.22f));
            CreateDecoration(decorations.transform, "TownLamp_Placeholder", new Vector3(-5f, -2.5f, 0f), new Vector3(0.45f, 1.3f, 1f), new Color(0.83f, 0.66f, 0.31f));
        }

        private static void CreateNpcs()
        {
            var parent = new GameObject("NPCs");
            parent.transform.position = Vector3.zero;

            var npcObject = new GameObject("NPC_Pip_Miudinho");
            npcObject.transform.SetParent(parent.transform);
            npcObject.transform.position = new Vector3(-2f, -0.75f, 0f);
            npcObject.transform.localScale = new Vector3(1f, 1.5f, 1f);

            var spriteRenderer = npcObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.92f, 0.88f, 0.75f);
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Characters", spriteRenderer.sortingOrder);

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning("NPC_Pip_Miudinho placeholder SpriteRenderer was created without a sprite. Replace it with NPC art in a future art PR.");
            }

            var collider = npcObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var talkPoint = npcObject.AddComponent<NpcTalkPoint>();
            var serializedNpc = new SerializedObject(talkPoint);
            serializedNpc.FindProperty("_npcId").stringValue = "npc_pip_miudinho";
            serializedNpc.FindProperty("_displayName").stringValue = "Pip Miudinho";
            serializedNpc.FindProperty("_dialogueLine").stringValue = "Bem-vindo a Cindar's Hope. Ainda estamos abrindo a cidade.";
            SetReference(serializedNpc, "_spriteRenderer", spriteRenderer);
            SetReference(serializedNpc, "_collider", collider);
            serializedNpc.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(talkPoint);
        }

        private static void CreateTownCommerce()
        {
            var parent = new GameObject("TownCommerce");
            parent.transform.position = Vector3.zero;

            CreateBuyItemPoint(
                parent.transform,
                "Shop_Buy_WheatSeeds",
                new Vector3(1.5f, -0.75f, 0f),
                new Color(0.78f, 0.55f, 0.25f),
                "shop_town_seed_wheat",
                "seed_wheat",
                3,
                5,
                "Comprar trigo x3 por 5g");

            CreateBuyItemPoint(
                parent.transform,
                "Shop_Buy_CarrotSeeds",
                new Vector3(3f, -0.75f, 0f),
                new Color(0.9f, 0.45f, 0.18f),
                "shop_town_seed_carrot",
                "seed_carrot",
                2,
                6,
                "Comprar cenoura x2 por 6g");

            CreateSellAllPoint(
                parent.transform,
                "Shop_SellBox",
                new Vector3(4.5f, -0.75f, 0f),
                new Color(0.28f, 0.65f, 0.68f),
                "shop_town_sell_box",
                "Vender itens");

            CreateDecoration(parent.transform, "GeneralStorePlaceholder", new Vector3(3f, 0.75f, 0f), new Vector3(3.75f, 1.1f, 1f), new Color(0.42f, 0.31f, 0.24f));
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
