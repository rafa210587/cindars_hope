using System.Collections;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class SkillMeleeShapesPlayModeTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private EquipmentManager _equipment;
        private string _originalLeftHand;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null)
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
            _equipment = EquipmentManager.Instance;
            Assert.That(_equipment, Is.Not.Null);
            _originalLeftHand = _equipment.GetEquippedItem(EquipmentSlot.LeftHand);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var dagger = _equipment != null ? _equipment.GetItemDurability("item_weapon_dagger_copper") : null;
            if (dagger != null && dagger.IsBroken) _equipment.RepairItem("item_weapon_dagger_copper", dagger.MaxDurability);
            if (_equipment != null)
            {
                if (string.IsNullOrEmpty(_originalLeftHand)) _equipment.UnequipSlot(EquipmentSlot.LeftHand);
                else _equipment.EquipItem(EquipmentSlot.LeftHand, _originalLeftHand);
            }
            foreach (var item in _owned) if (item != null) Object.Destroy(item);
            _owned.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator OffhandCut_NoWeapon_RejectsWithoutSpendingStamina()
        {
            _equipment.UnequipSlot(EquipmentSlot.LeftHand);
            var stamina = Stamina();
            stamina.Initialize(100, 100);

            var result = ExecuteOffhand(Body("No offhand caster", new Vector2(2600, 2600)), 1);

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(MeleeEquipmentGate.RequiresOffhandFailureKey));
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            yield return null;
        }

        [UnityTest]
        public IEnumerator EquipDagger_InitializesDurability_AndWhiffCommits()
        {
            _equipment.EquipItem(EquipmentSlot.LeftHand, "item_weapon_dagger_copper");
            var durability = _equipment.GetItemDurability("item_weapon_dagger_copper");
            Assert.That(durability, Is.Not.Null);
            Assert.That(durability.CurrentDurability, Is.EqualTo(durability.MaxDurability));
            Assert.That(durability.MaxDurability, Is.EqualTo(100));
            var stamina = Stamina();
            stamina.Initialize(100, 100);

            var result = ExecuteOffhand(Body("Whiff caster", new Vector2(2650, 2650)), 1);

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(result.CostSpent, Is.True);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(90));
            yield return null;
        }

        [UnityTest]
        public IEnumerator OffhandCut_BrokenDaggerAndBow_AreRejectedBeforeCost()
        {
            var caster = Body("Rejected equipment caster", new Vector2(2700, 2700));
            var stamina = Stamina();
            stamina.Initialize(100, 100);
            _equipment.EquipItem(EquipmentSlot.LeftHand, "item_weapon_dagger_copper");
            var durability = _equipment.GetItemDurability("item_weapon_dagger_copper");
            Assert.That(durability, Is.Not.Null);
            durability.CurrentDurability = 0;

            var broken = ExecuteOffhand(caster, 1);
            Assert.That(broken.FailureReason, Is.EqualTo(MeleeEquipmentGate.RequiresOffhandFailureKey));
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));

            _equipment.EquipItem(EquipmentSlot.LeftHand, "item_weapon_bow_wood");
            var bow = ExecuteOffhand(caster, 1);
            Assert.That(bow.FailureReason, Is.EqualTo(MeleeEquipmentGate.RequiresOffhandFailureKey));
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            yield return null;
        }

        [UnityTest]
        public IEnumerator OffhandCut_RankThree_UsesArcAndDeduplicatesEnemyColliders()
        {
            _equipment.EquipItem(EquipmentSlot.LeftHand, "item_weapon_dagger_copper");
            Stamina().Initialize(100, 100);
            var caster = Body("Arc caster", new Vector2(2750, 2750));
            var inside = Enemy("Inside arc", (Vector2)caster.transform.position + Vector2.right * .7f, "inside");
            inside.gameObject.AddComponent<CircleCollider2D>().radius = .25f;
            var behind = Enemy("Behind arc", (Vector2)caster.transform.position + Vector2.left * .7f, "behind");
            Physics2D.SyncTransforms();

            var result = ExecuteOffhand(caster, 3);

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(inside.CurrentHp, Is.EqualTo(88), "Two colliders must still produce one 12-damage hit.");
            Assert.That(behind.CurrentHp, Is.EqualTo(100), "A target behind the 140-degree arc must not be hit.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator WhirlCut_RankThree_CapsAtSixAndFallsOffAfterThird()
        {
            Stamina().Initialize(100, 100);
            var caster = Body("Whirl phase11 caster", new Vector2(2800, 2800));
            var targets = new List<EnemyHealth>();
            for (int i = 0; i < 7; i++)
                targets.Add(Enemy($"Whirl phase11 target {i}", (Vector2)caster.transform.position + Vector2.right * .5f, $"target_{i:D2}"));
            Physics2D.SyncTransforms();
            var action = FindAction("skill_melee_whirl_cut");

            var result = new MeleeStrikeSkillEffectExecutor("fixture.whirl", "Corte Giratório", action)
                .Execute(new SkillEffectContext { Caster = caster, ActionData = action, Rank = 3 });

            Assert.That(result.Success, Is.True, result.FailureReason);
            for (int i = 0; i < 3; i++) Assert.That(targets[i].CurrentHp, Is.EqualTo(86));
            for (int i = 3; i < 6; i++) Assert.That(targets[i].CurrentHp, Is.EqualTo(90));
            Assert.That(targets[6].CurrentHp, Is.EqualTo(100));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AuthoredRange_IsMeasuredFromCaster_AndWhirlIsSymmetric()
        {
            _equipment.EquipItem(EquipmentSlot.LeftHand, "item_weapon_dagger_copper");
            Stamina().Initialize(100, 100);
            var caster = Body("Range caster", new Vector2(2850, 2850));
            var inside = Enemy("Inside authored range", (Vector2)caster.transform.position + Vector2.right, "inside_range");
            var beyond = Enemy("Beyond authored range", (Vector2)caster.transform.position + Vector2.right * 1.6f, "outside_range");
            Physics2D.SyncTransforms();

            Assert.That(ExecuteOffhand(caster, 1).Success, Is.True);
            Assert.That(inside.CurrentHp, Is.EqualTo(92));
            Assert.That(beyond.CurrentHp, Is.EqualTo(100), "The overlap shape must not extend the authored 1.2 range by shifting its center.");

            Stamina().Initialize(100, 100);
            var front = Enemy("Whirl front", (Vector2)caster.transform.position + Vector2.right * 1.5f, "whirl_front");
            var back = Enemy("Whirl back", (Vector2)caster.transform.position + Vector2.left * 1.5f, "whirl_back");
            Physics2D.SyncTransforms();
            var whirl = FindAction("skill_melee_whirl_cut");
            var result = new MeleeStrikeSkillEffectExecutor("fixture.whirl.range", "Corte Giratório", whirl)
                .Execute(new SkillEffectContext { Caster = caster, ActionData = whirl, Rank = 1 });
            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(front.CurrentHp, Is.EqualTo(90));
            Assert.That(back.CurrentHp, Is.EqualTo(90), "A 360-degree strike must have symmetric reach.");
            yield return null;
        }

        [UnityTest, Order(90)]
        public IEnumerator CaveScene_ControllerRejectsMissingOrBrokenOffhandBeforeCommit()
        {
            yield return SceneManager.LoadSceneAsync("CaveScene", LoadSceneMode.Single);
            yield return null;
            _equipment = EquipmentManager.Instance;
            Assert.That(_equipment, Is.Not.Null);
            var skills = SkillTreeManager.Instance;
            var controller = ActiveSkillExecutionController.Instance;
            Assert.That(skills, Is.Not.Null);
            Assert.That(controller, Is.Not.Null);
            var fixture = new SkillTreeSaveData();
            fixture.NodeRanks.Add(new SkillNodeRankEntry("melee_offhand_cut", 1));
            skills.RestoreFromSaveData(fixture, 100);
            Assert.That(skills.TryAssignActiveSlot(0, MeleeEquipmentGate.OffhandCutActionId), Is.True);
            var stamina = Stamina();
            stamina.Initialize(100, 100);

            _equipment.UnequipSlot(EquipmentSlot.LeftHand);
            controller.TryUseSlot(0);
            Assert.That(controller.HasActiveCast, Is.False);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);

            _equipment.EquipItem(EquipmentSlot.LeftHand, "item_weapon_dagger_copper");
            var durability = _equipment.GetItemDurability("item_weapon_dagger_copper");
            Assert.That(durability, Is.Not.Null);
            controller.TryUseSlot(0);
            Assert.That(controller.CurrentCastPhase, Is.EqualTo(CindarsHope.Skills.Runtime.SkillCastPhase.Windup));
            durability.CurrentDurability = 0;
            yield return new WaitForSeconds(.12f);
            Assert.That(controller.HasActiveCast, Is.False);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(100));
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero);
        }

        private SkillEffectResult ExecuteOffhand(GameObject caster, int rank)
        {
            var action = FindAction(MeleeEquipmentGate.OffhandCutActionId);
            var bootstrap = GameBootstrap.Instance;
            var gate = new MeleeEquipmentGate(_equipment, new EquippedItemResolver(
                bootstrap.ItemDatabase as CindarsHope.Inventory.Data.ItemDatabaseSO,
                bootstrap.WeaponDatabase,
                bootstrap.SpellDatabase));
            return new MeleeStrikeSkillEffectExecutor("fixture.offhand", "Corte da Mão Secundária", action, gate)
                .Execute(new SkillEffectContext { Caster = caster, ActionData = action, Rank = rank });
        }

        private SkillActionSO FindAction(string id)
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            SkillActionSO selected = null;
            foreach (var action in actions)
            {
                if (action.SkillActionId == id) selected = action;
                else Object.Destroy(action);
            }
            Assert.That(selected, Is.Not.Null, id);
            _owned.Add(selected);
            return selected;
        }

        private StaminaManager Stamina() => (StaminaManager)GameBootstrap.Instance.StaminaManager;

        private GameObject Body(string name, Vector2 position)
        {
            var go = new GameObject(name);
            _owned.Add(go);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            go.AddComponent<BoxCollider2D>().size = Vector2.one * .4f;
            return go;
        }

        private EnemyHealth Enemy(string name, Vector2 position, string instanceId)
        {
            var go = Body(name, position);
            go.GetComponent<Collider2D>().isTrigger = true;
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _owned.Add(data);
            data.enemyId = "phase11_fixture";
            data.maxHp = 100;
            var health = go.AddComponent<EnemyHealth>();
            health.Configure(data);
            health.ConfigureLootContext(instanceId, "phase11_run");
            return health;
        }
    }
}
