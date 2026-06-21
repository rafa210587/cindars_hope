using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Generation;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Wandering merchant that appears on some cave levels (roguelike shop encounter).
    ///
    /// Stable-run contract (ADR-0005 / FASE9F): appearance, position, and stock are all
    /// derived from CaveWorldSeed + CaveRunSeed + CaveLevel + stable salt. Revisiting the
    /// same level in the same run always yields the same merchant (or absence of one).
    /// No GUIDs, no timestamps, no UnityEngine.Random.
    ///
    /// Trade flow reuses the event-driven economy path (ItemPurchaseRequestedEvent /
    /// SellAllRequestedEvent handled by EconomyManager), so no modal shop UI is required
    /// inside CaveScene.
    /// </summary>
    public static class CaveWanderingMerchant
    {
        public const int AppearanceChancePercent = 22;
        private const string SpawnSalt = "wandering_merchant";
        private const int MinDistanceFromEntrance = 6;
        private const int MinDistanceFromExit = 3;

        private static GameObject s_merchantRoot;

        /// <summary>Deterministic stock catalog. Offers rotate per level via the level seed.</summary>
        public static readonly MerchantOffer[] OfferCatalog =
        {
            new MerchantOffer("item_consumable_potion_hp_small", 2, 60, "Comprar 2 Pocoes de Vida (60 ouro)"),
            new MerchantOffer("item_consumable_food_bread", 3, 30, "Comprar 3 Paes (30 ouro)"),
            new MerchantOffer("item_consumable_repair_kit_basic", 1, 45, "Comprar Kit de Reparo (45 ouro)"),
            new MerchantOffer("item_consumable_food_carrot_stew", 2, 40, "Comprar 2 Ensopados (40 ouro)"),
            new MerchantOffer("item_material_wood", 5, 25, "Comprar 5 Madeiras (25 ouro)"),
            new MerchantOffer("item_seed_carrot", 4, 20, "Comprar 4 Sementes de Cenoura (20 ouro)")
        };

        public readonly struct MerchantOffer
        {
            public MerchantOffer(string itemId, int amount, int totalCost, string prompt)
            {
                ItemId = itemId;
                Amount = amount;
                TotalCost = totalCost;
                Prompt = prompt;
            }

            public string ItemId { get; }
            public int Amount { get; }
            public int TotalCost { get; }
            public string Prompt { get; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            GameEventBus.Unsubscribe<CaveRuntimeMaterializationCompleteEvent>(OnLevelMaterialized);
            GameEventBus.Subscribe<CaveRuntimeMaterializationCompleteEvent>(OnLevelMaterialized);
        }

        private static void OnLevelMaterialized(CaveRuntimeMaterializationCompleteEvent evt)
        {
            DespawnCurrent();

            var level = evt?.GeneratedLevel;
            if (level == null)
            {
                return;
            }

            var runManager = CaveRunManager.Instance;
            string worldSeed = runManager != null ? runManager.CaveWorldSeed : string.Empty;
            string runSeed = runManager != null ? runManager.CaveRunSeed : string.Empty;

            if (!ShouldAppear(worldSeed, runSeed, level.CaveLevel))
            {
                return;
            }

            var tile = ResolveSpawnTile(level, worldSeed, runSeed);
            if (!tile.HasValue)
            {
                Debug.LogWarning($"[CaveWanderingMerchant] No safe tile on level {level.CaveLevel}; merchant skipped.");
                return;
            }

            SpawnMerchant(level, tile.Value, worldSeed, runSeed);
        }

        /// <summary>Pure decision function — also exercised by EditMode tests.</summary>
        public static bool ShouldAppear(string worldSeed, string runSeed, int caveLevel)
        {
            var hash = CaveEnemySpawnPlanner.StableHash($"{worldSeed}|{runSeed}|{caveLevel}|{SpawnSalt}");
            return Math.Abs(hash) % 100 < AppearanceChancePercent;
        }

        // fable_34 — the wandering merchant is one of the two consumers of the secret-quest API
        // (the other is a peaceful-monster interactable, F33). With a deterministic 15% chance it
        // also offers a cave-secret quest. The concrete scq_* content is authored by fable_52.
        private const string SecretQuestSalt = "cave_secret_offer";

        /// <summary>
        /// fable_34 (CA-3) — deterministic per-(seed, level) decision: does this merchant also
        /// offer a cave-secret quest? Uses the single rule in SecretQuestOffer (15% by seed).
        /// </summary>
        public static bool ShouldOfferSecretQuest(string worldSeed, string runSeed, int caveLevel)
        {
            var contextId = $"{worldSeed}|{runSeed}|{caveLevel}|{SecretQuestSalt}";
            return CindarsHope.Quests.SecretQuestOffer.ShouldMerchantOffer(runSeed, contextId);
        }

        /// <summary>Pure offer selection — deterministic pair of distinct catalog offers.</summary>
        public static (int firstIndex, int secondIndex) ResolveOfferIndices(string worldSeed, string runSeed, int caveLevel)
        {
            var hash = Math.Abs(CaveEnemySpawnPlanner.StableHash($"{worldSeed}|{runSeed}|{caveLevel}|{SpawnSalt}_stock"));
            int first = hash % OfferCatalog.Length;
            int second = (first + 1 + (hash / 7) % (OfferCatalog.Length - 1)) % OfferCatalog.Length;
            return (first, second);
        }

        private static Vector2Int? ResolveSpawnTile(CaveGeneratedLevel level, string worldSeed, string runSeed)
        {
            var seed = CaveEnemySpawnPlanner.StableHash($"{worldSeed}|{runSeed}|{level.CaveLevel}|{SpawnSalt}_tile");
            var candidates = level.WalkableTiles
                .Where(t => Manhattan(t, level.Entrance) >= MinDistanceFromEntrance
                            && Manhattan(t, level.Exit) >= MinDistanceFromExit
                            && !level.WallTiles.Contains(t))
                .OrderBy(t => CaveEnemySpawnPlanner.StableHash($"{seed}|{t.x}|{t.y}"))
                .ThenBy(t => t.x)
                .ThenBy(t => t.y);

            foreach (var tile in candidates)
            {
                return tile;
            }

            return null;
        }

        private static void SpawnMerchant(CaveGeneratedLevel level, Vector2Int tile, string worldSeed, string runSeed)
        {
            var worldPos = new Vector3(tile.x - level.Width * 0.5f, tile.y - level.Height * 0.5f, 0f);

            s_merchantRoot = new GameObject($"WanderingMerchant_L{level.CaveLevel}");
            s_merchantRoot.transform.position = worldPos;

            var renderer = s_merchantRoot.AddComponent<SpriteRenderer>();
            renderer.sprite = BuildMerchantSprite();
            renderer.color = new Color(0.85f, 0.7f, 0.3f);
            renderer.sortingOrder = 3;
            s_merchantRoot.transform.localScale = new Vector3(0.9f, 1.3f, 1f);

            var (firstIndex, secondIndex) = ResolveOfferIndices(worldSeed, runSeed, level.CaveLevel);
            CreateOfferPoint(s_merchantRoot.transform, OfferCatalog[firstIndex], new Vector3(-1f, 0f, 0f), level.CaveLevel);
            CreateOfferPoint(s_merchantRoot.transform, OfferCatalog[secondIndex], new Vector3(1f, 0f, 0f), level.CaveLevel);
            CreateSellPoint(s_merchantRoot.transform, level.CaveLevel);

            GameEventBus.Publish(new PlayerActionFeedbackEvent("Um mercador errante montou banca neste andar..."));
            Debug.Log($"[CaveWanderingMerchant] Spawned on level {level.CaveLevel} at grid ({tile.x},{tile.y}). Offers: {OfferCatalog[firstIndex].ItemId} + {OfferCatalog[secondIndex].ItemId}.");

            // fable_34/fable_52 — deterministic 15% chance to also surface a cave-secret quest through
            // the single SecretQuestOffer API. fable_52 authored the content: the merchant offers the 3
            // canonical merchant lists (scq_merchant_list_1/2/3) in a fixed sequence, each 1x per run,
            // stable per seed. The SecretQuestService owns the sequencing; until it is wired, fall back
            // to opening the channel for list 1 so the Secrets tab still surfaces.
            if (ShouldOfferSecretQuest(worldSeed, runSeed, level.CaveLevel))
            {
                var secretService = CindarsHope.Quests.Runtime.QuestRuntimeBootstrap.SecretQuestService;
                if (secretService != null)
                {
                    secretService.OfferNextMerchantList(runSeed, level.CaveLevel);
                }
                else
                {
                    CindarsHope.Quests.Runtime.QuestRuntimeBootstrap.QuestService?.OfferSecretQuest(
                        CindarsHope.Quests.SecretQuests.SecretQuestCatalog.MerchantList1Id);
                }
            }
        }

        private static void CreateOfferPoint(Transform parent, MerchantOffer offer, Vector3 localOffset, int caveLevel)
        {
            var pointObject = new GameObject($"MerchantOffer_{offer.ItemId}");
            pointObject.transform.SetParent(parent);
            pointObject.transform.localPosition = localOffset;
            pointObject.transform.localScale = new Vector3(0.7f, 0.55f, 1f);

            var renderer = pointObject.AddComponent<SpriteRenderer>();
            renderer.sprite = BuildMerchantSprite();
            renderer.color = new Color(0.5f, 0.36f, 0.2f);
            renderer.sortingOrder = 2;

            var collider = pointObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var buyPoint = pointObject.AddComponent<BuyItemPoint>();
            buyPoint.ConfigureOffer(
                $"shop_cave_wandering_merchant_l{caveLevel}",
                offer.ItemId,
                offer.Amount,
                offer.TotalCost,
                offer.Prompt);
        }

        private static void CreateSellPoint(Transform parent, int caveLevel)
        {
            var pointObject = new GameObject("MerchantSellPoint");
            pointObject.transform.SetParent(parent);
            pointObject.transform.localPosition = new Vector3(0f, -1.1f, 0f);
            pointObject.transform.localScale = new Vector3(0.7f, 0.55f, 1f);

            var renderer = pointObject.AddComponent<SpriteRenderer>();
            renderer.sprite = BuildMerchantSprite();
            renderer.color = new Color(0.32f, 0.45f, 0.3f);
            renderer.sortingOrder = 2;

            var collider = pointObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var sellPoint = pointObject.AddComponent<SellAllPoint>();
            sellPoint.ConfigureSource(
                $"shop_cave_wandering_merchant_l{caveLevel}",
                "Vender itens ao mercador errante");
        }

        private static void DespawnCurrent()
        {
            if (s_merchantRoot != null)
            {
                UnityEngine.Object.Destroy(s_merchantRoot);
                s_merchantRoot = null;
            }
        }

        private static Sprite BuildMerchantSprite()
        {
            // Small cached solid sprite so the merchant is visible in player builds
            // (builtin editor sprites are not available at runtime).
            if (s_solidSprite == null)
            {
                var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                texture.hideFlags = HideFlags.HideAndDontSave;
                var pixels = new Color[64];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                texture.SetPixels(pixels);
                texture.Apply();
                s_solidSprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
            }

            return s_solidSprite;
        }

        private static Sprite s_solidSprite;

        private static int Manhattan(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }
}
