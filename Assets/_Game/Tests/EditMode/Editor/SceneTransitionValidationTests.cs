using System;
using CindarsHope.Editor.Validation;
using CindarsHope.Foundation;
using CindarsHope.World.Scenes;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Tests.EditMode.Editor
{
    public class SceneTransitionValidationTests
    {
        private Scene _original;
        private Scene _selected;
        private Scene _other;

        [SetUp]
        public void SetUp()
        {
            _original = SceneManager.GetActiveScene();
            _selected = EditorSceneManager.NewPreviewScene();
            _other = EditorSceneManager.NewPreviewScene();
        }

        [TearDown]
        public void TearDown()
        {
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_other.IsValid()) EditorSceneManager.ClosePreviewScene(_other);
            if (_selected.IsValid()) EditorSceneManager.ClosePreviewScene(_selected);
        }

        [Test]
        public void MissingScene_FailsWithoutChangingActiveSceneOrPreviewContents()
        {
            AddObject(_selected, "unsaved fixture object");
            int count = SceneManager.sceneCount;
            bool dirty = _original.isDirty;

            var report = ValidateSceneTransitions.ValidateScenePaths(new[]
            {
                "Assets/missing-transition-fixture-" + Guid.NewGuid().ToString("N") + ".unity"
            });

            Assert.That(report.Passed, Is.False);
            Assert.That(report.Issues.Exists(issue => issue.Code == "SCENE_NOT_FOUND"), Is.True);
            Assert.That(report.Issues.Exists(issue => issue.Code == "MISSING_GATE"), Is.True);
            Assert.That(report.Issues.Exists(issue => issue.Code == "MISSING_ANCHOR"), Is.True);
            Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(_original));
            Assert.That(_original.isDirty, Is.EqualTo(dirty));
            Assert.That(_selected.GetRootGameObjects().Length, Is.EqualTo(1));
            Assert.That(SceneManager.sceneCount, Is.EqualTo(count));
        }

        [Test]
        public void ComponentsInAnotherLoadedScene_DoNotSatisfySelectedScene()
        {
            AddCanonicalComponents(_other, false);
            Assert.That(ValidateSceneTransitions.ValidateLoadedScenes(new[] { _other }).Passed, Is.True);

            var selectedReport = ValidateSceneTransitions.ValidateLoadedScenes(new[] { _selected });

            Assert.That(selectedReport.Passed, Is.False);
            Assert.That(selectedReport.Issues.FindAll(issue => issue.Code == "MISSING_GATE").Count, Is.EqualTo(4));
            Assert.That(selectedReport.Issues.FindAll(issue => issue.Code == "MISSING_ANCHOR").Count, Is.EqualTo(4));
        }

        [Test]
        public void InactiveSerializedComponents_AreInspectedWithoutMutatingScene()
        {
            AddCanonicalComponents(_selected, true);
            bool dirty = _selected.isDirty;
            int count = SceneManager.sceneCount;

            var report = ValidateSceneTransitions.ValidateLoadedScenes(new[] { _selected, _selected });

            Assert.That(report.Passed, Is.True, report.GetSummary("fixture"));
            Assert.That(_selected.isDirty, Is.EqualTo(dirty));
            Assert.That(_selected.GetRootGameObjects()[0].activeSelf, Is.False);
            Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(_original));
            Assert.That(SceneManager.sceneCount, Is.EqualTo(count));
        }

        [Test]
        public void DuplicateGateOrEmptyDestination_IsNotAValidConfiguration()
        {
            AddCanonicalComponents(_selected, false);
            var invalid = AddObject(_selected, "duplicate gate").AddComponent<SceneTransitionGate>();
            SetString(invalid, "_transitionId", SceneId.GateFarmTownExit);

            var report = ValidateSceneTransitions.ValidateLoadedScenes(new[] { _selected });

            Assert.That(report.Passed, Is.False);
            Assert.That(report.Issues.Exists(issue => issue.Code == "DUPLICATE_GATE"), Is.True);
            Assert.That(report.Issues.Exists(issue => issue.Code == "EMPTY_DESTINATION"), Is.True);
            Assert.That(report.Issues.Exists(issue => issue.Code == "EMPTY_TARGET_ANCHOR"), Is.True);
        }

        private static void AddCanonicalComponents(Scene scene, bool inactive)
        {
            string[] gates = { SceneId.GateFarmTownExit, SceneId.GateFarmCaveEntrance, SceneId.GateTownFarmExit, SceneId.GateCaveFarmExit };
            string[] anchors = { SceneId.SpawnFarmFromTown, SceneId.SpawnFarmFromCave, SceneId.SpawnTownFromFarm, SceneId.SpawnCaveFromFarm };
            for (int index = 0; index < gates.Length; index++)
            {
                var gate = AddObject(scene, gates[index]).AddComponent<SceneTransitionGate>();
                SetString(gate, "_transitionId", gates[index]);
                SetString(gate, "_toSceneId", "fixture_destination");
                SetString(gate, "_targetSpawnAnchorId", anchors[index]);
                gate.gameObject.SetActive(!inactive);
                var anchor = AddObject(scene, anchors[index]).AddComponent<SceneSpawnAnchor>();
                SetString(anchor, "_spawnAnchorId", anchors[index]);
                anchor.gameObject.SetActive(!inactive);
            }
        }

        private static GameObject AddObject(Scene scene, string name)
        {
            var gameObject = new GameObject(name);
            SceneManager.MoveGameObjectToScene(gameObject, scene);
            return gameObject;
        }

        private static void SetString(UnityEngine.Object target, string field, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
