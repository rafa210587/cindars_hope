using CindarsHope.Core;
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
using CindarsHope.Skills;
using CindarsHope.UI;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Shop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CindarsHope.UI.Hotbar;
using CindarsHope.Equipment;
using CindarsHope.Player.Progression;

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

            CreateGround();
            CreateBounds();
            CreateMainCamera(playerTransform);
            CreateSpawnPoints(playerTransform);
            CreatePortals();
            var npcManager = CreateNpcs(playerTransform, playerManager, inventoryManager, itemDatabase, shopManager, modalManager, shopUi);
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
                economyManager,
                shopManager,
                modalManager,
                npcManager);

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
            SetReference(serializedBootstrap, "_modalManager", modalManager);
            SetReference(serializedBootstrap, "_equipmentManager", bootstrap.GetComponent<EquipmentManager>());
            SetReference(serializedBootstrap, "_progressionManager", bootstrap.GetComponent<PlayerProgressionManager>());
            SetReference(serializedBootstrap, "_skillTreeManager", bootstrap.GetComponent<SkillTreeManager>());
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

            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
            var serializedSave = new SerializedObject(saveManager);
            SetReference(serializedSave, "_shopManager", shopManager);
            SetReference(serializedSave, "_skillTreeManager", bootstrap.GetComponent<SkillTreeManager>());
            serializedSave.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(saveManager);
            saveManager.RebindOptionalRuntimeManagers(
                bootstrap.GetComponent<EquipmentManager>(),
                bootstrap.GetComponent<PlayerProgressionManager>(),
                bootstrap.GetComponent<GameTimeManager>(),
                bootstrap.GetComponent<StaminaManager>(),
                bootstrap.GetComponent<StatusEffectManager>(),
                bootstrap.GetComponent<SkillTreeManager>());
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
            collider.size = new Vector2(0.85f, 0.85f);

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
            trigger.radius = 0.45f;

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

        private static void CreateMainCamera(Transform playerTransform)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.5f;
            camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);

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

            var buyPanel = CreatePanel(canvasObject.transform, "BuyPanel", Vector2.zero, new Vector2(540f, 520f));
            var buyGold = CreateText(buyPanel.transform, "Gold", new Vector2(-260f, 235f), new Vector2(190f, 34f), "Ouro:");
            var buyFeedback = CreateText(buyPanel.transform, "Feedback", new Vector2(0f, -195f), new Vector2(640f, 40f), string.Empty);
            var buyBack = CreateButton(buyPanel.transform, "BackButton", new Vector2(280f, -235f), new Vector2(140f, 40f), "Voltar");
            var buyContainer = CreateContainer(buyPanel.transform, "Items", new Vector2(0f, 15f), new Vector2(680f, 360f));
            var buyTemplate = CreateBuyItemTemplate(buyContainer);
            var buy = buyPanel.AddComponent<BuyPanel>();
            var serializedBuy = new SerializedObject(buy);
            SetReference(serializedBuy, "_canvasGroup", buyPanel.GetComponent<CanvasGroup>());
            SetReference(serializedBuy, "_itemsContainer", buyContainer);
            SetReference(serializedBuy, "_itemPrefab", buyTemplate);
            SetReference(serializedBuy, "_goldDisplay", buyGold);
            SetReference(serializedBuy, "_feedbackText", buyFeedback);
            SetReference(serializedBuy, "_backButton", buyBack);
            serializedBuy.ApplyModifiedPropertiesWithoutUndo();

            var sellPanel = CreatePanel(canvasObject.transform, "SellPanel", Vector2.zero, new Vector2(540f, 520f));
            var sellGold = CreateText(sellPanel.transform, "Gold", new Vector2(-260f, 235f), new Vector2(190f, 34f), "Ouro:");
            var sellFeedback = CreateText(sellPanel.transform, "Feedback", new Vector2(0f, -195f), new Vector2(640f, 40f), string.Empty);
            var sellBack = CreateButton(sellPanel.transform, "BackButton", new Vector2(280f, -235f), new Vector2(140f, 40f), "Voltar");
            var sellContainer = CreateContainer(sellPanel.transform, "Items", new Vector2(0f, 15f), new Vector2(680f, 360f));
            var sellTemplate = CreateSellItemTemplate(sellContainer);
            var sell = sellPanel.AddComponent<SellPanel>();
            var serializedSell = new SerializedObject(sell);
            SetReference(serializedSell, "_canvasGroup", sellPanel.GetComponent<CanvasGroup>());
            SetReference(serializedSell, "_itemsContainer", sellContainer);
            SetReference(serializedSell, "_itemPrefab", sellTemplate);
            SetReference(serializedSell, "_goldDisplay", sellGold);
            SetReference(serializedSell, "_feedbackText", sellFeedback);
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

            var pip = CreateDialogueNpc(
                parent.transform,
                "NPC_Pip_Miudinho",
                new Vector3(-5f, 1f, 0f),
                new Color(0.92f, 0.88f, 0.75f),
                "Assets/_Game/Data/NPCs/Npc_Pip_Miudinho.asset",
                modalManager,
                shopUi.DialogueModal,
                false);
            var reception = pip.AddComponent<PipReceptionController>();
            var serializedReception = new SerializedObject(reception);
            SetReference(serializedReception, "_playerTransform", playerTransform);
            serializedReception.ApplyModifiedPropertiesWithoutUndo();

            var weaponsShop = CreateShopNpc(
                parent.transform,
                "NPC_WeaponsArmorShop",
                new Vector3(-3f, 2f, 0f),
                new Color(0.64f, 0.45f, 0.3f),
                "Assets/_Game/Data/NPCs/Npc_Shop_Weapons_Armor.asset",
                "Assets/_Game/Data/Economy/Shop_Weapons_Armor.asset",
                playerManager,
                inventoryManager,
                itemDatabase,
                shopManager,
                modalManager,
                shopUi);
            var seedsShop = CreateShopNpc(
                parent.transform,
                "NPC_SeedsToolsShop",
                new Vector3(4.5f, 2f, 0f),
                new Color(0.42f, 0.72f, 0.34f),
                "Assets/_Game/Data/NPCs/Npc_Shop_Seeds_Tools.asset",
                "Assets/_Game/Data/Economy/Shop_Seeds_Tools.asset",
                playerManager,
                inventoryManager,
                itemDatabase,
                shopManager,
                modalManager,
                shopUi);

            var wanderer = CreateDialogueNpc(
                parent.transform,
                "NPC_Vaalara_Wanderer_01",
                new Vector3(0f, -1.5f, 0f),
                new Color(0.62f, 0.56f, 0.82f),
                "Assets/_Game/Data/NPCs/Npc_Vaalara_Wanderer_01.asset",
                modalManager,
                shopUi.DialogueModal,
                true);

            var manager = parent.AddComponent<NpcManager>();
            var serializedManager = new SerializedObject(manager);
            SetReference(serializedManager, "_dialogueModal", shopUi.DialogueModal);
            SetReference(serializedManager, "_modalManager", modalManager);
            SetReferences(serializedManager, "_npcs", pip.GetComponent<NpcController>(), wanderer.GetComponent<NpcController>());
            SetReferences(serializedManager, "_shopNpcs", weaponsShop.GetComponent<NpcShopController>(), seedsShop.GetComponent<NpcShopController>());
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            return manager;
        }

        private static void CreateTownCommerce()
        {
            var parent = new GameObject("TownCommerce");
            parent.transform.position = Vector3.zero;
            CreateDecoration(parent.transform, "WeaponsStorePlaceholder", new Vector3(-3f, 3.1f, 0f), new Vector3(3.2f, 1.1f, 1f), new Color(0.42f, 0.31f, 0.24f));
            CreateDecoration(parent.transform, "FarmStorePlaceholder", new Vector3(4.5f, 3.1f, 0f), new Vector3(3.2f, 1.1f, 1f), new Color(0.3f, 0.44f, 0.24f));
        }

        private static GameObject CreateShopNpc(
            Transform parent,
            string objectName,
            Vector3 position,
            Color color,
            string npcDataPath,
            string shopDataPath,
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
            SetReference(serialized, "_npcData", AssetDatabase.LoadAssetAtPath<NpcDataSO>(npcDataPath));
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
            bool canWander)
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
            SetReference(serializedController, "_npcData", AssetDatabase.LoadAssetAtPath<NpcDataSO>(npcDataPath));
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
                SetReference(serializedWanderer, "_npcData", AssetDatabase.LoadAssetAtPath<NpcDataSO>(npcDataPath));
                SetReference(serializedWanderer, "_rigidbody", body);
                serializedWanderer.FindProperty("_wanderBoundsMin").vector2Value = new Vector2(-6f, -3.5f);
                serializedWanderer.FindProperty("_wanderBoundsMax").vector2Value = new Vector2(6f, 3.5f);
                serializedWanderer.ApplyModifiedPropertiesWithoutUndo();
                SetReference(serializedController, "_wanderer", wanderer);
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
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

        private static BuyPanelItem CreateBuyItemTemplate(Transform parent)
        {
            var item = CreatePanel(parent, "BuyItemTemplate", Vector2.zero, new Vector2(500f, 48f));
            item.AddComponent<LayoutElement>().preferredHeight = 48f;
            var component = item.AddComponent<BuyPanelItem>();
            var serialized = new SerializedObject(component);
            SetReference(serialized, "_itemNameText", CreateText(item.transform, "Name", new Vector2(-245f, 0f), new Vector2(180f, 40f), string.Empty));
            SetReference(serialized, "_priceText", CreateText(item.transform, "Price", new Vector2(-60f, 0f), new Vector2(100f, 40f), string.Empty));
            SetReference(serialized, "_stockText", CreateText(item.transform, "Stock", new Vector2(80f, 0f), new Vector2(140f, 40f), string.Empty));
            SetReference(serialized, "_amountInput", CreateInputField(item.transform, "Amount", new Vector2(205f, 0f)));
            SetReference(serialized, "_buyButton", CreateButton(item.transform, "Buy", new Vector2(290f, 0f), new Vector2(85f, 38f), "Comprar"));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            item.SetActive(false);
            return component;
        }

        private static SellPanelItem CreateSellItemTemplate(Transform parent)
        {
            var item = CreatePanel(parent, "SellItemTemplate", Vector2.zero, new Vector2(500f, 48f));
            item.AddComponent<LayoutElement>().preferredHeight = 48f;
            var component = item.AddComponent<SellPanelItem>();
            var serialized = new SerializedObject(component);
            SetReference(serialized, "_itemNameText", CreateText(item.transform, "Name", new Vector2(-245f, 0f), new Vector2(180f, 40f), string.Empty));
            SetReference(serialized, "_priceText", CreateText(item.transform, "Price", new Vector2(-60f, 0f), new Vector2(100f, 40f), string.Empty));
            SetReference(serialized, "_amountText", CreateText(item.transform, "AmountOwned", new Vector2(80f, 0f), new Vector2(140f, 40f), string.Empty));
            SetReference(serialized, "_amountInput", CreateInputField(item.transform, "Amount", new Vector2(205f, 0f)));
            SetReference(serialized, "_sellButton", CreateButton(item.transform, "Sell", new Vector2(290f, 0f), new Vector2(85f, 38f), "Vender"));
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
