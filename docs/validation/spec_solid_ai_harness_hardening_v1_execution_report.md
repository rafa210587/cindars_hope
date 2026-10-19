# Execution Report — SOLID/AI e hardening do harness

> Spec: `spec_solid_ai_harness_hardening_v1`
> Data: 2026-09-07 (America/Sao_Paulo)
> Phase status: CODE_COMPLETE; testes contratuais PASS; strict global FAIL em docs.
> Não é claim BUILD_VALIDATED, UNITY_VALIDATED ou PLAYMODE_VALIDATED.

validated_adrs: []
validated_game_rules: []

## Scope

Pedido humano de qualidade, modularização, clean code, SOLID e uso de AI. Esta fatia
modifica instruções e tooling; o índice Roslyn e a refatoração runtime são fatias
separadas coordenadas pelo orquestrador. Nenhum asset, scene, prefab, save real ou C#
runtime foi editado por esta fatia. Sem commit/push.

## Acceptance criteria extracted

- Regras SOLID/contexto indexadas, reutilizando agentes e contratos reais de save.
- Payloads de edição cobertos por um normalizador e testes contratuais dos quatro guards.
- Geração preserva hooks/config, rejeita origem inválida e é idempotente no mesmo host.
- Strict separa exit codes/logs, inclui ratchet e mantém falhas de docs explícitas.
- Scripts passam no parser; dirty concorrente e preferências permanecem preservados.
- O resultado global deve ser reportado como FAIL enquanto strict não retornar exit 0.

## Existing systems audit

Foram reutilizados os agentes de arquitetura/não-regressão/implementação, o gerador
Codex e o strict existentes. O ratchet já existia e foi conectado ao strict, sem scanner
paralelo. A orientação de save agora corresponde a `ISaveSectionProvider`,
`SaveProviderRegistry`, `SaveMigrationRegistry` e DTOs reais com `string ItemId`.
Detalhes da Phase 0 e arquivos consultados constam na spec, §8–9 e §13.

## Changes

- Rule `solid-and-ai-context` (50 linhas) e skill `solid-refactoring` (91 linhas): SRP/OCP/
  LSP/ISP/DIP por responsabilidades e contratos reais, sem quotas de LOC/interfaces.
  Patterns dependem de necessidade observada; helpers/queries locais permanecem explícitos.
- XML útil e índice gerado consultável por classe substituem a proposta de YAML manual
  em toda classe. Não foi afirmada economia mensurada de tokens.
- `architecture-reviewer`, `non-regression-auditor` e `spec-implementer` estendidos;
  composition root/domain installers substituem orientação a novos auto-bootstraps.
- `save-load-pattern` reduzida a 111 linhas, com providers/registro/migrations reais.
  Removida atribuição de ID pela posição e orientação de IDs de conteúdo inteiros.
  Identidade de instância e dados legados preservam seu contrato; sem mudança de schema.
- Quatro guards usam `edit-tool-payload.ps1`: envelope string/objeto/JSON serializado,
  múltiplos arquivos, Add/Update/Delete/Move, origem+destino e paths normalizados.
  JSON é lido como UTF-8 (inclusive diferença BOM/code page Windows PowerShell).
  Formato inválido/ignorado não vira proteção fictícia: exit 2 com diagnóstico sem payload.
- Gerador lê settings canônico antes de resetar saídas, preserva eventos/opções/ordem,
  traduz matchers conhecidos e inclui Stop sync. Targets permanecem no repo e não
  atravessam reparse points. Config existente é preservado byte-exato; scaffold ausente
  usa propriedades root-level. Preferências do usuário não foram alteradas.
- Strict executa cada gate em processo separado, mantendo logs visíveis e exit code
  separado. Docs failure é FAIL, sem rótulo automático de legado. Ratchet existente
  integrado à sequência; falha interrompe passos posteriores.

## Files changed

Fontes: spec desta fatia; `CLAUDE.md`; `.claude/rules/{solid-and-ai-context,unity-architecture,RULES}.md`;
`.claude/skills/{solid-refactoring,save-load-pattern}/SKILL.md`; três agents citados acima;
`.claude/hooks/{edit-tool-payload,runtime-code-guard,protected-path-guard,guard-secrets,guard-large-files}.ps1`;
`tools/codex/{Generate-CodexHarness,Test-EditToolGuards,Test-CodexHarnessGeneration}.ps1`;
`tools/codex/README.md`; `tools/docs/{run_strict_validation,Test-StrictValidation}.ps1`; este report.

Saídas coordenadas: bloco gerado `AGENTS.md`, `.agents/skills/**`, `.codex/agents/**`,
`.codex/rules/**`, `.codex/hooks.json`. O diff real contém apenas arquivos cujo conteúdo
mudou na sincronização. `RULES.md`/keyart preexistentes foram preservados na fonte;
keyart passa a existir também na cópia gerada como consequência da paridade.

## Spec Compliance Matrix

| Critério | Evidência | Resultado |
|---|---|---|
| SOLID/contexto e save coerentes | Rule 50 linhas; skill 91; save skill 111; índices e três agentes atualizados | Implementado; revisão de contratos |
| Normalização e guards | `Test-EditToolGuards.ps1`, 77 assertions | PASS, exit 0 |
| Paridade/idempotência/config | `Test-CodexHarnessGeneration.ps1`, 30 assertions; geração real e config hash preservado | PASS, exit 0 |
| Strict honesto com ratchet | `Test-StrictValidation.ps1`, 23 assertions; execução real para em docs | Contrato PASS; gate global FAIL |
| Sintaxe e escopo | Parse de 10 scripts, diff check, backup e inventário prévios | PASS |
| BUILD_VALIDATED condicionado ao strict | 73 diagnósticos globais iguais ao baseline; strict exit 1 | Não atendido; claim não realizado |

## Validation

| Comando | Exit code | Resultado |
|---|---:|---|
| `powershell -NoProfile -ExecutionPolicy Bypass -File tools/codex/Test-EditToolGuards.ps1` | 0 | PASS: 77 assertions; processos de guard invocados explicitamente |
| `powershell -NoProfile -ExecutionPolicy Bypass -File tools/codex/Test-CodexHarnessGeneration.ps1` | 0 | PASS: 30 assertions; repo temporário, opções/ordem/idempotência/config byte-exato |
| `powershell -NoProfile -ExecutionPolicy Bypass -File tools/docs/Test-StrictValidation.ps1` | 0 | PASS: 23 assertions; noisy success, falhas, exception, docs, missing script, ratchet e fail-fast |
| Parser PowerShell nos 10 scripts novos/alterados | 0 | PASS: nenhum erro de sintaxe |
| `git diff --check` nos paths da fatia | 0 | PASS; avisos LF/CRLF do Git não são erros de diff |
| `powershell -NoProfile -ExecutionPolicy Bypass -File tools/codex/Generate-CodexHarness.ps1` | 0 | PASS real: 67 skills, 16 commands, 10 agents, 23 rules; 0 frontmatter/copy issues |
| `powershell -NoProfile -ExecutionPolicy Bypass -File tools/docs/run_strict_validation.ps1` | 1 | FAIL real: `DOCS_VALIDATION_FAILURE`; corrupção PASS; build/ratchet/diff/quality NOT RUN nessa invocação por fail-fast |

Os testes de geração/strict criam fixtures temporárias e as removem somente após
validar o target absoluto. Nenhum teste executa generators Unity nem modifica runtime.
Falhas globais de documentação incluem specs antigas sem markers/headers e falsos
positivos preexistentes do scanner de placeholders em variáveis do gerador. Nenhum
baseline foi aumentado, nenhuma falha foi mascarada como PASS. Marcadores/headers da
spec desta fatia e nomes de variáveis dos testes novos foram corrigidos após a primeira execução.
Reexecução intermediária do strict: exit 1, 92 diagnósticos de docs no snapshot da validação,
nenhum citando a spec/report/skill/rule/testes novos desta fatia. Log `strict-final.log`
no backup abaixo. Esse total é observação do working tree concorrente, não nova baseline.
Após corrigir os últimos markers/headers das outras duas specs SOLID e os nomes de
variáveis do ClassContext, o recheck final do orquestrador registrou 73 diagnósticos de
docs. Comparação com a baseline normalizando somente números de linha dos arquivos
PowerShell: zero diferenças, nenhum diagnóstico novo. O orquestrador também reexecutou
as suítes desta fatia (77/30/23 PASS), ratchet (PASS) e build de 7/7 projetos (exit 0).
Esses checks independentes não convertem o strict global FAIL em BUILD_VALIDATED.

## Testing Quality Gate

PASS para os contratos de tooling alterados. Asserções verificam resultados observáveis:
exit codes e diagnóstico, conteúdo removido versus adicionado, ordem de execução, origem
inválida preservando saídas, config personalizado com BOM/CRLF byte-exato e idempotência.
As invocações explícitas NÃO demonstram que o Codex desktop disparou os hooks automaticamente.
Idempotência da geração foi testada em subprocessos Windows PowerShell 5.1, o comando
canônico documentado. O pretty-print JSON difere entre PowerShell 5.1 e 7: alternar host
pode mudar somente o layout de `.codex/hooks.json`. Comparação read-only confirmou
equivalência semântica dos hooks atuais; não há claim de bytes iguais entre hosts.

## Backup e preservação de preferências

Backup antes da geração real: `C:/Users/Rafa/AppData/Local/Temp/cindars-harness-before-0f354be5c5aa4340b3bfd6260fe9bdf6`.
Contém 117 arquivos e `inventory.json` com paths/hashes. Não houve path gerado exclusivo;
`delegated-execution` divergia apenas pelo frontmatter sintetizado, com corpo idêntico.
SHA256 de `.codex/config.toml` antes/depois:
`6F369E1652B349C0B19AD365C038ADE22D65FA2BBD293C3C53FC4A39152A39C4`.
Log da primeira execução strict real: `strict-real.log` no mesmo backup temporário.

## Residual Risks

- Não existe garantia absoluta de SOLID por regras, métricas, cabeçalhos ou regex;
  review e testes continuam necessários.
- Guards inspecionam os formatos declarados e conteúdo recebido; não substituem
  validação de diff/build nem protegem edições realizadas por ferramentas não roteadas.
- PostToolUse não desfaz a alteração; exit 2 exige correção posterior.
- O ratchet por regex só mede a dívida declarada; não prova ausência de ciclos completos.
- Config existente não é corrigido automaticamente se já for inválido: preservar
  preferências é intencional. O config atual permanece inalterado.
- Strict global vermelho impede BUILD_VALIDATED desta spec; continuar sua integração
  não autoriza alterar arte/specs de outras sessões para produzir verde artificial.

## Unity validation

```text
Unity validation: NOT RUN
Reason: Esta fatia altera apenas harness/tooling; não executou Unity nem alterou runtime.
Command attempted: nenhum comando Unity nesta fatia
Residual risk: Unity compile not validated locally por esta fatia; resultados runtime são registrados separadamente pelo orquestrador.
```

## Honest status rationale

Spec permanece em `.specs/a_implementar/`, sem promoção ou commit. Testes específicos
passaram; falha global de docs e validação runtime/humana são registradas separadamente.
O strict real permanece FAIL com os 73 diagnósticos finais; o check de qualidade global
também foi reportado FAIL pelo orquestrador por três scenes modificadas anteriormente.
Essas falhas não foram ocultadas nem corrigidas incidentalmente nesta fatia. Por isso,
CODE_COMPLETE e testes contratuais PASS não significam gate global verde ou BUILD_VALIDATED.
