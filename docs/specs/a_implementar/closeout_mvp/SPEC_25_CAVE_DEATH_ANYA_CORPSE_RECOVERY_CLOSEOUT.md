# SPEC_25 — Cave entry, death, Anya e corpse recovery closeout

> Spec ID: `spec_mvp_closeout_25_cave_death_anya_corpse_recovery_closeout`  
> Ordem: 25  
> Status: A implementar  
> Depende de: SPEC_24  
> Bloqueia: SPEC_26, SPEC_28-29  
> Tipo: Cave/Death/Save/Interaction/UI mínima

## /speckit.specify

Fechar a SPEC 15 histórica, hoje parcial, tornando o fluxo de entrada na cave, morte, retorno, Fonte de Anya e corpse recovery funcional como MVP.

## /speckit.plan

Ler:

```text
docs/specs/implementados/spec_cave_entry_death_anya_corpse_recovery.md
Assets/_Game/Scripts/Player/Death/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Interaction/**
Assets/_Game/Scripts/UI/**
```

Implementação esperada:

1. Auditar `PlayerDeathController`, `Corpse`, `CorpseRecoveryManager`, `DeathSaveData`.
2. Resolver ou implementar MVP mínimo para gaps citados no registry:
   - CaveDeathResolver;
   - DeathSystemBootstrap;
   - CorpseInteractable;
   - CorpseSpawner;
   - AnyaFountain/AnyaFountainInteractable;
   - AnyaRespawnService;
   - RestoreDeathSaveData TODO.
3. Garantir save/load de corpse/death.
4. Garantir fluxo morte → retorno → recuperar corpo.
5. Não invadir UI final da SPEC_28; usar UI mínima/dev modal se necessário.

## /speckit.tasks

- [ ] Death/corpse audit real.
- [ ] RestoreDeathSaveData resolvido ou bloqueio documentado.
- [ ] Corpse interactable funcional.
- [ ] Anya fountain/respawn MVP funcional.
- [ ] Save/load death/corpse PASS.
- [ ] Relatório `docs/validation/spec_mvp_closeout_25_cave_death_anya_corpse_recovery_closeout_execution_report.md`.

## Regressão a evitar

- Não quebrar cave run state.
- Não quebrar save/load geral.
- Não quebrar skill respec Anya da SPEC_26.
- Não apagar inventário/equipment indevidamente.
- Não criar UI final fora da SPEC_28.

## Critérios de aceite

- Fluxo death/corpse/Anya MVP funciona em Play Mode.
- SPEC 15 histórica pode ser promovida para completo MVP ou ter residual factual pós-MVP.
