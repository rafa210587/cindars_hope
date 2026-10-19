using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CindarsHope.Camera;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.Editor.Validation;
using CindarsHope.NPC;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.World;
using CindarsHope.World.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Dev
{
    /// <summary>Domain-reload-safe live Town evidence; memory-only framing, no gameplay input or saving.</summary>
    [InitializeOnLoad]
    public static class TownKeyartPlayModeCapture
    {
        private const string StateKey = "CindarsHope.TownKeyartPlayModeCapture";
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string ExtraNpcId = "npc_vaalara_wanderer_01";
        // Use a fully featured house with a canonical resident for the autonomous-door scenario.
        // The resident is ordered to walk from its live position through the authored approach;
        // no actor is teleported or disabled for this proof.
        private const string HouseId = "House_CarvalhoTorto";
        private const string ResidentNpcId = "npc_hund";
        private const int Width = 640, Height = 480;
        private const int OverviewCount = 8;
        private const double TimeoutSeconds = 300, SettleSeconds = 2, HousePhaseTimeoutSeconds = 20;
        private static readonly string[] ViewIds = { "spawn", "plaza", "temple", "watermill", "market", "south_gate", "obstacle_front", "obstacle_behind", "house_exterior_closed", "house_exterior_open", "house_interior_revealed", "house_exit_covered" };
        private static Session session;
        private static bool positioned;
        private static double settleStarted;
        private static int stableUpdates;
        private static Vector3 previousCamera;
        private static HouseRig houseRig;
        private static bool doorTransitionPending;
        private static bool pendingDoorDesiredOpen;
        private static string pendingDoorPhase;
        private static bool pendingDoorBeforeOpen;
        private static List<string> pendingDoorFeedback;

        [Serializable] private sealed class FileHash { public string path, sha256; }
        [Serializable] private sealed class SceneRecord { public string path; public bool loaded, active; }
        [Serializable] private sealed class SpriteMeasure
        {
            public string objectPath, spritePath, spriteName;
            public float pixelsPerUnit, rendererAlpha;
            public bool rendererEnabled, activeInHierarchy, builtinPlaceholder, uiSprite, missingAssetPath;
            public Rect spriteRect, opaquePixelRect;
            public Vector3 localScale, lossyScale, worldBoundsSize;
            public Vector2 opaqueWorldSize, opaqueProjectedPixels;
        }
        [Serializable] private sealed class NpcMeasure
        {
            public string id, objectPath;
            public string[] controllerTypes;
            public bool active, hasPlaceholderSprite, hasVisiblePlaceholderSprite;
            public Vector3 position, lossyScale;
            public SpriteMeasure[] sprites;
        }
        [Serializable] private sealed class RendererState
        {
            public string objectPath, spritePath, spriteName;
            public bool enabled, active;
            public float alpha;
        }
        [Serializable] private sealed class HouseState
        {
            public string stage;
            public bool doorOpen, blockerEnabled, leafEnabled, valid;
            public int playerRoofOverlapCount;
            public float fixedTime, settledFixedSeconds;
            public RendererState[] roofs, interiors;
        }
        [Serializable] private sealed class DoorAction
        {
            public string phase;
            public bool beforeOpen, afterOpen;
            public string[] feedback;
        }
        [Serializable] private sealed class HousePhase
        {
            public string id, status = "FAIL", reason;
            public HouseState state;
            public TownKeyartLivePhysicsProbe.DoorSweep sweep;
        }
        [Serializable] private sealed class HouseEvidence
        {
            public string houseId = HouseId, status = "NOT_RUN", reason, restorationReason;
            public string method = "HouseDoorInteractable.Interact(player) is called directly and its synchronous feedback is observed. Real player collider casts measure the same portal segment closed/open. Teleport framing is followed by normal Play Mode physics ticks; RoofReveal alpha/interior enabled state is observed, never assigned or invoked privately. This does not prove interaction selection, E input, walking, animation fluency or human visual acceptance. Door state is polled each Editor update; two autonomous changes between samples cannot be excluded.";
            public bool initialized, initialOpen, expectedOpen, unstable, restored;
            public Vector2 originalPlayer, originalVelocity, outside, inside;
            public float phaseFixedStart, restoreFixedStart;
            public double restoreStarted;
            public HouseState originalState, restoredState;
            public List<HousePhase> phases = new List<HousePhase>();
            public List<DoorAction> actions = new List<DoorAction>();
            public bool npcAutoOpenObserved;
            public string npcAutoOpenStatus = "NOT_RUN", npcAutoOpenReason;
            public Vector2 residentStart, residentDoorApproach, residentExitTarget;
        }
        private sealed class HouseRig
        {
            internal HouseDoorInteractable door;
            internal Collider2D blocker;
            internal SpriteRenderer leaf;
            internal RoofRevealController roof;
            internal BoxCollider2D trigger;
            internal SpriteRenderer[] roofs, interiors;
            internal float coveredAlpha, revealedAlpha;
            internal bool animatedPresentation;
            internal NpcDweller resident;
            internal NpcWanderer residentWanderer;
        }
        [Serializable] private sealed class View
        {
            public string id, status = "PASS", reason, utc, path, cameraPath, sortingMode;
            public int width = Width, height = Height;
            public Vector3 requestedPosition, playerPosition, cameraPosition, sortAxis;
            public float orthographicSize, renderAspect, originalAspect;
            public SpriteMeasure[] playerSprites;
        }
        [Serializable] private sealed class ScaleEvidence
        {
            public string status = "NOT_RUN", generationReportPath, generationReportHash;
            public string method = "Read-only comparison of all 30 actors after normal Awake against the generator's global post-Awake baseline and the saved Town scene. World shape offsets are relative to the actor origin to permit normal NPC movement. No scale/profile/collider correction is applied by this capture.";
            public string transitionIsolation = "NOT_RUN: hashes protect shared profile files, but Farm-to-Town-to-Farm behavior requires a separate real-router session; this house/camera session does not switch scenes during Play Mode.";
            public float maximumWorldShapeDelta;
            public TownKeyartActorScale.ActorState[] originalBaseline, authored, afterAwake;
            public TownKeyartActorScale.FileStamp[] globalsBefore, globalsAfter, contractInputs;
        }
        [Serializable] private sealed class Session
        {
            public string status = "RUNNING", stage = "entering", failure, startedUtc, playStartedUtc, finishedUtc;
            public string output, unityVersion, scenePath = ScenePath, sceneHashBefore, sceneHashAfter, saveRoot;
            public string physicsPath, physicsStatus = "NOT_RUN", obstacleName;
            public Vector2 obstacleFront, obstacleBehind, requested;
            public string limitations = "Eight overview views plus four house phases from the actual CameraFollow2D camera after Awake and settling. Teleports frame only; no walking input, interaction selection, animation fluency or visual acceptance is proven. The house report separately records actual public door calls and automatic physics-trigger reveal observations. Camera.Render excludes ScreenSpaceOverlay UI. Output target640x480 may differ from editor-window aspect; orthographicSize/sorting/actor scale are not overridden. Route physics is a bounded live collider snapshot; work endpoints use1.25u approach radius, doors/mural exact swept endpoints. Active NPC census is a technical gate, not an art-quality gate; builtin/UI/empty-path sprite flags are reported separately.";
            public string saveIsolation = "SaveInput disabled in memory before EnterPlaymode. No save/load calls or scene saves. All player saves recursively hashed before/after; original editor scene setup restored.";
            public int disabledSaveInputs, totalNpcCount, activeNpcCount, npcControllerObjectCount, activeCanonicalNpcCount;
            public bool extraNpcPreserved;
            public List<string> npcIdentityErrors = new List<string>();
            public SceneRecord[] originalSetup;
            public FileHash[] saveHashesBefore, saveHashesAfter;
            public List<View> views = new List<View>();
            public List<string> runtimeErrors = new List<string>();
            public NpcMeasure[] npcs;
            public string[] visiblePlaceholderNpcIds = Array.Empty<string>();
            public string placeholderEvidence = "NOT_MEASURED";
            public HouseEvidence house = new HouseEvidence();
            public ScaleEvidence actorScale = new ScaleEvidence();
            public bool sceneUnchanged, savesUnchanged, setupRestored;
        }

        static TownKeyartPlayModeCapture()
        {
            string saved = SessionState.GetString(StateKey, "");
            if (string.IsNullOrEmpty(saved)) return;
            session = JsonUtility.FromJson<Session>(saved); Attach();
        }

        public static void RunBatch()
        {
            try { Begin(); }
            catch (Exception error)
            {
                Debug.LogException(error);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                else throw;
            }
        }
        public static void BeginBatch() => RunBatch();

        private static void Begin()
        {
            if (session != null) throw new InvalidOperationException("A Town gameplay capture session is already active.");
            doorTransitionPending = false;
            pendingDoorFeedback = null;
            if (Environment.GetCommandLineArgs().Any(a => string.Equals(a, "-quit", StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("RunBatch requires omitting -quit; it exits after Play Mode and evidence completion.");
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Unsaved scene detected; nothing discarded.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Length > 1 && setup.Any(s => string.IsNullOrEmpty(s.path))) throw new InvalidOperationException("Save unnamed scenes before capturing a multi-scene setup.");
            string output = Environment.GetEnvironmentVariable("CINDARS_TOWN_GAMEPLAY_OUTPUT");
            if (string.IsNullOrWhiteSpace(output)) output = "art/town-keyart-rework/evidence/playmode-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            output = Path.GetFullPath(output);
            if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any()) throw new IOException("Evidence directory is not empty; choose a new immutable revision: " + output);
            string saveRoot = Path.Combine(Application.persistentDataPath, "saves");
            string resolvedSaveRoot = Path.GetFullPath(saveRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (output.Equals(resolvedSaveRoot, StringComparison.OrdinalIgnoreCase) || output.StartsWith(resolvedSaveRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Evidence output cannot be placed inside the player's save directory.");
            Directory.CreateDirectory(output);
            session = new Session
            {
                output = output, startedUtc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion,
                sceneHashBefore = Hash(ScenePath), saveRoot = saveRoot, saveHashesBefore = HashFiles(saveRoot),
                originalSetup = setup.Select(s => new SceneRecord { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray()
            };
            Persist(); Attach();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                ReadScaleBaseline(scene);
                foreach (var input in Components<SaveInput>(scene)) { if (input.enabled) session.disabledSaveInputs++; input.enabled = false; }
                if (session.disabledSaveInputs == 0) throw new InvalidOperationException("No enabled SaveInput was found to isolate; inspect the intended Town scene.");
                Persist(); EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void Attach()
        {
            EditorApplication.update -= Tick; EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= OnPlayState; EditorApplication.playModeStateChanged += OnPlayState;
            Application.logMessageReceived -= OnLog; Application.logMessageReceived += OnLog;
        }
        private static void OnLog(string message, string stack, LogType type)
        {
            if (session == null || (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)) return;
            if (session.runtimeErrors.Count < 128) session.runtimeErrors.Add(message + "\n" + stack);
            Persist();
        }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (session == null) return;
            if (state == PlayModeStateChange.EnteredPlayMode) { session.stage = "capturing"; session.playStartedUtc = DateTime.UtcNow.ToString("O"); positioned = false; Persist(); }
            else if (state == PlayModeStateChange.EnteredEditMode) Complete();
        }

        private static void Tick()
        {
            if (session == null) return;
            try
            {
                if ((DateTime.UtcNow - DateTime.Parse(session.startedUtc).ToUniversalTime()).TotalSeconds > TimeoutSeconds && session.stage != "restoring_house")
                {
                    if (session.stage == "exiting")
                    {
                        session.status = "FAIL"; session.failure = "Timeout exiting Play Mode; hashes/setup restoration not proven.";
                        File.WriteAllText(Path.Combine(session.output, "capture-metadata.json"), JsonUtility.ToJson(session, true));
                        if (Application.isBatchMode) EditorApplication.Exit(1);
                        else Detach();
                        return;
                    }
                    Fail("Timeout entering Play Mode or settling gameplay camera."); return;
                }
                if (!EditorApplication.isPlaying || (session.stage != "capturing" && session.stage != "restoring_house")) return;
                var scene = SceneManager.GetSceneByPath(ScenePath);
                var player = Components<PlayerController>(scene).SingleOrDefault();
                if (session.stage == "restoring_house") { TickHouseRestoration(player); return; }
                var camera = Components<UnityEngine.Camera>(scene).SingleOrDefault(c => c.isActiveAndEnabled && c.CompareTag("MainCamera"));
                if (player == null || camera == null) return;
                if (!camera.orthographic || camera.GetComponent<CameraFollow2D>() == null) throw new InvalidOperationException("Expected actual orthographic Town CameraFollow2D camera.");
                if (Components<SaveInput>(scene).Any(i => i.isActiveAndEnabled)) throw new InvalidOperationException("SaveInput re-enabled during capture.");
                if (session.views.Count >= OverviewCount) { TickHouse(player, camera); return; }
                if (!positioned)
                {
                    string id = ViewIds[session.views.Count];
                    if (id != "spawn" && !PositionView(player, id))
                    {
                        session.views.Add(new View { id = id, status = "FAIL", reason = "Framing candidates obstructed; no incorrect replacement view captured.", requestedPosition = session.requested });
                        Persist(); FinishOrContinue(); return;
                    }
                    if (id == "spawn") session.requested = player.transform.position;
                    settleStarted = EditorApplication.timeSinceStartup; stableUpdates = 0; previousCamera = camera.transform.position; positioned = true; return;
                }
                stableUpdates = (camera.transform.position - previousCamera).sqrMagnitude < .000001f ? stableUpdates + 1 : 0;
                previousCamera = camera.transform.position;
                if (EditorApplication.timeSinceStartup - settleStarted < SettleSeconds || stableUpdates < 8) return;
                Capture(player, camera);
                if (session.views.Count == 1)
                {
                    MeasureLiveScale(scene);
                    session.npcs = MeasureNpcs(scene, camera);
                    session.totalNpcCount = session.npcs.Length; session.activeNpcCount = session.npcs.Count(n => n.active);
                    session.visiblePlaceholderNpcIds = session.npcs.Where(n => n.hasVisiblePlaceholderSprite).Select(n => n.id).ToArray();
                    session.placeholderEvidence = session.visiblePlaceholderNpcIds.Length > 0 ? "FLAGGED_NOT_VISUAL_ACCEPTANCE" : "NO_VISIBLE_BUILTIN_PLACEHOLDER_DETECTED_NOT_VISUAL_ACCEPTANCE";
                    if (session.visiblePlaceholderNpcIds.Length > 0) Debug.LogWarning("[TownKeyartPlayMode] NPC sprites still provisional after Awake: " + string.Join(", ", session.visiblePlaceholderNpcIds));
                }
                // Run after all overview views have settled (~16s), allowing the real schedule/door
                // lifecycle to move residents through their approach points. No actor is disabled.
                if (session.views.Count == OverviewCount)
                {
                    var physics = TownKeyartLivePhysicsProbe.Evaluate(player);
                    session.physicsPath = Path.Combine(session.output, "live-physics.json");
                    File.WriteAllText(session.physicsPath, JsonUtility.ToJson(physics, true)); session.physicsStatus = physics.status;
                    Debug.Log("[TownKeyartPlayMode] Live physics " + physics.status + ": " + physics.reason);
                }
                positioned = false; Persist(); FinishOrContinue();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void FinishOrContinue()
        {
            if (session.views.Count < ViewIds.Length) return;
            BeginHouseRestoration();
        }

        private static void ReadScaleBaseline(Scene scene)
        {
            string path = Environment.GetEnvironmentVariable("CINDARS_TOWN_SCALE_BASELINE_REPORT");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new InvalidOperationException("CA11 requires CINDARS_TOWN_SCALE_BASELINE_REPORT pointing to the fresh PASS report from TownKeyartActorScale.Apply; no inferred pre-pilot baseline is accepted.");
            path = Path.GetFullPath(path);
            var report = JsonUtility.FromJson<TownKeyartActorScale.Report>(File.ReadAllText(path));
            if (report == null || report.status != "PASS" || report.factor != TownKeyartActorScale.Factor || report.actors == null || report.actors.Count != 30 ||
                report.actors.Any(a => a == null || a.status != "PASS" || a.expectedOriginalAfterAwake == null || a.after == null) || report.actors.Select(a => a.id).Distinct().Count() != 30)
                throw new InvalidOperationException("Scale baseline is incomplete, idempotent-only or incompatible; regenerate Town with a fresh report.");
            var proof = session.actorScale;
            proof.generationReportPath = path; proof.generationReportHash = Hash(path);
            proof.globalsBefore = TownKeyartActorScale.GlobalStamps(); proof.contractInputs = TownKeyartActorScale.ContractStamps();
            TownKeyartActorScale.RequireSameStamps(report.globalsBefore, report.globalsAfter);
            TownKeyartActorScale.RequireSameStamps(report.globalsAfter, proof.globalsBefore);
            TownKeyartActorScale.RequireSameStamps(report.contractInputs, proof.contractInputs);
            proof.originalBaseline = report.actors.Select(a => a.expectedOriginalAfterAwake).OrderBy(a => a.id, StringComparer.Ordinal).ToArray();
            proof.authored = TownKeyartActorScale.Observe(scene);
            foreach (var actor in proof.authored)
            {
                var recorded = report.actors.Single(a => a.id == actor.id).after;
                if (recorded.profilePath != actor.profilePath || recorded.profileId != actor.profileId || recorded.profileVisualScale != actor.profileVisualScale || recorded.profileColliderScale != actor.profileColliderScale)
                    throw new InvalidOperationException("Saved Town profile does not match generation evidence: " + actor.id);
                TownKeyartActorScale.ComparePhysics(recorded, actor);
            }
            proof.status = "AWAITING_AWAKE";
        }

        private static void MeasureLiveScale(Scene scene)
        {
            var proof = session.actorScale;
            proof.afterAwake = TownKeyartActorScale.Observe(scene);
            if (proof.authored == null || proof.originalBaseline == null || proof.authored.Length != 30 || proof.afterAwake.Length != 30 || proof.originalBaseline.Length != 30)
                throw new InvalidOperationException("Scale lifecycle evidence does not contain the complete roster.");
            foreach (var live in proof.afterAwake)
            {
                var authored = proof.authored.Single(a => a.id == live.id);
                var original = proof.originalBaseline.Single(a => a.id == live.id);
                if (authored.profilePath != live.profilePath || authored.profileId != live.profileId || live.profileColliderScale != 0 ||
                    Mathf.Abs(live.profileVisualScale - original.profileVisualScale * TownKeyartActorScale.Factor) > .0001f)
                    throw new InvalidOperationException("Actor profile changed on Awake or does not use the approved Town factor: " + live.id);
                proof.maximumWorldShapeDelta = Mathf.Max(proof.maximumWorldShapeDelta, TownKeyartActorScale.ComparePhysics(authored, live), TownKeyartActorScale.ComparePhysics(original, live));
            }
            TownKeyartActorScale.RequireSameStamps(proof.contractInputs, TownKeyartActorScale.ContractStamps());
            proof.status = "PASS";
        }

        private static void TickHouse(PlayerController player, UnityEngine.Camera camera)
        {
            if (houseRig == null) houseRig = ReadHouseRig(player.gameObject.scene);
            var proof = session.house;
            if (!proof.initialized) InitializeHouse(player);
            if (proof.npcAutoOpenStatus == "PENDING" || proof.npcAutoOpenStatus == "OBSERVED")
            {
                TickNpcAutoOpen(player);
                return;
            }
            if (!CompletePendingDoorTransition()) return;
            EnsureDoorStable();
            int phase = session.views.Count - OverviewCount;
            string id = ViewIds[session.views.Count];
            if (!positioned)
            {
                bool inside = phase == 2;
                session.requested = inside ? proof.inside : proof.outside;
                if (!TownKeyartLivePhysicsProbe.TryFrame(player, session.requested, true, out _))
                    throw new InvalidOperationException(id + ": exact framing point obstructed; NPCs/colliders were not removed.");
                if (phase == 0 || phase == 3) SetDoorThroughInteraction(player, false, id);
                else if (phase == 1) SetDoorThroughInteraction(player, true, id);
                proof.phaseFixedStart = Time.fixedTime;
                settleStarted = EditorApplication.timeSinceStartup;
                stableUpdates = 0; previousCamera = camera.transform.position; positioned = true; Persist(); return;
            }
            if (EditorApplication.timeSinceStartup - settleStarted > HousePhaseTimeoutSeconds)
                throw new InvalidOperationException(id + ": camera or normal physics ticks did not settle within20 seconds.");
            stableUpdates = (camera.transform.position - previousCamera).sqrMagnitude < .000001f ? stableUpdates + 1 : 0;
            previousCamera = camera.transform.position;
            if (EditorApplication.timeSinceStartup - settleStarted < SettleSeconds || stableUpdates < 8 || Time.fixedTime - proof.phaseFixedStart < Time.fixedDeltaTime * 3f) return;
            var measured = new HousePhase { id = id, state = ReadHouseState(player, id, phase == 2) };
            if (phase == 0 || phase == 1)
                measured.sweep = TownKeyartLivePhysicsProbe.ProbeDoorSegment(player, houseRig.blocker, proof.outside, proof.inside);
            bool expectedOpen = phase == 1 || phase == 2;
            bool valid = measured.state.valid && measured.state.doorOpen == expectedOpen && Vector2.Distance(player.transform.position, session.requested) < .05f;
            if (measured.sweep != null)
            {
                bool queryComplete = measured.sweep.status == "MEASURED" && measured.sweep.sourceClear && measured.sweep.destinationClear;
                valid &= queryComplete && (phase == 0
                    ? measured.sweep.blockerHit && measured.sweep.hitObjects.Length > 0 && measured.sweep.hitObjects.All(path => path == measured.sweep.blockerPath)
                    : measured.sweep.segmentClear && !measured.sweep.blockerHit);
            }
            measured.status = valid ? "PASS" : "FAIL";
            measured.reason = valid ? "Nonempty configured renderers, door state, automatic reveal state and applicable real collider query match this phase." : "State, framing or closed/open collision contract failed; inspect arrays and sweep hits.";
            proof.phases.Add(measured);
            Capture(player, camera);
            session.views[session.views.Count - 1].status = measured.status;
            session.views[session.views.Count - 1].reason = measured.reason;
            positioned = false; Persist();
            if (!valid) { Fail(id + ": " + measured.reason); return; }
            if (phase == 3) { proof.status = "PASS"; proof.reason = "Four house phases observed; restoration remains a separate required gate."; }
            FinishOrContinue();
        }

        private static HouseRig ReadHouseRig(Scene scene)
        {
            var house = Components<Transform>(scene).Single(t => t.name == HouseId);
            var rig = new HouseRig
            {
                door = house.GetComponentsInChildren<HouseDoorInteractable>(true).Single(),
                roof = house.GetComponentsInChildren<RoofRevealController>(true).Single()
            };
            rig.resident = Components<NpcDweller>(scene).SingleOrDefault(dweller => dweller.NpcId == ResidentNpcId);
            rig.residentWanderer = rig.resident == null ? null : rig.resident.GetComponent<NpcWanderer>();
            if (!rig.door.isActiveAndEnabled || !rig.roof.isActiveAndEnabled) throw new InvalidOperationException(HouseId + ": door/reveal component inactive.");
            var doorData = new SerializedObject(rig.door);
            rig.blocker = RequiredProperty(doorData, "_blocker").objectReferenceValue as Collider2D;
            rig.leaf = RequiredProperty(doorData, "_leafRenderer").objectReferenceValue as SpriteRenderer;
            if (rig.blocker == null || rig.blocker.isTrigger || !rig.blocker.gameObject.activeInHierarchy || rig.leaf == null || rig.leaf.sprite == null || !rig.leaf.gameObject.activeInHierarchy)
                throw new InvalidOperationException(HouseId + ": missing/inactive solid blocker or visible-leaf reference.");
            rig.trigger = rig.roof.GetComponents<BoxCollider2D>().Single(c => c.isTrigger && c.enabled);
            var roofData = new SerializedObject(rig.roof);
            rig.roofs = RequiredRenderers(roofData, "_roofRenderers");
            rig.interiors = RequiredRenderers(roofData, "_interiorRenderers");
            rig.coveredAlpha = RequiredProperty(roofData, "_hiddenAlpha").floatValue;
            rig.revealedAlpha = RequiredProperty(roofData, "_revealedAlpha").floatValue;
            var presentationFrames = RequiredProperty(doorData, "_closedToOpenFrames");
            rig.animatedPresentation = presentationFrames.isArray && presentationFrames.arraySize >= 2;
            if (!(rig.coveredAlpha >= .95f && rig.coveredAlpha <= 1f && rig.revealedAlpha >= 0f && rig.revealedAlpha <= .05f))
                throw new InvalidOperationException(HouseId + ": opaque exterior / revealed interior alpha contract not configured.");
            if (rig.roofs.Intersect(rig.interiors).Any()) throw new InvalidOperationException(HouseId + ": renderer belongs to both roof and interior arrays.");
            foreach (var renderer in rig.roofs.Concat(rig.interiors).Append(rig.leaf))
                if (ReadAlpha(renderer.sprite).width <= 0f) throw new InvalidOperationException(ObjectPath(renderer.transform) + ": sprite has no real alpha occupancy.");
            return rig;
        }

        private static SerializedProperty RequiredProperty(SerializedObject data, string name) => data.FindProperty(name) ?? throw new InvalidOperationException(data.targetObject.name + ": required serialized contract missing: " + name);
        private static SpriteRenderer[] RequiredRenderers(SerializedObject data, string name)
        {
            var property = RequiredProperty(data, name);
            if (!property.isArray || property.arraySize == 0) throw new InvalidOperationException(HouseId + ": empty required renderer array " + name);
            var result = new SpriteRenderer[property.arraySize];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = property.GetArrayElementAtIndex(i).objectReferenceValue as SpriteRenderer;
                if (result[i] == null || result[i].sprite == null || !result[i].gameObject.activeInHierarchy)
                    throw new InvalidOperationException(HouseId + ": invalid renderer at " + name + "[" + i + "].");
            }
            if (result.Distinct().Count() != result.Length) throw new InvalidOperationException(HouseId + ": duplicated renderer refs in " + name);
            return result;
        }

        private static void InitializeHouse(PlayerController player)
        {
            var proof = session.house;
            if (!TownCityLayout.TryGetBuilding(HouseId, out var lot)) throw new InvalidOperationException("Missing house lot " + HouseId);
            proof.originalPlayer = player.transform.position;
            var body = player.GetComponent<Rigidbody2D>();
            if (body == null) throw new InvalidOperationException("Real player Rigidbody2D required for house cycle.");
            proof.originalVelocity = body.linearVelocity;
            proof.initialOpen = houseRig.door.IsOpen;
            proof.expectedOpen = proof.initialOpen;
            proof.originalState = ReadHouseState(player, "before_house_cycle", false);
            if (!proof.originalState.valid) throw new InvalidOperationException("House cycle preflight requires consistent covered house and player outside its trigger; no state was forced.");
            proof.initialized = true; proof.status = "RUNNING";
            Vector2 inward = lot.DoorSide == TownDoorSide.North ? Vector2.down : lot.DoorSide == TownDoorSide.West ? Vector2.right : lot.DoorSide == TownDoorSide.East ? Vector2.left : Vector2.up;
            var solid = TownKeyartLivePhysicsProbe.PlayerSolid(player);
            var centerOffset = (Vector2)(solid.bounds.center - player.transform.position);
            float extent = Mathf.Abs(inward.x) * solid.bounds.extents.x + Mathf.Abs(inward.y) * solid.bounds.extents.y;
            var doorCenter = (Vector2)houseRig.blocker.transform.TransformPoint(houseRig.blocker.offset);
            proof.inside = (Vector2)houseRig.trigger.bounds.ClosestPoint(doorCenter) + inward * (extent + .35f) - centerOffset;
            bool exteriorFound = false;
            try
            {
                for (int offset = 0; offset <= 3; offset++)
                {
                    var candidate = (Vector2)lot.DoorApproach - inward * (.5f * offset);
                    if (!TownKeyartLivePhysicsProbe.TryFrame(player, candidate, true, out _) || RoofOverlapCount(player) != 0) continue;
                    proof.outside = candidate; exteriorFound = true; break;
                }
                bool interiorFound = false;
                for (float padding = .35f; padding <= 2.35f; padding += .25f)
                {
                    var candidate = (Vector2)houseRig.trigger.bounds.ClosestPoint(doorCenter) + inward * (extent + padding) - centerOffset;
                    if (!TownKeyartLivePhysicsProbe.TryFrame(player, candidate, true, out _) || RoofOverlapCount(player) == 0) continue;
                    proof.inside = candidate; interiorFound = true; break;
                }
                if (!exteriorFound || !interiorFound)
                    throw new InvalidOperationException("House framing has no clear exterior/interior pair; no collider or NPC was disabled.");
                var fromCenter = proof.outside + centerOffset - doorCenter;
                var toCenter = proof.inside + centerOffset - doorCenter;
                if (Vector2.Dot(fromCenter, inward) >= 0f || Vector2.Dot(toCenter, inward) <= 0f)
                    throw new InvalidOperationException("House sweep endpoints do not straddle the actual door plane.");
            }
            finally { TownKeyartLivePhysicsProbe.SetPosition(player, proof.originalPlayer); body.linearVelocity = proof.originalVelocity; }
            BeginNpcAutoOpen(lot, proof);
            Persist();
        }

        private static void BeginNpcAutoOpen(TownBuildingLot lot, HouseEvidence proof)
        {
            if (houseRig.resident == null || houseRig.residentWanderer == null || !houseRig.resident.gameObject.activeInHierarchy)
                throw new InvalidOperationException($"Resident {ResidentNpcId} with NpcWanderer is required for the autonomous-door proof.");
            proof.residentStart = houseRig.resident.transform.position;
            proof.residentDoorApproach = lot.DoorApproach;
            proof.residentExitTarget = (Vector2)lot.DoorApproach + TownAccessMetrics.Outward(lot.DoorSide) * 2f;
            // Real movement: retain the current position, traverse the authored doorway approach,
            // and enter the same interior target used by the schedule contract.
            houseRig.residentWanderer.SetDestination(lot.Center, lot.DoorApproach, .3f);
            proof.npcAutoOpenStatus = "PENDING";
            proof.npcAutoOpenReason = "Waiting for resident to traverse authored approach and trigger HouseDoorInteractable.";
            settleStarted = EditorApplication.timeSinceStartup;
        }

        private static void TickNpcAutoOpen(PlayerController player)
        {
            var proof = session.house;
            if (proof.npcAutoOpenStatus != "PENDING" && proof.npcAutoOpenStatus != "OBSERVED") return;
            if (proof.npcAutoOpenStatus == "PENDING" && houseRig.door.IsOpen && !houseRig.door.IsAnimating && !houseRig.blocker.enabled)
            {
                proof.npcAutoOpenObserved = true;
                proof.npcAutoOpenStatus = "OBSERVED";
                proof.npcAutoOpenReason = "Resident opened the door through normal Rigidbody2D movement; blocker was released only after the transition.";
                settleStarted = EditorApplication.timeSinceStartup;
                // Ask the same resident to leave through the door. Closing is then owned by the
                // runtime's trigger exit/auto-close policy, not by this capture harness.
                houseRig.residentWanderer.SetDestination(proof.residentExitTarget, proof.residentDoorApproach, .3f);
                Persist();
                return;
            }
            if (proof.npcAutoOpenStatus == "OBSERVED" && !houseRig.door.IsOpen && houseRig.blocker.enabled)
            {
                proof.npcAutoOpenStatus = "PASS";
                proof.npcAutoOpenReason += " Resident exited and the doorway closed/restored naturally.";
                Persist();
            }
            else if (EditorApplication.timeSinceStartup - settleStarted > HousePhaseTimeoutSeconds)
            {
                proof.npcAutoOpenStatus = "FAIL";
                proof.npcAutoOpenReason = "Resident did not complete the authored approach/door transition within the timeout.";
                Fail("NPC autonomous door proof failed: " + proof.npcAutoOpenReason);
            }
        }

        private static int RoofOverlapCount(PlayerController player)
        {
            int count = 0;
            foreach (var collider in player.GetComponentsInChildren<Collider2D>())
            {
                bool recognizedPlayer = collider.CompareTag("Player") || collider.attachedRigidbody != null && collider.attachedRigidbody.CompareTag("Player");
                if (recognizedPlayer && collider.enabled && collider.gameObject.activeInHierarchy && houseRig.trigger.Distance(collider).isOverlapped) count++;
            }
            return count;
        }

        private static HouseState ReadHouseState(PlayerController player, string stage, bool inside)
        {
            if (houseRig == null || houseRig.door == null || houseRig.roof == null || houseRig.blocker == null || houseRig.leaf == null || houseRig.trigger == null)
                throw new InvalidOperationException("Required house references were destroyed during Play Mode.");
            var state = new HouseState
            {
                stage = stage, doorOpen = houseRig.door.IsOpen, blockerEnabled = houseRig.blocker.enabled, leafEnabled = houseRig.leaf.enabled,
                fixedTime = Time.fixedTime, settledFixedSeconds = Time.fixedTime - session.house.phaseFixedStart,
                playerRoofOverlapCount = RoofOverlapCount(player), roofs = RendererStates(houseRig.roofs), interiors = RendererStates(houseRig.interiors)
            };
            float expectedAlpha = inside ? houseRig.revealedAlpha : houseRig.coveredAlpha;
            state.valid = houseRig.door.isActiveAndEnabled && houseRig.roof.isActiveAndEnabled && houseRig.trigger.enabled &&
                state.blockerEnabled == !state.doorOpen && state.leafEnabled == (houseRig.animatedPresentation || !state.doorOpen) &&
                state.roofs.Length > 0 && state.roofs.All(r => r.enabled && r.active && !string.IsNullOrEmpty(r.spriteName) && Mathf.Abs(r.alpha - expectedAlpha) < .01f) &&
                state.interiors.Length > 0 && state.interiors.All(r => r.enabled == inside && r.active && !string.IsNullOrEmpty(r.spriteName) && r.alpha > 0f) &&
                (inside ? state.playerRoofOverlapCount > 0 : state.playerRoofOverlapCount == 0);
            return state;
        }

        private static RendererState[] RendererStates(SpriteRenderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0 || renderers.Any(r => r == null || r.sprite == null))
                throw new InvalidOperationException("House phase has missing/empty renderers; cannot infer PASS.");
            return renderers.Select(r => new RendererState
            {
                objectPath = ObjectPath(r.transform), spriteName = r.sprite.name, spritePath = AssetDatabase.GetAssetPath(r.sprite),
                enabled = r.enabled, active = r.gameObject.activeInHierarchy, alpha = r.color.a
            }).ToArray();
        }

        private static void EnsureDoorStable()
        {
            if (houseRig.door.IsOpen == session.house.expectedOpen) return;
            session.house.unstable = true; session.house.status = "FAIL";
            throw new InvalidOperationException("UNSTABLE: door changed outside the expected public interaction; autonomous NPCs were not suppressed.");
        }

        private static void SetDoorThroughInteraction(PlayerController player, bool desiredOpen, string phase)
        {
            if (houseRig.door.IsOpen == desiredOpen) { session.house.expectedOpen = desiredOpen; return; }
            if (!houseRig.door.CanInteract(player.gameObject)) throw new InvalidOperationException("House door rejected actual player interactor.");
            bool before = houseRig.door.IsOpen;
            var messages = new List<string>();
            using (GameEventBus.Subscribe<PlayerActionFeedbackEvent>(evt => messages.Add(evt.Message)))
                houseRig.door.Interact(player.gameObject);
            session.house.expectedOpen = desiredOpen;
            Physics2D.SyncTransforms();
            if (houseRig.door.IsAnimating)
            {
                doorTransitionPending = true;
                pendingDoorDesiredOpen = desiredOpen;
                pendingDoorPhase = phase;
                pendingDoorBeforeOpen = before;
                pendingDoorFeedback = messages;
                return;
            }
            session.house.actions.Add(new DoorAction { phase = phase, beforeOpen = before, afterOpen = houseRig.door.IsOpen, feedback = messages.ToArray() });
            if (houseRig.door.IsOpen != desiredOpen || !messages.Any(message => !string.IsNullOrWhiteSpace(message)))
                throw new InvalidOperationException("Public door interaction did not produce expected toggle and nonempty feedback event.");
        }

        private static bool CompletePendingDoorTransition()
        {
            if (!doorTransitionPending) return true;
            if (houseRig.door.IsAnimating) return false;
            var feedback = pendingDoorFeedback ?? new List<string>();
            session.house.actions.Add(new DoorAction
            {
                phase = pendingDoorPhase,
                beforeOpen = pendingDoorBeforeOpen,
                afterOpen = houseRig.door.IsOpen,
                feedback = feedback.ToArray()
            });
            doorTransitionPending = false;
            pendingDoorFeedback = null;
            if (houseRig.door.IsOpen != pendingDoorDesiredOpen || !feedback.Any(message => !string.IsNullOrWhiteSpace(message)))
                throw new InvalidOperationException("Public door interaction did not produce expected toggle and nonempty feedback event.");
            return true;
        }

        private static void BeginHouseRestoration()
        {
            if (!EditorApplication.isPlaying || !session.house.initialized) { ExitPlay(); return; }
            try
            {
                var player = Components<PlayerController>(SceneManager.GetSceneByPath(ScenePath)).Single();
                if (houseRig == null) houseRig = ReadHouseRig(player.gameObject.scene);
                TownKeyartLivePhysicsProbe.SetPosition(player, session.house.originalPlayer);
                SetDoorThroughInteraction(player, session.house.initialOpen, "restore_initial_door");
                session.house.restoreFixedStart = Time.fixedTime;
                session.house.restoreStarted = EditorApplication.timeSinceStartup;
                session.stage = "restoring_house"; Persist();
            }
            catch (Exception error)
            {
                session.house.restorationReason = error.ToString(); session.house.status = "FAIL"; ExitPlay();
            }
        }

        private static void TickHouseRestoration(PlayerController player)
        {
            try
            {
                if (EditorApplication.timeSinceStartup - session.house.restoreStarted > HousePhaseTimeoutSeconds)
                    throw new InvalidOperationException("House restoration did not receive normal physics ticks within20 seconds.");
                if (player == null) return;
                if (houseRig == null) houseRig = ReadHouseRig(player.gameObject.scene);
                if (!CompletePendingDoorTransition()) return;
                EnsureDoorStable();
                if (Time.fixedTime - session.house.restoreFixedStart < Time.fixedDeltaTime * 3f) return;
                var restored = ReadHouseState(player, "restored_after_house_cycle", false);
                session.house.restoredState = restored;
                session.house.restored = restored.valid && restored.doorOpen == session.house.initialOpen && Vector2.Distance(player.transform.position, session.house.originalPlayer) < .05f;
                if (!session.house.restored) throw new InvalidOperationException("Initial door/player/covered-house state was not restored naturally.");
                var body = player.GetComponent<Rigidbody2D>();
                if (body == null) throw new InvalidOperationException("Player body disappeared before velocity restoration.");
                body.linearVelocity = session.house.originalVelocity;
                session.house.restorationReason = "Original door state and player position/velocity restored in memory; covered roof/interior state observed after normal physics ticks.";
            }
            catch (Exception error) { session.house.restorationReason = error.ToString(); session.house.status = "FAIL"; }
            ExitPlay();
        }

        private static void ExitPlay()
        {
            session.stage = "exiting"; Persist();
            if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.ExitPlaymode(); else Complete();
        }

        private static bool PositionView(PlayerController player, string id)
        {
            Vector2 desired;
            if (id == "plaza") desired = TownCityLayout.CentralPlazaCenter + Vector2.down * 6f;
            else if (id == "temple" || id == "watermill" || id == "market")
            {
                string house = id == "temple" ? "House_Temple" : id == "watermill" ? "House_Fishery" : "House_MarketHall";
                if (!TownCityLayout.TryGetBuilding(house, out var lot)) throw new InvalidOperationException("Missing framing lot " + house);
                desired = lot.DoorApproach;
            }
            else if (id == "south_gate") desired = Components<SceneSpawnPoint>(player.gameObject.scene).Single(s => s.SpawnId == "town_from_farm").Position;
            else
            {
                if (string.IsNullOrEmpty(session.obstacleName)) SelectObstacle(player);
                desired = id == "obstacle_front" ? session.obstacleFront : session.obstacleBehind;
            }
            session.requested = desired;
            return TownKeyartLivePhysicsProbe.TryFrame(player, desired, id.StartsWith("obstacle_"), out _);
        }

        private static void SelectObstacle(PlayerController player)
        {
            var original = player.transform.position;
            try
            {
                var actor = TownKeyartLivePhysicsProbe.PlayerSolid(player);
                foreach (var tree in Components<BoxCollider2D>(player.gameObject.scene).Where(c => c.name.StartsWith("TownTree_") && !c.isTrigger).OrderBy(c => c.name))
                {
                    float baseClearance = tree.bounds.extents.y + actor.bounds.extents.y;
                    // Dense forest dressing can occupy the first legal pixel row around a tree. Search
                    // farther along the same occlusion axis; both exact physical placements must clear.
                    for (float padding = .25f; padding <= 3f; padding += .25f)
                    {
                        float clearance = baseClearance + padding;
                        var front = (Vector2)tree.bounds.center + Vector2.down * clearance;
                        var behind = (Vector2)tree.bounds.center + Vector2.up * clearance;
                        if (!TownKeyartLivePhysicsProbe.TryFrame(player, front, true, out _) || !TownKeyartLivePhysicsProbe.TryFrame(player, behind, true, out _)) continue;
                        session.obstacleName = ObjectPath(tree.transform); session.obstacleFront = front; session.obstacleBehind = behind; return;
                    }
                }
                throw new InvalidOperationException("No physical tree offers clear paired front/behind framing probes.");
            }
            finally { TownKeyartLivePhysicsProbe.SetPosition(player, original); }
        }

        private static void Capture(PlayerController player, UnityEngine.Camera camera)
        {
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active; float previousAspect = camera.aspect;
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32); Texture2D texture = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0); texture.Apply();
                string id = ViewIds[session.views.Count], path = Path.Combine(session.output, id + ".png");
                if (File.Exists(path)) throw new IOException("Refusing to overwrite live view: " + path);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                session.views.Add(new View
                {
                    id = id, path = path, utc = DateTime.UtcNow.ToString("O"), cameraPath = ObjectPath(camera.transform),
                    requestedPosition = session.requested, playerPosition = player.transform.position, cameraPosition = camera.transform.position,
                    orthographicSize = camera.orthographicSize, originalAspect = previousAspect, renderAspect = camera.aspect,
                    sortingMode = camera.transparencySortMode.ToString(), sortAxis = camera.transparencySortAxis,
                    playerSprites = Measure(player.GetComponentsInChildren<SpriteRenderer>(), camera)
                });
            }
            finally
            {
                camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static SpriteMeasure[] Measure(IEnumerable<SpriteRenderer> renderers, UnityEngine.Camera camera)
        {
            var result = new List<SpriteMeasure>();
            foreach (var renderer in renderers)
            {
                var sprite = renderer.sprite;
                if (sprite == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy || renderer.color.a <= 0f) continue;
                Rect occupied = ReadAlpha(sprite);
                if (occupied.width <= 0f || occupied.height <= 0f) continue;
                var scale = renderer.transform.lossyScale;
                var world = new Vector2(occupied.width / sprite.pixelsPerUnit * Mathf.Abs(scale.x), occupied.height / sprite.pixelsPerUnit * Mathf.Abs(scale.y));
                result.Add(new SpriteMeasure
                {
                    objectPath = ObjectPath(renderer.transform), spritePath = AssetDatabase.GetAssetPath(sprite), spriteName = sprite.name,
                    pixelsPerUnit = sprite.pixelsPerUnit, spriteRect = sprite.rect, opaquePixelRect = occupied,
                    rendererEnabled = renderer.enabled, activeInHierarchy = renderer.gameObject.activeInHierarchy, rendererAlpha = renderer.color.a,
                    builtinPlaceholder = IsPlaceholder(sprite), uiSprite = sprite.name.IndexOf("UISprite", StringComparison.OrdinalIgnoreCase) >= 0,
                    missingAssetPath = string.IsNullOrEmpty(AssetDatabase.GetAssetPath(sprite)),
                    localScale = renderer.transform.localScale, lossyScale = scale, worldBoundsSize = renderer.bounds.size,
                    opaqueWorldSize = world, opaqueProjectedPixels = world * (Height / (2f * camera.orthographicSize))
                });
            }
            return result.ToArray();
        }

        private static NpcMeasure[] MeasureNpcs(Scene scene, UnityEngine.Camera camera)
        {
            // Both concrete controller types represent actors; neither is a proxy for the full roster.
            // Group by GameObject first so a mixed-controller actor is never counted twice.
            var actors = Components<NpcShopController>(scene).Cast<Behaviour>()
                .Concat(Components<NpcController>(scene).Cast<Behaviour>())
                .GroupBy(controller => controller.gameObject).ToArray();
            session.npcControllerObjectCount = actors.Length;
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<NpcMeasure>();
            foreach (var actor in actors)
            {
                string path = ObjectPath(actor.Key.transform);
                var ids = actor.Select(controller => controller is NpcShopController shop
                    ? shop.NpcData != null ? shop.NpcData.NpcId : null
                    : ((NpcController)controller).NpcId).ToArray();
                var distinctIds = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToArray();
                if (ids.Any(string.IsNullOrWhiteSpace) || distinctIds.Length != 1)
                {
                    session.npcIdentityErrors.Add(path + ": missing or conflicting controller IDs (" + string.Join(",", ids) + ").");
                    continue;
                }
                string id = distinctIds[0];
                if (!seenIds.Add(id))
                {
                    session.npcIdentityErrors.Add(path + ": duplicate NPC ID " + id + " on another GameObject; not counted twice.");
                    continue;
                }
                result.Add(new NpcMeasure
                {
                    id = id, objectPath = path, controllerTypes = actor.Select(c => c.GetType().Name).ToArray(),
                    active = actor.All(c => c.isActiveAndEnabled), position = actor.Key.transform.position,
                    lossyScale = actor.Key.transform.lossyScale,
                    sprites = Measure(actor.Key.GetComponentsInChildren<SpriteRenderer>(true), camera)
                });
                var measurement = result[result.Count - 1];
                measurement.hasPlaceholderSprite = actor.Key.GetComponentsInChildren<SpriteRenderer>(true).Any(r => r.sprite != null && IsPlaceholder(r.sprite));
                measurement.hasVisiblePlaceholderSprite = measurement.sprites.Any(s => s.builtinPlaceholder);
            }
            return result.ToArray();
        }

        private static bool IsPlaceholder(Sprite sprite)
        {
            string path = AssetDatabase.GetAssetPath(sprite);
            return string.IsNullOrEmpty(path) || path.IndexOf("unity_builtin_extra", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("unity default resources", StringComparison.OrdinalIgnoreCase) >= 0 || sprite.name.IndexOf("UISprite", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool HasVisibleSprite(NpcMeasure npc) => npc.sprites != null && npc.sprites.Any(sprite =>
            sprite.rendererEnabled && sprite.activeInHierarchy && sprite.rendererAlpha > 0f &&
            sprite.opaquePixelRect.width > 0f && sprite.opaquePixelRect.height > 0f &&
            sprite.opaqueWorldSize.x > 0f && sprite.opaqueWorldSize.y > 0f);

        private static bool ValidateRoster()
        {
            var expected = new HashSet<string>(TownCityLayout.AllNpcPlaces.Select(place => place.NpcId), StringComparer.Ordinal);
            if (session.npcs == null) return false;
            session.activeCanonicalNpcCount = session.npcs.Count(npc => expected.Contains(npc.id) && npc.active && HasVisibleSprite(npc));
            session.extraNpcPreserved = session.npcs.Any(npc => npc.id == ExtraNpcId && npc.active && HasVisibleSprite(npc));
            return expected.Count == 28 && session.activeCanonicalNpcCount == 28 && session.extraNpcPreserved &&
                session.npcIdentityErrors.Count == 0 && session.npcs.Select(npc => npc.id).Distinct(StringComparer.Ordinal).Count() == session.npcs.Length;
        }

        private static Rect ReadAlpha(Sprite sprite)
        {
            var source = sprite.texture; var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false, true);
            try
            {
                Graphics.Blit(source, target); RenderTexture.active = target;
                texture.ReadPixels(sprite.rect, 0, 0); texture.Apply(); var pixels = texture.GetPixels32();
                int minX = texture.width, minY = texture.height, maxX = -1, maxY = -1;
                for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                    if (pixels[y * texture.width + x].a > 0) { minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y); maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y); }
                return maxX < minX ? Rect.zero : new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static void Fail(string reason)
        {
            session.failure = reason; session.status = "FAIL";
            if (session.house.initialized) { session.house.status = "FAIL"; session.house.reason = reason; }
            Persist(); BeginHouseRestoration();
        }

        private static void Complete()
        {
            if (session == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            bool passed = false;
            try
            {
                // Discard only our preflight-authorized memory changes; the scene file is never saved.
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                session.sceneHashAfter = Hash(ScenePath); session.saveHashesAfter = HashFiles(session.saveRoot);
                session.sceneUnchanged = session.sceneHashBefore == session.sceneHashAfter;
                session.savesUnchanged = HashesEqual(session.saveHashesBefore, session.saveHashesAfter);
                RestoreSetup(); session.setupRestored = true;
                session.actorScale.globalsAfter = TownKeyartActorScale.GlobalStamps();
                TownKeyartActorScale.RequireSameStamps(session.actorScale.globalsBefore, session.actorScale.globalsAfter);
                TownKeyartActorScale.RequireSameStamps(session.actorScale.contractInputs, TownKeyartActorScale.ContractStamps());
                bool views = session.views.Count == ViewIds.Length && session.views.All(v => v.status == "PASS" && File.Exists(v.path) && new FileInfo(v.path).Length > 0 && v.playerSprites != null && v.playerSprites.Length > 0);
                bool roster = ValidateRoster();
                bool house = session.house.status == "PASS" && session.house.restored && !session.house.unstable && session.house.phases.Count == 4 && session.house.phases.All(p => p.status == "PASS");
                passed = views && roster && house && session.actorScale.status == "PASS" && session.sceneUnchanged && session.savesUnchanged && session.runtimeErrors.Count == 0 && string.IsNullOrEmpty(session.failure) && session.physicsStatus == "PASS" && File.Exists(session.physicsPath);
                if (!passed && string.IsNullOrEmpty(session.failure)) session.failure = "One or more evidence gates failed: views, roster, house cycle/restoration, scale lifecycle/baseline, runtime errors, unchanged scene/saves/shared profiles, or live physics. Inspect separate reports; no visual or input acceptance is implied.";
            }
            catch (Exception error) { session.failure = error.ToString(); }
            finally
            {
                session.status = passed ? "PASS" : "FAIL"; session.finishedUtc = DateTime.UtcNow.ToString("O");
                Detach(); SessionState.EraseString(StateKey);
                File.WriteAllText(Path.Combine(session.output, "capture-metadata.json"), JsonUtility.ToJson(session, true));
                string message = "[TownKeyartPlayMode] " + session.status + ": " + session.output + (passed ? "" : " — " + session.failure);
                session = null; houseRig = null;
                if (passed) Debug.Log(message); else Debug.LogError(message);
                if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static void RestoreSetup()
        {
            if (session.originalSetup.Length == 0 || session.originalSetup.All(s => string.IsNullOrEmpty(s.path))) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            else EditorSceneManager.RestoreSceneManagerSetup(session.originalSetup.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
        }
        private static void Detach()
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayState; Application.logMessageReceived -= OnLog;
        }
        private static void Persist() => SessionState.SetString(StateKey, JsonUtility.ToJson(session));
        private static string Hash(string path)
        {
            using var stream = File.OpenRead(path); using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }
        private static FileHash[] HashFiles(string root) => Directory.Exists(root) ? Directory.GetFiles(root, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal).Select(p => new FileHash { path = p, sha256 = Hash(p) }).ToArray() : Array.Empty<FileHash>();
        private static bool HashesEqual(FileHash[] a, FileHash[] b) => a.Length == b.Length && a.Zip(b, (x, y) => x.path == y.path && x.sha256 == y.sha256).All(same => same);
        private static IEnumerable<T> Components<T>(Scene scene) where T : Component => scene.IsValid() ? scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)) : Enumerable.Empty<T>();
        private static string ObjectPath(Transform transform) => transform.parent == null ? transform.name : ObjectPath(transform.parent) + "/" + transform.name;
    }
}
