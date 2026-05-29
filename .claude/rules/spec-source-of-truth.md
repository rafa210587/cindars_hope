# Rule: Spec Source Of Truth

The only active source for specs is:

```text
docs/specs/
```

## Prohibited

- Recreating root `specs/`.
- Recreating root `spec/`.
- Treating `docs_old/**` as an active spec source.

## Required

Use:

- `docs/specs/SPEC_EXECUTION_ORDER.md`
- target spec under `docs/specs/a_implementar/`
- implemented specs under `docs/specs/implementados/`
- refinements under `docs/refinements/`

When a spec is completed, update the relevant registry/status/log files according to the project execution protocol.
