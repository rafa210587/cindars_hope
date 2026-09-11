---
name: unity-performance-profiling
description: Measure Unity frame time, allocations, rendering and memory for a reproducible scenario. Use for reported hitches, performance budgets or before/after optimization evidence; distinguish static audit from profiling.
---

# Skill: Unity performance profiling

Choose metrics from the observed symptom and target platform. Static patterns identify hypotheses;
they do not establish a bottleneck, frame budget violation or measured improvement.

## Procedure

1. Identify the scenario and current build/scene inputs. Record hardware, Editor/Player version,
   resolution, quality, VSync/frame cap, focus, profiler instrumentation and warmup/sample interval.
   Reuse the same save/seed and workload where relevant; do not introduce artificial RNG requirements.
2. Prefer a Development Player on target hardware for performance conclusions. Editor measurements
   are useful diagnostics but include Editor overhead; disclose this limit. Avoid comparing a
   deeply instrumented run with an uninstrumented baseline.
3. Capture only relevant signals: main/render thread and GPU time, allocation samples/GC spikes,
   UI rebuilds, physics, draw/SetPass counts, texture or total memory. Missing counters are unavailable,
   not zero. Separate waits/VSync/unfocused idle from actual CPU work.
4. Retain profiler artifacts or timestamped samples. For hitch analysis record sample count,
   distribution/tail and worst event with its context; averages alone can hide intermittent stalls.
5. Connect the measured marker to the smallest owning path. Check frequency and workload before
   recommending pooling, caching, atlases or layout changes. Do not change pixel-art compression,
   packages, save contracts or rendering architecture to improve a metric outside authorized scope.
6. If implementing an authorized optimization, rerun equivalent conditions and check affected
   gameplay/visual behavior. Performance gains do not prove unchanged behavior.

## Routing and output

- Live tooling: [unity-mcp-operations](../unity-mcp-operations/SKILL.md), only when available.
- Regression gates: [unity-validation](../unity-validation/SKILL.md).
- A read-only performance-auditor returns findings; the implementation owner makes changes.

Deliver scenario, environment, artifact paths, measured values/units, hypothesis versus confirmed
cost, comparison limitations and next action. Without a capture, report a static audit and mark
profiling NOT RUN. Do not invent a universal FPS, allocation or draw-call threshold.

For instrumentation choices, consult the [Unity profiling manual](https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-profiling-applications.html)
and the installed version's API, not provider-specific counter names from memory.
