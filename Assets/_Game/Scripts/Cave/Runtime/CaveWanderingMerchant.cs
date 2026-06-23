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
    /// fable_78 (SLICE 5 / criterio 14.7): the stock is now THEMED PER BIOME BAND and has
    /// wider variety. Each band (stone/fungal/ice/fire/ruins/deep/void — the same canonical
    /// switch as CaveBiomeLayoutProfile.ForLevel) exposes its own offer pool; the level still
    /// selects N distinct offers deterministically, but now from the pool of THAT band. The
    /// flat OfferCatalog surface and ResolveOfferIndices are preserved (back-compat) as the
    /// "always available" baseline goods carried on every floor.
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

        /// <summary>How many distinct offers the merchant stocks on a level.</summary>
        public const int OffersPerLevel = 3;

        private static GameObject s_merchantRoot;

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

        // ── Baseline (band-agnostic) goods carried on every floor ────────────────────────
        // Preserved as the historical flat surface (back-compat for OfferCatalog /
        // ResolveOfferIndices). Every level's themed pool starts from these and appends the
        // band-specific offers below. All ids are canonical (CanonicalItemCatalog).
        public static readonly MerchantOffer[] OfferCatalog =
        {
            new MerchantOffer("item_consumable_potion_hp_small", 2, 60, "Comprar 2 Pocoes de Vida (60 ouro)"),
            new MerchantOffer("item_consumable_food_bread", 3, 30, "Comprar 3 Paes (30 ouro)"),
            new MerchantOffer("item_consumable_repair_kit_basic", 1, 45, "Comprar Kit de Reparo (45 ouro)"),
            new MerchantOffer("item_consumable_food_miners_ration", 2, 90, "Comprar 2 Racoes de Minerador (90 ouro)"),
            new MerchantOffer("item_material_wood", 5, 25, "Comprar 5 Madeiras (25 ouro)"),
            new MerchantOffer("item_material_stone", 5, 25, "Comprar 5 Pedras (25 ouro)")
        };

        // ── Band-specific themed offers ──────────────────────────────────────────────────
        // One pool per canonical band. Each pool is the band flavour ON TOP of the baseline
        // goods (the spawn path concatenates baseline + band). All ids are canonical.
        private static readonly MerchantOffer[] StoneBandOffers =
        {
            new MerchantOffer("item_material_copper_ore", 3, 30, "Comprar 3 Minerios de Cobre (30 ouro)"),
            new MerchantOffer("item_material_iron_ore", 2, 36, "Comprar 2 Minerios de Ferro (36 ouro)"),
            new MerchantOffer("item_consumable_food_bread", 5, 45, "Comprar 5 Paes (45 ouro)")
        };

        private static readonly MerchantOffer[] FungalBandOffers =
        {
            new MerchantOffer("item_material_glowcap", 3, 48, "Comprar 3 Glowcaps (48 ouro)"),
            new MerchantOffer("item_material_spores", 4, 50, "Comprar 4 Esporos (50 ouro)"),
            new MerchantOffer("item_consumable_potion_mp_small", 2, 100, "Comprar 2 Pocoes de Mana (100 ouro)"),
            new MerchantOffer("item_essence_toxic", 1, 50, "Comprar Essencia Toxica (50 ouro)")
        };

        private static readonly MerchantOffer[] IceBandOffers =
        {
            new MerchantOffer("item_material_frost_core", 1, 50, "Comprar Nucleo Gelido (50 ouro)"),
            new MerchantOffer("item_consumable_potion_ice_resist", 2, 140, "Comprar 2 Pocoes de Resist. ao Gelo (140 ouro)"),
            new MerchantOffer("item_essence_ice", 1, 65, "Comprar Essencia de Gelo (65 ouro)")
        };

        private static readonly MerchantOffer[] FireBandOffers =
        {
            new MerchantOffer("item_material_ember_fang", 1, 50, "Comprar Presa de Brasa (50 ouro)"),
            new MerchantOffer("item_consumable_potion_fire_resist", 2, 140, "Comprar 2 Pocoes de Resist. ao Fogo (140 ouro)"),
            new MerchantOffer("item_essence_fire", 1, 65, "Comprar Essencia de Fogo (65 ouro)")
        };

        private static readonly MerchantOffer[] RuinsBandOffers =
        {
            new MerchantOffer("item_material_silver_ore", 2, 66, "Comprar 2 Minerios de Prata (66 ouro)"),
            new MerchantOffer("item_consumable_repair_kit_standard", 1, 90, "Comprar Kit de Reparo Padrao (90 ouro)"),
            new MerchantOffer("item_consumable_potion_hp_medium", 2, 180, "Comprar 2 Pocoes Maiores de Vida (180 ouro)")
        };

        private static readonly MerchantOffer[] DeepBandOffers =
        {
            new MerchantOffer("item_material_mithril_ore", 1, 88, "Comprar Minerio de Mithril (88 ouro)"),
            new MerchantOffer("item_material_arcane_crystal", 1, 66, "Comprar Cristal Arcano (66 ouro)"),
            new MerchantOffer("item_essence_arcane", 1, 82, "Comprar Essencia Arcana (82 ouro)"),
            new MerchantOffer("item_consumable_repair_kit_superior", 1, 180, "Comprar Kit de Reparo Superior (180 ouro)")
        };

        private static readonly MerchantOffer[] VoidBandOffers =
        {
            new MerchantOffer("item_essence_void", 1, 99, "Comprar Essencia do Vazio (99 ouro)"),
            new MerchantOffer("item_material_star_iron", 1, 132, "Comprar Ferro Estelar (132 ouro)"),
            new MerchantOffer("item_consumable_potion_mp_medium", 2, 190, "Comprar 2 Pocoes Maiores de Mana (190 ouro)")
        };

        // Canonical band id -> themed offers. Keys match CaveBiomeLayoutProfile.BandId.
        private static readonly IReadOnlyDictionary<string, MerchantOffer[]> BandOffers =
            new Dictionary<string, MerchantOffer[]>(StringComparer.Ordinal)
            {
                { "stone", StoneBandOffers },
                { "fungal", FungalBandOffers },
                { "ice", IceBandOffers },
                { "fire", FireBandOffers },
                { "ruins", RuinsBandOffers },
                { "deep", DeepBandOffers },
                { "void", VoidBandOffers }
            };

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

        /// <summary>The canonical biome band id for this level (stone/fungal/.../void).</summary>
        public static string ResolveBandId(int caveLevel)
        {
            return CaveBiomeLayoutProfile.ForLevel(caveLevel).BandId;
        }

        /// <summary>
        /// The full pool of offers available on a level: the band-agnostic baseline goods plus
        /// the themed offers for this level's biome band. Deterministic (function of band only).
        /// </summary>
        public static IReadOnlyList<MerchantOffer> ResolveLevelOfferPool(int caveLevel)
        {
            var pool = new List<MerchantOffer>(OfferCatalog);
            if (BandOffers.TryGetValue(ResolveBandId(caveLevel), out var bandOffers))
            {
                pool.AddRange(bandOffers);
            }

            return pool;
        }

        /// <summary>
        /// Pure stock selection — deterministic, distinct offers drawn from THIS level's biome
        /// pool (baseline + band). Same (seed, level) always yields the same stock; different
        /// bands yield pools that differ by their themed offers.
        /// </summary>
        public static IReadOnlyList<MerchantOffer> ResolveBiomeStock(string worldSeed, string runSeed, int caveLevel)
        {
            var pool = ResolveLevelOfferPool(caveLevel);
            var selectedIndices = SelectDistinctIndices(worldSeed, runSeed, caveLevel, pool.Count, OffersPerLevel);

            var stock = new List<MerchantOffer>(selectedIndices.Count);
            foreach (var index in selectedIndices)
            {
                stock.Add(pool[index]);
            }

            return stock;
        }

        /// <summary>
        /// Back-compat flat selection — two distinct indices into OfferCatalog (the baseline
        /// goods). Preserved for callers/tests that predate the biome pools. New stock logic
        /// uses ResolveBiomeStock.
        /// </summary>
        public static (int firstIndex, int secondIndex) ResolveOfferIndices(string worldSeed, string runSeed, int caveLevel)
        {
            var hash = Math.Abs(CaveEnemySpawnPlanner.StableHash($"{worldSeed}|{runSeed}|{caveLevel}|{SpawnSalt}_stock"));
            int first = hash % OfferCatalog.Length;
            int second = (first + 1 + (hash / 7) % (OfferCatalog.Length - 1)) % OfferCatalog.Length;
            return (first, second);
        }

        /// <summary>
        /// Deterministic selection of <paramref name="take"/> distinct indices in [0, count),
        /// seeded by (worldSeed, runSeed, caveLevel). No UnityEngine.Random; FNV-1a via StableHash.
        /// </summary>
        private static List<int> SelectDistinctIndices(string worldSeed, string runSeed, int caveLevel, int count, int take)
        {
            var selected = new List<int>(Math.Min(take, count));
            if (count <= 0)
            {
                return selected;
            }

            // Order all indices by a stable per-index hash, then take the first N. Deterministic
            // and guarantees distinct picks. ThenBy index keeps ties stable.
            var ordered = Enumerable.Range(0, count)
                .OrderBy(i => CaveEnemySpawnPlanner.StableHash($"{worldSeed}|{runSeed}|{caveLevel}|{SpawnSalt}_stock|{i}"))
                .ThenBy(i => i);

            foreach (var index in ordered)
            {
                selected.Add(index);
                if (selected.Count >= take)
                {
                    break;
                }
            }

            return selected;
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

            var stock = ResolveBiomeStock(worldSeed, runSeed, level.CaveLevel);

            // Lay the buy points out symmetrically around the merchant; sell point below.
            float spacing = 1f;
            float startX = -((stock.Count - 1) * spacing) * 0.5f;
            for (int i = 0; i < stock.Count; i++)
            {
                var offset = new Vector3(startX + i * spacing, 0f, 0f);
                CreateOfferPoint(s_merchantRoot.transform, stock[i], offset, level.CaveLevel);
            }

            CreateSellPoint(s_merchantRoot.transform, level.CaveLevel);

            GameEventBus.Publish(new PlayerActionFeedbackEvent("Um mercador errante montou banca neste andar..."));
            var offerIds = string.Join(" + ", stock.Select(o => o.ItemId));
            Debug.Log($"[CaveWanderingMerchant] Spawned on level {level.CaveLevel} ({ResolveBandId(level.CaveLevel)}) at grid ({tile.x},{tile.y}). Offers: {offerIds}.");

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
