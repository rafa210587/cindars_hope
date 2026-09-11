using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CindarsHope.Camera;
using CindarsHope.Diagnostics;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Optional live door/water proof. Framing setup is separate from controlled physical traversal.</summary>
    internal sealed class FarmDoorAndWaterProbe : IDisposable
    {
        [Serializable] internal sealed class Sample
        {
            public string phase, path, leafSprite, eventMarker;
            public float time, fixedTime, roofAlpha;
            public int doorFrame, waterFrame = -1, fixedSteps;
            public int waterAnimationFlagBits;
            public string waterAnimationFlags;
            public bool blockerEnabled, doorOpen, doorAnimating;
            public Vector2 playerPosition;
        }
        [Serializable] internal sealed class Evidence
        {
            public string status = "RUNNING", reason;
            public float traversalCaptureInterval = 0.09f;
            public string timing = "Every PNG carries its observed Time.time and fixedTime; preview timing must use consecutive timestamp deltas, never an invented constant playback rate. Moving-body snapshots target 0.09s intervals; stationary duplicates are omitted. Door poses are captured when observed to change. Water PNGs are sampled periodically regardless of frame changes and tagged after Camera.Render; identical frames are retained as evidence.";
            public string method = "Initial framing at the earlier physically validated house selection position. Thereafter actual player Rigidbody2D.MovePosition in FixedUpdate, with PlayerController temporarily disabled. Live selector invokes only HouseDoorInteractable. No forced alpha/door state, injected input, portal, save or load. Water reads actual Tilemap.GetAnimationFrame. This is controlled-body traversal, not a movement-input test.";
            public Vector2 originalPlayerPosition, preparedPlayerPosition;
            public Vector3 houseCameraPosition, lakeCameraPosition;
            public float cameraAspect, orthographicSize;
            public int width, height, waterAnimationFrameCount;
            public string waterColliderStateBefore, waterColliderStateAfter, waterAnimationFlagsBefore;
            public bool waterAdvancedObserved, waterWrapObserved;
            public int waterDistinctFrames;
            public float waterSampleDuration;
            public List<string> checks = new List<string>();
            public List<Sample> samples = new List<Sample>();
        }
        [Serializable] private sealed class PolygonState
        {
            public string entityId;
            public string name;
            public bool enabled, isTrigger;
            public Vector2 offset;
            public Matrix4x4 localToWorld;
            public int[] pathLengths;
            public Vector2[] points;
        }
        [Serializable] private sealed class WaterState { public PolygonState[] polygons; }
        private enum Phase { Approach, ClosedPush, Opening, Doorway, Inside, Exit, Closing, ClosedRetest, Water, Finished }
        private readonly PlayerController player;
        private readonly Rigidbody2D body;
        private readonly Collider2D solid;
        private readonly InteractionSystem selector;
        private readonly HouseDoorInteractable door;
        private readonly BoxCollider2D blocker, usefulInterior;
        private readonly SpriteRenderer leaf, roof;
        private readonly Sprite[] frames;
        private readonly SpriteRenderer[] interiors;
        private readonly Tilemap water;
        private readonly PolygonCollider2D[] waterColliders;
        private readonly UnityEngine.Camera camera;
        private readonly CameraFollow2D follow;
        private readonly bool playerEnabled, followEnabled;
        private readonly Vector3 originalPlayerPosition, originalCameraPosition;
        private readonly Vector2 originalVelocity;
        private readonly float originalAngularVelocity, originalOrtho;
        private readonly string outputDirectory;
        private readonly FarmProbeBodyDriver driver;
        private readonly Vector2 outside, inDoorway, inside;
        private readonly Vector3Int waterCell;
        private readonly HashSet<int> seenDoorFrames = new HashSet<int>();
        private readonly HashSet<int> seenClosingFrames = new HashSet<int>();
        private readonly HashSet<int> seenWaterFrames = new HashSet<int>();
        private Phase phase;
        private float phaseStarted, started, lastWaterCapture;
        private int lastDoorFrame = -1, lastClosingFrame = -1, lastWaterFrame = -1;
        private bool disposed;
        internal Evidence Result { get; } = new Evidence();

        internal FarmDoorAndWaterProbe(PlayerController player, UnityEngine.Camera camera,
            FarmInteractionSelectionProbe.Evidence selections, string outputDirectory)
        {
            this.player = player;
            this.camera = camera;
            Require(player.CompareTag("Player"), "Farm player must carry the Player identity consumed by doors/reveal.");
            this.outputDirectory = Path.Combine(outputDirectory, "motion");
            originalPlayerPosition = player.transform.position;
            originalCameraPosition = camera.transform.position;
            originalOrtho = camera.orthographicSize;
            playerEnabled = player.enabled;
            follow = camera.GetComponent<CameraFollow2D>();
            Require(follow != null, "Motion evidence requires the original gameplay camera follow.");
            followEnabled = follow.enabled;
            body = player.GetComponent<Rigidbody2D>();
            Require(body != null && body.bodyType == RigidbodyType2D.Dynamic, "Motion probe requires the actual dynamic player body.");
            originalVelocity = body.linearVelocity;
            originalAngularVelocity = body.angularVelocity;
            solid = player.GetComponentsInChildren<Collider2D>().Single(c => c.enabled && !c.isTrigger);
            selector = player.GetComponentsInChildren<InteractionSystem>().Single();
            var roots = player.gameObject.scene.GetRootGameObjects();
            door = roots.SelectMany(r => r.GetComponentsInChildren<HouseDoorInteractable>(true))
                .Single(d => d.transform.parent != null && d.transform.parent.name == "FarmHouse");
            blocker = door.GetComponents<BoxCollider2D>().Single(c => !c.isTrigger);
            var doorData = new SerializedObject(door);
            leaf = (SpriteRenderer)doorData.FindProperty("_leafRenderer").objectReferenceValue;
            var frameData = doorData.FindProperty("_closedToOpenFrames");
            frames = new Sprite[frameData.arraySize];
            for (var i = 0; i < frames.Length; i++) frames[i] = (Sprite)frameData.GetArrayElementAtIndex(i).objectReferenceValue;
            Require(frames.Length >= 3 && leaf != null && !door.IsOpen && blocker.enabled, "Expected configured animated door initially closed.");
            var reveal = door.transform.parent.GetComponentsInChildren<RoofRevealController>(true).Single();
            var revealData = new SerializedObject(reveal);
            usefulInterior = (BoxCollider2D)revealData.FindProperty("_feetInterior").objectReferenceValue;
            Require(usefulInterior != null, "Farm roof must opt in to useful-interior feet occupancy.");
            roof = door.transform.parent.Find("Roof").GetComponent<SpriteRenderer>();
            var interiorData = revealData.FindProperty("_interiorRenderers");
            interiors = new SpriteRenderer[interiorData.arraySize];
            for (var i = 0; i < interiors.Length; i++) interiors[i] = (SpriteRenderer)interiorData.GetArrayElementAtIndex(i).objectReferenceValue;
            Require(interiors.Length > 0, "House interior renderers are not configured.");
            water = roots.SelectMany(r => r.GetComponentsInChildren<Tilemap>(true)).Single(t => t.name == "Water");
            waterCell = water.WorldToCell(new Vector3(24f, -10f));
            Require(water.GetTile(waterCell) != null, "No water tile at world (24,-10).");
            Result.waterAnimationFrameCount = water.GetAnimationFrameCount(waterCell);
            Require(Result.waterAnimationFrameCount == 6, "Expected the authored six-frame water tile at world (24,-10).");
            waterColliders = roots.SelectMany(r => r.GetComponentsInChildren<PolygonCollider2D>(true))
                .Where(c => c.name == "Collision_farm_spatial_lake" || c.name == "RiverCollider_AboveBridge" ||
                    c.name == "RiverCollider_BelowBridge")
                .OrderBy(c => c.name, StringComparer.Ordinal).ToArray();
            Require(waterColliders.Length == 3 && waterColliders.All(c => c.enabled && !c.isTrigger),
                "Expected enabled solid lake and both river physical paths.");
            var checkedPosition = selections.selections.Single(s => s.id == "house" && s.status == "PASS").checkedPosition;
            var centerOffset = (Vector2)solid.bounds.center - body.position;
            var doorCenter = (Vector2)blocker.transform.TransformPoint(blocker.offset);
            outside = new Vector2(doorCenter.x, blocker.bounds.min.y - solid.bounds.extents.y - centerOffset.y - 0.25f);
            inDoorway = new Vector2(doorCenter.x, doorCenter.y - centerOffset.y);
            inside = new Vector2(doorCenter.x, usefulInterior.bounds.min.y + 0.45f);
            Result.originalPlayerPosition = originalPlayerPosition;
            Result.preparedPlayerPosition = checkedPosition;
            Result.cameraAspect = camera.aspect;
            Result.orthographicSize = originalOrtho;
            Result.width = camera.pixelWidth;
            Result.height = camera.pixelHeight;
            Require(Result.width > 0 && Result.height > 0, "Gameplay render dimensions are invalid.");
            Directory.CreateDirectory(this.outputDirectory);
            try
            {
                player.enabled = false;
                follow.enabled = false;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.position = checkedPosition; // Framing preparation, never traversal evidence.
                player.transform.position = new Vector3(checkedPosition.x, checkedPosition.y, originalPlayerPosition.z);
                Physics2D.SyncTransforms();
                driver = player.gameObject.AddComponent<FarmProbeBodyDriver>();
                driver.Body = body;
                camera.transform.position = new Vector3(door.transform.parent.position.x,
                    door.transform.parent.position.y + 0.5f, originalCameraPosition.z);
                Result.houseCameraPosition = camera.transform.position;
                started = Time.time;
                ChangePhase(Phase.Approach, outside);
            }
            catch { Dispose(); throw; }
        }

        internal bool Tick()
        {
            if (phase == Phase.Finished) return true;
            try
            {
                Require(Time.time - started < 20f, "Motion phase exceeded 20 gameplay seconds.");
                Require(Mathf.Abs(camera.aspect - Result.cameraAspect) < 0.0001f, "Gameplay aspect changed during motion evidence.");
                var elapsed = Time.time - phaseStarted;
                CaptureTraversal();
                switch (phase)
                {
                    case Phase.Approach:
                        if (!At(outside)) { Require(elapsed < 3f, "Controlled approach could not reach the aligned doorway."); break; }
                        Require(roof.color.a > 0.99f, "House revealed while feet were outside.");
                        Capture("house_approach");
                        ChangePhase(Phase.ClosedPush, inside);
                        break;
                    case Phase.ClosedPush:
                        if (elapsed < 0.8f) break;
                        AssertClosedStop();
                        Result.checks.Add("Closed door stopped the real driven player body.");
                        Stop(); Capture("closed_blocked");
                        SelectAndInteract();
                        ChangePhase(Phase.Opening);
                        SampleOpening();
                        break;
                    case Phase.Opening:
                        SampleOpening();
                        if (door.IsAnimating) { Require(elapsed < 2f, "Door opening did not complete."); break; }
                        Require(door.IsOpen && !blocker.enabled && seenDoorFrames.Count >= 3,
                            "Opening lacked intermediate poses or a clear final pose.");
                        Result.checks.Add("Opening exposed intermediate frames; blocker released only at final clear pose.");
                        ChangePhase(Phase.Doorway, inDoorway);
                        break;
                    case Phase.Doorway:
                        if (!At(inDoorway)) { Require(elapsed < 2f, "Open door did not admit the body into the gap."); break; }
                        Stop();
                        SelectAndInteract();
                        Require(door.IsOpen && !door.IsAnimating && !blocker.enabled, "Door began closing onto the occupying body.");
                        Result.checks.Add("Live close interaction was refused with the player body occupying the doorway.");
                        Capture("occupied_safe_close");
                        ChangePhase(Phase.Inside, inside);
                        break;
                    case Phase.Inside:
                        if (!At(inside) || elapsed < 0.2f) { Require(elapsed < 2f, "Player could not enter the useful interior."); break; }
                        Require(roof.color.a < 0.01f && interiors.All(r => r != null && r.enabled), "Feet entry did not reveal the live interior.");
                        Stop(); Capture("inside_revealed");
                        Result.checks.Add("Actual feet entry revealed configured interior without forcing alpha.");
                        ChangePhase(Phase.Exit, outside);
                        break;
                    case Phase.Exit:
                        if (!At(outside) || elapsed < 0.2f) { Require(elapsed < 2f, "Player could not leave through the open doorway."); break; }
                        Require(roof.color.a > 0.99f && interiors.All(r => r != null && !r.enabled), "Feet exit did not restore the exterior.");
                        Stop(); Capture("outside_restored");
                        Result.checks.Add("Actual feet exit restored the exterior and hid the interior.");
                        SelectAndInteract();
                        ChangePhase(Phase.Closing);
                        SampleClosing();
                        break;
                    case Phase.Closing:
                        SampleClosing();
                        if (door.IsAnimating) { Require(elapsed < 2f, "Closing did not complete."); break; }
                        Require(!door.IsOpen && blocker.enabled && seenClosingFrames.Count >= 3,
                            "Closing lacked intermediate poses or a final blocked pose.");
                        Result.samples[Result.samples.Count - 1].eventMarker = "closed_again";
                        Result.checks.Add("Closing exposed at least three distinct poses; blocker returned only at the final closed frame.");
                        ChangePhase(Phase.ClosedRetest, inside);
                        break;
                    case Phase.ClosedRetest:
                        if (elapsed < 0.8f) break;
                        AssertClosedStop(); Stop(); Capture("closed_retested");
                        Result.checks.Add("Closed-again door stopped a second controlled traversal attempt.");
                        camera.transform.position = new Vector3(24f, -10f, originalCameraPosition.z);
                        Result.lakeCameraPosition = camera.transform.position;
                        Result.waterColliderStateBefore = WaterFingerprint();
                        Result.waterAnimationFlagsBefore = water.GetTileAnimationFlags(waterCell).ToString();
                        lastWaterCapture = -1f;
                        ChangePhase(Phase.Water);
                        break;
                    case Phase.Water:
                        Require(elapsed < 4f, "Water observation exceeded its 4s limit without proving the loop.");
                        Require(WaterFingerprint() == Result.waterColliderStateBefore, "Water collision changed during animation.");
                        // Render periodically even if the last reported frame is unchanged. Batch
                        // rendering may update native tile animation; gating renders by that frame
                        // could suppress the very update this probe is meant to observe.
                        if (lastWaterCapture < 0f || elapsed - lastWaterCapture >= 0.18f)
                        {
                            var sample = Capture("water_" + Result.samples.Count.ToString("D3"), captureWater: true);
                            var frame = sample.waterFrame;
                            var flags = (TileAnimationFlags)sample.waterAnimationFlagBits;
                            Require((flags & (TileAnimationFlags.LoopOnce | TileAnimationFlags.PauseAnimation)) == 0,
                                "Water tile is configured to stop/pause: " + sample.waterAnimationFlags);
                            Require(frame >= 0 && frame < Result.waterAnimationFrameCount, "Water reported an invalid animation frame.");
                            seenWaterFrames.Add(frame);
                            if (lastWaterFrame >= 0)
                            {
                                if (frame > lastWaterFrame) Result.waterAdvancedObserved = true;
                                else if (frame < lastWaterFrame && Result.waterAdvancedObserved) Result.waterWrapObserved = true;
                            }
                            lastWaterCapture = sample.time - phaseStarted;
                            lastWaterFrame = frame;
                            Result.waterDistinctFrames = seenWaterFrames.Count;
                            Result.waterSampleDuration = lastWaterCapture;
                            Require(WaterFingerprint() == Result.waterColliderStateBefore,
                                "Water collision changed during Camera.Render.");
                        }
                        if (Result.waterSampleDuration < 2.6f) break;
                        if (seenWaterFrames.Count != 6 || !Result.waterWrapObserved)
                        {
                            break;
                        }
                        Result.waterColliderStateAfter = WaterFingerprint();
                        Result.checks.Add("Periodic PNGs sampled after Camera.Render show all six water indices and a lower index after forward progress (wrap), over >=2.6s; LoopOnce/Pause absent and all three water colliders unchanged. Repeated frames remain in evidence.");
                        Result.status = "PASS";
                        phase = Phase.Finished;
                        Dispose();
                        return true;
                }
                return false;
            }
            catch (Exception error)
            {
                Result.status = "FAIL"; Result.reason = error.ToString();
                phase = Phase.Finished;
                Dispose();
                throw;
            }
        }

        private void AssertClosedStop()
        {
            Require(blocker.enabled && !door.IsOpen, "Closed blocker unexpectedly disabled.");
            Require(solid.bounds.max.y <= blocker.bounds.min.y + 0.06f, "Driven player crossed the closed leaf.");
            Require(Mathf.Abs(solid.bounds.max.y - blocker.bounds.min.y) < 0.12f, "Player did not actually reach the closed leaf.");
        }
        private void SampleOpening()
        {
            var index = Array.IndexOf(frames, leaf.sprite);
            Require(index >= 0, "Door renderer is not displaying a configured animation frame.");
            if (!blocker.enabled) Require(index == frames.Length - 1 && !door.IsAnimating,
                "Blocker released before the final clear frame.");
            seenDoorFrames.Add(index);
            if (index == lastDoorFrame) return;
            lastDoorFrame = index;
            Capture("opening_" + index.ToString("D2"));
        }
        private void SampleClosing()
        {
            var index = Array.IndexOf(frames, leaf.sprite);
            Require(index >= 0, "Closing renderer left the configured frame sequence.");
            if (blocker.enabled) Require(index == 0 && !door.IsAnimating && !door.IsOpen,
                "Blocker returned before the final closed pose.");
            seenClosingFrames.Add(index);
            if (index == lastClosingFrame) return;
            lastClosingFrame = index;
            Capture("closing_" + index.ToString("D2"));
        }

        private void CaptureTraversal()
        {
            if (!driver.Driving || Result.samples.Count == 0 || At(driver.Target)) return;
            var previous = Result.samples[Result.samples.Count - 1];
            if (Time.time - previous.time < Result.traversalCaptureInterval ||
                Vector2.Distance(body.position, previous.playerPosition) < 0.025f) return;
            // Each sample is an actual observed body position. No interpolation or synthetic frames.
            Capture("traverse_" + phase.ToString().ToLowerInvariant() + "_" + Result.samples.Count.ToString("D3"));
        }

        private void SelectAndInteract()
        {
            var selected = selector.GetCurrentInteractable();
            Require(ReferenceEquals(selected, door), "Live InteractionSystem did not select the FarmHouse door.");
            selected.Interact(player.gameObject);
        }
        private void ChangePhase(Phase next, Vector2? target = null)
        {
            phase = next; phaseStarted = Time.time;
            driver.Driving = target.HasValue;
            if (target.HasValue) driver.Target = target.Value;
        }
        private void Stop() { driver.Driving = false; body.linearVelocity = Vector2.zero; }
        private bool At(Vector2 target) => Vector2.Distance(body.position, target) < 0.06f;
        private static void Require(bool condition, string reason) { if (!condition) throw new InvalidOperationException(reason); }

        private string WaterFingerprint()
        {
            return JsonUtility.ToJson(new WaterState { polygons = waterColliders.Select(c =>
            {
                Require(c != null, "Water collider was destroyed.");
                var paths = new Vector2[c.pathCount][];
                for (var i = 0; i < paths.Length; i++) paths[i] = c.GetPath(i);
                return new PolygonState { entityId = c.GetEntityId().ToString(), name = c.name, enabled = c.enabled,
                    isTrigger = c.isTrigger, offset = c.offset, localToWorld = c.transform.localToWorldMatrix,
                    pathLengths = paths.Select(p => p.Length).ToArray(), points = paths.SelectMany(p => p).ToArray() };
            }).ToArray() });
        }

        private Sample Capture(string id, bool captureWater = false)
        {
            var sample = new Sample { phase = id, time = Time.time, fixedTime = Time.fixedTime,
                playerPosition = body.position, fixedSteps = driver.FixedSteps,
                doorFrame = Array.IndexOf(frames, leaf.sprite), leafSprite = leaf.sprite != null ? leaf.sprite.name : "null",
                blockerEnabled = blocker.enabled, doorOpen = door.IsOpen, doorAnimating = door.IsAnimating,
                roofAlpha = roof.color.a, path = Path.Combine(outputDirectory, id + ".png") };
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            var target = new RenderTexture(Result.width, Result.height, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = null;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                if (captureWater)
                {
                    // Annotate the rendered observation, not the potentially stale pre-render index.
                    sample.time = Time.time;
                    sample.fixedTime = Time.fixedTime;
                    sample.waterFrame = water.GetAnimationFrame(waterCell);
                    var flags = water.GetTileAnimationFlags(waterCell);
                    sample.waterAnimationFlagBits = (int)flags;
                    sample.waterAnimationFlags = flags.ToString();
                }
                RenderTexture.active = target;
                texture = new Texture2D(Result.width, Result.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, Result.width, Result.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(sample.path, texture.EncodeToPNG());
                Result.samples.Add(sample);
                return sample;
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (driver != null) UnityEngine.Object.DestroyImmediate(driver);
            if (player != null)
            {
                player.transform.position = originalPlayerPosition;
                if (body != null)
                {
                    body.position = originalPlayerPosition;
                    body.linearVelocity = originalVelocity;
                    body.angularVelocity = originalAngularVelocity;
                }
                player.enabled = playerEnabled;
            }
            if (camera != null) { camera.transform.position = originalCameraPosition; camera.orthographicSize = originalOrtho; }
            if (follow != null) follow.enabled = followEnabled;
            Physics2D.SyncTransforms();
        }
    }
}
