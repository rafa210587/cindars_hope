using System.Reflection;
using NUnit.Framework;
using CindarsHope.World.Scenes;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_77 — Estrutura do SceneFadeOverlay. Testes de comportamento de runtime
    /// (coroutine de fade, subscribe/unsubscribe) requerem Play Mode scenario.
    /// </summary>
    [TestFixture]
    public class SceneFadeOverlayTests
    {
        [Test]
        public void SceneFadeOverlayBootstrap_HasRuntimeInitializeAttribute()
        {
            var methods = typeof(SceneFadeOverlayBootstrap)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
            bool found = false;
            foreach (var m in methods)
            {
                if (m.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>() != null)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "SceneFadeOverlayBootstrap deve ter RuntimeInitializeOnLoadMethod.");
        }

        [Test]
        public void SceneFadeOverlayController_ComponentExists()
        {
            var go = new GameObject("TestFadeOverlay");
            go.AddComponent<Canvas>();
            go.AddComponent<CanvasGroup>();
            var ctrl = go.AddComponent<SceneFadeOverlayController>();
            Assert.IsNotNull(ctrl, "SceneFadeOverlayController deve ser adicionavel como componente.");
            Object.DestroyImmediate(go);
        }
    }
}
