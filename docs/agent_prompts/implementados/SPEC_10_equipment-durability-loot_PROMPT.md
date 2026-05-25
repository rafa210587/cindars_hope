# Prompt para Claude Code/Codex — Cindar's Hope

**Status:** ✅ **IMPLEMENTADO** em 2026-05-25

Esta SPEC foi completamente implementada. Consulte:
- `docs/specs/implementados/spec_equipment_durability_environment_loot_runtime.md` — Status técnico
- `PROJECT_LOG.md` — Histórico de implementação

---

# SPEC 10 — Equipment, Durability, Environment e Loot

## Branch sugerida

```bash
git checkout dev
git pull
git checkout -b feature/spec-10-equipment-durability-loot
```

Se a branch já existir, reutilize-a. Antes de alterar, rode `git status` e registre se o working tree não estiver limpo.

## Spec alvo

```text
docs/specs/implementados/spec_equipment_durability_environment_loot_runtime.md
```

## Dependências diretas

Depende de 07 e 09. Bloqueia 11, 12, 13, 14 e 17.

## Objetivo desta execução

Fechar equipment real: slots, seleção por inventory, durabilidade, repair kit, resistências ambientais e integração com loot/drop.

## Status de implementação

✅ **Completado em 2026-05-24/2026-05-25**

- EquipmentManager, DurabilityTracker, EquipmentDataSO implementados
- Equipment slots posicionais (LeftHand, RightHand, Head, Chest, Legs, Boots, Ring1, Ring2, Accessory)
- RepairKit MVP funcional
- Stats derivados com AttackSpeed
- Environmental resistance MVP (Toxic, Cold, Heat)
- LootTableSO com equipment instance generation
- Save/load completo
- Equipment HUD minimal
- Play Mode testing: deferred para humano validar
- UI final: deferred para SPEC 17

## Próximos passos

1. Play Mode testing manual (humano)
2. UI consolidada em SPEC 17
3. SPEC 12 (Player Combat) pode prosseguir

## Commit

```bash
git add <arquivos>
git commit -m "feat: finalizar spec 10 - equipment durability loot"
```

Não faça push.
