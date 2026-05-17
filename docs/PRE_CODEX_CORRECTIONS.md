# Correções recomendadas antes de rodar Codex nos próximos PRs

## Bloqueantes operacionais

1. Copiar `CLAUDE_v1.2.md` como `CLAUDE.md` na raiz do repositório.
2. Garantir `AGENTS.md` na raiz do repositório.
3. Copiar os documentos v2.6 para `/docs`.
4. Abrir Unity e validar Console sem erro antes do PR-001.
5. Criar branch `feature/fase8-pr-001-core-foundation`.
6. Confirmar `git status` limpo antes e depois da aplicação.

## Correções documentais pequenas

1. Em `GDD_v2.6.md`, atualizar referências internas antigas:
   - `ARCH_fase4_v2.1.md` → `ARCH_fase4_v2.2.md`
   - `FASE5_ambiente_v1.1.md` → `FASE5_ambiente_v1.2.md`
   - `FASE6_FARM_backlog_v1.1.md` → `FASE6_FARM_backlog_v1.2.md`
   - `FASE6_INDEX_global_v1.1.md` → `FASE6_INDEX_global_v1.2.md`
   - `FASE7_SPEC_MVP_FARM_v2.1.md` → `FASE7_SPEC_MVP_FARM_v2.2.md`

2. Em `FASE5_ambiente_v1.2.md`, remover linha duplicada `9–13 | Execução, Polish, Testes, Build, Iteração`.

3. Em `FASE5_ambiente_v1.2.md`, corrigir qualquer orientação que diga para rodar `specify init .` dentro de `Assets/`. O correto é rodar na raiz do repositório/projeto Unity.

4. Em `FASE6_FARM_backlog_v1.2.md`, corrigir o cabeçalho que ainda menciona `v1.1`.

## Não fazer agora

- Não criar Player neste PR.
- Não criar InventoryManager neste PR.
- Não criar SaveManager neste PR.
- Não criar cena Unity neste PR.
- Não adicionar arte/sprites neste PR.
- Não pedir ao Codex para implementar múltiplos PRs de uma vez.
