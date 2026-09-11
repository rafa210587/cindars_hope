using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CindarsHope.Camera;
using CindarsHope.Farm.Animals;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Dev
{
    /// <summary>Isolated native full-capacity barn observation for the v19 herd.</summary>
    internal sealed class FarmHerdMotionCapture : IDisposable
    {
        [Serializable] internal sealed class AnimalSample
        {
            public string instanceId, animalDataId, profileId, sprite, mode, direction, health, nearestSolid;
            public Vector2 position, bodySize;
            public Bounds bodyBounds, rendererBounds;
            public Rect outerBounds, allowedCenterBounds;
            public float displacement, solidDistance;
            public int fixedTicks, frameIndex;
            public bool paused, interactionFrozen, centerInside, fullBodyInside, penetratesSolid;
        }

        [Serializable] internal sealed class Frame
        {
            public string path;
            public float time, fixedTime, minimumPairDistance, minimumSolidDistance;
            public int generation;
            public AnimalSample[] animals;
        }

        [Serializable] internal sealed class AnimalStats
        {
            public string instanceId, animalDataId, profileId;
            public float travel, walkingTravel, maximumFrozenStep;
            public int distinctSprites, fixedTicks;
            public bool interactionObserved, interactionStopped, stateUnchanged;
            public string[] modes, directions, sprites;
        }

        [Serializable] internal sealed class Evidence
        {
            public string status = "RUNNING", reason;
            public string method = "One isolated 60s native Play Mode observation of a full Barn_01: cow, sheep, goat and a second cow are each released through AnimalReleaseHandler, then all four are restored once through FarmAnimalsSectionProvider plus RespawnExistingAnimals. Every materialization settles for at least two real FixedUpdate ticks followed by Physics2D.SyncTransforms. No animal pose, target, RNG, transform, physics step, profile, care or product state is driven. Each interaction runs with zero feed and must expose a native stopped/frozen interval without state mutation. Camera framing and 5fps herd crops are observational. The player is disabled and temporarily placed just south of the pen only in the labeled full-view scale contexts; this is framing, not route/input evidence. No SaveManager or disk save/load is invoked. PASS is scoped to these four instances/seed and the sampled visual stream, not human acceptance or all farm routes.";
            public float observedSeconds, minimumPairDistance, minimumSolidDistance;
            public int releasedCount, restoredCount, renderWidth, renderHeight, totalFixedTicks;
            public bool fullCapacity, restoreSucceeded, purchaseItemsConsumed, zeroFeedThroughout;
            public bool inventoryRestored, animalStateUnchanged, allInteractableAfterRestore;
            public string inventoryHashBefore, inventoryHashAfter;
            public List<string> contextFrames = new List<string>();
            public List<string> checks = new List<string>();
            public List<AnimalStats> animals = new List<AnimalStats>();
            public List<Frame> frames = new List<Frame>();
        }

        private sealed class Tracker
        {
            public string id, dataId;
            public FarmAnimalRuntime runtime;
            public FarmAnimalMotionFixedObserver ticks;
            public Vector2 last;
            public float lastStep, travel, walkingTravel, maxFrozenStep;
            public int accumulatedTicks;
            public bool frozen, stopped;
            public readonly HashSet<string> modes = new HashSet<string>();
            public readonly HashSet<string> directions = new HashSet<string>();
            public readonly HashSet<string> sprites = new HashSet<string>();
            public StateSnapshot state;
        }

        private enum Phase { SettleInitial, Observe, AwaitDestroyed, AcquireRestored, SettleRestored, Done }

        private const string HousingId = "barn_01";
        private const float Duration = 60f;
        private const float RestoreAt = 30f;
        private const float Tolerance = 0.011f;
        private readonly Scene scene;
        private readonly UnityEngine.Camera camera;
        private readonly CameraFollow2D follow;
        private readonly bool originalFollow;
        private readonly Vector3 originalCamera;
        private readonly PlayerController player;
        private readonly bool originalPlayerEnabled;
        private readonly Vector3 originalPlayerPosition;
        private readonly Rigidbody2D playerBody;
        private readonly Vector2 originalPlayerBodyPosition, originalPlayerVelocity;
        private readonly InventoryManager inventory;
        private readonly FarmAnimalRegistry registry;
        private readonly AnimalReleaseHandler handler;
        private readonly FarmAnimalsSectionProvider provider = new FarmAnimalsSectionProvider();
        private readonly InventorySaveData originalInventory;
        private readonly FarmAnimalsSaveData originalAnimals;
        private readonly Collider2D[] solids;
        private readonly string output;
        private readonly List<Tracker> trackers = new List<Tracker>();
        private readonly List<FarmAnimalRuntime> destroyed = new List<FarmAnimalRuntime>();
        private FarmAnimalsSaveData herdSnapshot;
        private Phase phase;
        private int generation;
        private float phaseStarted, segmentStarted, observedBeforeRestore, lastCapture = -100f;
        private bool interactionInvoked, hasDistance, disposed;

        internal Evidence Result { get; } = new Evidence();

        internal FarmHerdMotionCapture(Scene scene, PlayerController player, UnityEngine.Camera camera, string outputDirectory)
        {
            Require(Application.isPlaying && scene.IsValid() && scene.isLoaded, "Herd capture requires the isolated Farm Play session.");
            this.scene = scene; this.player = player; this.camera = camera;
            Require(player != null && camera != null && camera.orthographic, "Expected player and orthographic gameplay camera.");
            follow = camera.GetComponent<CameraFollow2D>(); Require(follow != null, "Expected CameraFollow2D.");
            originalFollow = follow.enabled; originalCamera = camera.transform.position;
            originalPlayerEnabled = player.enabled; originalPlayerPosition = player.transform.position;
            playerBody = player.GetComponent<Rigidbody2D>();
            originalPlayerBodyPosition = playerBody != null ? playerBody.position : (Vector2)originalPlayerPosition;
            originalPlayerVelocity = playerBody != null ? playerBody.linearVelocity : Vector2.zero;
            inventory = DomainManagerRegistry.Get<IInventoryRuntime>() as InventoryManager;
            registry = FarmAnimalRegistry.Instance;
            Require(inventory != null && registry != null, "Farm bootstrap did not expose inventory/animal registry.");
            handler = Components<AnimalReleaseHandler>(scene).Single(h => h.name == "Barn_01" && h.HousingId == HousingId);
            originalInventory = inventory.CaptureSaveData();
            originalAnimals = provider.Capture(new GameSaveData()) as FarmAnimalsSaveData;
            Require(originalAnimals != null, "Could not snapshot the original in-memory animal section.");
            Require(registry.GetByHousing(HousingId).Count == 0 && handler.RuntimeCount == 0,
                "Isolated herd capture requires an initially empty barn.");
            foreach (var item in new[] { FarmAnimalCatalog.ItemCalfCow, FarmAnimalCatalog.ItemLambSheep, FarmAnimalCatalog.ItemKidGoat, FarmAnimalCatalog.ItemFeed })
                Require(inventory.GetAmount(item) == 0, "Isolated herd capture requires zero initial quantity: " + item);
            solids = Components<Collider2D>(scene).Where(c => c.enabled && !c.isTrigger && !c.transform.IsChildOf(player.transform)).ToArray();
            output = Path.Combine(outputDirectory, "herd-motion"); Directory.CreateDirectory(output);
            Result.renderWidth = camera.pixelWidth; Result.renderHeight = camera.pixelHeight;
            Require(Result.renderWidth > 0 && Result.renderHeight > 0, "Gameplay camera has no render dimensions.");
            try
            {
                follow.enabled = false; player.enabled = false;
                PositionPlayerForScaleContext();
                Release(FarmAnimalCatalog.ItemCalfCow, FarmAnimalCatalog.AnimalCow);
                Release(FarmAnimalCatalog.ItemLambSheep, FarmAnimalCatalog.AnimalSheep);
                Release(FarmAnimalCatalog.ItemKidGoat, FarmAnimalCatalog.AnimalGoat);
                Release(FarmAnimalCatalog.ItemCalfCow, FarmAnimalCatalog.AnimalCow);
                Result.purchaseItemsConsumed = inventory.GetAmount(FarmAnimalCatalog.ItemCalfCow) == 0 &&
                    inventory.GetAmount(FarmAnimalCatalog.ItemLambSheep) == 0 && inventory.GetAmount(FarmAnimalCatalog.ItemKidGoat) == 0;
                Result.zeroFeedThroughout = inventory.GetAmount(FarmAnimalCatalog.ItemFeed) == 0;
                Require(Result.purchaseItemsConsumed && Result.zeroFeedThroughout,
                    "Canonical release did not consume only the four temporary purchase items.");
                Result.releasedCount = trackers.Count;
                Result.fullCapacity = trackers.Count == 4 && handler.RuntimeCount == 4 && registry.GetByHousing(HousingId).Count == 4;
                Require(Result.fullCapacity, "Canonical release did not fill Barn_01 with exactly four runtimes.");
                AttachTrackers();
                phase = Phase.SettleInitial; phaseStarted = Time.time;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal bool Tick()
        {
            if (disposed || phase == Phase.Done) return true;
            try
            {
                Require(Mathf.Abs(Time.timeScale - 1f) < 0.0001f, "Herd observation requires normal time scale.");
                if (phase == Phase.SettleInitial) Settle(true);
                else if (phase == Phase.Observe) Observe();
                else if (phase == Phase.AwaitDestroyed) Restore();
                else if (phase == Phase.AcquireRestored) AcquireRestored();
                else if (phase == Phase.SettleRestored) Settle(false);
                return phase == Phase.Done;
            }
            catch (Exception error)
            {
                Result.status = "FAIL"; Result.reason = error.ToString(); Dispose(); throw;
            }
        }

        private void Release(string purchaseItemId, string expectedDataId)
        {
            Require(inventory.AddItem(purchaseItemId, 1), "Could not add temporary barn purchase item: " + purchaseItemId);
            var result = handler.TryReleaseFromInventory(inventory.gameObject, out var instanceId);
            Require(result == AnimalReleaseHandler.ReleaseAttemptResult.Success, "Canonical barn release failed: " + result);
            Require(handler.TryGetRuntime(instanceId, out var runtime) && runtime != null && runtime.AnimalDataId == expectedDataId,
                "Released runtime identity mismatch for " + expectedDataId);
            trackers.Add(new Tracker { id = instanceId, dataId = expectedDataId, runtime = runtime });
        }

        private void AttachTrackers()
        {
            foreach (var tracker in trackers)
            {
                Require(tracker.runtime.MotionProfile != null && tracker.runtime.MotionProfile.AnimalDataId == tracker.dataId,
                    "Motion profile identity mismatch for " + tracker.id);
                Require(tracker.runtime.GetComponentsInChildren<Collider2D>().All(c => c.isTrigger), "Animal collider could block player: " + tracker.id);
                tracker.ticks = tracker.runtime.gameObject.AddComponent<FarmAnimalMotionFixedObserver>();
                tracker.last = tracker.runtime.transform.position;
            }
        }

        private void Settle(bool initial)
        {
            if (trackers.Any(t => t.runtime == null || t.ticks == null || t.ticks.FixedTicks < 2))
            { Require(Time.time - phaseStarted < 2f, "All four herd runtimes did not settle for two FixedUpdate ticks."); return; }
            Physics2D.SyncTransforms();
            foreach (var tracker in trackers)
            {
                tracker.last = tracker.runtime.transform.position;
                if (initial) tracker.state = StateSnapshot.From(registry.GetAnimal(tracker.id));
            }
            FrameHerd(true);
            Result.contextFrames.Add(CaptureFull(initial ? "context_herd_start" : "context_herd_restored"));
            if (!initial)
            {
                Result.allInteractableAfterRestore = trackers.All(t => t.runtime.CanInteract(inventory.gameObject));
                Require(Result.allInteractableAfterRestore, "One or more herd members became inaccessible after restore.");
                Result.checks.Add("restore: four stable IDs/profile identities rematerialized and remained interactable");
            }
            segmentStarted = Time.time; lastCapture = -100f; phase = Phase.Observe;
        }

        private void Observe()
        {
            FrameHerd(false);
            float observed = observedBeforeRestore + Time.time - segmentStarted;
            Result.zeroFeedThroughout &= inventory.GetAmount(FarmAnimalCatalog.ItemFeed) == 0;
            Require(Result.zeroFeedThroughout, "Feed inventory changed during cosmetic herd observation.");
            foreach (var tracker in trackers)
            {
                Vector2 now = tracker.runtime.transform.position;
                float step = Vector2.Distance(tracker.last, now);
                tracker.lastStep = step;
                if (step < 0.25f)
                {
                    tracker.travel += step;
                    if (tracker.runtime.MotionMode == AnimalMotionMode.Walking) tracker.walkingTravel += step;
                    if (tracker.runtime.IsInteractionFrozen) tracker.maxFrozenStep = Mathf.Max(tracker.maxFrozenStep, step);
                }
                tracker.last = now;
                if (tracker.runtime.IsInteractionFrozen)
                {
                    tracker.frozen = true;
                    if (tracker.runtime.IsMotionPaused) tracker.stopped = true;
                }
                var renderer = Renderer(tracker);
                tracker.modes.Add(tracker.runtime.MotionMode.ToString());
                tracker.directions.Add(tracker.runtime.FacingDirection.ToString());
                tracker.sprites.Add(renderer.sprite != null ? renderer.sprite.name : "null");
            }
            ValidateGeometry();
            if (Time.time - lastCapture >= 0.19f) CaptureFrame();
            if (!interactionInvoked && observed >= 15f) InteractAll();
            if (generation == 0 && observed >= RestoreAt) BeginRestore(observed);
            else if (observed >= Duration) Finish(observed);
        }

        private void InteractAll()
        {
            Require(inventory.GetAmount(FarmAnimalCatalog.ItemFeed) == 0, "Feed appeared during cosmetic observation.");
            foreach (var tracker in trackers)
            {
                Require(tracker.runtime.CanInteract(inventory.gameObject), "Herd member is not interactable: " + tracker.id);
                tracker.runtime.Interact(inventory.gameObject);
            }
            interactionInvoked = true;
        }

        private void BeginRestore(float observed)
        {
            observedBeforeRestore = observed;
            herdSnapshot = provider.Capture(new GameSaveData()) as FarmAnimalsSaveData;
            Require(herdSnapshot != null && herdSnapshot.Animals.Count(a => trackers.Any(t => t.id == a.AnimalInstanceId)) == 4,
                "Provider did not capture all four herd members.");
            Result.contextFrames.Add(CaptureFull("context_herd_before_restore"));
            destroyed.Clear();
            foreach (var tracker in trackers)
            {
                tracker.accumulatedTicks += tracker.ticks.FixedTicks;
                destroyed.Add(tracker.runtime);
                UnityEngine.Object.Destroy(tracker.runtime.gameObject);
                tracker.runtime = null; tracker.ticks = null;
            }
            phase = Phase.AwaitDestroyed; phaseStarted = Time.time;
        }

        private void Restore()
        {
            if (destroyed.Any(runtime => runtime != null))
            { Require(Time.time - phaseStarted < 2f, "Herd runtimes did not destroy before provider restore."); return; }
            provider.Restore(herdSnapshot);
            handler.RespawnExistingAnimals(registry.GetByHousing(HousingId), inventory, registry);
            phase = Phase.AcquireRestored; phaseStarted = Time.time;
        }

        private void AcquireRestored()
        {
            foreach (var tracker in trackers)
            {
                if (!handler.TryGetRuntime(tracker.id, out var runtime) || runtime == null)
                { Require(Time.time - phaseStarted < 2f, "Restored herd runtime missing: " + tracker.id); return; }
                tracker.runtime = runtime;
            }
            Result.restoredCount = trackers.Select(t => t.runtime).Distinct().Count();
            Result.restoreSucceeded = Result.restoredCount == 4 && handler.RuntimeCount == 4 && registry.GetByHousing(HousingId).Count == 4;
            Require(Result.restoreSucceeded, "Restore did not rematerialize exactly four unique runtimes.");
            generation = 1; AttachTrackers(); phase = Phase.SettleRestored; phaseStarted = Time.time;
        }

        private void Finish(float observed)
        {
            Result.observedSeconds = observed;
            Result.contextFrames.Add(CaptureFull("context_herd_end"));
            Result.animalStateUnchanged = trackers.All(t => t.state.Matches(registry.GetAnimal(t.id)));
            Require(Result.animalStateUnchanged && Result.purchaseItemsConsumed && Result.zeroFeedThroughout,
                "Cosmetic herd motion/interaction changed care, product or inventory transaction state.");
            foreach (var tracker in trackers)
            {
                tracker.accumulatedTicks += tracker.ticks.FixedTicks;
                Result.totalFixedTicks += tracker.accumulatedTicks;
                Require(tracker.travel > 0.2f && tracker.walkingTravel > 0.1f, "No physical walk displacement: " + tracker.id);
                foreach (var mode in new[] { "Idle", "Walking", "Peck", "Rest" })
                    Require(tracker.modes.Contains(mode), "Missing native mode " + mode + ": " + tracker.id);
                Require(tracker.frozen && tracker.stopped && tracker.maxFrozenStep <= 0.002f,
                    "Interaction did not demonstrate a stopped frozen interval: " + tracker.id);
                Require(!tracker.sprites.Contains("null") && tracker.sprites.Count >= 6, "Insufficient native visual poses: " + tracker.id);
                Result.animals.Add(new AnimalStats { instanceId = tracker.id, animalDataId = tracker.dataId,
                    profileId = tracker.runtime.MotionProfile.AnimalDataId, travel = tracker.travel, walkingTravel = tracker.walkingTravel,
                    maximumFrozenStep = tracker.maxFrozenStep, distinctSprites = tracker.sprites.Count,
                    fixedTicks = tracker.accumulatedTicks, interactionObserved = tracker.frozen, interactionStopped = tracker.stopped,
                    stateUnchanged = tracker.state.Matches(registry.GetAnimal(tracker.id)), modes = tracker.modes.OrderBy(x => x).ToArray(),
                    directions = tracker.directions.OrderBy(x => x).ToArray(), sprites = tracker.sprites.OrderBy(x => x).ToArray() });
            }
            Require(Result.minimumPairDistance >= -Tolerance && Result.minimumSolidDistance >= -Tolerance,
                "Herd exceeded measured body/solid penetration tolerance.");
            Result.checks.Add("herd: four profiles, four modes each, physical travel, interaction stop and inclusive full-body bounds");
            Result.inventoryHashBefore = JsonUtility.ToJson(originalInventory);
            Result.status = "PASS"; phase = Phase.Done; Dispose();
        }

        private void ValidateGeometry()
        {
            string failure = null;
            float framePair = float.PositiveInfinity, frameSolid = float.PositiveInfinity;
            for (int i = 0; i < trackers.Count; i++)
            {
                var tracker = trackers[i]; var runtime = tracker.runtime; var body = Body(tracker);
                Vector2 center = body.bounds.center; Rect allowed = runtime.AllowedCenterBounds; Rect outer = runtime.MotionBoundsWorld;
                bool centerInside = Inclusive(allowed, center);
                bool fullInside = body.bounds.min.x >= outer.xMin - Tolerance && body.bounds.max.x <= outer.xMax + Tolerance &&
                    body.bounds.min.y >= outer.yMin - Tolerance && body.bounds.max.y <= outer.yMax + Tolerance;
                var nearest = solids.Where(c => c != null && c.enabled).Select(c => new { c, d = body.Distance(c).distance }).OrderBy(x => x.d).First();
                frameSolid = Mathf.Min(frameSolid, nearest.d);
                if (!centerInside || !fullInside || nearest.d < -Tolerance)
                    failure = $"{tracker.id}: center=({center.x:F4},{center.y:F4}), allowed=({allowed.xMin:F4},{allowed.yMin:F4})..({allowed.xMax:F4},{allowed.yMax:F4}), body=({body.bounds.min.x:F4},{body.bounds.min.y:F4})..({body.bounds.max.x:F4},{body.bounds.max.y:F4}), outer=({outer.xMin:F4},{outer.yMin:F4})..({outer.xMax:F4},{outer.yMax:F4}), solid={nearest.c.name}, distance={nearest.d:F4}.";
                for (int j = i + 1; j < trackers.Count; j++)
                {
                    float distance = body.Distance(Body(trackers[j])).distance;
                    framePair = Mathf.Min(framePair, distance);
                    if (distance < -Tolerance) failure = $"{tracker.id}/{trackers[j].id}: pair signed distance={distance:F4}.";
                }
            }
            if (!hasDistance)
            { Result.minimumPairDistance = framePair; Result.minimumSolidDistance = frameSolid; hasDistance = true; }
            else
            { Result.minimumPairDistance = Mathf.Min(Result.minimumPairDistance, framePair); Result.minimumSolidDistance = Mathf.Min(Result.minimumSolidDistance, frameSolid); }
            if (failure != null)
            {
                CaptureFrame();
                Require(false, "Herd geometry violation after two-tick settle: " + failure);
            }
        }

        private void CaptureFrame()
        {
            Physics2D.SyncTransforms();
            var samples = trackers.Select(Sample).ToArray();
            var herdBounds = Renderer(trackers[0]).bounds;
            foreach (var tracker in trackers.Skip(1)) herdBounds.Encapsulate(Renderer(tracker).bounds);
            var frame = new Frame { generation = generation, time = Time.time, fixedTime = Time.fixedTime,
                minimumPairDistance = PairMinimum(), minimumSolidDistance = SolidMinimum(), animals = samples };
            frame.path = CaptureCrop("herd_" + Result.frames.Count.ToString("D4"), herdBounds);
            Result.frames.Add(frame); lastCapture = Time.time;
        }

        private AnimalSample Sample(Tracker tracker)
        {
            var runtime = tracker.runtime; var body = Body(tracker); var renderer = Renderer(tracker);
            var nearest = solids.Where(c => c != null && c.enabled).Select(c => new { c, d = body.Distance(c).distance }).OrderBy(x => x.d).First();
            Vector2 center = body.bounds.center; Rect outer = runtime.MotionBoundsWorld; Rect allowed = runtime.AllowedCenterBounds;
            return new AnimalSample { instanceId = tracker.id, animalDataId = runtime.AnimalDataId,
                profileId = runtime.MotionProfile != null ? runtime.MotionProfile.AnimalDataId : "null",
                sprite = renderer.sprite != null ? renderer.sprite.name : "null", mode = runtime.MotionMode.ToString(),
                direction = runtime.FacingDirection.ToString(), health = runtime.CurrentHealthState.ToString(), position = runtime.transform.position,
                bodySize = runtime.BodySize, bodyBounds = body.bounds, rendererBounds = renderer.bounds, outerBounds = outer,
                allowedCenterBounds = allowed, displacement = tracker.lastStep,
                solidDistance = nearest.d, nearestSolid = nearest.c.name, fixedTicks = tracker.ticks.FixedTicks,
                frameIndex = runtime.SpriteAnimator != null ? runtime.SpriteAnimator.CurrentFrameIndex : -1,
                paused = runtime.IsMotionPaused, interactionFrozen = runtime.IsInteractionFrozen, centerInside = Inclusive(allowed, center),
                fullBodyInside = body.bounds.min.x >= outer.xMin - Tolerance && body.bounds.max.x <= outer.xMax + Tolerance &&
                    body.bounds.min.y >= outer.yMin - Tolerance && body.bounds.max.y <= outer.yMax + Tolerance,
                penetratesSolid = nearest.d < -Tolerance };
        }

        private float PairMinimum()
        {
            float minimum = float.PositiveInfinity;
            for (int i = 0; i < trackers.Count; i++) for (int j = i + 1; j < trackers.Count; j++)
                minimum = Mathf.Min(minimum, Body(trackers[i]).Distance(Body(trackers[j])).distance);
            return minimum;
        }

        private float SolidMinimum() => trackers.Min(t => solids.Where(c => c != null && c.enabled).Min(c => Body(t).Distance(c).distance));
        private static bool Inclusive(Rect rect, Vector2 point) => point.x >= rect.xMin - 0.0001f && point.x <= rect.xMax + 0.0001f && point.y >= rect.yMin - 0.0001f && point.y <= rect.yMax + 0.0001f;
        private static Collider2D Body(Tracker tracker) => tracker.runtime.GetComponentsInChildren<Collider2D>().Single(c => c.enabled);
        private static SpriteRenderer Renderer(Tracker tracker) => tracker.runtime.GetComponentsInChildren<SpriteRenderer>().Single(r => r.enabled);

        private void PositionPlayerForScaleContext()
        {
            Vector2 position = (Vector2)handler.transform.position + new Vector2(-0.65f, -3.9f);
            player.transform.position = new Vector3(position.x, position.y, originalPlayerPosition.z);
            if (playerBody != null) { playerBody.position = position; playerBody.linearVelocity = Vector2.zero; }
            Physics2D.SyncTransforms();
        }

        private void FrameHerd(bool includePlayer)
        {
            var bounds = Renderer(trackers[0]).bounds;
            foreach (var tracker in trackers.Skip(1)) bounds.Encapsulate(Renderer(tracker).bounds);
            if (includePlayer)
                foreach (var renderer in player.GetComponentsInChildren<SpriteRenderer>().Where(r => r.enabled)) bounds.Encapsulate(renderer.bounds);
            camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, originalCamera.z);
        }

        private string CaptureFull(string id) => Capture(id, null);
        private string CaptureCrop(string id, Bounds bounds) => Capture(id, bounds);
        private string Capture(string id, Bounds? cropBounds)
        {
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var target = new RenderTexture(Result.renderWidth, Result.renderHeight, 24, RenderTextureFormat.ARGB32); Texture2D image = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var crop = new RectInt(0, 0, Result.renderWidth, Result.renderHeight);
                if (cropBounds.HasValue)
                {
                    var a = camera.WorldToViewportPoint(cropBounds.Value.min); var b = camera.WorldToViewportPoint(cropBounds.Value.max);
                    int x0 = Mathf.Clamp(Mathf.FloorToInt(a.x * Result.renderWidth) - 24, 0, Result.renderWidth - 1);
                    int y0 = Mathf.Clamp(Mathf.FloorToInt(a.y * Result.renderHeight) - 24, 0, Result.renderHeight - 1);
                    int x1 = Mathf.Clamp(Mathf.CeilToInt(b.x * Result.renderWidth) + 24, x0 + 1, Result.renderWidth);
                    int y1 = Mathf.Clamp(Mathf.CeilToInt(b.y * Result.renderHeight) + 24, y0 + 1, Result.renderHeight);
                    crop = new RectInt(x0, y0, x1 - x0, y1 - y0);
                }
                image = new Texture2D(crop.width, crop.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(crop.x, crop.y, crop.width, crop.height), 0, 0); image.Apply();
                string path = Path.Combine(output, id + ".png"); File.WriteAllBytes(path, image.EncodeToPNG()); return path;
            }
            finally
            {
                camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
        }

        public void Dispose()
        {
            if (disposed) return; disposed = true;
            try
            {
                var ids = new HashSet<string>(trackers.Select(t => t.id));
                foreach (var runtime in Components<FarmAnimalRuntime>(scene).Where(r => ids.Contains(r.AnimalInstanceId)).ToArray())
                    if (runtime != null) UnityEngine.Object.Destroy(runtime.gameObject);
                inventory.RestoreFromSaveData(originalInventory); provider.Restore(originalAnimals);
                Result.inventoryHashAfter = JsonUtility.ToJson(inventory.CaptureSaveData());
                Result.inventoryRestored = Result.inventoryHashBefore == Result.inventoryHashAfter;
                if (Result.status == "PASS")
                {
                    Require(Result.inventoryRestored, "Temporary herd inventory was not restored in memory.");
                    Result.checks.Add("transactions: purchase items consumed, feed stayed zero, care/products unchanged, inventory restored in memory");
                }
            }
            finally
            {
                player.enabled = originalPlayerEnabled;
                player.transform.position = originalPlayerPosition;
                if (playerBody != null) { playerBody.position = originalPlayerBodyPosition; playerBody.linearVelocity = originalPlayerVelocity; }
                Physics2D.SyncTransforms();
                camera.transform.position = originalCamera; follow.enabled = originalFollow;
            }
        }

        private static IEnumerable<T> Components<T>(Scene source) where T : Component => source.IsValid()
            ? source.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)) : Enumerable.Empty<T>();
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

        private sealed class StateSnapshot
        {
            private bool fed, product; private int care, lastProduct; private AnimalHealthState health;
            internal static StateSnapshot From(AnimalInstanceState state) => new StateSnapshot
            { fed = state.FedToday, product = state.ProductReady, care = state.CareScore, lastProduct = state.LastProductDay, health = state.HealthState };
            internal bool Matches(AnimalInstanceState state) => state != null && state.FedToday == fed && state.ProductReady == product &&
                state.CareScore == care && state.LastProductDay == lastProduct && state.HealthState == health;
        }
    }
}
