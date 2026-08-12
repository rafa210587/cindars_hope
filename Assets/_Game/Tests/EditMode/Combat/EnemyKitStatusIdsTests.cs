using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core.Data;
using CindarsHope.EditorTools.Combat;
using NUnit.Framework;
using UnityEditor;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// spec_content_enemy_status_kit_ids_v1 — os 12 StatusEffect IDs referenciados por 43
    /// EnemyActionSO (kits "minor"/buff) devem resolver no StatusEffectDatabaseSO. Um assert
    /// dedicado por ID, para que uma falha aponte exatamente qual ID quebrou.
    /// </summary>
    public class EnemyKitStatusIdsTests
    {
        private StatusEffectDatabaseSO _database;

        [SetUp]
        public void SetUp()
        {
            var guids = AssetDatabase.FindAssets("t:StatusEffectDatabaseSO");
            Assert.IsNotEmpty(guids, "StatusEffectDatabaseSO asset nao encontrado — rode CindarsHope/Inicializar Projeto.");

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _database = AssetDatabase.LoadAssetAtPath<StatusEffectDatabaseSO>(path);
            Assert.IsNotNull(_database, $"Falha ao carregar StatusEffectDatabaseSO em '{path}'.");
        }

        private void AssertResolves(string id)
        {
            bool found = _database.TryGetById(id, out var effect);
            Assert.IsTrue(found, $"StatusEffect Id '{id}' nao resolve no StatusEffectDatabaseSO (rode CindarsHope/Inicializar Projeto).");
            Assert.IsNotNull(effect, $"StatusEffect Id '{id}' resolveu para entrada nula.");
            Assert.AreEqual(id, effect.Id, $"Entrada resolvida para '{id}' tem Id divergente ('{effect.Id}').");
        }

        [Test]
        public void StatusSlowMinor_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusSlowMinor);

        [Test]
        public void StatusBurnMinor_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusBurnMinor);

        [Test]
        public void StatusChillMinor_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusChillMinor);

        [Test]
        public void StatusPoisonMinor_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusPoisonMinor);

        [Test]
        public void StatusBleedMinor_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusBleedMinor);

        [Test]
        public void StatusConfuseMinor_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusConfuseMinor);

        [Test]
        public void StatusRootMinor_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusRootMinor);

        [Test]
        public void StatusHaste_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusHaste);

        [Test]
        public void StatusShield_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusShield);

        [Test]
        public void StatusFrenzy_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusFrenzy);

        [Test]
        public void StatusRegen_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusRegen);

        [Test]
        public void StatusGuard_Resolves() => AssertResolves(GenerateCanonicalStatusEffects.StatusGuard);
    }
}
