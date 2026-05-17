# Checklist PR-001 — Core Foundation

## Antes de aplicar

- [ ] Git limpo: `git status`
- [ ] Branch criada: `feature/fase8-pr-001-core-foundation`
- [ ] Unity abre sem erro antes da mudança
- [ ] `CLAUDE.md` e `AGENTS.md` estão na raiz do repo

## Depois de copiar os arquivos

- [ ] Unity recompila scripts
- [ ] Console sem erro novo
- [ ] `GameEventBus.cs` compila
- [ ] Todos os eventos compilam
- [ ] Nenhum evento usa `GameObject`, `Transform`, `MonoBehaviour` ou `ScriptableObject`
- [ ] Nenhum arquivo fora do PR-001 foi alterado sem necessidade

## Commit

```bash
git add Assets/_Game/Scripts/Core docs/PR001_CORE_FOUNDATION_HANDOFF.md prompts/PR001_CODEX_REVIEW_PROMPT.md CHECKLIST_PR001.md
git commit -m "feat: adicionar fundação de eventos core"
```
