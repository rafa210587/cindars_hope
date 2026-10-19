# Unity Validation — Testing Quality Gate Addendum

Unity compile validation is mandatory for runtime/Unity changes, but it is not enough to prove behavior.

## Distinctions

```text
dotnet build:
  fallback C# compile signal.

Unity batchmode compile:
  authoritative Unity compile signal.

EditMode tests:
  deterministic logic and pure service behavior.

PlayMode tests / human scenario:
  scene, UI, input, prefab and gameplay-flow behavior.
```

## Rule

When a spec changes runtime code, use `.claude/rules/testing-quality-gate.md` to decide whether the task needs:

```text
EditMode automated tests;
PlayMode automated tests;
human Play Mode scenario;
explicit justification for no automated tests;
residual risk entry.
```

## Reporting

The validation report must not say `validated` unless the relevant level actually ran or was explicitly marked NOT RUN with reason.

Compile success alone is not enough for `ACCEPTED` runtime/gameplay closeout.
