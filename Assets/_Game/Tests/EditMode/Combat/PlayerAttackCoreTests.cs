using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// Testes determinísticos para PlayerAttackCore — dodge state machine, cooldown por slot
    /// e dispatch de AttackPath. Sem MonoBehaviour, sem Physics2D, sem Time.time real.
    /// </summary>
    public class PlayerAttackCoreTests
    {
        // ---- Helpers ----

        private static ItemDataSO MakeItem(ItemCategory category, string weaponId = null, string spellId = null)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Category = category;
            if (weaponId != null) item.WeaponId = weaponId;
            if (spellId != null) item.SpellId = spellId;
            return item;
        }

        private static WeaponDataSO MakeWeapon(WeaponType type)
        {
            var w = ScriptableObject.CreateInstance<WeaponDataSO>();
            w.Type = type;
            return w;
        }

        // ----------------------------------------------------------------
        // Dodge cooldown
        // ----------------------------------------------------------------

        [Test]
        public void CanDodge_ReturnsFalse_BeforeCooldownExpires()
        {
            var core = new PlayerAttackCore();
            float now = 10f;
            core.BeginDodge(now, durationSeconds: 0.2f);
            // Simulate UpdateDodge so IsDodging clears
            core.UpdateDodge(now + 1f);

            // Try again 0.1 s later — cooldown = 0.5 s, should still be locked
            bool canDodge = core.CanDodge(now + 0.1f, cooldownSeconds: 0.5f, hasEnoughStamina: true);
            Assert.IsFalse(canDodge, "Dodge should be blocked before cooldown expires.");
        }

        [Test]
        public void CanDodge_ReturnsTrue_AfterCooldownExpires()
        {
            var core = new PlayerAttackCore();
            float now = 10f;
            core.BeginDodge(now, durationSeconds: 0.2f);
            core.UpdateDodge(now + 1f); // clear isDodging

            bool canDodge = core.CanDodge(now + 0.6f, cooldownSeconds: 0.5f, hasEnoughStamina: true);
            Assert.IsTrue(canDodge, "Dodge should be allowed after cooldown expires.");
        }

        [Test]
        public void CanDodge_ReturnsFalse_WhenNoStamina()
        {
            var core = new PlayerAttackCore();
            bool canDodge = core.CanDodge(currentTime: 100f, cooldownSeconds: 0f, hasEnoughStamina: false);
            Assert.IsFalse(canDodge, "CanDodge must return false when stamina is insufficient.");
        }

        // ----------------------------------------------------------------
        // Dodge state
        // ----------------------------------------------------------------

        [Test]
        public void IsDodging_IsTrue_AfterBeginDodge()
        {
            var core = new PlayerAttackCore();
            core.BeginDodge(currentTime: 5f, durationSeconds: 0.3f);
            Assert.IsTrue(core.IsDodging, "IsDodging must be true immediately after BeginDodge.");
        }

        [Test]
        public void UpdateDodge_ReturnsTrue_WhenDodgeEnds_AndIsDodgingBecomesFalse()
        {
            var core = new PlayerAttackCore();
            float start = 5f;
            float duration = 0.2f;
            core.BeginDodge(start, duration);

            // Still within dodge window
            bool ended = core.UpdateDodge(start + 0.1f);
            Assert.IsFalse(ended, "UpdateDodge should return false while still dodging.");
            Assert.IsTrue(core.IsDodging, "IsDodging should still be true mid-dodge.");

            // Past dodge window
            ended = core.UpdateDodge(start + duration + 0.01f);
            Assert.IsTrue(ended, "UpdateDodge must return true when dodge window closes.");
            Assert.IsFalse(core.IsDodging, "IsDodging must be false after dodge ends.");
        }

        // ----------------------------------------------------------------
        // Attack cooldown per slot
        // ----------------------------------------------------------------

        [Test]
        public void CanAttackSlot_ReturnsFalse_BeforeCooldownExpires()
        {
            var core = new PlayerAttackCore();
            float now = 10f;
            core.RecordAttack(EquipmentSlot.LeftHand, now);

            bool canAttack = core.CanAttackSlot(EquipmentSlot.LeftHand, now + 0.3f, cooldownSeconds: 0.5f);
            Assert.IsFalse(canAttack, "Left hand attack should be blocked before cooldown expires.");
        }

        [Test]
        public void CanAttackSlot_ReturnsTrue_AfterCooldownExpires()
        {
            var core = new PlayerAttackCore();
            float now = 10f;
            core.RecordAttack(EquipmentSlot.LeftHand, now);

            bool canAttack = core.CanAttackSlot(EquipmentSlot.LeftHand, now + 0.6f, cooldownSeconds: 0.5f);
            Assert.IsTrue(canAttack, "Left hand attack should be allowed after cooldown expires.");
        }

        [Test]
        public void CanAttackSlot_SlotsAreIndependent()
        {
            var core = new PlayerAttackCore();
            float now = 10f;
            core.RecordAttack(EquipmentSlot.LeftHand, now);

            // Right hand was never used — cooldown is expired from float.MinValue
            bool rightCanAttack = core.CanAttackSlot(EquipmentSlot.RightHand, now, cooldownSeconds: 0.5f);
            Assert.IsTrue(rightCanAttack, "Right hand cooldown should be independent of left hand.");
        }

        // ----------------------------------------------------------------
        // ResolveAttackPath dispatch
        // ----------------------------------------------------------------

        [Test]
        public void ResolveAttackPath_Ammo_ReturnsBow()
        {
            var core = new PlayerAttackCore();
            var item = MakeItem(ItemCategory.Ammo);
            var path = core.ResolveAttackPath(item, bowCheck: null, wandCheck: null);
            Assert.AreEqual(AttackPath.Bow, path);
        }

        [Test]
        public void ResolveAttackPath_Magic_ReturnsSpell()
        {
            var core = new PlayerAttackCore();
            var item = MakeItem(ItemCategory.Magic);
            var path = core.ResolveAttackPath(item, bowCheck: null, wandCheck: null);
            Assert.AreEqual(AttackPath.Spell, path);
        }

        [Test]
        public void ResolveAttackPath_WeaponWithSpellIdAndWand_ReturnsWandSpell()
        {
            var core = new PlayerAttackCore();
            var item = MakeItem(ItemCategory.Weapon, weaponId: "weapon_wand_fire", spellId: "spell_fireball");
            var wand = MakeWeapon(WeaponType.Wand);
            var path = core.ResolveAttackPath(item, bowCheck: null, wandCheck: wand);
            Assert.AreEqual(AttackPath.WandSpell, path);
        }

        [Test]
        public void ResolveAttackPath_WeaponWithBow_ReturnsBlockedBowHand()
        {
            var core = new PlayerAttackCore();
            var item = MakeItem(ItemCategory.Weapon, weaponId: "weapon_bow_short");
            var bow = MakeWeapon(WeaponType.Bow);
            var path = core.ResolveAttackPath(item, bowCheck: bow, wandCheck: null);
            Assert.AreEqual(AttackPath.BlockedBowHand, path);
        }

        [Test]
        public void ResolveAttackPath_WeaponNormal_ReturnsMelee()
        {
            var core = new PlayerAttackCore();
            var item = MakeItem(ItemCategory.Weapon, weaponId: "weapon_sword_iron");
            var sword = MakeWeapon(WeaponType.Sword);
            // bowCheck is not a bow, wandCheck is null
            var path = core.ResolveAttackPath(item, bowCheck: sword, wandCheck: null);
            Assert.AreEqual(AttackPath.Melee, path);
        }

        [Test]
        public void ResolveAttackPath_NullItem_ReturnsMelee()
        {
            var core = new PlayerAttackCore();
            var path = core.ResolveAttackPath(itemData: null, bowCheck: null, wandCheck: null);
            Assert.AreEqual(AttackPath.Melee, path, "Null item (empty slot) should dispatch as Melee (unarmed).");
        }

        [Test]
        public void ResolveAttackPath_NonWeaponCategory_ReturnsBlockedNonWeapon()
        {
            var core = new PlayerAttackCore();
            var item = MakeItem(ItemCategory.Tool);
            var path = core.ResolveAttackPath(item, bowCheck: null, wandCheck: null);
            Assert.AreEqual(AttackPath.BlockedNonWeapon, path, "Non-weapon category should be blocked.");
        }
    }
}
