---
name: unity-mcp-operations
description: Operate an available Unity MCP connection with explicit Editor identity, mutation ownership and reload recovery. Use for live scene inspection or authorized Editor commands; not for assuming or installing a provider.
---

# Skill: Unity MCP operations

Use the actual tool schema and target Editor state. Tool names and transport differ by provider;
a configured server or generated skill does not prove an active connection.

## Before operating

1. Discover available tools and record provider/version when exposed, project absolute path,
   Editor version, instance identity, loaded scenes, dirty state and Edit/Play Mode.
   Availability is per agent: a parent connection may be filtered by a subagent's tool allowlist.
   If filtered, hand the exact operation, inputs and expected artifact to the enabled Editor owner
   and inspect the returned evidence. Do not request human Inspector work for that limitation alone.
2. Confirm this is the requested project. If identity cannot be resolved, do not mutate it.
3. One owner controls the Editor. Other agents may inspect files or review captured evidence;
   they must not run competing scene changes, tests, imports or Play Mode transitions.
4. Classify the requested operation: read, transient execution, or persistent mutation. A method
   called "validate" or "capture" may regenerate assets; inspect its implementation first.

## Execution and recovery

- Read only the hierarchy/components/log window needed for the task; avoid whole-project dumps.
- Use existing generators/repair commands for persistent results. Keep the owning generator
  consistent with any authorized Inspector change; preserve GUIDs, IDs and unrelated dirty state.
- Do not save all scenes, clear console history, close the user's Editor or start a competing
  instance as a generic recovery step. Record relevant logs before any authorized clearing.
- Before dependent operations, wait for compile/import/reload completion using observed state,
  a bounded deadline and backoff. Preserve actionable failures instead of reconnecting forever.
- After reload, reacquire instance/object handles and inspect fresh console output. Do not
  reuse stale instance IDs or assume a fixed reload duration.
- If a mutating call times out, first inspect whether it applied. Retry only when absence of the
  effect or operation idempotence is established. An uncertain result is not a failed mutation.
- Running tests/Play Mode changes transient state: preserve user work and use isolated test save
  data when persistence is involved. Do not treat scene rollback as rollback of disk save files.

## Evidence and fallback

Use [unity-validation](../unity-validation/SKILL.md) for gates and fresh logs/XML/captures.
Map each claim to the tool result and actual artifact; successful RPC or console silence is
insufficient. Camera rendering does not prove overlay Canvas or real input behavior.
If tools are absent, use existing authorized runners when available. Mark only the unavailable
operation NOT RUN; do not fabricate tool calls or request infrastructure for a read-only task.

Only when selecting/upgrading a provider, consult
[external capability adoption](../harness-authoring/references/external-capabilities.md).
Report identity, operation, changed paths, evidence and remaining uncertainty.
