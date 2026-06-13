# SPEC_20 — Equipment, durability, environment e loot closeout

> Spec ID: `spec_mvp_closeout_20_equipment_durability_environment_loot_closeout`  
> Ordem: 20  
> Status: A implementar  
> Depende de: SPEC_19  
> Bloqueia: SPEC_21-29  
> Tipo: Runtime/Data/Save/Validation

## /speckit.specify

Fechar a SPEC 10 histórica, hoje marcada como parcial, garantindo que equipment, durability, environment zones e loot funcionem como MVP sem quebrar inventory, combat ou save.

## /speckit.plan

Ler:

```text
.specs/implementados/spec_equipment_durability_environment_loot_runtime.md
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Data/Items/**
Assets/_Game/Data/Combat/**
```

Implementar somente gaps residuais comprovados:

1. Auditar equipamento por slot, equip/unequip, item instance/id e durabilidade.
2. Confirmar se equipment usa `ItemId` simplificado ou instância real; documentar contrato real.
3. Corrigir apenas inconsistências que quebrem save/load, combat stats ou UI mínima.
4. Validar loot tables e drops integrados ao inventory.
5. Validar environment resistance/durability se já houver runtime; se inexistente, criar MVP mínimo somente se a spec histórica exigir e sem afetar combat tuning.
6. Criar/estender validators para itens equipáveis:
   - weapon item sem WeaponId;
   - spell item sem SpellId;
   - equippable sem slot permitido;
   - durability inválida;
   - loot table com item inexistente.

## /speckit.tasks

- [ ] Auditoria real de equipment/durability/loot.
- [ ] Correções mínimas implementadas.
- [ ] Save/load de equipment/durability validado.
- [ ] Loot drop → inventory validado.
- [ ] Validators sem errors críticos.
- [ ] Relatório `docs/validation/spec_mvp_closeout_20_equipment_durability_environment_loot_closeout_execution_report.md`.

## Regressão a evitar

- Não quebrar bow/arrow/fireball.
- Não quebrar shop buy/sell de equipment.
- Não alterar balanceamento amplo.
- Não alterar save schema sem migration.
- Não quebrar hotbar/equipment slot picker.

## Critérios de aceite

- Equipment MVP funcional em Play Mode.
- Durability/environment/loot classificados como completo MVP ou pendência explicitamente movida para pós-MVP.
- SPEC 10 pode ser promovida com evidência ou permanecer parcial com escopo residual reduzido e factual.
