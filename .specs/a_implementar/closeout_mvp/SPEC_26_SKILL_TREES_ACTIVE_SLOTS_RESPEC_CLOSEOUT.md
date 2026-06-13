# SPEC_26 — Skill trees, active slots e respec Anya closeout

> Spec ID: `spec_mvp_closeout_26_skill_trees_active_slots_respec_closeout`  
> Ordem: 26  
> Status: A implementar  
> Depende de: SPEC_25  
> Bloqueia: SPEC_28-29  
> Tipo: Skills/Progression/Save/UI mínima

## /speckit.specify

Fechar a SPEC 16 histórica, hoje implementada em código mas com Unity/Play Mode pendentes.

## /speckit.plan

Ler:

```text
.specs/implementados/spec_skill_trees_active_slots_respec_anya_runtime.md
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Player/Progression/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Data/Skills/**
```

Implementação esperada:

1. Auditar 5 árvores/55 nodes.
2. Validar skill point a cada 2 níveis.
3. Validar compra de nodes, preconditions, capstones e active slots.
4. Validar passives aplicados.
5. Validar respec na Fonte de Anya depois da SPEC_25.
6. Validar save/load v5.
7. Criar validators para skill node graph:
   - node sem id;
   - node duplicado;
   - prerequisite inexistente;
   - active skill sem action;
   - capstone inválido;
   - árvore sem root.

## /speckit.tasks

- [ ] Skill tree validator PASS.
- [ ] Purchase/respec Play Mode PASS.
- [ ] Active slots PASS.
- [ ] Save/load skills PASS.
- [ ] Relatório `docs/validation/spec_mvp_closeout_26_skill_trees_active_slots_respec_closeout_execution_report.md`.

## Regressão a evitar

- Não quebrar PlayerProgressionManager.
- Não quebrar Anya flow da SPEC_25.
- Não quebrar UI final futura da SPEC_28.
- Não alterar save schema sem migration.

## Critérios de aceite

- Skill runtime validado em Unity/Play Mode.
- SPEC 16 histórica pode ser promovida para completo MVP.
