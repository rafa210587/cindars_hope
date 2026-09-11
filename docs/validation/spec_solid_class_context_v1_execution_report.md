# Execution report — spec_solid_class_context_v1

Status: CODE_COMPLETE_WITH_GLOBAL_GATES_FAILING. Data: 2026-09-08.

## Acceptance criteria extracted

Consulta seletiva com parser real; partial/nested/generic/assembly/XML corretos; membros
opt-in; limite de saída; exportação determinística e segura; falha de parse explícita.

## Existing systems audit

Roslyn já acompanha PowerShell7. Reusado parser/asmdefs/csproj atuais. Nenhuma dependência
instalada, catálogo manual paralelo ou mudança de runtime para suportar a ferramenta.

## Spec Compliance Matrix

| Critério | Resultado / evidência |
|---|---|
| Syntax, XML, tipos e membros | PASS — fixtures com partial, generic, nested e interfaces |
| Filtros, saída limitada e exportação | PASS — contratos de MaxResults/IncludeTests/OutputPath |
| Idempotência e proteção contra parse incompleto | PASS — SHA256 e erro sem sobrescrever índice |
| Consulta real | PASS —1698arquivos,2633declarações,2618tipos |
| Economia de tokens / SOLID certificado | Não reivindicado; ferramenta de navegação |

## Validation

Criados Get-ClassContext.ps1 e Test-ClassContext.ps1; documentação AI_CODE_CONTEXT.md,
rule/skill do harness referenciam a consulta seletiva. Nenhum cabeçalho repetitivo inserido em massa.

Testes: 15 contratos PASS; full query1698 arquivos/2633 declarações/2618 tipos na configuração
corrente, sem parse errors. Revisão independente encontrou parâmetros opcionais truncados e
membros implícitos de interface ausentes; corrigidos e caracterizados antes da entrega.

Semântica de saída: partials separados por localização; dados da sintaxe/asmdef/csproj,
XML existente ou null; membros opt-in; sourceHash; exportação idempotente fora de Assets.
Ausência de csproj é explícita. Não cobre delegates, asmref, branches inativos nem chamadas
resolvidas semanticamente. Não reivindica medição de tokens ou prova de SOLID.

Validação: `pwsh -File tools/architecture/Test-ClassContext.ps1`, exit0;
consulta por InventoryManager/SaveManager/IInventoryRuntime, full export e review.
Docs/strict globais ainda FAIL; nenhum claim BUILD_VALIDATED, commit ou promoção.

Detalhes finais: `docs/validation/SOLID_AI_PROJECT_AUDIT.md`.

## Honest status rationale

Contratos tooling passaram; `run_strict_validation.ps1` permanece FAIL por73diagnósticos
globais de docs iguais ao baseline. Não é evidência de falha do parser, nem autorização
para alegar BUILD_VALIDATED da spec ou ocultar a falha global.
