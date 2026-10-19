# SPEC_27 — Visual scale, camera e sprite profiles closeout

> Spec ID: `spec_mvp_closeout_27_visual_scale_camera_sprite_profiles_closeout`  
> Ordem: 27  
> Status: A implementar  
> Depende de: SPEC_24 ou análise paralela após SPEC_18  
> Bloqueia: SPEC_28-29  
> Tipo: Visual/Camera/Scene/Validation

## /speckit.specify

Fechar a SPEC 17A histórica, hoje implementada em código mas com Play Mode humano pendente.

## /speckit.plan

Ler:

```text
.specs/implementados/spec_visual_world_scale_camera_sprite_profiles.md
Assets/_Game/Scripts/**/VisualScale*
Assets/_Game/Scripts/**/CameraScale*
Assets/_Game/Data/Config/**
Assets/_Game/Scripts/Editor/**Scale**
Assets/_Game/Scripts/Cave/**
```

Implementação esperada:

1. Validar `VisualScaleProfileSO` e `VisualScaleApplicator`.
2. Validar `CameraScaleConfigSO` e `CameraScaleController`.
3. Validar bounds Farm/Town/Cave.
4. Validar cave corridors parametrizados.
5. Rodar Play Mode visual smoke:
   - Farm não parece vazia/desproporcional;
   - Town bounds corretos;
   - Cave corridors navegáveis;
   - câmera acompanha sem jitter crítico.
6. Corrigir apenas tuning/config; não reescrever runtime.

## /speckit.tasks

- [ ] Scale config validator PASS.
- [ ] Farm visual smoke PASS.
- [ ] Town visual smoke PASS.
- [ ] Cave visual smoke PASS.
- [ ] Relatório `docs/validation/spec_mvp_closeout_27_visual_scale_camera_sprite_profiles_closeout_execution_report.md`.

## Regressão a evitar

- Não quebrar colliders de árvores/floresta/lago.
- Não quebrar camera bounds.
- Não quebrar cave materialization.
- Não alterar gameplay.

## Critérios de aceite

- Visual scale/camera validado em Play Mode.
- SPEC 17A histórica pode ser promovida para completo MVP ou manter residual visual/polish explícito.
