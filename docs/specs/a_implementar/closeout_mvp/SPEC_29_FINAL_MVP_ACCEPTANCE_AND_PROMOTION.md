# SPEC_29 — Final MVP acceptance e promoção documental

> Spec ID: `spec_mvp_closeout_29_final_mvp_acceptance_and_promotion`  
> Ordem: 29  
> Status: A implementar  
> Depende de: SPEC_18-28  
> Bloqueia: Novas fases pós-MVP  
> Tipo: Validation/Docs/Release

## /speckit.specify

Fechar o ciclo MVP, promovendo specs concluídas e registrando pendências pós-MVP sem mascarar lacunas.

## /speckit.plan

Ler:

```text
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/a_implementar/closeout_mvp/**
docs/validation/spec_mvp_closeout_*.md
```

Implementação esperada:

1. Consolidar relatórios SPEC_18-28.
2. Rodar build runtime/editor.
3. Rodar docs validation.
4. Rodar editor validators.
5. Rodar Play Mode acceptance final.
6. Promover specs concluídas de `a_implementar` para `implementados` somente se todas as evidências existirem.
7. Atualizar `SPEC_EXECUTION_ORDER.md`.
8. Atualizar `IMPLEMENTATION_STATUS.md`.
9. Criar release note MVP.
10. Criar backlog pós-MVP.

## /speckit.tasks

- [ ] Build runtime/editor PASS.
- [ ] Docs validation PASS.
- [ ] Validators Unity PASS ou pendências documentadas.
- [ ] Play Mode final PASS.
- [ ] Specs promovidas conforme evidência.
- [ ] `docs/validation/spec_mvp_closeout_29_final_mvp_acceptance_and_promotion_execution_report.md` criado.
- [ ] `docs/release/MVP_ACCEPTANCE_REPORT.md` criado.
- [ ] `docs/backlog/post_mvp_backlog.md` criado.

## Regressão a evitar

- Não promover spec parcial como completa.
- Não apagar histórico sem cópia em implementados.
- Não declarar Unity PASS sem rodar Unity.
- Não declarar Play Mode PASS sem checklist humano.

## Critérios de aceite

- Estado MVP factual.
- Próximas fases podem ser planejadas com base em backlog real.
- Specs ativas e implementadas estão organizadas e sem ambiguidade.
