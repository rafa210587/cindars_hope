using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Sequential, live-body evidence for every Town walk-in lot. It never saves the scene or removes NPCs.</summary>
    [InitializeOnLoad]
    public static class TownAllEntriesPlayModeProbe
    {
        private const string StateKey = "CindarsHope.TownAllEntriesPlayModeProbe";
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const float SettleSeconds = .18f;
        private const float PhaseTimeoutSeconds = 8f;
        private const float DoorPreparationTimeoutSeconds = 4f;
        private const float BootTimeoutSeconds = 30f;
        private const int InteriorCaptureWidth = 800;
        private const int InteriorCaptureHeight = 600;
        private static Session session;
        private static double phaseStarted;
        private static TownAllEntriesBodyMotion mover;
        private static InteractionSystem selector;

        [Serializable] private sealed class RendererState { public bool enabled; public float alpha; }
        [Serializable] private sealed class FurnitureEvidence
        {
            public string path, status, reason;
            public bool enabled, trigger, hasVisibleRenderer;
        }
        [Serializable] private sealed class EntryEvidence
        {
            public string houseId, classification, status = "NOT_RUN", reason;
            public bool blockerClosed, blockerStayedDuringAnimation, openedAfterAnimation, closedSweepBlocked, openSweepClear;
            public bool outsideInteractionDidNotReveal, insideRevealed, exitCovered, insideInteractionAvailable, outsideInteractionAvailable;
            public bool occupiedCloseSafe, shopOpenAttempted, closePreparationAttempted;
            public string shopClosedHours = "NOT_RUN: shop-hours service gate is not exercised by this physical-entry probe.";
            public TownKeyartLivePhysicsProbe.DoorSweep closedSweep, openSweep;
            public RendererState roofOutside, roofInside, roofExit;
            public string interiorCapturePath;
            public int pilotWalkWaypoints;
            public bool pilotWalkComplete;
            public List<string> pilotPoseCaptures = new List<string>();
            public List<TownKeyartLivePhysicsProbe.DoorSweep> pilotFurnitureSweeps = new List<TownKeyartLivePhysicsProbe.DoorSweep>();
            public List<FurnitureEvidence> furniture = new List<FurnitureEvidence>();
        }
        [Serializable] private sealed class FileHash { public string path, sha256; }
        [Serializable] private sealed class SceneRecord { public string path; public bool loaded, active; }
        [Serializable] private sealed class Session
        {
            public string status = "RUNNING", stage = "entering", output, failure, startedUtc, finishedUtc;
            public string method = "Runs one selector-chosen public HouseDoorInteractable.Interact call outside and one inside for each of 23 walk-in lots. It polls real Player Collider2D casts/overlaps through the existing TownKeyartLivePhysicsProbe, waits normal Play Mode time for leaf animation and RoofRevealController feet occupancy, and retains all NPCs. Door approaches and portal entry/exit use the real Player Rigidbody2D.MovePosition in FixedUpdate; direct repositioning is limited to clear pre-approach framing/restoration. This is not E-input, human visual, shop-service or NPC-route proof.";
            public string limitations = "AnimalYard is intentionally exterior_only. Shop opening is physical only; closed-hours service behavior remains NOT_RUN until the time/shop integration supplies a controllable clock. Furniture evidence verifies materialized enabled non-trigger collider plus visible renderer; it does not infer missing art or prove every possible approach vector.";
            public int expectedWalkIns = 23, playerNpcBodiesAtStart, index, originalGamePhase;
            public string scope = "ALL_23_WALK_INS", onlyHouse;
            public bool originalPlayerControllerEnabled, clockTouched, sceneUnchanged, savesUnchanged, setupRestored;
            public float originalGamePhaseTimer, phaseFixedStarted;
            public double bootStarted;
            public Vector2 originalPlayerPosition, originalVelocity;
            public string saveRoot, sceneHashBefore, sceneHashAfter;
            public SceneRecord[] originalSetup;
            public FileHash[] saveHashesBefore, saveHashesAfter;
            public List<EntryEvidence> entries = new List<EntryEvidence>();
        }
        private sealed class Rig
        {
            internal TownBuildingLot lot;
            internal Transform house;
            internal HouseDoorInteractable door;
            internal Collider2D blocker, sensor;
            internal RoofRevealController reveal;
            internal BoxCollider2D feet;
            internal SpriteRenderer[] roofs, interiors;
        }
        private static Rig rig;

        static TownAllEntriesPlayModeProbe()
        {
            var json = SessionState.GetString(StateKey, "");
            if (string.IsNullOrEmpty(json)) return;
            session = JsonUtility.FromJson<Session>(json);
            Attach();
        }

        [MenuItem("Cindar's Hope/Validation/Run Town All Entries Live Probe")]
        public static void RunBatch()
        {
            if (session != null) throw new InvalidOperationException("Town all-entry probe is already running.");
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before starting the Town all-entry probe.");
            var onlyHouse = Environment.GetEnvironmentVariable("CINDARS_TOWN_ALL_ENTRIES_HOUSE");
            if (!string.IsNullOrWhiteSpace(onlyHouse) && onlyHouse != "House_GateKeeper")
                throw new ArgumentException("The optional scoped pilot accepts only House_GateKeeper; omit it for the complete 23-entry gate.");
            if (SceneManager.sceneCount > 0 && Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).Any(s => s.isDirty))
                throw new InvalidOperationException("Unsaved scene detected; probe refuses to discard it.");
            var output = Environment.GetEnvironmentVariable("CINDARS_TOWN_ALL_ENTRIES_OUTPUT");
            if (string.IsNullOrWhiteSpace(output)) output = "docs/validation/town_all_entries_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            output = Path.GetFullPath(output);
            if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any()) throw new IOException("Output directory must be new or empty: " + output);
            Directory.CreateDirectory(output);
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Length > 1 && setup.Any(s => string.IsNullOrEmpty(s.path))) throw new InvalidOperationException("Save unnamed scenes before probing a multi-scene setup.");
            var saveRoot = Path.Combine(Application.persistentDataPath, "saves");
            session = new Session { output = output, startedUtc = DateTime.UtcNow.ToString("O"), saveRoot = saveRoot,
                sceneHashBefore = Hash(ScenePath), saveHashesBefore = HashFiles(saveRoot),
                originalSetup = setup.Select(s => new SceneRecord { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() };
            if (!string.IsNullOrWhiteSpace(onlyHouse))
            {
                session.onlyHouse = onlyHouse;
                session.scope = "SCOPED_GATEKEEPER_ONLY";
                session.expectedWalkIns = 1;
                session.method = "SCOPED GateKeeper pilot only (1 lot, not the full 23-entry gate). " + session.method.Replace("each of 23 walk-in lots", "the single selected lot");
            }
            Persist(); Attach();
            try
            {
                var town = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                // Input/save is not driven. Saving is disabled in memory only, as in TownKeyartPlayModeCapture.
                foreach (var input in Components<SaveInput>(town)) input.enabled = false;
                EditorApplication.EnterPlaymode();
            }
            catch (Exception exception) { Fail(exception.ToString()); }
        }

        private static void Attach()
        {
            EditorApplication.update -= Tick; EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= OnPlayState; EditorApplication.playModeStateChanged += OnPlayState;
        }
        private static void OnPlayState(PlayModeStateChange change)
        {
            if (session == null) return;
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                // SceneManager can briefly return an invalid path-scene during the first editor
                // update after EnteredPlayMode. Boot is deliberately outside every house phase.
                session.stage = "boot_wait"; session.bootStarted = EditorApplication.timeSinceStartup; Persist();
            }
            if (change == PlayModeStateChange.EnteredEditMode) Complete();
        }
        private static void Tick()
        {
            if (session == null || !EditorApplication.isPlaying) return;
            try
            {
                var scene = SceneManager.GetSceneByPath(ScenePath);
                if (session.stage == "boot_wait") { WaitForBoot(scene); return; }
                if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("Town scene became unavailable after completed boot.");
                var player = Components<PlayerController>(scene).Single();
                if (session.stage == "next") BeginNext(scene, player);
                else ExecutePhase(player);
            }
            catch (Exception exception)
            {
                if (!RecordLotFailure(exception)) Fail(exception.ToString());
            }
        }

        private static void WaitForBoot(Scene scene)
        {
            var elapsed = EditorApplication.timeSinceStartup - session.bootStarted;
            var valid = scene.IsValid() && scene.isLoaded;
            var players = valid ? Components<PlayerController>(scene).ToArray() : Array.Empty<PlayerController>();
            var clock = GameBootstrap.Instance != null ? GameBootstrap.Instance.GameTimeManager : null;
            if (valid && players.Length == 1 && clock != null)
            {
                session.stage = "next"; phaseStarted = EditorApplication.timeSinceStartup; Persist();
                return;
            }
            if (elapsed > BootTimeoutSeconds)
                throw new TimeoutException("Town boot did not become ready in " + BootTimeoutSeconds + "s; sceneValid=" + valid + "; players=" + players.Length + "; clock=" + (clock != null) + ". No house phase started.");
        }

        private static void BeginNext(Scene scene, PlayerController player)
        {
            if (session.index == 0 && session.entries.Count == 0)
            {
                session.originalPlayerPosition = player.transform.position;
                var body = player.GetComponent<Rigidbody2D>(); session.originalVelocity = body == null ? Vector2.zero : body.linearVelocity;
                session.originalPlayerControllerEnabled = player.enabled; player.enabled = false;
                mover = player.gameObject.AddComponent<TownAllEntriesBodyMotion>();
                mover.Stop();
                selector = player.GetComponentsInChildren<InteractionSystem>(true).Single();
                var clock = GameBootstrap.Instance != null ? GameBootstrap.Instance.GameTimeManager : null;
                if (clock == null) throw new InvalidOperationException("Town live probe requires the real GameTimeManager to set shop hours.");
                session.originalGamePhase = (int)clock.CurrentPhase; session.originalGamePhaseTimer = clock.PhaseTimer;
                clock.RestorePhaseState((int)GamePhaseChangedEvent.GamePhase.Day, clock.CurrentPhaseDurationSeconds * .5f);
                session.clockTouched = true;
                session.playerNpcBodiesAtStart = Components<Collider2D>(scene).Count(c => c.enabled && !c.isTrigger && c.GetComponentInParent<CindarsHope.NPC.NpcController>() != null);
                if (session.playerNpcBodiesAtStart == 0) throw new InvalidOperationException("No live NPC solid bodies found; probe must not declare circulation after removing them.");
                if (!TownKeyartDoorClassification.Validate(out var classificationReason)) throw new InvalidOperationException(classificationReason);
            }
            var lots = TownCityLayout.AllBuildings.Where(l => TownKeyartDoorClassification.For(l.Name) != TownKeyartDoorClassification.ExteriorOnly).ToArray();
            if (lots.Length != 23) throw new InvalidOperationException("Expected 23 walk-in Town lots before scope selection.");
            if (!string.IsNullOrEmpty(session.onlyHouse)) lots = lots.Where(l => l.Name == session.onlyHouse).ToArray();
            if (lots.Length != session.expectedWalkIns) throw new InvalidOperationException("Selected lot count differs from the declared probe scope.");
            if (session.index >= lots.Length) { session.stage = "restoring"; phaseStarted = EditorApplication.timeSinceStartup; Persist(); return; }
            mover.Stop();
            // This is an open-hours entry scenario, not a day-cycle test. Restore noon for each
            // lot so a long capture run cannot reach closing time halfway through a shop exit.
            var entryClock = GameBootstrap.Instance.GameTimeManager;
            entryClock.RestorePhaseState((int)GamePhaseChangedEvent.GamePhase.Day, entryClock.CurrentPhaseDurationSeconds * .5f);
            GameEventBus.Publish(new GameTimeTickEvent(entryClock.PhaseTimer, entryClock.CurrentDay));
            rig = ReadRig(scene, lots[session.index]);
            var entry = new EntryEvidence { houseId = rig.lot.Name, classification = TownKeyartDoorClassification.For(rig.lot.Name), shopOpenAttempted = TownKeyartDoorClassification.For(rig.lot.Name) == TownKeyartDoorClassification.ShopHoursGated };
            session.entries.Add(entry);
            // NPCs remain active. A door they have left open must first close through its public
            // API; if traffic keeps it open, retain a failed/inconclusive gate instead of calling
            // the normal closed-door assertions on an invalid initial state.
            if (rig.door.IsOpen || rig.door.IsAnimating)
            {
                if (rig.door.IsOpen)
                {
                    rig.door.Interact(player.gameObject);
                    entry.closePreparationAttempted = true;
                }
                session.stage = "preparing_closed"; phaseStarted = EditorApplication.timeSinceStartup; Persist(); return;
            }
            StartEntry(player, entry);
        }

        private static void StartEntry(PlayerController player, EntryEvidence entry)
        {
            // DoorApproach is intentionally 1.5u away for navigation; it is outside the 0.45u
            // interaction selection range. Frame a separate, still exterior sensor position.
            // Start clear of the sensor, then enter it through normal physics so the relay
            // registers the door. Teleporting directly inside an overlap can miss Enter.
            var outward = PhysicalOutsideDirection();
            var sensorEdge = Vector2.Distance(DoorwayCenter(), rig.sensor.ClosestPoint(DoorwayCenter() + outward * 10f));
            TownKeyartLivePhysicsProbe.SetPosition(player, DoorwayCenter() + outward * (sensorEdge + 1.5f));
            mover.MoveTo(DoorSelectionOutside());
            entry.roofOutside = State(rig.roofs.First());
            entry.outsideInteractionDidNotReveal = Covered();
            entry.blockerClosed = rig.blocker.enabled && !rig.door.IsOpen;
            entry.closedSweep = TownKeyartLivePhysicsProbe.ProbeDoorSegment(player, rig.blocker, Outside(), Inside());
            entry.closedSweepBlocked = entry.closedSweep.blockerHit && !entry.closedSweep.segmentClear;
            session.stage = "approaching_outside"; phaseStarted = EditorApplication.timeSinceStartup; session.phaseFixedStarted = Time.fixedTime; Persist();
        }

        private static void ExecutePhase(PlayerController player)
        {
            var entry = session.entries[session.entries.Count - 1];
            if (EditorApplication.timeSinceStartup - phaseStarted > PhaseTimeoutSeconds) throw new TimeoutException(entry.houseId + " phase timed out: " + session.stage + "; player=" + player.transform.position + "; target=" + (mover == null ? "none" : mover.Target.ToString()) + "; doorOpen=" + rig.door.IsOpen + "; animating=" + rig.door.IsAnimating + "; blocker=" + rig.blocker.enabled + MotionDiagnostic(player));
            switch (session.stage)
            {
                case "preparing_closed":
                    if (EditorApplication.timeSinceStartup - phaseStarted > DoorPreparationTimeoutSeconds)
                    {
                        entry.status = "FAIL";
                        entry.reason = "INCONCLUSIVE: door remained open while active NPC traffic was retained; closed-door checks were not run.";
                        session.index++; session.stage = "next"; Persist(); return;
                    }
                    if (rig.door.IsAnimating) return;
                    if (rig.door.IsOpen && !entry.closePreparationAttempted)
                    {
                        rig.door.Interact(player.gameObject);
                        entry.closePreparationAttempted = true;
                        return;
                    }
                    if (rig.door.IsOpen) return;
                    StartEntry(player, entry); return;
                case "approaching_outside":
                    // The real selector defines reachable interaction range; a fixed target
                    // near the leaf can lie inside the player's collision clearance.
                    if (!mover.Arrived && !ReferenceEquals(selector.GetCurrentInteractable(), rig.door)) return;
                    mover.Stop();
                    session.stage = "waiting_outside_selection"; phaseStarted = EditorApplication.timeSinceStartup; session.phaseFixedStarted = Time.fixedTime; Persist(); return;
                case "waiting_outside_selection":
                    if (Time.fixedTime - session.phaseFixedStarted < Time.fixedDeltaTime * 2f) return;
                    var outsideSelected = selector.GetCurrentInteractable();
                    entry.outsideInteractionAvailable = ReferenceEquals(outsideSelected, rig.door);
                    if (!entry.outsideInteractionAvailable) throw new InvalidOperationException(entry.houseId + " outside selector did not choose its real door; " + SelectorDiagnostic(player, outsideSelected));
                    outsideSelected.Interact(player.gameObject);
                    entry.blockerStayedDuringAnimation = rig.door.IsAnimating && rig.blocker.enabled;
                    session.stage = "waiting_open"; phaseStarted = EditorApplication.timeSinceStartup; Persist(); return;
                case "waiting_open":
                    if (rig.door.IsAnimating) return;
                    entry.openedAfterAnimation = rig.door.IsOpen && !rig.blocker.enabled;
                    entry.openSweep = TownKeyartLivePhysicsProbe.ProbeDoorSegment(player, rig.blocker, Outside(), Inside());
                    entry.openSweepClear = entry.openSweep.segmentClear && !entry.openSweep.blockerHit;
                    mover.MoveTo(InteriorPoint());
                    phaseStarted = EditorApplication.timeSinceStartup; session.stage = "walking_inside"; Persist(); return;
                case "walking_inside":
                    if (!mover.Arrived) return;
                    phaseStarted = EditorApplication.timeSinceStartup; session.stage = "waiting_inside"; Persist(); return;
                case "waiting_inside":
                    if (EditorApplication.timeSinceStartup - phaseStarted < SettleSeconds) return;
                    entry.roofInside = State(rig.roofs.First()); entry.insideRevealed = Revealed();
                    CaptureInteriorIfRequested(player.gameObject.scene, entry);
                    AuditFurniture(entry);
                    AuditPilotFurniture(player, entry);
                    if (entry.houseId == "House_GateKeeper")
                    {
                        mover.MoveTo((Vector2)rig.house.position + PilotWalkPoints[0]);
                        phaseStarted = EditorApplication.timeSinceStartup; session.stage = "pilot_furniture_walk"; Persist(); return;
                    }
                    // Occupy the authored doorway with the real Player solid then attempt the public close.
                    mover.MoveTo(DoorwayCenter());
                    phaseStarted = EditorApplication.timeSinceStartup; session.stage = "walking_doorway"; Persist(); return;
                case "walking_doorway":
                    if (!mover.Arrived) return;
                    var insideSelected = selector.GetCurrentInteractable();
                    entry.insideInteractionAvailable = ReferenceEquals(insideSelected, rig.door);
                    if (!entry.insideInteractionAvailable) throw new InvalidOperationException(entry.houseId + " inside selector did not choose its real door; " + SelectorDiagnostic(player, insideSelected));
                    insideSelected.Interact(player.gameObject);
                    entry.occupiedCloseSafe = rig.door.IsOpen || rig.door.IsAnimating;
                    mover.MoveTo(InteriorPoint());
                    phaseStarted = EditorApplication.timeSinceStartup; session.stage = "walking_back_inside"; Persist(); return;
                case "pilot_furniture_walk":
                    if (!mover.Arrived) return;
                    if (entry.pilotWalkWaypoints == 1 || entry.pilotWalkWaypoints == 4 || entry.pilotWalkWaypoints == 6)
                        CaptureInteriorIfRequested(player.gameObject.scene, entry, "approach_" + entry.pilotWalkWaypoints);
                    entry.pilotWalkWaypoints++;
                    if (entry.pilotWalkWaypoints < PilotWalkPoints.Length)
                        mover.MoveTo((Vector2)rig.house.position + PilotWalkPoints[entry.pilotWalkWaypoints]);
                    else
                    {
                        entry.pilotWalkComplete = true;
                        mover.MoveTo(DoorwayCenter());
                        session.stage = "walking_doorway";
                    }
                    phaseStarted = EditorApplication.timeSinceStartup; Persist(); return;
                case "walking_back_inside":
                    if (!mover.Arrived) return;
                    if (rig.door.IsOpen) rig.door.Interact(player.gameObject);
                    phaseStarted = EditorApplication.timeSinceStartup; session.stage = "waiting_close"; Persist(); return;
                case "waiting_close":
                    if (rig.door.IsAnimating) return;
                    if (!rig.door.IsOpen) rig.door.Interact(player.gameObject);
                    session.stage = "waiting_reopen"; phaseStarted = EditorApplication.timeSinceStartup; Persist(); return;
                case "waiting_reopen":
                    if (rig.door.IsAnimating) return;
                    if (!rig.door.IsOpen) throw new InvalidOperationException(entry.houseId + " did not reopen from inside.");
                    mover.MoveTo(Outside());
                    phaseStarted = EditorApplication.timeSinceStartup; session.stage = "walking_exit"; Persist(); return;
                case "walking_exit":
                    if (!mover.Arrived) return;
                    phaseStarted = EditorApplication.timeSinceStartup; session.stage = "waiting_exit"; Persist(); return;
                case "waiting_exit":
                    if (EditorApplication.timeSinceStartup - phaseStarted < SettleSeconds) return;
                    entry.roofExit = State(rig.roofs.First()); entry.exitCovered = Covered();
                    entry.status = Pass(entry) ? "PASS" : "FAIL";
                    entry.reason = entry.status == "PASS" ? "All measured physical-entry checks passed with live Player body and NPC bodies retained." : "Inspect individual flags and sweeps.";
                    session.index++; session.stage = "next"; Persist(); return;
                case "restoring":
                    mover.Stop();
                    TownKeyartLivePhysicsProbe.SetPosition(player, session.originalPlayerPosition);
                    var body = player.GetComponent<Rigidbody2D>(); if (body != null) body.linearVelocity = session.originalVelocity;
                    player.enabled = session.originalPlayerControllerEnabled;
                    if (session.clockTouched)
                    {
                        var clock = GameBootstrap.Instance != null ? GameBootstrap.Instance.GameTimeManager : null;
                        if (clock != null) clock.RestorePhaseState(session.originalGamePhase, session.originalGamePhaseTimer);
                    }
                    EditorApplication.ExitPlaymode(); return;
            }
        }

        private static Rig ReadRig(Scene scene, TownBuildingLot lot)
        {
            var house = Components<Transform>(scene).Single(t => t.name == lot.Name);
            var door = house.GetComponentsInChildren<HouseDoorInteractable>(true).Single();
            var reveal = house.GetComponentsInChildren<RoofRevealController>(true).Single();
            var data = new SerializedObject(reveal);
            var feet = data.FindProperty("_feetInterior").objectReferenceValue as BoxCollider2D;
            var roofProperty = data.FindProperty("_roofRenderers"); var interiorProperty = data.FindProperty("_interiorRenderers");
            var roofs = ReadRenderers(roofProperty); var interiors = ReadRenderers(interiorProperty);
            var sensor = door.GetComponents<Collider2D>().Single(c => c.isTrigger);
            if (door.PhysicalBlocker == null || feet == null || !feet.isTrigger || roofs.Length == 0 || interiors.Length == 0) throw new InvalidOperationException(lot.Name + " lacks configured blocker/feet/reveal renderers.");
            return new Rig { lot = lot, house = house, door = door, blocker = door.PhysicalBlocker, sensor = sensor, reveal = reveal, feet = feet, roofs = roofs, interiors = interiors };
        }
        private static SpriteRenderer[] ReadRenderers(SerializedProperty property)
        {
            var result = new SpriteRenderer[property.arraySize];
            for (var i = 0; i < result.Length; i++) result[i] = property.GetArrayElementAtIndex(i).objectReferenceValue as SpriteRenderer;
            if (result.Any(r => r == null)) throw new InvalidOperationException("RoofReveal renderer reference is null.");
            return result;
        }
        private static void AuditFurniture(EntryEvidence entry)
        {
            var colliders = rig.house.GetComponentsInChildren<Collider2D>(true).Where(c => c.enabled && !c.isTrigger && c != rig.blocker &&
                !c.name.StartsWith("Wall_", StringComparison.Ordinal) && c.GetComponentInParent<HouseDoorInteractable>() == null).ToArray();
            foreach (var collider in colliders)
            {
                // A generated support collider can be a child of the visual owner. Walk only its
                // own ancestor chain up to (but excluding) the house, never siblings or house art.
                var visible = HasLocalOwnerRenderer(collider.transform);
                entry.furniture.Add(new FurnitureEvidence { path = PathOf(collider.transform), enabled = collider.enabled, trigger = collider.isTrigger, hasVisibleRenderer = visible,
                    status = visible ? "PASS" : "FAIL", reason = visible ? "Enabled non-trigger physical base has a visible authored renderer." : "Solid base has no visible renderer in the materialized interior." });
            }
            if (entry.furniture.Count == 0) entry.furniture.Add(new FurnitureEvidence { status = "FAIL", reason = "No interior furniture solid was materialized." });
        }
        private static void AuditPilotFurniture(PlayerController player, EntryEvidence entry)
        {
            if (entry.houseId != "House_GateKeeper") return;
            var origin = (Vector2)player.transform.position;
            var playerSolid = player.GetComponentsInChildren<Collider2D>().Single(c => c.enabled && !c.isTrigger);
            var bodyOffset = (Vector2)playerSolid.bounds.center - origin;
            foreach (Transform child in rig.house)
            {
                if (!child.name.StartsWith("Furniture_GateKeeper", StringComparison.Ordinal)) continue;
                var solid = child.GetComponent<BoxCollider2D>();
                if (solid == null) throw new InvalidOperationException(child.name + " lacks its authored solid base.");
                // Snapshot casts of the real Player solid, not a claim of walking every vector.
                var sweep = TownKeyartLivePhysicsProbe.ProbeDoorSegment(player, solid, origin,
                    (Vector2)solid.bounds.center - bodyOffset);
                entry.pilotFurnitureSweeps.Add(sweep);
            }
            if (entry.pilotFurnitureSweeps.Count != 3 || entry.pilotFurnitureSweeps.Any(s =>
                    s.status != "MEASURED" || !s.sourceClear || !s.blockerHit || s.destinationClear))
                throw new InvalidOperationException("GateKeeper pilot furniture did not block the real Player collider cast from its interior pocket; inspect pilotFurnitureSweeps.");
        }

        private static bool HasLocalOwnerRenderer(Transform collider)
        {
            if (collider.GetComponentsInChildren<SpriteRenderer>(true).Any(IsVisibleRenderer)) return true;
            // Supports are generated below the visual owner, while normal prop roots own a
            // Visual child. Ancestors are checked only for a renderer on that exact transform:
            // never inspect an ancestor subtree, which could accept a sibling's artwork.
            for (var owner = collider.parent; owner != null && owner != rig.house; owner = owner.parent)
            {
                var renderer = owner.GetComponent<SpriteRenderer>();
                if (IsVisibleRenderer(renderer)) return true;
            }
            return false;
        }
        private static bool IsVisibleRenderer(SpriteRenderer renderer) => renderer != null && renderer.sprite != null && renderer.enabled && renderer.color.a > 0f;
        private static readonly Vector2[] PilotWalkPoints =
        {
            new Vector2(0f, -1.85f), new Vector2(-1.65f, -1.85f), new Vector2(0f, -1.85f),
            Vector2.zero, new Vector2(.25f, 0f), new Vector2(0f, -1.2f),
            new Vector2(1f, -1.2f), new Vector2(0f, -1.2f), Vector2.zero
        };

        private static void CaptureInteriorIfRequested(Scene scene, EntryEvidence entry, string pose = null)
        {
            if ((pose == null && entry.interiorCapturePath != null) || !IsInteriorCaptureLot(rig.lot.Name)) return;
            var camera = Components<UnityEngine.Camera>(scene).SingleOrDefault(value => value.isActiveAndEnabled && value.CompareTag("MainCamera"));
            if (camera == null) throw new InvalidOperationException(rig.lot.Name + " interior capture requires the active MainCamera.");
            if (!camera.orthographic) throw new InvalidOperationException(rig.lot.Name + " interior capture requires the real orthographic MainCamera.");

            var originalTarget = camera.targetTexture;
            var originalActive = RenderTexture.active;
            var originalPosition = camera.transform.position;
            var originalRotation = camera.transform.rotation;
            var originalOrthographic = camera.orthographic;
            var originalSize = camera.orthographicSize;
            var originalAspect = camera.aspect;
            var originalProjection = camera.projectionMatrix;
            var target = new RenderTexture(InteriorCaptureWidth, InteriorCaptureHeight, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                FrameInterior(camera, rig.lot);
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(InteriorCaptureWidth, InteriorCaptureHeight, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, InteriorCaptureWidth, InteriorCaptureHeight), 0, 0);
                image.Apply();
                var path = Path.Combine(session.output, "interior_" + rig.lot.Name.Replace("House_", "").ToLowerInvariant() + (pose == null ? "" : "_" + pose) + ".png");
                if (File.Exists(path)) throw new IOException("Refusing to overwrite interior capture: " + path);
                File.WriteAllBytes(path, image.EncodeToPNG());
                if (pose == null) entry.interiorCapturePath = path;
                else entry.pilotPoseCaptures.Add(path);
            }
            finally
            {
                camera.targetTexture = originalTarget;
                camera.transform.SetPositionAndRotation(originalPosition, originalRotation);
                camera.orthographic = originalOrthographic;
                camera.orthographicSize = originalSize;
                camera.aspect = originalAspect;
                camera.projectionMatrix = originalProjection;
                RenderTexture.active = originalActive;
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
        private static bool IsInteriorCaptureLot(string houseId) => houseId == "House_Prison" || houseId == "House_Archive" || houseId == "House_Registry" || houseId == "House_GateKeeper";
        private static void FrameInterior(UnityEngine.Camera camera, TownBuildingLot lot)
        {
            const float padding = 1.25f;
            var aspect = (float)InteriorCaptureWidth / InteriorCaptureHeight;
            camera.transform.position = new Vector3(lot.Center.x, lot.Center.y, camera.transform.position.z);
            // Fit the authored lot equally for every capture, so before/after runs produce the
            // same framing without depending on the player or CameraFollow2D's current target.
            camera.orthographicSize = Mathf.Max((lot.Size.y + padding * 2f) * .5f, (lot.Size.x + padding * 2f) / (2f * aspect));
            camera.aspect = aspect;
            camera.ResetProjectionMatrix();
        }
        private static bool Pass(EntryEvidence e) => e.blockerClosed && e.blockerStayedDuringAnimation && e.openedAfterAnimation && e.closedSweepBlocked && e.openSweepClear && e.outsideInteractionDidNotReveal && e.insideRevealed && e.exitCovered && e.insideInteractionAvailable && e.outsideInteractionAvailable && e.occupiedCloseSafe && e.furniture.All(f => f.status == "PASS") && (e.houseId != "House_GateKeeper" || (e.pilotWalkComplete && e.pilotWalkWaypoints == PilotWalkPoints.Length && e.pilotPoseCaptures.Count == 3));
        private static Vector2 Outside() => rig.lot.DoorApproach;
        private static Vector2 Inside() => InteriorPoint();
        // Collider.bounds is empty whenever an open door disables its blocker. This derives the
        // same authored physical center used by HouseDoorInteractable occupancy instead.
        private static Vector2 DoorwayCenter()
        {
            if (!(rig.blocker is BoxCollider2D box)) throw new InvalidOperationException(rig.lot.Name + " blocker must be BoxCollider2D.");
            return box.transform.TransformPoint(box.offset);
        }
        // Use the actual authored approach relative to the materialized blocker. Temple's
        // catalog DoorSide points opposite its scene door; using it aimed through the closed leaf.
        private static Vector2 PhysicalOutsideDirection()
        {
            var delta = (Vector2)rig.lot.DoorApproach - DoorwayCenter();
            if (delta.sqrMagnitude < .01f) throw new InvalidOperationException(rig.lot.Name + " has no distinct exterior approach.");
            return delta.normalized;
        }
        private static Vector2 DoorSelectionOutside() => DoorwayCenter() + PhysicalOutsideDirection() * .7f;
        private static string MotionDiagnostic(PlayerController player)
        {
            var solid = TownKeyartLivePhysicsProbe.PlayerSolid(player);
            var contacts = new Collider2D[64];
            var count = solid.GetContacts(contacts);
            var paths = contacts.Take(count).Where(c => c != null).Select(c => PathOf(c.transform));
            var nearby = Components<CindarsHope.NPC.NpcController>(player.gameObject.scene)
                .Where(n => Vector2.Distance(n.transform.position, player.transform.position) < 4f)
                .Select(n => PathOf(n.transform) + "@" + n.transform.position);
            return "; bodyBounds=" + solid.bounds + "; contacts=[" + string.Join(",", paths) +
                "]; contactsBufferFull=" + (count == contacts.Length) + "; nearbyNpcs=[" + string.Join(",", nearby) + "]";
        }

        private static string SelectorDiagnostic(PlayerController player, IInteractable selected)
        {
            var trigger = new SerializedObject(selector).FindProperty("_interactionTrigger").objectReferenceValue as Collider2D;
            var origin = trigger != null ? (Vector2)trigger.bounds.center : (Vector2)selector.transform.position;
            var candidates = typeof(InteractionSystem).GetField("_candidates", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(selector) as System.Collections.IEnumerable;
            var registered = new List<string>();
            if (candidates != null)
                foreach (var candidate in candidates)
                {
                    var collider = candidate.GetType().GetField("Collider")?.GetValue(candidate) as Collider2D;
                    if (collider != null) registered.Add(PathOf(collider.transform));
                }
            return "selected=" + (selected is Component component ? PathOf(component.transform) : selected?.GetType().Name ?? "null") +
                "; player=" + player.transform.position + "; trigger=" + (trigger == null ? "null" : PathOf(trigger.transform)) +
                "; origin=" + origin + "; sensor=" + rig.sensor.bounds +
                "; closestDistance=" + Vector2.Distance(origin, rig.sensor.ClosestPoint(origin)) +
                "; registered=[" + string.Join(",", registered) + "]";
        }
        private static Vector2 InteriorPoint() => TownAccessMetrics.InteriorPocket(rig.lot).center;
        private static RendererState State(SpriteRenderer renderer) => new RendererState { enabled = renderer.enabled, alpha = renderer.color.a };
        private static bool Covered() => rig.roofs.All(r => r.enabled && r.color.a > .99f) && rig.interiors.All(r => !r.enabled);
        private static bool Revealed() => rig.roofs.All(r => r.color.a < .01f) && rig.interiors.All(r => r.enabled && r.color.a > 0f);
        private static bool RecordLotFailure(Exception exception)
        {
            if (rig == null || session == null || session.entries == null || session.entries.Count == 0 || session.index >= session.expectedWalkIns) return false;
            var entry = session.entries[session.entries.Count - 1];
            if (entry.status != "NOT_RUN") return false;
            mover?.Stop();
            entry.status = "FAIL";
            entry.reason = "Probe exception: " + exception.Message;
            session.index++; session.stage = "next"; phaseStarted = EditorApplication.timeSinceStartup; Persist();
            return true;
        }
        private static void Fail(string reason) { if (session == null) return; session.failure = reason; session.status = "FAIL"; Persist(); if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.ExitPlaymode(); else Complete(); }
        private static void Complete()
        {
            if (session == null) return;
            try
            {
                session.sceneHashAfter = Hash(ScenePath); session.saveHashesAfter = HashFiles(session.saveRoot);
                session.sceneUnchanged = session.sceneHashBefore == session.sceneHashAfter;
                session.savesUnchanged = HashesEqual(session.saveHashesBefore, session.saveHashesAfter);
                RestoreSetup(); session.setupRestored = true;
                if (string.IsNullOrEmpty(session.failure)) session.status = session.entries.Count == session.expectedWalkIns && session.entries.All(e => e.status == "PASS") && session.sceneUnchanged && session.savesUnchanged ? "PASS" : "FAIL";
                session.finishedUtc = DateTime.UtcNow.ToString("O");
                File.WriteAllText(Path.Combine(session.output, "town-all-entries.json"), JsonUtility.ToJson(session, true));
                Debug.Log("[TownAllEntriesPlayModeProbe] " + session.status + ": " + session.output);
                if (Application.isBatchMode) EditorApplication.Exit(session.status == "PASS" ? 0 : 1);
            }
            finally { Detach(); SessionState.EraseString(StateKey); session = null; rig = null; }
        }
        private static void Persist() => SessionState.SetString(StateKey, JsonUtility.ToJson(session));
        private static void Detach() { EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayState; }
        private static void RestoreSetup()
        {
            if (session.originalSetup == null || session.originalSetup.Length == 0 || session.originalSetup.All(s => string.IsNullOrEmpty(s.path))) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            else EditorSceneManager.RestoreSceneManagerSetup(session.originalSetup.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
        }
        private static string Hash(string path) { using var stream = File.OpenRead(path); using var sha = SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""); }
        private static FileHash[] HashFiles(string root) => Directory.Exists(root) ? Directory.GetFiles(root, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal).Select(p => new FileHash { path = p, sha256 = Hash(p) }).ToArray() : Array.Empty<FileHash>();
        private static bool HashesEqual(FileHash[] a, FileHash[] b) => a != null && b != null && a.Length == b.Length && a.Zip(b, (x, y) => x.path == y.path && x.sha256 == y.sha256).All(same => same);
        private static IEnumerable<T> Components<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
        private static string PathOf(Transform value) => value.parent == null ? value.name : PathOf(value.parent) + "/" + value.name;
    }

    /// <summary>Temporary controller for the real Player Rigidbody2D. It is added only to the unsaved Play Mode clone.</summary>
    internal sealed class TownAllEntriesBodyMotion : MonoBehaviour
    {
        private Rigidbody2D body;
        private Vector2 target;
        public bool Arrived { get; private set; } = true;
        public Vector2 Target => target;
        public void MoveTo(Vector2 destination) { target = destination; Arrived = false; }
        public void Stop() { if (body != null) body.linearVelocity = Vector2.zero; Arrived = true; }
        private void Awake() => body = GetComponent<Rigidbody2D>();
        private void FixedUpdate()
        {
            if (Arrived || body == null) return;
            // Test the actual post-simulation body position from the preceding fixed step. A blocked
            // MovePosition therefore never advances the probe to PASS merely because its request existed.
            if (Vector2.Distance(body.position, target) < .04f) { Arrived = true; return; }
            var next = Vector2.MoveTowards(body.position, target, 4f * Time.fixedDeltaTime);
            body.MovePosition(next);
        }
    }
}
