# SPEC — Resultados confiáveis de validação e testes úteis

> **Spec ID:** `spec_validation_outcomes_and_test_cleanup_v1`
> **Status:** CODE_COMPLETE
> **Wave:** QUALITY_IMPROVEMENTS
> **Type:** Editor / Tests / Maintenance
> **Depends on:** `docs/validation/TEST_VALIDATION_EFFICIENCY_REVIEW.md`
> **Scope:** manutenção e refatoração autorizadas pelo pedido humano de 2026-09-08; sem nova feature de gameplay.
> **Ordem de execucao:** baseline → mudanças → testes integrados → revisão/evidência
> **Depende de:** baseline em `TestResults/quality-improvements/20260908-000202/`
> **Bloqueia:** closeout das melhorias de validação

required_adrs: []
required_game_rules: []

# /speckit.specify

## Problema e critérios de aceite

1. O menu de validação não pode contar retorno normal como sucesso quando o validador logou erro, retornou falha ou lançou exception. Avisos não são erros. Resultado não configurado nunca é PASS.
2. Validação de transições deve reprovar cenas, gates e anchors ausentes, inspecionar apenas as cenas selecionadas, incluindo componentes inativos serializados, e preservar cenas já abertas/dirty. Não abrir uma cena com Single para restaurar o editor.
3. Remover os oito testes de atribuição/List identificados na auditoria; consolidar oito casos de escala já cobertos pela tabela concreta e retirar a falsa prova de integração por duas chamadas iguais. Manter defaults, cálculos, guards e consequências comportamentais.
6. Executar os três scans de source/GUID/predefined assemblies no ratchet PowerShell canônico com contratos adversariais; manter no EditMode as duas provas de assemblies compiladas sem UnityEngine. Retirar listas manuais de filenames que não representam fronteiras de dependência.
7. Corrigir quatro casos de restore inválido de stamina/status para usar managers válidos com estado sentinela, distinguindo guarda de DTO inválido da guarda de manager ausente.
4. Cobrir regressões do mecanismo de validação com fixtures isoladas. Não mascarar as quatro falhas preexistentes de farm nem alterar valores artísticos para passar testes.
5. API pública de gameplay, IDs, campos serializados, GUIDs e save schema permanecem intactos. Nenhum asset/cena do projeto será regravado pelo código de validação.

## Existing systems audit

Reusar `ValidationReport`, `ValidationIssue`, `IProjectValidator` e `ProjectValidationRunner`. Os validadores legados publicam logs ou possuem wrappers `Run`/`ValidateCountsMenu`; adaptar a orquestração para consumir esses resultados. A inspeção de transições permanece no validador existente, com entrada testável de cenas carregadas. Nenhum framework, event bus ou registry paralelo.

# /speckit.plan

## Contratos e decisões

- `ValidationReport.Passed`: somente `IsConfigured && !HasErrors`; summary diferencia NOT_CONFIGURED de PASS.
- `ProjectValidationRunner.RunLoggedValidation(string, Action)`: adapta validadores síncronos legados ao report existente; captura logs de erro/assert/exception e warnings durante a chamada, converte exception em issue e remove listener em finally. Não trata logs posteriores como parte do resultado.
- `RunValidators`: propaga relatório não configurado, falha de validator e exception, continuando os demais validadores; nenhum resultado vazio inválido vira aprovação.
- `CindarsHopeMenu.ValidarProjeto`: usa passo de validação separado dos passos de geração/reparo. Wrappers existentes de shops/skills expõem seus erros, em vez de descartar listas/booleans.
- `ValidateSceneTransitions`: separa aquisição temporária de cenas de inspeção; fornece report de cenas carregadas para teste sem salvar arquivos. Preserva a seleção original e fecha somente cenas abertas pela própria validação, em finally.

## Arquivos permitidos

- `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs`
- `Assets/_Game/Scripts/Editor/Validation/ProjectValidationRunner.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidationReport.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateSceneTransitions.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneLayoutV4.cs`: ownership/try-finally e cena obrigatória ausente como erro; manter checks e layout intactos.
- `Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs`: ownership/try-finally aditivo, sem modificar checks de conteúdo; achado no fechamento do batch.
- `Assets/_Game/Tests/EditMode/Editor/ProjectValidationRunnerTests.cs`
- `Assets/_Game/Tests/EditMode/Editor/SceneTransitionValidationTests.cs` e `.meta`
- `Assets/_Game/Tests/EditMode/UI/InventoryEquipmentTooltipTests.cs`
- `Assets/_Game/Tests/EditMode/UI/MenuProjectionTests.cs`
- `Assets/_Game/Tests/EditMode/Architecture/Editor/ArchitectureRatchetTests.cs`
- `Assets/_Game/Tests/EditMode/Combat/EnemyScaleResolverPlayerRelativeTests.cs`
- `Assets/_Game/Tests/EditMode/Save/SaveSectionProviderTests.cs`
- Esta spec e seu execution report; audit, CURRENT_STATE, PROJECT_LOG e IMPLEMENTATION_STATUS para evidência final.

Leitura adicional: `ValidationIssue`, `ValidationSeverity`, `SceneTransitionGate`, `SceneSpawnAnchor`, `SceneId` e os wrappers já chamados pelo menu. Proibidos edits de `.unity`, `.prefab`, `.asset`, `Packages`, `ProjectSettings`, farm/keyart e cave procedural.

# /speckit.tasks

## Execução

1. Baseline registrada: 2.904 casos, 2.900 passes, mesmas quatro falhas de farm; fingerprint inalterado antes/depois do Unity.
2. Remover oito testes tautológicos pelo nome exato; refatorar o mecanismo de outcomes e adicionar testes de falha, warning, exception/unsubscribe e ausência de configuração.
3. Corrigir inspeção de transições; testar ausência, isolamento entre cenas e preservação do estado aberto/dirty com cenas temporárias em memória.
4. Validar junto às demais mudanças C# autorizadas em uma execução integrada. Compilação do Test Runner conta para Editor; .NET/global e PlayMode conforme matriz e risco.
5. Reportar PASS/FAIL/NOT RUN com artefatos, incluindo dívida global. Não promover a spec enquanto os requisitos aplicáveis não forem satisfeitos.

## Riscos residuais

Captura de logs é um adapter para validadores síncronos legados; não é protocolo para tarefas assíncronas. Erros reais emitidos durante a validação devem reprovar, mesmo que a chamada retorne. A migração gradual para reports tipados evita depender permanentemente do console. Testes de cena usam objetos temporários e Editor API, sem edição manual de YAML.
