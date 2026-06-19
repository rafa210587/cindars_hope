using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// fable_48 — munição de arco (parte runtime): dano da flecha (§18), tags + matching de
    /// vulnerabilidade (F06), status elemental on-hit (F01) e seleção determinística de munição.
    /// Tudo PURO/determinístico (sem Play Mode): testa o ArrowBallisticsResolver, o ArrowAmmoSelector
    /// e o VulnerabilityMatcher (F06) com alvo sintético. A presença dos 6 itens no catálogo (CA-5)
    /// é coberta por BowAmmoCatalogTests (assembly de Editor, onde vive a tabela do catálogo F32).
    /// </summary>
    public class BowAmmoElementalArrowsTests
    {
        // ───────────────────────────── CA-1: fórmula §18 com ArrowDamage ─────────────────────────

        [Test]
        public void ArrowDamage_IronIsTwoMoreThanWood()
        {
            var wood = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowWoodId);
            var iron = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowIronId);

            Assert.AreEqual(2, wood.ArrowDamage, "Wood = +2 (§19).");
            Assert.AreEqual(4, iron.ArrowDamage, "Iron = +4 (§19).");
            Assert.AreEqual(2, iron.ArrowDamage - wood.ArrowDamage, "CA-1: iron causa exatamente +2 sobre wood.");
        }

        [Test]
        public void ArrowDamage_CanonicalTableMatchesSpec()
        {
            // §19 × §8: wood +2 | iron +4 | steel +6 | silver +4 | fire +3 | frost +3.
            Assert.AreEqual(2, ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowWoodId).ArrowDamage);
            Assert.AreEqual(4, ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowIronId).ArrowDamage);
            Assert.AreEqual(6, ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowSteelId).ArrowDamage);
            Assert.AreEqual(4, ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowSilverId).ArrowDamage);
            Assert.AreEqual(3, ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowFireId).ArrowDamage);
            Assert.AreEqual(3, ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowFrostId).ArrowDamage);
        }

        [Test]
        public void ArrowDamage_SumEntersBeforeDerivedMultipliers()
        {
            // CA-1: a soma (bowWeapon + bowBonus + arrowDamage) é o input ANTES do derive. Modelamos
            // o derive como um multiplicador linear (proxy do FinalDamage do StatsProvider) e provamos
            // que somar o dano da flecha antes do multiplicador escala corretamente.
            const int bowWeaponDamage = 10;
            const int bowBonus = 2;
            const float deriveMultiplier = 1.5f; // proxy de um multiplicador derivado qualquer.

            int Derive(int preDerive) => Mathf.RoundToInt(preDerive * deriveMultiplier);

            var woodDamage = Derive(bowWeaponDamage + bowBonus + ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowWoodId).ArrowDamage);
            var ironDamage = Derive(bowWeaponDamage + bowBonus + ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowIronId).ArrowDamage);

            // (12+2)*1.5=21 vs (12+4)*1.5=24 — a diferença de +2 de flecha vira +3 após o derive.
            Assert.AreEqual(21, woodDamage);
            Assert.AreEqual(24, ironDamage);
            Assert.Greater(ironDamage, woodDamage, "Munição melhor => mais dano final (a soma entra antes do derive).");
        }

        // ───────────────────────── CA-2: tags + matching de vulnerabilidade (F06) ────────────────

        [Test]
        public void Tags_SilverArrowCarriesSilverTag()
        {
            var silver = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowSilverId);
            CollectionAssert.Contains(silver.Tags, ArrowBallisticsResolver.TagSilver, "Silver carrega a tag Silver.");
            CollectionAssert.Contains(silver.Tags, ArrowBallisticsResolver.TagPierce, "Toda flecha carrega Pierce (§19).");
        }

        [Test]
        public void Tags_PhysicalArrowsNeverCarryElementalTag()
        {
            foreach (var id in new[] { ArrowBallisticsResolver.ArrowWoodId, ArrowBallisticsResolver.ArrowIronId, ArrowBallisticsResolver.ArrowSteelId })
            {
                var b = ArrowBallisticsResolver.Resolve(id);
                CollectionAssert.DoesNotContain(b.Tags, ArrowBallisticsResolver.TagFire, $"{id} não tem Fire.");
                CollectionAssert.DoesNotContain(b.Tags, ArrowBallisticsResolver.TagIce, $"{id} não tem Ice.");
                CollectionAssert.DoesNotContain(b.Tags, ArrowBallisticsResolver.TagSilver, $"{id} não tem Silver.");
            }
        }

        [Test]
        public void Matching_SilverBonusOnlyAgainstDeclaredVulnerability()
        {
            // Alvo sintético COM vulnerabilidade Silver declarada (regra do adapter F06).
            var vulnerableProfile = ScriptableObject.CreateInstance<EnemyVulnerabilityProfileSO>();
            vulnerableProfile.MaterialMultipliers = new[]
            {
                new MaterialMultiplier { MaterialTag = ArrowBallisticsResolver.TagSilver, Multiplier = 2f }
            };

            var silverTags = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowSilverId).Tags;

            float vulnMult = VulnerabilityMatcher.GetDamageMultiplier(vulnerableProfile, DamageType.Physical, silverTags);
            Assert.AreEqual(2f, vulnMult, 0.001f, "CA-2: contra alvo que declara Silver, há bônus.");

            // Alvo sintético SEM a vulnerabilidade => NENHUM bônus (1.0), nunca bônus genérico.
            var neutralProfile = ScriptableObject.CreateInstance<EnemyVulnerabilityProfileSO>();
            float neutralMult = VulnerabilityMatcher.GetDamageMultiplier(neutralProfile, DamageType.Physical, silverTags);
            Assert.AreEqual(1f, neutralMult, 0.001f, "CA-2: sem vulnerabilidade declarada, sem bônus algum.");

            // Sem perfil nenhum também é neutro.
            float noProfileMult = VulnerabilityMatcher.GetDamageMultiplier(null, DamageType.Physical, silverTags);
            Assert.AreEqual(1f, noProfileMult, 0.001f, "Perfil nulo = neutro.");

            Object.DestroyImmediate(vulnerableProfile);
            Object.DestroyImmediate(neutralProfile);
        }

        [Test]
        public void Matching_WoodArrowGetsNoBonusEvenAgainstSilverWeakEnemy()
        {
            var silverWeak = ScriptableObject.CreateInstance<EnemyVulnerabilityProfileSO>();
            silverWeak.MaterialMultipliers = new[]
            {
                new MaterialMultiplier { MaterialTag = ArrowBallisticsResolver.TagSilver, Multiplier = 2f }
            };

            var woodTags = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowWoodId).Tags;
            float mult = VulnerabilityMatcher.GetDamageMultiplier(silverWeak, DamageType.Physical, woodTags);
            Assert.AreEqual(1f, mult, 0.001f, "Flecha de madeira (sem tag Silver) não ganha bônus anti-silver.");

            Object.DestroyImmediate(silverWeak);
        }

        [Test]
        public void Matching_FireArrowElementBonusOnlyAgainstFireVulnerability()
        {
            var fireWeak = ScriptableObject.CreateInstance<EnemyVulnerabilityProfileSO>();
            fireWeak.ElementMultipliers = new[]
            {
                new ElementMultiplier { DamageType = DamageType.Fire, Multiplier = 1.5f }
            };

            var fire = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowFireId);
            Assert.AreEqual(DamageType.Fire, fire.DamageType, "Fire arrow é dano Fire.");

            float weakMult = VulnerabilityMatcher.GetDamageMultiplier(fireWeak, fire.DamageType, fire.Tags);
            Assert.AreEqual(1.5f, weakMult, 0.001f, "Contra alvo fraco a Fire, bônus elemental.");

            var neutral = ScriptableObject.CreateInstance<EnemyVulnerabilityProfileSO>();
            float neutralMult = VulnerabilityMatcher.GetDamageMultiplier(neutral, fire.DamageType, fire.Tags);
            Assert.AreEqual(1f, neutralMult, 0.001f, "Sem vulnerabilidade a Fire, sem bônus.");

            Object.DestroyImmediate(fireWeak);
            Object.DestroyImmediate(neutral);
        }

        // ───────────────────────────── CA-3: status elemental on-hit ─────────────────────────────

        [Test]
        public void Status_FireArrowMapsToBurnFrostToChill()
        {
            var fire = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowFireId);
            var frost = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowFrostId);

            Assert.AreEqual(ArrowBallisticsResolver.StatusBurnId, fire.StatusEffectId, "CA-3: fire→Burn.");
            Assert.AreEqual(ArrowBallisticsResolver.StatusChillId, frost.StatusEffectId, "CA-3: frost→Chill.");
            Assert.IsTrue(fire.HasStatus && frost.HasStatus, "Flechas elementais aplicam status.");
        }

        [Test]
        public void Status_PhysicalArrowsNeverApplyStatus()
        {
            foreach (var id in new[] { ArrowBallisticsResolver.ArrowWoodId, ArrowBallisticsResolver.ArrowIronId, ArrowBallisticsResolver.ArrowSteelId, ArrowBallisticsResolver.ArrowSilverId })
            {
                var b = ArrowBallisticsResolver.Resolve(id);
                Assert.IsFalse(b.HasStatus, $"CA-3: {id} (física) nunca aplica status.");
                Assert.IsNull(b.StatusEffectId, $"{id} sem statusEffectId.");
                Assert.AreEqual(0f, b.StatusChance, $"{id} chance 0.");
            }
        }

        [Test]
        public void Status_ElementalChanceIsTheSingleCanonicalLowChance()
        {
            var fire = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowFireId);
            var frost = ArrowBallisticsResolver.Resolve(ArrowBallisticsResolver.ArrowFrostId);

            // Ponto único: fire e frost compartilham a MESMA chance canônica baixa.
            Assert.AreEqual(ArrowBallisticsResolver.ElementalStatusChance, fire.StatusChance, 0.0001f);
            Assert.AreEqual(ArrowBallisticsResolver.ElementalStatusChance, frost.StatusChance, 0.0001f);
            Assert.AreEqual(fire.StatusChance, frost.StatusChance, 0.0001f, "Mesma chance (ponto único).");
            Assert.Less(ArrowBallisticsResolver.ElementalStatusChance, 0.5f, "§19: chance BAIXA.");
            Assert.Greater(ArrowBallisticsResolver.ElementalStatusChance, 0f, "Mas > 0.");
        }

        // ────────────────────────── CA-4: seleção determinística de munição ──────────────────────

        [Test]
        public void AutoSelect_PicksNextCompatibleInCanonicalOrder()
        {
            // Esgotou wood; inventário tem iron e fire. Ordem canônica => iron antes de fire.
            var inventory = new List<AmmoCandidate>
            {
                new AmmoCandidate(ArrowBallisticsResolver.ArrowFireId, "arrow", 10),
                new AmmoCandidate(ArrowBallisticsResolver.ArrowIronId, "arrow", 5),
            };

            var next = ArrowAmmoSelector.SelectNextCompatible(ArrowBallisticsResolver.ArrowWoodId, "arrow", inventory);
            Assert.AreEqual(ArrowBallisticsResolver.ArrowIronId, next, "CA-4: iron vem antes de fire (ordem canônica barata→cara).");
        }

        [Test]
        public void AutoSelect_SkipsDepletedAndZeroStock()
        {
            var inventory = new List<AmmoCandidate>
            {
                new AmmoCandidate(ArrowBallisticsResolver.ArrowWoodId, "arrow", 0),   // estoque zero, ignora
                new AmmoCandidate(ArrowBallisticsResolver.ArrowSilverId, "arrow", 3),
            };

            var next = ArrowAmmoSelector.SelectNextCompatible(ArrowBallisticsResolver.ArrowWoodId, "arrow", inventory);
            Assert.AreEqual(ArrowBallisticsResolver.ArrowSilverId, next, "Pula a recém-esgotada e estoques zero; pega silver.");
        }

        [Test]
        public void AutoSelect_RespectsAmmoTypeCompatibility()
        {
            var inventory = new List<AmmoCandidate>
            {
                new AmmoCandidate("item_ammo_bolt_iron", "bolt", 50),  // tipo incompatível
                new AmmoCandidate(ArrowBallisticsResolver.ArrowSteelId, "arrow", 8),
            };

            var next = ArrowAmmoSelector.SelectNextCompatible(ArrowBallisticsResolver.ArrowIronId, "arrow", inventory);
            Assert.AreEqual(ArrowBallisticsResolver.ArrowSteelId, next, "Só munição AmmoType=arrow é elegível.");
        }

        [Test]
        public void AutoSelect_ReturnsNullWhenNoCompatibleAmmo()
        {
            // CA-4 + anti-regressão: sem munição nenhuma compatível => null (mantém NoArrowsInInventory).
            Assert.IsNull(ArrowAmmoSelector.SelectNextCompatible(ArrowBallisticsResolver.ArrowWoodId, "arrow", new List<AmmoCandidate>()));

            var onlyDepleted = new List<AmmoCandidate>
            {
                new AmmoCandidate(ArrowBallisticsResolver.ArrowWoodId, "arrow", 4), // só a própria (excluída)
            };
            Assert.IsNull(
                ArrowAmmoSelector.SelectNextCompatible(ArrowBallisticsResolver.ArrowWoodId, "arrow", onlyDepleted),
                "Se só resta a recém-esgotada, não há próxima.");
        }

        [Test]
        public void AutoSelect_IsDeterministic()
        {
            var inventory = new List<AmmoCandidate>
            {
                new AmmoCandidate(ArrowBallisticsResolver.ArrowFrostId, "arrow", 2),
                new AmmoCandidate(ArrowBallisticsResolver.ArrowFireId, "arrow", 2),
                new AmmoCandidate(ArrowBallisticsResolver.ArrowSteelId, "arrow", 2),
            };

            var first = ArrowAmmoSelector.SelectNextCompatible(ArrowBallisticsResolver.ArrowWoodId, "arrow", inventory);
            var second = ArrowAmmoSelector.SelectNextCompatible(ArrowBallisticsResolver.ArrowWoodId, "arrow", inventory);
            Assert.AreEqual(first, second, "Mesma entrada => mesma saída (sem Random).");
            Assert.AreEqual(ArrowBallisticsResolver.ArrowSteelId, first, "steel < fire < frost na ordem canônica.");
        }

        // ───────────────────────── CA-5 (resolver side): roster fecha em 6 ───────────────────────

        [Test]
        public void Roster_ResolverCoversExactlySixCanonicalArrows()
        {
            // Lado runtime do CA-5: o resolver cobre exatamente os 6 ids canônicos, sem repetição.
            // O casamento contra os assets/catálogo (BV/AmmoType) vive em BowAmmoCatalogTests (Editor).
            Assert.AreEqual(6, ArrowBallisticsResolver.CanonicalOrder.Count, "Catálogo v1 fecha em 6 flechas.");
            Assert.AreEqual(6, ArrowBallisticsResolver.CanonicalOrder.Distinct().Count(), "Sem ids repetidos.");
            foreach (var id in ArrowBallisticsResolver.CanonicalOrder)
            {
                Assert.IsTrue(ArrowBallisticsResolver.IsCanonicalArrow(id), $"{id} resolve como canônica.");
            }
        }

        // ───────────────────────────── fallback determinístico ───────────────────────────────────

        [Test]
        public void Fallback_UnknownAmmoBehavesAsWoodenArrow()
        {
            // Unknown id => WoodenArrow fallback (resolver loga um warning, não verificado aqui para
            // não exigir UnityEngine.TestTools no assembly de runtime; o comportamento é o contrato).
            var unknown = ArrowBallisticsResolver.Resolve("item_ammo_arrow_does_not_exist");

            Assert.IsFalse(unknown.IsKnown, "Flecha desconhecida marcada como fallback.");
            Assert.AreEqual(2, unknown.ArrowDamage, "Fallback usa stats de WoodenArrow (+2).");
            Assert.AreEqual(DamageType.Physical, unknown.DamageType, "Fallback é físico.");
            Assert.IsFalse(unknown.HasStatus, "Fallback não aplica status.");
        }

        [Test]
        public void Fallback_KnownArrowsAreMarkedKnown()
        {
            foreach (var id in ArrowBallisticsResolver.CanonicalOrder)
            {
                Assert.IsTrue(ArrowBallisticsResolver.Resolve(id).IsKnown, $"{id} é canônica (IsKnown).");
                Assert.IsTrue(ArrowBallisticsResolver.IsCanonicalArrow(id), $"{id} reconhecida.");
            }
            Assert.IsFalse(ArrowBallisticsResolver.IsCanonicalArrow("nope"));
        }
    }
}
