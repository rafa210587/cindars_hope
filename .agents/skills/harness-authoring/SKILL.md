---
name: harness-authoring
description: Cria novos artefatos do harness (.claude/) — skills, rules, agents, commands, hooks — seguindo os padrões de qualidade e idioma do projeto. Usar ao escrever qualquer novo artefato de .claude/ ou ao precisar de template/checklist de qualidade por tipo.
---

# Skill: Autoria de Harness

Referência canônica de idioma, glossário e estrutura: `.claude/HARNESS_AUTHORING_STANDARD.md`.

**Regra central: todo artefato novo deve ser indexado em `CLAUDE.md` na mesma sessão em que é criado — skill na tabela Skills, agent na tabela Agents, command na tabela Commands.**

## Quando usar

- Criar nova skill, rule, agent, command ou hook
- Dúvida sobre template, linha count ou estrutura de qualquer artefato
- Revisar se artefato recém-criado segue o padrão antes de commitar

## Quando NÃO usar

- Atualizar artefato existente → usar `harness-audit` + edição direta
- Criar spec, refinement, validation report ou ADR → workflows próprios

---

## Templates e gates por tipo

### Skill (`skills/<name>/SKILL.md`)

**Frontmatter:**
```yaml
---
name: <kebab-case-inglês>
description: <1-2 frases PT-BR — o que faz + quando usar com gatilhos concretos>
---
```

**Estrutura obrigatória (nesta ordem):**
1. `# Skill: <Título em PT>` + frase de contexto/precedente real do projeto
2. **Regra central** em **negrito** (a coisa mais importante)
3. `## Quando usar` — bullets com gatilhos concretos (nomes de spec, sistema, situação)
4. Conteúdo principal — procedimento, ✅/❌, templates
5. **Checklist** — obrigatoriamente na primeira metade do arquivo
6. `## Quando NÃO usar` — bullets (seção obrigatória)
7. `## Quando parar e reportar` — se for skill de processo
8. `## Relacionados` — links `(skill:)`, `(rule:)`, `(agent:)`

**Gates de qualidade:**
```
[ ] ≤ 150 linhas
[ ] Checklist na primeira metade do arquivo
[ ] "Quando NÃO usar" presente
[ ] Nenhum identificador, path ou token de status traduzido
[ ] "version:" e "when_to_use:" removidos do frontmatter
[ ] Adicionada à tabela Skills do CLAUDE.md com trigger específico
```

---

### Rule (`rules/<name>.md`)

```markdown
# Rule: <Título em PT>

**Invariante:** <A regra em uma frase em negrito>

## O que nunca acontece
## Onde se aplica
## Enforcement
```

**Gates:**
```
[ ] ≤ 70 linhas (regras consolidadas) ou 5 linhas (stub apontando para canônica)
[ ] Invariante em negrito na abertura
[ ] Seção Enforcement nomeia hook ou review que a enforça
[ ] Adicionada à lista numerada em RULES.md
```

---

### Agent (`agents/<name>.md`)

**Frontmatter:**
```yaml
---
name: <kebab-case-inglês>
description: <PT-BR; o que faz + quando usar>
tools: Read, Glob, Grep, Bash  # omitir = todas as tools disponíveis
---
```

**Estrutura obrigatória:**
```markdown
# Agent: <Título em PT>

**Role:** <uma frase>

## Quando usar
## Leitura mínima
## Edições permitidas / Edições proibidas
## Validação
## Quando parar e reportar
## Saída esperada
## Skills a usar
```

**Gates:**
```
[ ] ≤ 130 linhas (sem exemplos de código redundantes com as rules)
[ ] Skills referenciadas por IDs kebab-case, não nomes informais
[ ] Sem paths obsoletos (SPEC_EXECUTION_ORDER.md, IMPLEMENTATION_STATUS.md)
[ ] Sem contagens hardcoded ("62 tests", "PASS 14/14")
[ ] Adicionado à tabela Agents do CLAUDE.md
```

---

### Command (`commands/<name>.md`)

Sem frontmatter. Título: `# /<nome>` (não traduzir o nome).

```markdown
# /<nome>

<Para que serve em 1-2 frases.>

**Arguments:** `$ARGUMENTS` — <o que aceita>

## Quando usar
## Procedimento
## Saída esperada
## Não fazer automaticamente
```

**Gates:**
```
[ ] ≤ 120 linhas
[ ] Skills/agents referenciados por IDs canônicos
[ ] Adicionado à tabela Commands do CLAUDE.md
```

---

### Hook (`hooks/<name>.ps1`)

- **Traduzir apenas:** comentários `#` e texto human-readable em `Write-Error`/`Write-Host`
- **Não alterar:** lógica, variáveis, regex, cmdlets, exit codes, strings que são IDs/paths
- Registrar no array `hooks` do `.claude/settings.json`

**Gates:**
```
[ ] Smoke test: invocar .ps1 diretamente sem crash
[ ] Lógica byte-idêntica ao original em refactors
[ ] Registrado em settings.json
```

---

## Checklist final (todo artefato)

```
[ ] Indexado no lugar certo em CLAUDE.md
[ ] Prosa em PT-BR; identificadores/paths/tokens de status em inglês
[ ] Nenhuma contagem hardcoded
[ ] Skills a usar: IDs canônicos kebab-case (não nomes informais)
[ ] Commit em português
```

## Relacionados

- `(skill: harness-audit)` — auditar e corrigir artefatos existentes
- `.claude/HARNESS_AUTHORING_STANDARD.md` — fonte canônica de idioma e glossário
- `(rule: docs-governance)` — paths canônicos de .claude/
