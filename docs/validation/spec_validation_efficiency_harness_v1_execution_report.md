# Execution report — validation efficiency harness v1

Data: 2026-09-08 UTC. Spec: `.specs/a_implementar/spec_validation_efficiency_harness_v1.md`.
Manutenção autorizada pelo humano nesta sessão, após TEST_VALIDATION_EFFICIENCY_REVIEW.
Owner desta fatia: harness/tooling. Sem execução do Unity ou build do jogo por este owner.

validated_adrs: []
validated_game_rules: []

## Acceptance criteria extracted

1. Matriz canônica seleciona gates por escopo; evidência verificável pode ser reutilizada.
2. Strict GLOBAL continua completo/fail-fast; seleção explícita produz SCOPED_PASS distinto.
3. Runner Unity recusa lock, timeout, XML antigo/zero/inconsistente/falho e filtro sem caso.
4. Versão vem de ProjectVersion; espaços em paths preservados; wrappers incluem scanner.
5. Scanner Tests depende de XML válido e mantém compile/crash críticos, sem reprovar
   apenas exception genérica esperada sob o Test Framework.
6. Builder usa uma invocação da solução em SDK compatível; fallback mantém todos os projetos.
7. Docs não interpreta variáveis .ps1 como placeholders; scope quality respeita autorização
   explícita e preserva proteção global; declaração de testes existentes não exige novo arquivo.
8. Ratchet é owner de dívida, GUIDs e predefined assembly; dados vazios/ausentes/inválidos falham.

## Existing systems audit

Reusados strict, builder, scanner, ratchet, matriz, rules, skills e commands existentes.
O helper `UnityValidation.Common.ps1` reúne processo, versão, lock e XML; `ValidationScope.ps1`
normaliza manifesto explícito. Nenhum cache automático, framework de testes ou runtime criado.

Phase 0 foi leitura/rg dos mecanismos e da revisão anterior. A investigação confirmou
assemblies históricas fixas, docs+strict repetidos, result XML sem frescor e .NET por projeto.
O teste adverso com log de uma linha encontrou também o bug do scanner: string era indexada
como caractere; `@(...)` agora preserva a coleção de linhas, e log vazio falha.

## Spec Compliance Matrix

| Critério | Implementação/evidência | Resultado nesta fatia |
|---|---|---|
| Seleção/reuso | SPEC_VALIDATION_MATRIX_MASTER; rules validation-truth/subagent-results; skills/commands centrais | Implementado, revisão textual |
| Global/scoped honesto | run_strict_validation + Test-StrictValidation | PASS, 31 assertions |
| Resultado/processo Unity | Common, RunUnityEditModeTests, RunUnityCompileValidation + fake executable | PASS, contratos de tooling |
| Scanner contextual | ScanUnityLogs Compile/Tests + fixtures de exception/compile error | PASS, incluído nos 25 assertions Unity |
| Grafo .NET/fallback | Invoke-UnityGeneratedProjectsBuild + fake dotnet com sete projetos | PASS contratual; build real não executado aqui |
| Docs/scope/reuso de testes | validate_docs, quality/diff, ValidationScope + Test-ValidationScope | PASS, 22 assertions após audit |
| Ratchet canônico | Test-ArchitectureRatchet + fixtures com regras/baselines próprias | PASS, 17 assertions; TSV reais intocados |
| Compatibilidade PS5.1 | Parser dos 15 scripts novos/alterados | PASS, zero erros |
| Paridade Codex | Geração real PS5.1, hashes e contrato do gerador | PASS; configuração/bloco manual intactos |
| Integração real Unity/Player | Coordenada pelo orquestrador após fechamento dos owners | NOT RUN por este owner |

## Validation

Comandos executados em Windows PowerShell 5.1, sobre fixtures temporárias exclusivas.
O fake executable C# foi compilado somente para simular processo/argumentos/exit/XML;
não referencia assemblies do jogo e não inicia Unity. Fake dotnet substitui PATH apenas
no processo da suíte, restaurado no finally. Fixtures removidas com containment verificado.

| Comando | Exit final | Saída observada |
|---|---:|---|
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/docs/Test-StrictValidation.ps1` | 0 | `STRICT_VALIDATION_TESTS: PASS (31 assertions; isolated temporary scripts)` |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/unity/Test-UnityValidationTooling.ps1` | 0 | `UNITY_VALIDATION_TOOLING_TESTS: PASS (25 assertions; fake executable only)` |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/docs/Test-ValidationScope.ps1` | 0 | `VALIDATION_SCOPE_TESTS: PASS (22 assertions; isolated fixtures)` |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/architecture/Test-ArchitectureRatchetContracts.ps1` | 0 | `ARCHITECTURE_RATCHET_CONTRACTS: PASS (17 assertions; immutable real baselines)` |
| `System.Management.Automation.Language.Parser.ParseFile` nos 15 scripts alterados | 0 | `POWERSHELL_5_1_PARSE: files=15 errors=0` |

Tentativas intermediárias: Unity contracts exit 1 revelou log de uma linha ignorado;
corrigido scanner e rerodada somente a suíte afetada. Scope contracts exit 1 revelou
captura de stderr esperado interrompendo a fixture PS5 e, depois, documentos básicos
ausentes na fixture antes do detector; fixtures corrigidas e suíte afetada rerodada.
Não esconder essas tentativas nem apresentar o primeiro run como verde.

Audit posterior encontrou dois escapes de contrato: arrays de null eram normalizados
para vazio, e reports inexistentes podiam satisfazer a contagem do diff. Ambos corrigidos:
arrays exigem strings não vazias; reportPaths explícitos devem existir; reports inexistentes
ou deletados no diff não satisfazem a exigência. Fixtures novas cobrem null simples,
múltiplo e misto, report ausente e deletado. Reexecutadas somente suites de Scope e Strict,
pois consomem o helper alterado; Unity e ratchet não tiveram inputs relevantes alterados.

O teste documental usa fixture incompleta e verifica especificamente diagnóstico de
placeholder (variáveis legítimas não; token de evidence não preenchido em link Markdown sim). Ele não afirma
que o conjunto global de docs da fixture/projeto passou.

### Interface de execução final

- Strict sem Gates: GLOBAL. Com `-Gates docs,quality` (CSV via -File ou array in-process):
  SCOPED. Gates permitidos: corruption,docs,build,architecture,diff,quality.
- `-ScopePath` exige Gates e JSON com arrays exatos `changedFiles`, `allowedPaths`,
  `reportPaths` opcional. Reviewer confirma completude, autorização e diff; não é cache.
- Wrappers compile/EditMode incluem scanner. Não repetir scan do mesmo log.
- TestFilter regex pode ter componentes separados por ponto-e-vírgula; cada componente
  precisa corresponder a caso realmente executado e aprovado.
- SDK 9.0.200+ usa .slnx em uma invocação build/restore implícito; SDK anterior usa fallback
  explícito por todos os projetos. A rodada real verifica o SDK/grafo instalados.

## Honest status rationale

CODE_COMPLETE nesta fatia, com 95 assertions contratuais PASS e parser PASS.
Não há claim de GLOBAL_PASS, Unity real, Player, PlayMode ou aceitação humana.
Falhas históricas de docs/farm permanecem assunto da rodada integrada; mudanças do detector
documental não equivalem à correção de toda a dívida. Baselines não foram aumentadas.

O strict SCOPED é opt-in e não decide suficiência sozinho. Evidência textual de testes
existentes é declaração para revisão; o script não valida sua proveniência automaticamente.
O modo global de quality preserva o check anterior de arquivos proibidos; o manifesto
explícito evita atribuir dirty de terceiros à tarefa. Autorização de YAML continua separada.

### Arquivos e limites

Alterados somente matriz, spec/report próprios, rules/skills/commands de validação,
scripts/contratos em tools/docs, tools/unity e ratchet autorizado em tools/architecture.
Nenhum Assets, baseline TSV real, PROJECT_LOG, CURRENT_STATE, IMPLEMENTATION_STATUS,
commit ou push por este owner. A geração Codex abaixo foi autorizada em etapa posterior.
Outros owners continuam no mesmo checkout.

Ownership foi ampliado após audit para harmonizar cinco agentes (unity-validator,
asset-wiring-specialist, spec-implementer, bugfix-investigator e test-author) e seis skills
de domínio (bootstrap-wiring, combat-data-wiring, editmode-test-authoring, npc-walk-animation,
scene-interactable-wiring e tilemap-world-rendering). Referências históricas de assemblies,
builds/docs repetidos e scanner separado foram substituídas pela matriz/runners, preservando
papéis e evidência de domínio.

### Paridade e evidência recuperável

Após autorização do orquestrador, executado `powershell.exe -NoProfile -NonInteractive
-ExecutionPolicy Bypass -File tools/codex/Generate-CodexHarness.ps1`, exit 0:
67 skills, 16 command-skills, 10 agents, 23 rules, zero problemas de frontmatter/cópia.
Hashes de todas as rules/skills copiadas coincidem com a fonte; bodies dos sete commands
alterados e referências à matriz nos cinco agents convertidos também conferidos.

Backup byte-exato dos 119 arquivos anteriores:
`C:/Users/Rafa/AppData/Local/Temp/cindars-parity-before-2456f484f1854e849b92f7b002ad3bd8`.
Config SHA256 antes/depois: `6F369E1652B349C0B19AD365C038ADE22D65FA2BBD293C3C53FC4A39152A39C4`.
Hashes dos bytes antes/depois do bloco gerado de AGENTS também permanecem iguais.

Evidência inspecionável em `TestResults/quality-improvements/tooling/`:

- `codex-generation.stdout.log`, `.stderr.log`, `.receipt.json` e `parity-before.json`,
  `parity-after.json`, `parity-diff.json`, `parity-verification.txt`.
- `strict-contracts`, `unity-tooling-contracts`, `scope-contracts`, `ratchet-contracts`:
  cada prefixo possui `.stdout.log`, `.stderr.log` e `.receipt.json`. Conteúdo recuperado
  verbatim dos eventos CommandExecution originais da sessão, com ordinal/event ID/data,
  comando e exit code. As 95 assertions não foram reexecutadas para produzir logs.
- `codex-generation-contracts.stdout.log`, `.stderr.log` e `.receipt.json`: contrato
  do gerador executado nesta etapa, `CODEX_HARNESS_TESTS: PASS (30 assertions; isolated
  temporary repository)`, exit 0. Esses 30 são adicionais aos 95, não substituem integração.

A tentativa inicial desse último contrato falhou por Get-FileHash não disponível no
ProcessStartInfo com módulo herdado do host; stdout/stderr/receipt preservados com
infixo `.initial`. Retry corrigiu somente PSModulePath do processo filho para módulos
WindowsPowerShell do sistema e passou; nenhum script foi alterado para esse retry.
Os receipts recuperados não inventam fingerprint de inputs no momento da execução.
