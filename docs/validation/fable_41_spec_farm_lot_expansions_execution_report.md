# Execution Report — fable_41_spec_farm_lot_expansions

> **Spec:** `.specs/a_implementar/fable/fable_41_spec_farm_lot_expansions.md`
> **Data:** 2026-06-20
> **Status:** BUILD_VALIDATED_WITH_WARNINGS
> **Executor:** Claude (spec-implementer — FABLE Batch 6, E30 / fable_41)

validated_adrs: [ADR-0006, ADR-0007]
validated_game_rules: [farm_rules.md, economy_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Em jogo novo, os 3 lotes nascem cercados com placa e TODO o conteúdo interno inativo | Gerador cria `FenceRoot`+`LotSign`+`ContentRoot` por lote, `ContentRoot.SetActive(false)`; `FarmLotProgression` nasce tudo `Locked`; testes `NewProgression_AllLots_StartLocked`, `Unlock_OneLot_DoesNotUnlockOthers` |
| CA-2 | Comprar a escritura (preço correto, 1 cada) + usá-la destrava na hora e persiste após save/load | `FarmLotCatalog` preços 2500/4000/3500 + 1 escritura cada (KeyItem MaxStack 1); `FarmLotService.UseDeed` idempotente + `FarmLotSceneBinding.ApplyVisualState`; round-trip `Capture/Restore`; testes `Unlock_FirstTime_*`, `Unlock_SecondTime_IsNoOp_*`, `Capture_Then_Restore_PreservesOwnedLots`, `Catalog_HasThreeLots_WithCanonicalPrices`, `Catalog_DeedItemId_ResolvesToCorrectLot` |
| CA-3 | Após destravar o lote oeste, 4 árvores frutíferas são colhíveis com fruta por estação | `FarmOrchardCatalog` (4 árvores, 4 estações) consumido pelo `FarmResourceNodeService` existente (sem 2º padrão de colheita); testes `Orchard_HasFourTrees_CoveringFourSeasons`, `Orchard_AppleTree_GivesFruit_OnlyInSpring`, `Orchard_AllSeasons_EachTreeFruitsInItsSeason`, `Orchard_Harvest_IsIdempotent_*` |
| CA-4 | Save legado (sem os campos novos) carrega com tudo travado, sem erro | `FarmLotProgression.Restore(null)` e lista ausente → tudo `Locked`; `SaveManager` restore null-safe; testes `Restore_NullSave_AllLocked_NoError`, `Restore_LegacySave_MissingOwnedLotsList_AllLocked`, `Restore_IgnoresUnknownLotIds_InSave` |

## Existing systems audit

```text
REUSADOS (sem criar paralelos):
- CreateMvpFarmScene (gerador único — ATUALIZADO com CreateFarmExpansionLots; nenhum 2º gerador);
- FarmResourceNodeService + ResourceNodeDefinition.RequiredSeason + ResourceNodeInstanceState
  (motor sazonal existente — pomar reusa; idempotência de depleção já testada na origem);
- FarmResourceInteractable + FarmResourceVisualController + FarmResourceReward
  (adaptador de coleta existente — árvores do pomar o reusam; sem 2º padrão de colheita);
- ItemUseManager + ItemUseHandler (pipeline de use-item F08; DeedUseHandler registrado nele
  via FarmLotRuntimeBootstrap — mesmo idioma do MagicItemRuntimeBootstrap; sem fluxo paralelo);
- GameSaveData.DailyGoals / FarmDailyGoalService.Instance.Capture/Restore (precedente F13 de
  campo aditivo null-safe — FarmLots o espelha 1:1);
- CanonicalItemCatalog.AddKeys (escrituras já existiam: item_key_lot_deed_north/east/_south);
- FarmExpansionZone (spec 05 — modelo de zona free-build; NÃO confundido com lotes-escritura;
  permanece intacto);
- FarmSceneZoneMarker / SetReference / GetBuiltinSprite (helpers do gerador).
RECONCILIADO:
- CanonicalItemCatalog: id da escritura do lote OESTE renomeado item_key_lot_deed_south →
  item_key_lot_deed_west para casar com FarmLotId.West (pomar = Oeste, HUD_LAYOUT §4).
  Nenhum teste/gerador referenciava _south (só catálogo + 1 doc de direção).
CRIADOS (aditivos, domínio Farm/Lots novo — autorizado pela spec):
- FarmLotId, FarmLotState, FarmLotDefinition, FarmLotCatalog, FarmLotsSaveData,
  FarmLotProgression (modelo puro), FarmOrchardCatalog (puro), FarmLotSceneBinding,
  FarmLotService, DeedUseHandler, FarmLotRuntimeBootstrap, FarmLotSignInteractable;
- FarmLotUnlockedEvent (Core/Events); GameSaveData.FarmLots (campo aditivo) + capture/restore.
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| 3 lotes cercados (norte/leste/oeste §4) com placa + conteúdo interno DESATIVADO | `CreateFarmExpansionLots`/`CreateExpansionLot`: FenceRoot+LotSign+ContentRoot, `ContentRoot.SetActive(false)` | OK |
| Conteúdo: Norte 12 plots / Leste pasto / Oeste 4 árvores | `CreateLotPlantingPlots` (12 FarmPlot idx 100+), `CreateLotPasture`, `CreateLotOrchard` (4 OrchardTree) | OK |
| FarmLotService {Locked,Owned} + UseDeed destrava (refs serializadas, sem Find) | `FarmLotService` lê `FarmLotSceneBinding` (refs diretas), toggle cerca/placa/conteúdo; zero Find | OK |
| UseDeed idempotente (2º uso/reload = no-op) | `FarmLotProgression.Unlock` retorna true só na transição Locked→Owned; `DeedUseHandler` não consome se false | OK |
| Escrituras 2500/4000/3500g, 1 cada | `FarmLotCatalog` preços + KeyItem MaxStack 1 no `CanonicalItemCatalog` | OK |
| Venda na prefeitura/Veska | escrituras são item de shop (catálogo F32); placement do vendedor é cena Town (F40 dona) → DEFERIDO (ver "Remaining work") | PARTIAL (data pronta; vendor wiring deferido) |
| Save: campos aditivos na seção farm (ownedLots), sem seção nova, sem migração | `GameSaveData.FarmLots` (campo aditivo, só IDs); ausência = lista vazia = Locked; sem bump de SchemaVersion | OK |
| Pomar sazonal (4 árvores, fruta por estação — padrão resource node) | `FarmOrchardCatalog` → `FarmResourceNodeService` (RequiredSeason); sem 2º padrão | OK |
| FarmLotUnlockedEvent publicado no destravamento | `FarmLotService.UseDeed` publica 1× via GameEventBus | OK |
| Sem GameObject.Find/FindObjectOfType runtime | refs serializadas pelo gerador; grep confirma zero ocorrências (só menções em comentário) | OK |
| Nenhuma referência Unity persistida | `FarmLotsSaveData` só `List<string>` | OK |
| Decisão 5.3 (EMENDA): gated só por dinheiro/recursos, sem gate de caverna | nenhum campo/fluxo de caverna no sistema; `RequiredFarmLevel=0` no pomar | OK |

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS (exit 0)
Assembly-CSharp: PASS (0E, 1 warning pré-existente CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (0E, 3 warnings pré-existentes)
check_spec_diff_completeness.ps1: PASS
Quality check (check_spec_quality.ps1): PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (estado/posse de lote, idempotência, save round-trip, regra sazonal)
Changed Unity scene/prefab/asset wiring: YES (CreateMvpFarmScene gerador — regeneração via gerador, DEFERIDA ao humano)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Farm/FarmLotsTests.cs — 21 testes)
Automated tests command: Unity Test Runner EditMode (NOT RUN aqui — Unity/PlayMode deferido por decisão do dono; compilam no Assembly-CSharp build exit 0)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (compra+destravar 1 lote; colher pomar)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: regeneração da FarmScene não executada (sem Unity) — os 3 lotes/placas/conteúdo
  e o wiring FarmLotService↔FarmLotSceneBinding existem só no código do gerador até o humano rodar
  o menu "CindarsHope/Create Scenes/Farm Scene"; venda da escritura na prefeitura/Veska precisa de
  wiring do vendedor na TownScene (F40 dona) — não tocado nesta spec.
```

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`: núcleo determinístico (estado de lote, idempotência, save
round-trip, regra sazonal, catálogo/preços) implementado, auditado e coberto por 21 testes
EditMode que compilam no `Assembly-CSharp` (build exit 0); todas as validações automatizadas
exit 0. Difere de `BUILD_VALIDATED` apenas porque (a) a regeneração da FarmScene e o Test Runner
EditMode exigem Unity (deferido pelo dono) e (b) a colocação do vendedor da escritura é cena Town
(F40 dona) — ambos `DEFERRED_TO_FINAL_VALIDATION`. Nenhuma alegação de PlayMode/aceite.

## Remaining work (deferido)

```text
1. Humano: rodar "CindarsHope/Create Scenes/Farm Scene" para materializar os 3 lotes na .unity
   (gerador atualizado; evidência de regeneração pendente — Unity não executável nesta sessão).
2. Humano: rodar Unity Test Runner EditMode (21 testes novos compilam; execução PlayMode deferida).
3. Vendedor da escritura: registrar item_key_lot_deed_north/east/west à venda (1 cada, 2500/4000/3500g)
   no ShopDataSO da prefeitura/Veska em TownScene — F40 é dona do gerador da cidade; fora do escopo
   de arquivos desta spec. Mapeamento deed→lot→preço já canônico em FarmLotCatalog.
4. Itens de fruta do pomar (item_crop_apple/cherry/pear/plum) usados como DropTableId — confirmar/gerar
   no catálogo F32 se ausentes (drop cai como id literal pelo FarmResourceNodeService; placeholder seguro).
5. Doc de direção ITEM_CATALOG_DIRECTION cita o id antigo _south — atualização documental fora do
   escopo de edição desta spec (anotado para docs-curator).
```

## Anti-regressão

```text
- Fazenda inicial (24 plots, zonas, entrada da caverna) intacta: CreateFarmExpansionLots é aditivo,
  não altera CreateFarmPlots/CreateFarmSceneFoundationZones.
- Zero GameObject.Find/FindObjectOfType em runtime (grep limpo em Farm/Lots).
- Save legado compatível: GameSaveData.FarmLots ausente = lista vazia = tudo Locked; sem migração,
  sem bump de SchemaVersion.
- Nenhuma referência Unity persistida (apenas List<string> de IDs).
- Shop/inventário sem mudança de contrato: escritura é KeyItem comum; DeedUseHandler usa o pipeline
  ItemUseManager existente.
- FarmExpansionZone (spec 05) não tocado.
```
