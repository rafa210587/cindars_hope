# SPEC_22 — Player combat, weapons, spells e skill actions closeout

> Spec ID: `spec_mvp_closeout_22_player_combat_weapons_spells_closeout`  
> Ordem: 22  
> Status: A implementar  
> Depende de: SPEC_21  
> Bloqueia: SPEC_23-29  
> Tipo: Combat/Input/Runtime/Validation

## /speckit.specify

Fechar a SPEC 12 histórica, hoje parcial, garantindo que player combat, weapons, spells, projectiles e skill actions estejam funcionais como MVP.

## /speckit.plan

Ler:

```text
.specs/implementados/spec_player_combat_weapons_spells_skill_actions_runtime.md
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Data/Combat/**
Assets/_Game/Data/Skills/**
```

Implementação esperada:

1. Auditar `PlayerAttackController` pós reorg.
2. Validar bow/arrow:
   - arrow dispara pela mão da arrow;
   - bow não dispara pela mão do bow;
   - consumo de arrow funciona;
   - sem arrow bloqueia com log.
3. Validar fireball:
   - mana/cooldown/range/speed/damage/status preservados;
   - projectile prefab atribuído;
   - burn aplica, se inimigo disponível.
4. Validar melee/unarmed/dodge.
5. Auditar skill actions ativas e relação com slots/respec.
6. Corrigir apenas gaps residuais comprovados.
7. Criar Play Mode checklist específico de combate.

## /speckit.tasks

- [ ] Bow/arrow Play Mode PASS.
- [ ] Fireball Play Mode PASS.
- [ ] Melee/unarmed/dodge PASS.
- [ ] Skill actions auditadas.
- [ ] Validators de projectile/combat databases PASS.
- [ ] Relatório `docs/validation/spec_mvp_closeout_22_player_combat_weapons_spells_closeout_execution_report.md`.

## Regressão a evitar

- Não mexer no input Q/E/Space salvo bug explícito.
- Não quebrar E com prioridade de interação.
- Não alterar save schema.
- Não reativar fluxos legados de spell/fireball.
- Não criar novo sistema paralelo de combat.

## Critérios de aceite

- Combat MVP validado em Play Mode.
- SPEC 12 histórica pode ser promovida para completo MVP ou manter residual claro só para polish/future content.
