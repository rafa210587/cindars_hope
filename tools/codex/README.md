# Codex Harness Generator — Cindar's Hope

Faz o **OpenAI Codex CLI** operar ~100% como o nosso harness do Claude Code, **sem duplicar
manutenção**. A fonte de verdade continua sendo `.claude/`. Este gerador lê `.claude/` e
materializa a versão que o Codex entende nativamente.

## Como rodar

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/codex/Generate-CodexHarness.ps1
```

`-WhatIf` para simular sem escrever. Rode sempre que editar algo em `.claude/` (skill, rule,
agent, command, hook, settings) para re-sincronizar o lado do Codex. É idempotente: apaga e
reconstrói as saídas geradas a cada execução.

## O que ele gera (nunca toca em `.claude/`)

| Saída | Origem em `.claude/` | Formato Codex |
|---|---|---|
| `AGENTS.md` (bloco entre marcadores `BEGIN/END CODEX-HARNESS`) | `CLAUDE.md` + rules + skills + agents | instruções que o Codex lê nativamente na raiz |
| `.agents/skills/<nome>/SKILL.md` | `.claude/skills/*` (64) + `.claude/commands/*` (16) | skills do Codex (frontmatter `name` + `description`) |
| `.codex/agents/<nome>.toml` | `.claude/agents/*.md` (10) | subagents do Codex (`developer_instructions`, `sandbox_mode`, `model_reasoning_effort`) |
| `.codex/rules/<nome>.md` | `.claude/rules/*` (21) | cópia verbatim, referenciada pelo `AGENTS.md` |
| `.codex/hooks.json` | `.claude/settings.json` (hooks) | mesmos eventos, apontando para os `.ps1` |
| `.codex/config.toml` | defaults + placeholder MCP | configuração do Codex |

## Mapa de equivalência

| Claude Code | Codex CLI |
|---|---|
| `CLAUDE.md` | `AGENTS.md` |
| `.claude/skills/<n>/SKILL.md` | `.agents/skills/<n>/SKILL.md` (mesmo formato) |
| `.claude/agents/*.md` (frontmatter) | `.codex/agents/*.toml` |
| `.claude/commands/*.md` (`/cmd`) | vira **skill** (`$cmd` / disparo por descrição) — custom prompts do Codex são user-level e deprecated |
| `.claude/rules/*.md` | `.codex/rules/*.md` + índice inline no `AGENTS.md` |
| `.claude/settings.json` (hooks) | `.codex/hooks.json` |

## Hooks — fonte única

O contrato de hook do Codex é praticamente idêntico ao do Claude Code (stdin JSON com
`tool_name`/`tool_input`, bloqueio via exit code `2` + stderr ou `permissionDecision: deny`).
Por isso `.codex/hooks.json` aponta **diretamente** para os mesmos scripts em
`.claude/hooks/*.ps1` — a lógica de guarda não é duplicada.

Mapeamento de eventos (espelha `settings.json`):

- `PreToolUse` shell (`Bash|PowerShell|shell`) → `pre-bash-guard.ps1`
- `PreToolUse` edição (`Edit|Write|apply_patch|write_file`) → `protected-path-guard`, `guard-secrets`, `guard-large-files`
- `PostToolUse` edição → `runtime-code-guard.ps1`
- `Stop` → `detect-change-scope.ps1`, `stop-summary-check.ps1`

## Permissões (`permissions.ask` do Claude)

O modelo de aprovação do Codex é mais grosso que o `permissions.ask`. Os bloqueios críticos
(git destrutivo, edição de `.unity/.prefab/.asset`, `Packages/**`, `ProjectSettings/**`) são
enforçados pelos hooks `PreToolUse` (via `pre-bash-guard` e a cadeia de edição). Ajustes finos
adicionais podem ser feitos com `approval_policy` no `.codex/config.toml`.

## Modelos e routing

O Codex usa a própria família de modelos, então o routing Opus/Sonnet/Haiku **não** é
transferido como nome de modelo. Os agents são gerados **sem** `model` fixo (herdam o modelo
pai) e usam `model_reasoning_effort` (reviewers/auditores = `high`) + `sandbox_mode`
(audit-only = `read-only`). A filosofia "planeja no loop principal, delega execução" fica escrita
no bloco gerado do `AGENTS.md`.

## Limitação conhecida

Arquivos de origem corrompidos em `.claude/` (disco) geram placeholders válidos no lado do
Codex com um aviso. Recupere a versão limpa do histórico git e re-execute o gerador.
