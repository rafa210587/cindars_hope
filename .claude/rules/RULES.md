# Regras do Agente — Cindar's Hope

Rules são invariantes do projeto (o *que não pode driftar*). Skills explicam workflows; hooks fazem o enforcement mecânico. Consolidado em 2026-06-12 — rules absorvidas permanecem como stubs para preservar links históricos vindos de docs/.

> **Custo de contexto (2026-06-23):** o invariante de cada rule fica sempre carregado; o detalhe verboso de algumas rules "review-lens/processo" foi movido para skills on-demand (marcadas abaixo com → skill). O stub no path da rule carrega o invariante em todo turno; a skill carrega o detalhe só quando o contexto exige.

## Active Rules

**Validation & Spec Lifecycle**
1. [Validation Truth](./validation-truth.md) — exit code 0 ou não passou; falha de script nunca é secundária; sem claims prematuros de aceitação; níveis de validação honestos
2. [Spec Quality Gate](./spec_quality_gate.md) — status taxonomy canônica; checklist BUILD_VALIDATED; promotion exige evidência *(detalhe → skill `spec-execution`)*
3. [Spec Dependency Resolution](./spec_dependency_resolution.md) — auto-resolve de chains same-wave; return-to-origin *(detalhe → skill `spec-execution`)*
4. [Testing Quality Gate](./testing-quality-gate.md) — mudanças de código exigem automated tests, Play Mode scenario, ou residual risk *(detalhe → skill `spec-execution`)*

**Code Architecture**
5. [Unity Architecture Invariants](./unity-architecture.md) — sem runtime global search; só GameEventBus; save DTOs com simple types + IDs; namespaces proibidos
6. [Cave Stable Run](./cave-stable-run.md) — contrato stable-run da FASE9F para cave procedural
7. [ID Stability](./id-stability.md) — IDs de domínio como const string; nunca renomear sem migration; prefixo de domínio obrigatório

**Unity, Git & Environment Safety**
8. [Unity Assets & Editor Safety](./unity-assets.md) — sem YAML manual; evidência de generated asset; sem batchmode paralelo
9. [No Unsafe Git](./no-unsafe-git.md) — git destrutivo exige autorização humana per-instance (enforcement via permissions.ask)
10. [Windows / PowerShell Only](./windows_powershell_only.md) — sintaxe PowerShell; checagem de exit code; env failure policy
10b. [Editor Generation Orchestration](./editor-generation-orchestration.md) — toda geração/validação/reparo via os 3 comandos canônicos (Inicializar/Validar/Reparar); sem `[MenuItem]` avulso por gerador

**Context & Docs**
11. [Context Reading Policy](./context-reading-policy.md) — context mínimo; sem PROJECT_LOG por padrão
12. [Docs Governance](./docs-governance.md) — só caminhos canônicos; sem delete sem candidate; ADRs/game_rules canônicos

**Code Craft, Resilience & Safety** (qualidade geral de engenharia — complementa os architecture invariants; revisados, nem todos hook-gated)
13. [C# Style & Craft](./csharp-style.md) — naming/immutability; sinalização de falha via `bool`+`FailureReason` *(detalhe → skill `non-regression-review`)*
14. [Error Handling & Resilience](./error-handling-resilience.md) — taxonomy de falha em quatro categorias; logs com contexto; fail fast em dev *(detalhe → skill `non-regression-review`)*
15. [Gameplay Design Pattern Selection](./gameplay-design-patterns.md) — qual pattern usar, mapeado a precedentes; domain em C# puro, engine como adapter *(detalhe → skill `non-regression-review`)*
16. [Security & File Safety](./security-and-files.md) — sem secrets em código/configs/logs; arquivos sensíveis e saves reais não editados sem pedido; ler scripts de build/deploy por inteiro primeiro
17. [Sem Magic Balance Values](./no-magic-balance-values.md) — thresholds, custos, duração e dano em SOs de balance ou consts nomeadas; nunca literais inline *(detalhe → skill `non-regression-review`)*
18. [Escada de Minimalismo de Código](./code-minimalism-ladder.md) — write-less-code: suba a decision ladder (YAGNI → reuse → built-in → nativo → dep → minimal) antes de implementar; carve-out de segurança para validação/erro/save/security (inspirado no ponytail)

**Orquestração de Subagents**
19. [Resultado de Subagent Não É Evidência](./subagent-results-not-evidence.md) — o orquestrador verifica artefatos no disco + re-roda o build antes de dar tarefa delegada por concluída; nunca confia na narração *(detalhe → skill `delegated-execution`)*

## Mechanical Enforcement (hooks em .claude/settings.json)

| Hook | Event | Enforces |
|------|-------|----------|
| `pre-bash-guard.ps1` | PreToolUse (Bash/PowerShell) | sem batchmode Unity paralelo; sem `dotnet build` filtrado |
| `protected-path-guard.ps1` | PreToolUse (Edit/Write) | docs_old/, legacy paths, root specs/, tests fora de Tests/EditMode |
| `guard-secrets.ps1` | PreToolUse (Edit/Write) | bloqueia escrever secrets reais (cloud keys, private keys, provider tokens, hardcoded credentials) — rule: security-and-files |
| `guard-large-files.ps1` | PreToolUse (Edit/Write) | bloqueia um único Write > 1 MB (dumps acidentais de data/log/blob) |
| `runtime-code-guard.ps1` | PostToolUse (Edit/Write) | forbidden search APIs, namespaces proibidos, nomes de classe duplicados |
| `detect-change-scope.ps1` + `stop-summary-check.ps1` | Stop | snapshot de change-scope + checklist de closeout adaptativo; bloqueia o stop em forbidden paths |
| `permissions.ask` (settings.json) | — | autorização humana per-instance: unsafe git, edits em .unity/.prefab/.asset, Packages/, ProjectSettings/ |

Hooks manuais invocados por commands (não automáticos): `spec-promotion-guard`, `delete-guard`, `docs-status-honesty-check`, `test-scenario-required-guard`, `run-required-validations`, `check-csproj-includes`, `check-runtime-forbidden-search` (versão full-diff), `check-cave-stable-run-scope`, `context-policy-check`, `decision-rule-reference-guard`, `post-edit-docs-validate`.

## Application

- Aplique estas rules a toda tarefa rodada por agente neste repositório.
- Se uma tarefa conflitar com uma rule, pause e peça autorização humana explícita.
- Se uma rule for intencionalmente ignorada, registre o motivo em `PROJECT_LOG.md` e no `docs/validation/*.md` relevante.
