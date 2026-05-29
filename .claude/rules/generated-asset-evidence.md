# Rule: Generated Asset Evidence

When a spec depends on generated Unity assets, closeout must include evidence.

## Required Evidence

- Menu or `-executeMethod` used.
- Log path.
- Exit code.
- Asset folders affected.
- Expected asset count.
- Actual asset count.
- Any divergence between requested menu name and actual editor method.

## Blocked Generation

If asset generation cannot run, document:

```text
Asset generation: BLOCKED
Reason: <Unity lock | license | timeout | sandbox | approval | compile error>
Command attempted: <command>
Residual risk: assets may be stale/missing until generated in Unity
```

## Prohibited

- Do not claim generated assets exist solely because the generator code exists.
- Do not silently accept mismatched roster/registry assets.
- Do not manually fake generated assets unless the spec explicitly permits it.
