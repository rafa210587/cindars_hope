using System.Reflection;
using CindarsHope.Core.Events;
using CindarsHope.World;
using CindarsHope.World.Data;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    public sealed class TreeNodeVisualTests
    {
        private GameObject _object;
        private TreeNode _tree;
        private SpriteRenderer _renderer;
        private TreeDataSO _data;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("TreeVisualTest");
            _renderer = _object.AddComponent<SpriteRenderer>();
            _tree = _object.AddComponent<TreeNode>();
            _data = ScriptableObject.CreateInstance<TreeDataSO>();
            _data.Id = "test_tree_visual";
            _data.RequiredHits = 4;
            _data.MaxHp = 4;
            _data.RegrowthDays = 2;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_object);
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void AuthoredWhiteTint_SurvivesAwakeAndHealthyRestore()
        {
            _tree.Configure(0, _data, null, _renderer, healthyTint: Color.white);
            Assert.That(_renderer.color, Is.EqualTo(Color.white));
            InvokeHandler("Awake");
            Assert.That(_renderer.color, Is.EqualTo(Color.white));
            _tree.RestoreFromSaveData(new TreeSaveData { CurrentHp = 4 });
            Assert.That(_renderer.color, Is.EqualTo(Color.white));
            Assert.That(_tree.CurrentHp, Is.EqualTo(4));
            Assert.That(_tree.IsChopped, Is.False);
        }

        [Test]
        public void OmittedTint_PreservesLegacyHealthyAndDamageColors()
        {
            var healthy = new Color(0.24f, 0.48f, 0.22f);
            var damaged = new Color(0.58f, 0.42f, 0.24f);
            _tree.Configure(0, _data, null, _renderer);
            Assert.That(_renderer.color, Is.EqualTo(healthy));
            _tree.RestoreFromSaveData(new TreeSaveData { CurrentHp = 2, HitsTaken = 2 });
            Assert.That(_renderer.color, Is.EqualTo(Color.Lerp(healthy, damaged, 0.5f)));
            Assert.That(_tree.CaptureSaveData().HitsTaken, Is.EqualTo(2));
        }

        [Test]
        public void AuthoredTint_DamageAndChopStillApply_ThenRegrowthRestoresWhite()
        {
            _tree.Configure(0, _data, null, _renderer, healthyTint: Color.white);
            _tree.RestoreFromSaveData(new TreeSaveData { CurrentHp = 2, HitsTaken = 2 });
            Assert.That(_renderer.color, Is.EqualTo(Color.Lerp(Color.white, new Color(0.58f, 0.42f, 0.24f), 0.5f)));
            _tree.RestoreFromSaveData(new TreeSaveData { HitsTaken = 4, CurrentHp = 0, IsChopped = true, RegrowthRemainingDays = 2 });
            var stumpTint = new Color(0.45f, 0.45f, 0.45f, 0.45f);
            Assert.That(_renderer.color, Is.EqualTo(stumpTint));
            InvokeHandler("OnDayStarted", default(DayStartedEvent));
            Assert.That(_tree.RegrowthRemainingDays, Is.EqualTo(1));
            Assert.That(_tree.IsChopped, Is.True);
            Assert.That(_renderer.color, Is.EqualTo(stumpTint));
            InvokeHandler("OnDayStarted", default(DayStartedEvent));
            Assert.That(_tree.IsChopped, Is.False);
            Assert.That(_tree.HitsTaken, Is.Zero);
            Assert.That(_tree.CurrentHp, Is.EqualTo(4));
            Assert.That(_renderer.color, Is.EqualTo(Color.white));
        }

        private void InvokeHandler(string name, params object[] arguments)
        {
            // EditMode invokes the handler directly; it does not pretend to simulate Unity messages.
            var method = typeof(TreeNode).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(_tree, arguments);
        }
    }
}
