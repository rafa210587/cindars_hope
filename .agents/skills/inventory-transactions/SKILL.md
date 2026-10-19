---
name: inventory-transactions
description: Change inventory stack, capacity, split, move, merge or transfer behavior. Use when item mutations must remain atomic, event-consistent and save-safe.
---

# Skill: Inventory transactions

Reuse the current inventory aggregate and slot operations. Validate the whole transaction before
mutation so failure cannot lose, duplicate or partially apply items.

## Essential workflow
1. Read the active spec and inspect current inventory operations and consumer interfaces. Do not
   rely on historical capacity, method names or schema fields without confirming current code.
2. Define preconditions, mutation order, rollback/compensation and the exact success/failure
   event surface. Expected gameplay refusal returns the established result contract.
3. Read [transaction contracts](references/transaction-contracts.md) for stack/capacity rules,
   composite transfers and adapters. Load only the relevant section.
4. If persistence or compatibility is affected, read
   [save, validation and regressions](references/save-validation-and-regressions.md).
5. Test conservation and side effects before/after failures, not only the returned boolean.
   Preserve stable item IDs and simple save DTOs.

Use `save-load-pattern` for schema/migration, `ui-modal-stack` for inventory UI lifecycle,
and `data-catalog-authoring` for bulk item content. Deliver operation semantics, affected
consumer seams, event/save impact, tests and residual integration risks.
