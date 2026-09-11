# HYPOTHETICAL example — capacity calculation

Teaching scenario; neither describes the project's current state nor proposes a new system. A and B are fictional roles, not classes to create.

- Contract: supposedly authorized refactoring of a local calculation; exact files must still be confirmed before execution.
- Problem: facade A mixes free-space calculation with notification dispatch.
- State: A retains ownership of the collection; the extraction does not receive A as a service locator.
- Consumer: A queries capacity before accepting an operation.
- Reuse: first check for an equivalent existing pure operation B; in this scenario, reuse it.
- Before → after: only the calculation moves to B; public API and event publication stay in A.
- Smaller alternative: a private method suffices if no other concrete reason for B exists.

| Observable case | Input/state | Expected result |
|---|---|---|
| Space available | Capacity 5, occupancy 3 | Returns 2; collection unchanged |
| Exact limit | Capacity 5, occupancy 5 | Returns 0; collection unchanged |
| Operation rejected | Request exceeds free space | Same rejection reason; no additional event |

Risk: changing calculation/notification order may publish an incorrect acceptance. Characterize rejection and effects before extraction, reusing existing tests when they cover the contract.

Execution and validation: NOT RUN because this is an example. No log, fixture or class is established here. Actual gate selection depends on confirmed files and consumers.
