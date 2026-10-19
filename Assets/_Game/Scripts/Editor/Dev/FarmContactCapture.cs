using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CindarsHope.Camera;
using CindarsHope.Diagnostics;
using CindarsHope.Farm;
using CindarsHope.Farm.Scene;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.World;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Dev
{
    /// <summary>Optional contact-only branch of the existing isolated capture session.</summary>
    internal sealed class FarmContactCapture : IDisposable
    {
        [Serializable] internal sealed class Sample
        {
            public string phase, path, fountainSprite, selected;
            public string[] touchingSolids;
            public float time, fixedTime;
            public Vector2 feet;
            public Bounds bodyBounds;
            public Vector3 cameraPosition;
            public bool canFish;
            public int fixedSteps;
        }
        [Serializable] internal sealed class Evidence
        {
            public string status = "RUNNING", reason;
            public string method = "Controlled actual Rigidbody2D pushes via existing FarmProbeBodyDriver; PlayerController temporarily disabled. Safe initial placements are framing setup, not traversals. Live native animation, collision and interaction selection are observed; no sorting/sprite/alpha forcing, save/load, portal or fishing rewards invoked. PASS covers listed contact/selection checks, not input, actual cast success, visual acceptance or complete circulation. PNGs plus world collider geometry support separate overlay review.";
            public float orthographicSize, cameraAspect;
            public int width, height;
            public List<Sample> samples = new List<Sample>();
            public List<string> checks = new List<string>();
            public List<Setup> setups = new List<Setup>();
            public Bounds[] fountainBodies;
            public Vector2[] lakePolygon, lowerRiverPolygon;
            public int fountainDistinctFrames;
        }
        [Serializable] internal sealed class Setup
        {
            public string phase, predictedContact;
            public Vector2 position;
            public float predictedContactDistance;
            public int candidatesChecked;
            public List<string> rejected = new List<string>();
        }
        private sealed class Push { public string id; public Vector2 start, target; }
        private readonly PlayerController player;
        private readonly Rigidbody2D body;
        private readonly Collider2D solid;
        private readonly Collider2D[] obstacles;
        private readonly InteractionSystem selector;
        private readonly FishingSpot fishing;
        private readonly BoxCollider2D stance;
        private readonly SpriteRenderer fountain;
        private readonly UnityEngine.Camera camera;
        private readonly CameraFollow2D follow;
        private readonly FarmProbeBodyDriver driver;
        private readonly bool playerEnabled, followEnabled;
        private readonly Vector3 originalPlayer, originalCamera;
        private readonly Vector2 originalVelocity;
        private readonly float originalAngularVelocity;
        private readonly string output;
        private readonly List<Push> pushes = new List<Push>();
        private readonly HashSet<string> fountainFrames = new HashSet<string>();
        private readonly RaycastHit2D[] setupHits = new RaycastHit2D[64];
        private int index = -1, initialSteps;
        private float began, lastCapture;
        private Vector2 prepared;
        private bool disposed;
        internal Evidence Result { get; } = new Evidence();

        internal FarmContactCapture(PlayerController player, UnityEngine.Camera camera, string directory)
        {
            this.player = player; this.camera = camera;
            body = player.GetComponent<Rigidbody2D>();
            solid = player.GetComponentsInChildren<Collider2D>().Single(c => c.enabled && !c.isTrigger);
            selector = player.GetComponentsInChildren<InteractionSystem>().Single();
            var roots = player.gameObject.scene.GetRootGameObjects();
            obstacles = roots.SelectMany(r => r.GetComponentsInChildren<Collider2D>())
                .Where(c => c.enabled && !c.isTrigger && !c.transform.IsChildOf(player.transform) &&
                    (Physics2D.GetLayerCollisionMask(solid.gameObject.layer) & (1 << c.gameObject.layer)) != 0).ToArray();
            var source = roots.Single(r => r.name == "FonteAnya");
            fountain = source.transform.Find("FountainDepthGroup/Visual").GetComponent<SpriteRenderer>();
            fishing = roots.Single(r => r.name == "FishingSpot").GetComponent<FishingSpot>();
            stance = fishing.transform.Find("FishingStanceZone").GetComponent<BoxCollider2D>();
            follow = camera.GetComponent<CameraFollow2D>();
            Require(body != null && body.bodyType == RigidbodyType2D.Dynamic && follow != null && camera.orthographic,
                "Expected dynamic player and gameplay orthographic follow camera.");
            originalPlayer = player.transform.position; originalCamera = camera.transform.position;
            originalVelocity = body.linearVelocity; originalAngularVelocity = body.angularVelocity;
            playerEnabled = player.enabled; followEnabled = follow.enabled;
            Result.orthographicSize = camera.orthographicSize; Result.cameraAspect = camera.aspect;
            Result.width = camera.pixelWidth; Result.height = camera.pixelHeight;
            Result.fountainBodies = source.GetComponentsInChildren<Collider2D>().Where(c => !c.isTrigger).Select(c => c.bounds).ToArray();
            Require(Result.fountainBodies.Length == 5, "Expected basin and four measured column feet.");
            Result.lakePolygon = FarmSceneSpatialContract.LakeCollisionPath.ToArray();
            Result.lowerRiverPolygon = FarmSceneSpatialContract.RiverBelowBridgeCollisionPath.ToArray();
            var rect = FarmSettlementPhysicsContract.FountainBasin;
            var offset = (Vector2)solid.bounds.center - body.position;
            for (int i = 0; i < 8; i++)
            {
                var direction = new Vector2(Mathf.Cos(i * Mathf.PI / 4), Mathf.Sin(i * Mathf.PI / 4));
                var radius = rect.Size * 0.5f + (Vector2)solid.bounds.extents + Vector2.one * 0.5f;
                pushes.Add(new Push { id = "fountain_" + i, start = rect.Center + Vector2.Scale(direction, radius) - offset,
                    target = rect.Center - offset });
            }
            foreach (var foot in FarmSettlementPhysicsContract.FountainColumnFeet)
            {
                // The lower columns have barrels on their south side. Approach their north
                // faces through the gap between the upper/lower column feet instead. The eight
                // basin sectors above remain unchanged; column checks still hit the named foot.
                var approach = foot.Center.y < rect.Center.y ? Vector2.up : Vector2.down;
                pushes.Add(new Push { id = foot.Id, start = foot.Center + approach * (foot.Size.y * 0.5f + solid.bounds.extents.y + 0.45f) - offset,
                    target = foot.Center - offset });
            }
            var banks = new[] { new Vector2(25.6f,-2f), new Vector2(25.5f,-4f), new Vector2(32.3f,-2f), new Vector2(33f,-4f) };
            for (int i = 0; i < banks.Length; i++) pushes.Add(new Push { id = "confluence_" + i,
                start = banks[i] - offset, target = new Vector2(29f, banks[i].y) - offset });
            output = Path.Combine(directory, "contact");
            Directory.CreateDirectory(output);
            driver = player.gameObject.AddComponent<FarmProbeBodyDriver>();
            driver.Body = body;
            player.enabled = false; follow.enabled = false;
        }

        internal bool Tick()
        {
            try
            {
                Require(Mathf.Abs(camera.orthographicSize - Result.orthographicSize) < 0.001f, "Gameplay zoom changed.");
                if (index < 0) { index = 0; Prepare(); return false; }
                float elapsed = Time.time - began;
                if (Time.time - lastCapture >= 0.2f) { Capture(); lastCapture = Time.time; }
                if (elapsed < 1.6f || driver.FixedSteps - initialSteps < 12) return false;
                driver.Driving = false; body.linearVelocity = Vector2.zero;
                if (index < pushes.Count)
                {
                    Require(Vector2.Distance(body.position, pushes[index].target) > 0.18f, "Entered solid target: " + pushes[index].id);
                    Require(Vector2.Distance(body.position, prepared) > 0.04f, "No demonstrated approach movement: " + pushes[index].id);
                    Require(IsClear(), "Player penetrated a solid: " + pushes[index].id);
                    Require(obstacles.Any(c => c != null && c.enabled && IsExpectedContact(c) && solid.Distance(c).distance < 0.04f), "No observed intended solid contact at " + Phase);
                    Result.checks.Add(pushes[index].id + ": physical push blocked without penetration");
                }
                else
                {
                    bool expected = index == pushes.Count;
                    Require(fishing.CanInteract(player.gameObject) == expected, "Fishing stance result mismatch at " + Phase);
                    Require((selector.GetCurrentInteractable() == fishing) == expected, "Live fishing selection mismatch at " + Phase);
                    Require(IsClear(), "Fishing stance overlaps water/solid: " + Phase);
                    if (expected) Require(Vector2.Distance(body.position, stance.bounds.center) < 0.1f, "Did not reach tip through deck.");
                    Result.checks.Add(Phase + ": selection=" + expected);
                }
                Capture(); index++;
                if (index < pushes.Count + 3) { Prepare(); return false; }
                Result.fountainDistinctFrames = fountainFrames.Count;
                Require(fountainFrames.Count == 5, "Not all five native fountain poses were observed.");
                Result.status = "PASS"; Dispose(); return true;
            }
            catch (Exception e) { Result.status = "FAIL"; Result.reason = e.ToString(); Dispose(); throw; }
        }
        private string Phase => index < pushes.Count ? pushes[index].id :
            index == pushes.Count ? "fishing_tip" : index == pushes.Count + 1 ? "fishing_center" : "fishing_entrance";
        private void Prepare()
        {
            Vector2 start = index < pushes.Count ? pushes[index].start :
                index == pushes.Count + 2 ? new Vector2(FarmLevel1LayoutContract.DockEntranceX, FarmLevel1LayoutContract.DockEntranceY) :
                    new Vector2(FarmLevel1LayoutContract.FishingSpotX, FarmLevel1LayoutContract.FishingSpotY);
            driver.Driving = false;
            // Search nearby two-dimensional positions in the same approach sector. A radial-only
            // search can remain trapped between a basin and a column even when a clear lane exists.
            var outward = index < pushes.Count ? (start - pushes[index].target).normalized : Vector2.up;
            var setup = new Setup { phase = Phase };
            Result.setups.Add(setup);
            bool clear = false;
            for (int ring = 0; ring <= 8 && !clear; ring++)
            for (int x = -ring; x <= ring && !clear; x++)
            for (int y = -ring; y <= ring && !clear; y++)
            {
                if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != ring) continue;
                // Fishing checks retain their exact authored start; shifting them could hide a
                // blocked entrance or accidentally turn a negative selection check into another spot.
                if (index >= pushes.Count && ring != 0) continue;
                var candidate = start + new Vector2(x, y) * 0.2f;
                if (index < pushes.Count && Vector2.Dot((candidate - pushes[index].target).normalized, outward) < 0.9238795f)
                    continue; // +/-22.5 degrees preserves the intended cardinal/diagonal approach.
                setup.candidatesChecked++;
                body.position = candidate; body.linearVelocity = Vector2.zero;
                player.transform.position = new Vector3(body.position.x, body.position.y, originalPlayer.z);
                Physics2D.SyncTransforms();
                var blocker = obstacles.FirstOrDefault(c => c != null && c.enabled && solid.Distance(c).distance <= 0.12f);
                if (blocker != null)
                {
                    NoteRejection(setup, "clearance: " + blocker.name);
                    continue;
                }
                if (index < pushes.Count && !HasUsableApproach(setup)) continue;
                setup.position = candidate;
                clear = true;
            }
            Require(clear, "No collision-free intended approach for " + Phase + " after " + setup.candidatesChecked +
                " sector-grid candidates; " + string.Join(", ", setup.rejected));
            prepared = body.position;
            driver.Target = index < pushes.Count ? pushes[index].target :
                index == pushes.Count ? (Vector2)stance.bounds.center : body.position;
            driver.Driving = index <= pushes.Count;
            camera.transform.position = new Vector3(driver.Target.x, driver.Target.y + 1.5f, originalCamera.z);
            began = Time.time; initialSteps = driver.FixedSteps; lastCapture = float.NegativeInfinity;
        }
        private bool HasUsableApproach(Setup setup)
        {
            var delta = pushes[index].target - body.position;
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(solid.gameObject.layer));
            int count = solid.Cast(delta.normalized, filter, setupHits, delta.magnitude);
            if (count == setupHits.Length) { NoteRejection(setup, "cast buffer full"); return false; }
            Collider2D first = null;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var hit = setupHits[i];
                if (hit.collider == null || hit.collider.transform.IsChildOf(player.transform) || hit.distance >= distance) continue;
                first = hit.collider; distance = hit.distance;
            }
            if (first == null) { NoteRejection(setup, "cast misses solid"); return false; }
            if (!IsExpectedContact(first)) { NoteRejection(setup, "first contact: " + first.name); return false; }
            // Driver moves at3u/s for1.6s. Leave time for an actual stable contact, and enough
            // free travel to prove movement rather than starting against the collider skin.
            if (distance < 0.16f || distance > 4.2f) { NoteRejection(setup, "cast travel outside0.16..4.2u"); return false; }
            setup.predictedContact = first.name;
            setup.predictedContactDistance = distance;
            return true;
        }
        private bool IsExpectedContact(Collider2D collider)
        {
            if (Phase.StartsWith("fountain_", StringComparison.Ordinal))
                return collider.name == "SolidBasin" || collider.name.StartsWith("FountainColumn", StringComparison.Ordinal);
            if (Phase.StartsWith("FountainColumn", StringComparison.Ordinal)) return collider.name == Phase;
            return collider.name == "RiverCollider_BelowBridge" || collider.name == "Collision_farm_spatial_lake";
        }
        private static void NoteRejection(Setup setup, string reason)
        {
            if (setup.rejected.Count < 12 && !setup.rejected.Contains(reason)) setup.rejected.Add(reason);
        }
        private bool IsClear() => obstacles.All(c => c == null || !c.enabled || solid.Distance(c).distance >= -0.01f);
        private void Capture()
        {
            var target = RenderTexture.GetTemporary(Result.width, Result.height, 24);
            var prior = camera.targetTexture; var active = RenderTexture.active;
            Texture2D image = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image = new Texture2D(Result.width, Result.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0,0,Result.width,Result.height),0,0); image.Apply();
                string path = Path.Combine(output, Phase + "_" + Result.samples.Count.ToString("D4") + ".png");
                File.WriteAllBytes(path,image.EncodeToPNG());
                var sprite = fountain.sprite != null ? fountain.sprite.name : "null";
                if (index < 12 && sprite != "null") fountainFrames.Add(sprite);
                var selected = selector.GetCurrentInteractable();
                Result.samples.Add(new Sample { phase = Phase, path = path, time = Time.time, fixedTime = Time.fixedTime,
                    feet = body.position, bodyBounds = solid.bounds, cameraPosition = camera.transform.position,
                    touchingSolids = obstacles.Where(c => c != null && c.enabled && solid.Distance(c).distance < 0.04f).Select(c => c.name).ToArray(),
                    fountainSprite = sprite, canFish = fishing.CanInteract(player.gameObject),
                    selected = selected is Component component ? component.name : "none", fixedSteps = driver.FixedSteps });
            }
            finally
            {
                camera.targetTexture = prior; RenderTexture.active = active;
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                RenderTexture.ReleaseTemporary(target);
            }
        }
        public void Dispose()
        {
            if (disposed) return; disposed = true;
            if (driver != null) { driver.Driving = false; UnityEngine.Object.DestroyImmediate(driver); }
            if (player != null)
            {
                player.transform.position = originalPlayer;
                body.position = originalPlayer; body.linearVelocity = originalVelocity; body.angularVelocity = originalAngularVelocity;
                player.enabled = playerEnabled;
            }
            if (camera != null) camera.transform.position = originalCamera;
            if (follow != null) follow.enabled = followEnabled;
            Physics2D.SyncTransforms();
        }
        private static void Require(bool condition, string reason) { if (!condition) throw new InvalidOperationException(reason); }
    }
}

