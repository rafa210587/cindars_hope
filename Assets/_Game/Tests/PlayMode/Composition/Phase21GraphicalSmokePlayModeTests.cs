using System;
using System.Collections;
using System.Text.RegularExpressions;
using CindarsHope.Cave;
using CindarsHope.Cave.Runtime;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    /// <summary>
    /// AC07 smoke: proves that each gameplay scene can present a live avatar and
    /// skill runtime to a real graphics device. PNG output is optional and is
    /// only enabled by CINDARS_SKILL_CAPTURE_DIR.
    ///
    /// This is an automated graphics smoke, not human visual acceptance: a
    /// reviewer must still inspect telegraph readability and enemy reaction.
    /// </summary>
    public sealed class Phase21GraphicalSmokePlayModeTests
    {
        private static readonly string[] GameplayScenes =
        {
            "FarmScene",
            "TownScene",
            "CaveScene"
        };

        [UnityTest]
        public IEnumerator FarmTownCaveScenes_PresentGameplayOnGraphicsDevice_AndCaptureWhenRequested()
        {
            Assert.That(SystemInfo.graphicsDeviceType,
                Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null),
                "AC07 requires a graphics device; a headless run cannot certify visual acceptance.");

            LogAssert.Expect(LogType.Error,
                new Regex("^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));

            foreach (string sceneName in GameplayScenes)
            {
                if (sceneName == "CaveScene")
                {
                    yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
                    yield return null;
                    Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("FarmScene"));
                    var entrance = FindInActiveScene<CaveEntranceInteractable>();
                    Assert.That(entrance, Is.Not.Null, "FarmScene precisa expor a entrada canônica da Cave.");
                    Assert.That(PlayerController.ActiveInstance, Is.Not.Null);
                    entrance.Interact(PlayerController.ActiveInstance.gameObject);
                }
                else
                {
                    yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                }
                for (var frame = 0; frame < 120 &&
                     (SceneManager.GetActiveScene().name != sceneName ||
                      PlayerController.ActiveInstance == null || ActiveSkillExecutionController.Instance == null);
                     frame++)
                    yield return null;

                if (sceneName == "CaveScene")
                {
                    CaveLevelRuntimeController level = null;
                    for (var frame = 0; frame < 240; frame++)
                    {
                        level = FindInActiveScene<CaveLevelRuntimeController>();
                        if (level != null && level.CurrentGeneratedLevel != null &&
                            level.Materializer != null && level.Materializer.GeneratedRuntimeRoot != null &&
                            level.Materializer.LastMaterializationResult != null &&
                            level.Materializer.LastMaterializationResult.CreatedFloorTiles > 0)
                            break;
                        yield return null;
                    }
                    Assert.That(level, Is.Not.Null, "CaveLevelRuntimeController ausente após entrada canônica.");
                    Assert.That(level.CurrentGeneratedLevel, Is.Not.Null, "Cave run não gerou nível.");
                    Assert.That(level.Materializer.GeneratedRuntimeRoot, Is.Not.Null,
                        "Cave run não materializou mundo.");
                    Assert.That(level.Materializer.LastMaterializationResult.CreatedFloorTiles, Is.GreaterThan(0));
                    // A materialização coincide com a transição/fade; capturar após estabilizar.
                    yield return new WaitForSeconds(2f);
                }

                yield return null;
                yield return null;

                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName));
                Assert.That(PlayerController.ActiveInstance, Is.Not.Null,
                    sceneName + " must materialize the playable avatar.");
                Assert.That(ActiveSkillExecutionController.Instance, Is.Not.Null,
                    sceneName + " must materialize the active skill runtime.");

                UnityEngine.Camera camera = UnityEngine.Camera.main;
                Assert.That(camera, Is.Not.Null, sceneName + " must expose the gameplay camera.");
                Assert.That(camera.enabled, Is.True, sceneName + " gameplay camera must be enabled.");

                string captureDirectory = Environment.GetEnvironmentVariable("CINDARS_SKILL_CAPTURE_DIR");
                var visibleFraction = CaptureCamera(camera, captureDirectory, sceneName);
                Assert.That(visibleFraction, Is.GreaterThan(.05f),
                    sceneName + " camera rendered almost entirely black.");
            }
        }

        private static T FindInActiveScene<T>() where T : Component
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static float CaptureCamera(UnityEngine.Camera camera, string directory, string sceneName)
        {
            const int width = 1280;
            const int height = 720;
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;

            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();

                var pixels = image.GetPixels32();
                var visible = 0;
                var sampled = 0;
                for (var index = 0; index < pixels.Length; index += 64)
                {
                    var pixel = pixels[index];
                    if (pixel.r + pixel.g + pixel.b > 30) visible++;
                    sampled++;
                }

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    byte[] png = image.EncodeToPNG();
                    Assert.That(png, Is.Not.Null.And.Not.Empty, sceneName + " camera did not produce pixels.");
                    System.IO.Directory.CreateDirectory(directory);
                    System.IO.File.WriteAllBytes(
                        System.IO.Path.Combine(directory, "phase21-" + sceneName + ".png"), png);
                }
                return sampled > 0 ? (float)visible / sampled : 0f;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                if (image != null) UnityEngine.Object.Destroy(image);
                target.Release();
                UnityEngine.Object.Destroy(target);
            }
        }
    }
}
