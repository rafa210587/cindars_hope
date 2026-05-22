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
- `spec/implementado` absorvido em `docs/refinements/implementados`.
- `spec/preparado` absorvido em `docs/specs/a_implementar`.
- FASE9H-K copiadas de `docs_old` para `docs/specs/a_implementar`.
- FASE9L criada como placeholder controlado.
- `spec/` removida.
- `specs/` mantida como SpecKit operacional.
