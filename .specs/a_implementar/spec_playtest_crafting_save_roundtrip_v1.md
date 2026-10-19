# Correção do roundtrip de crafting após playtest

> Spec ID: spec_playtest_crafting_save_roundtrip_v1
> Status: UNITY_VALIDATED em 2026-09-27 (EditMode 166/166 e 29/29); save/load jogado pendente. Sem promoção.
> Wave: correções do playtest 2026-09-27; Type: Runtime/Save; Domain: Craft/Save; Priority: P1
> Executor: root, modelo herdado; risco de integridade requer revisão independente.
> Parallelizable: sim, fora dos arquivos abaixo; Repo lock scope: CraftingJob, CraftingStation restore, CraftingRuntime restore e SaveManager.Migration.
> Ordem de execucao: 1
> Depende de: nenhuma implementação nova
> Bloqueia: aceite do save/load integrado das demais fatias

required_adrs: []
required_game_rules: []

# /speckit.specify

## Objetivo e Phase 0
Refinamento D01/AC01 em `docs/refinements/a_implementar/ref_playtest_correcoes_e_experiencia_v1.md`.
Playtest gravou arquivo que falha com `Crafting job has an unknown output`. JSON preservado em
`TestResults/exploratory-playtest/crafting-unloadable-slot.json` contém trabalho inteiramente default.
Inspeção 2026-09-27: `CraftingStation.CaptureSaveData()` usa Job=null quando ociosa; serialização
Unity materializa objeto default. `SaveManager.ValidateOriginCarriers` rejeita output zero mesmo
nesse caso. `CraftingStation.LoadFromSaveData` também retorna sem limpar job anterior quando o
save é ocioso. `CraftingRuntime.LoadFromSaveData` mantém estações ausentes no snapshot anterior.
Branch/diff compartilhado já contém alterações de harness e farm; preservar integralmente.

## Requisitos e aceite
- AC01: estação ociosa atravessa JsonUtility→preflight→restore sem erro; representação default
  legada é reconhecida somente se TODOS os campos não carregam conteúdo.
- AC02: job real conserva saída, ingredientes, tempo, status e origem; output desconhecido ou
  quantidade inválida continua recusado antes de restore e sem reescrever arquivo original.
- AC03: carregar snapshot ocioso/sem estações sobre sessão com job não mantém trabalho/saída
  da sessão descartada, preservando referências das estações existentes na cena.
- AC04: fixture real do playtest valida com catálogo real; roundtrips ativo/concluído/ocioso
  e legacy v5/v6 passam; nenhum ID/schema/penalidade/receita alterado.

# /speckit.plan

## Contratos e ownership
Reusar `CraftingRuntime`, `CraftingStation`, `CraftingSectionProvider`, `SaveManager`, JsonUtility.
State owner de job: CraftingStation; provider delega sem um segundo serializer/migration.
Proposto: propriedade somente leitura `CraftingJobSaveData.IsEmptySerializedJob` (não serializada),
classifica null/empty strings, zeros e listas vazias em todos os campos do DTO; não ignora whitespace
ou ingredientes/origens/reserva presentes. Sem campos persistidos novos.
Métodos existentes: `CraftingStation.LoadFromSaveData(CraftingStationSaveData, RecipeDatabaseSO, bool)`;
`CraftingRuntime.LoadFromSaveData(CraftingRuntimeSaveData, bool)`;
`SaveManager.ValidateOriginCarriers(GameSaveData, out string)`.
Commit point de load permanece após preflight; normalize somente DTO candidato, nunca sessão/file.

## Edits permitidos
- `Assets/_Game/Scripts/Craft/CraftingJob.cs`: classificador de placeholder vazio no DTO.
- `Assets/_Game/Scripts/Save/SaveManager.Migration.cs`: após validar identidade da estação,
  normalize placeholder vazio para null; valide conteúdo de qualquer job real como antes.
- `Assets/_Game/Scripts/Save/SaveManager.cs`: publicar GameLoadedEvent de falha também nos retornos
  antecipados de arquivo ausente/preflight e exceção; UI recebe resultado sem depender de logs.
- `Assets/_Game/Scripts/Craft/CraftingStation.cs`: reset job ao carregar snapshot válido ocioso.
- `Assets/_Game/Scripts/Craft/CraftingRuntime.cs`: reset jobs/níveis das estações existentes antes
  de aplicar snapshot completo, mantendo referências; ausência de estação significa estado inicial.
- `Assets/_Game/Scripts/Save/Providers/CraftingSectionProvider.cs`: seção legada ausente restaura
  snapshot vazio, evitando conservar trabalho da sessão descartada.
- `Assets/_Game/Scripts/Save/Migrations/InventoryOriginV5ToV6Migration.cs`: reconhecer o mesmo
  placeholder vazio antes da migração de origens; preservar validação de qualquer job com payload.
- `Assets/_Game/Tests/EditMode/Save/PurchasedOriginSavePreflightTests.cs`: roundtrip real de JsonUtility,
  default compatível, job inválido rejeitado, fixture preservada não reescrita.
- `Assets/_Game/Tests/EditMode/Economy/CraftingStationJobTests.cs`: regressão restore sobre estação ocupada.
- Testes específicos adicionais em `Assets/_Game/Tests/EditMode/Save/CraftingSaveRoundTripTests.cs` e .meta.
Proibido: mudar schema/IDs, save real do usuário, cenas/prefabs, catálogo/receitas, UI ou sistemas paralelos.
Patterns: `save-load-pattern`, `editmode-test-authoring`; DTO permanece simples e sem Unity refs.

## Falhas e limites
Somente objeto integralmente vazio é ausência de job. RecipeId ausente em job com conteúdo não
pode ser tratado como ocioso; rejeitar no preflight para impedir perda silenciosa. Preservar job
com recipe ID não vazio mas catálogo de receita ausente, como contrato atual de fallback salvo.
Não usar correção para aceitar IDs de item desconhecidos. Null section segue default canônico.

# /speckit.tasks
- [x] T01 AC01/02: classificador e preflight sem relaxar validação de conteúdo.
- [x] T02 AC03: restore autoritativo limpa sessão descartada preservando objetos de estação.
- [x] T03 AC01–04: regressões JsonUtility/legacy/catálogo real e save inválido.
- [x] T04 AC01–04: filtros exigidos incluídos no XML 166/166 PASS; revisão independente concluída; finish-spec decidiu NO por rota jogada pendente e gate global.

## Validação
Gate único: testes Unity dos filtros `CraftingSaveRoundTripTests|PurchasedOriginSavePreflightTests|CraftingStationJobTests|SaveAtomicWriteTests|SaveCompatibilityFixtureTests`, zero falhas, total positivo; inclui roundtrip/compatibilidade/restore e atomicidade protegidos.
Resultados esperados não são evidência. XML planejado em `TestResults/playtest-corrections-editmode.xml`.
Caso Test Runner não esteja disponível, registrar tentativa e risco, manter PARTIAL; não declarar aceitação.
