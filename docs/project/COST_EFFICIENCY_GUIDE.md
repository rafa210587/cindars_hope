# Guia de Eficiência de Custo — Cindar's Hope

> Como gastar menos tokens sem perder qualidade. Criado 2026-06-23 após uma semana de custo alto.
> Os números são estimativas estruturais (não há billing por sessão exposto), mas as alavancas são reais.

## TL;DR das alavancas (por impacto)

1. **Escolha o modelo certo para a tarefa** — maior economia, sob seu controle direto.
2. **Sessões longas e contínuas** — mantêm o cache de prompt quente; evita re-pagar ~25-30k tokens de contexto fixo.
3. **Script > reasoning para trabalho homogêneo** — edição em massa (catálogos, assets, IDs) por gerador, não por agent item-a-item.
4. **Delegue trabalho mecânico a agents Sonnet** — já configurado nos agents abaixo.

---

## 1. Roteamento de modelo

O loop principal e os agents não precisam todos ser Opus. Opus ≈ 5x o custo de Sonnet; Haiku é ainda mais barato.

| Tipo de tarefa | Modelo | Por quê |
|---|---|---|
| Arquitetura, design de spec/refinamento, debug difícil, game design review, decisões | **Opus 4.8** | Exige julgamento real |
| Implementar spec mecânica, asset wiring, docs migration, rodar validação, EditMode tests, edição de catálogo/bestiário | **Sonnet 4.6** | Padrão claro, baixo julgamento |
| Reconcile status, docs-health, harness audit, commit messages, lookups simples | **Haiku 4.5** | Quase mecânico |

**Como aplicar:**
- **Loop principal:** troque o modelo da sessão (seletor de modelo do app) para Sonnet quando a sessão for de implementação/validação mecânica. Use Opus só em sessões de design/decisão.
- **Agents:** já configurados via `model:` no frontmatter. Sonnet em: `spec-implementer`, `unity-validator`, `docs-curator`, `asset-wiring-specialist`, `test-author`, `non-regression-auditor`, `performance-auditor`. Opus (herdado) em: `architecture-reviewer`, `game-design-reviewer`, `bugfix-investigator`.
- **`/fast` NÃO economiza:** é Opus com saída mais rápida, mesmo preço.

---

## 2. Disciplina de cache de prompt

O cache de prompt da Anthropic expira em ~5 minutos. O contexto fixo deste projeto (CLAUDE.md + ~33 rules + descrições de skills/agents) é **~25-30k tokens carregados em todo turno**.

- **Cache quente** (turnos seguidos, < 5 min): esse contexto sai com desconto.
- **Cache frio** (sessão nova OU sessão retomada após ociosa): re-paga tudo cheio.

**Regras práticas:**
- Agrupe trabalho relacionado numa **única sessão contínua**. Não abra 4 sessões curtas pro que era 1 fluxo.
- Não deixe uma sessão parada 20 min e volte — isso é cache miss garantido. Se vai pausar, pause de vez e retome num bloco dedicado.
- Batch de specs homogêneas: rode em sequência na mesma sessão (`/loop-spec-batch-strict`) em vez de uma sessão por spec.

---

## 3. Script > reasoning para trabalho homogêneo

Conteúdo repetitivo (ex.: 60+ `bestiary_*.asset`, catálogos de itens, IDs em massa) custa caro se cada item passa por reasoning de um agent. Prefira:
- Um **editor script Unity** ou **script PowerShell** que faz o passe inteiro.
- O modelo entra só para **escrever o gerador** e **revisar o resultado** — não para iterar item-a-item.

As skills `data-catalog-authoring` e `editor-validator-authoring` já apontam esse caminho.

---

## 4. Higiene de contexto (manutenção contínua)

- Os 14 stubs de rules já foram encolhidos para ~300 chars cada (feito). Não os deixe crescer de novo.
- Skills dormentes inflam o system prompt. Ver auditoria periódica via `/audit-harness` e a memória `project_skill_system_review_*`.
- A `context-reading-policy` (não ler PROJECT_LOG/IMPLEMENTATION_STATUS por padrão) é uma rule de custo — respeite-a.

---

## Checklist rápido antes de uma sessão

- [ ] A tarefa é mecânica? → Sonnet no loop principal (ou delegue a agent Sonnet).
- [ ] É design/decisão difícil? → Opus.
- [ ] Vou tocar conteúdo em massa? → escrever gerador, não iterar.
- [ ] É trabalho relacionado ao que já fiz hoje? → mesma sessão, não uma nova.
