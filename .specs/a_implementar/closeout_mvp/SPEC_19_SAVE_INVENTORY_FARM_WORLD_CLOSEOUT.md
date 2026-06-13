# SPEC_19 — Save, inventory, farm e world activities closeout

> Spec ID: `spec_mvp_closeout_19_save_inventory_farm_world_closeout`  
> Ordem: 19  
> Status: A implementar  
> Depende de: SPEC_18  
> Bloqueia: SPEC_20-29  
> Tipo: Runtime/Save/Inventory/Farm/World

## /speckit.specify

### Objetivo

Fechar os escopos parciais de specs 02, 03, 04 e 05 sem reescrever sistemas já funcionais.

### Gaps conhecidos

Conforme `.specs/SPEC_EXECUTION_ORDER.md`:

- SPEC_02: infraestrutura de migration existe, mas escopo permanece parcial.
- SPEC_03: inventory tem slots/capacity/painel mínimo; Drop runtime e Use específico permanecem pendentes.
- SPEC_04: farm planting/irrigação existem, mas Play Mode/polimento pendem.
- SPEC_05: loot table, fishing timing e tree HP/regrowth existem; spawner dinâmico/cave fishing/farm scene spots pendem.

## /speckit.plan

### Arquivos a ler

```text
.specs/implementados/spec_save_002_schema_migration_v2.md
.specs/implementados/spec_inventory_002_slots_capacity_ui_final.md
.specs/implementados/spec_farm_004_irrigacao_solo_planting_ui.md
.specs/implementados/spec_world_002_activities_fishing_trees_pickups_loot.md
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/UI/Hotbar/**
Assets/_Game/Scripts/Editor/Validation/**
```

### Implementação esperada

1. Auditar estado real antes de editar.
2. Confirmar se ainda faltam Use/Drop runtime no inventory.
3. Fechar comportamento mínimo para item use/drop sem criar UI final.
4. Confirmar save/load round-trip de inventory, hotbar, farm plots, trees, pickups e world activities.
5. Corrigir gaps pequenos de farm planting/irrigation/fishing/tree regrowth, se existirem.
6. Criar validators ou estender existentes para detectar:
   - hotbar apontando para item inexistente;
   - pickup sem id persistente;
   - farm plot sem save id;
   - tree sem save id;
   - fishing spot sem interaction/collider.
7. Não alterar schema de save sem migration explícita.

## /speckit.tasks

- [ ] Relatório de auditoria do estado real.
- [ ] Inventory use/drop mínimo fechado ou bloqueio documentado.
- [ ] Farm/world activity Play Mode smoke validado.
- [ ] Save/load round-trip validado para inventory/farm/world.
- [ ] Validators atualizados.
- [ ] `docs/validation/spec_mvp_closeout_19_save_inventory_farm_world_closeout_execution_report.md` criado.
- [ ] Status em `docs/IMPLEMENTATION_STATUS.md` atualizado se specs 02-05 mudarem de parcial para completo.

## Regressão a evitar

- Não quebrar starter inventory.
- Não quebrar hotbar consistency check.
- Não quebrar pickups persistentes.
- Não quebrar tree colliders.
- Não quebrar farm planting/irrigation já existente.
- Não quebrar fishing spot/lake interaction.

## Critérios de aceite

- Save/load preserva inventory/hotbar/farm/world no Play Mode.
- Inventory drop/use mínimo existe ou é explicitamente movido para SPEC_28 se for UI-only.
- Validators sem erros críticos.
- Specs 02-05 podem ser promovidas para “completo MVP” ou mantidas parciais com gaps claramente remanescentes.
