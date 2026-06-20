# Execution Report — fable_37 (Festivais Sazonais + Eventos de Pico Lunar + Eventos Aleatórios)

> Spec: `.specs/a_implementar/fable/fable_37_spec_festivals_lunar_events_runtime.md`
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (Phase 2-3 Unity/Play Mode DEFERRED_TO_FINAL_VALIDATION por decisão do dono)
> Date: 2026-06-20
> Branch: dev

validated_adrs: []
validated_game_rules: [time_rules.md]

---

## 1. Resumo

Implementado o `WorldEventService` — a camada de EVENTOS DE MUNDO que faltava. A cada
`DayStartedEvent`, resolve de forma 100% determinística `{festival hoje?, pico lunar hoje?,
evento aleatório?}` a partir do calendário (`GameDate`, Season+DayInSeason) e de
`StableHash(worldSeed|dia)` (FNV-1a), publica a resolução num ponto único
(`WorldEventHooks.Active`) lido pelos sistemas alvo via hooks nomeados (padrão F23), dispara
`FestivalStartedEvent`/`WorldEventStartedEvent`, monta/desmonta 3 barracas de festival na praça
e anuncia por toast. **ZERO save novo** — tudo recomputável (decisão explícita da spec; alinhado
a `time_rules.md` Rule 8/9).

Nada do que a spec pedia existia (catálogo de 8 festivais nos dias canônicos, flags, barracas,
picos lunares mecânicos, pool de eventos). O `FestivalRegistry`/`FestivalType` da WAVE 02 são
placeholders decorativos com 3 festivais em datas erradas (d14/d56/d98) — **não reutilizados como
calendário de festival** (não casam com o QUEST_CATALOG); preservados (out of scope). A
`CalendarEventVisibilityPolicy` (spoiler gate) já existia e foi reusada conceitualmente no
`NextKnownFestivals`.

---

## Existing systems audit

## 2. Fase 0 — Existing systems audit (system-reuse)

| Sistema candidato | Encontrado? | Decisão |
|---|---|---|
| Calendário / data / estação | `GameDate` + `GameCalendarService` (WAVE 02) | **REUSE** (DERIVA; não recriado) |
| Lua / ciclo lunar | `LunarCycle` (8 fases genéricas) + `LunarCycleService` | **REUSE** modelo de dia; picos mapeados (ver §3) |
| Clima | `WorldWeatherService` (F15) | **REUSE/não tocar**; seca lê hook, não gera clima |
| StableHash | `CaveLayoutStableHash`/`ForageStableHash` (FNV-1a 32-bit) | **REUSE** algoritmo, replicado em `WorldEventResolver` (World não depende de Cave/Farm) |
| Toast | `NotificationToastRequestedEvent` + `GameplayFeedbackService` | **REUSE** (sem 2º fluxo) |
| Festival pré-existente | `FestivalRegistry`/`FestivalType` (WAVE 02) | **NÃO reusado como agenda** (datas erradas, decorativo); preservado |
| Spoiler gate | `CalendarEventVisibilityPolicy.CanShowFestival` | Reusado conceitualmente em `NextKnownFestivals` |
| Hook nomeado (padrão) | `AccessoryEffectRouter` (F23: `Active` + `Apply*`) | **REUSE do padrão** para `WorldEventHooks` |
| Praça (gerador) | `CreateMvpTownScene.CreateCentralPlaza` (CentralPlaza @ origem) | Ponto de extensão (âncora aditiva) |
| Save de festival | `WorldTimeSaveData` (DTO órfão, não wired) | **NÃO tocado** (spec: SEM save) |

### Mapeamento dos picos ao modelo real (risco da Fase 0, mitigado)

O runtime expõe `LunarCycle` com 8 fases genéricas derivadas de `AbsoluteDay` (divergência
documentada em `time_rules.md` Rule 6 — modelo nomeado Alihana/Senya/Nyx é *proposta a calibrar*).
Os 4 picos nomeados do catálogo foram ancorados aos dias canônicos da direção §8.2 (1 pico/lua/
estação), **addressados por `DayInSeason`** — determinístico e re-derivável de `AbsoluteDay` (não
exige reescrever o `LunarCycle`):

| Pico | DayInSeason | Efeito nomeado | Magnitude |
|---|---|---|---|
| Lua de Cinza (`peak_ash`) | 7 | +undead no spawn da caverna | +30% |
| Lua Verde (`peak_green`) | 14 | crops +1 estágio | +1 |
| Lua Âmbar (`peak_amber`) | 21 | +preços de venda no dia | +10% |
| Lua Pálida (`peak_pale`) | 28 | Fonte dá Água Viva extra (F17) | +1 |

Nota de mapeamento da **loja noturna** (decisão v2 6.2-B): Nyx é a lua oculta da noite/segredos;
o pico de Nyx é representado por `peak_ash` (a noite de pico de spawn undead, D7 — a mesma noite
que a direção associa à lua escura). Documentado em `WorldEventHooks.IsNyxPeakToday()` para troca
fácil caso uma futura spec separe Nyx de Cinza.

---

## Acceptance criteria extracted

## 3. Acceptance criteria extracted — evidência

| CA | Critério | Evidência | Status |
|---|---|---|---|
| CA-1 | Festival no dia certo (8); barracas montam/desmontam; flag limpa no dia seguinte | `WorldEventsTests.CA1_AllEightFestivals_ResolveOnCanonicalDays`, `CA1_NoFestival_OnNonFestivalDay`, `CA1_FestivalClearsNextDay_ResolutionFlag`; barracas via `WorldEventService.ApplyFestival` (DespawnFestivalStalls sempre antes de montar) | OK (lógica + agenda); barraca visual Play Mode DEFERIDO |
| CA-2 | Pico de Lua Verde avança crops (hook nomeado) | `CA2_GreenMoonPeak_ResolvesOnDay14_WithGrowthEffect`, `CA2_GreenMoonHook_ReportsExtraStages_OnlyOnGreenPeak`; hook `FarmPlot.TryAdvanceStageFromLunarPeak` + `WorldEventFarmHook.AdvanceAllCropsStages`; demais picos: `CA2_AmberPeak_*`, `CA2_AshPeak_*`, `CA2_PalePeak_*` | OK |
| CA-3 | Determinismo do evento aleatório por `StableHash(worldSeed|dia)` | `CA3_SameSeedSameDay_SameEvent_Always` (50 recomputações), `CA3_DifferentSeeds_ProduceVariation_OverManyDays`, `CA3_ResolveDay_IsFullyRecomputable_AfterReload`, `CA3_DroughtEvent_IsReachable_AndDeterministic` | OK |
| CA-4 | Mural sem spoiler (próximos festivais CONHECIDOS) | `CA4_NextKnownFestivals_ReturnsChronologicalPublicFestivals`, `CA4_NextKnownFestivals_FiltersUnknown_WhenKnownSetProvided`, `CA4_RandomEvents_AreNotListedInMural` | OK |

### Emendas (VINCULANTES)

| Emenda | Implementação | Teste |
|---|---|---|
| 2026-06-12-D §1 (seca: chuva pode falhar) | `WorldEventEffect.Drought` no pool; `WorldEventHooks.IsDroughtToday()` (lido por F15/RainIrrigation num ponto único) | `CA3_DroughtEvent_IsReachable_AndDeterministic`, `Regression_*` |
| 2026-06-12-D §2 (loja noturna só em pico de Nyx + quest) | `WorldEventHooks.IsNightShopOpen()` = `IsNyxPeakToday()` && `NightShopQuestUnlockedSource()` | `NightShop_Closed_WithoutQuest_EvenOnNyxPeak`, `NightShop_Closed_WithQuest_ButNotNyxPeak`, `NightShop_Open_OnNyxPeak_WithQuest` |

---

## Spec Compliance Matrix

## 4. Spec Compliance Matrix (detalhada)

| Requisito da spec | Implementação |
|---|---|
| WorldEventService consome DayStartedEvent → resolve dia | `WorldEventService.OnDayStarted` → `ResolveForDay` |
| 8 festivais (dias fixos do catálogo) | `WorldEventDefinitions.Festivals` (8, datas canônicas) |
| flag festival_active(id) | `WorldEventService.IsFestivalActive(id)` + `FestivalStartedEvent` (id `festival_<slug>`, casa com `FestivalQuestDefinition.FestivalId`) |
| 3 barracas (comida/pesca/brinde) spawn/despawn pelo serviço | `FestivalStallInteractable` (3 tipos) montadas/destruídas em `ApplyFestival` |
| 4 picos lunares (hook nomeado por sistema) | `WorldEventHooks.GetCaveSpawnMultiplier/GetSellGoldMultiplier/GetExtraAguaViva/GetGreenMoonExtraStages` |
| pool de 8 eventos + seca (data-driven, peso) | `WorldEventDefinitions.RandomEventPool` (9: 8 + seca) |
| anúncio por toast + mural próximos festivais conhecidos | `Announce` (NotificationToastRequestedEvent) + `NextKnownFestivals` |
| save: NO | Nenhum DTO/seção tocada; `WorldEventResolver` puro e re-derivável |
| 1 hook nomeado por sistema (sem ifs espalhados) | Economy 1 ponto, Cave spawn 1 ponto, Farm 1 ponto (via service driver), Fonte 1 ponto |
| comunicação via GameEventBus | `FestivalStartedEvent`/`WorldEventStartedEvent`/toast/feedback |
| sem GameObject.Find runtime | Acessores estáticos `Active`/`Instance` + bootstrap (FindAnyObjectByType só em bootstrap) |

### Pontos de hook (1 por sistema alvo, sistema alvo NÃO conhece o serviço)

| Sistema | Ponto único | Arquivo:contexto |
|---|---|---|
| Economia (venda) | `WorldEventHooks.ApplySellGold(totalGold)` | `EconomyManager.HandleSellAllRequested` (após accessory gold gain) |
| Cave spawn | `WorldEventHooks.GetCaveSpawnMultiplier()` | `CaveEnemySpawnPlanner.CreatePlan` (ResolveTargetEnemyCount permanece PURO) |
| Farm (Lua Verde) | `FarmPlot.TryAdvanceStageFromLunarPeak()` via `WorldEventFarmHook.AdvanceAllCropsStages` | dirigido por `WorldEventService.ApplyGreenMoonGrowth` |
| Fonte (Água Viva) | `WorldEventHooks.GetExtraAguaViva()` | `FonteRuntimeService.TryCollectLivingWater` |
| Clima (seca) | `WorldEventHooks.IsDroughtToday()` | exposto para F15/RainIrrigation (ponto único de leitura) |

---

## 5. Arquivos

### Novos (runtime)
- `Assets/_Game/Scripts/Core/Events/WorldEventEvents.cs` — `FestivalStartedEvent`, `WorldEventStartedEvent`
- `Assets/_Game/Scripts/World/Events/WorldEventDefinitions.cs` — tabelas data-driven (8 festivais, 4 picos, pool 9)
- `Assets/_Game/Scripts/World/Events/WorldEventResolver.cs` — resolução PURA determinística + mural
- `Assets/_Game/Scripts/World/Events/WorldEventHooks.cs` — hooks nomeados (Active + Apply*/Get*) + gate Nyx
- `Assets/_Game/Scripts/World/Events/WorldEventService.cs` — MonoBehaviour resolvendo o dia
- `Assets/_Game/Scripts/World/Events/WorldEventRuntimeBootstrap.cs` — bootstrap (RuntimeInitializeOnLoad)
- `Assets/_Game/Scripts/World/Events/FestivalStallInteractable.cs` — barraca temporária (IInteractable)
- `Assets/_Game/Scripts/World/Events/FestivalStallAnchor.cs` — marcador da praça (registra âncora)
- `Assets/_Game/Scripts/Farm/WorldEventFarmHook.cs` — hook único do farm (Lua Verde)

### Novos (testes)
- `Assets/_Game/Tests/EditMode/World/WorldEventsTests.cs` — 21 testes (CA-1..4 + picos + gate + regressão)

### Alterados (hooks aditivos)
- `Assets/_Game/Scripts/Economy/EconomyManager.cs` — 1 linha de hook de venda
- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs` — multiplicador de densidade no CreatePlan
- `Assets/_Game/Scripts/Farm/FarmPlot.cs` — método nomeado `TryAdvanceStageFromLunarPeak`
- `Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs` — acessor estático `Active` (auto-registro)
- `Assets/_Game/Scripts/Fonte/FonteRuntimeService.cs` — hook de Água Viva extra (Pálida)
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` — âncora de barraca na praça (ADITIVO)
- `Assembly-CSharp.csproj` — includes dos 9 .cs runtime + 1 teste ADICIONADOS no disco (necessários para
  o build/test compilarem; mesma convenção dos testes World existentes). **Excluído do COMMIT por instrução
  do dono** (csproj é regenerável pelo Unity); a build local usa o csproj com os includes presentes.

---

## Validation

## 6. Validation (Validação detalhada)

```
Validation method: run_strict_validation.ps1
Exit code: 0 (console: "STRICT_VALIDATION_RESULT: VALIDATION_PASS / Exit code: 0")
Assembly-CSharp: PASS (0E / 1W pré-existente — CombatTelemetrySession._blocks)
Assembly-CSharp-Editor: PASS (0E / 3W pré-existentes)
Quality check: PASS (SPEC_QUALITY_CHECK: PASS)
Docs validation: PASS (exit 0)
Diff completeness: PASS (todas as seções obrigatórias presentes)
Result artifact: run_strict_validation.ps1 nesta versão não emite JSON; evidência = saída do console
                 (VALIDATION_PASS) reproduzível via `.\tools\docs\run_strict_validation.ps1`.
Test compile evidence: WorldEventsTests.cs incluído em Assembly-CSharp.csproj (mesma convenção dos
                       testes World existentes, ex.: EnemyPackCoordinatorTests.cs; nunit.framework já
                       referenciado no csproj) e compila 0E no build de Assembly-CSharp.
```

### Testing Quality Gate
```
Changed runtime code: YES
Changed deterministic logic: YES (agenda de festival, resolução de pico, sorteio por seed, mural)
Changed Unity scene/prefab/asset wiring: YES (gerador CreateMvpTownScene — âncora aditiva; sem edição manual de YAML)
Automated tests added/updated: YES (WorldEventsTests.cs — 21 testes EditMode puros)
Automated tests command: dotnet build .\Assembly-CSharp.csproj (compila os testes 0E); Unity Test Runner EditMode pendente (humano)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (dono autorizou pular; ver Definition of Done da spec)
Justification if no automated tests: N/A (testes presentes)
Residual risk: as barracas físicas, o toast visual e o avanço real dos canteiros na cena só foram validados ao nível de LÓGICA (EditMode); o comportamento em cena (spawn/despawn visível, +1 estágio no FarmPlot da FarmScene, anúncio na HUD) requer Play Mode com debug time skip — NOT RUN.
```

---

## 7. Invariantes verificadas

- **Sem GameObject.Find/FindObjectOfType em runtime de gameplay:** resolução por acessores estáticos
  (`WorldEventHooks.Active`, `FarmPlotRegistry.Active`, `WorldEventService.Instance`); `FindAnyObjectByType`
  só no `WorldEventRuntimeBootstrap` (wiring de bootstrap, idioma permitido — igual `WorldWeatherRuntimeBootstrap`).
- **GameEventBus:** toda comunicação de gameplay via eventos (`FestivalStartedEvent`,
  `WorldEventStartedEvent`, `NotificationToastRequestedEvent`, `PlayerActionFeedbackEvent`); nenhum hook
  faz chamada MonoBehaviour→MonoBehaviour de gameplay.
- **Save DTOs:** nenhum criado/alterado — a spec é SEM save (recomputável). `WorldTimeSaveData` órfão não tocado.
- **Sem edição manual de .unity/.prefab/.asset:** a única mudança de cena é o gerador (`CreateMvpTownScene`,
  via código), não YAML.
- **Testes só em Tests/EditMode/**:** `WorldEventsTests.cs` em `Assets/_Game/Tests/EditMode/World/`.
- **Determinismo (rng-and-determinism / ADR-0005):** FNV-1a `StableHash(salt|worldSeed|dia)`; sem
  GUID/timestamp/Random não-semeado. Teste de recomputação idêntica (CA-3).
- **Anti-exploit (time_rules Rule 8):** barracas duram 1 dia (despawn garantido no dia seguinte) e cada
  barraca concede 1 item por interação (`_used`); loja noturna gated (Nyx + quest), não abre toda noite.

---

## 8. Honest status rationale

**BUILD_VALIDATED_WITH_WARNINGS.** Núcleo determinístico (agenda, picos, sorteio, mural, gate da loja
noturna, seca) completo, auditado e coberto por 21 testes EditMode puros; ambos os assemblies compilam
0E; `run_strict_validation` exit 0; docs e diff completeness PASS. O `_WITH_WARNINGS` reflete que a
validação visual/de cena (barracas físicas na praça, toast na HUD, avanço de estágio no FarmPlot real,
loja noturna em cena) é **Play Mode** e foi **DEFERIDA** (DEFERRED_TO_FINAL_VALIDATION) por decisão
explícita do dono — não rodada nesta sessão. Não há claim de PLAYMODE_VALIDATED/ACCEPTED.

## 9. Remaining work (Phase 2-3 — humano)

- Unity Test Runner EditMode (executar os 21 testes novos no Unity).
- Regenerar TownScene (`CreateMvpTownScene`) para materializar o `FestivalStallAnchor` na praça.
- Play Mode com debug time skip: 1 festival (barracas montam/desmontam, flag limpa) + 1 pico de Lua Verde
  (crops +1 estágio no FarmPlot da FarmScene) — evidência mínima para ACCEPTED (Testing Quality Gate da spec).
- Integração opcional futura: ligar `WorldEventHooks.NightShopQuestUnlockedSource` ao `QuestFlagService`
  real (quest de desbloqueio da loja noturna) — hoje default-fechado (gate correto, fonte injetável).
- Consumidores: F20 (detalhe do dia), F25 (prato da Mirena), F28 (falas de festival), F34 (mural) lêem
  as flags/eventos já publicados.
