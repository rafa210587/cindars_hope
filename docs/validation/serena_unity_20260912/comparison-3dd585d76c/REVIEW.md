# Serena versus rg — matched comparison

Result: equal answers on this narrow known-symbol workload; the current Serena integration
does not justify replacing targeted rg as the default. No configuration was changed.

Three questions, two repetitions, alternating engine order in the second round. Both engines
read the same captured Foundation/Gameplay/Runtime Compile sources. Shared source capture
took 13.55 s and is excluded from both engine times. Serena's own startup/copy/restore cost
is reported separately. No Unity gameplay validation or full-agent model run was performed.

| Metric, six queries per engine | rg | Serena with current proxy |
|---|---:|---:|
| Correct answers after review | 6/6 | 6/6 |
| Total query time | 0.258 s | 143.329 s |
| Median query time | 0.030 s | 23.968 s |
| Output content tokens, o200k_base proxy | 1916 | 1848 |
| Additional server startup | none | 21.181 s |

Serena saved 68 output-content tokens (3.55%) across six queries, with about 555 times the
query latency in this run. Its tool-list JSON also measured 1,895 proxy tokens, excluded
from the table: actual client schema injection, caching and billing differ. These are
reproducible token counts of output text using o200k_base, NOT measured billed/account tokens,
model reasoning tokens, monetary cost or proof of end-to-end agent savings. MCP metadata
and duplicated structured/text fields are excluded; only one content copy is counted.

Per question, per repetition: method body rg 126 / Serena 172 tokens; hash references
rg 690 / Serena 632; input references rg 142 / Serena 120. Baseline searches use two context
lines, known names/paths and only the same source scope; not whole-file dumps. Serena adds
symbol metadata and enclosing member names that were not required by these questions.

Independent review compared raw outputs: identical normalized method body; identical
reference file/line sets (five StableHash references: four invocations and one XML cref;
one Acquire invocation). LSP zero-based lines were converted to one-based lines. Both rounds
were checked; reference ordering varied without changing the sets or token counts.
No matching backend processes remained after the run.

Limits: two repetitions, small known-symbol sample, no overload/alias/rename investigation
or editing task. The latency includes the current proxy's integrity checks; it does not
measure bare Serena or isolate each internal cost. Results cannot rank all semantic workloads.
Keep rg as the normal route and reserve Serena for questions where semantic disambiguation
is actually needed. Further claims require a separate end-to-end agent/cost benchmark.

Machine results and exact commands: [results.json](results.json) and
`tools/serena/compare_navigation.py`. Raw outputs for both engines and repetitions are in
this directory. The results.json correctness field records its pre-review state; this
review supplies the independent correctness decision.
