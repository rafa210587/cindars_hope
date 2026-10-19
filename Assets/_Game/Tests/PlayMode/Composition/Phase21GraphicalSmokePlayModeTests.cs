using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CindarsHope.Cave;
using CindarsHope.Cave.Runtime;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

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
                if (sceneName == "CaveScene" && !string.IsNullOrWhiteSpace(captureDirectory))
                {
                    CaptureCaveRenderComposition(camera, captureDirectory);
                    CaptureCaveLayerVariants(camera, captureDirectory);
                    CaptureCaveBackgroundVariant(camera, captureDirectory);
                }
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

        // Companion to the Cave PNG: each tile's world center/tint and all visible sprite bounds
        // allow a dark screen pixel to be traced back to a renderer in the same materialized run.
        // Read-only: no scene, seed, object or save mutation.
        private static void CaptureCaveRenderComposition(UnityEngine.Camera camera, string directory)
        {
            var levelController = FindInActiveScene<CaveLevelRuntimeController>();
            Assert.That(levelController, Is.Not.Null);
            var level = levelController.CurrentGeneratedLevel;
            Assert.That(level, Is.Not.Null);
            var run = FindInActiveScene<CaveRunManager>();
            var root = levelController.Materializer.GeneratedRuntimeRoot;
            Assert.That(root, Is.Not.Null);

            var output = new StringBuilder();
            output.AppendLine("scene=CaveScene");
            output.AppendLine("level=" + level.CaveLevel + " layoutHash=" + level.LayoutHash +
                " worldSeed=" + (run != null ? run.CaveWorldSeed : "NULL") +
                " runSeed=" + (run != null ? run.CaveRunSeed : "NULL"));
            output.AppendLine("entrance=" + level.Entrance + " width=" + level.Width + " height=" + level.Height);
            output.AppendLine("cameraPosition=" + camera.transform.position +
                " orthographicSize=" + camera.orthographicSize + " aspect=" + camera.aspect +
                " capturePixels=1280x720");

            foreach (var tilemap in root.GetComponentsInChildren<Tilemap>(true))
            {
                var tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
                output.AppendLine("tilemap=" + tilemap.name + " bounds=" + tilemap.cellBounds +
                    " sort=" + (tilemapRenderer != null
                        ? tilemapRenderer.sortingLayerName + "/" + tilemapRenderer.sortingOrder : "NULL") +
                    " tileAnchor=" + tilemap.tileAnchor + " color=" + tilemap.color +
                    " scale=" + tilemap.transform.lossyScale + " shader=" +
                    (tilemapRenderer != null && tilemapRenderer.sharedMaterial != null &&
                     tilemapRenderer.sharedMaterial.shader != null
                        ? tilemapRenderer.sharedMaterial.shader.name : "NULL"));
                var seenSprites = new HashSet<string>();
                foreach (var cell in tilemap.cellBounds.allPositionsWithin)
                {
                    var tile = tilemap.GetTile(cell);
                    if (tile == null) continue;
                    var sprite = tilemap.GetSprite(cell);
                    if (sprite != null && seenSprites.Add(tile.name))
                    {
                        output.AppendLine("tileSprite=" + tile.name + " rect=" + sprite.rect +
                            " bounds=" + sprite.bounds + " pivot=" + sprite.pivot +
                            " texture=" + sprite.texture.name + " textureSize=" +
                            sprite.texture.width + "x" + sprite.texture.height +
                            " alpha=" + ProbeTextureAlpha(sprite.texture));
                    }
                    output.AppendLine("cell=" + cell + " world=" + tilemap.GetCellCenterWorld(cell) +
                        " tile=" + tile.name + " tint=" + tilemap.GetColor(cell) +
                        " transform=" + tilemap.GetTransformMatrix(cell));
                }
            }

            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                output.AppendLine("sprite=" + renderer.name + " bounds=" + renderer.bounds +
                    " color=" + renderer.color + " sort=" + renderer.sortingLayerName +
                    "/" + renderer.sortingOrder + " texture=" +
                    (renderer.sprite != null ? renderer.sprite.texture.name : "NULL") +
                    " shader=" + (renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null
                        ? renderer.sharedMaterial.shader.name : "NULL"));
            }

            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "phase21-CaveScene-render.txt"), output.ToString());
        }

        private static string ProbeTextureAlpha(Texture source)
        {
            var oldActive = RenderTexture.active;
            var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            var readback = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                Graphics.Blit(source, target);
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                readback.Apply();
                var min = 255;
                var max = 0;
                foreach (var pixel in readback.GetPixels32())
                {
                    if (pixel.a < min) min = pixel.a;
                    if (pixel.a > max) max = pixel.a;
                }
                return min + ".." + max;
            }
            catch (Exception ex)
            {
                return "UNAVAILABLE(" + ex.GetType().Name + ")";
            }
            finally
            {
                RenderTexture.active = oldActive;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.Destroy(readback);
            }
        }

        // Diagnostic renders occur in the same frame and restore every enabled flag before
        // returning. They isolate the vignette and World overlays without changing level data.
        private static void CaptureCaveLayerVariants(UnityEngine.Camera camera, string directory)
        {
            var levelController = FindInActiveScene<CaveLevelRuntimeController>();
            Assert.That(levelController, Is.Not.Null);
            var root = levelController.Materializer.GeneratedRuntimeRoot;
            Assert.That(root, Is.Not.Null);

            SpriteRenderer vignette = null;
            var sprites = root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in sprites)
            {
                if (renderer.name == "CaveVignetteOverlay") vignette = renderer;
            }
            Assert.That(vignette, Is.Not.Null);

            var vignetteWasEnabled = vignette.enabled;
            try
            {
                vignette.enabled = false;
                CaptureCamera(camera, directory, "CaveScene-no-vignette");
            }
            finally
            {
                vignette.enabled = vignetteWasEnabled;
            }

            var tilemapRenderers = root.GetComponentsInChildren<TilemapRenderer>(true);
            var spriteEnabled = new bool[sprites.Length];
            var tilemapEnabled = new bool[tilemapRenderers.Length];
            try
            {
                for (var i = 0; i < sprites.Length; i++)
                {
                    spriteEnabled[i] = sprites[i].enabled;
                    if (sprites[i].sortingLayerName == "World") sprites[i].enabled = false;
                }
                for (var i = 0; i < tilemapRenderers.Length; i++)
                {
                    tilemapEnabled[i] = tilemapRenderers[i].enabled;
                    if (tilemapRenderers[i].sortingLayerName == "World") tilemapRenderers[i].enabled = false;
                }
                CaptureCamera(camera, directory, "CaveScene-ground-only");
            }
            finally
            {
                for (var i = 0; i < sprites.Length; i++) sprites[i].enabled = spriteEnabled[i];
                for (var i = 0; i < tilemapRenderers.Length; i++) tilemapRenderers[i].enabled = tilemapEnabled[i];
            }
        }

        private static void CaptureCaveBackgroundVariant(UnityEngine.Camera camera, string directory)
        {
            var oldFlags = camera.clearFlags;
            var oldColor = camera.backgroundColor;
            try
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.magenta;
                CaptureCamera(camera, directory, "CaveScene-magenta-background");
            }
            finally
            {
                camera.backgroundColor = oldColor;
                camera.clearFlags = oldFlags;
            }
        }
    }
}
