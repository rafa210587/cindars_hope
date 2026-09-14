using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
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
    /// <summary>Confirma o contrato de resolução da Fagulha no pipeline de gameplay real.</summary>
    public sealed class Phase21SkillActionOutcomePlayModeTests
    {
        private readonly List<SkillActionResolvedEvent> _events = new List<SkillActionResolvedEvent>();
        private SkillTreeManager _skills;
        private ManaManager _mana;
        private SkillTreeSaveData _originalSkills;
        private int _originalMana;

        [UnitySetUp]
        public IEnumerator PrepareFarmScene()
        {
            LogAssert.Expect(LogType.Error,
                new Regex("^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));
            yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(GameBootstrap.Instance, Is.Not.Null);
            _skills = SkillTreeManager.Instance;
            _mana = GameBootstrap.Instance.ManaManager as ManaManager;
            Assert.That(_skills, Is.Not.Null, "FarmScene não materializou SkillTreeManager.");
            Assert.That(_mana, Is.Not.Null, "FarmScene não materializou ManaManager.");

            _originalSkills = _skills.CaptureSaveData();
            _originalMana = _mana.CurrentMana;
            _skills.RestoreFromSaveData(new SkillTreeSaveData
            {
                NodeRanks = new List<SkillNodeRankEntry> { new SkillNodeRankEntry("magic_fire_spark", 1) }
            }, 50);
            Assert.That(_skills.TryAssignActiveSlot(0, "skill_magic_fire_spark"), Is.True,
                "Fagulha R1 não pôde ser equipada no slot real.");
            GameEventBus.Subscribe<SkillActionResolvedEvent>(OnResolved);
        }

        [UnityTearDown]
        public IEnumerator RestoreFarmState()
        {
            GameEventBus.Unsubscribe<SkillActionResolvedEvent>(OnResolved);
            if (_skills != null && _originalSkills != null)
                _skills.RestoreFromSaveData(_originalSkills, 50);
            if (_mana != null)
                _mana.SetMana(_originalMana);
            _events.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator FireSpark_ReportsRefusedForManaThenOneCommitAndCooldownRefusal()
        {
            var controller = ActiveSkillExecutionController.Instance;
            Assert.That(controller, Is.Not.Null, "Executor real não está disponível.");
            var actionId = "skill_magic_fire_spark";

            // O executor persiste entre cenas e a recarga pertence a actionId.
            var cooldownDeadline = Time.time + 10f;
            while (controller.GetSlotCooldownRemaining(0) > 0f && Time.time < cooldownDeadline)
                yield return null;
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.EqualTo(0f).Within(.01f));

            _mana.SetMana(0);
            _events.Clear();
            controller.TryUseSlot(0);
            yield return new WaitForSeconds(.5f);
            Assert.That(Find(actionId, SkillActionResolvedEvent.Refused), Is.EqualTo(1));
            Assert.That(Last(actionId).Rank, Is.EqualTo(1));
            Assert.That(Last(actionId).FailureKey, Is.EqualTo("InsufficientMana"));
            Assert.That(Find(actionId, SkillActionResolvedEvent.Committed), Is.Zero);

            _mana.SetMana(_mana.MaxMana);
            _events.Clear();
            controller.TryUseSlot(0);
            yield return new WaitForSeconds(.5f);
            Assert.That(Find(actionId, SkillActionResolvedEvent.Committed), Is.EqualTo(1));
            Assert.That(Find(actionId, SkillActionResolvedEvent.Refused), Is.Zero);

            _events.Clear();
            controller.TryUseSlot(0);
            Assert.That(Find(actionId, SkillActionResolvedEvent.Refused), Is.EqualTo(1));
            Assert.That(Last(actionId).FailureKey, Is.EqualTo("Cooldown"));
            Assert.That(Find(actionId, SkillActionResolvedEvent.Committed), Is.Zero);
        }

        private void OnResolved(SkillActionResolvedEvent evt) => _events.Add(evt);

        private int Find(string actionId, string outcome)
        {
            var count = 0;
            foreach (var evt in _events)
                if (evt.ActionId == actionId && evt.Outcome == outcome) count++;
            return count;
        }

        private SkillActionResolvedEvent Last(string actionId)
        {
            for (var i = _events.Count - 1; i >= 0; i--)
                if (_events[i].ActionId == actionId) return _events[i];
            Assert.Fail($"Nenhum evento de resolução para {actionId}.");
            return default;
        }
    }
}
