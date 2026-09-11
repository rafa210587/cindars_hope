using CindarsHope.Combat;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class SkillExecutionFoundationTests
    {
        [Test] public void MissingResourceOrStatus_ReturnsFailureWithoutReportedDebit()
        {
            var caster = new GameObject("Caster");
            try
            {
                var executor = new ProjectileSkillEffectExecutor("test", "test", 10, 10, 5, DamageType.Fire, 10);
                var result = executor.Execute(new SkillEffectContext { Caster = caster });
                Assert.That(result.Success, Is.False);
                Assert.That(result.CostSpent, Is.False);
                executor = new ProjectileSkillEffectExecutor("test", "test", 10, 10, 5, DamageType.Fire, 0, statusEffectId: "missing", statusApplyChance: 1);
                Assert.That(executor.Execute(new SkillEffectContext { Caster = caster }).FailureReason, Is.EqualTo("InvalidStatus"));
            }
            finally { Object.DestroyImmediate(caster); }
        }

        [Test] public void Melee_MultipleColliders_SingleDamageApplication()
        {
            var caster = new GameObject("Caster");
            var target = new GameObject("Target");
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            data.enemyId = "fixture"; data.maxHp = 100;
            try
            {
                target.transform.position = Vector3.right;
                target.AddComponent<BoxCollider2D>();
                var child = new GameObject("Second collider"); child.transform.SetParent(target.transform, false);
                child.AddComponent<CircleCollider2D>();
                var health = target.AddComponent<EnemyHealth>(); health.Configure(data);
                Physics2D.SyncTransforms();
                var executor = new MeleeStrikeSkillEffectExecutor("test", "test", 10, 2, 360, 0, knockbackForce: 0);
                Assert.That(executor.Execute(new SkillEffectContext { Caster = caster }).Success, Is.True);
                Assert.That(health.CurrentHp, Is.EqualTo(90));
            }
            finally { Object.DestroyImmediate(target); Object.DestroyImmediate(caster); Object.DestroyImmediate(data); }
        }

        [Test] public void Controller_InvalidSlotsAndDisabledAvatar_AreSafe()
        {
            var host = new GameObject("Skill host");
            var avatar = new GameObject("Avatar");
            try
            {
                var controller = host.AddComponent<ActiveSkillExecutionController>();
                controller.BindAvatar(avatar, null);
                Assert.That(controller.BoundAvatar, Is.SameAs(avatar));
                avatar.SetActive(false);
                controller.BindAvatar(avatar, null);
                Assert.That(controller.BoundAvatar, Is.Null);
                Assert.DoesNotThrow(() => controller.TryUseSlot(-1));
                Assert.DoesNotThrow(() => controller.TryUseSlot(4));
            }
            finally { Object.DestroyImmediate(host); Object.DestroyImmediate(avatar); }
        }
    }
}
