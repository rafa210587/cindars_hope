using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CindarsHope.Camera;
using CindarsHope.Editor.Validation;
using CindarsHope.Farm;
using CindarsHope.Farm.Scene;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Dev
{
    /// <summary>
    /// Sessão Editor recuperável após domain reload. Captura enquadramento real, não travessia
    /// ou fluidez; nenhuma alteração temporária da cena é persistida.
    /// </summary>
    [InitializeOnLoad]
    internal static class FarmPlayModeCaptureSession
    {
        private const string StateKey = "CindarsHope.FarmGameplayCapture";
        private const string ScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private static string OutputDirectory => Environment.GetEnvironmentVariable("CINDARS_FARM_GAMEPLAY_OUTPUT") ?? "docs/validation/playmode/farm_gameplay_review";
        private const double TimeoutSeconds = 120;
        private const double SettleSeconds = 2;
        private static readonly string[] ViewIds = { "spawn", "bridge", "cultivation", "homestead", "forest", "mountain", "animals", "lake", "border_north", "border_south", "border_west", "border_east", "corner_nw", "corner_ne", "corner_sw", "corner_se", "orchard", "pasture" };
        private static Session session;
        private static double settleStarted;
        private static Vector3 previousCameraPosition;
        private static int stableUpdates;
        private static bool positioned;
        private static FarmInteractionSelectionProbe interactionProbe;
        private static FarmDoorAndWaterProbe motionProbe;
        private static FarmAmbientPlaybackCapture ambientProbe;
        private static FarmContactCapture contactProbe;
        private static FarmAnimalMotionCapture animalMotionProbe;
        private static FarmHerdMotionCapture herdMotionProbe;

        [Serializable] private sealed class FileHash { public string path; public string sha256; }
        [Serializable] private sealed class NpcEvidence
        {
            public string npcId, objectName;
            public bool active;
            public Vector3 position;
        }
        [Serializable] private sealed class SpriteMeasure
        {
            public string objectPath, spritePath, spriteName, scaleProfilePath;
            public int textureWidth, textureHeight;
            public float pixelsPerUnit;
            public Rect spriteRect;
            public Vector3 localScale, lossyScale, scaleRootLossyScale, spriteWorldBoundsCenter, spriteWorldBoundsSize;
            public Vector2 projectedPixels;
        }
        [Serializable] private sealed class View
        {
            public string id, utc, path, cameraObjectPath;
            public string transparencySortMode;
            public Vector3 transparencySortAxis;
            public Vector3 playerPosition, cameraPosition;
            public float orthographicSize, cameraAspect;
            public int width, height;
            public SpriteMeasure[] sprites;
        }
        [Serializable] private sealed class Session
        {
            public string status = "RUNNING", stage = "entering", failure, startedUtc, finishedUtc;
            public string playStartedUtc, scenePath = ScenePath, sceneSha256Before, sceneSha256After;
            public string unityVersion, persistentRoot, saveRoot;
            public string saveHashScope = "All files recursively under SaveManager's saves directory; Unity Editor Analytics is outside the player save directory.";
            public string saveIsolation = "Direct Farm bootstrap; SaveInput disabled in memory before Play Mode. No save/load calls. Scene reopened without saving on exit.";
            public string limitations = "Static camera views at current gameplay aspect. Teleports are framing only; do not prove movement, collision traversal, animation fluidity or human acceptance. Bounds include sprite geometry, not alpha-only silhouette. Screen Space Overlay UI is not rendered by Camera.Render.";
            public int disabledSaveInputs;
            public FileHash[] saveHashesBefore, saveHashesAfter;
            public List<View> views = new List<View>();
            public List<string> runtimeErrors = new List<string>();
            public NpcEvidence[] npcs;
            public FarmPhysicalRouteProbe.Evidence physicalRoutes;
            public bool physicalRoutesEvaluated;
            public FarmInteractionSelectionProbe.Evidence interactionSelection;
            public bool interactionSelectionStarted, interactionSelectionEvaluated;
            public bool motionRequested, motionStarted, motionEvaluated;
            public string motionResultStatus = "NOT REQUESTED";
            public FarmDoorAndWaterProbe.Evidence motion;
            public bool ambientOnlyRequested, ambientStarted, ambientEvaluated;
            public FarmAmbientPlaybackCapture.Evidence ambient;
            public bool contactOnlyRequested, contactStarted, contactEvaluated;
            public FarmContactCapture.Evidence contact;
            public bool animalMotionOnlyRequested, animalMotionStarted, animalMotionEvaluated;
            public FarmAnimalMotionCapture.Evidence animalMotion;
            public bool herdMotionOnlyRequested, herdMotionStarted, herdMotionEvaluated;
            public FarmHerdMotionCapture.Evidence herdMotion;
        }

        static FarmPlayModeCaptureSession()
        {
            string saved = SessionState.GetString(StateKey, "");
            if (string.IsNullOrEmpty(saved)) return;
            session = JsonUtility.FromJson<Session>(saved);
            Attach();
        }

        internal static void Begin()
        {
            if (session != null) throw new InvalidOperationException("Farm capture session already active.");
            if (Environment.GetCommandLineArgs().Any(arg => string.Equals(arg, "-quit", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Farm gameplay capture requires omitting -quit.");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before starting farm capture.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Unsaved scene detected; capture will not discard user edits.");
            if (!Application.isBatchMode && (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().path != ScenePath))
                throw new InvalidOperationException("Interactive capture requires only FarmScene open, with no unsaved edits.");

            Directory.CreateDirectory(OutputDirectory);
            session = new Session
            {
                startedUtc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion,
                sceneSha256Before = Hash(ScenePath), persistentRoot = Application.persistentDataPath,
                saveRoot = Path.Combine(Application.persistentDataPath, "saves"),
                saveHashesBefore = HashFiles(Path.Combine(Application.persistentDataPath, "saves")),
                motionRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_MOTION") == "1",
                ambientOnlyRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_AMBIENT") == "1",
                contactOnlyRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_CONTACT") == "1",
                animalMotionOnlyRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_ANIMAL_MOTION") == "1",
                herdMotionOnlyRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_HERD_MOTION") == "1"
            };
            if (session.ambientOnlyRequested)
            {
                // Ambient-only capture deliberately skips unrelated static/route/interaction gates.
                session.motionRequested = false;
                session.limitations = "Real native ambient playback; camera-only framing at gameplay zoom. No movement/input, interaction or route proof. Screen Space Overlay UI is absent from Camera.Render. Inspect ambient.method and timestamps.";
            }
            if (session.contactOnlyRequested)
            {
                session.motionRequested = false;
                session.ambientOnlyRequested = false;
                session.limitations = "Controlled-body contact and live fishing selection only. No successful cast, input, complete circulation or human visual acceptance claimed; inspect contact.method.";
            }
            if (session.animalMotionOnlyRequested)
            {
                session.motionRequested = false;
                session.ambientOnlyRequested = false;
                session.contactOnlyRequested = false;
                session.limitations = "Native boat and one isolated chicken pilot only. No input, visually closed fence, other species, global gameplay or human acceptance claimed; inspect animalMotion.method.";
            }
            if (session.herdMotionOnlyRequested)
            {
                session.motionRequested = false;
                session.ambientOnlyRequested = false;
                session.contactOnlyRequested = false;
                session.animalMotionOnlyRequested = false;
                session.limitations = "Full-capacity Barn_01 herd motion only. No input/routes, all random seeds, global gameplay or human visual acceptance claimed; inspect herdMotion.method.";
            }
            Persist();
            Attach();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                foreach (var input in Components<SaveInput>(scene))
                {
                    if (input.enabled) session.disabledSaveInputs++;
                    input.enabled = false;
                }
                Persist();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Fail(error.Message); }
        }

        private static void Attach()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= OnPlayState;
            EditorApplication.playModeStateChanged += OnPlayState;
            Application.logMessageReceived -= OnLog;
            Application.logMessageReceived += OnLog;
        }

        private static void OnLog(string message, string stack, LogType type)
        {
            if (session == null || (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)) return;
            session.runtimeErrors.Add(message + "\n" + stack);
            Persist();
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (session == null) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                session.stage = session.herdMotionOnlyRequested ? "herd-motion" : session.animalMotionOnlyRequested ? "animal-motion" : session.contactOnlyRequested ? "contact" : session.ambientOnlyRequested ? "ambient" : "capturing";
                session.playStartedUtc = DateTime.UtcNow.ToString("O");
                positioned = false;
                Persist();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                DisposeContact();
                DisposeAmbient();
                DisposeAnimalMotion();
                DisposeHerdMotion();
                Persist();
                motionProbe?.Dispose();
                motionProbe = null;
            }
            else if (state == PlayModeStateChange.EnteredEditMode) Complete();
        }

        private static void Tick()
        {
            if (session == null) return;
            try
            {
                string clock = string.IsNullOrEmpty(session.playStartedUtc) ? session.startedUtc : session.playStartedUtc;
                if ((DateTime.UtcNow - DateTime.Parse(clock).ToUniversalTime()).TotalSeconds > ((session.motionRequested || session.ambientOnlyRequested || session.contactOnlyRequested || session.animalMotionOnlyRequested || session.herdMotionOnlyRequested) ? 180 : TimeoutSeconds))
                {
                    if (session.stage == "exiting")
                    {
                        if (EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            session.status = "FAIL";
                            session.failure = "Timeout exiting Play Mode; scene never saved. Editor shutdown required in batch.";
                            session.finishedUtc = DateTime.UtcNow.ToString("O");
                            File.WriteAllText(Path.Combine(OutputDirectory, "capture-metadata.json"), JsonUtility.ToJson(session, true));
                            if (Application.isBatchMode) EditorApplication.Exit(1);
                            else { EditorApplication.update -= Tick; Debug.LogError(session.failure); }
                        }
                        else Complete();
                        return;
                    }
                    Fail("Timeout waiting for Play Mode, camera stabilization or captures.");
                    return;
                }
                if ((session.stage != "capturing" && session.stage != "interactions" && session.stage != "motion" && session.stage != "ambient" && session.stage != "contact" && session.stage != "animal-motion" && session.stage != "herd-motion") || !EditorApplication.isPlaying) return;
                var scene = SceneManager.GetSceneByPath(ScenePath);
                var player = Components<PlayerController>(scene).SingleOrDefault();
                var camera = Components<UnityEngine.Camera>(scene).SingleOrDefault(c => c.CompareTag("MainCamera") && c.isActiveAndEnabled);
                if (player == null || camera == null) return;
                if (Components<SaveInput>(scene).Any(input => input.isActiveAndEnabled))
                    throw new InvalidOperationException("SaveInput unexpectedly enabled during capture.");
                if (session.stage == "herd-motion")
                {
                    if (herdMotionProbe == null)
                    {
                        if (session.herdMotionStarted)
                            throw new InvalidOperationException("Herd motion probe lost during reload; evidence is incomplete.");
                        herdMotionProbe = new FarmHerdMotionCapture(scene, player, camera, OutputDirectory);
                        session.herdMotion = herdMotionProbe.Result;
                        session.herdMotionStarted = true;
                        Persist();
                    }
                    if (!herdMotionProbe.Tick()) return;
                    session.herdMotionEvaluated = true;
                    DisposeHerdMotion();
                    session.stage = "exiting";
                    Persist();
                    EditorApplication.ExitPlaymode();
                    return;
                }
                if (session.stage == "animal-motion")
                {
                    if (animalMotionProbe == null)
                    {
                        if (session.animalMotionStarted)
                            throw new InvalidOperationException("Animal motion probe lost during reload; evidence is incomplete.");
                        animalMotionProbe = new FarmAnimalMotionCapture(scene, camera, OutputDirectory);
                        session.animalMotion = animalMotionProbe.Result;
                        session.animalMotionStarted = true;
                        Persist();
                    }
                    if (!animalMotionProbe.Tick()) return;
                    session.animalMotionEvaluated = true;
                    DisposeAnimalMotion();
                    session.stage = "exiting";
                    Persist();
                    EditorApplication.ExitPlaymode();
                    return;
                }
                if (session.stage == "contact")
                {
                    if (contactProbe == null)
                    {
                        if (session.contactStarted)
                            throw new InvalidOperationException("Contact probe lost during reload; evidence is incomplete.");
                        contactProbe = new FarmContactCapture(player, camera, OutputDirectory);
                        session.contact = contactProbe.Result;
                        session.contactStarted = true;
                        Persist();
                    }
                    if (!contactProbe.Tick()) return;
                    session.contactEvaluated = true;
                    DisposeContact();
                    session.stage = "exiting";
                    Persist();
                    EditorApplication.ExitPlaymode();
                    return;
                }
                if (session.stage == "ambient")
                {
                    if (ambientProbe == null)
                    {
                        if (session.ambientStarted)
                            throw new InvalidOperationException("Ambient probe lost during reload; evidence is incomplete.");
                        ambientProbe = new FarmAmbientPlaybackCapture(scene, camera, OutputDirectory);
                        session.ambient = ambientProbe.Result;
                        session.ambientStarted = true;
                        Persist();
                    }
                    if (!ambientProbe.Tick()) return;
                    session.ambientEvaluated = true;
                    DisposeAmbient();
                    session.stage = "exiting";
                    Persist();
                    EditorApplication.ExitPlaymode();
                    return;
                }
                if (session.stage == "motion")
                {
                    if (motionProbe == null)
                    {
                        if (session.motionStarted)
                            throw new InvalidOperationException("Motion probe lost during reload; evidence is incomplete.");
                        motionProbe = new FarmDoorAndWaterProbe(player, camera, session.interactionSelection, OutputDirectory);
                        session.motion = motionProbe.Result;
                        session.motionStarted = true;
                        session.motionResultStatus = "RUNNING";
                        Persist();
                    }
                    if (!motionProbe.Tick()) return;
                    session.motionEvaluated = true;
                    session.motionResultStatus = session.motion.status;
                    motionProbe.Dispose();
                    motionProbe = null;
                    session.stage = "exiting";
                    Persist();
                    EditorApplication.ExitPlaymode();
                    return;
                }
                if (session.stage == "interactions")
                {
                    if (interactionProbe == null)
                    {
                        if (session.interactionSelectionStarted)
                            throw new InvalidOperationException("Interaction probe lost during reload; incomplete selection evidence.");
                        interactionProbe = new FarmInteractionSelectionProbe(player, session.physicalRoutes);
                        session.interactionSelection = interactionProbe.Result;
                        session.interactionSelectionStarted = true;
                        Persist();
                    }
                    if (!interactionProbe.Tick()) return;
                    session.interactionSelectionEvaluated = true;
                    interactionProbe.Dispose();
                    interactionProbe = null;
                    session.stage = session.motionRequested ? "motion" : "exiting";
                    Debug.Log("Farm interaction selection: " + session.interactionSelection.status);
                    Persist();
                    if (!session.motionRequested) EditorApplication.ExitPlaymode();
                    return;
                }
                if (!camera.orthographic || camera.GetComponent<CameraFollow2D>() == null)
                    throw new InvalidOperationException("Expected orthographic gameplay camera with CameraFollow2D.");
                if (!positioned)
                {
                    if (session.views.Count > 0) PositionView(player, ViewIds[session.views.Count]);
                    settleStarted = EditorApplication.timeSinceStartup;
                    previousCameraPosition = camera.transform.position;
                    stableUpdates = 0;
                    positioned = true;
                    return;
                }
                stableUpdates = (camera.transform.position - previousCameraPosition).sqrMagnitude < 0.000001f ? stableUpdates + 1 : 0;
                previousCameraPosition = camera.transform.position;
                if (EditorApplication.timeSinceStartup - settleStarted < SettleSeconds || stableUpdates < 10) return;
                Capture(player, camera);
                if (session.views.Count == 1 && !session.physicalRoutesEvaluated)
                {
                    session.physicalRoutes = FarmPhysicalRouteProbe.Evaluate(player);
                    session.npcs = Components<CindarsHope.NPC.NpcController>(scene)
                        .Select(npc => new NpcEvidence { npcId = npc.NpcId, objectName = npc.name,
                            active = npc.isActiveAndEnabled, position = npc.transform.position }).ToArray();
                    session.physicalRoutesEvaluated = true;
                    Debug.Log("Farm physical routes: " + session.physicalRoutes.status + " — " + session.physicalRoutes.reason);
                }
                positioned = false;
                Persist();
                if (session.views.Count == ViewIds.Length)
                {
                    session.stage = "interactions";
                    Persist();
                }
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void Teleport(PlayerController player, Vector2 position)
        {
            player.transform.position = new Vector3(position.x, position.y, player.transform.position.z);
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; }
            Physics2D.SyncTransforms();
        }

        private static void PositionView(PlayerController player, string id)
        {
            Vector2 desired = id switch
            {
                // V4's escarpment reaches y16.7 here; frame it from the actual southern approach.
                "border_north" => new Vector2(0f, 14f),
                "border_south" => new Vector2(0f, -21f),
                "border_west" => new Vector2(-31f, 0f),
                "border_east" => new Vector2(FarmLevel1LayoutContract.CityExitX, FarmLevel1LayoutContract.CityExitY),
                "corner_nw" => new Vector2(-31f, 16f),
                "corner_ne" => new Vector2(31f, 16f),
                "corner_sw" => new Vector2(-30f, -20f),
                "corner_se" => new Vector2(27f, -20f),
                "orchard" => new Vector2(FarmLevel1LayoutContract.OrchardMinX + FarmLevel1LayoutContract.OrchardWidth, FarmLevel1LayoutContract.OrchardMinY + 1f),
                "pasture" => new Vector2(FarmLevel1LayoutContract.PastureMinX + FarmLevel1LayoutContract.PastureWidth + 1f, FarmLevel1LayoutContract.PastureMinY + 2f),
                "bridge" => FarmSceneCompositionContract.BridgePassageProbe,
                "cultivation" => new Vector2(FarmLevel1LayoutContract.CropFieldMinX + FarmLevel1LayoutContract.CropFieldWidth / 2f, FarmLevel1LayoutContract.CropFieldMinY + FarmLevel1LayoutContract.CropFieldHeight / 2f),
                "homestead" => new Vector2(FarmLevel1LayoutContract.HouseStartX, FarmLevel1LayoutContract.HouseMinY - 1f),
                "forest" => new Vector2(FarmLevel1LayoutContract.FonteAnchorX, FarmLevel1LayoutContract.FonteAnchorY - 1.3f),
                "mountain" => new Vector2(FarmLevel1LayoutContract.SpawnFromCaveX, FarmLevel1LayoutContract.SpawnFromCaveY),
                "animals" => new Vector2(FarmLevel1LayoutContract.AnimalBuildingsMinX + FarmLevel1LayoutContract.AnimalBuildingsWidth / 2f, FarmLevel1LayoutContract.AnimalBuildingsMinY - 1f),
                "lake" => new Vector2(FarmLevel1LayoutContract.FishingSpotX, FarmLevel1LayoutContract.FishingSpotY),
                _ => throw new InvalidOperationException("Unknown capture view: " + id)
            };
            var collider = player.GetComponentsInChildren<Collider2D>().Single(c => !c.isTrigger && c.enabled);
            var original = player.transform.position;
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(collider.gameObject.layer));
            var overlaps = new Collider2D[64];
            // Use the true collider and its offset; nearby probes are framing only.
            foreach (var offset in new[] { Vector2.zero, Vector2.left * 0.5f, Vector2.right * 0.5f,
                Vector2.up * 0.5f, Vector2.down * 0.5f, Vector2.left, Vector2.right, Vector2.up, Vector2.down })
            {
                if (id == "lake" && offset != Vector2.zero) break; // Require the authored deck, never fall back onto shore.
                Teleport(player, desired + offset);
                int count = collider.OverlapCollider(filter, overlaps);
                if (count >= overlaps.Length) continue;
                bool blocked = false;
                for (int i = 0; i < count; i++) if (!overlaps[i].transform.IsChildOf(player.transform)) { blocked = true; break; }
                if (!blocked) return;
            }
            Teleport(player, original);
            throw new InvalidOperationException("All framing probes obstructed for " + id + " near " + desired);
        }

        private static void Capture(PlayerController player, UnityEngine.Camera camera)
        {
            string id = ViewIds[session.views.Count];
            int width = camera.pixelWidth, height = camera.pixelHeight;
            if (width <= 0 || height <= 0) throw new InvalidOperationException("Gameplay camera has no valid render dimensions.");
            var priorTarget = camera.targetTexture;
            var priorActive = RenderTexture.active;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = null;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                string path = Path.Combine(OutputDirectory, id + ".png");
                File.WriteAllBytes(path, texture.EncodeToPNG());
                var measures = new List<SpriteMeasure>();
                AddSpriteMeasures(measures, player.GetComponentsInChildren<SpriteRenderer>(), camera, width, height);
                if (measures.Count == 0) throw new InvalidOperationException("Player has no visible SpriteRenderer.");
                foreach (var bin in Components<CindarsHope.Farm.Shipping.ShippingBinInteractable>(player.gameObject.scene))
                {
                    AddSpriteMeasures(measures, bin.GetComponentsInChildren<SpriteRenderer>(), camera, width, height);
                }
                session.views.Add(new View
                {
                    id = id, utc = DateTime.UtcNow.ToString("O"), path = path, cameraObjectPath = ObjectPath(camera.transform),
                    playerPosition = player.transform.position, cameraPosition = camera.transform.position,
                    transparencySortMode = camera.transparencySortMode.ToString(), transparencySortAxis = camera.transparencySortAxis,
                    orthographicSize = camera.orthographicSize, cameraAspect = camera.aspect, width = width, height = height, sprites = measures.ToArray()
                });
            }
            finally
            {
                camera.targetTexture = priorTarget;
                RenderTexture.active = priorActive;
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void AddSpriteMeasures(List<SpriteMeasure> measures, IEnumerable<SpriteRenderer> renderers,
            UnityEngine.Camera camera, int width, int height)
        {
            foreach (var renderer in renderers)
            {
                var sprite = renderer.sprite;
                if (sprite == null || !renderer.enabled) continue;
                var bounds = renderer.bounds;
                var min = camera.WorldToViewportPoint(bounds.min);
                var max = camera.WorldToViewportPoint(bounds.max);
                var scale = renderer.GetComponentInParent<VisualScaleApplicator>();
                measures.Add(new SpriteMeasure
                {
                    objectPath = ObjectPath(renderer.transform), spritePath = AssetDatabase.GetAssetPath(sprite), spriteName = sprite.name,
                    textureWidth = sprite.texture.width, textureHeight = sprite.texture.height, pixelsPerUnit = sprite.pixelsPerUnit,
                    spriteRect = sprite.rect, localScale = renderer.transform.localScale, lossyScale = renderer.transform.lossyScale,
                    scaleRootLossyScale = scale != null ? scale.transform.lossyScale : renderer.transform.root.lossyScale,
                    scaleProfilePath = scale != null && scale.Profile != null ? AssetDatabase.GetAssetPath(scale.Profile) : "",
                    spriteWorldBoundsCenter = bounds.center, spriteWorldBoundsSize = bounds.size,
                    projectedPixels = new Vector2(Mathf.Abs(max.x - min.x) * width, Mathf.Abs(max.y - min.y) * height)
                });
            }
        }

        private static void DisposeContact()
        {
            if (contactProbe == null) return;
            session.contact = contactProbe.Result;
            contactProbe.Dispose();
            contactProbe = null;
        }

        private static void DisposeAmbient()
        {
            if (ambientProbe == null) return;
            session.ambient = ambientProbe.Result;
            ambientProbe.Dispose();
            ambientProbe = null;
        }

        private static void DisposeAnimalMotion()
        {
            if (animalMotionProbe == null) return;
            session.animalMotion = animalMotionProbe.Result;
            animalMotionProbe.Dispose();
            animalMotionProbe = null;
        }

        private static void DisposeHerdMotion()
        {
            if (herdMotionProbe == null) return;
            session.herdMotion = herdMotionProbe.Result;
            herdMotionProbe.Dispose();
            herdMotionProbe = null;
        }

        private static void Fail(string reason)
        {
            if (session.contactOnlyRequested && session.contact != null)
            {
                session.contact.status = "FAIL";
                session.contact.reason = reason;
            }
            DisposeContact();
            if (session.ambientOnlyRequested && session.ambient != null)
            {
                session.ambient.status = "FAIL";
                session.ambient.reason = reason;
            }
            DisposeAmbient();
            if (session.animalMotionOnlyRequested && session.animalMotion != null)
            {
                session.animalMotion.status = "FAIL";
                session.animalMotion.reason = reason;
            }
            DisposeAnimalMotion();
            if (session.herdMotionOnlyRequested && session.herdMotion != null)
            {
                session.herdMotion.status = "FAIL";
                session.herdMotion.reason = reason;
            }
            DisposeHerdMotion();
            if (session.motionRequested)
            {
                session.motionResultStatus = "FAIL";
                if (session.motion != null && session.motion.status == "RUNNING")
                { session.motion.status = "FAIL"; session.motion.reason = reason; }
            }
            motionProbe?.Dispose();
            motionProbe = null;
            interactionProbe?.Dispose();
            interactionProbe = null;
            session.failure = reason;
            session.status = "FAIL";
            session.stage = "exiting";
            Persist();
            if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.ExitPlaymode();
            else Complete();
        }

        private static void Complete()
        {
            if (session == null) return;
            // Wait for the real Edit Mode transition before reopening the scene.
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            bool pass = false;
            try
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                session.sceneSha256After = Hash(ScenePath);
                session.saveHashesAfter = HashFiles(session.saveRoot);
                bool unchanged = session.sceneSha256Before == session.sceneSha256After &&
                    JsonUtility.ToJson(new HashEnvelope { files = session.saveHashesBefore }) == JsonUtility.ToJson(new HashEnvelope { files = session.saveHashesAfter });
                if (session.herdMotionOnlyRequested)
                {
                    bool evidenceValid = session.herdMotionEvaluated && session.herdMotion != null &&
                        session.herdMotion.status == "PASS" && session.herdMotion.frames.Count > 0 &&
                        session.herdMotion.frames.All(frame => File.Exists(frame.path) && new FileInfo(frame.path).Length > 0) &&
                        session.herdMotion.contextFrames.Count == 4 &&
                        session.herdMotion.contextFrames.All(path => File.Exists(path) && new FileInfo(path).Length > 0);
                    pass = unchanged && string.IsNullOrEmpty(session.failure) && session.runtimeErrors.Count == 0 && evidenceValid;
                    if (!unchanged) session.failure = "Scene or persistent files changed; inspect before/after hashes.";
                    if (session.runtimeErrors.Count > 0 && string.IsNullOrEmpty(session.failure))
                        session.failure = "Runtime errors occurred; inspect runtimeErrors.";
                    if (!evidenceValid && string.IsNullOrEmpty(session.failure))
                        session.failure = "Requested herd motion proof failed or absent; inspect herdMotion.";
                    if (unchanged) Debug.Log("Persistent files unchanged: PASS");
                    return;
                }
                if (session.animalMotionOnlyRequested)
                {
                    bool evidenceValid = session.animalMotionEvaluated && session.animalMotion != null &&
                        session.animalMotion.status == "PASS" && session.animalMotion.samples.Count > 0 &&
                        session.animalMotion.samples.All(sample => File.Exists(sample.path) && new FileInfo(sample.path).Length > 0) &&
                        session.animalMotion.contextFrames.Count == 3 &&
                        session.animalMotion.contextFrames.All(path => File.Exists(path) && new FileInfo(path).Length > 0);
                    pass = unchanged && string.IsNullOrEmpty(session.failure) && session.runtimeErrors.Count == 0 && evidenceValid;
                    if (!unchanged) session.failure = "Scene or persistent files changed; inspect before/after hashes.";
                    if (session.runtimeErrors.Count > 0 && string.IsNullOrEmpty(session.failure))
                        session.failure = "Runtime errors occurred; inspect runtimeErrors.";
                    if (!evidenceValid && string.IsNullOrEmpty(session.failure))
                        session.failure = "Requested animal motion proof failed or absent; inspect animalMotion.";
                    if (unchanged) Debug.Log("Persistent files unchanged: PASS");
                    return;
                }
                if (session.contactOnlyRequested)
                {
                    bool evidenceValid = session.contactEvaluated && session.contact != null &&
                        session.contact.status == "PASS" && session.contact.samples.Count > 0 &&
                        session.contact.samples.All(sample => File.Exists(sample.path) && new FileInfo(sample.path).Length > 0);
                    pass = unchanged && string.IsNullOrEmpty(session.failure) && session.runtimeErrors.Count == 0 && evidenceValid;
                    if (!unchanged) session.failure = "Scene or persistent files changed; inspect before/after hashes.";
                    if (session.runtimeErrors.Count > 0 && string.IsNullOrEmpty(session.failure))
                        session.failure = "Runtime errors occurred; inspect runtimeErrors.";
                    if (!evidenceValid && string.IsNullOrEmpty(session.failure))
                        session.failure = "Requested contact playback proof failed or absent; inspect contact.";
                    if (unchanged) Debug.Log("Persistent files unchanged: PASS");
                    return; // The existing finally persists metadata and exits with pass/fail.
                }
                if (session.ambientOnlyRequested)
                {
                    bool evidenceValid = session.ambientEvaluated && session.ambient != null &&
                        session.ambient.status == "PASS" && session.ambient.samples.Count > 0 &&
                        session.ambient.samples.All(sample => File.Exists(sample.path) && new FileInfo(sample.path).Length > 0);
                    pass = unchanged && string.IsNullOrEmpty(session.failure) && session.runtimeErrors.Count == 0 && evidenceValid;
                    if (!unchanged) session.failure = "Scene or persistent files changed; inspect before/after hashes.";
                    if (session.runtimeErrors.Count > 0 && string.IsNullOrEmpty(session.failure))
                        session.failure = "Runtime errors occurred; inspect runtimeErrors.";
                    if (!evidenceValid && string.IsNullOrEmpty(session.failure))
                        session.failure = "Requested ambient playback proof failed or absent; inspect ambient.";
                    if (unchanged) Debug.Log("Persistent files unchanged: PASS");
                    return; // The existing finally persists metadata and exits with pass/fail.
                }
                bool sortingValid = session.views.Count == ViewIds.Length && session.views.All(view =>
                    view.transparencySortMode == TransparencySortMode.CustomAxis.ToString() &&
                    (view.transparencySortAxis - Vector3.up).sqrMagnitude < 0.000001f);
                pass = sortingValid && unchanged && string.IsNullOrEmpty(session.failure) && session.runtimeErrors.Count == 0 && session.views.Count == ViewIds.Length &&
                    session.physicalRoutesEvaluated && session.physicalRoutes != null && session.physicalRoutes.status == "PASS" &&
                    session.interactionSelectionEvaluated && session.interactionSelection != null && session.interactionSelection.status == "PASS" &&
                    (!session.motionRequested || (session.motionEvaluated && session.motion != null &&
                        session.motion.status == "PASS" && session.motion.samples.Count > 0 &&
                        session.motion.samples.All(sample => File.Exists(sample.path) && new FileInfo(sample.path).Length > 0))) &&
                    session.views.All(view => File.Exists(view.path) && new FileInfo(view.path).Length > 0);
                if (!sortingValid && string.IsNullOrEmpty(session.failure)) session.failure = "Farm camera sorting is not CustomAxis/Y in all eight live views; inspect transparencySortMode/Axis.";
                if (!unchanged) session.failure = "Scene or persistent files changed; inspect before/after hashes.";
                if (session.runtimeErrors.Count > 0 && string.IsNullOrEmpty(session.failure)) session.failure = "Runtime errors occurred; inspect runtimeErrors.";
                if ((!session.physicalRoutesEvaluated || session.physicalRoutes == null || session.physicalRoutes.status != "PASS") && string.IsNullOrEmpty(session.failure)) session.failure = "Physical route proof failed or absent; inspect physicalRoutes.";
                if ((!session.interactionSelectionEvaluated || session.interactionSelection == null || session.interactionSelection.status != "PASS") && string.IsNullOrEmpty(session.failure)) session.failure = "Real interaction selection failed or absent; inspect interactionSelection.";
                if (session.motionRequested && (!session.motionEvaluated || session.motion == null || session.motion.status != "PASS"))
                {
                    session.motionResultStatus = "FAIL";
                    if (string.IsNullOrEmpty(session.failure)) session.failure = "Requested door/water motion proof failed or absent; inspect motion.";
                }
                if (unchanged) Debug.Log("Persistent files unchanged: PASS");
            }
            catch (Exception error) { session.failure = error.ToString(); }
            finally
            {
                session.status = pass ? "PASS" : "FAIL";
                session.finishedUtc = DateTime.UtcNow.ToString("O");
                EditorApplication.update -= Tick;
                EditorApplication.playModeStateChanged -= OnPlayState;
                Application.logMessageReceived -= OnLog;
                SessionState.EraseString(StateKey);
                File.WriteAllText(Path.Combine(OutputDirectory, "capture-metadata.json"), JsonUtility.ToJson(session, true));
                if (pass) Debug.Log(session.herdMotionOnlyRequested ? "Farm herd motion capture: PASS" : session.animalMotionOnlyRequested ? "Farm animal motion capture: PASS" : session.contactOnlyRequested ? "Farm contact capture: PASS" : session.ambientOnlyRequested ? "Farm ambient playback: PASS" :
                    "Farm gameplay capture: PASS (" + ViewIds.Length + "/" + ViewIds.Length + ")");
                else Debug.LogError("Farm gameplay capture: FAIL — " + session.failure);
                session = null;
                if (Application.isBatchMode) EditorApplication.Exit(pass ? 0 : 1);
            }
        }

        [Serializable] private sealed class HashEnvelope { public FileHash[] files; }
        private static void Persist() => SessionState.SetString(StateKey, JsonUtility.ToJson(session));
        private static string Hash(string path)
        {
            using var stream = File.OpenRead(path);
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        private static FileHash[] HashFiles(string root) => Directory.Exists(root)
            ? Directory.GetFiles(root, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => new FileHash { path = path, sha256 = Hash(path) }).ToArray()
            : Array.Empty<FileHash>();
        private static IEnumerable<T> Components<T>(Scene scene) where T : Component => scene.IsValid()
            ? scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)) : Enumerable.Empty<T>();
        private static string ObjectPath(Transform target) => target.parent == null ? target.name : ObjectPath(target.parent) + "/" + target.name;
    }
}
