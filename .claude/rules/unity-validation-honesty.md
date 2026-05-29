# Rule: Unity Validation Honesty

Do not overstate validation results.

## Required Distinctions

- `dotnet build` passing means C# fallback compile passed.
- Unity batchmode passing means Unity compile validation passed.
- Play Mode/manual flows passing means gameplay validation passed.

These are different results.

## Blocked Unity Validation

If Unity cannot run because of lock, license, sandbox, timeout, package manager, or approval limits, report:

```text
Unity validation: NOT RUN or BLOCKED
Reason: <specific blocker>
Command attempted: <command>
Residual risk: Unity compile/Play Mode not validated locally
```

## Error Triage

- Any `error CS` is a real compile problem unless proven stale.
- Package/test assembly "not valid" warnings without `error CS` are tooling noise unless they prevent the target validation from running.
- "Another Unity instance is running" is an editor lock, not a code failure.

Never convert a blocked validation into a pass.
