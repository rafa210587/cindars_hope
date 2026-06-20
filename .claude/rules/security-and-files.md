# Rule: Security & File Safety

Complements [no-unsafe-git.md](./no-unsafe-git.md) (destructive git) and [unity-assets.md](./unity-assets.md) (protected Unity YAML). This rule covers secrets and sensitive non-code files.

## No secrets in the repo

- Never write secrets into code, version-controlled config, or logs: API keys, tokens, private keys, passwords, connection strings with credentials.
- Use placeholders/fictional values in examples; load real values from an untracked config or environment variable.
- Enforced mechanically by hook `guard-secrets.ps1` (PreToolUse Edit/Write), which blocks content matching common secret formats.

## Sensitive files — do not modify without an explicit request

`.env`, secret/credential files, keystores, certificates, signing keys, and **real user save files**. Reading for audit is fine; editing requires the task to name the file explicitly.

> Note: real player saves are runtime data, not the save *schema*. Changing save DTOs/migrations is normal spec work (skill `save-load-pattern`); overwriting an actual user's save file on disk is not.

## Build / deploy / tooling scripts

Before editing anything under `tools/`, build scripts, or CI/deploy config, read the whole file and explain the impact first. These scripts run validation gates and asset pipelines — a silent change can mask a broken gate (rule `validation-truth`).

## General

- Prefer reversible changes and small diffs.
- No broad cleanup command (`Remove-Item -Recurse`, `git clean`, mass delete) to fix a local problem without stating exactly what will be removed (see rule `no-unsafe-git`).

## Enforcement

- Hook `guard-secrets.ps1` (PreToolUse) blocks secret-shaped content.
- `permissions.ask` (settings.json) gates Packages/, ProjectSettings/, and Unity asset YAML.
- Reviewed by skill `non-regression-review` before closeout.
