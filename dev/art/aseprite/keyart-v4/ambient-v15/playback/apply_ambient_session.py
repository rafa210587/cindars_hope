"""Root-only integration after releasing the Unity/Assets window.

Run from the repository root with --apply. No Unity execution or validation.
Promotes the adjacent helper and changes only the existing session's optional
CINDARS_FARM_AMBIENT=1 mode. Other capture modes keep their existing gates.
Use the existing capture entry point without -quit, with the ambient env set
and CINDARS_FARM_MOTION unset. Output remains capture-metadata.json plus PNGs
under ambient-playback/. Actual sample timestamps are suitable for HTML playback.
Requires the exact reviewed session hash; if it changed, review/rebase this patch.
"""
import argparse
import datetime
import hashlib
import json
from pathlib import Path

EXPECTED_SHA256 = '9d9f9cd6bdc27939f7e36246a8239da365dba940e3d1e73933b993fa90d36050'
RELATIVE = Path('Assets/_Game/Scripts/Editor/Dev/FarmPlayModeCaptureSession.cs')


def digest(data):
    return hashlib.sha256(data).hexdigest()


def build_patch(source):
    def replace(old, new):
        nonlocal source
        if source.count(old) != 1:
            raise RuntimeError('Expected exactly one anchor: ' + old[:110])
        source = source.replace(old, new, 1)

    replace('        private static FarmDoorAndWaterProbe motionProbe;',
            '        private static FarmDoorAndWaterProbe motionProbe;\n'
            '        private static FarmAmbientPlaybackCapture ambientProbe;')
    replace('            public FarmDoorAndWaterProbe.Evidence motion;',
            '            public FarmDoorAndWaterProbe.Evidence motion;\n'
            '            public bool ambientOnlyRequested, ambientStarted, ambientEvaluated;\n'
            '            public FarmAmbientPlaybackCapture.Evidence ambient;')
    replace('                motionRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_MOTION") == "1"',
            '                motionRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_MOTION") == "1",\n'
            '                ambientOnlyRequested = Environment.GetEnvironmentVariable("CINDARS_FARM_AMBIENT") == "1"')
    replace('            Persist();\n            Attach();\n            try',
            '''            if (session.ambientOnlyRequested)
            {
                // Ambient-only capture deliberately skips unrelated static/route/interaction gates.
                session.motionRequested = false;
                session.limitations = "Real native ambient playback; camera-only framing at gameplay zoom. No movement/input, interaction or route proof. Screen Space Overlay UI is absent from Camera.Render. Inspect ambient.method and timestamps.";
            }
            Persist();
            Attach();
            try''')
    replace('                session.stage = "capturing";',
            '                session.stage = session.ambientOnlyRequested ? "ambient" : "capturing";')
    replace('''            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                motionProbe?.Dispose();''',
            '''            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                DisposeAmbient();
                Persist();
                motionProbe?.Dispose();''')
    replace('(session.motionRequested ? 180 : TimeoutSeconds)',
            '((session.motionRequested || session.ambientOnlyRequested) ? 180 : TimeoutSeconds)')
    replace('(session.stage != "capturing" && session.stage != "interactions" && session.stage != "motion")',
            '(session.stage != "capturing" && session.stage != "interactions" && session.stage != "motion" && session.stage != "ambient")')
    replace('                if (session.stage == "motion")\n',
            '''                if (session.stage == "ambient")
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
''')
    replace('''        private static void Fail(string reason)
        {
            if (session.motionRequested)''',
            '''        private static void DisposeAmbient()
        {
            if (ambientProbe == null) return;
            session.ambient = ambientProbe.Result;
            ambientProbe.Dispose();
            ambientProbe = null;
        }

        private static void Fail(string reason)
        {
            if (session.ambientOnlyRequested && session.ambient != null)
            {
                session.ambient.status = "FAIL";
                session.ambient.reason = reason;
            }
            DisposeAmbient();
            if (session.motionRequested)''')
    replace('''                bool sortingValid = session.views.Count == ViewIds.Length && session.views.All(view =>''',
            '''                if (session.ambientOnlyRequested)
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
                bool sortingValid = session.views.Count == ViewIds.Length && session.views.All(view =>''')
    replace('                if (pass) Debug.Log("Farm gameplay capture: PASS (" + ViewIds.Length + "/" + ViewIds.Length + ")");',
            '''                if (pass) Debug.Log(session.ambientOnlyRequested ? "Farm ambient playback: PASS" :
                    "Farm gameplay capture: PASS (" + ViewIds.Length + "/" + ViewIds.Length + ")");''')
    return source


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true', help='Apply after root releases the Unity/Assets window.')
    args = parser.parse_args()
    if not args.apply:
        parser.error('No changes made. Root must explicitly pass --apply after coordination.')
    root = Path.cwd()
    session_path = root / RELATIVE
    helper_path = session_path.with_name('FarmAmbientPlaybackCapture.cs')
    candidate = Path(__file__).resolve().with_name('FarmAmbientPlaybackCapture.cs.txt')
    original = session_path.read_bytes()
    if digest(original) != EXPECTED_SHA256:
        raise RuntimeError('Session changed since review; abort without writing. Review/rebase required.')
    if helper_path.exists():
        raise RuntimeError('Helper already exists; refusing to overwrite concurrent work.')
    newline = '\r\n' if b'\r\n' in original else '\n'
    source = original.decode('utf-8-sig').replace('\r\n', '\n')
    patched = build_patch(source).replace('\n', newline).encode('utf-8')
    if original.startswith(b'\xef\xbb\xbf'):
        patched = b'\xef\xbb\xbf' + patched
    helper_text = candidate.read_text(encoding='utf-8-sig')
    # Remove the candidate-only manual integration header from promoted C#.
    helper_text = helper_text[helper_text.index('using System;'):]
    helper_bytes = helper_text.replace('\r\n', '\n').replace('\n', newline).encode('utf-8')
    stamp = datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%S%fZ')
    backup_dir = candidate.parent / ('integration-backup-' + stamp)
    backup_dir.mkdir()
    (backup_dir / session_path.name).write_bytes(original)
    (backup_dir / 'FarmPlayModeCaptureSession.patched.cs.txt').write_bytes(patched)
    manifest = {'session': str(session_path), 'helper': str(helper_path),
                'beforeSha256': digest(original), 'afterSha256': digest(patched),
                'helperSha256': digest(helper_bytes), 'candidateSha256': digest(candidate.read_bytes())}
    (backup_dir / 'hashes.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    # Recheck immediately before writes; do not overwrite changes made during preparation.
    if session_path.read_bytes() != original or helper_path.exists():
        raise RuntimeError('Targets changed during preparation; backups retained, no live write.')
    with helper_path.open('xb') as output:
        output.write(helper_bytes)
    session_path.write_bytes(patched)
    print('Applied only the helper and optional ambient session mode. No Unity/validation executed.')
    print('Backup and hashes: ' + str(backup_dir))


if __name__ == '__main__':
    main()
