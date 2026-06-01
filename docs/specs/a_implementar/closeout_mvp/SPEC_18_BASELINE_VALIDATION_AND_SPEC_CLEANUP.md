# SPEC_18 — Baseline validation e cleanup operacional

> Spec ID: `spec_mvp_closeout_18_baseline_validation_and_spec_cleanup`  
> Ordem: 18  
> Status: A implementar  
> Depende de: reorg SPEC_12 + residual fixes pós SPEC_12  
> Bloqueia: SPEC_19-29  
> Tipo: Validation/Docs/Editor

## /speckit.specify

### Objetivo

Estabelecer baseline factual antes de fechar specs parciais. Esta spec não implementa gameplay novo.

### Escopo

- Confirmar build runtime/editor.
- Confirmar docs validation.
- Confirmar que residual fixes pós SPEC_12 foram aplicados.
- Rodar repair/validators Unity.
- Criar status de execução confiável para specs 19-29.
- Marcar pacote `docs/specs/a_implementar/reorg` como fechado/não reexecutável sem mover arquivos.

### Fora de escopo

- Refatorar runtime.
- Alterar gameplay.
- Alterar save schema.
- Alterar scenes manualmente por YAML.
- Implementar specs 19-29.

## /speckit.plan

### Arquivos a ler

```text
AGENTS.md
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/backlog/reorg_architecture_residual_backlog.md
docs/validation/reorg_residual_townscene_combat_bootstrap_wiring_fix_execution_report.md
docs/specs/a_implementar/reorg/README_EXECUTION_ORDER.md
Assets/_Game/Scripts/Editor/Repair/RepairTownSceneCombatBootstrapWiring.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs
```

### Implementação

1. Rodar validações C# obrigatórias.
2. No Unity Editor, rodar `CindarsHope/Repair/Scenes/Repair TownScene Combat Bootstrap Wiring`.
3. Verificar no Inspector ou via validator que `TownScene` tem `_manaManager`, `_weaponDatabase`, `_spellDatabase`, `_statusEffectDatabase`.
4. Rodar validators principais.
5. Criar `docs/validation/spec_mvp_closeout_18_baseline_validation_and_spec_cleanup_execution_report.md`.
6. Criar `docs/specs/a_implementar/reorg/README_STATUS.md` com status `CLOSED — DO NOT REEXECUTE`.
7. Atualizar `PROJECT_LOG.md`.
8. Atualizar `docs/IMPLEMENTATION_STATUS.md` se validação real mudou.

## /speckit.tasks

### Tasks

- [ ] Build runtime/editor PASS.
- [ ] `tools/docs/validate_docs.ps1` PASS.
- [ ] TownScene repair executado ou marcado NOT RUN com motivo.
- [ ] Projectile Prefab Validator PASS ou erros documentados.
- [ ] Combat Database Validator PASS ou erros documentados.
- [ ] Farm/Town MVP Validator PASS ou erros documentados.
- [ ] Cave MVP Validator PASS ou erros documentados.
- [ ] Play Mode smoke mínimo executado ou NOT RUN factual.
- [ ] `README_STATUS.md` criado em `docs/specs/a_implementar/reorg/`.

## Critérios de aceite

- Nenhum erro C#.
- Nenhum erro crítico nos validators principais.
- `CombatRuntimeInstaller` não reporta `WeaponDatabase null`, `SpellDatabase null` ou `ManaManager null` em TownScene após repair.
- Reorg não aparece como fila ativa acidental.
- SPEC_19 liberada somente se baseline estiver estável.

## Stop conditions

Parar se:

- build falhar;
- Unity não importar scripts;
- repair falhar;
- validators acusarem erro crítico de asset/cena;
- Play Mode acusar erro crítico novo em startup.
