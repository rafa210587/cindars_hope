---
name: docs-governance
description: Organiza, arquiva, indexa e mantém a governança da documentação, sem tocar em runtime code. Use para organização de documentos, planejamento de arquivamento, delete candidates, limpeza de roadmap e atualização de índices (CURRENT_STATE.md, DOCUMENT_INDEX.md).
---

# Skill: Governança de Documentação

## Quando usar

- Organizar a estrutura da documentação
- Identificar e registrar delete candidates
- Mover arquivos para o archive
- Atualizar CURRENT_STATE.md ou DOCUMENT_INDEX.md
- Limpar specs/refinements superseded

## Leitura mínima

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md`
3. `docs/project/DOCUMENT_GOVERNANCE.md`
4. `docs/project/DOCUMENT_INDEX.md`

## Leitura opcional

- `docs/project/DOCUMENT_DELETE_CANDIDATES.md` (se estiver revisando candidates)
- `docs/project/HISTORY_LOG_POLICY.md` (se houver trabalho no PROJECT_LOG)

## Não ler por padrão

```
PROJECT_LOG.md (unless audit scope)
ROADMAP.md (unless roadmap cleanup)
Runtime code files
```

## Procedimento

### Para revisão de delete candidate

1. Leia DOCUMENT_DELETE_CANDIDATES.md
2. Verifique se cada candidate ainda existe
3. Cheque se alguma spec ativa, current_state ou validation report referencia o candidate
4. Se referenciado: marque como "cannot delete yet; referenced by X"
5. Se não referenciado: confirme que está pronto para deleção
6. NÃO delete sem autorização humana explícita

### Para mover/arquivar documento

1. Identifique a origem e o destino
2. Verifique se a pasta de destino existe
3. Mova o arquivo (git mv preferido para preservar histórico)
4. Atualize a entrada em DOCUMENT_INDEX.md
5. Atualize quaisquer arquivos que referenciavam o caminho antigo
6. Rode a docs validation

### Para atualizar CURRENT_STATE.md

1. Leia o CURRENT_STATE.md atual
2. Identifique informação obsoleta ou ausente
3. Atualize de forma mínima — mantenha o arquivo abaixo de ~100 linhas
4. Verifique que nada foi removido acidentalmente

### Para atualizar DOCUMENT_INDEX.md

1. Audite novos arquivos em `docs/` ainda não indexados
2. Adicione entradas com a função e o link corretos
3. Remova entradas de arquivos deletados

## Regras

- NÃO delete sem entrada em `DOCUMENT_DELETE_CANDIDATES.md` e autorização humana
- NÃO mova specs para `implementados/` a partir desta skill (use `/finish-spec`)
- Sempre rode `tools/docs/validate_docs.ps1` após qualquer mudança em docs
- CURRENT_STATE.md deve permanecer abaixo de ~100 linhas
- NÃO edite runtime files

## Validação

```powershell
.\tools\docs\validate_docs.ps1
```

Esperado: PASS 14/14

## Regressões comuns

- Deletar acidentalmente um arquivo referenciado por uma spec ativa
- Marcar uma spec como superseded quando ela tem dependências ativas
- Deixar o CURRENT_STATE.md longo demais (anula o propósito de token-efficiency)

## Quando parar e reportar

- Um arquivo a ser deletado é referenciado por uma spec ativa ou pelo CURRENT_STATE.md
- Uma movimentação quebraria um caminho referenciado em algum arquivo de governança
- A docs validation falha após as mudanças
