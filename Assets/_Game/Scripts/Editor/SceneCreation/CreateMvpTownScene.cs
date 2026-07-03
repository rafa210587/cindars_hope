using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.City;
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
using CindarsHope.Editor.ScaleSystem;
using CindarsHope.World.Scale;
using CindarsHope.Editor.Art;

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

        [MenuItem("CindarsHope/Dev/Recriar somente TownScene %#t", priority = 41)]
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
            // Muralha de pedra visível (torres de canto, postes de portão, ameias) sobre a linha da borda.
            CreateCityWall();
            // Estrutura de chão: pads de distrito (zonas) + ruas de terra ligando praça↔distritos↔
            // portões. Vão por baixo de tudo (Ground, sortingOrder negativo) para os elementos não
            // flutuarem no vazio. São tiras estreitas / zonas, não um slab de horizonte.
            CreateDistrictPads();
            CreateTownRoads();
            CreateMainCamera(playerTransform);
            CreateSpawnPoints(playerTransform);
            CreatePortals();
            // Garante o asset do líder da aldeia (não há gerador automático de NpcDataSO) antes de
            // materializar os NPCs, para o spec npc_velorin carregar os dados.
            EnsureChiefNpcAsset();
            // 4 NPCs da autossuficiência (Sael/Mella/Hess/Tibbet) + 3 lojas — mesmos assets idempotentes.
            EnsureVillageEconomyNpcAssets();
            var npcManager = CreateNpcs(playerTransform, playerManager, inventoryManager, itemDatabase, shopManager, modalManager, shopUi);
            CreateCentralPlaza();
            CreateMarketStalls();
            CreateHouses();
            // Praça de eventos (festivais) + distrito de mercado (bancas ao redor do Salão de Mercado).
            CreateEventsAndMarketDistricts();
            // (Removido: CreateFillerBuildings — eram caixotes/barris SÓ decorativos com collider sólido,
            //  blocos físicos sem motivo. O mapa já é preenchido pelas casas percorríveis reais.)
            // fable_11: schedule anchors (work/social/home per NPC). Must run after NPCs + houses exist.
            // (As casas agora são FÍSICAS percorríveis — CreateWalkInHouse — sem faixa off-field nem portas
            //  de teleporte. O anchor "home" cai dentro do interior físico da própria casa.)
            CreateNpcScheduleAnchors();
            CreateTownDecorations();
            // Arredores temáticos (margem nova do footprint): cemitério (NW), boca de gruta (E) e
            // tenda escondida do mercado noturno (S) — moradias ao relento de Maelor/Zrix/Yael.
            CreateTownOutskirts();
            CreateTownTrees();
            CreateTownProps();  // poço, fonte, lampiões, bancos, cerca, placa — preenche a cidade
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
            Debug.Log($"MVP TownScene created at {ScenePath} (120x90 v8 preservation-first layout).");
        }

        // Preservation audit: proves the 76×64 relayout dropped no materialized element. The baseline is
        // the canonical spec count (the relayout repositions, never removes); the "after" is what
        // the saved scene actually contains. Any mismatch is logged as an error (relayout regression).
        private static void LogRelayoutElementCountAudit(UnityEngine.SceneManagement.Scene scene)
        {
            int npcExpected = RefinedCanonicalTownNpcSpecs.Length + 1; // +1 wanderer
            // Âncoras work/social/home são criadas SÓ para os NPCs com agenda (RefinedCanonicalTownNpcSpecs).
            // O peregrino (wanderer) NÃO tem agenda (vaga numa zona), então NÃO gera as 3 âncoras — por isso
            // o baseline de âncoras usa o nº de NPCs AGENDADOS, não o total (que inclui o wanderer). Era o que
            // fazia a auditoria falsa-positivar: esperava (Refined+1)*3 enquanto o creator faz Refined*3.
            int scheduledNpcExpected = RefinedCanonicalTownNpcSpecs.Length;
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
                "[town-layout] Town 120x90 v8 element-count audit (before=canonical | after=scene):\n" +
                $"  NPCs:    before={npcExpected} after={npcActual}\n" +
                $"  Houses:  before={houseExpected} after={houseActual}\n" +
                $"  Trees:   before={treeExpected} after={treeActual}\n" +
                $"  Stalls:  before={shopExpected} after={stallActual}\n" +
                $"  Anchors: before={scheduledNpcExpected * 3} after={anchorActual} (work/social/home; só NPCs agendados)\n" +
                $"  Spawns:  before={TownDistrictLayout.StableSpawnIds.Length} after={spawnActual}\n" +
                $"  New districts: lake/park={lakeFound} townHall={hallFound} mural={muralFound}\n" +
                $"  Landmark statue present={boardFound}\n" +
                $"  Footprint: {TownDistrictLayout.WidthTiles}x{TownDistrictLayout.HeightTiles} " +
                $"(bounds +/-{TownDistrictLayout.HalfWidth}/+/-{TownDistrictLayout.HalfHeight})");

            // A invariante REAL do relayout e "nenhum elemento foi PERDIDO", nao "contagem identica":
            // specs posteriores (F10 AddMainQuestGiver etc.) ADICIONAM NPCs (e os 3 anchors derivados por NPC),
            // entao npcActual/anchorActual podem ser MAIORES que o baseline canonico sem ser regressao.
            // Regressao = perda; por isso usamos >= para NPCs e anchors e mantemos igualdade estrita para
            // houses/trees/stalls/spawns e os landmarks bool (que specs posteriores nao alteram).
            bool ok = npcActual >= npcExpected && houseActual == houseExpected &&
                      treeActual == treeExpected && stallActual == shopExpected &&
                      anchorActual >= scheduledNpcExpected * 3 &&
                      spawnActual == TownDistrictLayout.StableSpawnIds.Length &&
                      lakeFound && hallFound && muralFound && boardFound;
            if (!ok)
            {
                Debug.LogError("[fable_40] RELAYOUT REGRESSION: um elemento foi PERDIDO no relayout " +
                               "(contagem abaixo do baseline canonico), ou um district/landmark novo esta ausente. " +
                               "See the audit above.");
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
            // Tag built-in "Player" — usada pelo RoofRevealController para revelar o interior da casa
            // (esconder o telhado) quando o jogador entra. Tag padrão do Unity, sempre existe.
            player.tag = "Player";
            // Size from the authored Player scale profile (VisualScale 2.0), not a hardcoded localScale.
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
                Debug.LogWarning("[PlayerWalkAnimator] Town: Resources/PlayerSprites/walk/down/walk_down_01 nao encontrado. " +
                                 "Usando sprite builtin como fallback. Reimporte os sprites do player.");
            }
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 0;
            TrySetSortingLayer(spriteRenderer, "Characters", spriteRenderer.sortingOrder);

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

            // 120x90 footprint. The south collider is split around the actual gate opening.
            float w = TownDistrictLayout.WidthTiles;
            float h = TownDistrictLayout.HeightTiles;
            float halfW = TownDistrictLayout.HalfWidth;
            float halfH = TownDistrictLayout.HalfHeight;
            const float gateHalf = 4f;
            CreateBound("Top", bounds.transform, new Vector2(0f, halfH + 0.5f), new Vector2(w + 1f, 1f));
            float southSegmentWidth = halfW - gateHalf;
            CreateBound("Bottom_West", bounds.transform, new Vector2(-(gateHalf + southSegmentWidth * 0.5f), -(halfH + 0.5f)), new Vector2(southSegmentWidth, 1f));
            CreateBound("Bottom_East", bounds.transform, new Vector2(gateHalf + southSegmentWidth * 0.5f, -(halfH + 0.5f)), new Vector2(southSegmentWidth, 1f));
            CreateBound("Left", bounds.transform, new Vector2(-(halfW + 0.5f), 0f), new Vector2(1f, h + 1f));
            CreateBound("Right", bounds.transform, new Vector2(halfW + 0.5f, 0f), new Vector2(1f, h + 1f));
        }

        // Detalhes fortificados da muralha: torres, portão sul e ameias.
        // O CORPO do muro + collider vêm de CreateBounds (cor de pedra); aqui é só o visual de muralha.
        private static void CreateCityWall()
        {
            var parent = new GameObject("CityWall");
            parent.transform.position = Vector3.zero;
            float wx = TownDistrictLayout.HalfWidth + 0.5f;   // 38.5
            float wy = TownDistrictLayout.HalfHeight + 0.5f;  // 32.5
            var stone = new Color(0.56f, 0.54f, 0.50f);
            var stoneDark = new Color(0.34f, 0.32f, 0.30f);
            const float gateHalf = 4f;

            // Merlões com vão somente no lado sul.
            for (float x = -wx + 2f; x <= wx - 2f + 0.01f; x += 3f)
            {
                CreateDecoration(parent.transform, "Merlon_N", new Vector3(x, wy + 0.4f, 0f), new Vector3(1.2f, 0.9f, 1f), stoneDark);
                if (Mathf.Abs(x) > gateHalf + 1f)
                {
                    CreateDecoration(parent.transform, "Merlon_S", new Vector3(x, -wy - 0.4f, 0f), new Vector3(1.2f, 0.9f, 1f), stoneDark);
                }
            }
            for (float y = -wy + 2f; y <= wy - 2f + 0.01f; y += 3f)
            {
                CreateDecoration(parent.transform, "Merlon_E", new Vector3(wx + 0.4f, y, 0f), new Vector3(0.9f, 1.2f, 1f), stoneDark);
                CreateDecoration(parent.transform, "Merlon_W", new Vector3(-wx - 0.4f, y, 0f), new Vector3(0.9f, 1.2f, 1f), stoneDark);
            }

            // Torres de canto (sólidas).
            CreateTower(parent.transform, "Tower_NW", new Vector3(-wx, wy, 0f), stone, stoneDark, 3.0f);
            CreateTower(parent.transform, "Tower_NE", new Vector3(wx, wy, 0f), stone, stoneDark, 3.0f);
            CreateTower(parent.transform, "Tower_SW", new Vector3(-wx, -wy, 0f), stone, stoneDark, 3.0f);
            CreateTower(parent.transform, "Tower_SE", new Vector3(wx, -wy, 0f), stone, stoneDark, 3.0f);
            CreateTower(parent.transform, "GatePost_W", new Vector3(-(gateHalf + 0.6f), -wy, 0f), stone, stoneDark, 1.8f);
            CreateTower(parent.transform, "GatePost_E", new Vector3(gateHalf + 0.6f, -wy, 0f), stone, stoneDark, 1.8f);
        }

        private static void CreateTower(Transform parent, string name, Vector3 position, Color stone, Color stoneDark, float size)
        {
            var tower = new GameObject(name);
            tower.transform.SetParent(parent);
            tower.transform.position = position;

            var baseGo = new GameObject("Base");
            baseGo.transform.SetParent(tower.transform);
            baseGo.transform.localPosition = Vector3.zero;
            baseGo.transform.localScale = new Vector3(size + 0.4f, size + 0.4f, 1f);
            var baseRenderer = baseGo.AddComponent<SpriteRenderer>();
            var towerStoneBase = WorldSpriteLibrary.Building("wall_stone");
            if (towerStoneBase != null) { baseRenderer.sprite = towerStoneBase; baseRenderer.color = Color.white; }
            else { baseRenderer.sprite = GetBuiltinSprite(); baseRenderer.color = stoneDark; }
            baseRenderer.sortingOrder = 8;
            TrySetSortingLayer(baseRenderer, "Items", baseRenderer.sortingOrder);

            var topGo = new GameObject("Top");
            topGo.transform.SetParent(tower.transform);
            topGo.transform.localPosition = Vector3.zero;
            topGo.transform.localScale = new Vector3(size, size, 1f);
            var topRenderer = topGo.AddComponent<SpriteRenderer>();
            var towerStoneTop = WorldSpriteLibrary.Building("wall_stone");
            if (towerStoneTop != null) { topRenderer.sprite = towerStoneTop; topRenderer.color = Color.white; }
            else { topRenderer.sprite = GetBuiltinSprite(); topRenderer.color = stone; }
            topRenderer.sortingOrder = 9;
            TrySetSortingLayer(topRenderer, "Items", topRenderer.sortingOrder);

            var col = tower.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = new Vector2(size, size);
        }

        // Perímetro: MURALHA DE PEDRA visível na linha da borda (CreateCityWall adiciona torres de canto,
        // postes do portão e ameias). A floresta fica POR FORA da muralha (BuildBorderTreeRing). Cor de
        // pedra para o muro ler como muralha, não como força invisível.
        private static readonly Color BorderWallColor = new Color(0.50f, 0.48f, 0.44f);
        // Earthy interior wall so a house interior reads as an enclosed room.
        private static readonly Color InteriorWallColor = new Color(0.3f, 0.25f, 0.21f);

        private static void CreateBound(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var bound = new GameObject(name);
            bound.transform.SetParent(parent);
            bound.transform.position = position;

            var collider = bound.AddComponent<BoxCollider2D>();
            collider.size = size;

            AddWallVisual(bound.transform, size, BorderWallColor, "Wall");
        }

        // Adds a visible quad sprite matching a wall collider. The visual is a SCALED CHILD so the
        // parent's BoxCollider2D.size (in local units) is unaffected by the visual scale.
        private static void AddWallVisual(Transform parent, Vector2 size, Color color, string sortingLayer)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBuiltinSprite();
            renderer.color = color;
            renderer.sortingOrder = 1;
            TrySetSortingLayer(renderer, sortingLayer, renderer.sortingOrder);
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

            // IDs stay frozen; both arrivals now use the wide south avenue.
            var defaultSpawn = CreateSpawnPoint(parent.transform, "town_default", new Vector3(0f, -24f, 0f));
            var fromFarmSpawn = CreateSpawnPoint(parent.transform, "town_from_farm", new Vector3(0f, -(TownDistrictLayout.HalfHeight - 5f), 0f));

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

            // Entrada da cidade / saída para a fazenda no portão central sul.
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
            var plazaCenter = new Vector3(TownCityLayout.CentralPlazaCenter.x, TownCityLayout.CentralPlazaCenter.y, 0f);

            // Plaza floor (large light slab under the statue, sorting below everything else)
            var floor = new GameObject("PlazaFloor");
            floor.transform.SetParent(plaza.transform);
            floor.transform.position = plazaCenter;
            var floorRenderer = floor.AddComponent<SpriteRenderer>();
            var plazaTile = WorldSpriteLibrary.Ground("ground_cobble");
            if (plazaTile != null)
            {
                floor.transform.localScale = Vector3.one;
                floorRenderer.sprite = plazaTile;
                floorRenderer.color = Color.white;
                floorRenderer.drawMode = SpriteDrawMode.Tiled;
                floorRenderer.tileMode = SpriteTileMode.Continuous;
                floorRenderer.size = TownCityLayout.CentralPlazaSize;
            }
            else
            {
                floor.transform.localScale = new Vector3(TownCityLayout.CentralPlazaSize.x, TownCityLayout.CentralPlazaSize.y, 1f);
                floorRenderer.sprite = GetBuiltinSprite();
                floorRenderer.color = new Color(0.69f, 0.66f, 0.6f);
            }
            floorRenderer.sortingOrder = 0;
            TrySetSortingLayer(floorRenderer, "Ground", floorRenderer.sortingOrder);

            var statue = new GameObject("WarriorStatue");
            statue.transform.SetParent(plaza.transform);
            statue.transform.position = plazaCenter;

            // Fonte/bacia central: a referência pede uma praça cívica ampla, não apenas uma estátua
            // sobre um quadrado pequeno. Placeholder substituível pela sprite final.
            var fountainBasin = CreateStatuePart(plaza.transform, "FountainBasin", plazaCenter, new Vector3(4.6f, 3.6f, 1f), new Color(0.46f, 0.50f, 0.54f), 1);
            var fountainCollider = fountainBasin.AddComponent<BoxCollider2D>();
            fountainCollider.isTrigger = false;
            fountainCollider.size = Vector2.one;
            CreateStatuePart(plaza.transform, "FountainWater", plazaCenter, new Vector3(3.6f, 2.6f, 1f), new Color(0.24f, 0.52f, 0.70f), 2);

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
            CreateDecoration(plaza.transform, "PlazaBench_N", plazaCenter + new Vector3(0f, 7f, 0f), new Vector3(2.2f, 0.55f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaBench_S", plazaCenter + new Vector3(0f, -7f, 0f), new Vector3(2.2f, 0.55f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaBench_E", plazaCenter + new Vector3(7f, 0f, 0f), new Vector3(0.55f, 2.2f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaBench_W", plazaCenter + new Vector3(-7f, 0f, 0f), new Vector3(0.55f, 2.2f, 1f), new Color(0.5f, 0.38f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaPlanter_NE", plazaCenter + new Vector3(9.5f, 9.5f, 0f), new Vector3(1.4f, 1.4f, 1f), new Color(0.3f, 0.52f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaPlanter_NW", plazaCenter + new Vector3(-9.5f, 9.5f, 0f), new Vector3(1.4f, 1.4f, 1f), new Color(0.3f, 0.52f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaPlanter_SE", plazaCenter + new Vector3(9.5f, -9.5f, 0f), new Vector3(1.4f, 1.4f, 1f), new Color(0.3f, 0.52f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaPlanter_SW", plazaCenter + new Vector3(-9.5f, -9.5f, 0f), new Vector3(1.4f, 1.4f, 1f), new Color(0.3f, 0.52f, 0.26f));
            CreateDecoration(plaza.transform, "PlazaLamp_NE", plazaCenter + new Vector3(11f, 5.5f, 0f), new Vector3(0.4f, 1.5f, 1f), new Color(0.76f, 0.62f, 0.30f));
            CreateDecoration(plaza.transform, "PlazaLamp_NW", plazaCenter + new Vector3(-11f, 5.5f, 0f), new Vector3(0.4f, 1.5f, 1f), new Color(0.76f, 0.62f, 0.30f));
            CreateDecoration(plaza.transform, "PlazaLamp_SE", plazaCenter + new Vector3(11f, -5.5f, 0f), new Vector3(0.4f, 1.5f, 1f), new Color(0.76f, 0.62f, 0.30f));
            CreateDecoration(plaza.transform, "PlazaLamp_SW", plazaCenter + new Vector3(-11f, -5.5f, 0f), new Vector3(0.4f, 1.5f, 1f), new Color(0.76f, 0.62f, 0.30f));

            // (FestivalStallAnchor foi movido para a PRAÇA DE EVENTOS dedicada — ver CreateEventsAndMarketDistricts.)
        }

        // Praça de eventos (festivais) + distrito de mercado. A praça de eventos é uma área ABERTA
        // dedicada onde o WorldEventService monta as barracas temporárias; o distrito de mercado agrupa
        // bancas cobertas ao redor do Salão de Mercado (House_MarketHall). Ambos em zonas reservadas
        // (o placer não põe casa em cima).
        private static void CreateEventsAndMarketDistricts()
        {
            var parent = new GameObject("TownEventsMarket");
            parent.transform.position = Vector3.zero;

            // ── Praça de eventos (sul) ──
            CreateGroundSlab(parent.transform, "EventsPlaza_Ground", EventsPlazaCenter, new Vector2(15f, 9f), new Color(0.66f, 0.62f, 0.52f), -1);
            CreateDecoration(parent.transform, "EventsPlaza_Stage",
                EventsPlazaCenter + new Vector3(0f, 2.6f, 0f), new Vector3(5f, 1.4f, 1f), new Color(0.50f, 0.38f, 0.26f)); // tablado
            var festivalAnchor = new GameObject("FestivalStallAnchor");
            festivalAnchor.transform.SetParent(parent.transform);
            festivalAnchor.transform.position = EventsPlazaCenter;
            festivalAnchor.AddComponent<CindarsHope.World.Events.FestivalStallAnchor>();

            // ── Distrito de mercado (norte) — bancas cobertas ao redor do Salão de Mercado ──
            CreateGroundSlab(parent.transform, "MarketSquare_Ground", MarketHallCenter + new Vector3(0f, -8.5f, 0f), new Vector2(16f, 5f), new Color(0.64f, 0.57f, 0.44f), -1);
            float[] sx = { -7f, -4f, 4f, 7f };
            for (int i = 0; i < sx.Length; i++)
            {
                CreateMarketStall(parent.transform, $"MarketSquare_Stall_{i:00}", MarketHallCenter + new Vector3(sx[i], -8.5f, 0f), MarketAwningColor(i));
            }
            CreateMarketStall(parent.transform, "MarketSquare_Stall_04", MarketHallCenter + new Vector3(-5.5f, -10.8f, 0f), MarketAwningColor(4));
            CreateMarketStall(parent.transform, "MarketSquare_Stall_05", MarketHallCenter + new Vector3(5.5f, -10.8f, 0f), MarketAwningColor(5));
        }

        private static Color MarketAwningColor(int i)
        {
            var palette = new[]
            {
                new Color(0.72f, 0.45f, 0.40f), new Color(0.45f, 0.60f, 0.50f),
                new Color(0.50f, 0.52f, 0.70f), new Color(0.72f, 0.66f, 0.45f),
                new Color(0.66f, 0.50f, 0.62f), new Color(0.50f, 0.66f, 0.66f),
            };
            return palette[i % palette.Length];
        }

        // Banca de mercado decorativa: balcão (bloqueia atrás) + toldo colorido (cosmético, sem collider).
        private static void CreateMarketStall(Transform parent, string name, Vector3 position, Color awningColor)
        {
            var stall = new GameObject(name);
            stall.transform.SetParent(parent);
            stall.transform.position = position;

            var counter = new GameObject("Counter");
            counter.transform.SetParent(stall.transform);
            counter.transform.localPosition = Vector3.zero;
            counter.transform.localScale = new Vector3(2.0f, 0.6f, 1f);
            var counterRenderer = counter.AddComponent<SpriteRenderer>();
            counterRenderer.sprite = GetBuiltinSprite();
            counterRenderer.color = new Color(0.46f, 0.34f, 0.22f);
            counterRenderer.sortingOrder = 1;
            TrySetSortingLayer(counterRenderer, "Items", counterRenderer.sortingOrder);
            var counterCollider = counter.AddComponent<BoxCollider2D>();
            counterCollider.isTrigger = false;
            counterCollider.size = Vector2.one;

            var awning = new GameObject("Awning");
            awning.transform.SetParent(stall.transform);
            awning.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            awning.transform.localScale = new Vector3(2.3f, 0.5f, 1f);
            var awningRenderer = awning.AddComponent<SpriteRenderer>();
            awningRenderer.sprite = GetBuiltinSprite();
            awningRenderer.color = awningColor;
            awningRenderer.sortingOrder = 4;
            TrySetSortingLayer(awningRenderer, "Items", awningRenderer.sortingOrder);
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
                stall.transform.position = TownCityLayout.ResolveNpcStallPosition(
                    spec.NpcId,
                    spec.LayoutPosition + new Vector3(0f, 1.15f, 0f));

                // Counter (walkable in front, blocks behind)
                var counter = new GameObject("Counter");
                counter.transform.SetParent(stall.transform);
                counter.transform.localPosition = Vector3.zero;
                counter.transform.localScale = new Vector3(2.1f, 0.6f, 1f);
                var counterRenderer = counter.AddComponent<SpriteRenderer>();
                var stallCounter = WorldSpriteLibrary.Prop("crate");
                if (stallCounter != null) { counterRenderer.sprite = stallCounter; counterRenderer.color = Color.white; }
                else { counterRenderer.sprite = GetBuiltinSprite(); counterRenderer.color = new Color(0.46f, 0.34f, 0.22f); }
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
        private static readonly Vector2 HouseSizeDefault = new Vector2(2.6f, 1.8f);

        // Casas FÍSICAS percorríveis (pedido: "a casa deveria ser real, física, só entrar e andar por
        // elas mesmo. Não ser cena separada"). Cada casa tem chão andável + 4 paredes sólidas com um VÃO
        // de porta embaixo + móveis + telhado que esconde o interior até o jogador entrar
        // (RoofRevealController). SEM teleporte, SEM cena separada, SEM faixa off-field.
        //
        // Coordenadas JÁ no footprint final 76×64 (NÃO passam por Reposition) para controlar exatamente o
        // espaçamento e nunca sobrepor. Layout em colunas com ruas entre elas; núcleo cívico ao norte;
        // praça central livre; corredor de entrada a oeste (y≈0) livre. AuditHouseOverlaps valida na geração.
        private const float WallThickness = 1f;
        private const float InteriorInsetPerSide = 1f;
        private const float DoorGapWidth = 1.8f;

        // Centros de marcos/áreas reservadas (não recebem casas). Devem bater com onde os marcos são
        // de fato criados (praça, salão de mercado, praça de eventos) e com os landmarks de canto.
        private static readonly Vector3 MarketHallCenter = new Vector3(-48f, 15f, 0f);
        private static readonly Vector3 EventsPlazaCenter = new Vector3(0f, -17f, 0f);

        // Compatibility projection consumed by the existing house/home helpers. TownCityLayout is
        // the sole placement source; this tuple array preserves the generator's established API.
        private static (string name, Vector3 position, Color baseColor, Vector2 size)[] s_houseSpecs;
        private static (string name, Vector3 position, Color baseColor, Vector2 size)[] TownHouseSpecs
            => s_houseSpecs ??= BuildHouseSpecsFromLots();

        private static (string name, Vector3 position, Color baseColor, Vector2 size)[] BuildHouseSpecsFromLots()
        {
            var lots = TownCityLayout.AllBuildings;
            var result = new (string, Vector3, Color, Vector2)[lots.Count];
            for (var i = 0; i < lots.Count; i++)
            {
                var lot = lots[i];
                result[i] = (lot.Name, new Vector3(lot.Center.x, lot.Center.y, 0f), lot.Color, lot.Size);
            }

            return result;
        }

        private static void CreateHouses()
        {
            var parent = new GameObject("TownHouses");
            parent.transform.position = Vector3.zero;

            // TownCityLayout owns each lot and its frontage. Stable House_* names remain unchanged.
            foreach (var (name, position, baseColor, size) in TownHouseSpecs)
            {
                if (!TownCityLayout.TryGetBuilding(name, out var lot))
                {
                    Debug.LogError($"[town-layout] Missing lot definition for {name}.");
                    continue;
                }

                CreateWalkInHouse(parent.transform, name, position, baseColor, size, lot.DoorSide, lot.Archetype);
            }

            AuditHouseOverlaps();
        }

        // Constrói uma casa FÍSICA percorrível, no MESMO lugar do exterior. O jogador entra pelo vão da
        // parede de baixo, anda pelo cômodo, e o telhado some enquanto ele está dentro (RoofRevealController).
        private static void CreateWalkInHouse(
            Transform parent,
            string name,
            Vector3 position,
            Color baseColor,
            Vector2 size,
            TownDoorSide doorSide,
            TownBuildingArchetype archetype)
        {
            var house = new GameObject(name);
            house.transform.SetParent(parent);
            house.transform.position = position;

            float hw = size.x * 0.5f;
            float hh = size.y * 0.5f;
            var interiorSize = new Vector2(
                Mathf.Max(1f, size.x - InteriorInsetPerSide * 2f),
                Mathf.Max(1f, size.y - InteriorInsetPerSide * 2f));
            float ihw = interiorSize.x * 0.5f;
            float ihh = interiorSize.y * 0.5f;
            float wallX = hw - WallThickness * 0.5f;
            float wallY = hh - WallThickness * 0.5f;
            bool big = size.x >= 6f;
            bool standardNpcHouse = Mathf.Approximately(size.x, TownCityLayout.StandardHouseExteriorSize.x) &&
                                    Mathf.Approximately(size.y, TownCityLayout.StandardHouseExteriorSize.y);
            bool horizontalDoor = doorSide == TownDoorSide.South || doorSide == TownDoorSide.North;
            Vector3 doorLocalPosition;
            switch (doorSide)
            {
                case TownDoorSide.North: doorLocalPosition = new Vector3(0f, wallY, 0f); break;
                case TownDoorSide.West: doorLocalPosition = new Vector3(-wallX, 0f, 0f); break;
                case TownDoorSide.East: doorLocalPosition = new Vector3(wallX, 0f, 0f); break;
                default: doorLocalPosition = new Vector3(0f, -wallY, 0f); break;
            }

            // Chão (andável — SEM collider). Tom claro derivado da cor base.
            var floor = new GameObject("Floor");
            floor.transform.SetParent(house.transform);
            floor.transform.localPosition = Vector3.zero;
            var floorRenderer = floor.AddComponent<SpriteRenderer>();
            var floorTile = WorldSpriteLibrary.Ground("ground_deck");
            if (floorTile != null)
            {
                floor.transform.localScale = Vector3.one;
                floorRenderer.sprite = floorTile;
                floorRenderer.color = Color.white;
                floorRenderer.drawMode = SpriteDrawMode.Tiled;
                floorRenderer.tileMode = SpriteTileMode.Continuous;
                floorRenderer.size = interiorSize;
            }
            else
            {
                floor.transform.localScale = new Vector3(interiorSize.x, interiorSize.y, 1f);
                floorRenderer.sprite = GetBuiltinSprite();
                floorRenderer.color = Color.Lerp(baseColor, new Color(0.85f, 0.80f, 0.72f), 0.5f);
            }
            floorRenderer.sortingOrder = 0;
            TrySetSortingLayer(floorRenderer, "Items", floorRenderer.sortingOrder);

            // Paredes sólidas (visual de PEDRA com contorno — ver AddStoneWallVisual). Três paredes
            // inteiras + a parede do lado da porta dividida em duas, deixando o VÃO no centro.
            if (horizontalDoor)
            {
                CreateInteriorWall(house.transform, "Wall_Left", new Vector3(-wallX, 0f, 0f), new Vector2(WallThickness, size.y));
                CreateInteriorWall(house.transform, "Wall_Right", new Vector3(wallX, 0f, 0f), new Vector2(WallThickness, size.y));
                float sideWidth = (size.x - DoorGapWidth) * 0.5f;
                float sideCenter = (DoorGapWidth + sideWidth) * 0.5f;
                string doorWall = doorSide == TownDoorSide.South ? "Bottom" : "Top";
                float oppositeY = -doorLocalPosition.y;
                CreateInteriorWall(house.transform, $"Wall_{(doorSide == TownDoorSide.South ? "Top" : "Bottom")}", new Vector3(0f, oppositeY, 0f), new Vector2(size.x, WallThickness));
                CreateInteriorWall(house.transform, $"Wall_{doorWall}L", new Vector3(-sideCenter, doorLocalPosition.y, 0f), new Vector2(sideWidth, WallThickness));
                CreateInteriorWall(house.transform, $"Wall_{doorWall}R", new Vector3(sideCenter, doorLocalPosition.y, 0f), new Vector2(sideWidth, WallThickness));
            }
            else
            {
                CreateInteriorWall(house.transform, "Wall_Top", new Vector3(0f, wallY, 0f), new Vector2(size.x, WallThickness));
                CreateInteriorWall(house.transform, "Wall_Bottom", new Vector3(0f, -wallY, 0f), new Vector2(size.x, WallThickness));
                float sideHeight = (size.y - DoorGapWidth) * 0.5f;
                float sideCenter = (DoorGapWidth + sideHeight) * 0.5f;
                string doorWall = doorSide == TownDoorSide.West ? "Left" : "Right";
                float oppositeX = -doorLocalPosition.x;
                CreateInteriorWall(house.transform, $"Wall_{(doorSide == TownDoorSide.West ? "Right" : "Left")}", new Vector3(oppositeX, 0f, 0f), new Vector2(WallThickness, size.y));
                CreateInteriorWall(house.transform, $"Wall_{doorWall}T", new Vector3(doorLocalPosition.x, sideCenter, 0f), new Vector2(WallThickness, sideHeight));
                CreateInteriorWall(house.transform, $"Wall_{doorWall}B", new Vector3(doorLocalPosition.x, -sideCenter, 0f), new Vector2(WallThickness, sideHeight));
            }

            // Porta funcional no vão: FECHADA tranca a passagem; aperte E para ABRIR (desliza) e entrar.
            CreateHouseDoor(house.transform, doorLocalPosition, doorSide);

            // Móveis (andáveis — CreateInteriorProp não põe collider). Cama sempre; mesa/estante nas maiores.
            var bedProp = CreateInteriorProp(house.transform, "Bed", new Vector3(-ihw + 0.9f, ihh - 0.7f, 0f), new Vector3(1.6f, 1f, 1f), new Color(0.5f, 0.36f, 0.5f));
            if (name == "House_Inn")
            {
                // fable_57: a cama da estalagem é interagível e paga.
                bedProp.name = "GuestBed_Inn";
                var trigger = bedProp.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(1.6f, 1.0f);
                var gate = bedProp.AddComponent<InnBedPaymentGate>();
                gate.Configure(InnLodgingPaymentResolver.DefaultNightlyRate);
                EditorUtility.SetDirty(gate);
            }
            CreateInteriorProp(house.transform, "Table", new Vector3(ihw - 0.9f, -ihh + 0.8f, 0f), new Vector3(1.3f, 0.9f, 1f), new Color(0.46f, 0.34f, 0.22f));
            if (big)
            {
                CreateInteriorProp(house.transform, "Furniture_Shelf", new Vector3(ihw - 1.0f, ihh - 0.55f, 0f), new Vector3(1.7f, 0.7f, 1f), new Color(0.42f, 0.32f, 0.22f));
                CreateInteriorProp(house.transform, "Furniture_Rug", new Vector3(0.4f, -0.2f, 0f), new Vector3(2.6f, 1.6f, 1f), new Color(0.40f, 0.26f, 0.24f));
            }
            if (standardNpcHouse)
            {
                // Núcleo doméstico obrigatório dentro dos 6x5 úteis: cozinha compacta na parede sul,
                // fogão no canto e armário. São props caminháveis para não estreitar a circulação.
                CreateInteriorProp(house.transform, "Furniture_KitchenCounter", new Vector3(-1.15f, -ihh + 0.45f, 0f), new Vector3(2.2f, 0.7f, 1f), new Color(0.54f, 0.38f, 0.22f));
                CreateInteriorProp(house.transform, "Furniture_Stove", new Vector3(-ihw + 0.55f, -0.85f, 0f), new Vector3(0.8f, 0.8f, 1f), new Color(0.30f, 0.29f, 0.28f));
                CreateInteriorProp(house.transform, "Furniture_Cupboard", new Vector3(ihw - 0.45f, 0.65f, 0f), new Vector3(0.7f, 1.3f, 1f), new Color(0.46f, 0.33f, 0.20f));
            }

            // Construções especiais grandes ganham um segundo ambiente. Casas residenciais 8x7
            // preservam integralmente o salão interno útil de 6x5.
            if (size.x >= 10f || archetype == TownBuildingArchetype.Temple)
            {
                float dividerX = hw - 2.6f; // separa um cômodo lateral à direita
                float passage = 1.6f;       // vão de passagem na base da divisória
                float segH = (size.y - passage) * 0.5f;
                CreateInteriorWall(house.transform, "Wall_Divider", new Vector3(dividerX, hh - segH * 0.5f, 0f), new Vector2(WallThickness, segH));
                if (archetype == TownBuildingArchetype.Temple)
                {
                    CreateInteriorProp(house.transform, "Furniture_Altar", new Vector3(0f, ihh - 0.6f, 0f), new Vector3(2.0f, 0.8f, 1f), new Color(0.55f, 0.48f, 0.30f));
                    CreateInteriorProp(house.transform, "Furniture_Pew", new Vector3(-1.0f, -0.5f, 0f), new Vector3(3.2f, 0.6f, 1f), new Color(0.42f, 0.30f, 0.20f));
                }
                else
                {
                    CreateInteriorProp(house.transform, "Furniture_ServiceCounter", new Vector3(0f, ihh - 0.6f, 0f), new Vector3(2.8f, 0.8f, 1f), Color.Lerp(baseColor, Color.black, 0.25f));
                }
            }

            // Estação de ofício da casa (forja/alambique/tear/fogão/bancada): aperte E para abrir o craft
            // filtrado por aquele WorkshopType. Só casas-ofício recebem; o resto ignora.
            TryAddCraftingStation(house.transform, name, new Vector3(-hw + 1.6f, -hh + 1.4f, 0f));

            // Telhado: cobre TODO o footprint (+ beiral). sortingOrder alto ⇒ esconde chão/paredes/móveis/NPC
            // enquanto o jogador está fora. O RoofRevealController zera seu alpha quando o jogador entra.
            var roof = new GameObject("Roof");
            roof.transform.SetParent(house.transform);
            roof.transform.localPosition = Vector3.zero;
            var roofRenderer = roof.AddComponent<SpriteRenderer>();
            var roofTile = WorldSpriteLibrary.Building("roof_redtile");
            if (roofTile != null)
            {
                roof.transform.localScale = Vector3.one;
                roofRenderer.sprite = roofTile;
                roofRenderer.color = RoofTint(archetype);
                roofRenderer.drawMode = SpriteDrawMode.Tiled;
                roofRenderer.tileMode = SpriteTileMode.Continuous;
                roofRenderer.size = new Vector2(size.x + 0.6f, size.y + 0.6f);
            }
            else
            {
                roof.transform.localScale = new Vector3(size.x + 0.9f, size.y + 0.9f, 1f);
                roofRenderer.sprite = GetBuiltinSprite();
                roofRenderer.color = Color.Lerp(baseColor, new Color(0.5f, 0.18f, 0.12f), 0.6f);
            }
            roofRenderer.sortingOrder = 20;
            TrySetSortingLayer(roofRenderer, "Items", roofRenderer.sortingOrder);

            CreateBuildingIdentityDetails(house.transform, name, size, doorSide, archetype, baseColor);

            // Cumeeira: faixa escura no topo do telhado (só estética de "telhado de duas águas").
            var ridge = new GameObject("RoofRidge");
            ridge.transform.SetParent(house.transform);
            ridge.transform.localPosition = new Vector3(0f, hh * 0.15f, 0f);
            ridge.transform.localScale = new Vector3(size.x + 0.9f, 0.35f, 1f);
            var ridgeRenderer = ridge.AddComponent<SpriteRenderer>();
            ridgeRenderer.sprite = GetBuiltinSprite();
            ridgeRenderer.color = new Color(0.30f, 0.10f, 0.07f);
            ridgeRenderer.sortingOrder = 21;
            TrySetSortingLayer(ridgeRenderer, "Items", ridgeRenderer.sortingOrder);

            // Destaque claro logo acima da cumeeira => leitura de telhado de duas aguas.
            var ridgeHi = new GameObject("RoofRidgeHighlight");
            ridgeHi.transform.SetParent(house.transform);
            ridgeHi.transform.localPosition = new Vector3(0f, hh * 0.15f + 0.30f, 0f);
            ridgeHi.transform.localScale = new Vector3(size.x + 0.9f, 0.16f, 1f);
            var ridgeHiRenderer = ridgeHi.AddComponent<SpriteRenderer>();
            ridgeHiRenderer.sprite = GetBuiltinSprite();
            ridgeHiRenderer.color = new Color(1f, 0.85f, 0.70f, 0.35f);
            ridgeHiRenderer.sortingOrder = 21;
            TrySetSortingLayer(ridgeHiRenderer, "Items", ridgeHiRenderer.sortingOrder);

            // Sombra de beiral na borda inferior do telhado (destaca a casa do chao).
            var eave = new GameObject("RoofEaveShadow");
            eave.transform.SetParent(house.transform);
            eave.transform.localPosition = new Vector3(0f, -hh - 0.35f, 0f);
            eave.transform.localScale = new Vector3(size.x + 0.9f, 0.35f, 1f);
            var eaveRenderer = eave.AddComponent<SpriteRenderer>();
            eaveRenderer.sprite = GetBuiltinSprite();
            eaveRenderer.color = new Color(0f, 0f, 0f, 0.30f);
            eaveRenderer.sortingOrder = 20;
            TrySetSortingLayer(eaveRenderer, "Items", eaveRenderer.sortingOrder);

            // Chamine + fumacinha no canto superior do telhado.
            var chimney = new GameObject("Chimney");
            chimney.transform.SetParent(house.transform);
            chimney.transform.localPosition = new Vector3(hw - 0.9f, hh + 0.15f, 0f);
            chimney.transform.localScale = new Vector3(0.7f, 0.9f, 1f);
            var chimneyRenderer = chimney.AddComponent<SpriteRenderer>();
            chimneyRenderer.sprite = GetBuiltinSprite();
            chimneyRenderer.color = new Color(0.45f, 0.30f, 0.24f);
            chimneyRenderer.sortingOrder = 22;
            TrySetSortingLayer(chimneyRenderer, "Items", chimneyRenderer.sortingOrder);

            var smoke = new GameObject("ChimneySmoke");
            smoke.transform.SetParent(house.transform);
            smoke.transform.localPosition = new Vector3(hw - 0.9f, hh + 0.85f, 0f);
            smoke.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            var smokeRenderer = smoke.AddComponent<SpriteRenderer>();
            smokeRenderer.sprite = GetBuiltinSprite();
            smokeRenderer.color = new Color(0.85f, 0.85f, 0.85f, 0.5f);
            smokeRenderer.sortingOrder = 22;
            TrySetSortingLayer(smokeRenderer, "Items", smokeRenderer.sortingOrder);

            // Trigger de revelação: cobre o footprint; some os dois renderers de telhado ao jogador entrar.
            var revealObject = new GameObject("RoofReveal");
            revealObject.transform.SetParent(house.transform);
            revealObject.transform.localPosition = Vector3.zero;
            var revealTrigger = revealObject.AddComponent<BoxCollider2D>();
            revealTrigger.isTrigger = true;
            revealTrigger.size = interiorSize;
            var reveal = revealObject.AddComponent<RoofRevealController>();
            reveal.Configure(new[] { roofRenderer, ridgeRenderer, ridgeHiRenderer, eaveRenderer, chimneyRenderer, smokeRenderer });
            EditorUtility.SetDirty(reveal);
        }

        // Mapa casa-ofício → estação de craft. Só estas casas recebem estação; as demais ignoram.
        private static void TryAddCraftingStation(Transform house, string houseName, Vector3 localPos)
        {
            WorkshopType type;
            string label;
            Color color;
            switch (houseName)
            {
                case "House_Blacksmith": type = WorkshopType.Forge;          label = "Usar a forja [E]";        color = new Color(0.70f, 0.32f, 0.16f); break;
                case "House_AlchemyLab": type = WorkshopType.Alchemy;        label = "Usar o alambique [E]";    color = new Color(0.30f, 0.60f, 0.42f); break;
                case "House_Inn":        type = WorkshopType.CookingStation; label = "Cozinhar [E]";            color = new Color(0.66f, 0.42f, 0.22f); break;
                case "House_Workshop":   type = WorkshopType.Carpentry;      label = "Usar a bancada [E]";      color = new Color(0.52f, 0.40f, 0.24f); break;
                case "House_Residential_3": type = WorkshopType.Sewing;      label = "Usar o tear [E]";         color = new Color(0.52f, 0.46f, 0.62f); break; // Mirela
                case "House_MarketHall": type = WorkshopType.Workbench;      label = "Usar a bancada do mercado [E]"; color = new Color(0.52f, 0.50f, 0.44f); break;
                default: return;
            }

            CreateCraftingStation(house, localPos, color, $"station_{houseName.ToLowerInvariant()}", type, label);
        }

        // Estação física interagível: prop colorido + trigger de interação + CraftingStationInteractable.
        // Ao apertar E, publica o evento que abre o craft daquele WorkshopType (CraftingModal assina).
        private static void CreateCraftingStation(Transform house, Vector3 localPos, Color color, string stationId, WorkshopType type, string label)
        {
            var station = CreateInteriorProp(house, $"Station_{type}", localPos, new Vector3(1.6f, 1.1f, 1f), color);
            station.GetComponent<SpriteRenderer>().sortingOrder = 2; // acima do chão/móveis, abaixo do telhado

            var trigger = station.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = Vector2.one; // local 1 × escala ⇒ área de interação ~1.6×1.1

            // Componente canônico de estação (CraftingPoint), modo DESACOPLADO: sem refs no momento da
            // geração da cidade, ele publica o evento e o CraftingModal da cena abre o craft do tipo.
            var craftingPoint = station.AddComponent<CraftingPoint>();
            craftingPoint.Configure(stationId, type);
            EditorUtility.SetDirty(craftingPoint);
        }

        // Porta funcional no vão da casa (lado virado para a rua). Desenhada ACIMA do telhado
        // (sortingOrder > 20) para ser visível de fora; fechada tranca o vão (collider sólido),
        // aperte E para deslizar e abrir. A folha acompanha a orientação N/S/E/W do frontage.
        private static void CreateHouseDoor(Transform house, Vector3 localPosition, TownDoorSide side)
        {
            var door = new GameObject("Door");
            door.transform.SetParent(house);
            door.transform.localPosition = localPosition;

            bool horizontal = side == TownDoorSide.North || side == TownDoorSide.South;
            Vector3 panelScale = horizontal
                ? new Vector3(DoorGapWidth, WallThickness * 2f, 1f)
                : new Vector3(WallThickness * 2f, DoorGapWidth, 1f);
            Vector2 blockerSize = horizontal
                ? new Vector2(DoorGapWidth, WallThickness)
                : new Vector2(WallThickness, DoorGapWidth);
            Vector2 triggerSize = horizontal
                ? new Vector2(DoorGapWidth + 0.6f, WallThickness + 1.4f)
                : new Vector2(WallThickness + 1.4f, DoorGapWidth + 0.6f);

            // Umbral escuro (a abertura) atrás da folha — aparece quando a porta desliza.
            var threshold = new GameObject("Threshold");
            threshold.transform.SetParent(door.transform);
            threshold.transform.localPosition = Vector3.zero;
            threshold.transform.localScale = horizontal
                ? new Vector3(DoorGapWidth, WallThickness * 1.2f, 1f)
                : new Vector3(WallThickness * 1.2f, DoorGapWidth, 1f);
            var thr = threshold.AddComponent<SpriteRenderer>();
            thr.sprite = GetBuiltinSprite();
            thr.color = new Color(0.10f, 0.08f, 0.07f);
            thr.sortingOrder = 22; // acima do telhado (20) ⇒ visível de fora
            TrySetSortingLayer(thr, "Items", thr.sortingOrder);

            // Folha de madeira: desliza para o lado ao abrir.
            var leaf = new GameObject("Leaf");
            leaf.transform.SetParent(door.transform);
            var closedLocalPos = Vector3.zero;
            var openLocalPos = horizontal
                ? new Vector3(DoorGapWidth * 0.92f, 0f, 0f)
                : new Vector3(0f, DoorGapWidth * 0.92f, 0f);
            leaf.transform.localPosition = closedLocalPos;
            leaf.transform.localScale = panelScale;
            var leafRenderer = leaf.AddComponent<SpriteRenderer>();
            var doorSprite = WorldSpriteLibrary.Building("door_wood");
            leafRenderer.sprite = doorSprite != null ? doorSprite : GetBuiltinSprite();
            leafRenderer.color = doorSprite != null ? Color.white : new Color(0.34f, 0.22f, 0.13f);
            leafRenderer.sortingOrder = 23;
            TrySetSortingLayer(leafRenderer, "Items", leafRenderer.sortingOrder);

            // Collider SÓLIDO que tranca o vão (desligado quando aberta).
            var blocker = door.AddComponent<BoxCollider2D>();
            blocker.isTrigger = false;
            blocker.size = blockerSize;

            // Trigger de interação (sempre ligado): permite abrir de fora e fechar de dentro.
            var interact = door.AddComponent<BoxCollider2D>();
            interact.isTrigger = true;
            interact.size = triggerSize;

            var interactable = door.AddComponent<HouseDoorInteractable>();
            interactable.Configure(leaf.transform, blocker, closedLocalPos, openLocalPos);
            EditorUtility.SetDirty(interactable);
        }

        private static Color RoofTint(TownBuildingArchetype archetype)
        {
            switch (archetype)
            {
                case TownBuildingArchetype.Temple: return new Color(0.95f, 0.90f, 0.72f);
                case TownBuildingArchetype.Civic: return new Color(0.78f, 0.82f, 0.88f);
                case TownBuildingArchetype.Market: return new Color(1f, 0.82f, 0.58f);
                case TownBuildingArchetype.Workshop: return new Color(0.72f, 0.54f, 0.40f);
                case TownBuildingArchetype.Bakery: return new Color(0.86f, 0.46f, 0.24f);
                case TownBuildingArchetype.Forge: return new Color(0.30f, 0.28f, 0.28f);
                case TownBuildingArchetype.Alchemy: return new Color(0.25f, 0.55f, 0.48f);
                case TownBuildingArchetype.Tannery: return new Color(0.52f, 0.34f, 0.20f);
                case TownBuildingArchetype.Watermill: return new Color(0.28f, 0.42f, 0.56f);
                case TownBuildingArchetype.Warehouse: return new Color(0.48f, 0.28f, 0.16f);
                case TownBuildingArchetype.Noble: return new Color(0.42f, 0.30f, 0.62f);
                case TownBuildingArchetype.Rural: return new Color(0.72f, 0.82f, 0.62f);
                case TownBuildingArchetype.Guard: return new Color(0.66f, 0.70f, 0.78f);
                case TownBuildingArchetype.Inn: return new Color(0.88f, 0.64f, 0.48f);
                default: return Color.white;
            }
        }

        private static void CreateBuildingIdentityDetails(
            Transform house,
            string houseName,
            Vector2 size,
            TownDoorSide doorSide,
            TownBuildingArchetype archetype,
            Color baseColor)
        {
            var details = new GameObject("FacadeIdentity");
            details.transform.SetParent(house);
            details.transform.localPosition = Vector3.zero;

            var windowSprite = WorldSpriteLibrary.Building("window_wood");
            var signColor = Color.Lerp(baseColor, RoofTint(archetype), 0.45f);
            bool horizontal = doorSide == TownDoorSide.North || doorSide == TownDoorSide.South;
            float frontage = horizontal ? size.x : size.y;
            float halfFrontage = frontage * 0.5f;

            for (var i = 0; i < 2; i++)
            {
                float offset = i == 0 ? -halfFrontage * 0.55f : halfFrontage * 0.55f;
                var window = new GameObject($"Window_{i + 1}");
                window.transform.SetParent(details.transform);
                window.transform.localPosition = horizontal
                    ? new Vector3(offset, doorSide == TownDoorSide.South ? -size.y * 0.5f - 0.04f : size.y * 0.5f + 0.04f, 0f)
                    : new Vector3(doorSide == TownDoorSide.West ? -size.x * 0.5f - 0.04f : size.x * 0.5f + 0.04f, offset, 0f);
                window.transform.localScale = horizontal
                    ? new Vector3(0.9f, 0.55f, 1f)
                    : new Vector3(0.55f, 0.9f, 1f);
                var renderer = window.AddComponent<SpriteRenderer>();
                renderer.sprite = windowSprite != null ? windowSprite : GetBuiltinSprite();
                renderer.color = windowSprite != null ? Color.white : signColor;
                renderer.sortingOrder = 22;
                TrySetSortingLayer(renderer, "Items", renderer.sortingOrder);
            }

            var sign = new GameObject($"Sign_{houseName.Substring("House_".Length)}");
            sign.transform.SetParent(details.transform);
            sign.transform.localPosition = horizontal
                ? new Vector3(0f, doorSide == TownDoorSide.South ? -size.y * 0.5f - 0.18f : size.y * 0.5f + 0.18f, 0f)
                : new Vector3(doorSide == TownDoorSide.West ? -size.x * 0.5f - 0.18f : size.x * 0.5f + 0.18f, 0f, 0f);
            sign.transform.localScale = horizontal ? new Vector3(0.75f, 0.32f, 1f) : new Vector3(0.32f, 0.75f, 1f);
            var signRenderer = sign.AddComponent<SpriteRenderer>();
            signRenderer.sprite = GetBuiltinSprite();
            signRenderer.color = signColor;
            signRenderer.sortingOrder = 24;
            TrySetSortingLayer(signRenderer, "Items", signRenderer.sortingOrder);

            CreateSemanticBuildingPlaceholder(details.transform, houseName, size, doorSide);
        }

        // Replaceable semantic markers: they make each large placeholder readable before the final
        // Claude-authored sprites arrive, while preserving stable House_* roots and gameplay colliders.
        private static void CreateSemanticBuildingPlaceholder(Transform parent, string houseName, Vector2 size, TownDoorSide doorSide)
        {
            string token;
            Color color;
            switch (houseName)
            {
                case "House_Temple": token = "BellAndSanctuary"; color = new Color(0.30f, 0.50f, 0.78f); break;
                case "House_Manor": token = "NobleCrest"; color = new Color(0.24f, 0.44f, 0.70f); break;
                case "House_Chamber": token = "DarkStoneCivicSeal"; color = new Color(0.24f, 0.25f, 0.28f); break;
                case "House_Archive": token = "PurpleNobleCrest"; color = new Color(0.52f, 0.30f, 0.72f); break;
                case "House_MarketHall": token = "MarketAwning"; color = new Color(0.78f, 0.42f, 0.22f); break;
                case "House_Bakery": token = "BreadRoofSign"; color = new Color(0.92f, 0.68f, 0.28f); break;
                case "House_Inn": token = "MugRoofSign"; color = new Color(0.74f, 0.50f, 0.24f); break;
                case "House_Blacksmith": token = "ForgeFire"; color = new Color(0.90f, 0.28f, 0.10f); break;
                case "House_AlchemyLab": token = "PotionBottles"; color = new Color(0.28f, 0.78f, 0.68f); break;
                case "House_Workshop": token = "WoodAndTools"; color = new Color(0.62f, 0.40f, 0.20f); break;
                case "House_Tannery": token = "HangingHides"; color = new Color(0.64f, 0.42f, 0.24f); break;
                case "House_Fishery": token = "WaterWheel"; color = new Color(0.30f, 0.56f, 0.74f); break;
                case "House_AnimalYard": token = "WarehouseGate"; color = new Color(0.38f, 0.22f, 0.12f); break;
                default: token = "ResidenceNumber"; color = new Color(0.84f, 0.76f, 0.54f); break;
            }

            bool horizontal = doorSide == TownDoorSide.North || doorSide == TownDoorSide.South;
            var marker = new GameObject($"SemanticPlaceholder_{token}");
            marker.transform.SetParent(parent);
            marker.transform.localPosition = horizontal
                ? new Vector3(0f, (doorSide == TownDoorSide.South ? -1f : 1f) * (size.y * 0.5f + 0.32f), 0f)
                : new Vector3((doorSide == TownDoorSide.West ? -1f : 1f) * (size.x * 0.5f + 0.32f), 0f, 0f);
            marker.transform.localScale = horizontal ? new Vector3(1.8f, 0.65f, 1f) : new Vector3(0.65f, 1.8f, 1f);
            var renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBuiltinSprite();
            renderer.color = color;
            renderer.sortingOrder = 25;
            TrySetSortingLayer(renderer, "Items", renderer.sortingOrder);
        }

        // Auditoria de geração: avisa se duas casas se sobrepõem, se alguma sai do playfield, ou se um
        // spawn cai dentro de uma casa. Como não há Play Mode aqui, é a rede de segurança do layout.
        private static void AuditHouseOverlaps()
        {
            const float margin = 0.3f;
            for (int a = 0; a < TownHouseSpecs.Length; a++)
            {
                var (nameA, posA, _, sizeA) = TownHouseSpecs[a];
                float axMin = posA.x - sizeA.x * 0.5f, axMax = posA.x + sizeA.x * 0.5f;
                float ayMin = posA.y - sizeA.y * 0.5f, ayMax = posA.y + sizeA.y * 0.5f;

                if (axMin < -TownDistrictLayout.HalfWidth || axMax > TownDistrictLayout.HalfWidth ||
                    ayMin < -TownDistrictLayout.HalfHeight || ayMax > TownDistrictLayout.HalfHeight)
                {
                    Debug.LogError($"[walk-in-houses] {nameA} sai do playfield ({axMin:0.0}..{axMax:0.0}, {ayMin:0.0}..{ayMax:0.0}).");
                }

                for (int b = a + 1; b < TownHouseSpecs.Length; b++)
                {
                    var (nameB, posB, _, sizeB) = TownHouseSpecs[b];
                    bool overlapX = Mathf.Abs(posA.x - posB.x) < (sizeA.x + sizeB.x) * 0.5f + margin;
                    bool overlapY = Mathf.Abs(posA.y - posB.y) < (sizeA.y + sizeB.y) * 0.5f + margin;
                    if (overlapX && overlapY)
                    {
                        Debug.LogError($"[walk-in-houses] sobreposição: {nameA} × {nameB}.");
                    }
                }

                if (!TownCityLayout.TryGetBuilding(nameA, out var lot))
                {
                    Debug.LogError($"[town-layout] {nameA} não possui definição de lote.");
                    continue;
                }

                foreach (var road in TownCityLayout.AllRoads)
                {
                    if (lot.Overlaps(road))
                    {
                        Debug.LogError($"[town-layout] {nameA} invade a via {road.Id}.");
                    }
                }

                if (!TownCityLayout.IsPointOnRoad(lot.DoorApproach, 0.4f))
                {
                    Debug.LogError($"[town-layout] acesso da porta de {nameA} não alcança a malha viária.");
                }
            }

            Debug.Log($"[walk-in-houses] {TownHouseSpecs.Length} prédios percorríveis posicionados (auditoria concluída).");
        }

        // ─── fable_11: schedule anchors (work/social/home per NPC) ───────────────────────────────
        // Canonical social hubs by archetype: tavern (Gruta's corner), plaza center, night market.
        // Legacy authoring coordinates — repositioned into the 76×64 footprint at use.
        private static readonly Vector3 TavernSocialAnchorLegacy = new Vector3(9.5f, 5.2f, 0f);
        private static readonly Vector3 PlazaSocialAnchorLegacy = new Vector3(0f, -1.5f, 0f);
        private static readonly Vector3 NightMarketAnchorLegacy = new Vector3(11f, -9.5f, 0f);

        // ── Moradias temáticas (footprint 76×64) ──────────────────────────────────────────────────
        // Pontos de dormir AO RELENTO (no campo, y<24) — o NPC caminha até lá à noite e dorme à vista.
        // Coords no footprint; os landmarks visíveis são criados em CreateTownOutskirts nas MESMAS coords.
        // Footprint 76×64 — pontos de dormir ao relento, nos cantos/bordas (batem com as zonas reservadas).
        private static readonly Vector3 CemeteryPosition = new Vector3(-36f, 35f, 0f);
        private static readonly Vector3 CaveMouthPosition = new Vector3(57f, -4f, 0f);
        private static readonly Vector3 NightMarketBackPosition = new Vector3(54f, -16f, 0f);
        private static readonly Vector3 StatueGardenSleepPosition = new Vector3(-3f, -4f, 0f);    // praça — Liora

        private readonly struct HomeAssignment
        {
            public readonly bool IsIndoor;
            public readonly string HouseName;       // se indoor: nome em TownHouseSpecs
            public readonly Vector3 OutdoorPosition; // se outdoor: ponto no campo

            private HomeAssignment(bool indoor, string houseName, Vector3 pos)
            {
                IsIndoor = indoor; HouseName = houseName; OutdoorPosition = pos;
            }

            public static HomeAssignment Indoor(string houseName) => new HomeAssignment(true, houseName, Vector3.zero);
            public static HomeAssignment Outdoor(Vector3 pos) => new HomeAssignment(false, null, pos);
        }

        // Cada NPC dorme num lugar que combina com ele. Indoor = interior da casa (o jogador entra e o
        // encontra); Outdoor = dorme à vista no ponto temático. Não-mapeado cai no fallback (casa mais próxima).
        private static readonly Dictionary<string, HomeAssignment> TownNpcHomes = new Dictionary<string, HomeAssignment>
        {
            // Modelo MISTO (spec 15.2): compartilhar só com relação real; senão, casa própria.
            ["npc_corvus"] = HomeAssignment.Indoor("House_Temple"),       // padre mora na igreja
            ["npc_velorin"] = HomeAssignment.Indoor("House_Manor"),       // líder na mansão
            ["npc_orlan"] = HomeAssignment.Indoor("House_Inn"),           // ┐ tocam a estalagem juntos
            ["npc_gruta"] = HomeAssignment.Indoor("House_Inn"),           // ┘ (compartilhada)
            ["npc_gurd"] = HomeAssignment.Indoor("House_CarvalhoTorto"),  // ┐ família Carvalho-Torto
            ["npc_hund"] = HomeAssignment.Indoor("House_CarvalhoTorto"),  // ┘ (compartilhada)
            ["npc_mara"] = HomeAssignment.Indoor("House_Registry"),       // casa própria
            ["npc_tovin"] = HomeAssignment.Indoor("House_Tovin"),         // casa própria
            ["npc_brumdar"] = HomeAssignment.Indoor("House_Blacksmith"),  // casa própria
            ["npc_dagna"] = HomeAssignment.Indoor("House_Dagna"),         // casa própria
            ["npc_thalindra"] = HomeAssignment.Indoor("House_Archive"),   // casa própria
            ["npc_nimble"] = HomeAssignment.Indoor("House_Workshop"),     // casa própria
            ["npc_ozzra"] = HomeAssignment.Indoor("House_AlchemyLab"),    // casa própria
            ["npc_eiran"] = HomeAssignment.Indoor("House_AnimalYard"),    // casa própria
            ["npc_alaric"] = HomeAssignment.Indoor("House_GateKeeper"),   // casa própria
            ["npc_pip"] = HomeAssignment.Indoor("House_Pip"),             // casa própria
            ["npc_sylveth"] = HomeAssignment.Indoor("House_Residential_1"),
            ["npc_renko"] = HomeAssignment.Indoor("House_Residential_2"),
            ["npc_mirela"] = HomeAssignment.Indoor("House_Residential_3"),
            ["npc_savra"] = HomeAssignment.Indoor("House_Residential_4"),
            // Ao relento, em pontos temáticos:
            ["npc_maelor"] = HomeAssignment.Outdoor(CemeteryPosition),       // dorme no cemitério (noturno, sem memória)
            ["npc_yael"] = HomeAssignment.Outdoor(NightMarketBackPosition),  // fundo escondido do mercado noturno
            ["npc_liora"] = HomeAssignment.Outdoor(StatueGardenSleepPosition), // jardim da estátua, ao relento
            ["npc_zrix"] = HomeAssignment.Outdoor(CaveMouthPosition),        // batedor dorme na boca da gruta
            // Autossuficiência da vila (slice village_economy):
            ["npc_sael"] = HomeAssignment.Indoor("House_Fishery"),           // pescador mora na pescaria
            ["npc_mella"] = HomeAssignment.Indoor("House_Bakery"),           // padeira mora na padaria
            ["npc_hess"] = HomeAssignment.Indoor("House_Tannery"),           // curtidor mora na tanoaria
            ["npc_tibbet"] = HomeAssignment.Outdoor(CemeteryPosition + new Vector3(3.5f, -1.5f, 0f)), // coveiro dorme entre as covas
        };

        private static int HouseIndexByName(string houseName)
        {
            for (int i = 0; i < TownHouseSpecs.Length; i++)
            {
                if (TownHouseSpecs[i].name == houseName) return i;
            }
            return -1;
        }

        // Resolve o ponto do anchor "home" do NPC pela moradia temática (indoor → interior da casa;
        // outdoor → ponto no campo). Fallback: interior da casa mais próxima (comportamento anterior).
        private static Vector3 ResolveHomeAnchorPosition(TownNpcSpec spec)
        {
            if (TownNpcHomes.TryGetValue(spec.NpcId, out var home))
            {
                if (home.IsIndoor)
                {
                    int hi = HouseIndexByName(home.HouseName);
                    if (hi >= 0) return InteriorCenterForHouseIndex(hi) + new Vector3(0f, -0.6f, 0f);
                }
                else
                {
                    return home.OutdoorPosition;
                }
            }

            int nearest = NearestHouseIndexTo(spec.LayoutPosition);
            return nearest >= 0
                ? InteriorCenterForHouseIndex(nearest) + new Vector3(0f, -0.6f, 0f)
                : spec.LayoutPosition;
        }

        private static void CreateNpcScheduleAnchors()
        {
            var parent = new GameObject("NpcScheduleAnchors");
            parent.transform.position = Vector3.zero;

            int index = 0;

            foreach (var spec in RefinedCanonicalTownNpcSpecs)
            {
                var archetype = NpcScheduleBlockResolver.ArchetypeFromMovementProfile(
                    spec.MovementProfile, !string.IsNullOrWhiteSpace(spec.ShopDataPath));

                // Work = role-specific building frontage, desk, route or landmark.
                CreateScheduleAnchor(parent.transform, spec.NpcId,
                    NpcScheduleBlockResolver.WorkAnchorSuffix, spec.LayoutPosition);

                // Social = explicit role-appropriate hub; fallback preserves the prior archetype rule.
                Vector3 socialLegacy = archetype == NpcScheduleArchetype.Night ? NightMarketAnchorLegacy
                    : (spec.Position.y > 3f ? TavernSocialAnchorLegacy : PlazaSocialAnchorLegacy);
                var socialFallback = TownDistrictLayout.Reposition(socialLegacy);
                CreateScheduleAnchor(parent.transform, spec.NpcId,
                    NpcScheduleBlockResolver.SocialAnchorSuffix,
                    TownCityLayout.ResolveNpcSocialPosition(spec.NpcId, socialFallback));

                // Home TEMÁTICO: cada NPC dorme num lugar que combina com ele (templo, forja, oficina,
                // arquivo, lab, curral, posto/cemitério, boca de gruta, jardim, ou casa residencial).
                // Indoor → interior da casa (entra para encontrar o NPC à noite); Outdoor → dorme à
                // vista no ponto temático. Mapa em TownNpcHomes; fallback = casa mais próxima.
                var home = ResolveHomeAnchorPosition(spec);
                bool hasDoor = TryResolveHomeDoorApproach(spec, out var doorApproach);
                CreateScheduleAnchor(parent.transform, spec.NpcId,
                    NpcScheduleBlockResolver.HomeAnchorSuffix, home, hasDoor, doorApproach);

                index++;
            }

            Debug.Log($"[fable_11] Created schedule anchors for {index} NPC(s) " +
                      $"({index * 3} anchors: work/social/home).");
        }

        private static void CreateScheduleAnchor(Transform parent, string npcId, string suffix, Vector3 position,
            bool hasApproach = false, Vector3 approachPoint = default)
        {
            var anchorId = $"npc_{npcId}_{suffix}";
            var go = new GameObject($"Anchor_{anchorId}");
            go.transform.SetParent(parent);
            go.transform.position = position;

            var anchor = go.AddComponent<NpcScheduleAnchor>();
            var serialized = new SerializedObject(anchor);
            SetSerializedString(serialized, "_anchorId", anchorId);
            SetSerializedString(serialized, "_npcId", npcId);
            serialized.FindProperty("_hasApproach").boolValue = hasApproach;
            serialized.FindProperty("_approachPoint").vector3Value = approachPoint;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(anchor);
        }

        // Ponto de aproximação da PORTA, logo fora da casa do morador (indoor). O NPC passa por aqui
        // antes de entrar ⇒ alinha com o vão e usa a porta em vez de cruzar a parede. A porta vira para
        // a rua (sul no norte da cidade / norte no sul), então o ponto fica do lado de fora da porta.
        private static bool TryResolveHomeDoorApproach(TownNpcSpec spec, out Vector3 approach)
        {
            approach = default;
            if (!TownNpcHomes.TryGetValue(spec.NpcId, out var home) || !home.IsIndoor)
            {
                return false;
            }

            int hi = HouseIndexByName(home.HouseName);
            if (hi < 0)
            {
                return false;
            }

            if (!TownCityLayout.TryGetBuilding(home.HouseName, out var lot))
            {
                return false;
            }

            approach = lot.DoorApproach;
            return TownCityLayout.IsPointOnRoad(approach, 0.4f);
        }

        // (As casas agora são FÍSICAS percorríveis — ver CreateWalkInHouse. A antiga faixa off-field de
        //  interiores (y>+40) + portas de teleporte foi removida. CreateInteriorWall/CreateInteriorProp
        //  permanecem como helpers reutilizados pela casa percorrível.)
        private static void CreateInteriorWall(Transform parent, string name, Vector3 localPos, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.localPosition = localPos;
            var collider = wall.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
            collider.size = size;

            AddStoneWallVisual(wall.transform, size);
        }

        // Visual de PAREDE DE PEDRA: contorno escuro (junta) + face de pedra + uma faixa de base mais
        // escura no rodapé. Lê como alvenaria de verdade quando o jogador entra (não um bloco chapado).
        private static void AddStoneWallVisual(Transform parent, Vector2 size)
        {
            // Contorno escuro (um pouco maior, atrás) — dá borda à parede.
            var outline = new GameObject("Outline");
            outline.transform.SetParent(parent);
            outline.transform.localPosition = Vector3.zero;
            outline.transform.localScale = new Vector3(size.x + 0.18f, size.y + 0.18f, 1f);
            var outlineRenderer = outline.AddComponent<SpriteRenderer>();
            outlineRenderer.sprite = GetBuiltinSprite();
            outlineRenderer.color = new Color(0.20f, 0.18f, 0.16f);
            outlineRenderer.sortingOrder = 4;
            TrySetSortingLayer(outlineRenderer, "Items", outlineRenderer.sortingOrder);

            // Face de pedra (cinza-quente).
            var face = new GameObject("Face");
            face.transform.SetParent(parent);
            face.transform.localPosition = Vector3.zero;
            var faceRenderer = face.AddComponent<SpriteRenderer>();
            var wallTile = WorldSpriteLibrary.Building("wall_stone");
            if (wallTile != null)
            {
                face.transform.localScale = Vector3.one;
                faceRenderer.sprite = wallTile;
                faceRenderer.color = Color.white;
                faceRenderer.drawMode = SpriteDrawMode.Tiled;
                faceRenderer.tileMode = SpriteTileMode.Continuous;
                faceRenderer.size = size;
            }
            else
            {
                face.transform.localScale = new Vector3(size.x, size.y, 1f);
                faceRenderer.sprite = GetBuiltinSprite();
                faceRenderer.color = new Color(0.57f, 0.54f, 0.49f);
            }
            faceRenderer.sortingOrder = 5;
            TrySetSortingLayer(faceRenderer, "Items", faceRenderer.sortingOrder);

            // Rodapé mais escuro (faixa de base) — sensação de espessura/sombra da parede.
            var baseStrip = new GameObject("Base");
            baseStrip.transform.SetParent(parent);
            baseStrip.transform.localPosition = new Vector3(0f, -size.y * 0.32f, 0f);
            baseStrip.transform.localScale = new Vector3(size.x, size.y * 0.34f, 1f);
            var baseRenderer = baseStrip.AddComponent<SpriteRenderer>();
            baseRenderer.sprite = GetBuiltinSprite();
            baseRenderer.color = new Color(0.42f, 0.39f, 0.35f);
            baseRenderer.sortingOrder = 6;
            TrySetSortingLayer(baseRenderer, "Items", baseRenderer.sortingOrder);
        }

        private static GameObject CreateInteriorProp(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
        {
            var prop = new GameObject(name);
            prop.transform.SetParent(parent);
            prop.transform.localPosition = localPos;
            prop.transform.localScale = scale;
            var renderer = prop.AddComponent<SpriteRenderer>();
            var furniture = InteriorSpriteFor(name);
            if (furniture != null) { renderer.sprite = furniture; renderer.color = Color.white; }
            else { renderer.sprite = GetBuiltinSprite(); renderer.color = color; }
            renderer.sortingOrder = 1;
            TrySetSortingLayer(renderer, "Items", renderer.sortingOrder);
            return prop;
        }

        // Mapeia o nome do movel interno -> sprite em Art/Generated/World/interior. null = sem match
        // (usa fallback colorido no chamador). Estacoes de oficio (Station_*) ficam com cor propria.
        private static Sprite InteriorSpriteFor(string propName)
        {
            string key = null;
            if (propName == "Bed" || propName == "GuestBed_Inn") key = "bed";
            else if (propName == "Table") key = "table";
            else if (propName == "Furniture_Shelf") key = "cupboard";
            else if (propName == "Furniture_Rug") key = "rug";
            else if (propName == "Furniture_Pew") key = "chair";
            return key != null ? WorldSpriteLibrary.Interior(key) : null;
        }

        // Props decorativos da cidade (poco, fonte, lampioes, bancos, cerca, placa). Puramente visuais
        // (sem collider) — dao vida a cidade que antes era so chao + casas. Sprites em World/props;
        // fallback cinza se ausente. Posicoes proximas das ruas/praca central (footprint 76x64).
        private static void CreateTownProps()
        {
            var parent = new GameObject("TownProps");
            parent.transform.position = Vector3.zero;

            void P(string name, string key, Vector3 pos, float scale)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent.transform);
                go.transform.position = pos;
                go.transform.localScale = new Vector3(scale, scale, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                var sprite = WorldSpriteLibrary.Prop(key);
                if (sprite != null) { sr.sprite = sprite; sr.color = Color.white; }
                else { sr.sprite = GetBuiltinSprite(); sr.color = new Color(0.6f, 0.6f, 0.6f); }
                sr.sortingOrder = 2;
                TrySetSortingLayer(sr, "Items", sr.sortingOrder);
            }

            P("Well", "well", new Vector3(-7f, 6f, 0f), 2.2f);
            P("Fountain", "fountain", new Vector3(7f, -6f, 0f), 2.4f);
            P("Streetlamp_1", "streetlamp", new Vector3(3.2f, 7.5f, 0f), 2.0f);
            P("Streetlamp_2", "streetlamp", new Vector3(3.2f, -7.5f, 0f), 2.0f);
            P("Streetlamp_3", "streetlamp", new Vector3(-9.5f, 0.5f, 0f), 2.0f);
            P("Streetlamp_4", "streetlamp", new Vector3(11.5f, 0.5f, 0f), 2.0f);
            P("Bench_1", "bench", new Vector3(5f, 2.6f, 0f), 1.6f);
            P("Bench_2", "bench", new Vector3(-5f, -2.6f, 0f), 1.6f);
            P("SignPost", "sign_post", new Vector3(0.5f, 3.4f, 0f), 1.5f);
            for (int i = 0; i < 5; i++)
            {
                P($"Fence_{i}", "fence", new Vector3(-14f + i * 1.4f, -8.6f, 0f), 1.4f);
            }
        }

        // Centro do interior FÍSICO da casa de índice i (coords finais, no MESMO lugar do exterior).
        // Consumido pelo anchor "home" do morador → o NPC dorme dentro da própria casa percorrível.
        private static Vector3 InteriorCenterForHouseIndex(int i)
        {
            return TownHouseSpecs[i].position;
        }

        // Índice da casa (em TownHouseSpecs) mais próxima de uma posição no footprint final.
        // Usado para mandar o morador "dormir" dentro da casa percorrível mais perto do seu posto.
        private static int NearestHouseIndexTo(Vector3 layoutPosition)
        {
            int nearest = -1;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < TownHouseSpecs.Length; i++)
            {
                // TownHouseSpecs já está em coords finais (sem Reposition) — comparar direto.
                var housePos = TownHouseSpecs[i].position;
                float sqr = (housePos - layoutPosition).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = i;
                }
            }

            return nearest;
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

            // Legacy town-wide props are repositioned into the 76×64 footprint via R().
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
            var lakeRenderer = lake.AddComponent<SpriteRenderer>();
            var lakeTile = WorldSpriteLibrary.Ground("ground_water");
            if (lakeTile != null)
            {
                lake.transform.localScale = Vector3.one;
                lakeRenderer.sprite = lakeTile; lakeRenderer.color = Color.white;
                lakeRenderer.drawMode = SpriteDrawMode.Tiled; lakeRenderer.tileMode = SpriteTileMode.Continuous;
                lakeRenderer.size = new Vector2(18f, 12f);
            }
            else
            {
                lake.transform.localScale = new Vector3(18f, 12f, 1f);
                lakeRenderer.sprite = GetBuiltinSprite(); lakeRenderer.color = new Color(0.27f, 0.45f, 0.62f);
            }
            lakeRenderer.sortingOrder = 0;
            TrySetSortingLayer(lakeRenderer, "Ground", lakeRenderer.sortingOrder);

            var lakeCollider = lake.AddComponent<BoxCollider2D>();
            lakeCollider.isTrigger = false;
            lakeCollider.size = new Vector2(18f, 12f);

            // Cais no lado sul: ponto de leitura para Sael e aproximação segura sem entrar na água.
            CreateDecoration(
                district.transform,
                "LakeDock_South",
                TownDistrictLayout.LakeCenter + new Vector3(0f, -6.6f, 0f),
                new Vector3(4.5f, 1.4f, 1f),
                new Color(0.46f, 0.34f, 0.22f));

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
            var hallRenderer = hall.AddComponent<SpriteRenderer>();
            var hallTile = WorldSpriteLibrary.Building("wall_stone");
            if (hallTile != null)
            {
                hall.transform.localScale = Vector3.one;
                hallRenderer.sprite = hallTile; hallRenderer.color = Color.white;
                hallRenderer.drawMode = SpriteDrawMode.Tiled; hallRenderer.tileMode = SpriteTileMode.Continuous;
                hallRenderer.size = new Vector2(4.5f, 3.2f);
            }
            else
            {
                hall.transform.localScale = new Vector3(4.5f, 3.2f, 1f);
                hallRenderer.sprite = GetBuiltinSprite(); hallRenderer.color = new Color(0.6f, 0.58f, 0.52f);
            }
            hallRenderer.sortingOrder = 2;
            TrySetSortingLayer(hallRenderer, "Items", hallRenderer.sortingOrder);
            var hallCollider = hall.AddComponent<BoxCollider2D>();
            hallCollider.isTrigger = false;
            hallCollider.size = Vector2.one;

            // Mural on the south wall of the town hall (fable_34 — read-only announcements).
            CreateDecoration(district.transform, "TownHallMural", TownDistrictLayout.TownHallMural, new Vector3(3.2f, 0.9f, 1f), new Color(0.7f, 0.55f, 0.4f));
            CreateMuralInteractable(district.transform, TownDistrictLayout.TownHallMural);

            // fable_34 — Hund's notice board (Board_Contratos) just south of the town hall.
            CreateNoticeBoard(district.transform, TownDistrictLayout.TownHallMural + new Vector3(-2.5f, -1.6f, 0f));
        }

        // fable_34 — read-only mural interactable (QuestSource.Mural). Trigger collider + component.
        private static void CreateMuralInteractable(Transform parent, Vector3 position)
        {
            var mural = new GameObject("TownHallMural_Interactable");
            mural.transform.SetParent(parent);
            mural.transform.position = position;
            mural.transform.localScale = new Vector3(3.2f, 0.9f, 1f);

            var collider = mural.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            mural.AddComponent<CindarsHope.Quests.Runtime.MuralInteractable>();
        }

        // fable_34 — notice board interactable (Board_Contratos / QuestSource.Board). The daily
        // contracts are generated at runtime by QuestRuntimeBootstrap; this board exposes the
        // accept/turn-in interaction. Sprite + trigger + QuestBoardInteractable.
        private static void CreateNoticeBoard(Transform parent, Vector3 position)
        {
            var board = new GameObject("Board_Contratos");
            board.transform.SetParent(parent);
            board.transform.position = position;
            board.transform.localScale = new Vector3(1.1f, 1.4f, 1f);

            var renderer = board.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBuiltinSprite();
            renderer.color = new Color(0.55f, 0.4f, 0.25f);
            renderer.sortingOrder = 2;
            TrySetSortingLayer(renderer, "Items", renderer.sortingOrder);

            var collider = board.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            board.AddComponent<CindarsHope.Quests.Runtime.QuestBoardInteractable>();
        }

        // Algumas árvores espalhadas DENTRO da cidade (pedido do jogador), além da floresta da borda.
        // Posições em pontos de baixa densidade de casas (cantos da praça + entre distritos); cada uma
        // vira uma pequena zona reservada (ReservedZones) para nenhuma casa cair em cima.
        private static readonly Vector3[] ScatteredTreePositions =
        {
            new Vector3(-57f, -10f, 0f), new Vector3(-56f, -21f, 0f),
            new Vector3(-45f, -6f, 0f), new Vector3(-38f, -8f, 0f),
            new Vector3(-10f, 12f, 0f), new Vector3(10f, 12f, 0f),
            new Vector3(-10f, -12f, 0f), new Vector3(10f, -12f, 0f),
        };

        // Floresta da borda: agora fica POR FORA da muralha de pedra (a muralha é o limite visível;
        // a mata é a profundidade atrás dela). Deixa um vão central ao SUL para o portão da fazenda.
        private static readonly Vector3[] TownTreePositions = BuildBorderTreeRing();

        private static Vector3[] BuildBorderTreeRing()
        {
            var list = new List<Vector3>();
            float wallX = TownDistrictLayout.HalfWidth + 0.5f;   // 38.5 — linha da muralha oeste/leste
            float wallY = TownDistrictLayout.HalfHeight + 0.5f;  // 32.5 — linha da muralha norte/sul
            float[] depths = { 1.2f, 2.8f, 4.4f };  // TRÊS fileiras FORA da muralha (mata atrás do muro)
            const float step = 2.6f;     // árvores (~3 tiles) se sobrepõem ⇒ cada fileira fecha totalmente
            const float gateHalf = 4f;   // metade do corredor sul (entrada da cidade / saída p/ fazenda)

            foreach (float d in depths)
            {
                float tx = wallX + d;
                float ty = wallY + d;

                // Norte completo; sul preserva o corredor do portão em todas as profundidades.
                for (float x = -tx; x <= tx + 0.01f; x += step)
                {
                    list.Add(new Vector3(x, ty, 0f));
                    if (Mathf.Abs(x) > gateHalf)
                    {
                        list.Add(new Vector3(x, -ty, 0f));
                    }
                }

                // Laterais oeste/leste completas.
                for (float y = -wallY + step; y <= wallY - step + 0.01f; y += step)
                {
                    list.Add(new Vector3(-tx, y, 0f));
                    list.Add(new Vector3(tx, y, 0f));
                }
            }

            // Árvores espalhadas DENTRO da cidade (alguns pontos, fora da borda).
            list.AddRange(ScatteredTreePositions);
            return list.ToArray();
        }

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
            // Posições já estão no footprint 120x90 (anel de borda) — sem reposition.
            treeObject.transform.position = position;
            // Pequena variação determinística por índice para o anel não parecer um carimbo repetido.
            float scale = 2.7f + (treeIndex % 3) * 0.3f;
            treeObject.transform.localScale = new Vector3(scale, scale, 1f);

            var spriteRenderer = treeObject.AddComponent<SpriteRenderer>();
            string[] townTreeSpecies = { "tree_oak", "tree_pine", "tree_apple" };
            var townTreeSprite = WorldSpriteLibrary.Tree(townTreeSpecies[treeIndex % townTreeSpecies.Length]);
            if (townTreeSprite != null) { spriteRenderer.sprite = townTreeSprite; spriteRenderer.color = Color.white; }
            else { spriteRenderer.sprite = GetBuiltinSprite(); float greenShift = (treeIndex % 4) * 0.025f; spriteRenderer.color = new Color(0.22f + greenShift, 0.45f + greenShift, 0.2f); }
            spriteRenderer.sortingOrder = 2;
            TrySetSortingLayer(spriteRenderer, "Items", spriteRenderer.sortingOrder);

            bool isInternalTree = treeIndex >= TownTreePositions.Length - ScatteredTreePositions.Length;
            if (isInternalTree)
            {
                // World-space target ≈0.65×0.50 around the trunk/base. The object itself is scaled,
                // so collider local size is divided by that scale to avoid a canopy-sized blocker.
                var collider = treeObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = false;
                collider.size = new Vector2(0.65f / scale, 0.50f / scale);
                collider.offset = new Vector2(0f, -0.34f / scale);
            }
        }

        // Arredores temáticos na margem do footprint 76×64 — locais de dormir ao relento.
        // As posições batem com TownNpcHomes (Maelor→cemitério, Zrix→gruta, Yael→tenda noturna).
        private static void CreateTownOutskirts()
        {
            var parent = new GameObject("TownOutskirts");
            parent.transform.position = Vector3.zero;

            // Cemitério (NW) AMPLIADO: terreno de terra + cripta destacada + 9 lápides + cerca com portão.
            CreateGroundSlab(parent.transform, "Cemetery_Ground", CemeteryPosition, new Vector2(11f, 9f), new Color(0.52f, 0.50f, 0.46f), -1);
            CreateDecoration(parent.transform, "Cemetery_Crypt",
                CemeteryPosition + new Vector3(0f, 2.4f, 0f), new Vector3(3.4f, 2.4f, 1f), new Color(0.45f, 0.45f, 0.5f));
            for (int i = 0; i < 9; i++)
            {
                int col = i % 3, row = i / 3;
                float gx = CemeteryPosition.x - 3.2f + col * 1.6f;
                float gy = CemeteryPosition.y - 0.2f - row * 1.7f;
                CreateDecoration(parent.transform, $"Gravestone_{i:00}",
                    new Vector3(gx, gy, 0f), new Vector3(0.6f, 0.9f, 1f), new Color(0.56f, 0.56f, 0.59f));
            }
            // Cerca completa, decorativa e sem colisão, com vão de portão no sul-centro.
            var fenceColor = new Color(0.30f, 0.30f, 0.34f);
            CreateDecoration(parent.transform, "CemeteryFence_S_L",
                new Vector3(CemeteryPosition.x - 3.6f, CemeteryPosition.y - 4.3f, 0f), new Vector3(3.2f, 0.3f, 1f), fenceColor);
            CreateDecoration(parent.transform, "CemeteryFence_S_R",
                new Vector3(CemeteryPosition.x + 3.6f, CemeteryPosition.y - 4.3f, 0f), new Vector3(3.2f, 0.3f, 1f), fenceColor);
            CreateDecoration(parent.transform, "CemeteryGate_Post_L",
                new Vector3(CemeteryPosition.x - 1.0f, CemeteryPosition.y - 4.3f, 0f), new Vector3(0.3f, 0.9f, 1f), fenceColor);
            CreateDecoration(parent.transform, "CemeteryGate_Post_R",
                new Vector3(CemeteryPosition.x + 1.0f, CemeteryPosition.y - 4.3f, 0f), new Vector3(0.3f, 0.9f, 1f), fenceColor);
            CreateDecoration(parent.transform, "CemeteryFence_N",
                new Vector3(CemeteryPosition.x, CemeteryPosition.y + 4.3f, 0f), new Vector3(10.4f, 0.3f, 1f), fenceColor);
            CreateDecoration(parent.transform, "CemeteryFence_W",
                new Vector3(CemeteryPosition.x - 5.2f, CemeteryPosition.y, 0f), new Vector3(0.3f, 8.5f, 1f), fenceColor);
            CreateDecoration(parent.transform, "CemeteryFence_E",
                new Vector3(CemeteryPosition.x + 5.2f, CemeteryPosition.y, 0f), new Vector3(0.3f, 8.5f, 1f), fenceColor);

            // Boca de gruta (E): duas rochas (com colisão) e a abertura escura no meio. Onde Zrix dorme.
            CreateBlocker(parent.transform, "CaveMouth_RockL",
                new Vector3(CaveMouthPosition.x - 1.7f, CaveMouthPosition.y + 0.2f, 0f), new Vector2(1.6f, 2.4f), new Color(0.40f, 0.38f, 0.36f));
            CreateBlocker(parent.transform, "CaveMouth_RockR",
                new Vector3(CaveMouthPosition.x + 1.7f, CaveMouthPosition.y + 0.2f, 0f), new Vector2(1.6f, 2.4f), new Color(0.40f, 0.38f, 0.36f));
            CreateDecoration(parent.transform, "CaveMouth_Dark",
                CaveMouthPosition, new Vector3(2.2f, 2.6f, 1f), new Color(0.08f, 0.07f, 0.1f));

            // Tenda escondida do mercado noturno (S): onde Yael dorme, no fundo, longe dos olhos.
            CreateDecoration(parent.transform, "NightTent_Hidden",
                NightMarketBackPosition, new Vector3(2.4f, 1.7f, 1f), new Color(0.30f, 0.26f, 0.5f));
        }

        // ── Estrutura de chão: ruas, pads de distrito e construções de preenchimento ──────────────

        // Slab de chão puramente visual (Ground, sortingOrder dado; sem collider).
        private static void CreateGroundSlab(Transform parent, string name, Vector3 center, Vector2 size, Color color, int sortingOrder, string tileName = null)
        {
            var tile = tileName != null ? WorldSpriteLibrary.Ground(tileName) : null;
            if (tile != null)
            {
                // Chao via TILEMAP (best practice) — sem SpriteRenderer Tiled (estoura mesh) nem esticado.
                // Uma camada por sortingOrder dentro de um Grid compartilhado neste parent.
                float cs = WorldTilemapGround.SpriteWorldSize(tile);
                var tm = WorldTilemapGround.GetOrCreateLayer(parent, "TownWorldGrid", $"Ground_{sortingOrder}", cs, sortingOrder, "Ground");
                WorldTilemapGround.PaintRect(tm, tile, new Vector2(center.x, center.y), size);
                return;
            }
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = center;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetBuiltinSprite();
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            TrySetSortingLayer(sr, "Ground", sortingOrder);
        }

        // Malha determinística: eixos principais + frontages de cada fileira de lotes. A mesma fonte
        // é usada pelos testes de acesso das portas e pelos destinos dos NPCs.
        private static void CreateTownRoads()
        {
            var parent = new GameObject("TownRoads");
            parent.transform.position = Vector3.zero;
            var dirt = new Color(0.62f, 0.55f, 0.42f);
            foreach (var road in TownCityLayout.AllRoads)
            {
                CreateGroundSlab(
                    parent.transform,
                    $"Road_{road.Id}",
                    new Vector3(road.Center.x, road.Center.y, 0f),
                    road.Size,
                    dirt,
                    -1,
                    "ground_cobble");
            }
        }

        // Pads de distrito: zonas de chão de tom levemente distinto sob cada cluster, para os grupos
        // lerem como bairros. Contraste baixo (não vira patchwork). Ground, sortingOrder -2 (sob as ruas).
        private static void CreateDistrictPads()
        {
            var parent = new GameObject("TownDistrictPads");
            parent.transform.position = Vector3.zero;
            // Grama base COM VARIACAO sob TODA a cidade (best practice: base + variantes esparsas via
            // Tilemap). Substitui a laje unica lisa + os pads de distrito (que cobriam a variacao).
            WorldTilemapGround.PaintGrass(parent.transform, "TownWorldGrid", -6, "Ground", Vector2.zero,
                new Vector2(TownDistrictLayout.WidthTiles + 6f, TownDistrictLayout.HeightTiles + 6f));
        }

        // Construções de preenchimento (com colisão) nos vazios entre distritos: dão volume à cidade
        // e bloqueiam movimento. Decorativas (sem lógica). Posicionadas longe das ruas centrais.
        // (CreateFillerBuildings removido: eram blocos sólidos puramente decorativos, sem motivo de
        //  gameplay. CreateBlocker permanece — usado pelas rochas da boca de gruta, que de fato BLOQUEIAM
        //  a entrada da gruta, isto é, têm motivo para colidir.)

        // Bloqueador com motivo de gameplay: sprite + collider sólido (ex.: rochas que trancam a gruta).
        private static void CreateBlocker(Transform parent, string name, Vector3 center, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = center;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            var blockerRock = WorldSpriteLibrary.Prop("rock_ore_0");
            if (blockerRock != null) { sr.sprite = blockerRock; sr.color = Color.white; }
            else { sr.sprite = GetBuiltinSprite(); sr.color = color; }
            sr.sortingOrder = 2;
            TrySetSortingLayer(sr, "Items", sr.sortingOrder);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = Vector2.one;
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
            // Match 0.5 (largura+altura): com match=0 (so largura), telas mais altas/estreitas
            // escalavam a UI pela largura e empurravam o rodape do painel para fora da tela.
            canvasScaler.matchWidthOrHeight = 0.5f;

            new GameObject("ShopEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            // Caixa de dialogo no rodape. Altura 300 para o prompt + ate ~6 escolhas caberem DENTRO da caixa
            // (antes 250 com container de 120 deixava a 5a/6a opcao — "Adeus" — escapar pelo rodape). O cartao
            // de retrato do NPC ancora colado a esquerda dela (CreateNpcPortraitPanel), nao no canto da tela.
            var dialoguePanel = CreatePanel(canvasObject.transform, "DialogueModal", new Vector2(0f, -195f), new Vector2(820f, 300f));
            AnchorToBottomCenter(dialoguePanel, 28f);
            var dialogueText = CreateText(dialoguePanel.transform, "DialogueText", new Vector2(0f, 108f), new Vector2(740f, 60f), string.Empty);
            var continueButton = CreateButton(dialoguePanel.transform, "ContinueButton", new Vector2(0f, -118f), new Vector2(180f, 42f), "Continuar");
            // Container de escolhas: topo logo abaixo do prompt (~75) descendo ate ~-133, dentro da caixa.
            // 6 linhas de 28px + spacing 8 = 208 <= 210; childControlHeight=false mantem cada linha em 28.
            var choicesContainer = CreateContainer(dialoguePanel.transform, "Choices", new Vector2(0f, -30f), new Vector2(740f, 210f));
            var choiceTemplate = CreateButton(choicesContainer, "ChoiceTemplate", Vector2.zero, new Vector2(720f, 28f), string.Empty);
            choiceTemplate.gameObject.AddComponent<LayoutElement>().preferredHeight = 28f;
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

            // Painel lateral de retrato + afinidade do NPC (aparece ao conversar). Self-wiring por eventos.
            CreateNpcPortraitPanel(canvasObject.transform);

            modalManager.Initialize();
            return new ShopUiReferences
            {
                DialogueModal = dialogue,
                ShopMenuModal = menu,
                BuyPanel = buy,
                SellPanel = sell
            };
        }

        // Painel lateral (esquerda) com retrato placeholder, nome/raça/papel e a barra de afinidade
        // vermelho→amarelo→verde. O NpcInteractionPortraitHud se liga sozinho aos eventos de interação
        // e de opinião; aqui só montamos a hierarquia e plugamos as referências serializadas.
        private static void CreateNpcPortraitPanel(Transform canvasParent)
        {
            var panelGo = new GameObject("NpcPortraitPanel",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            panelGo.transform.SetParent(canvasParent, false);
            // Cartao ancorado ao CENTRO-RODAPE, a esquerda da caixa de dialogo (820 de largura => meia-largura
            // 410). Pivot direito-baixo: borda direita em -424 deixa ~14px de respiro do dialogo (antes -400
            // colava). Maior (230x326) e elevado (y=22) para "saltar" acima da caixa e ter destaque proprio.
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.sizeDelta = new Vector2(230f, 326f);
            panelRect.anchoredPosition = new Vector2(-424f, 22f);
            panelGo.GetComponent<Image>().color = new Color(0.11f, 0.12f, 0.16f, 0.96f);
            // Contorno fino para destacar o cartao do fundo / da caixa de dialogo.
            var cardOutline = panelGo.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.85f, 0.72f, 0.35f, 0.55f);
            cardOutline.effectDistance = new Vector2(2f, -2f);
            var group = panelGo.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            // Faixa de acento (dourada) no topo — leitura de "cabecalho do cartao".
            CreatePanelImage(panelGo.transform, "TopAccent", new Vector2(0f, 153f), new Vector2(218f, 6f), new Color(0.85f, 0.72f, 0.35f, 0.95f));

            // Moldura do retrato (atras) + retrato placeholder tintado pela feicao em runtime.
            CreatePanelImage(panelGo.transform, "PortraitFrame", new Vector2(0f, 84f), new Vector2(122f, 122f), new Color(0.30f, 0.33f, 0.40f));
            // Fundo preenche o frame inteiro (122) — sem borda dark visivel; o frame fica so de backing.
            var portraitBg = CreatePanelImage(panelGo.transform, "PortraitBackground", new Vector2(0f, 84f), new Vector2(122f, 122f), Color.white);
            portraitBg.enabled = false; // fundo por NPC e setado em runtime pelo HUD
            var portrait = CreatePanelImage(panelGo.transform, "Portrait", new Vector2(0f, 84f), new Vector2(108f, 108f), new Color(0.7f, 0.7f, 0.72f));

            var expression = CreateText(panelGo.transform, "Expression", new Vector2(0f, 12f), new Vector2(206f, 22f), "Normal");
            expression.alignment = TextAnchor.MiddleCenter;
            expression.fontStyle = FontStyle.Italic;

            var nameText = CreateText(panelGo.transform, "Name", new Vector2(0f, -18f), new Vector2(212f, 42f), "—");
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.fontSize = 19;
            nameText.fontStyle = FontStyle.Bold;

            // Separador sob o nome — organiza a diagramacao (cabeca: retrato+nome / corpo: raca/papel/afinidade).
            CreatePanelImage(panelGo.transform, "Separator", new Vector2(0f, -46f), new Vector2(160f, 2f), new Color(0.45f, 0.47f, 0.52f, 0.8f));

            var raceText = CreateText(panelGo.transform, "Race", new Vector2(0f, -64f), new Vector2(204f, 20f), "—");
            raceText.alignment = TextAnchor.MiddleCenter;
            raceText.fontSize = 14;

            var roleText = CreateText(panelGo.transform, "Role", new Vector2(0f, -86f), new Vector2(204f, 20f), "—");
            roleText.alignment = TextAnchor.MiddleCenter;
            roleText.fontSize = 14;

            var affinityLabel = CreateText(panelGo.transform, "AffinityLabel", new Vector2(0f, -118f), new Vector2(204f, 18f), "Afinidade");
            affinityLabel.alignment = TextAnchor.MiddleCenter;
            affinityLabel.fontSize = 13;

            // Barra: fundo + preenchimento (Image.type = Filled Horizontal).
            CreatePanelImage(panelGo.transform, "AffinityBarBg", new Vector2(0f, -140f), new Vector2(198f, 16f), new Color(0.05f, 0.05f, 0.07f, 1f));
            var fill = CreatePanelImage(panelGo.transform, "AffinityBarFill", new Vector2(0f, -140f), new Vector2(198f, 16f), new Color(0.95f, 0.85f, 0.25f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0.5f; // neutro (opinião 0)

            var hud = panelGo.AddComponent<CindarsHope.UI.Npc.NpcInteractionPortraitHud>();
            var serialized = new SerializedObject(hud);
            SetReference(serialized, "_panel", group);
            SetReference(serialized, "_portrait", portrait);
            SetReference(serialized, "_portraitBackground", portraitBg);
            SetReference(serialized, "_nameText", nameText);
            SetReference(serialized, "_raceText", raceText);
            SetReference(serialized, "_roleText", roleText);
            SetReference(serialized, "_expressionCaption", expression);
            SetReference(serialized, "_affinityFill", fill);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hud);
        }

        private static Image CreatePanelImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.sprite = GetBuiltinSprite();
            image.color = color;
            return image;
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
            new("npc_tovin", "NPC_Tovin_Registry", "Assets/_Game/Data/NPCs/Npc_Tovin.asset", "Assets/_Game/Data/Economy/Shop_Tovin.asset", new Vector3(-6.5f, 7f, 0f), new Color(0.58f, 0.64f, 0.72f), "Stationary/PermitDesk", false, 2f, EntityScaleCategory.NpcSmallfolk),
            new("npc_sylveth", "NPC_Sylveth_SeedVendor", "Assets/_Game/Data/NPCs/Npc_Sylveth.asset", "Assets/_Game/Data/Economy/Shop_Sylveth.asset", new Vector3(-3.5f, 7f, 0f), new Color(0.42f, 0.72f, 0.34f), "ShopKeeperFixed/FarmVisit", false, 2f),
            new("npc_renko", "NPC_Renko_GeneralMerchant", "Assets/_Game/Data/NPCs/Npc_Renko.asset", "Assets/_Game/Data/Economy/Shop_Renko.asset", new Vector3(0f, 7f, 0f), new Color(0.86f, 0.72f, 0.34f), "ShopKeeperFixed", false, 2f, EntityScaleCategory.NpcSmallfolk),
            new("npc_mirela", "NPC_Mirela_Tailor", "Assets/_Game/Data/NPCs/Npc_Mirela.asset", "Assets/_Game/Data/Economy/Shop_Mirela.asset", new Vector3(3.5f, 7f, 0f), new Color(0.82f, 0.48f, 0.62f), "ShopKeeperFixed", false, 2f),
            new("npc_orlan", "NPC_Orlan_Inn", "Assets/_Game/Data/NPCs/Npc_Orlan.asset", "Assets/_Game/Data/Economy/Shop_Orlan.asset", new Vector3(6.5f, 7f, 0f), new Color(0.66f, 0.56f, 0.42f), "ShopKeeperFixed", false, 2f),
            new("npc_gruta", "NPC_Gruta_Tavern", "Assets/_Game/Data/NPCs/Npc_Gruta.asset", "Assets/_Game/Data/Economy/Shop_Gruta.asset", new Vector3(9.5f, 6.5f, 0f), new Color(0.75f, 0.42f, 0.28f), "ShopKeeperFixed/TavernStage", false, 2.5f, EntityScaleCategory.NpcOrc),
            // Industry / blacksmith / quarry (W)
            new("npc_brumdar", "NPC_Brumdar_Blacksmith", "Assets/_Game/Data/NPCs/Npc_Brumdar.asset", "Assets/_Game/Data/Economy/Shop_Brumdar.asset", new Vector3(-12f, 2.5f, 0f), new Color(0.64f, 0.45f, 0.3f), "ShopKeeperFixed", false, 2f, EntityScaleCategory.NpcDwarf),
            new("npc_dagna", "NPC_Dagna_Quarry", "Assets/_Game/Data/NPCs/Npc_Dagna.asset", "Assets/_Game/Data/Economy/Shop_Dagna.asset", new Vector3(-12.5f, -2.5f, 0f), new Color(0.54f, 0.46f, 0.4f), "Patrol/QuarryRoad", true, 3f, EntityScaleCategory.NpcDwarf),
            new("npc_hund", "NPC_Hund_GuardRoute", "Assets/_Game/Data/NPCs/Npc_Hund.asset", "Assets/_Game/Data/Economy/Shop_Hund.asset", new Vector3(-8f, 0f, 0f), new Color(0.38f, 0.48f, 0.58f), "Patrol/TownRoad", true, 5f, EntityScaleCategory.NpcOrc),
            new("npc_thalindra", "NPC_Thalindra_Archive", "Assets/_Game/Data/NPCs/Npc_Thalindra.asset", "Assets/_Game/Data/Economy/Shop_Thalindra.asset", new Vector3(-9f, -5f, 0f), new Color(0.5f, 0.42f, 0.77f), "Stationary/ArchiveDesk", false, 2f),
            // South gate
            new("npc_alaric", "NPC_Alaric_GuardPost", "Assets/_Game/Data/NPCs/Npc_Alaric.asset", string.Empty, new Vector3(-5f, -10f, 0f), new Color(0.36f, 0.46f, 0.72f), "Patrol/TownGate", true, 3.5f),
            new("npc_pip", "NPC_Pip_TownEntrance", "Assets/_Game/Data/NPCs/Npc_Pip_Miudinho.asset", "Assets/_Game/Data/Economy/Shop_Pip.asset", new Vector3(-2.5f, -9f, 0f), new Color(0.38f, 0.72f, 0.86f), "WanderWithinZone", true, 3f, EntityScaleCategory.NpcSmallfolk),
            // Workshop / construction (SE)
            new("npc_nimble", "NPC_Nimble_Workshop", "Assets/_Game/Data/NPCs/Npc_Nimble.asset", "Assets/_Game/Data/Economy/Shop_Nimble.asset", new Vector3(7f, -5f, 0f), new Color(0.72f, 0.58f, 0.32f), "Patrol/WorkshopDesk", true, 2.5f, EntityScaleCategory.NpcSmallfolk),
            new("npc_gurd", "NPC_Gurd_ConstructionYard", "Assets/_Game/Data/NPCs/Npc_Gurd.asset", "Assets/_Game/Data/Economy/Shop_Gurd.asset", new Vector3(4f, -6.5f, 0f), new Color(0.62f, 0.36f, 0.32f), "Patrol/HeavyWorkZone", true, 3f, EntityScaleCategory.NpcOrc),
            // Night market (S)
            new("npc_yael", "NPC_Yael_NightMarket", "Assets/_Game/Data/NPCs/Npc_Yael.asset", "Assets/_Game/Data/Economy/Shop_Yael.asset", new Vector3(10f, -9f, 0f), new Color(0.28f, 0.24f, 0.62f), "NightOnly/WanderHidden", true, 3f),
            new("npc_maelor", "NPC_Maelor_NightRoute", "Assets/_Game/Data/NPCs/Npc_Maelor.asset", string.Empty, new Vector3(0f, -11.5f, 0f), new Color(0.22f, 0.24f, 0.32f), "NightOnly/WanderHidden", true, 4f),
            // Cave road / forest gate / alchemy (E)
            new("npc_zrix", "NPC_Zrix_CaveRoad", "Assets/_Game/Data/NPCs/Npc_Zrix.asset", "Assets/_Game/Data/Economy/Shop_Zrix.asset", new Vector3(12f, -2f, 0f), new Color(0.43f, 0.52f, 0.68f), "Patrol/CaveRoad", true, 3.5f, EntityScaleCategory.NpcDragonborn),
            new("npc_savra", "NPC_Savra_ForestGate", "Assets/_Game/Data/NPCs/Npc_Savra.asset", "Assets/_Game/Data/Economy/Shop_Savra.asset", new Vector3(13.5f, 4.5f, 0f), new Color(0.34f, 0.62f, 0.38f), "Patrol/HerbRoute", true, 3f, EntityScaleCategory.NpcDragonborn),
            new("npc_ozzra", "NPC_Ozzra_AlchemyLab", "Assets/_Game/Data/NPCs/Npc_Ozzra.asset", "Assets/_Game/Data/Economy/Shop_Ozzra.asset", new Vector3(11f, 2.5f, 0f), new Color(0.32f, 0.7f, 0.75f), "WanderWithinZone/Lab", true, 2.5f, EntityScaleCategory.NpcSmallfolk),
            // Animal yard (NE)
            new("npc_eiran", "NPC_Eiran_AnimalYard", "Assets/_Game/Data/NPCs/Npc_Eiran.asset", "Assets/_Game/Data/Economy/Shop_Eiran.asset", new Vector3(12f, 8.5f, 0f), new Color(0.44f, 0.68f, 0.42f), "WanderWithinZone/AnimalArea", true, 3f),
            // Statue garden (center)
            new("npc_liora", "NPC_Liora_StatueGarden", "Assets/_Game/Data/NPCs/Npc_Liora.asset", string.Empty, new Vector3(2.5f, -1.5f, 0f), new Color(0.68f, 0.62f, 0.9f), "WanderWithinZone/EveningStage", true, 3f),
            // Líder da aldeia — Ancião Velorin (Ninrorin idoso). Sem loja; fica na frente da Câmara
            // (casa das decisões) de dia; dorme na Mansão. O asset Npc_Velorin é criado por
            // EnsureChiefNpcAsset() no início de CreateScene (não há gerador automático de NpcDataSO).
            new("npc_velorin", "NPC_Velorin_Chamber", "Assets/_Game/Data/NPCs/Npc_Velorin.asset", string.Empty, new Vector3(6.5f, 11.5f, 0f), new Color(0.82f, 0.80f, 0.86f), "Stationary/ChamberDesk", false, 2f),
            // Autossuficiência da vila (slice village_economy) — assets criados por EnsureVillageEconomyNpcAssets().
            // Sael no cais (SO, perto do lago); Mella na praça/mercado (N); Hess na borda leste; Tibbet no cemitério (NO).
            new("npc_sael", "NPC_Sael_Dock", "Assets/_Game/Data/NPCs/Npc_Sael.asset", "Assets/_Game/Data/Economy/Shop_Sael.asset", new Vector3(-23f, -19f, 0f), new Color(0.36f, 0.50f, 0.62f), "ShopKeeperFixed/Dock", false, 2f),
            new("npc_mella", "NPC_Mella_Bakery", "Assets/_Game/Data/NPCs/Npc_Mella.asset", "Assets/_Game/Data/Economy/Shop_Mella.asset", new Vector3(0f, 17f, 0f), new Color(0.86f, 0.70f, 0.50f), "ShopKeeperFixed/Bakery", false, 2f),
            new("npc_hess", "NPC_Hess_Tannery", "Assets/_Game/Data/NPCs/Npc_Hess.asset", "Assets/_Game/Data/Economy/Shop_Hess.asset", new Vector3(24f, 2f, 0f), new Color(0.55f, 0.45f, 0.32f), "ShopKeeperFixed/Tannery", false, 2f, EntityScaleCategory.NpcDragonborn),
            new("npc_tibbet", "NPC_Tibbet_Cemetery", "Assets/_Game/Data/NPCs/Npc_Tibbet.asset", string.Empty, new Vector3(-23f, 18f, 0f), new Color(0.80f, 0.78f, 0.72f), "Stationary/Cemetery", false, 2f, EntityScaleCategory.NpcSmallfolk),
        };

        // Cria (ou atualiza) o NpcDataSO do líder da aldeia. Não há gerador automático de NpcDataSO no
        // projeto, então o scene-gen garante este asset (idempotente: load-or-create por caminho).
        // Ancião Velorin — Ninrorin (elfo cinzento) idoso, líder; sem loja; saúda via Opening/Closing
        // line (uma árvore de diálogo completa pode ser adicionada depois via TownNpcDialogueLibrary).
        private static void EnsureChiefNpcAsset()
        {
            const string path = "Assets/_Game/Data/NPCs/Npc_Velorin.asset";
            var npc = AssetDatabase.LoadAssetAtPath<NpcDataSO>(path);
            bool created = false;
            if (npc == null)
            {
                npc = ScriptableObject.CreateInstance<NpcDataSO>();
                AssetDatabase.CreateAsset(npc, path);
                created = true;
            }

            npc.NpcId = "npc_velorin";
            npc.DisplayName = "Anciao Velorin";
            npc.OpeningLine = "Bem-vindo a Cindar's Hope. Sou Velorin; ja vi esta aldeia nascer, cair e se reerguer. Os anos ensinam paciencia.";
            npc.ClosingLine = "Que a ordem de Kanthor guarde os seus passos, jovem. A aldeia conta com cada um de nos.";
            npc.DefaultSceneId = "TownScene";
            npc.MovementMode = NpcMovementMode.Static;
            EditorUtility.SetDirty(npc);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CreateMvpTownScene] Npc_Velorin (lider da aldeia) {(created ? "criado" : "atualizado")} em {path}.");
        }

        // 4 NPCs novos (Sael/Mella/Hess/Tibbet) + 3 lojas (sem gerador automático; idempotente como o chefe).
        private static void EnsureVillageEconomyNpcAssets()
        {
            EnsureTownNpcAsset("Assets/_Game/Data/NPCs/Npc_Sael.asset", "npc_sael", "Sael Mare-Quieta",
                "Bem-vindo ao cais. A pesca e arte de quem tem paciencia. Vendo peixe, isca e o silencio que o anzol pede.",
                "Va com a mare calma. A agua estara aqui; eu tambem.", "shop_sael");
            EnsureTownNpcAsset("Assets/_Game/Data/NPCs/Npc_Mella.asset", "npc_mella", "Mella Forno-Quente",
                "Entra, entra! Acabou de sair do forno. Em Cindar's Hope ninguem sai de maos vazias se eu puder evitar.",
                "Nao esquece de comer. Volta amanha, tem fornada nova ao amanhecer.", "shop_mella");
            EnsureTownNpcAsset("Assets/_Game/Data/NPCs/Npc_Hess.asset", "npc_hess", "Hess Couro-Fundo",
                "Devagar. O couro nao ensina pressa nenhuma. Curto peles; vendo couro e armadura leve.",
                "Va devagar. O que tiver de durar, durara. Volte quando precisar.", "shop_hess");
            EnsureTownNpcAsset("Assets/_Game/Data/NPCs/Npc_Tibbet.asset", "npc_tibbet", "Tibbet Vela-Torta",
                "Oh, ola. Desculpe a terra nas maos. Sou Tibbet, auxiliar do Padre Corvus: cuido do altar de dia e das covas de noite.",
                "Va com a luz e com a falta dela. Que a noite seja mansa com voce.", null);

            EnsureShopAsset("Assets/_Game/Data/Economy/Shop_Sael.asset", "shop_sael", "Pescaria do Sael", "npc_sael",
                new[] { ("item_fish_river_perch", 8, 0), ("item_fish_sun_bass", 6, 0), ("item_fish_amber_trout", 5, 0), ("item_consumable_food_grilled_fish", 6, 0) });
            EnsureShopAsset("Assets/_Game/Data/Economy/Shop_Mella.asset", "shop_mella", "Padaria da Mella", "npc_mella",
                new[] { ("item_consumable_food_bread", 12, 0), ("item_consumable_food_thandra_loaf", 6, 0), ("item_consumable_food_festival_cake", 2, 0), ("item_consumable_food_pumpkin_soup", 5, 0) });
            EnsureShopAsset("Assets/_Game/Data/Economy/Shop_Hess.asset", "shop_hess", "Tanoaria do Hess", "npc_hess",
                new[] { ("item_material_leather", 10, 0), ("item_material_hide", 8, 0), ("item_armor_light_leather", 2, 0) });
        }

        private static void EnsureTownNpcAsset(string path, string id, string name, string opening, string closing, string shopId)
        {
            var npc = AssetDatabase.LoadAssetAtPath<NpcDataSO>(path);
            bool created = false;
            if (npc == null)
            {
                npc = ScriptableObject.CreateInstance<NpcDataSO>();
                AssetDatabase.CreateAsset(npc, path);
                created = true;
            }

            npc.NpcId = id;
            npc.DisplayName = name;
            npc.OpeningLine = opening;
            npc.ClosingLine = closing;
            npc.DefaultSceneId = "TownScene";
            npc.MovementMode = NpcMovementMode.Static;
            if (!string.IsNullOrEmpty(shopId))
            {
                npc.ShopId = shopId;
            }

            EditorUtility.SetDirty(npc);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CreateMvpTownScene] {path} {(created ? "criado" : "atualizado")}.");
        }

        private static void EnsureShopAsset(string path, string id, string displayName, string npcId, (string itemId, int stock, int priceOverride)[] items)
        {
            var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
            bool created = false;
            if (shop == null)
            {
                shop = ScriptableObject.CreateInstance<ShopDataSO>();
                AssetDatabase.CreateAsset(shop, path);
                created = true;
            }

            shop.Id = id;
            shop.DisplayName = displayName;
            shop.NpcId = npcId;
            var entries = new ShopItemEntry[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                entries[i] = new ShopItemEntry
                {
                    ItemId = items[i].itemId,
                    BaseDailyStock = items[i].stock,
                    IsFiniteStock = true,
                    BuyPriceOverride = items[i].priceOverride
                };
            }

            shop.Items = entries;
            shop.DailyRestock = true;
            EditorUtility.SetDirty(shop);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CreateMvpTownScene] {path} {(created ? "criado" : "atualizado")} ({items.Length} itens).");
        }

        // fable_10 — explicit offered-quest mapping for the Act 1 main-quest givers. The
        // QuestGiverInteractable picks the first offerable/turn-in-able quest whose prerequisites
        // are complete, so listing the whole chain on the giver lets one NPC carry several steps.
        private static readonly Dictionary<string, string[]> MainQuestGiverOfferedIds =
            new Dictionary<string, string[]>
            {
                // Corvus opens Act 1 (mq_act1_01) and receives the final delivery (mq_act1_05).
                ["npc_corvus"] = new[] { "mq_act1_01_fonte_adormecida", "mq_act1_05_fragmento_da_agua" },
                // Thalindra: keep the existing supply quest, then the records step (mq_act1_02).
                ["npc_thalindra"] = new[] { "quest_first_supplies_for_cindar", "mq_act1_02_registros_perdidos" },
                // Maelor: the night clue (mq_act1_03) and the guardian step (mq_act1_04), plus the
                // pre-existing cave echo side quest.
                ["npc_maelor"] = new[] { "mq_act1_03_eco_da_agua", "mq_act1_04_guardiao_da_agua", "quest_echo_from_the_cave" },
            };

        private static void AddMainQuestGiver(GameObject npcObject, string npcId)
        {
            if (npcObject == null || string.IsNullOrEmpty(npcId)) return;
            if (!MainQuestGiverOfferedIds.TryGetValue(npcId, out var offeredIds)) return;

            var giver = npcObject.GetComponent<CindarsHope.Quests.Runtime.QuestGiverInteractable>();
            if (giver == null)
            {
                giver = npcObject.AddComponent<CindarsHope.Quests.Runtime.QuestGiverInteractable>();
            }

            var serialized = new SerializedObject(giver);
            serialized.FindProperty("_npcId").stringValue = npcId;
            var arrayProp = serialized.FindProperty("_offeredQuestIds");
            arrayProp.arraySize = offeredIds.Length;
            for (var i = 0; i < offeredIds.Length; i++)
            {
                arrayProp.GetArrayElementAtIndex(i).stringValue = offeredIds[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private readonly struct TownNpcSpec
        {
            public TownNpcSpec(string npcId, string objectName, string npcDataPath, string shopDataPath, Vector3 position, Color color, string movementProfile, bool canWander, float wanderRadius, EntityScaleCategory scaleCategory = EntityScaleCategory.NPC)
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
                ScaleCategory = scaleCategory;
            }

            public string NpcId { get; }
            public string ObjectName { get; }
            public string NpcDataPath { get; }
            public string ShopDataPath { get; }

            /// <summary>Legacy authoring position (36×30 grid). Use <see cref="LayoutPosition"/>
            /// for placement so unlisted elements remain inside the canonical 76×64 footprint.</summary>
            public Vector3 Position { get; }

            /// <summary>Role-specific work position from the preservation-first layout. Falls back
            /// to the legacy transform for NPCs not yet present in the canonical placement table.</summary>
            public Vector3 LayoutPosition => TownCityLayout.ResolveNpcWorkPosition(NpcId, TownDistrictLayout.Reposition(Position));

            public Color Color { get; }
            public string MovementProfile { get; }
            public bool CanWander { get; }
            public float WanderRadius { get; }

            /// <summary>Per-race visual scale category. Defaults to <see cref="EntityScaleCategory.NPC"/>
            /// (1.0× player). Override for dwarves (0.8×), halfling/goblin (0.7×), orc (1.2×).</summary>
            public EntityScaleCategory ScaleCategory { get; }
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

            // Collider sólido do jogador (BoxCollider2D no root; o trigger de interação fica num filho).
            // Cada NPC ignora a colisão com este collider → player atravessa NPCs, NPCs batem nas paredes.
            var playerCollider = playerTransform != null ? playerTransform.GetComponent<Collider2D>() : null;

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
                        spec.WanderRadius,
                        playerCollider,
                        playerManager,
                        inventoryManager,
                        itemDatabase,
                        shopManager,
                        modalManager,
                        shopUi,
                        spec.ScaleCategory);

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
                        spec.WanderRadius,
                        playerCollider,
                        spec.ScaleCategory);

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

                // fable_10 — wire QuestGiverInteractable on the Act 1 main-quest givers
                // (closes SCENE_WIRING_DEBT for npc_corvus / npc_maelor). Thalindra already
                // self-wires via its default offered quest ids; here we pin the explicit
                // offered ids so the prerequisite-gated giver flow resolves the whole chain.
                AddMainQuestGiver(npcObject, spec.NpcId);

                // spec_npc_physics_cat_companion — gato companheiro do Eiran.
                // Spawna como entidade separada, segue o Eiran via CompanionFollow.
                if (spec.NpcId == "npc_eiran")
                {
                    SpawnCatCompanion(parent.transform, npcObject.transform, spec.LayoutPosition);
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
                "Roam",
                8f,
                playerCollider,
                EntityScaleCategory.NPC);
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
            float wanderRadius,
            Collider2D playerCollider,
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            ItemDatabaseSO itemDatabase,
            ShopManager shopManager,
            ModalManager modalManager,
            ShopUiReferences shopUi,
            EntityScaleCategory scaleCategory = EntityScaleCategory.NPC)
        {
            var npcObject = new GameObject(objectName);
            npcObject.transform.SetParent(parent);
            npcObject.transform.position = position;
            // Size from the per-race NPC scale profile; falls back to a hardcoded scale if the profile
            // asset has not been generated yet (run CindarsHope/Inicializar Projeto to materialise it).
            if (!ScaleProfileLibrary.AttachApplicator(npcObject, scaleCategory))
            {
                npcObject.transform.localScale = new Vector3(1f, 1.5f, 1f);
            }
            var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(npcDataPath);
            var renderer = npcObject.AddComponent<SpriteRenderer>();
            var hasBodySprite = npcData != null && npcData.BodySprite != null;
            renderer.sprite = hasBodySprite ? npcData.BodySprite : GetBuiltinSprite();
            // Real body sprites render untinted (white); the per-NPC color only tints the placeholder square,
            // otherwise it would multiply over the artwork and wash the sprite with a color cast.
            renderer.color = hasBodySprite ? Color.white : color;
            renderer.sortingOrder = 2;
            TrySetSortingLayer(renderer, "Characters", renderer.sortingOrder);
            var collider = npcObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var controller = npcObject.AddComponent<NpcShopController>();
            var serialized = new SerializedObject(controller);
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
            ConfigureNpcMovement(npcObject, npcData, position, movementProfile, canWander: true, wanderRadius, playerCollider);
            AddPlacementMarker(npcObject, npcData, movementProfile);
            return npcObject;
        }

        // Dá ao NPC um corpo VIVO e SÓLIDO: (1) Rigidbody2D dinâmico + collider sólido pequeno nos pés
        // ⇒ bate em paredes/portas; (2) massa alta ⇒ o player ESBARRA no NPC e é bloqueado (resistência,
        // pedido humano 2026-06-30) sem conseguir empurrá-lo — NpcPhysicsBody removido daqui; (3)
        // NpcDweller ⇒ a porta da casa abre sozinha quando o morador chega; (4) NpcWanderer com tier de
        // movimento (lojista faz micro-vaivém no posto; andarilho cobre mais a cidade). O trigger 1x1 de
        // interação (já criado pelo caller) continua sendo o que o player usa para conversar.
        private static void ConfigureNpcMovement(
            GameObject npcObject, NpcDataSO npcData, Vector3 position, string movementProfile,
            bool canWander, float wanderRadius, Collider2D playerCollider)
        {
            var body = npcObject.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = npcObject.AddComponent<Rigidbody2D>();
            }
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.linearDamping = 4f;
            // Massa alta: o player ESBARRA no NPC (resistencia, pedido humano 2026-06-30) e nao consegue
            // empurra-lo de forma perceptivel; o NpcWanderer + damping mantem o NPC no lugar.
            body.mass = 50f;

            // Collider sólido pequeno nos "pés" (filho ⇒ offset/escala independentes do trigger 1x1).
            // CONTRA-ESCALA (fix 2026-06-30): o root do NPC já foi escalado pela VisualScale (AttachApplicator
            // roda ANTES disto), então um filho herdaria essa escala e o box de colisão ficaria 2–2.6× maior
            // que o sprite, bloqueando o player longe do NPC. Dividimos escala E offset do SolidBody pela
            // escala do root ⇒ collider com tamanho de MUNDO fixo (0.55×0.4) e offset fixo, por raça.
            float visualScale = npcObject.transform.localScale.y;
            if (visualScale <= 0.01f) visualScale = 1f;
            var bodyObject = new GameObject("SolidBody");
            bodyObject.transform.SetParent(npcObject.transform);
            // Centro VERTICAL do sprite (fix 2026-06-30): o pivot e BottomCenter (origem nos pes), entao um
            // offset negativo jogava a caixa pra BAIXO do sprite. Subimos meia-altura do personagem
            // (128px @ PPU 234 = 0.547u em escala 1 ⇒ meia = 0.273). localPosition escala pelo parent
            // (visualScale), entao o collider fica centrado no sprite de qualquer raca.
            bodyObject.transform.localPosition = new Vector3(0f, (128f / 234f) * 0.5f, 0f);
            bodyObject.transform.localScale = new Vector3(1f / visualScale, 1f / visualScale, 1f);
            var solid = bodyObject.AddComponent<BoxCollider2D>();
            solid.isTrigger = false;
            solid.size = new Vector2(0.55f, 0.4f);

            // Resistencia total (pedido humano 2026-06-30): o NPC NAO ignora mais a colisao com o player
            // (NpcPhysicsBody removido daqui) — o player esbarra e e bloqueado pelo NPC. A massa alta acima
            // impede que o player empurre o NPC. playerCollider mantido na assinatura para uso futuro.
            _ = playerCollider;

            var dweller = npcObject.AddComponent<NpcDweller>();
            dweller.Configure(npcData != null ? npcData.NpcId : string.Empty);

            var wanderer = npcObject.AddComponent<NpcWanderer>();
            var serializedWanderer = new SerializedObject(wanderer);
            SetReference(serializedWanderer, "_npcData", npcData);
            SetReference(serializedWanderer, "_rigidbody", body);
            serializedWanderer.ApplyModifiedPropertiesWithoutUndo();

            var tier = MovementTierFor(movementProfile, canWander, wanderRadius);
            float clampX = TownDistrictLayout.HalfWidth - 1f;   // 37
            float clampY = TownDistrictLayout.HalfHeight - 1f;  // 31
            wanderer.ConfigureMovement(tier.speed, tier.radius, tier.pauseMin, tier.pauseMax,
                new Vector2(-clampX, -clampY), new Vector2(clampX, clampY));
            EditorUtility.SetDirty(wanderer);
        }

        // spec_npc_physics_cat_companion — cria o gato do Eiran como entidade separada.
        // O gato tem: SpriteRenderer (sprite do gato se disponivel em Resources, senao placeholder),
        // Rigidbody2D Kinematic (nao bloqueia fisica — so visual/companheiro), CircleCollider2D
        // pequeno como trigger (nao bloqueia player), e CompanionFollow wired no Transform do Eiran.
        // Sem interacao (o gato e cosmético); sem NpcPhysicsBody (Kinematic ja nao colide solidamente).
        private static void SpawnCatCompanion(Transform parent, Transform eiranTransform, Vector3 eiranPosition)
        {
            const string CatObjectName = "Cat_Eiran_Companion";
            const string CatSpritePath = "CatCompanion/gpt_cat";
            const float CatVisualScale = 0.7f;   // gato e menor que NPC humano
            const float CatColliderRadius = 0.2f; // trigger pequeno — nao bloqueia player

            var cat = new GameObject(CatObjectName);
            cat.transform.SetParent(parent);
            // Spawna ligeiramente ao lado do Eiran para nao sobrepor sprites na cena.
            cat.transform.position = eiranPosition + new Vector3(0.5f, -0.3f, 0f);
            cat.transform.localScale = new Vector3(CatVisualScale, CatVisualScale, 1f);

            var renderer = cat.AddComponent<SpriteRenderer>();
            // Garante que o PNG do gato esteja importado COMO SPRITE antes de carregar (um PNG recem-copiado
            // importa como Texture default e Resources.Load<Sprite> retornaria null). Self-heal idempotente.
            EnsureSpriteImport("Assets/_Game/Resources/CatCompanion/gpt_cat.png");
            var catSprite = Resources.Load<Sprite>(CatSpritePath);
            renderer.sprite = catSprite != null ? catSprite : GetBuiltinSprite();
            if (catSprite == null)
            {
                renderer.color = new Color(0.55f, 0.42f, 0.28f); // laranja-caramelo como placeholder
                Debug.LogWarning(
                    $"[CreateMvpTownScene] Sprite do gato nao encontrado em Resources/{CatSpritePath}. " +
                    "Copie _art_archive/art/animations/_npc_tests/batch_v3/_cat_companion/gpt_cat.png " +
                    "para Assets/_Game/Resources/CatCompanion/gpt_cat.png e reimporte.");
            }
            else
            {
                renderer.color = Color.white;
            }
            renderer.sortingOrder = 1;
            TrySetSortingLayer(renderer, "Characters", renderer.sortingOrder);

            // Rigidbody2D Kinematic: sem gravidade, sem colisao solida — o gato e puro companheiro visual.
            var rb = cat.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Trigger minusculo — nao bloqueia o player; presente para coerencia de layer.
            var trigger = cat.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = CatColliderRadius;

            // CompanionFollow: segue o Eiran com leve atraso, para quando perto.
            var follow = cat.AddComponent<CompanionFollow>();
            follow.SetTarget(eiranTransform);
            EditorUtility.SetDirty(cat);

            Debug.Log($"[CreateMvpTownScene] Gato companheiro '{CatObjectName}' criado junto do Eiran " +
                      $"(sprite {(catSprite != null ? "OK" : "PLACEHOLDER")}).");
        }

        // Configura o TextureImporter de um PNG para Sprite pixel-art (idempotente). Necessario para que
        // Resources.Load<Sprite> funcione em PNGs adicionados fora do fluxo de AssignNpcBodySprites (ex.: gato).
        private static void EnsureSpriteImport(string assetPath)
        {
            var full = System.IO.Path.Combine(Application.dataPath, "..", assetPath);
            if (!System.IO.File.Exists(full))
            {
                return;
            }
            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
            {
                return;
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 234f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        // Tier de movimento por perfil. Todos têm vaivém; o raio/velocidade muda quem fica no posto
        // (lojista/estático), quem ronda a vizinhança (patrulha/zona) e quem cobre a cidade (roam).
        private static (float speed, float radius, float pauseMin, float pauseMax) MovementTierFor(
            string movementProfile, bool canWander, float wanderRadius)
        {
            string p = movementProfile ?? string.Empty;
            if (p.StartsWith("Roam"))
                return (1.5f, Mathf.Max(wanderRadius, 7f), 1.5f, 3.5f);
            if (p.StartsWith("Patrol"))
                return (1.6f, Mathf.Max(wanderRadius, 3.5f), 1.5f, 3.5f);
            if (p.StartsWith("WanderWithinZone"))
                return (1.3f, Mathf.Max(wanderRadius, 3f), 2f, 4f);
            if (p.StartsWith("NightOnly"))
                return (1.4f, Mathf.Max(wanderRadius, 3f), 2f, 4f);
            // Stationary / ShopKeeperFixed / desconhecido sem wander: micro-vaivém no posto (~2 tiles).
            if (!canWander || p.StartsWith("Stationary") || p.StartsWith("ShopKeeperFixed"))
                return (0.8f, 1.2f, 3f, 6f);
            return (1.3f, Mathf.Max(wanderRadius, 2.5f), 2.5f, 5f);
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
            float wanderRadius,
            Collider2D playerCollider,
            EntityScaleCategory scaleCategory = EntityScaleCategory.NPC)
        {
            var npcObject = new GameObject(objectName);
            npcObject.transform.SetParent(parent);
            npcObject.transform.position = position;
            // Size from the per-race NPC scale profile; falls back to a hardcoded scale if the profile
            // asset has not been generated yet (run CindarsHope/Inicializar Projeto to materialise it).
            if (!ScaleProfileLibrary.AttachApplicator(npcObject, scaleCategory))
            {
                npcObject.transform.localScale = new Vector3(1f, 1.5f, 1f);
            }
            var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(npcDataPath);
            var renderer = npcObject.AddComponent<SpriteRenderer>();
            var hasBodySprite = npcData != null && npcData.BodySprite != null;
            renderer.sprite = hasBodySprite ? npcData.BodySprite : GetBuiltinSprite();
            // Real body sprites render untinted (white); the per-NPC color only tints the placeholder square,
            // otherwise it would multiply over the artwork and wash the sprite with a color cast.
            renderer.color = hasBodySprite ? Color.white : color;
            renderer.sortingOrder = 2;
            TrySetSortingLayer(renderer, "Characters", renderer.sortingOrder);
            var collider = npcObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var controller = npcObject.AddComponent<NpcController>();
            var serializedController = new SerializedObject(controller);
            SetReference(serializedController, "_npcData", npcData);
            SetReference(serializedController, "_dialogueModal", dialogueModal);
            SetReference(serializedController, "_modalManager", modalManager);
            SetReference(serializedController, "_collider", collider);
            SetReference(serializedController, "_spriteRenderer", renderer);

            // TODO NPC se move agora (vaivém vivo + corpo sólido). O tier (micro no posto vs. ronda)
            // sai do movementProfile; canWander só decide se ele é "estático-vivo" ou andarilho.
            ConfigureNpcMovement(npcObject, npcData, position, movementProfile, canWander, wanderRadius, playerCollider);
            var wandererRef = npcObject.GetComponent<NpcWanderer>();
            SetReference(serializedController, "_wanderer", wandererRef);

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

        // Re-ancora um painel ao centro do RODAPE da tela: a borda de baixo fica a uma margem fixa
        // do fundo, então nunca é cortada quando a resolução/aspecto muda. Os filhos do painel usam
        // anchoredPosition relativo ao rect do painel, então o layout interno é preservado.
        private static void AnchorToBottomCenter(GameObject panel, float bottomMargin)
        {
            var rect = panel.GetComponent<RectTransform>();
            var size = rect.sizeDelta;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, bottomMargin);
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
