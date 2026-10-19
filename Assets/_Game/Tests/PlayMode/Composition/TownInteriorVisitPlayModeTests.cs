using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CindarsHope.Camera;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player.InputRouting;
using CindarsHope.World;
using CindarsHope.World.Scenes;
using CindarsHope.SceneManagement;
using CindarsHope.Save;
using CindarsHope.Player;
using CindarsHope.Save.Providers;
using CindarsHope.NPC;
using CindarsHope.NPC.Schedule;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class TownInteriorVisitPlayModeTests
    {
        private TownInteriorSceneLoader _additiveLoader;
        private string _additiveOperationId;
        private bool _loadedTownIntegrationScene;
        private Scene _integrationPersistentScene;
        private readonly HashSet<GameObject> _persistentRootsBeforeTownIntegration = new HashSet<GameObject>();
        private string _integrationValidationSaveDirectory;

        private sealed class CapturingInteriorAdmission : ITownInteriorVisitAdmission
        {
            public readonly List<TownInteriorVisitRequest> Requests = new List<TownInteriorVisitRequest>();

            public bool CanBegin(TownInteriorVisitRequest request, out string reason)
            {
                Requests.Add(request);
                reason = string.Empty;
                return true;
            }
        }

        private sealed class ControlledMovementKeys : IPlayerKeyReader
        {
            public bool Up;
            public bool Down;
            public bool Left;
            public bool Right;
            public bool GetKey(KeyCode key) => (Up && (key == KeyCode.W || key == KeyCode.UpArrow)) ||
                                                 (Down && (key == KeyCode.S || key == KeyCode.DownArrow)) ||
                                                 (Left && (key == KeyCode.A || key == KeyCode.LeftArrow)) ||
                                                 (Right && (key == KeyCode.D || key == KeyCode.RightArrow));
            public bool GetKeyDown(KeyCode key) => false;
            public bool GetKeyUp(KeyCode key) => false;
        }

        [UnityTearDown]
        public IEnumerator CleanupAdditiveInterior()
        {
            if (_additiveLoader != null && !string.IsNullOrEmpty(_additiveOperationId))
                _additiveLoader.CancelAndUnload(_additiveOperationId);
            if (_additiveLoader != null)
            {
                float deadline = Time.realtimeSinceStartup + 5f;
                while (SceneManager.GetSceneByName("TownInteriorScene").isLoaded &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.IsFalse(SceneManager.GetSceneByName("TownInteriorScene").isLoaded,
                    "The real additive interior must not leak into another PlayMode test.");
            }
            _additiveLoader = null;
            _additiveOperationId = null;
            if (!_loadedTownIntegrationScene) yield break;
            _loadedTownIntegrationScene = false;
            var cleanupScene = SceneManager.CreateScene("TownInteriorIntegrationCleanup_" + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(cleanupScene);
            var interiorScene = SceneManager.GetSceneByName("TownInteriorScene");
            if (interiorScene.isLoaded) yield return SceneManager.UnloadSceneAsync(interiorScene);
            var townScene = SceneManager.GetSceneByName("TownScene");
            if (townScene.isLoaded) yield return SceneManager.UnloadSceneAsync(townScene);
            var persistentScene = _integrationPersistentScene;
            if (persistentScene.IsValid() && persistentScene.isLoaded)
                foreach (var root in persistentScene.GetRootGameObjects())
                    if (!_persistentRootsBeforeTownIntegration.Contains(root))
                        UnityEngine.Object.Destroy(root);
            _persistentRootsBeforeTownIntegration.Clear();
            yield return null;
            if (!string.IsNullOrEmpty(_integrationValidationSaveDirectory))
            {
                var root = Path.GetFullPath(Path.Combine(Application.temporaryCachePath,
                    "town-interior-save-tests")).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var target = Path.GetFullPath(_integrationValidationSaveDirectory)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Assert.AreEqual(root, Path.GetDirectoryName(target),
                    "Cleanup may target only a direct child of the Town interior test cache.");
                Assert.IsTrue(Guid.TryParseExact(Path.GetFileName(target), "N", out _));
                if (Directory.Exists(target)) Directory.Delete(target, true);
                _integrationValidationSaveDirectory = null;
            }
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator RenkoPilot_RealTownPlayerCrossesDoorAndReturnsWithoutReentry()
        {
            const string houseId = "House_Residential_2";
            var captureDirectory = Environment.GetEnvironmentVariable("TOWN_CAPTURE_DIR");
            TownSceneRuntimeReferenceInstaller installer = null;
            TownInteriorVisitController visits = null;
            TownInteriorThreshold entryThreshold = null;
            PlayerController player = null;
            Rigidbody2D body = null;
            HouseDoorInteractable door = null;
            CameraFollow2D camera = null;
            UnityEngine.Camera gameplayCamera = null;
            CanvasGroup overlay = null;
            GameTimeManager clock = null;
            int originalClockPhase = 0;
            float originalClockTimer = 0f;
            SaveInput[] saveInputs = null;
            bool[] saveInputsEnabled = null;
            var eventOrder = new List<string>();
            var requests = new List<TownInteriorVisitRequest>();
            var finishes = new List<TownInteriorVisitFinishedEvent>();
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> preparePilot = (scene, mode) =>
            {
                if (scene.name != "TownScene") return;
                installer = FindInScene<TownSceneRuntimeReferenceInstaller>(scene);
                visits = FindInScene<TownInteriorVisitController>(scene);
                entryThreshold = FindInScene<TownInteriorThreshold>(scene);
                Assert.IsNotNull(installer, "TownScene needs its real runtime installer.");
                Assert.IsNotNull(visits, "TownScene needs the disabled Renko pilot controller.");
                Assert.IsNotNull(entryThreshold, "TownScene needs the disabled Renko physical entry threshold.");
                SetPrivate(installer, "_enableInteriorPilot", true); // test-memory opt-in before Start.
                visits.gameObject.SetActive(true);
                visits.enabled = true;
                entryThreshold.gameObject.SetActive(true);
                entryThreshold.enabled = true;
                entryThreshold.GetComponent<BoxCollider2D>().enabled = true;
                saveInputs = FindAllInScene<SaveInput>(scene);
                saveInputsEnabled = new bool[saveInputs.Length];
                for (var i = 0; i < saveInputs.Length; i++)
                { saveInputsEnabled[i] = saveInputs[i].enabled; saveInputs[i].enabled = false; }
            };
            Action<TownInteriorVisitRequestedEvent> requested = evt =>
            { if (evt.Request.HouseId == houseId) requests.Add(evt.Request); };
            Action<TownInteriorBlackoutReadyEvent> blackout = evt => eventOrder.Add(evt.OperationId + ":black");
            Action<TownInteriorPlayerPlacementRequestedEvent> placement = evt =>
            {
                eventOrder.Add(evt.OperationId + ":place-request");
                Assert.IsNotNull(overlay, "The real fade overlay must be present before placement.");
                Assert.GreaterOrEqual(overlay.alpha, .99f, "Player placement must remain under real blackout.");
            };
            Action<TownInteriorPlacementReadyEvent> placed = evt =>
            {
                eventOrder.Add(evt.OperationId + ":placed");
                if (camera == null || player == null || !string.Equals(evt.HouseId, houseId)) return;
                if (visits.CaptureCommitted().IsInside)
                    Assert.Less(Vector2.Distance(camera.transform.position, new Vector2(4096f, 4096f)), .5f,
                        "Interior camera must snap to the room bounds before fade-in, not pan from Town.");
                else
                    Assert.Less(Vector2.Distance(camera.transform.position, player.transform.position), .02f,
                        "Return camera must snap to the exterior Player before fade-in, not pan from the interior.");
            };
            Action<TownInteriorPlayerPlacementCompletedEvent> placementCompleted = evt =>
            {
                if (!evt.Success || player == null || body == null) return;
                Assert.Less(Vector2.Distance(body.position, player.transform.position), .02f,
                    "Physical body and rendered Transform must agree before placement ACK.");
            };
            Action<TownInteriorVisitFinishedEvent> finished = evt => { finishes.Add(evt); eventOrder.Add(evt.OperationId + ":finished"); };
            CapturePersistentRootIds(_persistentRootsBeforeTownIntegration);
            SceneManager.sceneLoaded += preparePilot;
            GameEventBus.Subscribe(requested);
            GameEventBus.Subscribe(blackout);
            GameEventBus.Subscribe(placement);
            GameEventBus.Subscribe(placed);
            GameEventBus.Subscribe(placementCompleted);
            GameEventBus.Subscribe(finished);
            try
            {
                // Fixture host setup is synchronous: global frame-budget bootstraps must not
                // expire in the runner's empty scene while the large Town asset streams in.
                SceneManager.LoadScene("TownScene", LoadSceneMode.Single);
                _loadedTownIntegrationScene = true;
                yield return null;
                for (var frame = 0; frame < 240 &&
                     (GameBootstrap.Instance == null || player == null || !visits.isActiveAndEnabled); frame++)
                {
                    player = PlayerController.ActiveInstance;
                    yield return null;
                }
                player = PlayerController.ActiveInstance;
                body = player != null ? player.GetComponent<Rigidbody2D>() : null;
                door = entryThreshold != null
                    ? entryThreshold.transform.parent.GetComponentInChildren<HouseDoorInteractable>(true) : null;
                camera = FindInScene<CameraFollow2D>(SceneManager.GetActiveScene());
                gameplayCamera = camera != null ? camera.GetComponent<UnityEngine.Camera>() : null;
                overlay = FindInLoadedScenes<CanvasGroup>(group => group.GetComponent<SceneFadeOverlayController>() != null,
                    GameBootstrap.Instance.gameObject.scene);
                if (saveInputs != null && saveInputs.Length == 0)
                {
                    saveInputs = FindAllInScene<SaveInput>(GameBootstrap.Instance.gameObject.scene);
                    saveInputsEnabled = new bool[saveInputs.Length];
                    for (var i = 0; i < saveInputs.Length; i++)
                    { saveInputsEnabled[i] = saveInputs[i].enabled; saveInputs[i].enabled = false; }
                }
                Assert.IsNotNull(player, "TownScene did not materialize the real Player.");
                Assert.IsNotNull(body, "TownScene Player needs its real Rigidbody2D.");
                Assert.IsNotNull(door, "Renko pilot must retain its real door.");
                Assert.IsNotNull(camera, "TownScene must retain its real follow camera.");
                Assert.IsNotNull(gameplayCamera, "TownScene follow rig must retain its real gameplay Camera.");
                Assert.IsNotNull(overlay, "TownScene must retain the real fade overlay.");
                Assert.AreSame(entryThreshold.transform.parent.GetComponentInChildren<HouseDoorInteractable>(), door,
                    "The entry threshold must use the Renko building door, not a substitute.");

                var save = GameBootstrap.Instance.SaveManager as SaveManager;
                Assert.IsNotNull(save, "The real booted SaveManager is required for installer readiness.");
                clock = GameBootstrap.Instance.GameTimeManager;
                Assert.IsNotNull(clock, "Renko entry must evaluate the real game clock.");
                originalClockPhase = (int)clock.CurrentPhase;
                originalClockTimer = clock.PhaseTimer;
                SetTownDayHour(clock, 12); // policy setup only; NPCs and all colliders remain active.
                GameEventBus.Publish(new GameTimeTickEvent(clock.PhaseTimer, clock.CurrentDay));
                for (var frame = 0; frame < 240 && !(bool)GetPrivate(installer, "_interiorReady"); frame++)
                    yield return null;
                Assert.IsTrue((bool)GetPrivate(installer, "_interiorReady"),
                    "Renko pilot did not bind the real Player, overlay, router, and SaveManager before timeout.");
                var saveBefore = SnapshotSaveFiles(save.SaveFilePath);
                var playerId = player;
                var cameraId = camera;
                var keys = new ControlledMovementKeys();
                player.KeyReader = keys;

                // Public door API is intentional here; this is not evidence of keyboard interaction.
                Debug.Log($"[RenkoVisit] Door initial open={door.IsOpen}, animating={door.IsAnimating}, hour={clock.CurrentHourOfDay}, timeScale={Time.timeScale}.");
                if (!door.IsOpen && !door.IsAnimating) door.Interact(player.gameObject);
                float doorDeadline = Time.realtimeSinceStartup + 5f;
                while ((!door.IsOpen || door.IsAnimating) && Time.realtimeSinceStartup < doorDeadline) yield return null;
                Assert.IsTrue(door.IsOpen && !door.IsAnimating,
                    $"Renko door failed: open={door.IsOpen}, animating={door.IsAnimating}, hour={clock.CurrentHourOfDay}, timeScale={Time.timeScale}.");

                body.position = (Vector2)entryThreshold.transform.position - Vector2.up * .8f;
                Physics2D.SyncTransforms();
                if (!string.IsNullOrWhiteSpace(captureDirectory))
                {
                    yield return new WaitForFixedUpdate();
                    yield return null;
                }
                camera.SnapToTarget(); // follows the real Player; does not change scale, offset, or zoom.
                CaptureTownPilotFrame(captureDirectory, gameplayCamera, "renko-before-entry.png");
                keys.Up = true;
                for (var frame = 0; frame < 360 && finishes.Count == 0; frame++) yield return new WaitForFixedUpdate();
                keys.Up = false;
                Assert.AreEqual(1, requests.Count, DescribeBlockedPath("entry", body, entryThreshold));
                Assert.AreEqual(1, finishes.Count, DescribeBlockedPath("entry completion", body, entryThreshold));
                Assert.IsTrue(finishes[0].Success, finishes[0].Reason);
                Assert.IsTrue(visits.CaptureCommitted().IsInside);
                Assert.AreEqual(houseId, visits.CaptureCommitted().HouseId);
                Assert.AreSame(playerId, PlayerController.ActiveInstance);
                Assert.AreSame(cameraId, FindInScene<CameraFollow2D>(SceneManager.GetActiveScene()));
                Assert.AreEqual("TownScene", SceneManager.GetActiveScene().name);
                AssertEventOrder(eventOrder, finishes[0].OperationId);
                yield return null;
                CaptureTownPilotFrame(captureDirectory, gameplayCamera, "renko-interior-idle.png");

                if (Environment.GetEnvironmentVariable("TOWN_FURNISHED_PILOT") == "1")
                {
                    Assert.IsTrue(visits.TryGetPresentedInterior(houseId, out var room));
                    var renkos = FindAllInScene<NpcShopController>(SceneManager.GetActiveScene());
                    NpcShopController renko = null;
                    int renkoCount = 0;
                    foreach (var shop in renkos)
                        if (shop.NpcData != null && shop.NpcData.NpcId == "npc_renko")
                        { renko = shop; renkoCount++; }
                    Assert.AreEqual(1, renkoCount, "Use the canonical Renko, never an interior clone.");
                    var presentation = renko.GetComponent<NpcInteriorPresentationAdapter>();
                    Assert.IsNotNull(presentation);
                    Assert.IsTrue(presentation.IsPresented, "Renko must be visibly attending at noon.");
                    Assert.IsTrue(presentation.CanInteractInCurrentSpace);
                    Assert.IsTrue(room.TryGetNpcAnchor("npc_renko", "npc_renko_work", out var work));
                    Assert.Less(Vector2.Distance(work.position, renko.transform.position), .05f);
                    var npcManager = FindInScene<NpcManager>(SceneManager.GetActiveScene()) ??
                        FindInScene<NpcManager>(GameBootstrap.Instance.gameObject.scene);
                    Assert.IsNotNull(npcManager);
                    var npcSnapshot = npcManager.CaptureSaveData();
                    var renkoSave = npcSnapshot.Npcs.Find(npc => npc.NpcId == "npc_renko");
                    Assert.IsNotNull(renkoSave);
                    Assert.AreEqual(houseId, renkoSave.InteriorHouseId);
                    Assert.Less(renkoSave.Position.sqrMagnitude, 100000f,
                        "NPC save Position must stay in Town, never at the additive presentation origin.");
                    Assert.AreEqual(renko.HasMet, renkoSave.HasMet);
                    var restoredNpcSnapshot = JsonUtility.FromJson<NpcManagerSaveData>(JsonUtility.ToJson(npcSnapshot));
                    npcManager.RestoreFromSaveData(new NpcManagerSaveData
                    { Npcs = new List<NpcSaveData> { restoredNpcSnapshot.Npcs.Find(npc => npc.NpcId == "npc_renko") } });
                    Assert.IsTrue(presentation.IsPresented);
                    Assert.Less(Vector2.Distance(work.position, renko.transform.position), .05f);
                    var props = room.transform.Find("Props");
                    Assert.IsNotNull(props);
                    Assert.GreaterOrEqual(props.childCount, 12);
                    Assert.GreaterOrEqual(props.GetComponentsInChildren<BoxCollider2D>().Length, 12);
                    var floorVisual = room.transform.Find("Floor").GetComponentInChildren<SpriteRenderer>();
                    Assert.Less(Vector2.Distance(floorVisual.bounds.center, room.WorldBounds.center), .02f,
                        "The tiled floor pivot must not shift the room away from its declared bounds.");
                    Assert.Less(Vector2.Distance(floorVisual.bounds.size, room.WorldBounds.size), .02f);
                    Assert.AreEqual(0, room.transform.Find("Floor").GetComponentsInChildren<Collider2D>().Length);
                    var sideVisual = room.transform.Find("Walls/LeftWall").GetComponentInChildren<SpriteRenderer>();
                    var backVisual = room.transform.Find("Walls/BackWall").GetComponentInChildren<SpriteRenderer>();
                    Assert.That(sideVisual.bounds.size.x, Is.InRange(.15f, .30f));
                    Assert.Greater(backVisual.bounds.size.y, sideVisual.bounds.size.x * 3f);
                    Debug.Log($"[RenkoFurniture] playerBody={body.GetComponent<Collider2D>().bounds.size}; npc={renko.transform.position}; props={props.childCount}.");

                    var startingPosition = body.position;
                    yield return WalkPilotTo(body, keys, (Vector2)room.transform.position + new Vector2(0f, 3f));
                    CaptureTownPilotFrame(captureDirectory, gameplayCamera, "renko-bedroom-route.png");
                    yield return WalkPilotTo(body, keys, startingPosition);
                    CaptureTownPilotFrame(captureDirectory, gameplayCamera, "renko-shop-return.png");
                }

                keys.Down = true;
                for (var frame = 0; frame < 360 && finishes.Count < 2; frame++) yield return new WaitForFixedUpdate();
                keys.Down = false;
                Assert.AreEqual(2, requests.Count, DescribeBlockedPath("exit", body, entryThreshold));
                Assert.AreEqual(2, finishes.Count, DescribeBlockedPath("exit completion", body, entryThreshold));
                Assert.IsTrue(finishes[1].Success, finishes[1].Reason);
                Assert.IsFalse(visits.CaptureCommitted().IsInside);
                Assert.AreEqual("TownScene", SceneManager.GetActiveScene().name);
                Assert.AreSame(playerId, PlayerController.ActiveInstance);
                Assert.AreSame(cameraId, FindInScene<CameraFollow2D>(SceneManager.GetActiveScene()));
                AssertEventOrder(eventOrder, finishes[1].OperationId);
                yield return null;
                if (!string.IsNullOrWhiteSpace(captureDirectory))
                    yield return new WaitForSecondsRealtime(1f); // idle capture, not evidence of the fade transition.
                CaptureTownPilotFrame(captureDirectory, gameplayCamera, "renko-after-return.png");
                for (var frame = 0; frame < 30; frame++) yield return new WaitForFixedUpdate();
                Assert.AreEqual(2, requests.Count, "Return pocket must not re-enter until a new physical crossing.");
                Assert.AreEqual(saveBefore, SnapshotSaveFiles(save.SaveFilePath),
                    "The isolated traversal must not write the user save, backup, or temporary slot files.");
                player.KeyReader = null;
            }
            finally
            {
                if (player != null) player.KeyReader = null;
                if (clock != null) clock.RestorePhaseState(originalClockPhase, originalClockTimer);
                if (saveInputs != null)
                    for (var i = 0; i < saveInputs.Length; i++) if (saveInputs[i] != null) saveInputs[i].enabled = saveInputsEnabled[i];
                GameEventBus.Unsubscribe(requested);
                GameEventBus.Unsubscribe(blackout);
                GameEventBus.Unsubscribe(placement);
                GameEventBus.Unsubscribe(placed);
                GameEventBus.Unsubscribe(placementCompleted);
                GameEventBus.Unsubscribe(finished);
                SceneManager.sceneLoaded -= preparePilot;
            }
        }

        [UnityTest]
        public IEnumerator RenkoPilot_BlockedReturnPocketRollsBackInsideThenRetryExits()
        {
            const string houseId = "House_Residential_2";
            const string failureKey = "ui.town_interior.visit_failed";
            TownSceneRuntimeReferenceInstaller installer = null;
            TownInteriorVisitController visits = null;
            TownInteriorThreshold entryThreshold = null;
            PlayerController player = null;
            Rigidbody2D body = null;
            HouseDoorInteractable door = null;
            CanvasGroup overlay = null;
            GameTimeManager clock = null;
            int originalClockPhase = 0;
            float originalClockTimer = 0f;
            SaveInput[] saveInputs = null;
            bool[] saveInputsEnabled = null;
            var finishes = new List<TownInteriorVisitFinishedEvent>();
            var lockEvents = new List<TownInteriorPlayerLockEvent>();
            var blackoutTokens = new HashSet<string>();
            var feedback = new List<PlayerActionFeedbackEvent>();
            var blockers = new List<GameObject>();
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> prepare = (scene, mode) =>
            {
                if (scene.name != "TownScene") return;
                installer = FindInScene<TownSceneRuntimeReferenceInstaller>(scene);
                visits = FindInScene<TownInteriorVisitController>(scene);
                entryThreshold = FindInScene<TownInteriorThreshold>(scene);
                Assert.IsNotNull(installer, "TownScene needs its real runtime installer.");
                Assert.IsNotNull(visits, "TownScene needs the real interior controller.");
                Assert.IsNotNull(entryThreshold, "TownScene needs the Renko entry threshold.");
                SetPrivate(installer, "_enableInteriorPilot", true);
                visits.gameObject.SetActive(true); visits.enabled = true;
                entryThreshold.gameObject.SetActive(true); entryThreshold.enabled = true;
                entryThreshold.GetComponent<BoxCollider2D>().enabled = true;
                saveInputs = FindAllInScene<SaveInput>(scene);
                saveInputsEnabled = new bool[saveInputs.Length];
                for (var i = 0; i < saveInputs.Length; i++)
                { saveInputsEnabled[i] = saveInputs[i].enabled; saveInputs[i].enabled = false; }
            };
            Action<TownInteriorVisitFinishedEvent> onFinished = evt =>
            { if (evt.HouseId == houseId) finishes.Add(evt); };
            Action<TownInteriorPlayerLockEvent> onLock = evt => lockEvents.Add(evt);
            Action<TownInteriorBlackoutReadyEvent> onBlackout = evt => blackoutTokens.Add(evt.OperationId);
            Action<PlayerActionFeedbackEvent> onFeedback = evt => feedback.Add(evt);
            CapturePersistentRootIds(_persistentRootsBeforeTownIntegration);
            SceneManager.sceneLoaded += prepare;
            GameEventBus.Subscribe(onFinished);
            GameEventBus.Subscribe(onLock);
            GameEventBus.Subscribe(onBlackout);
            GameEventBus.Subscribe(onFeedback);
            try
            {
                _loadedTownIntegrationScene = true;
                SceneManager.LoadScene("TownScene", LoadSceneMode.Single);
                yield return null;
                for (var frame = 0; frame < 240 &&
                     (GameBootstrap.Instance == null || PlayerController.ActiveInstance == null || !visits.isActiveAndEnabled); frame++)
                    yield return null;
                player = PlayerController.ActiveInstance;
                body = player != null ? player.GetComponent<Rigidbody2D>() : null;
                door = entryThreshold != null
                    ? entryThreshold.transform.parent.GetComponentInChildren<HouseDoorInteractable>(true) : null;
                var bootstrap = GameBootstrap.Instance;
                Assert.IsNotNull(bootstrap, "TownScene boot must complete before resolving its real fade overlay.");
                overlay = FindInLoadedScenes<CanvasGroup>(group => group.GetComponent<SceneFadeOverlayController>() != null,
                    bootstrap.gameObject.scene);
                Assert.IsNotNull(player); Assert.IsNotNull(body); Assert.IsNotNull(door);
                Assert.IsNotNull(overlay, "The rollback must use the real fade overlay.");
                clock = GameBootstrap.Instance.GameTimeManager;
                Assert.IsNotNull(clock);
                originalClockPhase = (int)clock.CurrentPhase;
                originalClockTimer = clock.PhaseTimer;
                SetTownDayHour(clock, 12);
                for (var frame = 0; frame < 240 && !(bool)GetPrivate(installer, "_interiorReady"); frame++) yield return null;
                Assert.IsTrue((bool)GetPrivate(installer, "_interiorReady"));

                var bindings = (TownInteriorVisitController.HouseBinding[])GetPrivate(visits, "_houses");
                Assert.IsNotNull(bindings);
                TownInteriorVisitController.HouseBinding renko = null;
                foreach (var binding in bindings) if (binding != null && binding.HouseId == houseId) { renko = binding; break; }
                Assert.IsNotNull(renko);
                Assert.AreEqual("Assets/_Game/Prefabs/City/Interiors/House_Residential_2.prefab",
                    AssetDatabase.GetAssetPath(renko.Prefab), "The real Renko prefab must own this visit.");
                Assert.IsNotNull(renko.ReturnPockets);
                Assert.Greater(renko.ReturnPockets.Length, 0);

                var keys = new ControlledMovementKeys();
                player.KeyReader = keys;
                if (!door.IsOpen && !door.IsAnimating) door.Interact(player.gameObject);
                float deadline = Time.realtimeSinceStartup + 5f;
                while ((!door.IsOpen || door.IsAnimating) && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.IsTrue(door.IsOpen && !door.IsAnimating);
                body.position = (Vector2)entryThreshold.transform.position - Vector2.up * .8f;
                Physics2D.SyncTransforms();
                keys.Up = true;
                for (var frame = 0; frame < 360 && finishes.Count == 0; frame++) yield return new WaitForFixedUpdate();
                keys.Up = false;
                Assert.AreEqual(1, finishes.Count, DescribeBlockedPath("entry", body, entryThreshold));
                Assert.IsTrue(finishes[0].Success, finishes[0].Reason);
                Assert.IsTrue(visits.CaptureCommitted().IsInside);

                // These are the test's only solids: existing Town NPCs and colliders are retained.
                foreach (var pocket in renko.ReturnPockets)
                {
                    Assert.IsNotNull(pocket);
                    var blocker = new GameObject("TR04_ReturnPocketBlocker", typeof(BoxCollider2D));
                    blocker.transform.position = pocket.position;
                    blocker.GetComponent<BoxCollider2D>().size = new Vector2(
                        player.GetComponent<Collider2D>().bounds.size.x + .6f,
                        player.GetComponent<Collider2D>().bounds.size.y + .6f);
                    blockers.Add(blocker);
                }
                Physics2D.SyncTransforms();
                keys.Down = true;
                for (var frame = 0; frame < 360 && finishes.Count < 2; frame++) yield return new WaitForFixedUpdate();
                keys.Down = false;
                Assert.AreEqual(2, finishes.Count, DescribeBlockedPath("blocked exit", body, entryThreshold));
                var failedExit = finishes[1];
                Assert.IsFalse(failedExit.Success, "A blocked return pocket must fail after the real blackout.");
                Assert.IsTrue(blackoutTokens.Contains(failedExit.OperationId), "Blocked exit must have reached real fade blackout.");
                Assert.LessOrEqual(overlay.alpha, .01f, "Rollback must complete its fade before the failure finishes.");
                Assert.IsTrue(visits.CaptureCommitted().IsInside, "Blocked exit must retain the Player on safe interior floor.");
                Assert.IsFalse(visits.IsTransitioning, "Blocked exit must release the transition state for a retry.");
                Assert.IsTrue(lockEvents.Exists(evt => evt.OperationId == failedExit.OperationId && !evt.Locked),
                    "Blocked exit must release the Player lock.");
                Assert.IsTrue(feedback.Exists(evt => evt.FailureKey == failureKey),
                    "Blocked exit must publish the localized visit-failure key, not a hard-coded English message.");

                foreach (var blocker in blockers)
                {
                    blocker.GetComponent<Collider2D>().enabled = false;
                    UnityEngine.Object.Destroy(blocker);
                }
                Physics2D.SyncTransforms();
                yield return null;
                // A failed crossing still requires leaving the sensor and approaching again.
                // Walk back into the room using the real movement controller, not a teleport.
                float retryStartY = body.position.y;
                keys.Up = true;
                for (var frame = 0; frame < 180 && body.position.y < retryStartY + 1.5f; frame++)
                    yield return new WaitForFixedUpdate();
                keys.Up = false;
                Assert.GreaterOrEqual(body.position.y, retryStartY + 1.5f,
                    "Rollback must leave movement available for a fresh doorway approach.");
                keys.Down = true;
                for (var frame = 0; frame < 360 && finishes.Count < 3; frame++) yield return new WaitForFixedUpdate();
                keys.Down = false;
                Assert.AreEqual(3, finishes.Count, DescribeBlockedPath("retry exit", body, entryThreshold));
                Assert.IsTrue(finishes[2].Success, finishes[2].Reason);
                Assert.IsFalse(visits.CaptureCommitted().IsInside);
                Assert.IsFalse(visits.IsTransitioning);
            }
            finally
            {
                if (player != null) player.KeyReader = null;
                if (clock != null) clock.RestorePhaseState(originalClockPhase, originalClockTimer);
                if (saveInputs != null)
                    for (var i = 0; i < saveInputs.Length; i++) if (saveInputs[i] != null) saveInputs[i].enabled = saveInputsEnabled[i];
                foreach (var blocker in blockers) if (blocker != null) UnityEngine.Object.Destroy(blocker);
                SceneManager.sceneLoaded -= prepare;
                GameEventBus.Unsubscribe(onFinished);
                GameEventBus.Unsubscribe(onLock);
                GameEventBus.Unsubscribe(onBlackout);
                GameEventBus.Unsubscribe(onFeedback);
            }
        }

        [UnityTest]
        public IEnumerator RenkoPilot_NoSafeRollbackRequiresRealOverlayRetryUntilInteriorIsSafe()
        {
            const string houseId = "House_Residential_2";
            TownSceneRuntimeReferenceInstaller installer = null;
            TownInteriorVisitController visits = null;
            TownInteriorThreshold entryThreshold = null;
            PlayerController player = null;
            Rigidbody2D body = null;
            HouseDoorInteractable door = null;
            CanvasGroup overlay = null;
            SceneFadeOverlayController overlayController = null;
            GameTimeManager clock = null;
            int originalClockPhase = 0;
            float originalClockTimer = 0f;
            SaveInput[] saveInputs = null;
            bool[] saveInputsEnabled = null;
            bool blockExitBlackout = false;
            GameObject interiorOriginBlocker = null;
            var blockers = new List<GameObject>();
            var finishes = new List<TownInteriorVisitFinishedEvent>();
            var recoveryRequired = new List<TownInteriorRecoveryRequiredEvent>();
            var lockEvents = new List<TownInteriorPlayerLockEvent>();
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> prepare = (scene, mode) =>
            {
                if (scene.name != "TownScene") return;
                installer = FindInScene<TownSceneRuntimeReferenceInstaller>(scene);
                visits = FindInScene<TownInteriorVisitController>(scene);
                entryThreshold = FindInScene<TownInteriorThreshold>(scene);
                Assert.IsNotNull(installer); Assert.IsNotNull(visits); Assert.IsNotNull(entryThreshold);
                SetPrivate(installer, "_enableInteriorPilot", true);
                visits.gameObject.SetActive(true); visits.enabled = true;
                entryThreshold.gameObject.SetActive(true); entryThreshold.enabled = true;
                entryThreshold.GetComponent<BoxCollider2D>().enabled = true;
                saveInputs = FindAllInScene<SaveInput>(scene);
                saveInputsEnabled = new bool[saveInputs.Length];
                for (var i = 0; i < saveInputs.Length; i++)
                { saveInputsEnabled[i] = saveInputs[i].enabled; saveInputs[i].enabled = false; }
            };
            Action<TownInteriorVisitFinishedEvent> onFinished = evt =>
            { if (evt.HouseId == houseId) finishes.Add(evt); };
            Action<TownInteriorRecoveryRequiredEvent> onRecovery = evt => recoveryRequired.Add(evt);
            Action<TownInteriorPlayerLockEvent> onLock = evt => lockEvents.Add(evt);
            Action<TownInteriorBlackoutReadyEvent> onBlackout = evt =>
            {
                if (!blockExitBlackout || interiorOriginBlocker != null || body == null) return;
                interiorOriginBlocker = new GameObject("TR04_InteriorOriginBlocker", typeof(BoxCollider2D));
                interiorOriginBlocker.transform.position = body.position;
                interiorOriginBlocker.GetComponent<BoxCollider2D>().size = new Vector2(
                    body.GetComponent<Collider2D>().bounds.size.x + .6f,
                    body.GetComponent<Collider2D>().bounds.size.y + .6f);
                blockers.Add(interiorOriginBlocker);
                Physics2D.SyncTransforms();
            };
            CapturePersistentRootIds(_persistentRootsBeforeTownIntegration);
            SceneManager.sceneLoaded += prepare;
            GameEventBus.Subscribe(onFinished);
            GameEventBus.Subscribe(onRecovery);
            GameEventBus.Subscribe(onLock);
            GameEventBus.Subscribe(onBlackout);
            try
            {
                _loadedTownIntegrationScene = true;
                SceneManager.LoadScene("TownScene", LoadSceneMode.Single);
                yield return null;
                for (var frame = 0; frame < 240 &&
                     (GameBootstrap.Instance == null || PlayerController.ActiveInstance == null || !visits.isActiveAndEnabled); frame++)
                    yield return null;
                player = PlayerController.ActiveInstance;
                body = player != null ? player.GetComponent<Rigidbody2D>() : null;
                door = entryThreshold != null
                    ? entryThreshold.transform.parent.GetComponentInChildren<HouseDoorInteractable>(true) : null;
                var bootstrap = GameBootstrap.Instance;
                Assert.IsNotNull(bootstrap); Assert.IsNotNull(player); Assert.IsNotNull(body); Assert.IsNotNull(door);
                overlay = FindInLoadedScenes<CanvasGroup>(group => group.GetComponent<SceneFadeOverlayController>() != null,
                    bootstrap.gameObject.scene);
                overlayController = overlay != null ? overlay.GetComponent<SceneFadeOverlayController>() : null;
                Assert.IsNotNull(overlay); Assert.IsNotNull(overlayController);
                var retryTransform = overlayController.transform.Find("InteriorRecoveryRetry");
                var retryButton = retryTransform != null
                    ? retryTransform.GetComponent<UnityEngine.UI.Button>() : null;
                Assert.IsNotNull(retryButton, "The real overlay must expose the configured recovery retry Button.");
                clock = bootstrap.GameTimeManager;
                Assert.IsNotNull(clock);
                originalClockPhase = (int)clock.CurrentPhase;
                originalClockTimer = clock.PhaseTimer;
                SetTownDayHour(clock, 12);
                for (var frame = 0; frame < 240 && !(bool)GetPrivate(installer, "_interiorReady"); frame++) yield return null;
                Assert.IsTrue((bool)GetPrivate(installer, "_interiorReady"));

                var bindings = (TownInteriorVisitController.HouseBinding[])GetPrivate(visits, "_houses");
                TownInteriorVisitController.HouseBinding renko = null;
                foreach (var binding in bindings) if (binding != null && binding.HouseId == houseId) { renko = binding; break; }
                Assert.IsNotNull(renko); Assert.IsNotNull(renko.Prefab);
                Assert.AreEqual("Assets/_Game/Prefabs/City/Interiors/House_Residential_2.prefab",
                    AssetDatabase.GetAssetPath(renko.Prefab));

                var keys = new ControlledMovementKeys();
                player.KeyReader = keys;
                if (!door.IsOpen && !door.IsAnimating) door.Interact(player.gameObject);
                float deadline = Time.realtimeSinceStartup + 5f;
                while ((!door.IsOpen || door.IsAnimating) && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.IsTrue(door.IsOpen && !door.IsAnimating);
                body.position = (Vector2)entryThreshold.transform.position - Vector2.up * .8f;
                Physics2D.SyncTransforms();
                keys.Up = true;
                for (var frame = 0; frame < 360 && finishes.Count == 0; frame++) yield return new WaitForFixedUpdate();
                keys.Up = false;
                Assert.AreEqual(1, finishes.Count); Assert.IsTrue(finishes[0].Success, finishes[0].Reason);
                Assert.IsTrue(visits.CaptureCommitted().IsInside);

                // Test-only solids cover every approved Town return pocket; real NPCs/colliders remain untouched.
                foreach (var pocket in renko.ReturnPockets)
                {
                    Assert.IsNotNull(pocket);
                    var blocker = new GameObject("TR04_ReturnPocketBlocker", typeof(BoxCollider2D));
                    blocker.transform.position = pocket.position;
                    blocker.GetComponent<BoxCollider2D>().size = new Vector2(
                        body.GetComponent<Collider2D>().bounds.size.x + .6f,
                        body.GetComponent<Collider2D>().bounds.size.y + .6f);
                    blockers.Add(blocker);
                }
                Physics2D.SyncTransforms();
                blockExitBlackout = true;
                keys.Down = true;
                for (var frame = 0; frame < 360 && recoveryRequired.Count == 0; frame++) yield return new WaitForFixedUpdate();
                keys.Down = false;
                Assert.AreEqual(1, recoveryRequired.Count, "No safe origin or return pocket must require explicit recovery.");
                var recoveryToken = recoveryRequired[0].OperationId;
                Assert.IsFalse(string.IsNullOrWhiteSpace(recoveryToken));
                Assert.AreEqual(1, finishes.Count, "Recovery-required state must not finish the visit.");
                Assert.GreaterOrEqual(overlay.alpha, .99f, "Recovery keeps the real overlay black.");
                Assert.IsTrue(visits.IsTransitioning);
                Assert.IsTrue(visits.CaptureCommitted().IsInside);
                Assert.IsTrue(retryButton.gameObject.activeInHierarchy && retryButton.interactable);
                Assert.IsFalse(lockEvents.Exists(evt => evt.OperationId == recoveryToken && !evt.Locked),
                    "Recovery must retain the visit Player lock until a safe rollback exists.");

                var recoveryCaptureDirectory = Environment.GetEnvironmentVariable("TOWN_RECOVERY_CAPTURE_DIR");
                if (!string.IsNullOrWhiteSpace(recoveryCaptureDirectory))
                {
                    Directory.CreateDirectory(recoveryCaptureDirectory);
                    ScreenCapture.CaptureScreenshot(Path.Combine(recoveryCaptureDirectory, "recovery-overlay.png"));
                    yield return null;
                }

                GameEventBus.Publish(new TownInteriorPlacementReadyEvent(recoveryToken, houseId));
                GameEventBus.Publish(new TownInteriorRecoveryRetryRequestedEvent("stale-recovery-token"));
                yield return null;
                Assert.GreaterOrEqual(overlay.alpha, .99f,
                    "Late forward placement must not fade a recovery-required blackout.");
                Assert.IsTrue(retryButton.gameObject.activeInHierarchy && retryButton.interactable,
                    "Late/stale callbacks must preserve the real recovery UI.");
                Assert.AreEqual(1, recoveryRequired.Count);
                Assert.AreEqual(1, finishes.Count);

                retryButton.onClick.Invoke(); // Real UI callback; this test makes no mouse-input claim.
                for (var frame = 0; frame < 180 && recoveryRequired.Count < 2; frame++) yield return null;
                Assert.AreEqual(2, recoveryRequired.Count, "Retry while every test obstacle remains must require recovery again.");
                Assert.AreEqual(recoveryToken, recoveryRequired[1].OperationId);
                Assert.AreEqual(1, finishes.Count);
                Assert.IsTrue(visits.IsTransitioning);
                Assert.IsFalse(lockEvents.Exists(evt => evt.OperationId == recoveryToken && !evt.Locked));

                visits.enabled = false;
                Assert.IsTrue(visits.IsTransitioning, "Unsafe disable must preserve the recovery transaction.");
                Assert.IsFalse(lockEvents.Exists(evt => evt.OperationId == recoveryToken && !evt.Locked),
                    "Unsafe disable must not release movement in an obstructed position.");
                visits.enabled = true;
                Assert.AreEqual(3, recoveryRequired.Count, "Re-enabling must restore the same recovery action.");
                Assert.AreEqual(recoveryToken, recoveryRequired[2].OperationId);

                foreach (var blocker in blockers)
                {
                    blocker.GetComponent<Collider2D>().enabled = false;
                    UnityEngine.Object.Destroy(blocker);
                }
                Physics2D.SyncTransforms();
                yield return null;
                Assert.IsTrue(retryButton.interactable, "A new recovery requirement must re-enable the real retry Button.");
                retryButton.onClick.Invoke();
                deadline = Time.realtimeSinceStartup + visits.TimeoutSeconds + 1f;
                while (finishes.Count < 2 && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(2, finishes.Count,
                    $"Safe retry must finish the original failed exit once. recovery={recoveryRequired.Count}, " +
                    $"body={body.position}, transform={player.transform.position}, alpha={overlay.alpha}, retry={retryButton.interactable}");
                Assert.IsFalse(finishes[1].Success);
                Assert.IsTrue(visits.CaptureCommitted().IsInside, "Safe recovery returns the Player to the retained interior.");
                Assert.IsFalse(visits.IsTransitioning);
                Assert.LessOrEqual(overlay.alpha, .01f, "Recovery must fade back in before controls release.");
                Assert.IsTrue(lockEvents.Exists(evt => evt.OperationId == recoveryToken && !evt.Locked));
            }
            finally
            {
                if (player != null) player.KeyReader = null;
                if (clock != null) clock.RestorePhaseState(originalClockPhase, originalClockTimer);
                if (saveInputs != null)
                    for (var i = 0; i < saveInputs.Length; i++) if (saveInputs[i] != null) saveInputs[i].enabled = saveInputsEnabled[i];
                foreach (var blocker in blockers) if (blocker != null) UnityEngine.Object.Destroy(blocker);
                SceneManager.sceneLoaded -= prepare;
                GameEventBus.Unsubscribe(onFinished);
                GameEventBus.Unsubscribe(onRecovery);
                GameEventBus.Unsubscribe(onLock);
                GameEventBus.Unsubscribe(onBlackout);
            }
        }

        [UnityTest]
        public IEnumerator RenkoPilot_RealSaveSlotDefersBlackoutAutosaveAndRoundTripsInterior()
        {
            const string houseId = "House_Residential_2";
            _integrationValidationSaveDirectory = Path.Combine(Application.temporaryCachePath,
                "town-interior-save-tests", Guid.NewGuid().ToString("N"));
            var expectedPath = Path.GetFullPath(Path.Combine(_integrationValidationSaveDirectory, "slot_1.json"));
            var personalPath = Path.Combine(Application.persistentDataPath, "saves", "slot_1.json");
            var personalBefore = SnapshotSaveFiles(personalPath);
            TownSceneRuntimeReferenceInstaller installer = null;
            TownInteriorVisitController visits = null;
            TownInteriorThreshold threshold = null;
            SaveManager save = null;
            PlayerController player = null;
            Rigidbody2D body = null;
            HouseDoorInteractable door = null;
            var finishes = new List<TownInteriorVisitFinishedEvent>();
            var loads = new List<GameLoadedEvent>();
            var saveResults = new List<GameSavedEvent>();
            bool autosaveRequestedDuringBlackout = false;
            string isolatedSlotBeforeTransition = null;
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> prepare = (scene, mode) =>
            {
                if (scene.name != "TownScene") return;
                installer = FindInScene<TownSceneRuntimeReferenceInstaller>(scene);
                visits = FindInScene<TownInteriorVisitController>(scene);
                threshold = FindInScene<TownInteriorThreshold>(scene);
                save = GameBootstrap.Instance != null ? GameBootstrap.Instance.SaveManager as SaveManager : null;
                if (save == null) save = FindInScene<SaveManager>(scene);
                Assert.IsNotNull(installer);
                Assert.IsNotNull(visits);
                Assert.IsNotNull(threshold);
                Assert.IsNotNull(save);
                save.ConfigureTemporarySaveDirectoryForValidation(_integrationValidationSaveDirectory);
                Assert.AreEqual(expectedPath, save.SaveFilePath, "Test isolation must precede any save I/O.");
                SetPrivate(installer, "_enableInteriorPilot", true);
                visits.gameObject.SetActive(true); visits.enabled = true;
                threshold.gameObject.SetActive(true); threshold.enabled = true;
                threshold.GetComponent<BoxCollider2D>().enabled = true;
            };
            Action<TownInteriorVisitFinishedEvent> onFinished = evt => finishes.Add(evt);
            Action<GameLoadedEvent> onLoaded = evt => loads.Add(evt);
            Action<GameSavedEvent> onSaved = evt => saveResults.Add(evt);
            Action<TownInteriorBlackoutReadyEvent> onBlackout = evt =>
            {
                if (autosaveRequestedDuringBlackout || save == null || !visits.IsTransitioning) return;
                autosaveRequestedDuringBlackout = true;
                Assert.IsFalse(save.SaveGame(), "Manual SaveGame must refuse while the interior blackout is active.");
                save.RequestAutosave();
                save.RequestAutosave();
                Assert.AreEqual(isolatedSlotBeforeTransition, SnapshotSaveFiles(expectedPath),
                    "Repeated deferred autosaves must not create an intermediate isolated slot during blackout.");
            };
            CapturePersistentRootIds(_persistentRootsBeforeTownIntegration);
            SceneManager.sceneLoaded += prepare;
            GameEventBus.Subscribe(onFinished);
            GameEventBus.Subscribe(onLoaded);
            GameEventBus.Subscribe(onSaved);
            GameEventBus.Subscribe(onBlackout);
            try
            {
                _loadedTownIntegrationScene = true;
                SceneManager.LoadScene("TownScene", LoadSceneMode.Single);
                yield return null;
                player = PlayerController.ActiveInstance;
                body = player != null ? player.GetComponent<Rigidbody2D>() : null;
                door = threshold != null
                    ? threshold.transform.parent.GetComponentInChildren<HouseDoorInteractable>(true) : null;
                Assert.IsNotNull(player); Assert.IsNotNull(body); Assert.IsNotNull(door);
                Assert.AreEqual(expectedPath, save.SaveFilePath);
                Assert.IsFalse(File.Exists(expectedPath), "The isolated slot must start empty.");
                isolatedSlotBeforeTransition = SnapshotSaveFiles(expectedPath);
                var clock = GameBootstrap.Instance.GameTimeManager;
                Assert.IsNotNull(clock);
                SetTownDayHour(clock, 12);
                for (var frame = 0; frame < 240 && !(bool)GetPrivate(installer, "_interiorReady"); frame++) yield return null;
                Assert.IsTrue((bool)GetPrivate(installer, "_interiorReady"));
                var keys = new ControlledMovementKeys();
                player.KeyReader = keys;
                if (!door.IsOpen && !door.IsAnimating) door.Interact(player.gameObject);
                float deadline = Time.realtimeSinceStartup + 5f;
                while ((!door.IsOpen || door.IsAnimating) && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.IsTrue(door.IsOpen && !door.IsAnimating);
                body.position = (Vector2)threshold.transform.position - Vector2.up * .8f;
                Physics2D.SyncTransforms();
                keys.Up = true;
                for (var frame = 0; frame < 360 && finishes.Count == 0; frame++) yield return new WaitForFixedUpdate();
                keys.Up = false;
                Assert.AreEqual(1, finishes.Count);
                Assert.IsTrue(finishes[0].Success, finishes[0].Reason);
                var committed = visits.CaptureCommitted();
                Assert.IsTrue(committed.IsInside);
                Assert.AreEqual(houseId, committed.HouseId);
                for (var frame = 0; frame < 120 && saveResults.FindAll(evt => evt.WasSuccessful).Count == 0; frame++)
                    yield return null;
                Assert.IsTrue(autosaveRequestedDuringBlackout,
                    "TR06 must issue repeated RequestAutosave calls only under the real blackout.");
                Assert.AreEqual(1, saveResults.FindAll(evt => !evt.WasSuccessful).Count,
                    "The manual blackout save must produce one rejected save feedback event.");
                Assert.IsNotEmpty(saveResults.Find(evt => !evt.WasSuccessful).Message,
                    "Rejected manual save feedback must carry a reason.");
                Assert.AreEqual(1, saveResults.FindAll(evt => evt.WasSuccessful).Count,
                    "Repeated blackout autosaves must coalesce into one committed save.");

                Assert.IsTrue(File.Exists(expectedPath), "Deferred autosave must write only after the interior commit.");
                var saved = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(expectedPath));
                Assert.AreEqual("TownScene", saved.CurrentSceneName);
                Assert.IsNotNull(saved.TownInteriorLocation);
                Assert.AreEqual(houseId, saved.TownInteriorLocation.HouseId);
                Assert.AreEqual(committed.AnchorId, saved.TownInteriorLocation.AnchorId);
                Assert.AreEqual(committed.ReturnAnchorId, saved.TownInteriorLocation.ReturnAnchorId);
                Assert.That(saved.TownInteriorLocation.LocalX, Is.EqualTo(committed.LocalX).Within(.02f));
                Assert.That(saved.TownInteriorLocation.LocalY, Is.EqualTo(committed.LocalY).Within(.02f));
                Assert.That(saved.Player.PlayerPosition.x, Is.EqualTo(committed.SafeTownX).Within(.02f));
                Assert.That(saved.Player.PlayerPosition.y, Is.EqualTo(committed.SafeTownY).Within(.02f));
                Assert.Greater(Vector2.Distance(saved.Player.PlayerPosition, player.transform.position), 100f,
                    "Legacy PlayerPosition must be the safe Town pocket, never the interior origin.");

                keys.Down = true;
                for (var frame = 0; frame < 360 && finishes.Count < 2; frame++) yield return new WaitForFixedUpdate();
                keys.Down = false;
                Assert.AreEqual(2, finishes.Count);
                Assert.IsTrue(finishes[1].Success, finishes[1].Reason);
                Assert.IsFalse(visits.CaptureCommitted().IsInside);
                int beforeLoadEvents = loads.Count;
                Assert.IsTrue(save.LoadGame(), "Real LoadGame must accept the isolated interior save.");
                deadline = Time.realtimeSinceStartup + 12f;
                while (loads.Count == beforeLoadEvents && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(beforeLoadEvents + 1, loads.Count);
                Assert.IsTrue(loads[loads.Count - 1].WasSuccessful);
                var restored = visits.CaptureCommitted();
                Assert.IsTrue(restored.IsInside);
                Assert.AreEqual(houseId, restored.HouseId);
                Assert.That(restored.LocalX, Is.EqualTo(saved.TownInteriorLocation.LocalX).Within(.05f));
                Assert.That(restored.LocalY, Is.EqualTo(saved.TownInteriorLocation.LocalY).Within(.05f));
                Assert.AreEqual("TownScene", SceneManager.GetActiveScene().name);

                // Legacy fixture uses the same real slot and SaveManager load path, with only the additive section absent.
                keys.Down = true;
                for (var frame = 0; frame < 360 && (visits.CaptureCommitted().IsInside || visits.IsTransitioning); frame++) yield return new WaitForFixedUpdate();
                keys.Down = false;
                Assert.IsFalse(visits.CaptureCommitted().IsInside);
                Assert.IsFalse(visits.IsTransitioning, "Legacy load must start after the exit transaction finishes.");
                saved.TownInteriorLocation = null;
                File.WriteAllText(expectedPath, JsonUtility.ToJson(saved, true));
                beforeLoadEvents = loads.Count;
                Assert.IsTrue(save.LoadGame());
                deadline = Time.realtimeSinceStartup + 12f;
                while (loads.Count == beforeLoadEvents && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(beforeLoadEvents + 1, loads.Count, "Legacy restore must finish through the real load lifecycle.");
                Assert.IsTrue(loads[loads.Count - 1].WasSuccessful);
                Assert.IsFalse(visits.CaptureCommitted().IsInside);
                Assert.Less(Vector2.Distance(body.position, saved.Player.PlayerPosition), .1f);

                // Unknown interior state must use the same isolated real load path and safely
                // fall back to Town, rather than retaining a stale additive-room commitment.
                saved.TownInteriorLocation = new TownInteriorLocationSaveData
                {
                    HouseId = "House_Removed_For_Validation",
                    AnchorId = "town_entry_House_Removed_For_Validation",
                    ReturnAnchorId = "town_return_House_Removed_For_Validation",
                    LocalX = 1f,
                    LocalY = 1f,
                    FacingX = 0f,
                    FacingY = -1f
                };
                File.WriteAllText(expectedPath, JsonUtility.ToJson(saved, true));
                beforeLoadEvents = loads.Count;
                Assert.IsTrue(save.LoadGame(), "Real LoadGame must accept an unknown-house isolated slot.");
                deadline = Time.realtimeSinceStartup + 12f;
                while (loads.Count == beforeLoadEvents && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(beforeLoadEvents + 1, loads.Count,
                    "Unknown-house restore must publish exactly one real GameLoaded event.");
                Assert.IsTrue(loads[loads.Count - 1].WasSuccessful);
                Assert.IsFalse(visits.CaptureCommitted().IsInside,
                    "Unknown-house restore must commit a safe Town fallback, not an orphaned interior.");
                var defaultTownSpawn = (Transform)GetPrivate(visits, "_defaultTownSpawn");
                Assert.IsNotNull(defaultTownSpawn);
                Assert.Less(Vector2.Distance(body.position, defaultTownSpawn.position), .1f,
                    "Unknown-house restore must use the registered safe exterior fallback.");
                Assert.AreEqual(personalBefore, SnapshotSaveFiles(personalPath),
                    "Real save/restore must not alter the personal slot, backup or temporary file.");
            }
            finally
            {
                if (player != null) player.KeyReader = null;
                SceneManager.sceneLoaded -= prepare;
                GameEventBus.Unsubscribe(onFinished);
                GameEventBus.Unsubscribe(onLoaded);
            }
        }

        [UnityTest]
        public IEnumerator RenkoExitThreshold_RealPrefabPhysicsCrossing_PublishesOneExitOnly()
        {
            const string houseId = "House_Residential_2";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Game/Prefabs/City/Interiors/House_Residential_2.prefab");
            Assert.IsNotNull(prefab, "The Renko exit test must exercise the generated prefab asset.");
            var interior = UnityEngine.Object.Instantiate(prefab);
            var threshold = interior.GetComponentInChildren<TownInteriorThreshold>();
            Assert.IsNotNull(threshold, "The materialized Renko interior requires its ExitThreshold.");
            var thresholdCollider = threshold.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(thresholdCollider);
            Assert.IsTrue(thresholdCollider.isTrigger);

            var player = new GameObject("RenkoExitPhysicsPlayer", typeof(Rigidbody2D), typeof(BoxCollider2D));
            player.tag = "Player";
            var body = player.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            var rootSolid = player.GetComponent<BoxCollider2D>();
            rootSolid.size = new Vector2(.2f, .2f);
            var childSolidObject = new GameObject("RenkoExitPhysicsPlayer_SecondSolid", typeof(BoxCollider2D));
            childSolidObject.transform.SetParent(player.transform, false);
            var childSolid = childSolidObject.GetComponent<BoxCollider2D>();
            childSolid.size = new Vector2(.2f, .2f);
            var admission = new CapturingInteriorAdmission();
            var published = new List<TownInteriorVisitRequest>();
            Action<TownInteriorVisitRequestedEvent> capturePublished = evt => published.Add(evt.Request);
            var previousSimulationMode = Physics2D.simulationMode;
            GameEventBus.Subscribe(capturePublished);
            Assert.IsTrue(SceneTransitionRouter.RegisterTownInteriorVisitAdmission(admission));
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                var center = (Vector2)threshold.transform.position;
                // A compound Player body must remain armed until both solids have exited.
                childSolidObject.transform.localPosition = Vector2.up * thresholdCollider.bounds.extents.y * .8f;
                body.position = center + Vector2.up * 1.2f;
                Physics2D.SyncTransforms();
                Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.AreEqual(0, published.Count, "Spawning north of ExitThreshold must not request an exit.");

                body.MovePosition(center + Vector2.up * .45f);
                Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.AreEqual(0, published.Count,
                    "Approaching and entering the sensor before its southward crossing must not request an exit.");

                body.MovePosition(center + Vector2.up * .1f);
                Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.AreEqual(0, published.Count,
                    "Remaining on the entry side of the sensor must not request an exit.");

                body.MovePosition(center + Vector2.down * .15f);
                Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.AreEqual(1, published.Count, "Crossing ExitThreshold southward must request one exit.");
                Assert.AreEqual(1, admission.Requests.Count);
                var request = published[0];
                Assert.IsTrue(request.Exiting);
                Assert.AreEqual(houseId, request.HouseId);
                Assert.AreEqual("town_entry_House_Residential_2", request.EntryAnchorId);
                Assert.AreEqual("town_return_House_Residential_2", request.ReturnAnchorId);
                Assert.AreEqual(request.OperationId, admission.Requests[0].OperationId);

                var rootOutsideChildStillInside = new Vector2(center.x,
                    thresholdCollider.bounds.min.y - rootSolid.bounds.extents.y -
                    (Physics2D.defaultContactOffset * 2f + .05f));
                body.MovePosition(rootOutsideChildStillInside);
                Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.IsFalse(rootSolid.IsTouching(thresholdCollider),
                    "TR01 precondition: first Player solid must have left the sensor.");
                Assert.IsTrue(childSolid.IsTouching(thresholdCollider),
                    "TR01 precondition: second solid on the same Rigidbody must still occupy the sensor.");
                Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.AreEqual(1, published.Count,
                    "One Rigidbody with a remaining solid in ExitThreshold must not publish a duplicate request.");
                Assert.AreEqual(1, admission.Requests.Count);

                body.MovePosition(center + Vector2.down * (thresholdCollider.bounds.extents.y + 1.5f));
                Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.IsFalse(childSolid.IsTouching(thresholdCollider),
                    "TR01 precondition: the body must be fully outside before it can rearm.");
                body.MovePosition(center + Vector2.up * 1.2f);
                Physics2D.Simulate(Time.fixedDeltaTime);
                body.MovePosition(center + Vector2.up * .1f);
                Physics2D.Simulate(Time.fixedDeltaTime);
                body.MovePosition(center + Vector2.down * .15f);
                Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.AreEqual(2, published.Count,
                    "ExitThreshold must rearm for a new crossing only after every Player solid exits.");
                Assert.AreEqual(2, admission.Requests.Count);
            }
            finally
            {
                SceneTransitionRouter.UnregisterTownInteriorVisitAdmission(admission);
                GameEventBus.Unsubscribe(capturePublished);
                Physics2D.simulationMode = previousSimulationMode;
                UnityEngine.Object.Destroy(interior);
                UnityEngine.Object.Destroy(player);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator Loader_RealAdditiveScene_InstantiatesRenkoRoomAndUnloadsWithoutChangingActiveScene()
        {
            const string sceneName = "TownInteriorScene";
            const string houseId = "House_Residential_2";
            var activeScene = SceneManager.GetActiveScene();
            Assert.IsFalse(SceneManager.GetSceneByName(sceneName).isLoaded);
            _additiveOperationId = Guid.NewGuid().ToString("N");
            _additiveLoader = new TownInteriorSceneLoader();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Game/Prefabs/City/Interiors/House_Residential_2.prefab");
            Assert.IsNotNull(prefab, "The Renko pilot must use the materialized prefab asset.");
            var marker = prefab.GetComponent<TownInteriorInstance>();
            Assert.IsNotNull(marker);
            Assert.IsTrue(marker.TryValidate(houseId, out var prefabReason), prefabReason);

            bool? loaded = null;
            string loadReason = null;
            yield return _additiveLoader.Load(_additiveOperationId, sceneName, houseId,
                prefab, new Vector2(4096f, 4096f),
                new Bounds(Vector3.zero, new Vector3(1000f, 1000f, 0f)),
                (success, reason) => { loaded = success; loadReason = reason; });
            Assert.IsTrue(loaded.HasValue && loaded.Value, loadReason);
            var additiveScene = SceneManager.GetSceneByName(sceneName);
            var instance = _additiveLoader.Instance;
            Assert.IsTrue(additiveScene.IsValid() && additiveScene.isLoaded);
            Assert.AreEqual(activeScene.handle, SceneManager.GetActiveScene().handle);
            Assert.IsTrue(_additiveLoader.IsLoaded);
            Assert.IsNotNull(instance);
            Assert.AreNotSame(marker, instance);
            Assert.AreEqual(additiveScene.handle, instance.gameObject.scene.handle);
            Assert.AreEqual(new Vector3(4096f, 4096f, 0f), instance.transform.position);
            Assert.AreEqual(houseId, instance.HouseId);
            Assert.AreEqual(new Vector2(16f, 12f), (Vector2)instance.WorldBounds.size);
            Assert.AreEqual(marker.EntryAnchor.localPosition, instance.EntryAnchor.localPosition);
            Assert.AreEqual(marker.ExitAnchor.localPosition, instance.ExitAnchor.localPosition);
            Assert.AreEqual(instance.transform.TransformPoint(marker.EntryAnchor.localPosition), instance.EntryAnchor.position);

            yield return _additiveLoader.Unload(_additiveOperationId);
            Assert.AreEqual(activeScene.handle, SceneManager.GetActiveScene().handle);
            Assert.IsFalse(SceneManager.GetSceneByName(sceneName).isLoaded);
            Assert.IsFalse(_additiveLoader.IsLoaded);
            Assert.IsNull(_additiveLoader.Instance);
            Assert.IsTrue(instance == null);
        }
#endif

        [UnityTest]
        public IEnumerator SaveManager_RejectedInteriorRestore_DiscardsDeferredAutosaveWithoutDiskWrite()
        {
            var host = new GameObject("RejectedRestoreHost", typeof(TownInteriorVisitController));
            var saveHost = new GameObject("RejectedRestoreSaveManager", typeof(SaveManager));
            var manager = saveHost.GetComponent<SaveManager>();
            int loadedCount = 0;
            bool wasSuccessful = true;
            Action<GameLoadedEvent> loaded = evt =>
            {
                loadedCount++;
                wasSuccessful = evt.WasSuccessful;
                // Independent safety guard: even if deferred-save cleanup regresses, this
                // isolated fixture must never flush to the user's persistent save path.
                manager.UnbindTownInteriorRuntime(host.GetComponent<TownInteriorVisitController>());
            };
            GameEventBus.Subscribe(loaded);
            try
            {
                InitializeInMemorySaveManager(manager);
                manager.BindTownInteriorRuntime(host.GetComponent<TownInteriorVisitController>(),
                    SceneManager.GetActiveScene().handle);
                SetPrivate(manager, "_autosaveDeferred", true);
                SetPrivate(manager, "_interiorLoadInProgress", true);
                var data = new GameSaveData { TownInteriorLocation = new TownInteriorLocationSaveData
                {
                    HouseId = "removed-house", AnchorId = "entry", ReturnAnchorId = "return",
                    FacingY = 1f, SafeTownX = 1005f, SafeTownY = 1005f
                } };
                var routine = (IEnumerator)typeof(SaveManager).GetMethod("ApplyTownInteriorLoad",
                    BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, new object[] { data });
                manager.StartCoroutine(routine);
                float deadline = Time.realtimeSinceStartup + 1f;
                while (loadedCount == 0 && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(1, loadedCount);
                Assert.IsFalse(wasSuccessful);
                Assert.IsFalse((bool)GetPrivate(manager, "_autosaveDeferred"));
                Assert.IsNull(GetPrivate(manager, "_pendingInteriorSaveData"));
                Assert.AreEqual(string.Empty, GetPrivate(manager, "_pendingInteriorRestoreToken"));
            }
            finally
            {
                GameEventBus.Unsubscribe(loaded);
                manager.Shutdown();
                UnityEngine.Object.Destroy(saveHost);
                UnityEngine.Object.Destroy(host);
            }
        }

        [UnityTest]
        public IEnumerator SaveManager_UnknownHouseRestoresStateUnderBlack_ThenPublishesLoadedOnce()
        {
            var actor = new GameObject("SaveRestoreActor", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlayerManager));
            var body = actor.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            actor.transform.position = new Vector3(1000f, 1000f, 0f);
            body.position = new Vector2(1000f, 1000f);
            var player = actor.GetComponent<PlayerManager>();
            player.RestoreState(30, 17, 2);
            var fallback = new GameObject("SaveRestoreFallback");
            fallback.transform.position = new Vector3(1005f, 1005f);
            var overlay = new GameObject("SaveRestoreOverlay", typeof(CanvasGroup), typeof(SceneFadeOverlayController));
            var group = overlay.GetComponent<CanvasGroup>();
            var host = new GameObject("SaveRestoreHost", typeof(TownInteriorVisitController));
            var controller = host.GetComponent<TownInteriorVisitController>();
            controller.Configure(actor.transform, actor.GetComponent<Collider2D>(),
                Array.Empty<TownInteriorVisitController.HouseBinding>(), "TownInteriorScene",
                new Bounds(new Vector3(1000f, 1000f), new Vector3(100f, 100f)), new Vector2(4096f, 4096f),
                .000001f, .000001f, .75f, 2f, _ => true);
            controller.BindRuntime(actor.transform, actor.GetComponent<Collider2D>(), _ => true,
                () => true, fallback.transform, () => Vector2.up);
            var saveHost = new GameObject("InMemorySaveManager", typeof(SaveManager));
            var manager = saveHost.GetComponent<SaveManager>();
            SetPrivate(manager, "_playerManager", player);
            SetPrivate(manager, "_playerTransform", actor.transform);
            var data = new GameSaveData
            {
                CurrentDay = 1,
                Player = new PlayerSaveData { MaxHP = 80, CurrentHP = 41, Gold = 73,
                    PlayerPosition = new Vector2(1005f, 1005f) },
                TownInteriorLocation = new TownInteriorLocationSaveData { HouseId = "removed-house",
                    AnchorId = "entry", ReturnAnchorId = "return", LocalX = 1f, LocalY = 2f,
                    FacingY = 1f, SafeTownX = 1005f, SafeTownY = 1005f }
            };
            int loadedCount = 0, preparationCount = 0, placementCount = 0;
            bool loadedSuccess = false;
            string token = null;
            Action<GameLoadedEvent> loaded = evt => { loadedCount++; loadedSuccess = evt.WasSuccessful; };
            Action<TownInteriorRestoreStateRequestedEvent> preparing = evt =>
            {
                token = evt.OperationId;
                preparationCount++;
                Assert.GreaterOrEqual(group.alpha, .99f);
                Assert.AreEqual(17, player.CurrentHP);
                Assert.AreEqual(0, loadedCount);
            };
            Action<TownInteriorPlayerPlacementRequestedEvent> placing = evt =>
            {
                placementCount++;
                Assert.AreEqual(token, evt.OperationId);
                Assert.GreaterOrEqual(group.alpha, .99f);
                Assert.AreEqual(41, player.CurrentHP);
                Assert.AreEqual(0, loadedCount);
                body.position = new Vector2(evt.WorldX, evt.WorldY);
                Physics2D.SyncTransforms();
                GameEventBus.Publish(new TownInteriorPlayerPlacementCompletedEvent(evt.OperationId, true));
            };
            GameEventBus.Subscribe(loaded);
            GameEventBus.Subscribe(preparing);
            GameEventBus.Subscribe(placing);
            try
            {
                InitializeInMemorySaveManager(manager, player);
                manager.BindTownInteriorRuntime(controller, SceneManager.GetActiveScene().handle);
                SetPrivate(manager, "_townInteriorRestoreTimeoutSeconds", 2f);
                SetPrivate(manager, "_interiorLoadInProgress", true);
                Physics2D.SyncTransforms();
                var routine = (IEnumerator)typeof(SaveManager).GetMethod("ApplyTownInteriorLoad",
                    BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, new object[] { data });
                manager.StartCoroutine(routine);
                float deadline = Time.realtimeSinceStartup + 3f;
                while (loadedCount == 0 && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(1, preparationCount);
                Assert.AreEqual(1, placementCount);
                Assert.AreEqual(1, loadedCount);
                Assert.IsTrue(loadedSuccess);
                Assert.AreEqual(41, player.CurrentHP);
                Assert.AreEqual(73, player.CurrentGold);
                Assert.AreEqual((Vector2)fallback.transform.position, body.position);
                Assert.IsFalse(controller.CaptureCommitted().IsInside);
                GameEventBus.Publish(new TownInteriorVisitFinishedEvent("stale-token", string.Empty, true, string.Empty));
                GameEventBus.Publish(new TownInteriorVisitFinishedEvent(token, string.Empty, true, string.Empty));
                yield return null;
                Assert.AreEqual(1, loadedCount, "Late completion must not publish another load.");
            }
            finally
            {
                GameEventBus.Unsubscribe(loaded);
                GameEventBus.Unsubscribe(preparing);
                GameEventBus.Unsubscribe(placing);
                manager.Shutdown();
                UnityEngine.Object.Destroy(saveHost);
                UnityEngine.Object.Destroy(host);
                UnityEngine.Object.Destroy(overlay);
                UnityEngine.Object.Destroy(fallback);
                UnityEngine.Object.Destroy(actor);
            }
        }

        [UnityTest]
        public IEnumerator Restore_UnknownHousePreparesUnderBlackBeforeFallbackPlacement()
        {
            var actor = new GameObject("RestoreActor", typeof(Rigidbody2D), typeof(BoxCollider2D));
            var body = actor.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            actor.transform.position = new Vector3(1000f, 1000f, 0f);
            body.position = new Vector2(1000f, 1000f);
            var fallback = new GameObject("RegisteredFallback");
            fallback.transform.position = new Vector3(1005f, 1005f, 0f);
            var overlay = new GameObject("RestoreOverlay", typeof(CanvasGroup), typeof(SceneFadeOverlayController));
            var group = overlay.GetComponent<CanvasGroup>();
            var host = new GameObject("RestoreHost", typeof(TownInteriorVisitController));
            var controller = host.GetComponent<TownInteriorVisitController>();
            controller.Configure(actor.transform, actor.GetComponent<Collider2D>(),
                Array.Empty<TownInteriorVisitController.HouseBinding>(), "TownInteriorScene",
                new Bounds(new Vector3(1000f, 1000f), new Vector3(100f, 100f)), new Vector2(4096f, 4096f),
                .000001f, .000001f, .75f, 2f, _ => true);
            controller.BindRuntime(actor.transform, actor.GetComponent<Collider2D>(), _ => true,
                () => true, fallback.transform, () => Vector2.up);
            string token = Guid.NewGuid().ToString("N");
            bool prepared = false, placed = false, finished = false, succeeded = false;
            Action<TownInteriorRestoreStateRequestedEvent> prepare = evt =>
            {
                if (evt.OperationId != token) return;
                Assert.GreaterOrEqual(group.alpha, .99f);
                prepared = true;
                GameEventBus.Publish(new TownInteriorRestoreStateReadyEvent(token, true));
            };
            Action<TownInteriorPlayerPlacementRequestedEvent> place = evt =>
            {
                if (evt.OperationId != token) return;
                Assert.IsTrue(prepared);
                Assert.GreaterOrEqual(group.alpha, .99f);
                body.position = new Vector2(evt.WorldX, evt.WorldY);
                Physics2D.SyncTransforms();
                placed = true;
                GameEventBus.Publish(new TownInteriorPlayerPlacementCompletedEvent(token, true));
            };
            Action<TownInteriorVisitFinishedEvent> finish = evt =>
            { if (evt.OperationId == token) { finished = true; succeeded = evt.Success; } };
            GameEventBus.Subscribe(prepare);
            GameEventBus.Subscribe(place);
            GameEventBus.Subscribe(finish);
            try
            {
                Physics2D.SyncTransforms();
                Assert.IsTrue(controller.TryRequestRestore(new TownInteriorLocationSnapshot(
                    "removed-house", "entry", "return", 1f, 2f, 0f, 1f, 9f, 9f), token, out var reason), reason);
                float deadline = Time.realtimeSinceStartup + 3f;
                while (!finished && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.IsTrue(prepared && placed && finished && succeeded);
                Assert.AreEqual((Vector2)fallback.transform.position, body.position);
                Assert.IsFalse(controller.CaptureCommitted().IsInside);
                Assert.LessOrEqual(group.alpha, .01f);
            }
            finally
            {
                GameEventBus.Unsubscribe(prepare);
                GameEventBus.Unsubscribe(place);
                GameEventBus.Unsubscribe(finish);
                UnityEngine.Object.Destroy(host);
                UnityEngine.Object.Destroy(overlay);
                UnityEngine.Object.Destroy(fallback);
                UnityEngine.Object.Destroy(actor);
            }
        }

        [UnityTest]
        public IEnumerator Loader_ConcurrentCleanupCannotClearNextOwner()
        {
            var scene = SceneManager.CreateScene("TownCleanupTest_" + Guid.NewGuid().ToString("N"));
            var content = new GameObject("OwnedInterior", typeof(TownInteriorInstance));
            SceneManager.MoveGameObjectToScene(content, scene);
            var next = new GameObject("NextInterior", typeof(TownInteriorInstance));
            var loader = new TownInteriorSceneLoader();
            SetPrivate(loader, "_ownedScene", scene);
            SetPrivate(loader, "_ownedObject", content);
            SetPrivate(loader, "_ownerToken", "old");
            try
            {
                var waiter = loader.Unload("old");
                Assert.IsTrue(waiter.MoveNext());
                loader.CancelAndUnload("old");
                float deadline = Time.realtimeSinceStartup + 5f;
                while (loader.Instance != null && Time.realtimeSinceStartup < deadline) yield return null;
                yield return null;
                Assert.IsFalse(scene.isLoaded);
                SetPrivate(loader, "_ownedObject", next);
                SetPrivate(loader, "_ownerToken", "new");
                Assert.IsFalse(waiter.MoveNext());
                Assert.AreSame(next.GetComponent<TownInteriorInstance>(), loader.Instance);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded) SceneManager.UnloadSceneAsync(scene);
                UnityEngine.Object.Destroy(next);
            }
        }

        [UnityTest]
        public IEnumerator DisabledController_PreservesOccupiedContentAndPlayerPosition()
        {
            var actor = new GameObject("InteriorLifecycleActor", typeof(Rigidbody2D), typeof(BoxCollider2D));
            actor.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            actor.transform.position = new Vector3(4096f, 4096f, 0f);
            var content = new GameObject("InteriorLifecycleContent", typeof(TownInteriorInstance));
            content.transform.position = actor.transform.position;
            var instance = content.GetComponent<TownInteriorInstance>();
            SetPrivate(instance, "_houseId", "House_Test");
            SetPrivate(instance, "_usableSize", new Vector2(12f, 10f));
            var host = new GameObject("InteriorLifecycleHost", typeof(TownInteriorVisitController));
            var controller = host.GetComponent<TownInteriorVisitController>();
            controller.Configure(actor.transform, actor.GetComponent<Collider2D>(),
                Array.Empty<TownInteriorVisitController.HouseBinding>(), "TownInteriorScene",
                new Bounds(Vector3.zero, new Vector3(100f, 100f, 0f)), new Vector2(4096f, 4096f),
                .2f, .25f, .75f, 10f, _ => true);
            var loader = (TownInteriorSceneLoader)typeof(TownInteriorVisitController)
                .GetField("_loader", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller);
            // Seed a committed occupancy, not a scene load. This regression only covers disable lifecycle.
            SetPrivate(loader, "_ownedObject", content);
            SetPrivate(loader, "_ownerToken", "occupied-content");
            SetPrivate(controller, "_contentOwnerToken", "occupied-content");
            SetPrivate(controller, "_committed", new TownInteriorLocationSnapshot(
                "House_Test", "entry", "return", 0f, 0f, 0f, 1f, 10f, 10f));
            try
            {
                Physics2D.SyncTransforms();
                var before = actor.transform.position;
                controller.enabled = false;
                yield return null;
                Assert.AreSame(instance, loader.Instance, "Occupied content must not be discarded without a confirmed exit.");
                Assert.AreEqual(before, actor.transform.position);
            }
            finally
            {
                UnityEngine.Object.Destroy(host);
                UnityEngine.Object.Destroy(content);
                UnityEngine.Object.Destroy(actor);
            }
        }

        private static void SetPrivate(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var match = root.GetComponentInChildren<T>(true);
                if (match != null) return match;
            }
            return null;
        }

        private static T FindInLoadedScenes<T>(Func<T, bool> predicate, Scene extraScene) where T : Component
        {
            foreach (var match in FindAllInLoadedScenes<T>())
                if (predicate(match)) return match;
            if (extraScene.IsValid() && extraScene.isLoaded)
                foreach (var match in FindAllInScene<T>(extraScene)) if (predicate(match)) return match;
            return null;
        }

        private static T[] FindAllInLoadedScenes<T>() where T : Component
        {
            var results = new List<T>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects()) results.AddRange(root.GetComponentsInChildren<T>(true));
            }
            return results.ToArray();
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (var root in scene.GetRootGameObjects()) results.AddRange(root.GetComponentsInChildren<T>(true));
            return results.ToArray();
        }

        private void CapturePersistentRootIds(ISet<GameObject> ids)
        {
            ids.Clear();
            var probe = new GameObject("TownTestPersistentSceneProbe");
            UnityEngine.Object.DontDestroyOnLoad(probe);
            _integrationPersistentScene = probe.scene;
            foreach (var root in _integrationPersistentScene.GetRootGameObjects())
                if (root != probe) ids.Add(root);
            UnityEngine.Object.Destroy(probe);
        }

        private static string SnapshotSaveFiles(string savePath)
        {
            var builder = new StringBuilder();
            foreach (var path in new[] { savePath, savePath + ".bak", savePath + ".tmp" })
            {
                builder.Append(path).Append('=');
                if (File.Exists(path))
                {
                    using var sha = SHA256.Create();
                    builder.Append(Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path))));
                }
                else builder.Append("<missing>");
                builder.Append(';');
            }
            return builder.ToString();
        }

        private static IEnumerator WalkPilotTo(Rigidbody2D body, ControlledMovementKeys keys, Vector2 target)
        {
            float deadline = Time.realtimeSinceStartup + 12f;
            while (Vector2.Distance(body.position, target) > .12f && Time.realtimeSinceStartup < deadline)
            {
                var delta = target - body.position;
                keys.Up = delta.y > .08f; keys.Down = delta.y < -.08f;
                keys.Left = delta.x < -.08f; keys.Right = delta.x > .08f;
                yield return new WaitForFixedUpdate();
            }
            keys.Up = keys.Down = keys.Left = keys.Right = false;
            Assert.Less(Vector2.Distance(body.position, target), .15f,
                $"Furnished pilot route stalled: body={body.position}, target={target}.");
        }

        private static void CaptureTownPilotFrame(string directory, UnityEngine.Camera camera, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directory)) return;
            Debug.Log($"[RenkoCapture] {fileName}: camera={camera.transform.position}, player={PlayerController.ActiveInstance.transform.position}, body={PlayerController.ActiveInstance.GetComponent<Rigidbody2D>().position}.");
            Assert.IsNotNull(camera, "Opt-in Town capture requires the real gameplay camera.");
            Assert.AreNotEqual(UnityEngine.Rendering.GraphicsDeviceType.Null, SystemInfo.graphicsDeviceType,
                "TOWN_CAPTURE_DIR requires a graphics device capable of Camera.Render.");
            const int width = 1280, height = 720;
            var priorTarget = camera.targetTexture;
            var priorActive = RenderTexture.active;
            var priorAspect = camera.aspect;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                // Preserve the camera's transform/orthographic size; only use the output's 16:9 aspect.
                camera.aspect = (float)width / height;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, fileName), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = priorTarget;
                camera.aspect = priorAspect;
                RenderTexture.active = priorActive;
                if (image != null) UnityEngine.Object.Destroy(image);
                target.Release();
                UnityEngine.Object.Destroy(target);
            }
        }

        private static void SetTownDayHour(GameTimeManager clock, int hour)
        {
            Assert.That(hour, Is.InRange(6, 17));
            clock.RestorePhaseState((int)GamePhaseChangedEvent.GamePhase.Day,
                clock.CurrentPhaseDurationSeconds * ((hour - 6 + .1f) / 12f));
            Assert.AreEqual(hour, clock.CurrentHourOfDay);
        }

        private static void AssertEventOrder(IReadOnlyList<string> events, string operationId)
        {
            var black = IndexOf(events, operationId + ":black");
            var placementRequested = IndexOf(events, operationId + ":place-request");
            var placed = IndexOf(events, operationId + ":placed");
            var finished = IndexOf(events, operationId + ":finished");
            Assert.GreaterOrEqual(black, 0, "Missing real blackout-ready event for " + operationId);
            Assert.Greater(placementRequested, black, "Placement request must follow blackout for " + operationId);
            Assert.Greater(placed, placementRequested, "Placement-ready must follow the real placement request for " + operationId);
            Assert.Greater(finished, placed, "Finish must follow placement-ready for " + operationId);
        }

        private static int IndexOf(IReadOnlyList<string> values, string expected)
        {
            for (var i = 0; i < values.Count; i++) if (values[i] == expected) return i;
            return -1;
        }

        private static string DescribeBlockedPath(string phase, Rigidbody2D body, TownInteriorThreshold threshold)
        {
            var contacts = new ContactPoint2D[16];
            var count = body != null ? body.GetContacts(contacts) : 0;
            var names = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                if (i > 0) names.Append(", ");
                names.Append(contacts[i].collider != null ? contacts[i].collider.name : "<none>");
            }
            return $"{phase} stalled: body={(body != null ? body.position.ToString() : "<null>")}, " +
                   $"threshold={(threshold != null ? threshold.transform.position.ToString() : "<null>")}, " +
                   $"contacts=[{names}]";
        }

        private static object GetPrivate(object target, string field) =>
            target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);

        // Only the providers used by this in-memory restore are wired. Full Initialize registers
        // process-wide runtime state and would contaminate unrelated PlayMode fixtures.
        private static void InitializeInMemorySaveManager(SaveManager manager, PlayerManager player = null)
        {
            typeof(SaveManager).GetField("<IsInitialized>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, true);
            if (player != null)
                SetPrivate(manager, "_playerProvider", new PlayerSectionProvider(player, null, null,
                    () => (Vector2)player.transform.position));
            var registry = (SaveProviderRegistry)GetPrivate(manager, "_providerRegistry");
            registry.Register<HotbarSaveData>(new HotbarSectionProvider(new HotbarState()), 0);
            typeof(SaveManager).GetMethod("SubscribeTownInteriorFinished",
                BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, null);
        }

        // Adapter regression only: not evidence of a materialized room or save round-trip.
        [UnityTest]
        public IEnumerator Overlay_CancelKeepsBlack_RejectsLatePlacementAndSingleCompletion()
        {
            var root = new GameObject("TownOverlayTest", typeof(CanvasGroup));
            var group = root.GetComponent<CanvasGroup>();
            root.AddComponent<SceneFadeOverlayController>();
            string token = Guid.NewGuid().ToString("N");
            bool black = false, finished = false;
            Action<TownInteriorBlackoutReadyEvent> onBlack = evt =>
            { if (evt.OperationId == token) { Assert.GreaterOrEqual(group.alpha, .99f); black = true; } };
            Action<TownInteriorFadeFinishedEvent> onFinished = evt =>
            { if (evt.OperationId == token) finished = true; };
            GameEventBus.Subscribe(onBlack);
            GameEventBus.Subscribe(onFinished);
            try
            {
                GameEventBus.Publish(new TownInteriorBlackoutRequestedEvent(token, 0f, 0f, .75f));
                yield return null;
                yield return null;
                Assert.IsTrue(black);
                GameEventBus.Publish(new SceneTransitionCompletedEvent("TownScene", "unrelated", Vector2.zero));
                GameEventBus.Publish(new TownInteriorVisitCancelledEvent(token));
                GameEventBus.Publish(new TownInteriorPlacementReadyEvent("old-token", "House"));
                GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, "House"));
                yield return null;
                yield return null;
                Assert.GreaterOrEqual(group.alpha, .99f);
                Assert.IsFalse(finished);
                GameEventBus.Publish(new TownInteriorPlacementReadyEvent(token, string.Empty));
                yield return null;
                yield return null;
                Assert.IsTrue(finished);
                Assert.AreEqual(0f, group.alpha);
            }
            finally
            {
                GameEventBus.Unsubscribe(onBlack);
                GameEventBus.Unsubscribe(onFinished);
                UnityEngine.Object.Destroy(root);
            }
        }
    }
}
