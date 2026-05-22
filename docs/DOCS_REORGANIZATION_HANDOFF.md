# Handoff - Reorganizacao documental

## Executado

- `docs/` antigo movido para `docs_old/`.
- Nova estrutura `docs/` criada.
- Specs implementadas normalizadas em `docs/specs/implementados/spec_*.md`.
- Refinamentos separados em `docs/refinements/`.
- Registry de specs criado.
- Pasta local `spec/` preservada sem alteracao e usada como fonte auxiliar de merge logico quando havia sobreposicao clara.

## Nao executado

- Nenhuma alteracao de codigo.
- Nenhuma validacao Unity Play Mode.
- Nenhum merge da branch antiga `feature/docs-fase9f-cave-stable-run-spec`.
- Nenhum commit da pasta local nao rastreada `spec/`.

## Proximos passos

- Materializar FASE9H-I-J-K-L em `docs/specs/a_implementar/`.
- Enriquecer specs com evidencia linha-a-linha se necessario.
- Rodar validacao Unity em tarefa separada.

## Consolidação pós-Codex

- `docs_old/` preservado.
- Documentos ativos copiados de `docs_old/` para `docs/design`, `docs/architecture`, `docs/operations` e `docs/roadmap`.
- `docs/refinements/implementados` absorvido em `docs/refinements/implementados`.
- `docs/specs/a_implementar` absorvido em `docs/specs/a_implementar`.
- FASE9H-K copiadas de `docs_old` para `docs/specs/a_implementar`.
- FASE9L criada como placeholder controlado.
- `spec/` removida.
- `specs/` mantida como SpecKit operacional.


## Terceira consolidação — preservação de refinamentos

- Specs implementadas adicionais criadas para pickups persistentes, enemy data-driven stats, HUD/tools debug, cave visual runtime, spawn anchor, snapshot replay full layout e hardening de debug/confinement.
- Refinements implementados absorvidos de docs_old/audits.
- Refinements futuros individuais criados para FASE9C remaining, FASE9D, FASE9E, FASE9F, FASE9G amendment, FASE9H, FASE9I, FASE9J, FASE9K, FASE9L e future ideas.
- docs/amendments, docs/validation e docs/backlog criados como camadas ativas.
- docs/specs/SPEC_REGISTRY_IMPLEMENTED.md e docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md atualizados.
- Nenhuma alteração de código.

