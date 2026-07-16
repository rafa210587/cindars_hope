using NUnit.Framework;
using UnityEngine;
using CindarsHope.Combat;
using CindarsHope.Cave.Runtime;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// Regression tests para o bugfix de balance: inimigos da caverna tinham HP muito baixo
    /// porque CaveBandScaling.ScaleHp nao era chamado durante o spawn e nao havia multiplicador
    /// global de balance.
    ///
    /// Root cause: todos os spawners chamavam Configure(enemyData) diretamente, usando so o
    /// HP base do asset (minimo da banda), sem aplicar ScaleHp nem qualquer multiplicador.
    ///
    /// Fix: ConfigureWithScaling(enemyData, caveLevel, hpMult) aplica ScaleHp + multiplicador.
    /// MaxHp agora retorna o valor escalado quando disponivel.
    /// </summary>
    [TestFixture]
    public class EnemyHpScalingRegressionTests
    {
        private GameObject _go;
        private EnemyHealth _health;
        private EnemyDataSO _enemyData;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestEnemy");
            _health = _go.AddComponent<EnemyHealth>();
            _enemyData = ScriptableObject.CreateInstance<EnemyDataSO>();
            _enemyData.maxHp = 16; // Goblin Scrounger base HP (band 1, lv 2)
            _enemyData.enemyLevel = 2;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_enemyData != null) Object.DestroyImmediate(_enemyData);
        }

        // arch: quebra do par mutuo Cave|Combat — ConfigureWithScaling agora recebe scaledBaseHp/
        // bandMinLevel PRE-COMPUTADOS pelo caller (spawners de Cave), em vez de calcular
        // CaveBandScaling internamente. Este helper replica a mesma formula do caller de producao
        // para manter os testes de regressao equivalentes byte-for-byte.
        private void ConfigureWithScalingHelper(EnemyDataSO enemyData, int caveLevel, float hpBaseMultiplier)
        {
            var bandMinLevel = CaveBandScaling.BandMinLevel(
                CaveBandScaling.BandForLevel(caveLevel > 0 ? caveLevel : enemyData.enemyLevel));
            var scaledBaseHp = caveLevel > 0
                ? CaveBandScaling.ScaleHp(enemyData.maxHp, caveLevel, bandMinLevel)
                : enemyData.maxHp;
            _health.ConfigureWithScaling(enemyData, caveLevel, hpBaseMultiplier, scaledBaseHp, bandMinLevel);
        }

        // --- Regression: Configure legado nao muda comportamento ---

        [Test]
        public void Configure_Legacy_MaxHp_Equals_AssetValue()
        {
            _health.Configure(_enemyData);
            Assert.AreEqual(16, _health.MaxHp,
                "Configure legado deve retornar HP do asset sem scaling.");
        }

        [Test]
        public void Configure_Legacy_CurrentHp_Equals_AssetValue()
        {
            _health.Configure(_enemyData);
            Assert.AreEqual(16, _health.CurrentHp,
                "Configure legado deve inicializar CurrentHp com HP do asset.");
        }

        // --- Regression: multiplicador base eleva o HP ---

        [Test]
        public void ConfigureWithScaling_HpMultiplier_ElevaMaxHp()
        {
            ConfigureWithScalingHelper(_enemyData, caveLevel: 2, hpBaseMultiplier: 3.5f);
            // Base 16, caveLevel=2, bandMin=1 (band Stone), steps=1, scaledHp = round(16 * 1.12) = 18.
            // multipliedHp = round(18 * 3.5) = 63
            Assert.Greater(_health.MaxHp, _enemyData.maxHp,
                "ConfigureWithScaling deve retornar MaxHp maior que o valor base do asset.");
        }

        [Test]
        public void ConfigureWithScaling_CurrentHp_EqualsMaxHp_AfterConfigure()
        {
            ConfigureWithScalingHelper(_enemyData, caveLevel: 2, hpBaseMultiplier: 3.5f);
            Assert.AreEqual(_health.MaxHp, _health.CurrentHp,
                "Apos ConfigureWithScaling, CurrentHp deve igualar MaxHp (HP cheio no spawn).");
        }

        // --- Regression: ScaleHp e aplicado pelo caveLevel ---

        [Test]
        public void ConfigureWithScaling_CaveLevel1_NoScaling_OnlyMultiplier()
        {
            // caveLevel=1, bandMin=1, steps=0: ScaleHp retorna baseHp sem crescimento.
            _enemyData.maxHp = 8; // Verdant Mite
            _enemyData.enemyLevel = 1;
            ConfigureWithScalingHelper(_enemyData, caveLevel: 1, hpBaseMultiplier: 3.5f);
            int expected = Mathf.Max(1, Mathf.RoundToInt(8 * 3.5f)); // 28
            Assert.AreEqual(expected, _health.MaxHp,
                "No nivel minimo da banda (steps=0) so o multiplicador deve ser aplicado.");
        }

        [Test]
        public void ConfigureWithScaling_MultiplierOne_OnlyScaleHpApplied()
        {
            // hpMult=1 -> apenas ScaleHp (sem multiplicador extra).
            // base 16, caveLevel=5, bandMin=1, steps=4, ScaleHp = round(16 * 1.12^4) = round(25.18) = 25
            ConfigureWithScalingHelper(_enemyData, caveLevel: 5, hpBaseMultiplier: 1f);
            int expectedScaled = CaveBandScaling.ScaleHp(16, 5, 1); // 25
            Assert.AreEqual(expectedScaled, _health.MaxHp,
                "Com hpMult=1.0 apenas ScaleHp deve ser aplicado.");
        }

        // --- Regression: objetivo de balance -- common Stone sobrevive 3+ hits ---

        [Test]
        public void ConfigureWithScaling_CommonStone_SurvivesThreeBasicAttacks()
        {
            // Ataque basico do player: espada ferro BaseDamage=15, scaling ~8, total ~23. Defense=1 -> efetivo=22.
            // Regra: common Stone sobrevive 3-5 hits de ataque basico.
            _enemyData.maxHp = 16; // Goblin Scrounger base
            _enemyData.enemyLevel = 2;
            ConfigureWithScalingHelper(_enemyData, caveLevel: 2, hpBaseMultiplier: 3.5f);

            const int basicAttackEffectiveDamage = 22; // baseDamage(15) + scaling(8) - defense(1)
            int hitsToKill = Mathf.CeilToInt(_health.MaxHp / (float)basicAttackEffectiveDamage);
            Assert.GreaterOrEqual(hitsToKill, 3,
                $"Common Stone (caveLevel=2, hpMult=3.5) deve sobreviver ao menos 3 hits de ataque basico. " +
                $"MaxHp={_health.MaxHp}, efetivo/hit={basicAttackEffectiveDamage}, hits para matar={hitsToKill}.");
        }

        // --- Regression: RestoreHp clampeia pelo MaxHp escalado, nao pelo asset ---

        [Test]
        public void RestoreHp_UsesScaledMaxHp_NotAssetValue()
        {
            ConfigureWithScalingHelper(_enemyData, caveLevel: 2, hpBaseMultiplier: 3.5f);
            int scaledMax = _health.MaxHp;

            // Tenta restaurar um HP maior que o asset original mas <= scaledMax
            int savedHp = scaledMax - 5;
            _health.RestoreHp(savedHp);

            Assert.AreEqual(savedHp, _health.CurrentHp,
                "RestoreHp deve clampar pelo MaxHp escalado, nao pelo asset.maxHp.");
        }
    }
}
