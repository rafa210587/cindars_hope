using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CindarsHope.Camera;
using CindarsHope.Farm.Animals;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Dev
{
    /// <summary>Observes the native v18 boat and chicken runtimes without driving their state.</summary>
    internal sealed class FarmAnimalMotionCapture : IDisposable
    {
        [Serializable] internal sealed class Sample
        {
            public string subject, path, sprite, mode, direction, health, nearestSolid;
            public float time, fixedTime, normalizedTime, displacement, solidDistance;
            public int generation, frameIndex, fixedTicks;
            public Vector2 position, bodySize;
            public Vector3 rendererCenter, rendererSize;
            public Rect motionBounds;
            public Bounds bodyBounds;
            public bool paused, interactionFrozen, centerInsideBounds, fullBodyInsideBounds, penetratesSolid;
        }

        [Serializable] internal sealed class StateStats
        {
            public string mode, direction;
            public int samples, distinctSprites;
            public float displacement;
        }

        [Serializable] internal sealed class Evidence
        {
            public string status = "RUNNING", reason;
            public string method = "Native Play Mode observation: ~10.5s boat (two 5s cycles), then >=60s of a chicken released through AnimalReleaseHandler and restored through FarmAnimalsSectionProvider plus RespawnExistingAnimals. One visible-free temporary BoxCollider2D inside the isolated pen supplies a reproducible obstructed route; it never moves or drives the chicken. Camera framing, three full-view context PNGs and 5fps crops do not drive Animator, animal targets, poses, physics, RNG, health or time. The interaction check runs only with zero feed, so it can observe the real freeze without applying care/product changes. Unavailable is supplied through an isolated in-memory save DTO/provider restore; no SaveManager, disk save/load or user save is invoked. PASS covers this one deterministic pilot and sampled contacts/bounds, not input, full housing capacity, animal-to-animal avoidance, a visually closed fence, all random seeds, global gameplay or human acceptance.";
            public float startedTime, endedTime, boatObservedSeconds, chickenObservedSeconds;
            public float boatNormalizedCycles, boatMaxTransformDrift, boatMaxRotationDrift, boatMaxScaleDrift;
            public float chickenTravel, walkingTravel, minimumSolidDistance;
            public int renderWidth, renderHeight, boatDistinctSprites, chickenDistinctSprites;
            public int releasedRuntimeCount, restoredRuntimeCount, fixedTicks;
            public bool releaseSucceeded, restoreSucceeded, instanceIdPreserved, interactionObserved;
            public bool interactionStateUnchanged, unavailableStopped, animalColliderIsTrigger;
            public string animalInstanceId, interactionPrompt, inventoryHashBefore, inventoryHashAfter;
            public List<string> contextFrames = new List<string>();
            public List<string> checks = new List<string>();
            public List<StateStats> stateStats = new List<StateStats>();
            public List<Sample> samples = new List<Sample>();
        }

        private enum Phase
        {
            Boat, AwaitReleasedRuntime, SettleReleasedRuntime, Chicken,
            AwaitDestroyedForRestore, AwaitRestoredRuntime, SettleRestoredRuntime,
            AwaitDestroyedForUnavailable, AwaitUnavailableRuntime, SettleUnavailableRuntime, Unavailable, Done
        }

        private const string HousingId = "coop_01";
        private const float BoatDuration = 10.5f;
        private const float ChickenDuration = 60f;
        private readonly Scene scene;
        private readonly UnityEngine.Camera camera;
        private readonly CameraFollow2D follow;
        private readonly bool originalFollow;
        private readonly Vector3 originalCamera;
        private readonly SpriteRenderer boat;
        private readonly Animator boatAnimator;
        private readonly Transform boatTransform;
        private readonly Vector3 boatPosition, boatScale;
        private readonly Quaternion boatRotation;
        private readonly float boatNormalizedStart;
        private readonly InventoryManager inventory;
        private readonly FarmAnimalRegistry registry;
        private readonly AnimalReleaseHandler releaseHandler;
        private readonly FarmAnimalsSectionProvider animalProvider = new FarmAnimalsSectionProvider();
        private readonly InventorySaveData originalInventory;
        private readonly FarmAnimalsSaveData originalAnimals;
        private readonly Collider2D[] solids;
        private readonly GameObject obstacleFixture;
        private readonly string output;
        private readonly float started;
        private readonly HashSet<string> boatSprites = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> chickenSprites = new HashSet<string>(StringComparer.Ordinal);
        private Phase phase = Phase.Boat;
        private FarmAnimalRuntime animal;
        private FarmAnimalRuntime releasedAnimal;
        private FarmAnimalMotionFixedObserver tickObserver;
        private FarmAnimalsSaveData releasedSnapshot;
        private FarmAnimalsSaveData unavailableSnapshot;
        private AnimalStateSnapshot interactionState;
        private Vector2 lastAnimalPosition;
        private float phaseStarted, lastCapture = -100f, chickenStarted, unavailableStarted;
        private int generation, accumulatedTicks;
        private bool disposed, interactionInvoked;

        internal Evidence Result { get; } = new Evidence();

        internal FarmAnimalMotionCapture(Scene scene, UnityEngine.Camera camera, string outputDirectory)
        {
            Require(Application.isPlaying && scene.IsValid() && scene.isLoaded, "Animal motion capture requires the isolated Farm Play session.");
            this.scene = scene; this.camera = camera;
            Require(camera != null && camera.orthographic, "Expected the live orthographic gameplay camera.");
            follow = camera.GetComponent<CameraFollow2D>();
            Require(follow != null, "Expected CameraFollow2D on the gameplay camera.");
            originalFollow = follow.enabled; originalCamera = camera.transform.position;
            var components = Components<Component>(scene).ToArray();
            boat = components.OfType<SpriteRenderer>().Single(r => r.name == "Visual_MooredBoat");
            boatAnimator = boat.GetComponent<Animator>();
            Require(boatAnimator != null && boatAnimator.enabled && boatAnimator.runtimeAnimatorController != null,
                "Expected the native boat Animator.");
            var clip = boatAnimator.runtimeAnimatorController.animationClips.Single();
            Require(Mathf.Abs(clip.length - 5f) < 0.01f, "Boat clip is not the authored 5 second loop.");
            boatTransform = boat.transform; boatPosition = boatTransform.position; boatRotation = boatTransform.rotation; boatScale = boatTransform.lossyScale;
            boatNormalizedStart = boatAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            inventory = DomainManagerRegistry.Get<IInventoryRuntime>() as InventoryManager;
            Require(inventory != null, "Active InventoryManager was not registered by the Farm bootstrap.");
            registry = FarmAnimalRegistry.Instance;
            Require(registry != null, "Farm animal provider has no active registry.");
            releaseHandler = components.OfType<AnimalReleaseHandler>().Single(h => h.name == "Coop_01");
            originalInventory = inventory.CaptureSaveData();
            originalAnimals = animalProvider.Capture(new GameSaveData()) as FarmAnimalsSaveData;
            Require(originalAnimals != null, "Could not capture the original in-memory animal section.");
            solids = components.OfType<Collider2D>().Where(c => c.enabled && !c.isTrigger).ToArray();
            obstacleFixture = new GameObject("ProbeOnly_ChickenObstacle");
            obstacleFixture.transform.position = releaseHandler.SharedWorldBounds.center;
            var fixtureCollider = obstacleFixture.AddComponent<BoxCollider2D>();
            fixtureCollider.size = new Vector2(0.16f, 1.2f);
            solids = solids.Concat(new[] { fixtureCollider }).ToArray();
            output = Path.Combine(outputDirectory, "animal-motion"); Directory.CreateDirectory(output);
            Result.renderWidth = camera.pixelWidth; Result.renderHeight = camera.pixelHeight;
            Require(Result.renderWidth > 0 && Result.renderHeight > 0, "Gameplay camera has no render dimensions.");
            started = phaseStarted = Time.time; Result.startedTime = started;
            follow.enabled = false;
            Frame(boat.bounds.center);
            Result.contextFrames.Add(CaptureFull("context_boat_start"));
        }

        internal bool Tick()
        {
            if (disposed || phase == Phase.Done) return true;
            try
            {
                Require(Mathf.Abs(Time.timeScale - 1f) < 0.0001f, "Observation requires normal time scale.");
                if (phase == Phase.Boat) TickBoat();
                else if (phase == Phase.AwaitReleasedRuntime) AcquireReleasedRuntime();
                else if (phase == Phase.SettleReleasedRuntime) SettleAnimalRuntime(true);
                else if (phase == Phase.Chicken) TickChicken();
                else if (phase == Phase.AwaitDestroyedForRestore) RestoreReleasedRuntime();
                else if (phase == Phase.AwaitRestoredRuntime) AcquireRestoredRuntime();
                else if (phase == Phase.SettleRestoredRuntime) SettleAnimalRuntime(false);
                else if (phase == Phase.AwaitDestroyedForUnavailable) RestoreUnavailableRuntime();
                else if (phase == Phase.AwaitUnavailableRuntime) TickUnavailable();
                else if (phase == Phase.SettleUnavailableRuntime) SettleUnavailableRuntime();
                else if (phase == Phase.Unavailable) ObserveUnavailable();
                return phase == Phase.Done;
            }
            catch (Exception error)
            {
                Result.status = "FAIL"; Result.reason = error.ToString(); Dispose(); throw;
            }
        }

        private void TickBoat()
        {
            ObserveBoatTransform();
            if (Time.time - lastCapture >= 0.19f) CaptureBoat();
            if (Time.time - phaseStarted < BoatDuration) return;
            Result.boatObservedSeconds = Time.time - phaseStarted;
            Result.boatNormalizedCycles = boatAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - boatNormalizedStart;
            Result.boatDistinctSprites = boatSprites.Count;
            Require(Result.boatNormalizedCycles >= 2f && boatSprites.Count == 4, "Did not observe two native boat cycles with all four sprites.");
            Require(Result.boatMaxTransformDrift < 0.0001f && Result.boatMaxRotationDrift < 0.001f && Result.boatMaxScaleDrift < 0.0001f,
                "Boat transform drifted during native playback.");
            Result.checks.Add("boat: two native 5s cycles, four sprites, stable position/rotation/scale");
            BeginRelease();
        }

        private void BeginRelease()
        {
            Require(inventory.GetAmount(FarmAnimalCatalog.ItemFeed) == 0,
                "Probe requires zero feed so interaction cannot mutate animal care state.");
            Require(inventory.AddItem(FarmAnimalCatalog.ItemChickChicken, 1), "Could not add one temporary known chick to isolated inventory.");
            var result = releaseHandler.TryReleaseFromInventory(inventory.gameObject, out var instanceId);
            Require(result == AnimalReleaseHandler.ReleaseAttemptResult.Success && !string.IsNullOrWhiteSpace(instanceId), "Canonical chicken release failed: " + result);
            Result.releaseSucceeded = true; Result.animalInstanceId = instanceId;
            phase = Phase.AwaitReleasedRuntime; phaseStarted = Time.time; lastCapture = -100f;
        }

        private void AcquireReleasedRuntime()
        {
            if (!releaseHandler.TryGetRuntime(Result.animalInstanceId, out animal) || animal == null)
            {
                Require(Time.time - phaseStarted < 2f, "Released chicken runtime was not registered."); return;
            }
            releasedAnimal = animal;
            Result.releasedRuntimeCount = RuntimeCount();
            Require(Result.releasedRuntimeCount == 1, "Release did not create exactly one runtime for its AnimalInstanceId.");
            BeginAnimalGeneration();
            phase = Phase.SettleReleasedRuntime; phaseStarted = Time.time;
        }

        private void TickChicken()
        {
            ObserveAnimal();
            float elapsed = Time.time - chickenStarted;
            if (!interactionInvoked && elapsed >= 18f)
            {
                var state = registry.GetAnimal(Result.animalInstanceId);
                interactionState = AnimalStateSnapshot.From(state);
                Result.interactionPrompt = animal.InteractionPrompt;
                Require(animal.CanInteract(inventory.gameObject), "Healthy released chicken is not interactable.");
                animal.Interact(inventory.gameObject); interactionInvoked = true;
            }
            if (elapsed >= 30f && generation == 0)
            {
                releasedSnapshot = animalProvider.Capture(new GameSaveData()) as FarmAnimalsSaveData;
                Require(releasedSnapshot != null, "Provider did not capture the released chicken.");
                accumulatedTicks += tickObserver != null ? tickObserver.FixedTicks : 0;
                UnityEngine.Object.Destroy(animal.gameObject); animal = null; tickObserver = null;
                phase = Phase.AwaitDestroyedForRestore; phaseStarted = Time.time;
                return;
            }
            if (elapsed < ChickenDuration) return;
            Result.chickenObservedSeconds = elapsed;
            FinishChickenAndStartUnavailable();
        }

        private void RestoreReleasedRuntime()
        {
            if (releasedAnimal != null) { Require(Time.time - phaseStarted < 2f, "Released runtime did not destroy before restore."); return; }
            animalProvider.Restore(releasedSnapshot);
            releaseHandler.RespawnExistingAnimals(registry.GetByHousing(HousingId), inventory, registry);
            phase = Phase.AwaitRestoredRuntime; phaseStarted = Time.time;
        }

        private void AcquireRestoredRuntime()
        {
            if (!releaseHandler.TryGetRuntime(Result.animalInstanceId, out animal) || animal == null)
            {
                Require(Time.time - phaseStarted < 2f, "Restored chicken runtime was not registered."); return;
            }
            generation = 1; Result.restoredRuntimeCount = RuntimeCount();
            Result.restoreSucceeded = Result.restoredRuntimeCount == 1;
            Result.instanceIdPreserved = animal.AnimalInstanceId == Result.animalInstanceId;
            Require(Result.restoreSucceeded && Result.instanceIdPreserved, "Restore did not preserve exactly one runtime and its instance ID.");
            BeginAnimalGeneration();
            phase = Phase.SettleRestoredRuntime; phaseStarted = Time.time;
        }

        private void SettleAnimalRuntime(bool released)
        {
            Require(animal != null && tickObserver != null, "Chicken runtime disappeared while settling after spawn/restore.");
            if (tickObserver.FixedTicks < 2)
            {
                Require(Time.time - phaseStarted < 2f, "Chicken runtime did not receive two FixedUpdate ticks after spawn/restore.");
                return;
            }
            Physics2D.SyncTransforms();
            lastAnimalPosition = animal.transform.position;
            lastCapture = -100f;
            Frame(animal.transform.position);
            if (released)
            {
                chickenStarted = Time.time;
                Result.contextFrames.Add(CaptureFull("context_chicken_start"));
            }
            else
            {
                Result.checks.Add("chicken: canonical release and provider-backed restore preserved one runtime/instance ID");
            }
            phase = Phase.Chicken;
        }

        private void FinishChickenAndStartUnavailable()
        {
            accumulatedTicks += tickObserver != null ? tickObserver.FixedTicks : 0;
            Result.fixedTicks = accumulatedTicks;
            Result.chickenDistinctSprites = chickenSprites.Count;
            Require(chickenSprites.Count >= 6 && !chickenSprites.Contains("null"), "Chicken presentation did not show distinct native poses.");
            Result.interactionStateUnchanged = interactionState != null && interactionState.Matches(registry.GetAnimal(Result.animalInstanceId));
            Require(Result.interactionObserved && Result.interactionStateUnchanged, "Interaction freeze was absent or gameplay state changed.");
            Require(Result.walkingTravel > 0.2f && Result.chickenTravel > 0.4f, "Chicken did not demonstrate physical walk displacement.");
            Require(Result.minimumSolidDistance >= -0.011f && Result.minimumSolidDistance < 0.2f,
                "Chicken either penetrated a solid or never demonstrated an obstructed approach.");
            var modes = new HashSet<string>(Result.samples.Where(s => s.subject == "chicken").Select(s => s.mode));
            foreach (var expected in new[] { "Idle", "Walking", "Peck", "Rest" }) Require(modes.Contains(expected), "Missing native chicken mode: " + expected);
            BuildStats();
            Result.checks.Add("chicken: >=60s native motion, all four modes, displacement, full bounds and sampled solid clearance");
            Result.contextFrames.Add(CaptureFull("context_chicken_end"));

            unavailableSnapshot = JsonUtility.FromJson<FarmAnimalsSaveData>(JsonUtility.ToJson(releasedSnapshot));
            var record = unavailableSnapshot.Animals.Single(a => a.AnimalInstanceId == Result.animalInstanceId);
            record.HealthState = (int)AnimalHealthState.Unavailable;
            releasedAnimal = animal;
            UnityEngine.Object.Destroy(animal.gameObject); animal = null; tickObserver = null;
            phase = Phase.AwaitDestroyedForUnavailable; unavailableStarted = Time.time;
        }

        private void RestoreUnavailableRuntime()
        {
            if (releasedAnimal != null) { Require(Time.time - unavailableStarted < 2f, "Healthy runtime did not destroy before health restore."); return; }
            animalProvider.Restore(unavailableSnapshot);
            releaseHandler.RespawnExistingAnimals(registry.GetByHousing(HousingId), inventory, registry);
            phase = Phase.AwaitUnavailableRuntime; unavailableStarted = Time.time; lastAnimalPosition = Vector2.positiveInfinity;
        }

        private void TickUnavailable()
        {
            if (animal == null && !releaseHandler.TryGetRuntime(Result.animalInstanceId, out animal))
            { Require(Time.time - unavailableStarted < 2f, "Unavailable chicken runtime was not restored."); return; }
            BeginAnimalGeneration();
            phase = Phase.SettleUnavailableRuntime; phaseStarted = Time.time;
        }

        private void SettleUnavailableRuntime()
        {
            Require(animal != null && tickObserver != null, "Unavailable chicken runtime disappeared while settling.");
            if (tickObserver.FixedTicks < 2)
            {
                Require(Time.time - phaseStarted < 2f, "Unavailable chicken did not receive two FixedUpdate ticks after restore.");
                return;
            }
            Physics2D.SyncTransforms();
            lastAnimalPosition = animal.transform.position;
            unavailableStarted = Time.time;
            phase = Phase.Unavailable;
        }

        private void ObserveUnavailable()
        {
            if (!float.IsPositiveInfinity(lastAnimalPosition.x))
                Require(Vector2.Distance(lastAnimalPosition, animal.transform.position) < 0.005f, "Unavailable chicken moved.");
            lastAnimalPosition = animal.transform.position;
            if (Time.time - unavailableStarted < 1f) return;
            Result.unavailableStopped = animal.CurrentHealthState == AnimalHealthState.Unavailable &&
                animal.MotionMode == AnimalMotionMode.Stopped && animal.IsMotionPaused;
            Require(Result.unavailableStopped, "Unavailable health did not stop native motion.");
            Result.checks.Add("health: provider-restored Unavailable state remained stopped for one real second");
            Result.endedTime = Time.time; Result.inventoryHashBefore = JsonUtility.ToJson(originalInventory);
            Result.status = "PASS"; phase = Phase.Done; Dispose();
        }

        private void BeginAnimalGeneration()
        {
            Require(animal.MotionProfile != null && animal.MotionProfile.AnimalDataId == FarmAnimalCatalog.AnimalChicken,
                "Released runtime did not resolve the chicken motion profile by AnimalDataId.");
            tickObserver = animal.gameObject.AddComponent<FarmAnimalMotionFixedObserver>();
            lastAnimalPosition = animal.transform.position;
            Result.animalColliderIsTrigger = animal.GetComponentsInChildren<Collider2D>().All(c => c.isTrigger);
            Require(Result.animalColliderIsTrigger, "Animal collider could block the player.");
            Frame(animal.transform.position);
        }

        private void ObserveAnimal()
        {
            Frame(animal.transform.position);
            Vector2 position = animal.transform.position;
            float displacement = Vector2.Distance(lastAnimalPosition, position);
            if (generation >= 0 && displacement < 0.25f)
            {
                Result.chickenTravel += displacement;
                if (animal.MotionMode == AnimalMotionMode.Walking) Result.walkingTravel += displacement;
            }
            lastAnimalPosition = position;
            if (animal.IsInteractionFrozen) Result.interactionObserved = true;
            if (Time.time - lastCapture >= 0.19f) CaptureAnimal(displacement);
        }

        private void CaptureBoat()
        {
            var state = boatAnimator.GetCurrentAnimatorStateInfo(0); var sprite = boat.sprite;
            var sample = new Sample { subject = "boat", sprite = sprite != null ? sprite.name : "null", time = Time.time,
                fixedTime = Time.fixedTime, normalizedTime = state.normalizedTime, position = boatTransform.position,
                rendererCenter = boat.bounds.center, rendererSize = boat.bounds.size };
            sample.path = CaptureCrop("boat", boat.bounds, Result.samples.Count);
            Result.samples.Add(sample); boatSprites.Add(sample.sprite); lastCapture = Time.time;
        }

        private void CaptureAnimal(float displacement)
        {
            var renderer = animal.GetComponentsInChildren<SpriteRenderer>().Single(r => r.enabled);
            var body = animal.GetComponentsInChildren<Collider2D>().Single(c => c.enabled);
            var nearest = solids.Where(c => c != null && c.enabled && !c.transform.IsChildOf(animal.transform))
                .Select(c => new { collider = c, distance = body.Distance(c).distance }).OrderBy(x => x.distance).First();
            Result.minimumSolidDistance = Result.samples.Any(s => s.subject == "chicken")
                ? Mathf.Min(Result.minimumSolidDistance, nearest.distance) : nearest.distance;
            var bounds = animal.MotionBoundsWorld; var center = (Vector2)body.bounds.center;
            var sample = new Sample { subject = "chicken", generation = generation, sprite = renderer.sprite != null ? renderer.sprite.name : "null",
                mode = animal.MotionMode.ToString(), direction = animal.FacingDirection.ToString(), health = animal.CurrentHealthState.ToString(),
                paused = animal.IsMotionPaused, interactionFrozen = animal.IsInteractionFrozen,
                frameIndex = animal.SpriteAnimator != null ? animal.SpriteAnimator.CurrentFrameIndex : -1,
                time = Time.time, fixedTime = Time.fixedTime, fixedTicks = accumulatedTicks + (tickObserver != null ? tickObserver.FixedTicks : 0),
                position = animal.transform.position, displacement = displacement, bodySize = animal.BodySize, motionBounds = bounds,
                bodyBounds = body.bounds, rendererCenter = renderer.bounds.center, rendererSize = renderer.bounds.size,
                nearestSolid = nearest.collider.name, solidDistance = nearest.distance, penetratesSolid = nearest.distance < -0.011f,
                // Rect.Contains excludes its maximum edge; authored spawn bounds include both edges.
                centerInsideBounds = center.x >= animal.AllowedCenterBounds.xMin - 0.0001f &&
                    center.x <= animal.AllowedCenterBounds.xMax + 0.0001f &&
                    center.y >= animal.AllowedCenterBounds.yMin - 0.0001f && center.y <= animal.AllowedCenterBounds.yMax + 0.0001f,
                fullBodyInsideBounds = body.bounds.min.x >= bounds.xMin - 0.011f && body.bounds.max.x <= bounds.xMax + 0.011f &&
                    body.bounds.min.y >= bounds.yMin - 0.011f && body.bounds.max.y <= bounds.yMax + 0.011f };
            sample.path = CaptureCrop("chicken", renderer.bounds, Result.samples.Count);
            Result.samples.Add(sample); chickenSprites.Add(sample.sprite); lastCapture = Time.time;
            Require(sample.centerInsideBounds && sample.fullBodyInsideBounds && !sample.penetratesSolid,
                $"Chicken bounds/solid violation after settled runtime: center=({center.x:F4},{center.y:F4}); " +
                $"allowedCenter=({animal.AllowedCenterBounds.xMin:F4},{animal.AllowedCenterBounds.yMin:F4}).." +
                $"({animal.AllowedCenterBounds.xMax:F4},{animal.AllowedCenterBounds.yMax:F4}); " +
                $"body=({body.bounds.min.x:F4},{body.bounds.min.y:F4})..({body.bounds.max.x:F4},{body.bounds.max.y:F4}); " +
                $"outer=({bounds.xMin:F4},{bounds.yMin:F4})..({bounds.xMax:F4},{bounds.yMax:F4}); " +
                $"nearestSolid={sample.nearestSolid}; signedDistance={sample.solidDistance:F4}; " +
                $"centerInside={sample.centerInsideBounds}; fullBodyInside={sample.fullBodyInsideBounds}; penetrates={sample.penetratesSolid}.");
        }

        private void ObserveBoatTransform()
        {
            Result.boatMaxTransformDrift = Mathf.Max(Result.boatMaxTransformDrift, Vector3.Distance(boatPosition, boatTransform.position));
            Result.boatMaxRotationDrift = Mathf.Max(Result.boatMaxRotationDrift, Quaternion.Angle(boatRotation, boatTransform.rotation));
            Result.boatMaxScaleDrift = Mathf.Max(Result.boatMaxScaleDrift, Vector3.Distance(boatScale, boatTransform.lossyScale));
        }

        private string CaptureCrop(string id, Bounds bounds, int index)
        {
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var target = new RenderTexture(Result.renderWidth, Result.renderHeight, 24, RenderTextureFormat.ARGB32); Texture2D image = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var a = camera.WorldToViewportPoint(bounds.min); var b = camera.WorldToViewportPoint(bounds.max);
                int x0 = Mathf.Clamp(Mathf.FloorToInt(a.x * Result.renderWidth) - 24, 0, Result.renderWidth - 1);
                int y0 = Mathf.Clamp(Mathf.FloorToInt(a.y * Result.renderHeight) - 24, 0, Result.renderHeight - 1);
                int x1 = Mathf.Clamp(Mathf.CeilToInt(b.x * Result.renderWidth) + 24, x0 + 1, Result.renderWidth);
                int y1 = Mathf.Clamp(Mathf.CeilToInt(b.y * Result.renderHeight) + 24, y0 + 1, Result.renderHeight);
                image = new Texture2D(x1 - x0, y1 - y0, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(x0, y0, image.width, image.height), 0, 0); image.Apply();
                string path = Path.Combine(output, id + "_" + index.ToString("D4") + ".png");
                File.WriteAllBytes(path, image.EncodeToPNG()); return path;
            }
            finally
            {
                camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private string CaptureFull(string id)
        {
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var target = new RenderTexture(Result.renderWidth, Result.renderHeight, 24, RenderTextureFormat.ARGB32); Texture2D image = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image = new Texture2D(Result.renderWidth, Result.renderHeight, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, Result.renderWidth, Result.renderHeight), 0, 0); image.Apply();
                string path = Path.Combine(output, id + ".png"); File.WriteAllBytes(path, image.EncodeToPNG()); return path;
            }
            finally
            {
                camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private void BuildStats()
        {
            foreach (var group in Result.samples.Where(s => s.subject == "chicken").GroupBy(s => s.mode + "/" + s.direction))
                Result.stateStats.Add(new StateStats { mode = group.First().mode, direction = group.First().direction,
                    samples = group.Count(), distinctSprites = group.Select(s => s.sprite).Distinct().Count(), displacement = group.Sum(s => s.displacement) });
        }

        private int RuntimeCount() => Components<FarmAnimalRuntime>(scene).Count(r => r.AnimalInstanceId == Result.animalInstanceId);
        private void Frame(Vector3 center) => camera.transform.position = new Vector3(center.x, center.y, originalCamera.z);

        public void Dispose()
        {
            if (disposed) return; disposed = true;
            try
            {
                foreach (var runtime in Components<FarmAnimalRuntime>(scene).Where(r => r.AnimalInstanceId == Result.animalInstanceId).ToArray())
                    if (runtime != null) UnityEngine.Object.Destroy(runtime.gameObject);
                if (obstacleFixture != null) UnityEngine.Object.Destroy(obstacleFixture);
                inventory.RestoreFromSaveData(originalInventory);
                animalProvider.Restore(originalAnimals);
                Result.inventoryHashAfter = JsonUtility.ToJson(inventory.CaptureSaveData());
                if (Result.status == "PASS") Require(Result.inventoryHashBefore == Result.inventoryHashAfter, "Temporary inventory state was not restored in memory.");
            }
            finally
            {
                Result.endedTime = Time.time;
                if (camera != null) camera.transform.position = originalCamera;
                if (follow != null) follow.enabled = originalFollow;
            }
        }

        private static IEnumerable<T> Components<T>(Scene source) where T : Component => source.IsValid()
            ? source.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)) : Enumerable.Empty<T>();
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

        private sealed class AnimalStateSnapshot
        {
            private bool fed, product; private int care, lastProduct; private AnimalHealthState health;
            internal static AnimalStateSnapshot From(AnimalInstanceState state) => new AnimalStateSnapshot
            { fed = state.FedToday, product = state.ProductReady, care = state.CareScore, lastProduct = state.LastProductDay, health = state.HealthState };
            internal bool Matches(AnimalInstanceState state) => state != null && state.FedToday == fed && state.ProductReady == product &&
                state.CareScore == care && state.LastProductDay == lastProduct && state.HealthState == health;
        }
    }

    internal sealed class FarmAnimalMotionFixedObserver : MonoBehaviour
    {
        internal int FixedTicks { get; private set; }
        private void FixedUpdate() => FixedTicks++;
    }
}
