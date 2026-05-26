# Prompt para Claude Code/Codex — Cindar's Hope

> **Status: IMPLEMENTADO** — 2026-05-25
> Execução: SPEC 15 código completo. Validação Unity cache pendente.

**Use o fluxo oficial:**

```text
/implement-spec spec_15_cave-entry-death-anya-corpse
```

Isso delegará ao comando `/implement-spec` que orquestra validações, regressão e closeout automático.

**Consulte:**
- `.claude/commands/implement-spec.md` — Fluxo completo
- `.claude/skills/spec-execution/SKILL.md` — Pattern de execução

**Regras:** Implementar somente SPEC 15, não ampliar escopo, validação humana no final.

---

## SPEC 15 — Cave Entry, Death, Anya e Corpse Recovery

Finalizar entrada da caverna, loadout, morte, respawn, Fonte de Anya e corpse recovery persistente.

Consulte `docs/specs/implementados/spec_cave_entry_death_anya_corpse_recovery.md` para detalhes completos.

---

## Entrega 2026-05-25

**16 erros de compilação corrigidos:**
- IInteractable contract: AnyaFountainInteractable, CorpseInteractable
- ModalBase abstract class criado
- ModalManager.OpenModal<T>() implementado
- CaveRunManager API alinhado (CaveRunSeed, CurrentCaveLevel)
- PlayerProgressionManager API alinhado (Level, CurrentXp, ResetCurrentLevelXp)
- PlayerProgressionEvents criados (XpChanged, LevelChanged)
- InventoryManager.GetAllItems() e EquipmentManager.GetAllEquippedItems() adicionados
- EquipmentManager.UnequipAll() adicionado
- ActiveSkillSlots: input bloqueado quando modal ativo
- FindObjectOfType removido de todos os interactables

**Evidência:** `docs/validation/SPEC15_FINALIZATION_SPEC16_PHASE0_VALIDATION_20260525.md`
