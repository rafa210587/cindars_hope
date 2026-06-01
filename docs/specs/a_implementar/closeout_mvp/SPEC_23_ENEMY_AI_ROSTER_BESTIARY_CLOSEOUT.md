# SPEC_23 — Enemy AI, roster, bestiary e faction locks closeout

> Spec ID: `spec_mvp_closeout_23_enemy_ai_roster_bestiary_closeout`  
> Ordem: 23  
> Status: A implementar  
> Depende de: SPEC_22  
> Bloqueia: SPEC_24-25, SPEC_28-29  
> Tipo: Enemy/Data/AI/Bestiary/Cave Ecology

## /speckit.specify

Fechar a spec ativa `docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` sem criar spawn/cave paralelo.

## /speckit.plan

Ler:

```text
docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Data/Enemies/**
Assets/_Game/Data/Combat/**
```

Implementação esperada:

1. Auditar o roster real de `EnemyDataSO`.
2. Fechar ou ajustar o requisito de 40 inimigos oficiais + hooks opcionais.
3. Validar factions técnicas revisadas.
4. Validar movement/size/vulnerability/action set profiles.
5. Confirmar `EnemyBrain` MVP e telegraph.
6. Confirmar bestiary runtime/save.
7. Confirmar XP/loot integration.
8. Criar/estender validators:
   - enemy sem faction;
   - enemy sem action set;
   - enemy sem size/movement/vulnerability profile;
   - enemy com loot/xp inválido;
   - enemy em cave band incompatível.

## /speckit.tasks

- [ ] Auditoria de enemy roster.
- [ ] Enemy validators PASS.
- [ ] Bestiary save/load validado.
- [ ] Enemy spawn resolver pronto para SPEC_24.
- [ ] Relatório `docs/validation/spec_mvp_closeout_23_enemy_ai_roster_bestiary_closeout_execution_report.md`.

## Regressão a evitar

- Não quebrar enemy health/damage/death.
- Não duplicar cave spawn resolver.
- Não copiar statblocks/textos proprietários.
- Não bloquear SPEC_24 com dados incompletos mascarados.

## Critérios de aceite

- Enemy/bestiary MVP está funcional e validável.
- Spec 13 original pode sair de `a_implementar` depois de promoção documental.
