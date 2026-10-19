using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.NPC;
using CindarsHope.NPC.Services;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// fable_25 — testes EditMode dos serviços únicos de NPC (CA-1..CA-5). Cobre: disponibilidade por
    /// NPC + gates de amizade/flag (modelo de opção desabilitada com motivo), custos (ouro/item),
    /// integração com F21 via efeito mockado, pendências temporais (encomenda 3 dias, contrato semanal,
    /// prato 1×/dia, pasto re-feed) e round-trip/legado de save. Tudo determinístico, sem cena Unity.
    /// </summary>
    [TestFixture]
    public class NpcServicesTests
    {
        // ── Fakes ────────────────────────────────────────────────────────────────────────────────

        private sealed class FakeContext : INpcServiceContext
        {
            public int Gold = 1000;
            public readonly HashSet<int> FriendshipLevels = new HashSet<int>(); // pares (hash) não usados; ver Friendship
            public readonly Dictionary<string, int> FriendshipByNpc = new Dictionary<string, int>();
            public readonly HashSet<string> Flags = new HashSet<string>();
            public readonly Dictionary<string, int> Items = new Dictionary<string, int>();

            public int SpentGold;
            public readonly List<string> RemovedItems = new List<string>();

            public bool FriendshipAtLeast(string npcId, int level)
                => FriendshipByNpc.TryGetValue(npcId, out var lvl) && lvl >= level;

            public bool FlagIsSet(string flagId) => Flags.Contains(flagId);

            public int CurrentGold() => Gold;

            public bool SpendGold(int amount)
            {
                if (Gold < amount) return false;
                Gold -= amount;
                SpentGold += amount;
                return true;
            }

            public bool HasItem(string itemId, int amount)
                => Items.TryGetValue(itemId, out var n) && n >= amount;

            public bool RemoveItem(string itemId, int amount)
            {
                if (!HasItem(itemId, amount)) return false;
                Items[itemId] -= amount;
                RemovedItems.Add(itemId);
                return true;
            }
        }

        private sealed class FakeEffects : INpcServiceEffects
        {
            public bool AnalysisAvailable = true;
            public bool RepairAvailable = true;
            public bool BathAvailable = true;
            public bool PastureAvailable = true;
            public string BookId = "item_book_test";
            public string HuntTarget = "enemy_test_elite";
            public bool HuntRegisterOk = true;
            public bool DishAvailable = true;
            public bool IdentifyAvailable = true;

            public int AnalysisCalls;
            public float ArmedRepairFraction = -1f;
            public int BathCalls;
            public int PastureCalls;
            public int DishCalls;
            public int IdentifyCalls;

            public bool TryCreatureAnalysis() { AnalysisCalls++; return AnalysisAvailable; }
            public bool ApplyRepairDiscount(float f) { ArmedRepairFraction = f; return RepairAvailable; }
            public bool ApplyThermalBath() { BathCalls++; return BathAvailable; }
            public bool ApplyPremiumPasture(int days) { PastureCalls++; return PastureAvailable; }
            public string ResolveOrderedBookItemId() => BookId;
            public string ResolveHuntContractTarget() => HuntTarget;
            public bool RegisterHuntContract(string t, int m) => HuntRegisterOk;
            public bool ApplyDailyDish() { DishCalls++; return DishAvailable; }
            public bool TryIdentifyRelic() { IdentifyCalls++; return IdentifyAvailable; }
        }

        private FakeContext _ctx;
        private FakeEffects _fx;
        private NpcServicesPendingState _pending;
        private NpcServiceExecutor _executor;
        private int _day;

        [SetUp]
        public void SetUp()
        {
            _ctx = new FakeContext();
            _fx = new FakeEffects();
            _pending = new NpcServicesPendingState();
            _day = 10;
            _executor = new NpcServiceExecutor(_pending, _ctx, _fx) { CurrentDayProvider = () => _day };
        }

        // ── CA-1: catálogo + disponibilidade por NPC ───────────────────────────────────────────────

        [Test]
        public void Catalog_HasEightServices_AcrossDistinctNpcs()
        {
            Assert.AreEqual(8, NpcServiceCatalog.ServiceCount);
            var npcs = new HashSet<string>();
            foreach (var s in NpcServiceCatalog.All) npcs.Add(s.NpcId);
            Assert.AreEqual(8, npcs.Count, "Cada serviço deve pertencer a um NPC distinto.");
        }

        [Test]
        public void Catalog_NpcIds_AreCanonicalRosterIds()
        {
            foreach (var s in NpcServiceCatalog.All)
            {
                Assert.IsTrue(NpcTownRosterRegistry.TryGet(s.NpcId, out _),
                    $"npcId '{s.NpcId}' do serviço '{s.ServiceId}' não está no roster canônico.");
            }
        }

        [Test]
        public void IsServiceProvider_TrueForThalindra_FalseForUnknown()
        {
            Assert.IsTrue(NpcServiceCatalog.IsServiceProvider("npc_thalindra"));
            Assert.IsFalse(NpcServiceCatalog.IsServiceProvider("npc_renko"));
            Assert.IsFalse(NpcServiceCatalog.IsServiceProvider(null));
        }

        // ── CA-4: opção desabilitada com motivo (descoberta > ocultação) ─────────────────────────────

        [Test]
        public void Option_FriendshipGated_DisabledWithReason_AndNeverHidden()
        {
            // Brumdar reparo exige amizade 3; sem amizade ⇒ desabilitada com motivo.
            var def = NpcServiceCatalog.GetById(NpcServiceCatalog.RepairDiscountServiceId);
            var option = _executor.BuildOption(def);

            Assert.IsFalse(option.Enabled);
            StringAssert.Contains("Amizade 3", option.DisabledReason);
            StringAssert.Contains("Amizade 3", option.Label, "O motivo deve aparecer no rótulo (não some).");
            StringAssert.Contains("Reparo", option.Label);
        }

        [Test]
        public void Option_FriendshipMet_Enabled_NoReason()
        {
            _ctx.FriendshipByNpc["npc_brumdar"] = 3;
            var def = NpcServiceCatalog.GetById(NpcServiceCatalog.RepairDiscountServiceId);
            var option = _executor.BuildOption(def);

            Assert.IsTrue(option.Enabled);
            Assert.IsEmpty(option.DisabledReason);
            Assert.AreEqual("Reparo com Desconto", option.Label);
        }

        [Test]
        public void Option_NoGate_AlwaysEnabled()
        {
            var def = NpcServiceCatalog.GetById(NpcServiceCatalog.ThermalBathServiceId);
            var option = _executor.BuildOption(def);
            Assert.IsTrue(option.Enabled);
        }

        // ── CA-1: gate fecha antes, abre depois (execução) ──────────────────────────────────────────

        [Test]
        public void Execute_GateClosed_BeforeFriendship_NoCharge()
        {
            var result = _executor.TryExecute(NpcServiceCatalog.PremiumPastureServiceId);
            Assert.AreEqual(NpcServiceOutcome.GateClosed, result.Outcome);
            Assert.AreEqual(0, _fx.PastureCalls, "Efeito não deve rodar com gate fechado.");
        }

        [Test]
        public void Execute_GateOpen_AfterFriendship_RunsEffect()
        {
            _ctx.FriendshipByNpc["npc_eiran"] = 4;
            var result = _executor.TryExecute(NpcServiceCatalog.PremiumPastureServiceId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, _fx.PastureCalls);
        }

        // ── Custos (ouro/item) ───────────────────────────────────────────────────────────────────

        [Test]
        public void Execute_ThermalBath_ChargesFiftyGold_OnSuccess()
        {
            _ctx.Gold = 50;
            var result = _executor.TryExecute(NpcServiceCatalog.ThermalBathServiceId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(NpcServiceCatalog.ThermalBathGoldCost, _ctx.SpentGold);
            Assert.AreEqual(0, _ctx.Gold);
        }

        [Test]
        public void Execute_InsufficientGold_NoEffect_NoCharge()
        {
            _ctx.Gold = 10; // banho custa 50
            var result = _executor.TryExecute(NpcServiceCatalog.ThermalBathServiceId);
            Assert.AreEqual(NpcServiceOutcome.InsufficientGold, result.Outcome);
            Assert.AreEqual(0, _ctx.SpentGold);
            Assert.AreEqual(0, _fx.BathCalls, "Efeito não roda sem ouro suficiente.");
        }

        [Test]
        public void Execute_EffectUnavailable_DoesNotCharge()
        {
            _ctx.Gold = 500;
            _fx.BathAvailable = false; // sistema-alvo (F16) ausente
            var result = _executor.TryExecute(NpcServiceCatalog.ThermalBathServiceId);
            Assert.AreEqual(NpcServiceOutcome.EffectUnavailable, result.Outcome);
            Assert.AreEqual(0, _ctx.SpentGold, "Efeito indisponível não cobra.");
        }

        // ── CA-2: análise (F21) consome custo + concede via efeito mockado ───────────────────────────

        [Test]
        public void Execute_Analysis_Charges80Gold_WhenGrantSucceeds()
        {
            _ctx.Gold = 80;
            _fx.AnalysisAvailable = true;
            var result = _executor.TryExecute(NpcServiceCatalog.AnalysisServiceId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, _fx.AnalysisCalls);
            Assert.AreEqual(NpcServiceCatalog.AnalysisGoldCost, _ctx.SpentGold);
        }

        [Test]
        public void Execute_Analysis_NothingToGrant_NoCharge()
        {
            _ctx.Gold = 80;
            _fx.AnalysisAvailable = false; // sem parte/nada novo (idempotência herdada da F21)
            var result = _executor.TryExecute(NpcServiceCatalog.AnalysisServiceId);
            Assert.AreEqual(NpcServiceOutcome.MissingItem, result.Outcome);
            Assert.AreEqual(0, _ctx.SpentGold);
        }

        // ── Reparo com desconto: arma a fração correta ───────────────────────────────────────────────

        [Test]
        public void Execute_RepairDiscount_ArmsThirtyPercent()
        {
            _ctx.FriendshipByNpc["npc_brumdar"] = 5;
            var result = _executor.TryExecute(NpcServiceCatalog.RepairDiscountServiceId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(NpcServiceCatalog.RepairDiscountFraction, _fx.ArmedRepairFraction, 0.0001f);
        }

        [Test]
        public void RepairDiscountState_AppliesAndConsumes()
        {
            NpcRepairDiscountState.Reset();
            NpcRepairDiscountState.ArmDiscount(0.30f);
            Assert.IsTrue(NpcRepairDiscountState.HasPendingDiscount);
            // 100 × (1 − 0.30) = 70 (floor).
            Assert.AreEqual(70, NpcRepairDiscountState.ApplyToRepairCost(100));
            // one-shot: consumido.
            Assert.IsFalse(NpcRepairDiscountState.HasPendingDiscount);
            Assert.AreEqual(100, NpcRepairDiscountState.ApplyToRepairCost(100));
            NpcRepairDiscountState.Reset();
        }

        // ── CA-3: encomenda chega no dia +3 ──────────────────────────────────────────────────────────

        [Test]
        public void BookOrder_DeliversOnDayPlusThree()
        {
            _ctx.Gold = 200;
            _day = 5;
            var result = _executor.TryExecute(NpcServiceCatalog.BookOrderServiceId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, _pending.PendingBookOrderCount);

            // Dia 6, 7: ainda não chegou.
            Assert.AreEqual(0, _executor.OnDayStarted(6).Count);
            Assert.AreEqual(0, _executor.OnDayStarted(7).Count);

            // Dia 8 (5+3): chega exatamente 1 livro.
            var due = _executor.OnDayStarted(8);
            Assert.AreEqual(1, due.Count);
            Assert.AreEqual("item_book_test", due[0]);
            Assert.AreEqual(0, _pending.PendingBookOrderCount, "Encomenda entregue só uma vez.");
        }

        // ── CA-3: contrato de caça semanal renova; conclui por inimigo abatido ──────────────────────

        [Test]
        public void HuntContract_WeeklyGate_BlocksSameWeek_RenewsNextWeek()
        {
            _day = 14; // semana 2 (14/7=2)
            var first = _executor.TryExecute(NpcServiceCatalog.HuntContractServiceId);
            Assert.IsTrue(first.Success);
            Assert.IsNotNull(_pending.HuntContract);

            // Mesma semana ⇒ bloqueado.
            var same = _executor.TryExecute(NpcServiceCatalog.HuntContractServiceId);
            Assert.AreEqual(NpcServiceOutcome.TemporallyUnavailable, same.Outcome);

            // Semana seguinte ⇒ renova.
            _day = 21; // semana 3
            var next = _executor.TryExecute(NpcServiceCatalog.HuntContractServiceId);
            Assert.IsTrue(next.Success);
        }

        [Test]
        public void HuntContract_CompletesOnTargetKill()
        {
            _day = 14;
            _fx.HuntTarget = "enemy_test_elite";
            _executor.TryExecute(NpcServiceCatalog.HuntContractServiceId);

            Assert.IsFalse(_executor.OnEnemyKilled("enemy_other"));
            Assert.IsTrue(_executor.OnEnemyKilled("enemy_test_elite"));
            Assert.IsTrue(_pending.HuntContract.Completed);
            // Idempotente: segunda morte não re-conclui.
            Assert.IsFalse(_executor.OnEnemyKilled("enemy_test_elite"));
        }

        // ── CA-3: prato do dia 1×/dia ────────────────────────────────────────────────────────────────

        [Test]
        public void DailyDish_OncePerDay_BlocksSecondSameDay_AllowsNextDay()
        {
            _ctx.FriendshipByNpc["npc_orlan"] = 2;
            _day = 10;
            var first = _executor.TryExecute(NpcServiceCatalog.DailyDishServiceId);
            Assert.IsTrue(first.Success);
            Assert.AreEqual(1, _fx.DishCalls);

            var second = _executor.TryExecute(NpcServiceCatalog.DailyDishServiceId);
            Assert.AreEqual(NpcServiceOutcome.TemporallyUnavailable, second.Outcome);
            Assert.AreEqual(1, _fx.DishCalls, "Prato não roda 2× no mesmo dia.");

            _day = 11;
            var nextDay = _executor.TryExecute(NpcServiceCatalog.DailyDishServiceId);
            Assert.IsTrue(nextDay.Success);
        }

        // ── Pasto premium: ativo por N dias (re-feed) ────────────────────────────────────────────────

        [Test]
        public void PremiumPasture_StaysActiveForThreeDays()
        {
            _ctx.FriendshipByNpc["npc_eiran"] = 4;
            _day = 10;
            Assert.IsTrue(_executor.TryExecute(NpcServiceCatalog.PremiumPastureServiceId).Success);

            // Dias 10, 11, 12 ativos (3 dias); dia 13 inativo.
            Assert.IsTrue(_executor.IsPremiumPastureActive(10));
            Assert.IsTrue(_executor.IsPremiumPastureActive(12));
            Assert.IsFalse(_executor.IsPremiumPastureActive(13));
        }

        // ── CA-5: persistência round-trip + load legado ─────────────────────────────────────────────

        [Test]
        public void Save_RoundTrip_PreservesPendencies()
        {
            _ctx.Gold = 1000;
            _ctx.FriendshipByNpc["npc_orlan"] = 2;
            _day = 5;
            _executor.TryExecute(NpcServiceCatalog.BookOrderServiceId);   // encomenda dia 8
            _day = 14;
            _executor.TryExecute(NpcServiceCatalog.HuntContractServiceId); // contrato semana 2
            _executor.TryExecute(NpcServiceCatalog.DailyDishServiceId);    // prato usado dia 14

            var data = _pending.Capture();

            var restored = new NpcServicesPendingState();
            restored.Restore(data);

            Assert.AreEqual(1, restored.PendingBookOrderCount);
            Assert.IsNotNull(restored.HuntContract);
            Assert.AreEqual("enemy_test_elite", restored.HuntContract.TargetEnemyId);
            Assert.AreEqual(14, restored.DailyDishLastUsedDay);
            Assert.IsFalse(restored.CanUseDailyDish(14), "Prato usado no dia 14 não pode repetir após load.");
            Assert.IsTrue(restored.CanUseDailyDish(15));
        }

        [Test]
        public void Save_LegacyNull_LoadsEmpty_NoError()
        {
            var state = new NpcServicesPendingState();
            Assert.DoesNotThrow(() => state.Restore(null));
            Assert.AreEqual(0, state.PendingBookOrderCount);
            Assert.IsNull(state.HuntContract);
            Assert.AreEqual(0, state.DailyDishLastUsedDay);
            Assert.IsTrue(state.CanUseDailyDish(1));
        }

        [Test]
        public void Save_RoundTrip_PreservesPastureActiveWindow()
        {
            _ctx.FriendshipByNpc["npc_eiran"] = 4;
            _day = 20;
            _executor.TryExecute(NpcServiceCatalog.PremiumPastureServiceId);

            var data = _pending.Capture();
            var restored = new NpcServicesPendingState();
            restored.Restore(data);

            Assert.IsTrue(restored.IsPremiumPastureActive(22));
            Assert.IsFalse(restored.IsPremiumPastureActive(23));
        }

        // ── Gate evaluator fail-closed sem predicado ─────────────────────────────────────────────────

        [Test]
        public void GateEvaluator_NullPredicates_FailClosedWithReason()
        {
            var def = NpcServiceCatalog.GetById(NpcServiceCatalog.PremiumPastureServiceId);
            var option = NpcServiceGateEvaluator.Evaluate(def, null, null);
            Assert.IsFalse(option.Enabled);
            StringAssert.Contains("Amizade 4", option.DisabledReason);
        }

        [Test]
        public void GateEvaluator_NullDefinition_DisabledSafe()
        {
            var option = NpcServiceGateEvaluator.Evaluate(null, null, null);
            Assert.IsFalse(option.Enabled);
        }
    }
}
