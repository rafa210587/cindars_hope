using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Events;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// fable_84 — Cobre todos os 10 valores de WeaponType contra o mapa canonico
    /// definido em WeaponAttackArchetypeMapper. Lógica pura, sem Play Mode.
    /// </summary>
    public class WeaponAttackArchetypeMapperTests
    {
        [Test]
        public void Sword_ReturnsSword()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Sword,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Sword));
        }

        [Test]
        public void Bow_ReturnsBow()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Bow,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Bow));
        }

        [Test]
        public void Axe_ReturnsHeavy()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Heavy,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Axe));
        }

        [Test]
        public void Hammer_ReturnsHeavy()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Heavy,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Hammer));
        }

        [Test]
        public void Spear_ReturnsThrust()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Thrust,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Spear));
        }

        [Test]
        public void Dagger_ReturnsDagger()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Dagger,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Dagger));
        }

        [Test]
        public void Staff_ReturnsCast()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Cast,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Staff));
        }

        [Test]
        public void Wand_ReturnsCast()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Cast,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Wand));
        }

        [Test]
        public void Tool_ReturnsSword_Fallback()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Sword,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Tool));
        }

        [Test]
        public void None_ReturnsSword_Fallback()
        {
            Assert.AreEqual(PlayerAttackAnimArchetype.Sword,
                WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.None));
        }
    }
}
