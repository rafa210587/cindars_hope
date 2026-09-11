using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CindarsHope.Camera;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Dev
{
    /// <summary>Observes real native Animator playback; never seeks, samples or drives an Animator.</summary>
    internal sealed class FarmAmbientPlaybackCapture : IDisposable
    {
        [Serializable] internal sealed class Sample
        {
            public string source, sprite, path;
            public float time, fixedTime, realtime, normalizedTime;
            public int frame, cycle;
            public RectInt cropPixels;
        }
        [Serializable] internal sealed class Jump
        {
            public int cycle;
            public float observedStart, observedEnd;
            public List<int> frames = new List<int>();
        }
        [Serializable] internal sealed class Evidence
        {
            public string status = "RUNNING", reason;
            public string method = "41 seconds of real Play Mode. Native Animators run normally; no Animator.Update/Play, animation sampling, state seek, frame/alpha writes or time-scale changes. Camera keeps gameplay orthographic zoom/aspect; PNGs are unscaled pixel crops of its render. Fountain/cascade receive >=1.4s continuous observation each. Fish jump windows receive ~10fps and idle windows sparse captures. Use actual timestamps for playback. This is camera-only animation evidence, not movement/input, overlay UI or human acceptance.";
            public float startedTime, endedTime, startedRealtime, endedRealtime, cameraAspect, orthographicSize;
            public float fishPeriod, fishActiveStart, observedJumpSpacing;
            public int renderWidth, renderHeight;
            public int fountainDistinctFrames, cascadeDistinctFrames, fishIdleSamples;
            public List<Sample> samples = new List<Sample>();
            public List<Jump> jumps = new List<Jump>();
        }
        private sealed class Subject
        {
            public string id;
            public SpriteRenderer renderer;
            public Animator animator;
            public Sprite[] sprites;
            public float period, windowStart = -1f, observedDuration;
            public Bounds bounds;
            public readonly HashSet<int> seen = new HashSet<int>();
        }
        private readonly UnityEngine.Camera camera;
        private readonly CameraFollow2D follow;
        private readonly bool originalFollow;
        private readonly Vector3 originalCamera;
        private readonly Subject[] subjects;
        private readonly string output;
        private readonly float started, startedRealtime;
        private float lastCapture = -100f, lastIdleCapture = -100f;
        private int currentSubject = -1;
        private bool disposed;
        internal Evidence Result { get; } = new Evidence();

        internal FarmAmbientPlaybackCapture(Scene scene, UnityEngine.Camera camera, string outputDirectory)
        {
            Require(Application.isPlaying && scene.IsValid() && scene.isLoaded, "Ambient capture requires the already isolated Farm Play session.");
            this.camera = camera;
            Require(camera != null && camera.orthographic, "Expected the live orthographic gameplay camera.");
            follow = camera.GetComponent<CameraFollow2D>();
            Require(follow != null, "Expected the existing CameraFollow2D.");
            originalFollow = follow.enabled;
            originalCamera = camera.transform.position;
            var roots = scene.GetRootGameObjects();
            var fountain = roots.Single(r => r.name == "FonteAnya").transform.Find("Visual").GetComponent<SpriteRenderer>();
            var cascade = roots.SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true))
                .Single(s => s.name == "Visual_RiverConfluenceCascade");
            var fish = roots.Single(r => r.name == "FarmAmbientVisuals").transform.Find("Visual_FishJump").GetComponent<SpriteRenderer>();
            subjects = new[] { Resolve("fountain", fountain, 5, 0.7f), Resolve("cascade", cascade, 5, 0.7f), Resolve("fish", fish, 8, 20f) };
            var fishClip = subjects[2].animator.runtimeAnimatorController.animationClips.Single();
            var spriteBinding = AnimationUtility.GetObjectReferenceCurveBindings(fishClip).Single(b => b.propertyName == "m_Sprite");
            Result.fishActiveStart = AnimationUtility.GetObjectReferenceCurve(fishClip, spriteBinding).First(k => k.value != null).time;
            Result.fishPeriod = 20f;
            Result.cameraAspect = camera.aspect;
            Result.orthographicSize = camera.orthographicSize;
            Result.renderWidth = camera.pixelWidth; Result.renderHeight = camera.pixelHeight;
            Require(Result.renderWidth > 0 && Result.renderHeight > 0, "Gameplay camera has no render dimensions.");
            output = Path.Combine(outputDirectory, "ambient-playback");
            Directory.CreateDirectory(output);
            started = Time.time; startedRealtime = Time.realtimeSinceStartup;
            Result.startedTime = started; Result.startedRealtime = startedRealtime;
            // All preconditions precede the sole scene change, which Dispose reverses.
            follow.enabled = false;
        }

        internal bool Tick()
        {
            if (disposed) return true;
            try
            {
                Require(Mathf.Abs(Time.timeScale - 1f) < 0.0001f, "Ambient observation requires unchanged normal time scale.");
                Require(Time.realtimeSinceStartup - startedRealtime < 60f, "Ambient playback failed to advance41 game seconds within60 real seconds.");
                Require(Mathf.Abs(camera.orthographicSize - Result.orthographicSize) < 0.0001f &&
                    Mathf.Abs(camera.aspect - Result.cameraAspect) < 0.0001f, "Gameplay camera zoom/aspect changed.");
                var fish = subjects[2];
                var fishState = fish.animator.GetCurrentAnimatorStateInfo(0);
                var fishPhase = Mathf.Repeat(fishState.normalizedTime, 1f) * fish.period;
                // Prioritize a little lead-in and the end/wrap of each observed native jump.
                var fishWindow = fishPhase >= Result.fishActiveStart - 0.2f || fishPhase < 0.2f;
                var selected = fishWindow ? 2 : subjects[0].observedDuration < 1.4f ? 0
                    : subjects[1].observedDuration < 1.4f ? 1 : 2;
                if (selected != currentSubject)
                {
                    if (currentSubject >= 0 && currentSubject < 2 && subjects[currentSubject].observedDuration < 1.4f)
                    { subjects[currentSubject].windowStart = -1f; subjects[currentSubject].observedDuration = 0f; subjects[currentSubject].seen.Clear(); }
                    currentSubject = selected;
                    var center = subjects[selected].bounds.center;
                    camera.transform.position = new Vector3(center.x, center.y, originalCamera.z);
                }
                var idle = selected == 2 && !fishWindow;
                if (Time.time - lastCapture >= 0.09f && (!idle || Time.time - lastIdleCapture >= 5f))
                {
                    var sample = Capture(subjects[selected]);
                    lastCapture = sample.time;
                    if (idle)
                    {
                        lastIdleCapture = sample.time;
                        Result.fishIdleSamples++;
                        if (sample.sprite != "null") Result.reason = "Fish displayed a sprite during the native waiting interval.";
                    }
                    else if (selected < 2)
                    {
                        var subject = subjects[selected];
                        if (subject.windowStart < 0f) subject.windowStart = sample.time;
                        subject.observedDuration = sample.time - subject.windowStart;
                    }
                    else TrackJump(sample);
                }
                if (Time.time - started < 41f) return false;
                Finish(); Dispose(); return true;
            }
            catch (Exception error)
            {
                Result.status = "FAIL"; Result.reason = error.ToString();
                Dispose(); throw;
            }
        }

        private void TrackJump(Sample sample)
        {
            // An initial partial jump is intentionally excluded. Each accepted jump starts with
            // observed source frame0 and reaches transparent source frame7 in the same native cycle.
            var jump = Result.jumps.SingleOrDefault(j => j.cycle == sample.cycle);
            if (jump == null && sample.frame == 0)
            {
                jump = new Jump { cycle = sample.cycle, observedStart = sample.time };
                Result.jumps.Add(jump);
            }
            if (jump == null || sample.frame < 0) return;
            if (!jump.frames.Contains(sample.frame)) jump.frames.Add(sample.frame);
            if (sample.frame == 7) jump.observedEnd = sample.time;
        }
        private void Finish()
        {
            Result.endedTime = Time.time; Result.endedRealtime = Time.realtimeSinceStartup;
            Result.fountainDistinctFrames = subjects[0].seen.Count;
            Result.cascadeDistinctFrames = subjects[1].seen.Count;
            var complete = Result.jumps.Where(j => j.observedEnd > j.observedStart && j.frames.Count == 8).ToArray();
            if (complete.Length >= 2) Result.observedJumpSpacing = complete[1].observedStart - complete[0].observedStart;
            var pass = string.IsNullOrEmpty(Result.reason) && subjects.Take(2).All(s => s.observedDuration >= 1.4f && s.seen.Count == 5) &&
                complete.Length >= 2 && Mathf.Abs(Result.observedJumpSpacing - 20f) < 0.25f && Result.fishIdleSamples >= 2;
            Result.status = pass ? "PASS" : "FAIL";
            if (!pass && string.IsNullOrEmpty(Result.reason))
                Result.reason = "Expected5poses per fountain/cascade, two complete8-pose fish jumps20s apart, and null-sprite idle observations. Inspect actual samples; no missing frame was synthesized.";
        }
        private Subject Resolve(string id, SpriteRenderer renderer, int expectedFrames, float period)
        {
            Require(renderer != null, "Missing ambient renderer: " + id);
            var animator = renderer.GetComponent<Animator>();
            Require(animator != null && animator.enabled && animator.runtimeAnimatorController != null &&
                animator.cullingMode == AnimatorCullingMode.AlwaysAnimate && Mathf.Abs(animator.speed - 1f) < 0.0001f,
                "Expected active uncullable native Animator at speed1: " + id);
            var clip = animator.runtimeAnimatorController.animationClips.Single();
            Require(Mathf.Abs(clip.length - period) < 0.001f, "Native clip period differs: " + id);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(clip)).OfType<Sprite>()
                .OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
            Require(sprites.Length == expectedFrames, "Unexpected authored sprite subassets: " + id);
            var local = sprites[0].bounds;
            var bounds = new Bounds(renderer.transform.TransformPoint(local.center), Vector3.zero);
            foreach (var x in new[] { local.min.x, local.max.x })
            foreach (var y in new[] { local.min.y, local.max.y }) bounds.Encapsulate(renderer.transform.TransformPoint(new Vector3(x, y, 0f)));
            return new Subject { id = id, renderer = renderer, animator = animator, sprites = sprites, period = period, bounds = bounds };
        }
        private Sample Capture(Subject subject)
        {
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var target = new RenderTexture(Result.renderWidth, Result.renderHeight, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                camera.targetTexture = target;
                camera.Render(); // Observe real native state after rendering; never manually evaluate animation.
                var state = subject.animator.GetCurrentAnimatorStateInfo(0);
                var sprite = subject.renderer.sprite;
                var a = camera.WorldToViewportPoint(subject.bounds.min);
                var b = camera.WorldToViewportPoint(subject.bounds.max);
                var x0 = Mathf.Clamp(Mathf.FloorToInt(a.x * Result.renderWidth) - 16, 0, Result.renderWidth - 1);
                var y0 = Mathf.Clamp(Mathf.FloorToInt(a.y * Result.renderHeight) - 16, 0, Result.renderHeight - 1);
                var x1 = Mathf.Clamp(Mathf.CeilToInt(b.x * Result.renderWidth) + 16, x0 + 1, Result.renderWidth);
                var y1 = Mathf.Clamp(Mathf.CeilToInt(b.y * Result.renderHeight) + 16, y0 + 1, Result.renderHeight);
                var crop = new RectInt(x0, y0, x1 - x0, y1 - y0);
                var sample = new Sample { source = subject.id, sprite = sprite != null ? sprite.name : "null",
                    frame = Array.IndexOf(subject.sprites, sprite), time = Time.time, fixedTime = Time.fixedTime,
                    realtime = Time.realtimeSinceStartup, normalizedTime = state.normalizedTime,
                    cycle = Mathf.FloorToInt(state.normalizedTime), cropPixels = crop,
                    path = Path.Combine(output, subject.id + "_" + Result.samples.Count.ToString("D3") + ".png") };
                RenderTexture.active = target;
                image = new Texture2D(crop.width, crop.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(crop.x, crop.y, crop.width, crop.height), 0, 0); image.Apply();
                File.WriteAllBytes(sample.path, image.EncodeToPNG());
                Result.samples.Add(sample);
                if (sample.frame >= 0) subject.seen.Add(sample.frame);
                return sample;
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
            if (disposed) return;
            disposed = true;
            Result.endedTime = Time.time; Result.endedRealtime = Time.realtimeSinceStartup;
            if (camera != null) camera.transform.position = originalCamera;
            if (follow != null) follow.enabled = originalFollow;
        }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
