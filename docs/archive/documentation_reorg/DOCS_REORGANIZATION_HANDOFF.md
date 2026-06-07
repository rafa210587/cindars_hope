# Handoff — Reorganização documental

> Branch: `docs/reorganizar-specs-implementadas`
> Escopo: documentação apenas.
> Base alvo: `dev`.

## Executado

- `docs/` antigo movido para `docs_old/`.
- Nova estrutura `docs/` criada para documentação ativa.
- `docs_old/` preservado como histórico integral.
- Specs implementadas normalizadas em `docs/specs/implementados/spec_*.md`.
- Specs futuras normalizadas em `docs/specs/a_implementar/spec_*.md`.
- Refinamentos implementados separados em `docs/refinements/implementados/ref_*.md`.
- Refinamentos futuros separados em `docs/refinements/a_implementar/ref_*.md`.
- Documentos ativos copiados para `docs/design`, `docs/architecture`, `docs/operations`, `docs/roadmap`, `docs/amendments`, `docs/validation` e `docs/backlog`.
- `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` criado com rastreabilidade de `docs_old/` para a estrutura ativa.
- Pasta raiz `spec/` absorvida e removida.
- Pasta raiz `specs/` mantida como SpecKit operacional por feature.

## Não executado

- Nenhuma alteração de código.
- Nenhuma validação Unity Play Mode.
- Nenhuma implementação de gameplay.

## Consolidação de refinamentos

- FASE9H, FASE9I, FASE9J e FASE9K foram trazidas de `docs_old/` para specs futuras ativas.
- FASE9L foi criada como placeholder controlado, pois veio de refinamento em conversa e ainda precisa virar spec completa antes de implementação.
- Specs implementadas adicionais foram criadas para pickups persistentes, enemy data-driven stats, HUD/tools debug, cave visual runtime, spawn anchor, snapshot replay full layout e hardening de debug/confinement.
- Refinements implementados foram absorvidos de `docs_old/audits`.
- Refinements futuros individuais foram criados para FASE9C remaining, FASE9D, FASE9E, FASE9F, FASE9G amendment, FASE9H, FASE9I, FASE9J, FASE9K, FASE9L e future ideas.

## Correção final pré-merge

- Placeholders de template conhecidos foram removidos dos arquivos ativos críticos.
- Headers de specs/refinements críticos foram corrigidos.
- `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` foi preenchido com caminhos reais.
- Registries apontam para o crosswalk.
- Specs futuras preservam monstros, IA, armas, ferramentas, skills, skill trees, progressão, UI/UX e cave run.
- `docs_old/` permanece preservado.
- `specs/` permanece preservado como SpecKit operacional.
- `spec/` permanece removido.

## Rastreabilidade documental

Arquivos principais:

- `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`
- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
- `docs/refinements/implementados/ref_implementados_map.md`
- `docs/refinements/a_implementar/ref_futuro_map.md`

## Próximos passos fora desta branch

- Validar Unity Play Mode em tarefa separada.
- Materializar FASE9L como spec completa antes de qualquer implementação de UI/UX final.
- Implementar specs futuras em branches próprias.
