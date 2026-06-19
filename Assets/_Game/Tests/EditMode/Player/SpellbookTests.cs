using System.Collections.Generic;
using CindarsHope.Inventory.Data;
using CindarsHope.Magic;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// fable_07 — testes da lógica determinística do grimório e do uso de itens de magia.
    /// Alvo: SpellbookState (C# puro) e SpellItemUseHandler (serviço puro com fakes), sem cena.
    /// Cobre os critérios de aceite:
    ///   CA-1 aprendizado persistente (learn idempotente, LearnableScroll consome+aprende, round-trip);
    ///   CA-2 fontes distintas (CastScroll casta sem aprender; Tome aprende no 3º uso; wand só equipada);
    ///   CA-3 zero regressão (magia equipada disponível via EquippedItem grant SEM entrar em knownSpellIds).
    /// E o save: default vazio, seção ausente (null), ID inválido (vazio) ignorado, round-trip, idempotência.
    /// </summary>
    public class SpellbookTests
    {
        // ----------------------------------------------------------------- SpellbookState: aprendizado

        [Test]
        public void TryLearn_NewSpell_AddsAndReturnsTrue()
        {
            var state = new SpellbookState();
            Assert.IsTrue(state.TryLearn("spell_fire_spark", SpellSourceType.LearnableScroll));
            Assert.IsTrue(state.IsKnown("spell_fire_spark"));
            Assert.AreEqual(1, CountOf(state.KnownSpellIds));
        }

        [Test]
        public void TryLearn_SameSpellTwice_IsIdempotent()
        {
            var state = new SpellbookState();
            Assert.IsTrue(state.TryLearn("spell_fire_spark", SpellSourceType.LearnableScroll));
            Assert.IsFalse(state.TryLearn("spell_fire_spark", SpellSourceType.LearnableScroll), "Re-aprender deve retornar false.");
            Assert.AreEqual(1, CountOf(state.KnownSpellIds), "Conjunto nao deve duplicar.");
        }

        [Test]
        public void TryLearn_NullOrEmpty_ReturnsFalse()
        {
            var state = new SpellbookState();
            Assert.IsFalse(state.TryLearn(null, SpellSourceType.LearnableScroll));
            Assert.IsFalse(state.TryLearn("  ", SpellSourceType.LearnableScroll));
            Assert.AreEqual(0, CountOf(state.KnownSpellIds));
        }

        // ----------------------------------------------------------------- CanCast por fonte

        [Test]
        public void CanCast_KnownSpell_True_UnknownSpell_False()
        {
            var state = new SpellbookState();
            state.TryLearn("spell_known", SpellSourceType.Tome);
            Assert.IsTrue(state.CanCast("spell_known"));
            Assert.IsFalse(state.CanCast("spell_unknown"));
        }

        [Test]
        public void CanCast_EquipmentGranted_True_WithoutBecomingKnown()
        {
            // CA-3: magia do item equipado é castável, mas NÃO entra em knownSpellIds.
            var state = new SpellbookState();
            state.AddEquipmentGrant("spell_spark");

            Assert.IsTrue(state.CanCast("spell_spark"), "Magia equipada deve ser castavel.");
            Assert.IsFalse(state.IsKnown("spell_spark"), "Magia equipada NAO deve ser conhecida.");
            Assert.AreEqual(0, CountOf(state.KnownSpellIds), "Grimorio permanente deve ficar vazio.");
        }

        [Test]
        public void RemoveEquipmentGrant_DropsCastability()
        {
            var state = new SpellbookState();
            state.AddEquipmentGrant("spell_spark");
            state.RemoveEquipmentGrant("spell_spark");
            Assert.IsFalse(state.CanCast("spell_spark"), "Desequipar remove a castabilidade.");
        }

        // ----------------------------------------------------------------- Tome progress

        [Test]
        public void RegisterTomeUse_LearnsOnThirdUse()
        {
            var state = new SpellbookState();

            Assert.IsFalse(state.RegisterTomeUse("spell_ice_shard", 3, out _), "1o uso: ainda estudando.");
            Assert.AreEqual(1, state.GetTomeUses("spell_ice_shard"));

            Assert.IsFalse(state.RegisterTomeUse("spell_ice_shard", 3, out _), "2o uso: ainda estudando.");
            Assert.AreEqual(2, state.GetTomeUses("spell_ice_shard"));

            Assert.IsTrue(state.RegisterTomeUse("spell_ice_shard", 3, out bool firstTime), "3o uso: aprende.");
            Assert.IsTrue(firstTime, "3o uso deve sinalizar aprendizado novo.");
            Assert.IsTrue(state.IsKnown("spell_ice_shard"));
            Assert.AreEqual(0, state.GetTomeUses("spell_ice_shard"), "Contador zera ao aprender.");
        }

        [Test]
        public void RegisterTomeUse_AlreadyKnown_IsNoOp()
        {
            var state = new SpellbookState();
            state.TryLearn("spell_ice_shard", SpellSourceType.LearnableScroll);

            Assert.IsTrue(state.RegisterTomeUse("spell_ice_shard", 3, out bool firstTime), "Ja conhecida: retorna true.");
            Assert.IsFalse(firstTime, "Nao deve sinalizar aprendizado novo.");
            Assert.AreEqual(0, state.GetTomeUses("spell_ice_shard"));
        }

        [Test]
        public void RegisterTomeUse_RequiredOneOrLess_LearnsImmediately()
        {
            var state = new SpellbookState();
            Assert.IsTrue(state.RegisterTomeUse("spell_x", 1, out var first1));
            Assert.IsTrue(first1);

            var state0 = new SpellbookState();
            Assert.IsTrue(state0.RegisterTomeUse("spell_y", 0, out var first0), "usesRequired<=0 deve aprender no 1o uso.");
            Assert.IsTrue(first0);
        }

        // ----------------------------------------------------------------- Save: default / missing / invalid / round-trip

        [Test]
        public void CaptureSaveData_Default_IsEmpty()
        {
            var state = new SpellbookState();
            var data = state.CaptureSaveData();
            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.KnownSpellIds.Count);
            Assert.AreEqual(0, data.TomeProgress.Count);
        }

        [Test]
        public void RestoreFromSaveData_Null_LeavesEmpty()
        {
            // Seção ausente (save legado) = grimório vazio, sem exceção.
            var state = new SpellbookState();
            state.RestoreFromSaveData(null);
            Assert.AreEqual(0, CountOf(state.KnownSpellIds));
            Assert.IsFalse(state.CanCast("anything"));
        }

        [Test]
        public void RestoreFromSaveData_InvalidIds_AreIgnored()
        {
            var state = new SpellbookState();
            var data = new SpellbookSaveData
            {
                KnownSpellIds = new List<string> { "spell_valid", "", null, "   " },
                TomeProgress = new List<TomeProgressEntry>
                {
                    new TomeProgressEntry { SpellId = "", Uses = 5 },
                    new TomeProgressEntry { SpellId = null, Uses = 2 },
                    new TomeProgressEntry { SpellId = "spell_tome", Uses = 0 } // Uses<=0 ignorado
                }
            };

            state.RestoreFromSaveData(data);

            Assert.IsTrue(state.IsKnown("spell_valid"));
            Assert.AreEqual(1, CountOf(state.KnownSpellIds), "Apenas o ID valido entra.");
            Assert.AreEqual(0, state.GetTomeUses("spell_tome"), "Tome com Uses<=0 nao entra.");
        }

        [Test]
        public void SaveRoundTrip_PreservesKnownSpellsAndTomeProgress()
        {
            var state = new SpellbookState();
            state.TryLearn("spell_a", SpellSourceType.LearnableScroll);
            state.TryLearn("spell_b", SpellSourceType.FonteStory);
            state.RegisterTomeUse("spell_c", 3, out _); // 1/3 estudando

            var captured = state.CaptureSaveData();

            // Novo estado restaurado a partir do capturado deve refletir o mesmo conhecimento.
            var restored = new SpellbookState();
            restored.RestoreFromSaveData(captured);

            Assert.IsTrue(restored.IsKnown("spell_a"));
            Assert.IsTrue(restored.IsKnown("spell_b"));
            Assert.IsFalse(restored.IsKnown("spell_c"), "spell_c ainda em estudo, nao conhecida.");
            Assert.AreEqual(1, restored.GetTomeUses("spell_c"), "Progresso de tomo preservado.");
            Assert.AreEqual(2, CountOf(restored.KnownSpellIds));
        }

        [Test]
        public void Restore_IsIdempotent_NoDuplication()
        {
            var state = new SpellbookState();
            var data = new SpellbookSaveData { KnownSpellIds = new List<string> { "spell_a", "spell_a" } };

            state.RestoreFromSaveData(data);
            state.RestoreFromSaveData(data); // restaurar de novo

            Assert.AreEqual(1, CountOf(state.KnownSpellIds), "HashSet nao duplica mesmo em re-restore.");
        }

        [Test]
        public void Capture_DoesNotPersistEquipmentGrants()
        {
            // Grants de equipamento são transitórios — não devem ir para o save.
            var state = new SpellbookState();
            state.AddEquipmentGrant("spell_spark");
            var data = state.CaptureSaveData();
            Assert.AreEqual(0, data.KnownSpellIds.Count, "Equipamento nao entra em knownSpellIds salvas.");
        }

        // ----------------------------------------------------------------- SpellItemUseHandler: fontes

        [Test]
        public void Handler_LearnableScroll_LearnsAndConsumes()
        {
            var inv = new FakeInventory();
            var item = MakeItem("scroll_learn_fire", SpellSourceType.LearnableScroll, taughtSpellId: "spell_fire");
            inv.Add(item, 1);
            var state = new SpellbookState();
            var handler = new SpellItemUseHandler(inv, state);

            var result = handler.UseItem("scroll_learn_fire");

            Assert.AreEqual(SpellItemUseHandler.UseResult.Learned, result);
            Assert.IsTrue(state.IsKnown("spell_fire"));
            Assert.AreEqual(0, inv.AmountOf("scroll_learn_fire"), "Scroll consumido.");
        }

        [Test]
        public void Handler_LearnableScroll_PrerequisiteNotMet_DoesNotLearnOrConsume()
        {
            var inv = new FakeInventory();
            var item = MakeItem("scroll_locked", SpellSourceType.LearnableScroll, taughtSpellId: "spell_locked", requiredNode: "node_magic_1");
            inv.Add(item, 1);
            var state = new SpellbookState();
            // Skill node bloqueado.
            var handler = new SpellItemUseHandler(inv, state, isSkillNodeUnlocked: _ => false);

            var result = handler.UseItem("scroll_locked");

            Assert.AreEqual(SpellItemUseHandler.UseResult.PrerequisiteNotMet, result);
            Assert.IsFalse(state.IsKnown("spell_locked"));
            Assert.AreEqual(1, inv.AmountOf("scroll_locked"), "Scroll NAO consumido quando bloqueado.");
        }

        [Test]
        public void Handler_LearnableScroll_AlreadyKnown_DoesNotConsume()
        {
            var inv = new FakeInventory();
            var item = MakeItem("scroll_dup", SpellSourceType.LearnableScroll, taughtSpellId: "spell_dup");
            inv.Add(item, 1);
            var state = new SpellbookState();
            state.TryLearn("spell_dup", SpellSourceType.LearnableScroll);
            var handler = new SpellItemUseHandler(inv, state);

            var result = handler.UseItem("scroll_dup");

            Assert.AreEqual(SpellItemUseHandler.UseResult.AlreadyKnown, result);
            Assert.AreEqual(1, inv.AmountOf("scroll_dup"), "Nao desperdica scroll de magia ja conhecida.");
        }

        [Test]
        public void Handler_CastScroll_CastsAndConsumes_WithoutLearning()
        {
            // CA-2: CastScroll casta sem aprender.
            var inv = new FakeInventory();
            var item = MakeItem("scroll_cast_heal", SpellSourceType.CastScroll, taughtSpellId: "spell_heal");
            inv.Add(item, 1);
            var state = new SpellbookState();
            bool castInvoked = false;
            var handler = new SpellItemUseHandler(inv, state, castSpell: id => { castInvoked = id == "spell_heal"; return true; });

            var result = handler.UseItem("scroll_cast_heal");

            Assert.AreEqual(SpellItemUseHandler.UseResult.Cast, result);
            Assert.IsTrue(castInvoked, "Deve castar a spell do scroll.");
            Assert.IsFalse(state.IsKnown("spell_heal"), "CastScroll NAO aprende.");
            Assert.AreEqual(0, inv.AmountOf("scroll_cast_heal"), "CastScroll consumido.");
        }

        [Test]
        public void Handler_CastScroll_CastFails_DoesNotConsume()
        {
            var inv = new FakeInventory();
            var item = MakeItem("scroll_cast_fail", SpellSourceType.CastScroll, taughtSpellId: "spell_x");
            inv.Add(item, 1);
            var state = new SpellbookState();
            var handler = new SpellItemUseHandler(inv, state, castSpell: _ => false);

            var result = handler.UseItem("scroll_cast_fail");

            Assert.AreEqual(SpellItemUseHandler.UseResult.CastFailed, result);
            Assert.AreEqual(1, inv.AmountOf("scroll_cast_fail"), "Cast falho nao consome.");
        }

        [Test]
        public void Handler_Tome_LearnsOnThirdUse()
        {
            // CA-2: Tome aprende no 3º uso.
            var inv = new FakeInventory();
            var item = MakeItem("tome_ice", SpellSourceType.Tome, taughtSpellId: "spell_ice", tomeUses: 3);
            inv.Add(item, 5);
            var state = new SpellbookState();
            var handler = new SpellItemUseHandler(inv, state);

            Assert.AreEqual(SpellItemUseHandler.UseResult.TomeStudied, handler.UseItem("tome_ice"));
            Assert.AreEqual(SpellItemUseHandler.UseResult.TomeStudied, handler.UseItem("tome_ice"));
            Assert.AreEqual(SpellItemUseHandler.UseResult.Learned, handler.UseItem("tome_ice"));

            Assert.IsTrue(state.IsKnown("spell_ice"));
            Assert.AreEqual(2, inv.AmountOf("tome_ice"), "3 estudos consomem 3 copias.");
        }

        [Test]
        public void Handler_EquippedItemSource_IsNotUsable()
        {
            // Wand (EquippedItem) não é "usável" via handler de uso.
            var inv = new FakeInventory();
            var item = MakeItem("wand_spark", SpellSourceType.EquippedItem);
            inv.Add(item, 1);
            var handler = new SpellItemUseHandler(inv, new SpellbookState());

            Assert.AreEqual(SpellItemUseHandler.UseResult.NotASpellItem, handler.UseItem("wand_spark"));
        }

        [Test]
        public void Handler_ItemNotInInventory_ReturnsNotFound()
        {
            var handler = new SpellItemUseHandler(new FakeInventory(), new SpellbookState());
            Assert.AreEqual(SpellItemUseHandler.UseResult.NotFound, handler.UseItem("nonexistent"));
        }

        // ----------------------------------------------------------------- helpers

        private static int CountOf(IReadOnlyCollection<string> collection)
        {
            return collection == null ? 0 : collection.Count;
        }

        private static ItemDataSO MakeItem(
            string id,
            SpellSourceType source,
            string taughtSpellId = null,
            int tomeUses = 3,
            string requiredNode = null)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.DisplayName = id;
            item.Category = ItemCategory.Magic;
            item.SpellSource = source;
            item.TaughtSpellId = taughtSpellId ?? string.Empty;
            item.TomeUsesRequired = tomeUses;
            item.RequiredSkillNodeId = requiredNode ?? string.Empty;
            return item;
        }

        /// <summary>Fake do inventário (apenas o necessário para o handler).</summary>
        private sealed class FakeInventory : SpellItemUseHandler.IInventoryUse
        {
            private readonly Dictionary<string, int> _amounts = new Dictionary<string, int>();
            private readonly Dictionary<string, ItemDataSO> _data = new Dictionary<string, ItemDataSO>();

            public void Add(ItemDataSO item, int amount)
            {
                _data[item.Id] = item;
                _amounts.TryGetValue(item.Id, out int current);
                _amounts[item.Id] = current + amount;
            }

            public int AmountOf(string itemId) => _amounts.TryGetValue(itemId, out int n) ? n : 0;

            public bool HasItem(string itemId, int amount = 1) => AmountOf(itemId) >= amount;

            public bool TryGetItemData(string itemId, out ItemDataSO itemData) => _data.TryGetValue(itemId, out itemData);

            public bool RemoveItem(string itemId, int amount)
            {
                if (AmountOf(itemId) < amount)
                {
                    return false;
                }

                _amounts[itemId] -= amount;
                return true;
            }
        }
    }
}
