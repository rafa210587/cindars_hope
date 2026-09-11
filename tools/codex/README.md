# Codex Harness Generator — Cindar's Hope

Materializa o harness do Codex a partir da fonte canônica `.claude/`. A geração preserva
preferências locais e compartilha os scripts de guard; paridade de arquivos não comprova
que o host carregou ou executou os hooks nesta sessão.

## Como rodar

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/codex/Generate-CodexHarness.ps1
```

`-WhatIf` para simular sem escrever. Rode sempre que editar algo em `.claude/` (skill, rule,
agent, command, hook, settings) para re-sincronizar o lado do Codex. O catálogo canônico é
`.claude/HARNESS_INDEX.md`; `CLAUDE.md` e o bloco gerado de `AGENTS.md` fazem o roteamento curto.
A geração atualiza saídas correspondentes, copia recursos das skills recursivamente e preserva
arquivos extras do usuário. Os targets devem permanecer no repo e não atravessar reparse points.
Settings ou catálogo inválidos falham no preflight. Inputs iguais devem produzir bytes iguais.

## O que ele gera (nunca toca em `.claude/`)

| Saída | Origem em `.claude/` | Formato Codex |
|---|---|---|
| `AGENTS.md` (bloco entre marcadores `BEGIN/END CODEX-HARNESS`) | roteamento do gerador + catálogo canônico | entrada curta; preserva invariantes fora do bloco |
| `.agents/skills/<nome>/**` | `.claude/skills/*` + `.claude/commands/*` | skills com frontmatter e referências carregadas sob demanda |
| `.codex/agents/<nome>.toml` | `.claude/agents/*.md` | subagents do Codex (`developer_instructions`, `sandbox_mode`, `model_reasoning_effort`) |
| `.codex/rules/<nome>.md` | `.claude/rules/*` | cópia verbatim, referenciada pelo `AGENTS.md` |
| `.codex/hooks.json` | `.claude/settings.json` (hooks) | mesmos eventos, apontando para os `.ps1` |
| `.codex/config.toml` | scaffold somente se ausente | preferências existentes preservadas byte a byte; defaults no nível raiz |

## Mapa de equivalência

| Claude Code | Codex CLI |
|---|---|
| `CLAUDE.md` | `AGENTS.md` |
| `.claude/skills/<n>/SKILL.md` | `.agents/skills/<n>/SKILL.md` (mesmo formato) |
| `.claude/agents/*.md` (frontmatter) | `.codex/agents/*.toml` |
| `.claude/commands/*.md` (`/cmd`) | vira **skill** (`$cmd` / disparo por descrição) — custom prompts do Codex são user-level e deprecated |
| `.claude/rules/*.md` | `.codex/rules/*.md`; descoberta pelo catálogo e `RULES.md` |
| `.claude/settings.json` (hooks) | `.codex/hooks.json` |

## Hooks — fonte única

O gerador lê `hooks` de `.claude/settings.json` e preserva eventos, ordem, comandos e
opções. `.codex/hooks.json` aponta para os mesmos scripts `.claude/hooks/*.ps1`.
Os quatro guards de edição usam `edit-tool-payload.ps1`: normaliza Write/Edit/write_file
e apply_patch multi-file (string/objeto, add/update/delete/move), lê JSON UTF-8 e retorna
exit 2 com diagnóstico quando não consegue inspecionar a entrada. PostToolUse não desfaz edits.

Mapeamento de eventos (espelha `settings.json`):

- Alternativas conhecidas de shell (`Bash|PowerShell`) ganham `shell|exec_command`.
- Alternativas conhecidas de edição (`Edit|Write`) ganham `apply_patch|write_file`.
- Matchers customizados são preservados; suas tools precisam de contrato compatível.
- `PreToolUse` edição → `protected-path-guard`, `guard-secrets`, `guard-large-files`.
- `PostToolUse` edição → `runtime-code-guard.ps1`
- `Stop` inclui os hooks canônicos, inclusive `sync-harness-and-tracing.ps1`.
- Sync compara hashes dos inputs e guarda somente execuções bem-sucedidas e estáveis.
  Dirty persistente sem mudança não repete geração; falhas continuam visíveis e tentadas no próximo Stop.

Testes explícitos (não evidenciam disparo automático pelo host):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/codex/Test-EditToolGuards.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/codex/Test-CodexHarnessGeneration.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/codex/Test-ProgressiveHarnessHooks.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/docs/Test-StrictValidation.ps1
```

## Permissões (`permissions.ask` do Claude)

O modelo de aprovação do Codex é mais grosso que o `permissions.ask`. Os bloqueios críticos
(git destrutivo, edição de `.unity/.prefab/.asset`, `Packages/**`, `ProjectSettings/**`) são
enforçados pelos hooks `PreToolUse` (via `pre-bash-guard` e a cadeia de edição). Ajustes finos
adicionais podem ser feitos com `approval_policy` no `.codex/config.toml`.

## Modelos e routing

O Codex usa a própria família de modelos, então o routing Opus/Sonnet/Haiku **não** é
transferido como nome de modelo. Os agents são gerados **sem** `model` fixo (herdam o modelo
pai) e usam `model_reasoning_effort` (reviewers/auditores = `high`) + `sandbox_mode`
(audit-only = `read-only`). `unity-validator` usa `workspace-write` para runners e evidência,
sem autorização para implementar correções. Delegação é proporcional: uma correção pequena
e seu teste podem permanecer no mesmo contexto; revisão independente depende do risco.
Esforço não comprova custo menor. Novos agentes podem exigir recarregar a configuração da sessão.

## Limitação conhecida

Arquivos de origem corrompidos em `.claude/` (disco) geram placeholders válidos no lado do
Codex com um aviso e resultado de falha. Corrija a fonte sem descartar trabalho válido e
re-execute o gerador; um placeholder não constitui paridade aprovada.
