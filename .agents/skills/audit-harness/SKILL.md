---
name: audit-harness
description: "Comando de workflow do projeto (equivalente ao /audit-harness do Claude Code). Audita o harness (`.claude/`) em busca de artefatos ausentes do índice, refs obsoletas, skills muito longas, skill names informais em agents e contagens hardcoded. Produz lista de ações corretivas priorizadas."
---

# /audit-harness

Audita o harness (`.claude/`) em busca de artefatos ausentes do índice, refs obsoletas, skills muito longas, skill names informais em agents e contagens hardcoded. Produz lista de ações corretivas priorizadas.

**Arguments:** `$ARGUMENTS` — opcional: `skills`, `agents`, `rules` para escopo parcial (padrão: todos)

## Quando usar

- Após adicionar batch de novos artefatos ao harness
- Quando uma skill ou agent parece não estar sendo disparado
- Antes de revisão do harness com o humano
- Periodicamente após waves grandes

## Procedimento

1. Usar skill `harness-audit` — executar o checklist completo das 8 dimensões
2. Reportar findings em ordem de impacto:
   - **Alta:** skills/agents ausentes do CLAUDE.md (nunca disparam)
   - **Alta:** refs obsoletas (enganam o Claude)
   - **Média:** skills > 150 linhas ou agents > 130 linhas
   - **Baixa:** skill names informais, contagens hardcoded, "Quando NÃO usar" ausente
3. Para cada finding, propor a ação corretiva mínima
4. Perguntar ao humano se deve executar as correções antes de qualquer edição

## Não fazer automaticamente

- Não commitar sem confirmação
- Não deletar artefatos
- Não mover specs para `implementados/`
- Não reescrever skills/agents sem aprovação — apenas reportar

## Saída esperada

```text
/audit-harness — <data>
──────────────────────
[resultado das 8 dimensões da skill harness-audit]

Ações recomendadas (priorizadas):
1. [Alta] Adicionar X ao CLAUDE.md — ausente do índice
2. [Alta] Corrigir ref "IMPLEMENTATION_STATUS.md" em agent Y
3. [Média] Reduzir skill Z de 180 para ≤ 150 linhas
...

Executar correções? (aguardando confirmação do humano)
```
