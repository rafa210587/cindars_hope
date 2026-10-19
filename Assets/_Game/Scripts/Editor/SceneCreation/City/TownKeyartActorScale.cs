using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CindarsHope.Editor.ScaleSystem;
using CindarsHope.NPC;
using CindarsHope.Player;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Town-only authoring pilot. No runtime component, shared-profile edit or scene/save write.</summary>
    public static class TownKeyartActorScale
    {
        public const float Factor = 2.25f;
        public const float PhysicalTolerance = 0.01f;
        public const string ProfileFolder = "Assets/_Game/Data/TownVisualScale";
        private const string GlobalFolder = "Assets/_Game/Data/Scale";
        private const string ExtraNpc = "npc_vaalara_wanderer_01";

        [Serializable] public sealed class FileStamp { public string path, sha256; }
        [Serializable] public sealed class PhysicalShape
        {
            public string path, type, rigidbodyPath;
            public bool enabled, active, trigger;
            public int layer;
            public Vector2 centerRelativeToActor;
            public Vector2[] cornersRelativeToActor;
            public float radius;
        }
        [Serializable] public sealed class ActorState
        {
            public string id, objectPath, profilePath, profileId, category;
            public Vector3 position, localScale;
            public float profileVisualScale, profileColliderScale;
            public PhysicalShape[] shapes;
        }
        [Serializable] public sealed class ActorChange
        {
            public string id, status;
            public Vector3 originalAuthoringScale;
            public float physicalMaxDelta;
            public ActorState expectedOriginalAfterAwake, after;
        }
        [Serializable] public sealed class Report
        {
            public string status = "FAIL", utc, unityVersion, scenePath, outputPath, sourceHash;
            public string baseline = "Original global profile after Awake; Player scale is 2, not the legacy 1.3 editor preview. ColliderScale 1 is identity. Geometry is sampled from actual collider shapes and transforms, not inferred from sprite bounds.";
            public string limits = "Editor authoring geometry only. Does not prove Awake, input, contacts, routes, interaction selection or Farm/Town transitions. ALREADY_APPLIED records do not provide a fresh pre-pilot baseline.";
            public float factor = Factor, tolerance = PhysicalTolerance;
            public FileStamp[] globalsBefore, globalsAfter, contractInputs;
            public List<ActorChange> actors = new List<ActorChange>();
        }
        private sealed class Actor
        {
            public string id;
            public GameObject root;
            public VisualScaleApplicator applicator;
            public VisualScaleProfileSO source, target;
            public bool applied;
            public Collider2D[] colliders;
            public Transform[] branches;
        }
        private sealed class UndoActor
        {
            public Actor actor;
            public VisualScaleProfileSO profile;
            public Transform[] transforms;
            public Vector3[] positions, scales;
            public Vector2[] offsets, sizes;
            public float[] radii;
        }

        /// <summary>Call once at the end of the Town factory, before SaveScene. Throws on unsupported input.</summary>
        public static Report Apply(Scene scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Town scale authoring cannot run in Play Mode.");
            RequireTown(scene);
            var actors = Gather(scene);
            foreach (var actor in actors) Preflight(actor);
            var globalsBefore = GlobalStamps();
            string outputPath = OutputPath();
            // Preflight the entire roster before generating profiles or changing any actor.
            foreach (var group in actors.GroupBy(a => a.source))
            {
                var target = ResolveTarget(group.Key);
                foreach (var actor in group) actor.target = target;
            }
            var report = new Report
            {
                utc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion,
                scenePath = scene.path, globalsBefore = globalsBefore, contractInputs = ContractStamps(), outputPath = outputPath,
                sourceHash = Hash("Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartActorScale.cs")
            };
            var undo = actors.Select(SnapshotUndo).ToArray();
            try
            {
                foreach (var actor in actors)
                {
                    var change = new ActorChange { id = actor.id, originalAuthoringScale = actor.root.transform.localScale };
                    if (actor.applied)
                    {
                        change.status = "ALREADY_APPLIED";
                        change.after = Observe(actor);
                    }
                    else
                    {
                        // Establish the reviewed, global post-Awake baseline before physical compensation.
                        actor.root.transform.localScale = Vector3.one * actor.source.VisualScale;
                        change.expectedOriginalAfterAwake = Observe(actor);
                        foreach (var collider in actor.colliders.Where(c => c.transform == actor.root.transform))
                        {
                            collider.offset /= Factor;
                            if (collider is BoxCollider2D box) box.size /= Factor;
                            else if (collider is CircleCollider2D circle) circle.radius /= Factor;
                        }
                        // Compensate each immediate child branch once, not once per descendant collider.
                        foreach (var branch in actor.branches)
                        {
                            branch.localPosition /= Factor;
                            branch.localScale /= Factor;
                        }
                        actor.applicator.Configure(actor.target);
                        actor.root.transform.localScale = Vector3.one * actor.target.VisualScale;
                        change.after = Observe(actor);
                        change.physicalMaxDelta = ComparePhysics(change.expectedOriginalAfterAwake, change.after);
                        change.status = "PASS";
                    }
                    report.actors.Add(change);
                }
                report.globalsAfter = GlobalStamps();
                RequireSameStamps(report.globalsBefore, report.globalsAfter);
                report.status = report.actors.All(a => a.status == "PASS") ? "PASS" : "IDEMPOTENT_NO_NEW_BASELINE";
                using (var stream = new FileStream(report.outputPath, FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream)) writer.Write(JsonUtility.ToJson(report, true));
                Debug.Log("[TownKeyartActorScale] " + report.status + "; actors=" + report.actors.Count + "; evidence=" + report.outputPath);
                return report;
            }
            catch
            {
                foreach (var state in undo) Restore(state);
                throw;
            }
        }

        /// <summary>Read-only measurements for the capture session, including after normal Awake.</summary>
        public static ActorState[] Observe(Scene scene)
        {
            RequireTown(scene);
            var actors = Gather(scene);
            foreach (var actor in actors) Preflight(actor);
            if (actors.Any(a => !a.applied)) throw new InvalidOperationException("Town pilot roster contains an actor still using a global profile.");
            return actors.Select(Observe).ToArray();
        }

        public static float ComparePhysics(ActorState before, ActorState after)
        {
            if (before == null || after == null || before.id != after.id || before.shapes == null || before.shapes.Length == 0 || after.shapes == null || before.shapes.Length != after.shapes.Length)
                throw new InvalidOperationException("Actor physical snapshot is missing, empty or mismatched.");
            float delta = 0;
            for (int i = 0; i < before.shapes.Length; i++)
            {
                var a = before.shapes[i]; var b = after.shapes[i];
                if (a == null || b == null || a.cornersRelativeToActor == null || b.cornersRelativeToActor == null)
                    throw new InvalidOperationException("Collider geometry evidence is null: " + before.id);
                if (a.path != b.path || a.type != b.type || a.rigidbodyPath != b.rigidbodyPath || a.enabled != b.enabled || a.active != b.active || a.trigger != b.trigger || a.layer != b.layer)
                    throw new InvalidOperationException("Collider identity/state changed: " + before.id + "/" + a.path);
                delta = Mathf.Max(delta, Vector2.Distance(a.centerRelativeToActor, b.centerRelativeToActor), Mathf.Abs(a.radius - b.radius));
                if (a.cornersRelativeToActor.Length != b.cornersRelativeToActor.Length) throw new InvalidOperationException("Collider geometry type changed.");
                for (int j = 0; j < a.cornersRelativeToActor.Length; j++)
                    delta = Mathf.Max(delta, Vector2.Distance(a.cornersRelativeToActor[j], b.cornersRelativeToActor[j]));
            }
            if (delta > PhysicalTolerance) throw new InvalidOperationException("World collider delta exceeds " + PhysicalTolerance + "u: " + before.id + " delta=" + delta);
            return delta;
        }

        public static FileStamp[] GlobalStamps()
        {
            var paths = Directory.GetFiles(GlobalFolder, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal).ToArray();
            if (paths.Length == 0) throw new InvalidOperationException("Shared scale profile directory is empty.");
            return paths.Select(p => new FileStamp { path = p.Replace('\\', '/'), sha256 = Hash(p) }).ToArray();
        }
        public static FileStamp[] ContractStamps()
        {
            string[] paths =
            {
                "Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartActorScale.cs",
                "Assets/_Game/Scripts/World/Scale/VisualScaleApplicator.cs",
                "Assets/_Game/Scripts/World/Scale/VisualScaleProfileSO.cs",
                "Assets/_Game/Scripts/Player/PlayerController.cs",
                "Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs",
                "Assets/_Game/Scripts/NPC/NpcWalkAnimator.cs",
                "Assets/_Game/Scripts/NPC/NpcPhysicsBody.cs",
                "Assets/_Game/Scripts/Interaction/InteractionSystem.cs"
            };
            return paths.OrderBy(p => p, StringComparer.Ordinal).Select(p => new FileStamp { path = p, sha256 = Hash(p) }).ToArray();
        }
        public static void RequireSameStamps(FileStamp[] before, FileStamp[] after)
        {
            if (before == null || after == null || before.Length == 0 || before.Length != after.Length ||
                !before.Select(s => s.path + ":" + s.sha256).SequenceEqual(after.Select(s => s.path + ":" + s.sha256)))
                throw new InvalidOperationException("Scale profile/contract input files changed or hash evidence is missing.");
        }

        private static Actor[] Gather(Scene scene)
        {
            var players = Components<PlayerController>(scene).ToArray();
            if (players.Length != 1) throw new InvalidOperationException("Town scale requires exactly one PlayerController.");
            var npcRoots = Components<NpcShopController>(scene).Select(c => c.gameObject)
                .Concat(Components<NpcController>(scene).Select(c => c.gameObject)).Distinct().ToArray();
            var result = new List<Actor> { new Actor { id = "player", root = players[0].gameObject } };
            foreach (var root in npcRoots)
            {
                var ids = root.GetComponents<NpcShopController>().Select(c => c.NpcData != null ? c.NpcData.NpcId : "")
                    .Concat(root.GetComponents<NpcController>().Select(c => c.NpcId)).Distinct().ToArray();
                if (ids.Length != 1 || string.IsNullOrEmpty(ids[0])) throw new InvalidOperationException("Missing/conflicting NPC ID: " + root.name);
                result.Add(new Actor { id = ids[0], root = root });
            }
            var expected = TownCityLayout.AllNpcPlaces.Select(p => p.NpcId).Concat(new[] { ExtraNpc }).OrderBy(id => id, StringComparer.Ordinal).ToArray();
            if (expected.Length != 29 || !result.Where(a => a.id != "player").Select(a => a.id).OrderBy(id => id, StringComparer.Ordinal).SequenceEqual(expected))
                throw new InvalidOperationException("Town scale requires all 28 canonical NPC IDs plus the preserved wanderer, once each.");
            return result.OrderBy(a => a.id, StringComparer.Ordinal).ToArray();
        }

        private static void Preflight(Actor actor)
        {
            actor.applicator = actor.root.GetComponent<VisualScaleApplicator>();
            if (actor.applicator == null || actor.applicator.Profile == null || actor.root.GetComponent<SpriteRenderer>() == null || actor.root.GetComponent<Rigidbody2D>() == null)
                throw new InvalidOperationException("Missing root renderer/body/scale profile: " + actor.id);
            if ((actor.id == "player" && actor.root.GetComponent<PlayerWalkAnimator>() == null) || (actor.id != "player" && actor.root.GetComponent<NpcWalkAnimator>() == null))
                throw new InvalidOperationException("Actor animator must stay on the same root: " + actor.id);
            var serialized = new SerializedObject(actor.applicator);
            var visual = serialized.FindProperty("_visualRoot").objectReferenceValue;
            if ((visual != null && visual != actor.root.transform) || !serialized.FindProperty("_applyOnAwake").boolValue)
                throw new InvalidOperationException("Unsupported visual root/Awake contract: " + actor.id);
            actor.source = ScaleProfileLibrary.Get(actor.applicator.Profile.Category);
            if (actor.source == null || !AssetDatabase.GetAssetPath(actor.source).StartsWith(GlobalFolder + "/", StringComparison.Ordinal) ||
                Mathf.Abs(actor.source.VisualScale - ExpectedGlobalScale(actor.source.Category)) > 0.0001f || !Mathf.Approximately(actor.source.ColliderScale, 1f))
                throw new InvalidOperationException("Unsupported global scale or multiplicative collider profile: " + actor.id);
            if ((actor.id == "player") != (actor.source.Category == EntityScaleCategory.Player))
                throw new InvalidOperationException("Player/NPC profile category mismatch: " + actor.id);
            actor.applied = AssetDatabase.GetAssetPath(actor.applicator.Profile) == TargetPath(actor.source);
            if (actor.applied)
            {
                ValidateClone(actor.source, actor.applicator.Profile);
                if (Vector3.Distance(actor.root.transform.localScale, Vector3.one * actor.applicator.Profile.VisualScale) > 0.0001f)
                    throw new InvalidOperationException("Town profile identity exists but baked root scale is inconsistent: " + actor.id);
            }
            else if (actor.applicator.Profile != actor.source) throw new InvalidOperationException("Unrecognized actor profile; no fallback: " + actor.id);
            if (Quaternion.Angle(actor.root.transform.rotation, Quaternion.identity) > 0.001f)
                throw new InvalidOperationException("Rotated actor requires a separately reviewed physical contract: " + actor.id);
            for (var parent = actor.root.transform.parent; parent != null; parent = parent.parent)
                if (Vector3.Distance(parent.localScale, Vector3.one) > 0.0001f || Quaternion.Angle(parent.localRotation, Quaternion.identity) > 0.001f)
                    throw new InvalidOperationException("Actor parent must have unit scale and rotation: " + actor.id);
            actor.colliders = actor.root.GetComponentsInChildren<Collider2D>(true).OrderBy(c => ColliderKey(c, actor.root.transform), StringComparer.Ordinal).ToArray();
            if (actor.colliders.Select(c => ColliderKey(c, actor.root.transform)).Distinct().Count() != actor.colliders.Length)
                throw new InvalidOperationException("Collider hierarchy has ambiguous duplicate paths: " + actor.id);
            if (actor.colliders.Length < 2 || actor.colliders.All(c => c.isTrigger) || actor.colliders.All(c => !c.isTrigger))
                throw new InvalidOperationException("Actor needs its actual solid and interaction colliders: " + actor.id);
            var branches = new HashSet<Transform>();
            foreach (var collider in actor.colliders)
            {
                if (!(collider is BoxCollider2D) && !(collider is CircleCollider2D)) throw new InvalidOperationException("Unsupported collider shape: " + actor.id + "/" + collider.GetType().Name);
                if (collider.attachedRigidbody != actor.root.GetComponent<Rigidbody2D>()) throw new InvalidOperationException("Collider must use the actor root rigidbody: " + actor.id);
                if (collider is BoxCollider2D box && (box.edgeRadius != 0 || box.autoTiling)) throw new InvalidOperationException("Rounded/auto-tiled actor boxes require explicit support: " + actor.id);
                for (var part = collider.transform; part != actor.root.transform; part = part.parent)
                    if (Quaternion.Angle(part.localRotation, Quaternion.identity) > 0.001f)
                        throw new InvalidOperationException("Rotated physical child requires explicit shape support: " + actor.id);
                var scale = collider.transform.lossyScale;
                if (scale.x <= 0 || scale.y <= 0 || (collider is CircleCollider2D && Mathf.Abs(scale.x - scale.y) > 0.0001f))
                    throw new InvalidOperationException("Unsupported mirrored/nonuniform circle: " + actor.id);
                if (collider.transform == actor.root.transform) continue;
                var branch = collider.transform;
                while (branch.parent != actor.root.transform) branch = branch.parent;
                if (branch.GetComponentsInChildren<SpriteRenderer>(true).Length != 0)
                    throw new InvalidOperationException("Physical child also owns art; compensation would alter visual intent: " + actor.id + "/" + branch.name);
                branches.Add(branch);
            }
            actor.branches = branches.ToArray();
        }

        private static float ExpectedGlobalScale(EntityScaleCategory category)
        {
            switch (category)
            {
                case EntityScaleCategory.Player: case EntityScaleCategory.NPC: return 2f;
                case EntityScaleCategory.NpcDwarf: return 1.8f;
                case EntityScaleCategory.NpcSmallfolk: return 1.7f;
                case EntityScaleCategory.NpcOrc: return 2.6f;
                case EntityScaleCategory.NpcDragonborn: return 2.14f;
                default: throw new InvalidOperationException("Category outside the Town actor pilot: " + category);
            }
        }
        private static string TargetPath(VisualScaleProfileSO source) => ProfileFolder + "/VisualScaleProfile_town_" + source.Category.ToString().ToLowerInvariant() + ".asset";
        private static VisualScaleProfileSO ResolveTarget(VisualScaleProfileSO source)
        {
            string path = TargetPath(source);
            var existing = AssetDatabase.LoadAssetAtPath<VisualScaleProfileSO>(path);
            if (existing != null) { ValidateClone(source, existing); return existing; }
            if (File.Exists(path)) throw new InvalidOperationException("Target profile path exists with incompatible contents: " + path);
            if (!AssetDatabase.IsValidFolder(ProfileFolder)) AssetDatabase.CreateFolder("Assets/_Game/Data", "TownVisualScale");
            var clone = UnityEngine.Object.Instantiate(source);
            clone.name = Path.GetFileNameWithoutExtension(path);
            clone.ProfileId = "town_" + source.ProfileId;
            clone.DisplayName = source.DisplayName + " (Town keyart x2.25)";
            clone.VisualScale = source.VisualScale * Factor;
            clone.ColliderScale = 0;
            AssetDatabase.CreateAsset(clone, path);
            AssetDatabase.SaveAssetIfDirty(clone);
            ValidateClone(source, clone);
            return clone;
        }
        private static void ValidateClone(VisualScaleProfileSO source, VisualScaleProfileSO clone)
        {
            if (clone == source || clone.ProfileId != "town_" + source.ProfileId || clone.Category != source.Category ||
                !Mathf.Approximately(clone.VisualScale, source.VisualScale * Factor) || clone.ColliderScale != 0 ||
                clone.FootprintSize != source.FootprintSize || clone.InteractionRadius != source.InteractionRadius || clone.SelectionRadius != source.SelectionRadius ||
                clone.NameplateOffset != source.NameplateOffset || clone.HintOffset != source.HintOffset || clone.DamageNumberOffset != source.DamageNumberOffset || clone.ShadowScale != source.ShadowScale)
                throw new InvalidOperationException("Town clone does not match the reviewed source/compensation contract: " + AssetDatabase.GetAssetPath(clone));
        }

        private static ActorState Observe(Actor actor)
        {
            var root = actor.root.transform;
            var profile = actor.applicator.Profile;
            return new ActorState
            {
                id = actor.id, objectPath = PathOf(root, null), position = root.position, localScale = root.localScale,
                profilePath = AssetDatabase.GetAssetPath(profile), profileId = profile.ProfileId, category = profile.Category.ToString(),
                profileVisualScale = profile.VisualScale, profileColliderScale = profile.ColliderScale,
                shapes = actor.colliders.Select(c => Shape(c, root)).ToArray()
            };
        }
        private static PhysicalShape Shape(Collider2D collider, Transform root)
        {
            var result = new PhysicalShape
            {
                path = ColliderKey(collider, root), type = collider.GetType().Name, rigidbodyPath = PathOf(collider.attachedRigidbody.transform, root),
                enabled = collider.enabled, active = collider.gameObject.activeInHierarchy, trigger = collider.isTrigger, layer = collider.gameObject.layer,
                centerRelativeToActor = collider.transform.TransformPoint(collider.offset) - root.position,
                cornersRelativeToActor = Array.Empty<Vector2>()
            };
            if (collider is BoxCollider2D box)
            {
                var half = box.size * 0.5f;
                result.cornersRelativeToActor = new[] { new Vector2(-half.x, -half.y), new Vector2(half.x, -half.y), new Vector2(half.x, half.y), new Vector2(-half.x, half.y) }
                    .Select(p => (Vector2)(box.transform.TransformPoint(box.offset + p) - root.position)).ToArray();
            }
            else if (collider is CircleCollider2D circle) result.radius = circle.radius * Mathf.Abs(circle.transform.lossyScale.x);
            return result;
        }

        private static UndoActor SnapshotUndo(Actor actor)
        {
            var transforms = new[] { actor.root.transform }.Concat(actor.branches).ToArray();
            return new UndoActor
            {
                actor = actor, profile = actor.applicator.Profile, transforms = transforms,
                positions = transforms.Select(t => t.localPosition).ToArray(), scales = transforms.Select(t => t.localScale).ToArray(),
                offsets = actor.colliders.Select(c => c.offset).ToArray(),
                sizes = actor.colliders.Select(c => c is BoxCollider2D b ? b.size : Vector2.zero).ToArray(),
                radii = actor.colliders.Select(c => c is CircleCollider2D b ? b.radius : 0).ToArray()
            };
        }
        private static void Restore(UndoActor state)
        {
            state.actor.applicator.Configure(state.profile);
            for (int i = 0; i < state.transforms.Length; i++) { state.transforms[i].localPosition = state.positions[i]; state.transforms[i].localScale = state.scales[i]; }
            for (int i = 0; i < state.actor.colliders.Length; i++)
            {
                var c = state.actor.colliders[i]; c.offset = state.offsets[i];
                if (c is BoxCollider2D box) box.size = state.sizes[i];
                else if (c is CircleCollider2D circle) circle.radius = state.radii[i];
            }
        }
        private static string ColliderKey(Collider2D collider, Transform root) => PathOf(collider.transform, root) + ":" + collider.GetType().Name + ":" + Array.IndexOf(collider.GetComponents<Collider2D>(), collider);
        private static string PathOf(Transform value, Transform stop)
        {
            if (value == stop) return ".";
            string result = value.name;
            while (value.parent != null && value.parent != stop) { value = value.parent; result = value.name + "/" + result; }
            return result;
        }
        private static IEnumerable<T> Components<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true));
        private static void RequireTown(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || (scene.name != "TownScene" && !string.IsNullOrEmpty(scene.path)))
                throw new InvalidOperationException("Town actor pilot requires the generated/loaded Town scene, never Farm or another saved scene.");
        }
        private static string Hash(string path)
        {
            using (var sha = SHA256.Create()) using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        private static string OutputPath()
        {
            string path = Environment.GetEnvironmentVariable("CINDARS_TOWN_SCALE_OUTPUT");
            if (string.IsNullOrWhiteSpace(path)) path = "art/town-keyart-rework/evidence/actor-scale-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".json";
            path = Path.GetFullPath(path);
            string evidence = Path.GetFullPath("art/town-keyart-rework/evidence") + Path.DirectorySeparatorChar;
            if (!path.StartsWith(evidence, StringComparison.OrdinalIgnoreCase) || File.Exists(path)) throw new IOException("Scale evidence must be a new file inside the Town evidence directory: " + path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }
    }
}
