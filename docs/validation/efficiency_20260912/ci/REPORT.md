# CI preflight pilot — 2026-09-12

## Result

The isolated GitHub Actions pilot was published and executed twice. The first
run exposed a checkout defect caused by a tracked gitlink without a `.gitmodules`
URL. The second run fetched only `ProjectVersion.txt` through the public GitHub
contents API at the event SHA, then verified the expected Unity version and the
remote Unity container manifest before correctly reporting the absent activation
mechanism.

| Item | Status | Evidence |
|---|---|---|
| Isolated branch push | PASS | `codex/efficiency-ci-pilot`, commit `72ab0dceddbbd99bd44183c84439a87030f0c039` |
| Workflow scope | PASS | One job, push trigger limited to that branch, `contents: read`, cancellation concurrency, 10-minute timeout |
| Runner allocation | PASS | [Run 34724853766](https://github.com/rafa210587/cindars_hope/actions/runs/34724853766) started `ubuntu-latest`; job [103637093576](https://github.com/rafa210587/cindars_hope/actions/runs/34724853766/job/103637093576) started and completed |
| Project version on runner | PASS | Second run fetched the metadata at commit `8d6f0d2c707e9e400f0d912e6991f64272fb23b5` and matched `6000.5.7f1` |
| Unity image manifest on runner | PASS | Second run completed `docker manifest inspect unityci/editor:ubuntu-6000.5.7f1-base-3.2.2`; no image pull was attempted |
| Activation gate | BLOCKED | `UNITY_LICENSE` and `UNITY_LICENSE_SERVER` were both absent; values were neither read nor emitted |
| Unity EditMode | NOT RUN | No activation route was approved and this pilot contains no Unity test job |

## Published workflow

`/.github/workflows/efficiency-pilot.yml` uses unauthenticated `curl` against
the public GitHub contents API, scoped to
`ProjectSettings/ProjectVersion.txt?ref=${GITHUB_SHA}`. It expects Unity
`6000.5.7f1`, then runs a manifest-only check of
`unityci/editor:ubuntu-6000.5.7f1-base-3.2.2`. The tag was confirmed before
publication through the Docker Hub repository metadata endpoint for
`unityci/editor`; no image was pulled. No GameCI action, Unity activation setup,
license request, test job, PR, merge or default-branch workflow was created.

## First-run log facts

First-run status: **failure**. The checkout fetched the intended commit, set the sparse
checkout and checked out the branch. Its cleanup then failed with:

```text
fatal: No url found for submodule path '.claude/worktrees/confident-fermi-2e43ee' in .gitmodules
```

The following two preflight steps were consequently skipped: `Verify Unity
project version` and `Verify Unity container manifest without pulling`. The
`if: always()` activation step still ran and failed explicitly with the intended
message that neither supported activation variable was configured. This is
separate from the repository checkout defect and from image availability: the
pilot could not claim either remote project-version or remote image-manifest
success.

Second-run status: **failure by design at the activation gate**. Run
[34724928703](https://github.com/rafa210587/cindars_hope/actions/runs/34724928703)
for commit `8d6f0d2c707e9e400f0d912e6991f64272fb23b5` completed its metadata fetch,
Unity version check and container-manifest check successfully. Its job
[103637290615](https://github.com/rafa210587/cindars_hope/actions/runs/34724928703/job/103637290615)
then failed explicitly because both supported activation variables were empty.
This verifies runner allocation, public metadata access and remote manifest
availability without pulling the image or attempting Unity activation.

Repository evidence at principal HEAD `c5a1a61ac21fd814a337151e7794f68ac89733c1`:
`160000 commit 6d19ea2cf1709774875596d45441148609a203de .claude/worktrees/confident-fermi-2e43ee`;
`.gitmodules` is absent. Recommendation: repair or remove that tracked gitlink in
a separate repository-hygiene change. It is not part of this CI pilot; the pilot
no longer traverses it.

## Validation status

```text
Unity validation: NOT RUN
Reason: the human declined activation and no Unity test job was created.
Command attempted: GitHub Actions run 34724928703, preflight job 103637290615.
Residual risk: Unity compile and EditMode remain unvalidated in GitHub Actions;
the image manifest exists remotely, but the image was not pulled or used to run
Unity.
```

## Worktree incident record

The first external worktree attempt used a full checkout at
`D:\Projetos\Cindars_Hope\cindars_hope_ci_pilot`. It remained in Git checkout
past the terminal timeout after reporting 18,901 files. Its associated commands
were PID 69876 (`git worktree add ...`) and child PID 71500. While it was locked
as `initializing`, `git worktree remove --force --force` was attempted; Git
reported that the path was not empty and the metadata directory could not be
deleted. No process was killed. Both processes later exited; the principal
worktree remained on `dev` at `c5a1a61ac21fd814a337151e7794f68ac89733c1`.

The residual first path and the later no-checkout worktree at
`D:\Projetos\Cindars_Hope\cindars_hope_ci_pilot_index_20260912` are intentionally
retained. Neither was removed during closeout. The latter holds the isolated
branch/index used for commit `72ab0dce`; it did not materialize the project tree.
