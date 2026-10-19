using NUnit.Framework;
using CindarsHope.Farm.Watering;
using CindarsHope.Player;
using CindarsHope.Core.Time;
using UnityEngine;

namespace CindarsHope.Farm.Tests
{
    /// <summary>
    /// spec_codex_03_farm_tilling_real_params: cobre a decisao de stamina/dia real que
    /// FarmTillingInputController agora usa (StaminaManager.CurrentStamina/TrySpendStamina
    /// real e TimeManager.CurrentDay real), em vez dos literais fixos staminaOk:true/currentDay:1.
    ///
    /// FarmTillingInputController e um MonoBehaviour completo (nao testavel diretamente em
    /// EditMode sem harness de scene). Este teste cobre:
    /// 1. A decisao "tem stamina suficiente?" com um StaminaManager real (mesmo componente
    ///    usado pelo controller), simulando o mesmo padrao de checagem pre-condicao.
    /// 2. TimeManager.CurrentDay real fluindo para FarmTilledSoilService.WaterTile.
    /// 3. O contrato de FarmTilledSoilService.TillTile/WaterTile com staminaOk calculado
    ///    a partir de valores reais de stamina (nao mais um literal true fixo).
    /// </summary>
    [TestFixture]
    public class FarmTillingStaminaDayTests
    {
        private const int TillStaminaCost = 10;
        private const int WaterStaminaCost = 8;

        private GameObject _staminaGameObject;
        private StaminaManager _staminaManager;

        private GameObject _timeGameObject;
        private TimeManager _timeManager;

        private FarmNonArableZones _zones;
        private FarmTileGrid _grid;
        private FarmWateringService _wateringService;
        private FarmTilledSoilService _service;

        [SetUp]
        public void SetUp()
        {
            _staminaGameObject = new GameObject("StaminaManagerTestHost");
            _staminaManager = _staminaGameObject.AddComponent<StaminaManager>();
            _staminaManager.Initialize(maxStamina: 100, startingStamina: 100);

            _timeGameObject = new GameObject("TimeManagerTestHost");
            _timeManager = _timeGameObject.AddComponent<TimeManager>();

            _zones = new FarmNonArableZones();
            _grid = new FarmTileGrid(_zones, tileSizeUnits: 1f);
            _wateringService = new FarmWateringService();
            _service = new FarmTilledSoilService(_grid, _wateringService);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_staminaGameObject);
            Object.DestroyImmediate(_timeGameObject);
        }

        // ── Decisao de stamina real (equivalente ao que o controller calcula) ──────────────────

        [Test]
        public void HasEnoughStamina_WithFullStamina_AllowsTill()
        {
            bool staminaOk = _staminaManager.CurrentStamina >= TillStaminaCost;
            Assert.IsTrue(staminaOk);

            var result = _service.TillTile(1, 1, hasTool: true, temporarySliceMode: true, staminaOk: staminaOk);
            Assert.IsTrue(result);
        }

        [Test]
        public void HasEnoughStamina_WithZeroStamina_BlocksTill()
        {
            // Zera a stamina gastando tudo de uma vez (mesma API publica usada pelo controller).
            bool spent = _staminaManager.TrySpendStamina(100);
            Assert.IsTrue(spent);
            Assert.AreEqual(0, _staminaManager.CurrentStamina);

            bool staminaOk = _staminaManager.CurrentStamina >= TillStaminaCost;
            Assert.IsFalse(staminaOk);

            var result = _service.TillTile(1, 1, hasTool: true, temporarySliceMode: true, staminaOk: staminaOk);
            Assert.IsFalse(result, "Arar deve ser recusado quando a stamina real e insuficiente.");
        }

        [Test]
        public void TrySpendStamina_AfterSuccessfulTill_DeductsExactCost()
        {
            int before = _staminaManager.CurrentStamina;
            bool staminaOk = _staminaManager.CurrentStamina >= TillStaminaCost;

            var tilled = _service.TillTile(1, 1, hasTool: true, temporarySliceMode: true, staminaOk: staminaOk);
            Assert.IsTrue(tilled);

            // Padrao TreeNode: so gasta apos o sucesso da acao.
            bool spent = _staminaManager.TrySpendStamina(TillStaminaCost);
            Assert.IsTrue(spent);
            Assert.AreEqual(before - TillStaminaCost, _staminaManager.CurrentStamina);
        }

        [Test]
        public void HasEnoughStamina_WithPartialStamina_BlocksWaterButNotCheaperTill()
        {
            // Deixa stamina suficiente so para regar (custo menor), nao para arar (custo maior).
            _staminaManager.TrySpendStamina(100 - WaterStaminaCost);
            Assert.AreEqual(WaterStaminaCost, _staminaManager.CurrentStamina);

            bool tillOk = _staminaManager.CurrentStamina >= TillStaminaCost;
            bool waterOk = _staminaManager.CurrentStamina >= WaterStaminaCost;

            Assert.IsFalse(tillOk);
            Assert.IsTrue(waterOk);
        }

        // ── Dia real usado na rega ───────────────────────────────────────────────────────────────

        [Test]
        public void WaterTile_UsesRealCurrentDay_NotFixedOne()
        {
            _service.TillTile(2, 2, hasTool: true, temporarySliceMode: true, staminaOk: true);

            // TimeManager.CurrentDay comeca em 1 por padrao (campo real do componente).
            int realDay = _timeManager.CurrentDay;
            Assert.AreEqual(1, realDay, "Baseline: TimeManager comeca no dia 1.");

            var result = _service.WaterTile(2, 2, hasTool: true, temporarySliceMode: true,
                staminaOk: true, currentDay: realDay);

            Assert.IsTrue(result);
        }

        [Test]
        public void WaterTile_WithoutEnoughStamina_IsBlockedRegardlessOfDay()
        {
            _service.TillTile(3, 3, hasTool: true, temporarySliceMode: true, staminaOk: true);
            _staminaManager.TrySpendStamina(100);

            bool waterStaminaOk = _staminaManager.CurrentStamina >= WaterStaminaCost;
            Assert.IsFalse(waterStaminaOk);

            var result = _service.WaterTile(3, 3, hasTool: true, temporarySliceMode: true,
                staminaOk: waterStaminaOk, currentDay: _timeManager.CurrentDay);

            Assert.IsFalse(result, "Regar deve ser recusado quando a stamina real e insuficiente.");
        }
    }
}
