# SPEC_21 — Damage, status, elements e resistances closeout

> Spec ID: `spec_mvp_closeout_21_damage_status_elements_resistances_closeout`  
> Ordem: 21  
> Status: A implementar  
> Depende de: SPEC_20  
> Bloqueia: SPEC_22-29  
> Tipo: Combat/Data/Runtime/Validation

## /speckit.specify

Fechar a SPEC 11 histórica, hoje parcial, consolidando dano, status, elementos e resistências como contrato único para player combat, enemies, fireball/burn e future enemy actions.

## /speckit.plan

Ler:

```text
docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Player/**Status**
Assets/_Game/Data/Combat/**
```

Implementação esperada:

1. Auditar `DamageType`, `DamageRequest`, `DamageCalculator`, resistances e status effects.
2. Confirmar que `Burn`, `Chill`, `Slow`, `Bleed`, `Root` etc. não foram tratados como `DamageType` se devem ser status.
3. Confirmar que fireball aplica burn via `StatusEffectDatabase` com fallback seguro.
4. Confirmar que enemy/player status runtimes não competem em namespaces ambíguos.
5. Corrigir gaps de nomenclatura/documentação sem reescrever combate.
6. Adicionar validators para:
   - spell com StatusEffectId inexistente;
   - status effect com duração/dano inválido;
   - resistance profile inexistente, se houver registry;
   - enemy com resistência inválida, se houver campo.

## /speckit.tasks

- [ ] Auditoria real de damage/status.
- [ ] Contrato documentado em relatório.
- [ ] Correções mínimas implementadas.
- [ ] Fireball/burn validado em Play Mode ou teste editor.
- [ ] Validators atualizados.
- [ ] Relatório `docs/validation/spec_mvp_closeout_21_damage_status_elements_resistances_closeout_execution_report.md`.

## Regressão a evitar

- Não quebrar `ProjectileBehaviour`.
- Não quebrar `SpellCastService`.
- Não mudar dano/range/cooldown sem spec explícita.
- Não quebrar enemy death/drops.
- Não quebrar save schema.

## Critérios de aceite

- Damage/status pipeline é consistente para player attacks e enemy status.
- Fireball/burn funciona e é validável.
- SPEC 11 histórica pode ser promovida para completo MVP ou ter residual explícito mínimo.
