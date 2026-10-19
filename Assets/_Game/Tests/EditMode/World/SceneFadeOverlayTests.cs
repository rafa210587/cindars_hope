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
        public void SceneFadeOverlayBootstrap_HasInstallMethod()
        {
            // Batch 4 (residual_v5): migrado de [RuntimeInitializeOnLoadMethod] auto-bootstrap
            // para instalação explícita via PresentationRuntimeInstaller.Install(owner), chamado
            // pelo GameRuntimeCompositionRoot. O contrato agora é um metodo publico estatico
            // Install(Transform) em vez do atributo de auto-init.
            var install = typeof(SceneFadeOverlayBootstrap)
                .GetMethod("Install", BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(install, "SceneFadeOverlayBootstrap deve expor Install(Transform) publico estatico.");
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
