using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CindarsHope.Cave;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Economy;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Player.Progression;
using CindarsHope.Quests.Runtime;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class Phase21ThorenCraftEconomyPlayModeTests
    {
        private const string OreId = "item_material_iron_ore";
        private const string BaseSwordId = "item_shop_weapon_sword_iron";
        private const string CapstoneId = "crafting_capstone_master_artisan";
        private const string SupportActionId = "skill_crafting_marca_eficiencia";
        private const string ForgeId = "farm_forge_01";
        private const string ShopPath = "Assets/_Game/Data/Economy/Shop_Brumdar.asset";

        [Serializable]
        private sealed class Observation
        {
            public string scenarioId;
            public string outcome = "NOT RUN";
            public string reason;
            public int level;
            public int capstoneRank;
            public int basePoints;
            public int actPoints;
            public int pointsSpent;
            public string stationId;
            public string recipeId;
            public string selection;
            public string outputItemId;
            public int goldBefore;
            public int goldAfter;
            public int oreBefore;
            public int oreAfter;
            public int oreConsumed;
            public int outputBefore;
            public int outputAfter;
            public int outputDelta;
            public int baseOutputBefore;
            public int baseOutputAfter;
            public int baseOutputDelta;
            public int dayIndex;
            public bool chargeAvailableBefore;
            public bool chargePendingAfterStart;
            public bool chargeConsumedAfterCollect;
            public bool chargeRestoredInFreshState;
            public int baseSellGold;
            public int qualitySellGold;
            public int baseSellWalletDelta;
            public int qualitySellWalletDelta;
            public int baseExpectedSellGold;
            public int qualityExpectedSellGold;
            public int baseValue;
            public int qualityValue;
            public string saveJson;
        }

        private SkillTreeManager _skills;
        private PlayerProgressionManager _progression;
        private InventoryManager _inventory;
        private PlayerManager _player;
        private CraftingRuntime _runtime;
        private CraftingStation _station;
        private CraftingSkillState _craftState;
        private RecipeDataSO _recipe;
        private SkillTreeSaveData _originalSkills;
        private PlayerProgressionSaveData _originalProgression;
        private QuestStateSectionSaveData _originalQuests;
        private InventorySaveData _originalInventory;
        private CraftingRuntimeSaveData _originalCraft;
        private int _originalGold;
        private bool _farmCachePreexisting;
        private bool _caveCachePreexisting;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _farmCachePreexisting = FarmSceneRuntimeStateCache.HasCachedState;
            _caveCachePreexisting = CaveRunStateCache.Cached != null;
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
            for (var frame = 0; frame < 180 && QuestRuntimeBootstrap.QuestService == null; frame++)
                yield return null;

            _skills = SkillTreeManager.Instance;
            _progression = PlayerProgressionManager.Instance;
            _inventory = GameBootstrap.Instance?.InventoryManager as InventoryManager;
            _player = GameBootstrap.Instance?.PlayerManager as PlayerManager;
            _craftState = DomainManagerRegistry.Get<CraftingSkillState>();
            Assert.That(_skills, Is.Not.Null);
            Assert.That(_progression, Is.Not.Null);
            Assert.That(_inventory, Is.Not.Null);
            Assert.That(_player, Is.Not.Null);
            Assert.That(_craftState, Is.Not.Null);
            Assert.That(QuestRuntimeBootstrap.QuestService, Is.Not.Null);
            Assert.That(_craftState.CaptureSaveData().ConsumedDayIndices, Is.Empty,
                "Diagnóstico exige processo batch isolado, sem carga já consumida.");
            Assert.That(_craftState.CaptureSaveData().PendingReservations, Is.Empty,
                "Diagnóstico exige processo batch isolado, sem reserva anterior.");
            Assert.That(ShopManager.Instance, Is.Not.Null);
            Assert.That(ShopManager.Instance.RegisteredShopIds, Is.Empty,
                "Diagnóstico exige processo batch isolado, sem sessão de loja anterior.");

            var points = new List<CraftingPoint>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                points.AddRange(root.GetComponentsInChildren<CraftingPoint>(true));
            var forge = points.SingleOrDefault(point => point.StationInstanceId == ForgeId);
            Assert.That(forge, Is.Not.Null, "FarmScene precisa materializar a forja física farm_forge_01.");
            Assert.That(forge.StationType, Is.EqualTo(WorkshopType.Forge));
            _runtime = CraftingRuntime.ActiveInstances.FirstOrDefault(runtime => runtime != null &&
                runtime.IsInitialized && runtime.InventoryManager == _inventory);
            Assert.That(_runtime, Is.Not.Null);
            Assert.That(_runtime.InventoryManager, Is.SameAs(_inventory));
            _station = _runtime.GetOrCreateStation(forge.StationInstanceId, forge.StationType);
            Assert.That(_station, Is.Not.Null);
            Assert.That(_station.IsBusy, Is.False,
                "Diagnóstico exige forja física livre no início do processo batch.");
            _recipe = _runtime.GetRecipesForStation(WorkshopType.Forge)
                .SingleOrDefault(recipe => recipe.Id == "recipe_forge_iron_sword");
            Assert.That(_recipe, Is.Not.Null, "Receita autorada deve estar disponível na forja Farm.");
            Assert.That(_recipe.StaminaCost, Is.Zero);
            Assert.That(_recipe.Ingredients.Count, Is.EqualTo(1));
            Assert.That(_recipe.Ingredients[0].ItemId, Is.EqualTo(OreId));
            Assert.That(_recipe.Ingredients[0].Amount, Is.EqualTo(2));
            Assert.That(_recipe.OutputItemId, Is.EqualTo(BaseSwordId));

            _originalSkills = _skills.CaptureSaveData();
            _originalProgression = _progression.CaptureSaveData();
            _originalQuests = QuestRuntimeBootstrap.CaptureSaveData();
            _originalInventory = _inventory.CaptureSaveData();
            _originalCraft = _runtime.CaptureSaveData();
            _originalGold = _player.CurrentGold;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_station != null && _station.HasActiveJob)
                _runtime.TryCancel(_station, out _);
            if (_originalCraft != null) _runtime?.LoadFromSaveData(_originalCraft);
            if (_originalInventory != null) _inventory?.RestoreFromSaveData(_originalInventory);
            if (_originalProgression != null) _progression?.RestoreFromSaveData(_originalProgression);
            if (_originalSkills != null) _skills?.RestoreFromSaveData(_originalSkills, _originalProgression?.Level ?? 1);
            if (_originalQuests != null) QuestRuntimeBootstrap.RestoreFromSaveData(_originalQuests);
            _player?.SetGold(_originalGold);
            if (!_farmCachePreexisting && FarmSceneRuntimeStateCache.HasCachedState)
                FarmSceneRuntimeStateCache.Clear();
            if (!_caveCachePreexisting && CaveRunStateCache.Cached != null)
                CaveRunStateCache.Set(null);
            yield return null;
        }

        [UnityTest]
        [Explicit("Diagnóstico T05: executar isoladamente em batch; modifica carga diária e QuestService global.")]
        public IEnumerator ThorenL50_R1QualityCraft_ConsumesTwoOreWithoutResalePremium()
        {
            const int level = Phase21BuildManifest.Level50;
            var row = NewRow("skills-phase21-thoren-craft-l50-r1-quality", level, 1);
            var sales = new List<EconomyTransactionCompletedEvent>();
            Action<EconomyTransactionCompletedEvent> onSale = evt =>
            {
                if (evt.WasSuccessful && evt.TransactionType == "ShopSell" &&
                    (evt.ItemId == BaseSwordId ||
                     evt.ItemId == LivingForgeOutputVariantCatalog.Quality1Id(BaseSwordId)))
                    sales.Add(evt);
            };
            GameEventBus.Subscribe(onSale);
            try
            {
                StageAndPurchase(level, 1, row);
                var qualityId = LivingForgeOutputVariantCatalog.Quality1Id(BaseSwordId);
                Assert.That(_inventory.TryGetItemData(BaseSwordId, out var baseItem), Is.True);
                Assert.That(_inventory.TryGetItemData(qualityId, out var qualityItem), Is.True);
                row.baseValue = baseItem.BaseValue;
                row.qualityValue = qualityItem.BaseValue;
                Assert.That(row.baseValue, Is.EqualTo(50));
                Assert.That(row.qualityValue, Is.EqualTo(row.baseValue));

                Assert.That(_inventory.TryAddItem(OreId, 4).Success, Is.True);
                row.goldBefore = _player.CurrentGold;
                row.baseOutputBefore = _inventory.GetAmount(BaseSwordId);
                Assert.That(_runtime.TryStartCraft(_station, _recipe, out var reason), Is.True, reason);
                yield return WaitAndCollect();
                row.baseOutputAfter = _inventory.GetAmount(BaseSwordId);
                row.baseOutputDelta = row.baseOutputAfter - row.baseOutputBefore;
                Assert.That(row.baseOutputDelta, Is.GreaterThanOrEqualTo(1));
                Assert.That(_craftState.IsChargeAvailable(row.dayIndex), Is.True);

                row.selection = LivingForgeBenefitChoice.Quality.ToString();
                row.outputItemId = qualityId;
                row.oreBefore = _inventory.GetAmount(OreId);
                row.outputBefore = _inventory.GetAmount(qualityId);
                row.chargeAvailableBefore = _craftState.IsChargeAvailable(row.dayIndex);
                Assert.That(row.chargeAvailableBefore, Is.True);
                Assert.That(_runtime.TryStartCraft(_station, _recipe,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality), out reason), Is.True, reason);
                row.oreAfter = _inventory.GetAmount(OreId);
                row.oreConsumed = row.oreBefore - row.oreAfter;
                row.chargePendingAfterStart = _craftState.HasPendingReservation(row.dayIndex);
                Assert.That(row.oreConsumed, Is.EqualTo(2));
                Assert.That(row.chargePendingAfterStart, Is.True);
                yield return WaitAndCollect();
                row.outputAfter = _inventory.GetAmount(qualityId);
                row.outputDelta = row.outputAfter - row.outputBefore;
                row.chargeConsumedAfterCollect = _craftState.IsChargeConsumed(row.dayIndex);
                Assert.That(row.outputDelta, Is.GreaterThanOrEqualTo(1));
                Assert.That(row.chargeConsumedAfterCollect, Is.True);
                Assert.That(_craftState.HasPendingReservation(row.dayIndex), Is.False);

                var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(ShopPath);
                var manager = ShopManager.Instance;
                Assert.That(shop, Is.Not.Null);
                Assert.That(manager, Is.Not.Null);
                Assert.That(manager.InitializeShop(shop), Is.True);
                row.baseExpectedSellGold = ShopManager.CalculateSellPrice(baseItem, shop);
                row.qualityExpectedSellGold = ShopManager.CalculateSellPrice(qualityItem, shop);
                Assert.That(row.qualityExpectedSellGold, Is.LessThanOrEqualTo(row.baseExpectedSellGold));
                var goldBeforeSale = _player.CurrentGold;
                var baseSale = manager.TrySellItem(shop.Id, BaseSwordId, 1, _player, _inventory);
                Assert.That(baseSale.Success, Is.True, baseSale.Message);
                row.baseSellWalletDelta = _player.CurrentGold - goldBeforeSale;
                Assert.That(sales.Count, Is.EqualTo(1));
                row.baseSellGold = sales[0].GoldDelta;
                goldBeforeSale = _player.CurrentGold;
                var qualitySale = manager.TrySellItem(shop.Id, qualityId, 1, _player, _inventory);
                Assert.That(qualitySale.Success, Is.True, qualitySale.Message);
                row.qualitySellWalletDelta = _player.CurrentGold - goldBeforeSale;
                Assert.That(sales.Count, Is.EqualTo(2));
                row.qualitySellGold = sales[1].GoldDelta;
                Assert.That(row.baseSellGold, Is.EqualTo(row.baseExpectedSellGold));
                Assert.That(row.qualitySellGold, Is.EqualTo(row.qualityExpectedSellGold));
                Assert.That(row.qualitySellGold, Is.LessThanOrEqualTo(row.baseSellGold));
                row.goldAfter = _player.CurrentGold;
                row.saveJson = SaveSnapshot();
                row.outcome = "PASS";
            }
            finally
            {
                GameEventBus.Unsubscribe(onSale);
                Write(row);
            }
        }

        [UnityTest]
        [Explicit("Diagnóstico T05: executar isoladamente em batch; modifica carga diária e QuestService global.")]
        public IEnumerator ThorenL54_R3SaveCommonMaterial_ConsumesOneOreAndPersistsCharge()
        {
            const int level = Phase21BuildManifest.Level54;
            var row = NewRow("skills-phase21-thoren-craft-l54-r3-save-common", level, 3);
            try
            {
                StageAndPurchase(level, 3, row);
                Assert.That(_runtime.IsLivingForgeCommonIngredient(OreId), Is.True);
                Assert.That(_inventory.TryAddItem(OreId, 2).Success, Is.True);
                row.selection = LivingForgeBenefitChoice.SaveCommonMaterial.ToString();
                row.outputItemId = BaseSwordId;
                row.goldBefore = _player.CurrentGold;
                row.oreBefore = _inventory.GetAmount(OreId);
                row.outputBefore = _inventory.GetAmount(BaseSwordId);
                row.chargeAvailableBefore = _craftState.IsChargeAvailable(row.dayIndex);
                Assert.That(row.chargeAvailableBefore, Is.True);
                Assert.That(_runtime.TryStartCraft(_station, _recipe,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.SaveCommonMaterial, OreId),
                    out var reason), Is.True, reason);
                row.oreAfter = _inventory.GetAmount(OreId);
                row.oreConsumed = row.oreBefore - row.oreAfter;
                row.chargePendingAfterStart = _craftState.HasPendingReservation(row.dayIndex);
                Assert.That(row.oreConsumed, Is.EqualTo(1));
                Assert.That(row.chargePendingAfterStart, Is.True);
                yield return WaitAndCollect();
                row.outputAfter = _inventory.GetAmount(BaseSwordId);
                row.outputDelta = row.outputAfter - row.outputBefore;
                row.chargeConsumedAfterCollect = _craftState.IsChargeConsumed(row.dayIndex);
                Assert.That(row.outputDelta, Is.GreaterThanOrEqualTo(1));
                Assert.That(row.chargeConsumedAfterCollect, Is.True);
                Assert.That(_craftState.HasPendingReservation(row.dayIndex), Is.False);
                Assert.That(_runtime.IsLivingForgeChargeAvailable, Is.False);
                row.goldAfter = _player.CurrentGold;
                Assert.That(row.goldAfter, Is.EqualTo(row.goldBefore));
                row.saveJson = SaveSnapshot();
                var saved = _craftState.CaptureSaveData();
                Assert.That(saved.ConsumedDayIndices, Does.Contain(row.dayIndex),
                    "O DTO precisa persistir a carga consumida.");
                var serialized = JsonUtility.ToJson(saved);
                var restoredDto = JsonUtility.FromJson<CraftingSkillSaveData>(serialized);
                var freshState = new CraftingSkillState();
                Assert.That(freshState.IsChargeConsumed(row.dayIndex), Is.False);
                freshState.RestoreFromSaveData(restoredDto);
                row.chargeRestoredInFreshState = freshState.IsChargeConsumed(row.dayIndex);
                Assert.That(row.chargeRestoredInFreshState, Is.True);
                Assert.That(freshState.HasPendingReservation(row.dayIndex), Is.False);
                row.outcome = "PASS";
            }
            finally { Write(row); }
        }

        private Observation NewRow(string id, int level, int rank) => new Observation
        {
            scenarioId = id, level = level, capstoneRank = rank,
            stationId = _station.StationInstanceId, recipeId = _recipe.Id,
            dayIndex = _craftState.CurrentDayIndex
        };

        private IEnumerator WaitAndCollect()
        {
            var deadline = Time.realtimeSinceStartup + 30f;
            while (!_station.HasCompletedOutput && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(_station.HasCompletedOutput, Is.True, "Craft de quatro segundos não terminou em 30 s reais.");
            Assert.That(_runtime.TryCollect(_station, out var reason), Is.True, reason);
        }

        private void StageAndPurchase(int level, int rank, Observation row)
        {
            var stage = _progression.CaptureSaveData();
            stage.TotalXp = ProgressionCurve.TotalXpForLevel(level);
            row.basePoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(level);
            stage.UnspentSkillPoints = row.basePoints;
            _progression.RestoreFromSaveData(stage);
            _skills.RestoreFromSaveData(new SkillTreeSaveData(), level);
            QuestRuntimeBootstrap.RestoreFromSaveData(new QuestStateSectionSaveData());
            Assert.That(_progression.Level, Is.EqualTo(level));

            var service = QuestRuntimeBootstrap.QuestService;
            var act1 = QuestMainActsIds.Finales[0].ActIdForSkillPoint;
            var act2 = QuestMainActsIds.Finales[1].ActIdForSkillPoint;
            Assert.That(service.TryAwardActSkillPoint(act1), Is.True);
            Assert.That(service.TryAwardActSkillPoint(act2), Is.True);
            row.actPoints = _progression.UnspentSkillPoints - row.basePoints;
            Assert.That(row.actPoints, Is.EqualTo(2));
            var questSave = QuestRuntimeBootstrap.CaptureSaveData();
            Assert.That(questSave.RewardedMainActIds, Does.Contain(act1));
            Assert.That(questSave.RewardedMainActIds, Does.Contain(act2));
            QuestRuntimeBootstrap.RestoreFromSaveData(questSave);
            Assert.That(service.TryAwardActSkillPoint(act1), Is.False);
            Assert.That(service.TryAwardActSkillPoint(act2), Is.False);
            Assert.That(_progression.UnspentSkillPoints, Is.EqualTo(row.basePoints + 2));

            Assert.That(PurchaseWithPrerequisites("crafting.marca_eficiencia", level,
                new HashSet<string>(), out var reason), Is.True, reason);
            Assert.That(_skills.TryAssignActiveSlot(0, SupportActionId), Is.True);
            Assert.That(PurchaseWithPrerequisites(CapstoneId, level,
                new HashSet<string>(), out reason), Is.True, reason);
            for (int current = 1; current < rank; current++)
                Assert.That(_skills.TryRankUpNode(CapstoneId, out reason), Is.True, reason);
            Assert.That(_skills.GetRank(CapstoneId), Is.EqualTo(rank));
            Assert.That(_runtime.LivingForgeRank, Is.EqualTo(rank));
            var reduction = CraftingPassiveConsumers.ResolveStationMaterialReduction(
                SkillModifierHooks.CraftCostReduction, _station.StationType,
                _recipe.RequiredStationType);
            var requirements = CraftingPassiveConsumers.BuildRequirements(
                _recipe.Ingredients, reduction, _runtime.IsLivingForgeCommonIngredient);
            Assert.That(requirements.Single(requirement => requirement.ItemId == OreId).Amount,
                Is.EqualTo(2), "Passivas compradas não podem reduzir a base antes da escolha do capstone.");
            row.pointsSpent = _skills.GetPointsSpentInTree("crafting");
            Assert.That(row.pointsSpent, Is.LessThanOrEqualTo(row.basePoints + row.actPoints));
            var skillSave = _skills.CaptureSaveData();
            _skills.RestoreFromSaveData(skillSave, level);
            Assert.That(_skills.GetRank(CapstoneId), Is.EqualTo(rank));
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo(SupportActionId));
        }

        private bool PurchaseWithPrerequisites(string nodeId, int level,
            HashSet<string> visiting, out string reason)
        {
            reason = string.Empty;
            if (_skills.GetRank(nodeId) > 0) return true;
            if (!_skills.NodeIndex.TryGetValue(nodeId, out var node))
            { reason = "Nó ausente: " + nodeId; return false; }
            if (!visiting.Add(nodeId))
            { reason = "Ciclo de pré-requisitos: " + nodeId; return false; }
            try
            {
                foreach (var prerequisite in node.PrerequisiteNodeIds ?? new List<string>())
                    if (!PurchaseWithPrerequisites(prerequisite, level, visiting, out reason)) return false;
                for (int attempt = 0; attempt < _skills.NodeIndex.Count * SkillTierRules.AbsoluteRankCap; attempt++)
                {
                    if (_skills.TryPurchaseNode(nodeId, level, null, out var feedback)) return true;
                    bool progressed = false;
                    foreach (var filler in _skills.NodeIndex.Values
                        .Where(n => n.TreeId == node.TreeId && n.Tier <= node.Tier &&
                            !n.IsCapstone && _skills.GetRank(n.SkillNodeId) == 0 &&
                            n.SkillNodeId != nodeId && !visiting.Contains(n.SkillNodeId))
                        .OrderBy(n => n.Tier).ThenBy(n => n.SkillNodeId))
                    {
                        if (!_skills.TryPurchaseNode(filler.SkillNodeId, level, null, out _)) continue;
                        progressed = true;
                        break;
                    }
                    if (progressed) continue;
                    foreach (var filler in _skills.NodeIndex.Values
                        .Where(n => n.TreeId == node.TreeId && !n.IsCapstone &&
                            _skills.GetRank(n.SkillNodeId) > 0 && n.SkillNodeId != nodeId)
                        .OrderBy(n => n.Tier).ThenBy(n => n.SkillNodeId))
                    {
                        if (!_skills.TryRankUpNode(filler.SkillNodeId, out _)) continue;
                        progressed = true;
                        break;
                    }
                    if (!progressed) { reason = feedback; return false; }
                }
                reason = "Limite de tentativas de compra";
                return false;
            }
            finally { visiting.Remove(nodeId); }
        }

        private string SaveSnapshot() => JsonUtility.ToJson(new Snapshot
        {
            skills = _skills.CaptureSaveData(),
            quests = QuestRuntimeBootstrap.CaptureSaveData(),
            craft = _runtime.CaptureSaveData(),
            charge = _craftState.CaptureSaveData(),
            inventory = _inventory.CaptureSaveData()
        });

        [Serializable]
        private sealed class Snapshot
        {
            public SkillTreeSaveData skills;
            public QuestStateSectionSaveData quests;
            public CraftingRuntimeSaveData craft;
            public CraftingSkillSaveData charge;
            public InventorySaveData inventory;
        }

        private static void Write(Observation row)
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Directory.CreateDirectory(directory);
            var stem = row.scenarioId;
            File.WriteAllText(Path.Combine(directory, stem + ".json"), JsonUtility.ToJson(row, true));
            File.WriteAllText(Path.Combine(directory, stem + ".csv"),
                "scenarioId,outcome,reason,level,capstoneRank,basePoints,actPoints,pointsSpent,stationId,recipeId,selection,outputItemId,goldBefore,goldAfter,oreBefore,oreAfter,oreConsumed,outputBefore,outputAfter,outputDelta,baseOutputBefore,baseOutputAfter,baseOutputDelta,dayIndex,chargeAvailableBefore,chargePendingAfterStart,chargeConsumedAfterCollect,chargeRestoredInFreshState,baseValue,qualityValue,baseSellGold,qualitySellGold,baseSellWalletDelta,qualitySellWalletDelta,baseExpectedSellGold,qualityExpectedSellGold\n" +
                string.Join(",", new[] { row.scenarioId, row.outcome, Escape(row.reason), row.level.ToString(),
                    row.capstoneRank.ToString(), row.basePoints.ToString(), row.actPoints.ToString(),
                    row.pointsSpent.ToString(), row.stationId, row.recipeId, row.selection, row.outputItemId,
                    row.goldBefore.ToString(), row.goldAfter.ToString(), row.oreBefore.ToString(),
                    row.oreAfter.ToString(), row.oreConsumed.ToString(), row.outputBefore.ToString(),
                    row.outputAfter.ToString(), row.outputDelta.ToString(), row.baseOutputBefore.ToString(),
                    row.baseOutputAfter.ToString(), row.baseOutputDelta.ToString(), row.dayIndex.ToString(), row.chargeAvailableBefore.ToString(),
                    row.chargePendingAfterStart.ToString(), row.chargeConsumedAfterCollect.ToString(),
                    row.chargeRestoredInFreshState.ToString(),
                    row.baseValue.ToString(), row.qualityValue.ToString(), row.baseSellGold.ToString(),
                    row.qualitySellGold.ToString(), row.baseSellWalletDelta.ToString(),
                    row.qualitySellWalletDelta.ToString(), row.baseExpectedSellGold.ToString(),
                    row.qualityExpectedSellGold.ToString() }) + "\n");
        }

        private static string Escape(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    }
}
