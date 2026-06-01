# Refinements — Cindar's Hope

> **Refinements are decision origin documents, NOT executable specs.**  
> **Agents executing a spec must NOT read refinements by default.**

---

## What Is a Refinement?

A refinement is a document that:
- Captures a decision, design choice, or requirement analysis
- Is created BEFORE a spec
- Defines the "why" and the constraints for the work
- Is NOT directly executable

A refinement becomes a spec via: `accepted → spec created → spec executed`

---

## Refinement States

```
inbox → accepted → implemented → archived
                ↓
            superseded
```

| State | Meaning |
|-------|---------|
| `inbox` | Newly created; pending review |
| `accepted` | Approved; spec being created from it |
| `implemented` | Corresponding spec was implemented |
| `superseded` | Replaced by a newer refinement or spec |
| `archived` | Historical record; no longer active |

---

## Reading Policy

| When | Should Agent Read? |
|------|-------------------|
| Executing a spec | NO — spec must be self-contained |
| Spec is ambiguous | ONLY if spec explicitly cites the refinement |
| Creating a new spec | YES — use accepted refinement as input |
| Audit/reconciliation | YES — historical context |

---

## Folder Structure

```
docs/04_REFINEMENTS/
  README.md                (this file)
  REFINEMENT_TEMPLATE.md   (template for new refinements)
  inbox/                   (new, pending review)
  accepted/                (approved refinements; spec pending)
  implemented/             (spec was created and executed)
  superseded/              (replaced)
  archived/                (historical)
```

**Current location of existing refinements:** `docs/refinements/`

Migration to `docs/04_REFINEMENTS/` is planned for SPEC_DOCS_31. Do not move yet.

---

## Rules

1. Every refinement that results in implementation must point to the spec it produced
2. The spec is the single source of truth for execution
3. If spec and refinement conflict → **follow the spec**
4. Agents reading a refinement must NOT treat it as a spec
5. Refinements do NOT have tasks lists that agents should execute

---

*Created: 2026-06-01 (SPEC_DOCS_30)*
