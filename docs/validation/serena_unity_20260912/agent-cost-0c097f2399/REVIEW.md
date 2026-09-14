# End-to-end cost gate — INCONCLUSIVE / NOT APPROVED

No guarantee of cost non-regression is established. No configuration was changed.

Protocol: same captured Unity source, GPT-5.6 Sol, medium reasoning, independent fresh
CLI sessions. Two investigations: gameplay RNG implementations/construction and preparable
skill effect implementations/validation/commit flow. A separate read-only reviewer created
the source-based answer key before the arm results were inspected. Same 16-tool-call and
800-word requested limits; hard 600-second wall limit. No monetary rates were inferred.

The baseline's shell execution was rejected by environment policy before any source read.
Its final answer correctly disclosed that it could not investigate. CLI exit 0 therefore
does NOT mean benchmark success. No permission bypass or equivalent access retry was used
by the orchestrator after identifying this blocker.

| Arm | Status | Native usage available |
|---|---|---|
| Without Serena | INVALID: no code access | input 109279; cached input 85760; output 811; reasoning output 293 |
| With Serena | STOPPED after baseline invalidation, 6 MCP calls completed | unavailable; NOT zero |

Baseline uncached input is 23519 by subtraction. Reasoning output is reported separately,
not added to output a second time. Baseline wall time was 27.67 s; candidate was cancelled
at 119.98 s. These times are failed-experiment costs, not comparative productivity results.
The candidate's exact owned process tree was verified and terminated to avoid spending
more on an invalid pair. No inference about candidate quality, savings or regression is valid.
The experiment and independent review also consume resources; their full aggregate billing
is not available from these arm counters.

Initial preflight directory `agent-cost-fd031c8eb8` failed configuration parsing before any
model turn. The harness was corrected to use `--ignore-user-config` and configure only the
candidate MCP per invocation. Both arms were then launched together once; no favorable-run
selection or repeated sampling occurred. The global/project configurations were untouched.

Native counter source: `turn.completed.usage` in baseline.jsonl. See the
[official non-interactive documentation](https://learn.chatgpt.com/docs/non-interactive-mode).
Raw invocation, stderr, events and summary are preserved in this directory. Candidate
counters are absent because it did not complete a turn. Subscription quota weighting and
actual billed cost were not measured and cannot be reconstructed from output size alone.

Independent expected facts: two concrete gameplay RNGs (SeededGameplayRandomSource and
UnityGameplayRandomSource), 13 preparable executor classes and three non-preparable concrete
ISkillEffectExecutor classes. Preparation is Validate(context), not an assumed Prepare API;
validation happens before cast and again at commit. No benchmark arm was granted an accuracy
PASS without completing the evidence-backed answer.

Cost gate remains NOT APPROVED. A valid rerun needs an authorized functioning read-only
environment for both arms and complete usage, followed by quality review and repeated
matched runs. Even a passing sample would only support the observed workload, not guarantee
future cost universally. Until then, automatic adoption cannot be justified as cost-neutral;
turning off default startup is the conservative recommendation, pending user direction.
