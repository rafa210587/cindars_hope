# Relatório da Fase 3 — CindarsHope.Foundation

Data: 2026-07-05

Branch: `dev`

Status técnico: `COMPLETE`

Publicação técnica: `1fcfde72`, confirmada em `origin/dev` com divergência `0 0`

## Objetivo

Introduzir a primeira fronteira de compilação do rework sem alterar comportamento observável. A
assembly deveria ser pequena, pura, reversível e consumida pelas assemblies Unity predefinidas sem
mudança de namespace ou de identidade serializada.

## Escopo implementado

Foi criada `Assets/_Game/Scripts/Foundation/CindarsHope.Foundation.asmdef` com:

- nome `CindarsHope.Foundation`;
- `autoReferenced: true`;
- nenhuma referência explícita;
- `noEngineReferences: true`;
- nenhum define, override ou precompiled reference.

Somente dois contratos foram movidos para `Foundation/Data`:

| Contrato | Namespace preservado | GUID preservado |
| --- | --- | --- |
| `IIdentifiedData` | `CindarsHope.Core.Data` | `319077d6cfe6c8f4192c1c6588d41076` |
| `IDataRegistry<T>` | `CindarsHope.Core.Data` | `e17342ebda143d849a3f7f1063674f53` |

Esses tipos usam apenas a BCL, já eram contratos transversais e não contêm lógica de domínio,
Unity, serialização, estado global ou comportamento de gameplay. Os arquivos `.meta` foram movidos
junto com os fontes. Enums e serviços de gameplay permaneceram fora da Foundation.

O Unity gerou a pasta e os metadados novos, a DLL
`Library/ScriptAssemblies/CindarsHope.Foundation.dll`, o projeto
`CindarsHope.Foundation.csproj` e a entrada correspondente em `cindars_hope.slnx`.

## Proteções adicionadas

`ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` verifica:

- os dois contratos na mesma assembly;
- nome real da assembly igual a `CindarsHope.Foundation`;
- ausência de referência a qualquer assembly `UnityEngine*`;
- exatamente dois fontes C# na fronteira inicial;
- ausência de `UnityEngine` no código da Foundation.

O baseline de GUID passou a proteger os dois contratos e o próprio `.asmdef`.

## Ferramentas corrigidas durante o gate

O primeiro gate expôs dois defeitos de infraestrutura que poderiam produzir validação enganosa:

1. `RunUnityEditModeTests.ps1` passava `-quit`, permitindo ao Unity encerrar antes de executar os
   testes, e não falhava quando o XML não era criado. O executor agora aguarda o processo real,
   exige um XML/test-run válido, aceita filtro e propaga falhas por código de saída.
2. Os scanners de arquitetura não normalizavam `-ProjectRoot .` para caminho absoluto. Isso
   invalidava todas as chaves por arquivo. Ambos agora resolvem o root antes do cálculo relativo.

Nenhum limite de dívida foi elevado para acomodar a mudança.

## Evidência

| Gate | Resultado |
| --- | --- |
| Importação/compilação Unity repetida | exit 0; sem erro C# |
| Projetos Unity gerados | 3 descobertos e compilados; 0 warnings, 0 erros |
| Testes de arquitetura | 7/7 PASS |
| Fixtures de compatibilidade de save | 6/6 PASS |
| EditMode completa | 69/82 PASS; as mesmas 13 falhas conhecidas |
| Architecture ratchet | PASS; 19 GUIDs verificados |
| Hardcode de assembly predefinida | 0 |
| Snapshot pré-asmdef | 18 tipos internos; 3 cruzamentos; 206 arestas; 49 pares mútuos |
| Logs de compilação/teste | sem erro C#, missing script ou `MissingReferenceException` |
| Validação documental | exit 1 somente por specs/harness já fora do escopo; nenhum erro da Fase 3 |

Resultados versionados:

- `TestResults/modularization-phase3-architecture-via-runner.xml`;
- `TestResults/modularization-phase3-save-fixtures.xml`;
- `TestResults/modularization-phase3-full-editmode.xml`.

As 13 falhas completas continuam restritas aos sete contratos de layout da cidade, dois testes do
`ProjectValidationRunner` e quatro contratos do catálogo de itens/flechas. Não houve falha nova em
relação à Fase 2.

`tools/docs/validate_docs.ps1` permanece vermelho pelo débito documental preexistente em
`spec_enemy_attack_kits_v1`, nas specs futuras de NPC/Town e pelos falsos positivos do
`Generate-CodexHarness.ps1`. Um falso positivo inicialmente provocado pelo nome local
`$sourceModule` no scanner novo foi eliminado; nenhum arquivo da Fase 3 aparece no resultado final.

## Observações operacionais

Na primeira importação do `.asmdef`, o wrapper de compile retornou exit 1 embora o log interno
terminasse com return code 0. Após a importação e geração dos projetos, uma execução independente do
processo Unity retornou exit 0; builds e testes subsequentes confirmaram a assembly válida.

Os três arquivos concorrentes de animação identificados no preflight não pertencem a esta fase e
permanecem fora dos commits:

- `Assets/_Game/Scripts/Editor/Enemy/GenerateEnemyWalkAnimations.cs`;
- `Assets/_Game/Scripts/Enemy/EnemyAnimator.cs`;
- `tools/enemy_anim/normalize_enemy_sheets.py`.

## Próximo gate

A Fase 4 não está autorizada. O próximo passo, após commit e publicação desta fase, é revisar o
resultado remoto e aguardar decisão explícita antes de introduzir composition root ou uma segunda
assembly.

Commits técnicos:

- `8e99b658` — `build(unity): tornar gates modulares confiaveis`;
- `1fcfde72` — `refactor(arquitetura): criar assembly Foundation`.
