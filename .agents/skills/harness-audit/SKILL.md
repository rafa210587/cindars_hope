---
name: harness-audit
description: Audita artefatos do harness (.claude/) — detecta skills ausentes do índice, agents com refs obsoletas ou skills informais, skills muito longas, contagens hardcoded e "Quando NÃO usar" faltando. Usar após waves grandes, ao adicionar batch de artefatos, ou quando o /audit-harness for chamado.
---

# Skill: Auditoria de Harness

Drift do harness é silencioso: uma skill fora do CLAUDE.md nunca dispara; um agent com ref obsoleta engana o Claude. Esta skill detecta os dois.

## Quando usar

- Após fechar uma wave com muitos artefatos novos
- Quando `/audit-harness` for chamado
- Quando uma skill ou agent parece não estar sendo ativado
- Antes de revisão de harness com o humano

## Quando NÃO usar

- Criar artefato novo → `harness-authoring`
- Auditar código de gameplay → `non-regression-review`

---

## Checklist de auditoria (8 dimensões)

### 1. Skills ausentes do CLAUDE.md

```powershell
# Skills no filesystem
$fs = Get-ChildItem .claude/skills -Directory | Select-Object -ExpandProperty Name | Sort-Object
# Skills no índice (extrair da tabela)
$idx = Select-String CLAUDE.md -Pattern "^\| \`\`(.+?)\`\`" | ForEach-Object {
    $_.Matches[0].Groups[1].Value
}
$fs | Where-Object { $_ -notin $idx }
```

```
[ ] Toda skill no filesystem está na tabela Skills do CLAUDE.md
[ ] Trigger de cada skill é específico (não "quando necessário")
```

### 2. Agents ausentes do CLAUDE.md

```powershell
$agents = Get-ChildItem .claude/agents -Filter "*.md" | Select-Object -ExpandProperty BaseName
$agentsIdx = Select-String CLAUDE.md -Pattern "^\| \`\`(.+?)\`\`" | ForEach-Object {
    $_.Matches[0].Groups[1].Value
}
$agents | Where-Object { $_ -notin $agentsIdx }
```

```
[ ] Todo agent no filesystem está na tabela Agents do CLAUDE.md
```

### 3. Skills muito longas (> 150 linhas)

```powershell
Get-ChildItem .claude/skills -Recurse -Filter "SKILL.md" | ForEach-Object {
    $n = (Get-Content $_.FullName).Count
    if ($n -gt 150) { "$n linhas: $($_.Directory.Name)" }
}
```

```
[ ] Nenhuma skill > 150 linhas
[ ] Skills 130-150 linhas: checar se checklist está na primeira metade
```

### 4. Agents muito longos (> 130 linhas)

```powershell
Get-ChildItem .claude/agents -Filter "*.md" | ForEach-Object {
    $n = (Get-Content $_.FullName).Count
    if ($n -gt 130) { "$n linhas: $($_.Name)" }
}
```

```
[ ] Nenhum agent > 130 linhas sem justificativa
```

### 5. Refs obsoletas em agents

```powershell
Select-String .claude/agents/*.md -Pattern "SPEC_EXECUTION_ORDER|IMPLEMENTATION_STATUS|docs_old" |
    Where-Object { $_ -notmatch "Nunca|proibido|Não ler" }
```

```
[ ] Nenhuma ref a SPEC_EXECUTION_ORDER.md como canônico
[ ] Nenhuma ref a IMPLEMENTATION_STATUS.md (usar CURRENT_STATE.md)
[ ] Nenhum docs_old/ como destino
```

### 6. Skill names informais em agents

```powershell
Select-String .claude/agents/*.md -Pattern "- \*\*\w[\w\s]+ (Skill|Pattern|Review)\*\*"
```

```
[ ] Skills referenciadas como IDs kebab-case, não nomes capitalizados informais
[ ] Ex.: "event-bus-pattern" não "Event Bus Pattern Skill"
```

### 7. Contagens hardcoded

```powershell
Select-String .claude/agents/*.md, (Get-ChildItem .claude/skills -Recurse -Filter "SKILL.md") `
    -Pattern "\d+ (tests|validators|PASS \d+/\d+)"
```

```
[ ] Sem "N tests do projeto", "PASS 14/14", "62 tests"
```

### 8. "Quando NÃO usar" ausente em skills

```powershell
Get-ChildItem .claude/skills -Recurse -Filter "SKILL.md" | ForEach-Object {
    $c = Get-Content $_.FullName -Raw
    if ($c -notmatch "Quando NÃO usar") { $_.Directory.Name }
}
```

```
[ ] Toda skill tem seção "Quando NÃO usar"
```

---

## Saída esperada

```text
Harness Audit
─────────────
Skills no filesystem: N | no CLAUDE.md: M | ausentes: [lista]
Agents no filesystem: N | no CLAUDE.md: M | ausentes: [lista]

Skills > 150 linhas: [nome (N linhas)]
Agents > 130 linhas: [nome (N linhas)]
Refs obsoletas: [arquivo:linha]
Skill names informais: [arquivo:linha]
Contagens hardcoded: [arquivo:linha]
Skills sem "Quando NÃO usar": [lista]

Ações corretivas priorizadas:
1. <ação> — impacto: <alto/médio/baixo>
```

## Relacionados

- `(skill: harness-authoring)` — criar artefatos novos corretamente
- `.claude/HARNESS_AUTHORING_STANDARD.md` — padrão canônico de idioma
- `(/audit-harness)` — command que invoca esta skill
