using CindarsHope.Combat.StatusEffect;
using DamageType = CindarsHope.Foundation.DamageType;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Core
{
    /// <summary>
    /// F01 — conjunto canônico de status (13 tipos + Hunger/Fatigue fora do ticker).
    /// Caracterização dos 4 existentes + semântica nova por tipo.
    /// </summary>
    public class StatusEffectCanonicalTests
    {
        private static StatusEffectSO Make(StatusEffectType type, int duration = 3, int dot = 2, float speedMult = 1f)
        {
            var so = ScriptableObject.CreateInstance<StatusEffectSO>();
            so.Id = $"status_{type.ToString().ToLowerInvariant()}";
            so.Type = type;
            so.DurationTurns = duration;
            so.DamagePerTurn = dot;
            so.MoveSpeedMultiplier = speedMult;
            return so;
        }

        // ------------------------------------------------ caracterização (4 existentes)

        [Test]
        public void Characterization_ApplyTickExpire_PreservedForLegacyStatuses()
        {
            var manager = new StatusEffectManager();
            var poison = Make(StatusEffectType.Poison, duration: 2, dot: 2);
            manager.ApplyStatusEffect(poison);

            Assert.IsTrue(manager.HasStatusEffect(poison.Id));
            Assert.AreEqual(2, manager.GetStatusDamageThisTurn(poison));

            manager.TickStatusEffects();
            Assert.IsTrue(manager.HasStatusEffect(poison.Id), "1 tick restante.");
            manager.TickStatusEffects();
            Assert.IsFalse(manager.HasStatusEffect(poison.Id), "Expirou após DurationTurns.");
        }

        // ------------------------------------------------ enum completo

        [Test]
        public void Enum_ContainsAllCanonicalTypes()
        {
            var required = new[]
            {
                StatusEffectType.Bleed, StatusEffectType.Burn, StatusEffectType.Chill,
                StatusEffectType.Poison, StatusEffectType.Stun, StatusEffectType.Root,
                StatusEffectType.Fear, StatusEffectType.ConfusionLite,
                StatusEffectType.DurabilityStress, StatusEffectType.Corruption,
                StatusEffectType.Slow, StatusEffectType.HeatStress, StatusEffectType.ColdStress
            };

            foreach (var type in required)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(StatusEffectType), type), type.ToString());
            }
        }

        // ------------------------------------------------ semântica por tipo

        [Test]
        public void Semantics_DamageOverTime_Set()
        {
            Assert.IsTrue(StatusEffectSemantics.IsDamageOverTime(StatusEffectType.Poison));
            Assert.IsTrue(StatusEffectSemantics.IsDamageOverTime(StatusEffectType.Burn));
            Assert.IsTrue(StatusEffectSemantics.IsDamageOverTime(StatusEffectType.Bleed));
            Assert.IsTrue(StatusEffectSemantics.IsDamageOverTime(StatusEffectType.Corruption));
            Assert.IsFalse(StatusEffectSemantics.IsDamageOverTime(StatusEffectType.Chill));
            Assert.IsFalse(StatusEffectSemantics.IsDamageOverTime(StatusEffectType.Fear));
        }

        [Test]
        public void Semantics_DamageTypes()
        {
            Assert.AreEqual(DamageType.Fire, StatusEffectSemantics.GetDamageType(StatusEffectType.Burn));
            Assert.AreEqual(DamageType.Toxic, StatusEffectSemantics.GetDamageType(StatusEffectType.Poison));
            Assert.AreEqual(DamageType.Toxic, StatusEffectSemantics.GetDamageType(StatusEffectType.Corruption));
            Assert.AreEqual(DamageType.Physical, StatusEffectSemantics.GetDamageType(StatusEffectType.Bleed));
            Assert.AreEqual(DamageType.Ice, StatusEffectSemantics.GetDamageType(StatusEffectType.ColdStress));
        }

        [Test]
        public void Semantics_MoveSpeed_RootAndStunZero_ChillUsesAsset()
        {
            Assert.AreEqual(0f, StatusEffectSemantics.GetMoveSpeedFactor(Make(StatusEffectType.Root)));
            Assert.AreEqual(0f, StatusEffectSemantics.GetMoveSpeedFactor(Make(StatusEffectType.Stun)));
            Assert.AreEqual(0.5f, StatusEffectSemantics.GetMoveSpeedFactor(Make(StatusEffectType.Chill, speedMult: 0.5f)), 0.001f);
            Assert.AreEqual(0.7f, StatusEffectSemantics.GetMoveSpeedFactor(Make(StatusEffectType.Slow, speedMult: 0.7f)), 0.001f);
            Assert.AreEqual(1f, StatusEffectSemantics.GetMoveSpeedFactor(Make(StatusEffectType.Burn)), "DoT não afeta velocidade.");
        }

        [Test]
        public void Semantics_BehaviorFlags()
        {
            Assert.IsTrue(StatusEffectSemantics.ForcesRetreat(StatusEffectType.Fear));
            Assert.IsFalse(StatusEffectSemantics.ForcesRetreat(StatusEffectType.Chill));
            Assert.IsTrue(StatusEffectSemantics.InvertsMovement(StatusEffectType.ConfusionLite));
            Assert.IsTrue(StatusEffectSemantics.BlocksPlayerAction(StatusEffectType.Stun));
            Assert.IsFalse(StatusEffectSemantics.BlocksPlayerAction(StatusEffectType.Slow));
        }

        [Test]
        public void NewFields_HaveNeutralDefaults()
        {
            var so = ScriptableObject.CreateInstance<StatusEffectSO>();
            Assert.AreEqual(1f, so.MoveSpeedMultiplier, "Default neutro — assets antigos intactos.");
            Assert.AreEqual(0f, so.BehaviorOverrideSeconds);
            Assert.AreEqual(1f, so.DurabilityWearMultiplier);
        }

        // ------------------------------------------------ stacking (política atual: refresh-by-add)

        [Test]
        public void Stacking_ReapplyAddsSecondInstance_LegacyPolicyCharacterized()
        {
            var manager = new StatusEffectManager();
            var bleed = Make(StatusEffectType.Bleed, duration: 2);
            manager.ApplyStatusEffect(bleed);
            manager.ApplyStatusEffect(bleed);

            // Política atual (caracterizada, não alterada): instâncias coexistem; dano por
            // turno reporta o valor do asset (não duplica via GetStatusDamageThisTurn).
            Assert.AreEqual(2, manager.GetActiveEffects().Count);
            Assert.AreEqual(bleed.DamagePerTurn, manager.GetStatusDamageThisTurn(bleed));
        }
    }
}
