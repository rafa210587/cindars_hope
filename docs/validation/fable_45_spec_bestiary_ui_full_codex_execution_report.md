# Execution Report — fable_45 Bestiary UI Full Codex

> Spec: `.specs/a_implementar/fable/fable_45_spec_bestiary_ui_full_codex.md`
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (UI runtime; Play Mode/visual deferido — DEFERRED_TO_FINAL_VALIDATION)
> Date: 2026-06-21
> Branch: dev

validated_adrs: []
validated_game_rules: [ui_modal_rules.md, cave_rules.md]

---

## Acceptance criteria extracted

| CA | Criterio | Evidencia | Status |
|----|----------|-----------|--------|
| CA-1 | Lista com descoberta progressiva: nunca-visto = silhueta+"???"; visto = nome; tier acima do permitido AUSENTE da lista; agrupamento por banda/familia estavel | EditMode `Build_NothingDiscovered_*`, `Build_DiscoveredCreature_ShowsName_AndGroupsByBandFamily`, `Build_TierThreeBoss_AppearsOnlyAfterDiscoveredAndFlagSet` + cenario humano passos 1-3 | OK (logica) / Play Mode DEFERRED |
| CA-2 | Ficha gated por categoria: so categorias IsVisible; 5 kills faz drops aparecerem ao rebind; nenhuma categoria bloqueada renderiza conteudo real | EditMode `BuildFicha_LockedCategories_RenderUnknownLabel_NeverRealData`, `BuildFicha_DropsAppearAfterFiveKills_OnRebind`, `BuildFicha_TierLockedCreature_ReturnsNull` + cenario humano passos 4-6 | OK (logica) / Play Mode DEFERRED |
| CA-3 | Narracao do catalogo so com identidade descoberta; contador X/Y consistente | EditMode `BuildFicha_Narration_OnlyWithIdentityDiscovered`, `NarrationTable_HasTextForEveryCatalogCreature`, `Build_Completeness_*`, `BuildFicha_DocumentedBonusLine_*` + cenario humano passos 7-8 | OK (logica) / Play Mode DEFERRED |
| CA-4 | Teclado navega lista<->ficha; Esc respeita stack; Tab cicla abas; gameplay bloqueado com painel aberto (contrato F14 intacto) | EditMode `BuildFocusOrder_*` (foco puro) + cenario humano passos 9-12 (Canvas/input/modal) | Play Mode DEFERRED (obrigatorio para UI) |
| EMENDA-D | Codex exibe bonus mecanico de FullyDocumented (+stats); fichas de boss aparecem apos derrota | `BuildFicha_DocumentedBonusLine_OnlyWhenFullyDocumented`; reveal-on-defeat via F21 `IsBossSource`/`RevealFullFicha` consumido pela projection | OK (logica) |

## Existing systems audit

Fase 0 (system-reuse audit) — sistemas reutilizados, nenhum paralelo criado:

| Sistema | Origem | Encontrado | Decisao |
|---------|--------|-----------|---------|
| `EnemyKnowledgeService` (IsVisible, GetState/Kills, FullyDocumentedDamageBonus, BestiaryKnowledgeCategory, GetLevel/Studied) | F21 (`Assets/_Game/Scripts/Bestiary/EnemyKnowledgeService.cs`) | SIM | CONSUMIDO — unica fonte de visibilidade/spoiler. NAO alterado. |
| `CanonicalBestiaryCatalog` (All=77 fichas, Bands metadata) + `BestiaryCreatureDef` (Band/Family/DisplayName/SpoilerTier/MinLevel/MaxLevel/Role/Notes/PrimaryDropItemId/PrimaryDamageTypeId/VulnerabilityMatrixProfileId) | F33 (`Assets/_Game/Scripts/Combat/Bestiary/`) | SIM | LIDO — fonte unica das fichas; sem duplicar dados. |
| `BestiaryKnowledgeUnlockedEvent` (GameEventBus) | F21 (`Core/Events/BestiaryEvents.cs`) | SIM | ASSINADO com unsubscribe ao fechar. Nenhum evento novo. |
| Painel unico F14: `ModalManager` (PushModal/TryPopIfCurrent), `UiFocusController` (SetElements/Apply/Clear), `GameplayScreenTab.Bestiary=5` | F14 (`Assets/_Game/Scripts/UI/...`) | SIM | REUTILIZADO; view registrada na aba existente (precedente `CalendarDayDetailScreenView`). Sem segundo painel/canvas/builder/focus. |
| Narracao por criatura | catalogo (texto em `BestiaryCreatureDef.Notes`, F33) | SIM (em codigo) | DECISAO FASE 0: gerador de asset (NarrationText no EnemyDataSO .asset) inviavel sem Unity Editor nesta sessao (regra generated-asset-evidence). Tabela estatica `BestiaryNarrationTable` (pura C#) construida do catalogo (`def.Notes`) — fonte unica, sem duplicar as 77 strings, EnemyDataSO NAO alterado. Ponto de extensao `Overrides` documentado e vazio. |

Decisao de narracao (gerador x tabela): **TABELA ESTATICA** (`BestiaryNarrationTable`), por inviabilidade de geracao de asset Unity no ambiente; texto derivado do catalogo canonico (zero duplicacao). Rollback: campo aditivo nunca foi criado; tabela e removivel sem impacto em save/gameplay.

## Spec Compliance Matrix

| Requisito da spec | Implementacao | Status |
|-------------------|---------------|--------|
| BestiaryScreenView (NOVA, adapter fino, padrao F14) substitui placeholder | `UI/Runtime/Screens/BestiaryScreenView.cs` (MonoBehaviour, `Configure/Open/Close/Rebind/SelectEntry/CycleFilter/HandleInput`) | OK |
| BestiaryCodexProjection pura (knowledge+data -> modelo lista/ficha) | `UI/Runtime/Screens/BestiaryCodexProjection.cs` (sem UnityEngine) | OK |
| Lista agrupada por banda/familia, ordenacao estavel | `Build` + `CompareStable` (band->family->minlevel->id) | OK |
| Nao-descoberto = silhueta + "???" | `IsSilhouette`, `UnknownLabel` | OK |
| Tier acima do permitido AUSENTE (nem silhueta) + fora do denominador | `IsTierLocked` (tier>=3 oculto ate flag); `VisibleTotal` exclui locked | OK |
| Ficha so categorias IsVisible; bloqueada = "???" | `BuildFicha` delega 100% a F21 `IsVisible`; linhas locked = `UnknownLabel` | OK |
| Narracao so com identidade descoberta | `BuildFicha` seta `NarrationText` so se `identity` | OK |
| Contador "Vistos X/N · Documentados Y/N"; documentado = core categories | `CompletenessLabel`, `CoreCategories`, `IsDocumented` | OK |
| Filtros v1 (All/Seen/Defeated/Family) | `CodexFilter` + `PassesFilter`; `CycleFilter` na view | OK |
| Navegacao por teclado (setas/Enter/Esc/Tab) via UiFocusController | `HandleInput`, `BuildFocusOrder`, `_focus.Apply` | OK |
| Empty state explicito | `EmptyStateMessage`, `IsEmpty`, `EmptyMessage` | OK |
| Rebind reativo: assina BestiaryKnowledgeUnlockedEvent, unsubscribe ao fechar | `Subscribe/Unsubscribe/OnKnowledgeUnlocked/OnDisable` | OK |
| EMENDA-D: bonus mecanico FullyDocumented + boss apos derrota | `DocumentedBonusLine` (+3% via `FullyDocumentedDamageBonus`); reveal-on-defeat herdado de F21 | OK |
| Sem segundo provedor/painel/builder/dados | tudo delega a F21/F33/F14 | OK |
| Sem GameObject.Find runtime; comms via GameEventBus | host injeta via `Configure`; assina bus | OK |
| Sem save | UI nao persiste estado | OK |

## Validation

```
Validation method: run_strict_validation.ps1
Assembly-CSharp: PASS (exit 0, 0 erros; 1 warning pre-existente CS0649 CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros; 3 warnings pre-existentes)
Docs validation: PASS (validate_docs.ps1 exit 0)
Spec diff completeness: PASS (apos este report; runtime+test+report presentes)
EditMode tests: BestiaryCodexTests.cs (18 testes) compilam em Assembly-CSharp (0E) — execucao no Unity Test Runner DEFERRED
```

Comandos:
- `dotnet build .\Assembly-CSharp.csproj --no-restore` -> 0E
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` -> 0E
- `.\tools\docs\validate_docs.ps1` -> exit 0
- `.\tools\docs\check_spec_diff_completeness.ps1` -> PASS (com este report)
- `.\tools\docs\run_strict_validation.ps1` -> exit 0 (apos este report)

Nota ambiental: 3 cenas `.unity` (Cave/Farm/Town) ja estavam modificadas no working tree ANTES desta spec; nao foram tocadas por fable_45.

### Testing Quality Gate
────────────────────
Changed runtime code: YES (UI projection + view + narration table; + csproj include gitignored)
Changed deterministic logic: YES (projection, completude, filtros, gating por tier/categoria)
Changed Unity scene/prefab/asset wiring: NO (decisao: tabela estatica em vez de asset gerado)
Automated tests added/updated: YES (`Assets/_Game/Tests/EditMode/UI/BestiaryCodexTests.cs`, 18 testes)
Automated tests command: Unity Test Runner EditMode (NOT RUN nesta sessao — sem Unity; compilam via dotnet 0E)
Manual Play Mode scenario: `docs/validation/playmode/fable_45_human_test_scenario.md`
Justification if no automated tests: N/A (testes adicionados)
Residual risk: Canvas/foco-de-teclado/bloqueio-de-gameplay/rebind-na-cena nao verificados em Play Mode (UI obrigatoria); logica pura coberta por EditMode. Registro F14 da aba e injecao via Configure assumem o host do painel (mesmo ponto das demais abas) — verificar no Play Mode.

## Honest status rationale

Status **BUILD_VALIDATED_WITH_WARNINGS**: nucleo de UI implementado e auditado; toda a logica
deterministica (projection, gating por tier e por categoria, completude, filtros, narracao,
foco) coberta por 18 EditMode tests que compilam 0E; builds runtime+editor 0E; docs PASS; diff
completeness PASS; strict validation exit 0. Phase 2-3 (Play Mode/visual Canvas) DEFERRED ao
gate final humano (regra: UI exige Play Mode; ambiente sem Unity nesta sessao). NAO reivindico
ACCEPTED nem PLAYMODE_VALIDATED — nao ha evidencia de Play Mode. EditMode "passou" NAO e
reivindicado (Test Runner nao executado); apenas compilacao 0E e reivindicada.

## Remaining work

- Executar `docs/validation/playmode/fable_45_human_test_scenario.md` no Unity (CA-1..CA-4 visuais).
- Rodar Unity Test Runner EditMode para os 18 testes de `BestiaryCodexTests.cs`.
- Host do painel F14: confirmar o registro/injecao da `BestiaryScreenView` na aba 5 (Configure
  com ModalManager + EnemyKnowledgeService + CanonicalBestiaryCatalog.All) no Play Mode.
- (Fora de escopo, spec futura) search/filtros completos §15.2, notas do jogador, Combat HUD §15.4,
  retratos ilustrados, opcional gerador de NarrationText no EnemyDataSO.asset com evidencia Unity.
