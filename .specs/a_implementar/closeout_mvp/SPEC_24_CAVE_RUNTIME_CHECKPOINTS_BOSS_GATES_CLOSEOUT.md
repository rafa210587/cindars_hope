# SPEC_24 — Cave runtime, generation, checkpoints e boss gates closeout

> Spec ID: `spec_mvp_closeout_24_cave_runtime_checkpoints_boss_gates_closeout`  
> Ordem: 24  
> Status: A implementar  
> Depende de: SPEC_23  
> Bloqueia: SPEC_25, SPEC_28-29  
> Tipo: Cave/Runtime/Save/UI mínima

## /speckit.specify

Fechar a spec ativa `.specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md` como MVP jogável.

## /speckit.plan

Ler:

```text
.specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Data/Cave/**
Assets/_Game/Data/Enemies/**
```

Implementação esperada:

1. Auditar cave run state, seed, level, snapshots e replay.
2. Garantir que nível visitado não rerolla dentro da mesma run.
3. Validar macro 100 níveis, biomas e boss gates/checkpoints.
4. Validar checkpoint portal e menu lateral mínimo.
5. Validar boss gates persistentes e drops únicos.
6. Integrar enemy spawn plan vindo da SPEC_23.
7. Validar common enemy respawn depois de 2 dias in-game sem trocar plan da run.
8. Validar resources/fishing/enemy plans no snapshot.
9. Validar confinement/camera bounds/materialization.
10. Criar validators de cave runtime/snapshot/gates.

## /speckit.tasks

- [ ] Cave snapshot replay PASS.
- [ ] Checkpoints/gates PASS.
- [ ] Enemy spawn plan estável PASS.
- [ ] Cave save/load PASS.
- [ ] Cave MVP validator PASS.
- [ ] Relatório `docs/validation/spec_mvp_closeout_24_cave_runtime_checkpoints_boss_gates_closeout_execution_report.md`.

## Regressão a evitar

- Não invadir death/corpse/Anya completo da SPEC_25.
- Não quebrar Farm/Town transitions.
- Não rerollar run indevidamente.
- Não duplicar enemy resolver.
- Não quebrar save schema sem migration.

## Critérios de aceite

- Cave MVP é jogável e persistente.
- Spec 14 original pode sair de `a_implementar` depois de promoção documental.
