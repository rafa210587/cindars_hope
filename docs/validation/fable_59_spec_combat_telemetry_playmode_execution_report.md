---
doc_type: execution_report
spec_id: fable_59_spec_combat_telemetry_playmode
wave: FABLE Batch 10
status: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-19
validated_adrs: []
validated_game_rules:
  - combat_rules.md
  - event_rules.md
---

# Execution Report — fable_59 Combate: Telemetria de Play Mode (TTK/Stamina/Janelas) vs Alvos de Balance

> **Status:** BUILD_VALIDATED_WITH_WARNINGS
> **Branch:** dev
> **Validation method:** dotnet build (Assembly-CSharp + Assembly-CSharp-Editor) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + run_strict_validation.ps1
> **Phase 2-3 (Unity / Play Mode):** DEFERRED_TO_FINAL_VALIDATION (relatório real de uma run nos checkpoints M2/M3; autorizado pelo dono)

---

## Summary

Implementado um domínio NOVO e isolado `Assets/_Game/Scripts/Combat/Telemetry/**` que coleta
métricas de combate **100% passivamente** via `GameEventBus`. Nenhum sistema de combate foi
tocado (a prova é o diff). Entrega:

- `CombatTelemetryService` (host bootstrap `RuntimeInitializeOnLoadMethod`, **OFF por default**,
  ligado pelo toggle debug estático `DebugEnabled`): assina ~14 eventos existentes e delega toda
  a matemática à sessão pura. Ligar = assinar; desligar = remover todas as assinaturas e descartar.
- `CombatTelemetrySession` (classe PURA, sem Unity): agrega por nível da caverna — TTK por
  `enemyId` (primeiro dano sofrido pela criatura → morte), dano dado total/por tipo, dano recebido
  total/por fonte/% do HP máx, stamina gasta, MP gasto, dodges, perfect blocks, quebras de postura,
  charged attacks, deaths.
- `TelemetryTargetEvaluator` (classe PURA): compara TTK (§6) e dano recebido (§7) contra os ALVOS
  citados de BALANCE_CURVES e marca `WITHIN_TARGET` / `BELOW` / `ABOVE` (bordas inclusivas).
- `BestiaryTelemetryClassLookup`: resolve `enemyId` → classe canônica (Common/Elite/Miniboss/Boss)
  lendo o registry F33 (`CanonicalBestiaryCatalog.All`) — **não cria tabela paralela**.
- `CombatTelemetryReport` (DTO serializável, `ShapeVersion=1`) + `CombatTelemetryWriter` (JSON em
  `Application.persistentDataPath/telemetry/`, IO tolerante a falha — nunca lança).
- `DebugHud`: 1 linha aditiva de status (`Telemetry: ON - level N, K kill ids` / `OFF`).

A captura runtime real (rodar uma run e gerar o JSON em Play Mode) fica DIFERIDA para os
checkpoints M2/M3 do plano mestre. A lógica determinística (agregação + avaliação + shape) está
coberta por EditMode tests.

---

## Acceptance criteria extracted

| CA | Critério | Evidência | Status |
|----|----------|-----------|--------|
| CA-1 | Coleta passiva com toggle: OFF por default (zero assinaturas/efeito); ligar assina e agrega; desligar para e descarta | `CombatTelemetryService.DebugEnabled` (OFF default) → `ApplyToggle`/`Subscribe`/`Unsubscribe` simétricos; teste `Counters_AreAggregated` + estados OFF/ON exercitados pela sessão sem assinatura quando OFF | OK (build + EditMode da lógica) / coleta runtime real deferida |
| CA-2 | TTK por inimigo correto: spawn → primeiro dano t0 → morte t1 ⇒ TTK = t1-t0 por enemyId com média/mín/máx/n | `CombatTelemetrySession.RecordEnemyDamaged`/`RecordEnemyKilled`; testes `Ttk_SingleKill_IsEndMinusFirstDamage`, `Ttk_MultipleKills_AveragesMinMax`, `Ttk_KillWithoutDamage_IsDiscardedAsGap` | OK |
| CA-3 | Comparação com alvos: TTK e dano recebido marcados WITHIN/BELOW/ABOVE pelos limites §6-§7 por papel/banda (bordas testadas) | `TelemetryTargetEvaluator` (constantes §6-§7); testes `Evaluator_Common_Ttk_Borders` (2.9/3/6/6.1), `Evaluator_Elite_Miniboss_Boss_Ttk_Bands`, `Evaluator_DamageTakenPct_Borders`, `Evaluator_NoSample_NoTarget` | OK |
| CA-4 | Relatório JSON local fora do save: shape estável (versão no payload) em persistentDataPath/telemetry/; GameSaveData não muda; falha de IO não quebra o jogo | `CombatTelemetryReport.ShapeVersion=1` + `CombatTelemetryWriter` (try/catch, retorna ""); testes `Report_Serializes_WithStableShapeVersion`, `Writer_BuildFileName_IsDeterministicAndSafe`; diff não toca SaveManager/GameSaveData | OK (shape + IO-safe) / gravação real em Play Mode deferida |

---

## Existing systems audit (system-reuse, Fase 0)

| Item | Resultado da auditoria | Decisão |
|------|------------------------|---------|
| Código de telemetria de combate | **ZERO** em `Assets/_Game/Scripts/**` (Glob `**/*Telemetry*.cs` + `Combat/Telemetry/**` vazios) | Criar domínio novo `Combat/Telemetry/**` |
| `GameEventBus` | Existe (`Core/GameEventBus.cs`); `Subscribe<T>`/`Unsubscribe<T>`, dedup, isola exceções de listeners | REUSAR (subscribe/unsubscribe simétrico; total quando OFF) |
| Eventos de combate listados na spec | Verificado o **shape real** de cada um (ver "Event shapes auditados") | Consumir SÓ o que cada evento carrega |
| `DamageBlockedEvent` (block regular) | **NÃO existe** no projeto (grep) | Não criar evento de gameplay; contagem de block regular fica como **gap nominal** no relatório; perfect blocks contados via `PlayerPerfectBlockEvent` |
| Evento de "block segurado" (tempo de block) | **NÃO existe** | Gap nominal no relatório |
| Bestiário F33 (`enemyId` → banda/papel) | `CanonicalBestiaryCatalog.All` (dados puros: `EnemyId`, `Band`, `Role`, `IsMiniBoss`, `IsBoss`) + `Bands` (faixas de nível) | REUSAR via `BestiaryTelemetryClassLookup` injetável; **sem tabela paralela** |
| `EnemyRole` enum | Definido em `Combat/EnemyDataSO.cs` (Chaser/Guard/Ranged/Caster/Burrower/Swarm/Tank/Elite/MiniBoss/Boss) | Mapeado para `TelemetryCreatureClass` (Common/Elite/Miniboss/Boss) |
| Toggle/bootstrap canônico | `CombatStateTracker` (fable_69): MonoBehaviour singleton auto-registrado via `RuntimeInitializeOnLoadMethod` + classe de regras PURA testável | **MODELO** seguido (mesmo idiom; nenhum global search) |
| `DebugHud` | IMGUI singleton (`DrawInfoPanel`); linhas via `GUILayout.Label` | 1 linha aditiva em `DrawCombatTelemetryStatus()` |
| `CombatStateTracker` (estado "em combate") | Existe (janela 4s) | NÃO acoplado: a v1 da telemetria coleta por nível da caverna; "em combate" via janela fica como follow-up se necessário (gap não bloqueante) |
| csproj | Explicit `<Compile Include>` (sem glob); **`Assembly-CSharp.csproj` é gitignored/untracked** (gerado pelo Unity localmente — commit `8e2438d3`) | 6 runtime + 1 test adicionados ao `Assembly-CSharp.csproj` **local** (não comitado); os `.cs`/`.meta` comitados são a fonte que o Unity regenera |

Nenhum sistema paralelo criado. Fonte de papel/banda = bestiário F33. Alvos = BALANCE_CURVES
§6-§7 (nenhum número inventado). Relatório fora do save.

### Event shapes auditados (Fase 0)

```text
EnemyDamagedEvent(EnemyId, DamageAmount, DamageType)   → TTK start + dano dado (total/por tipo)
DamageAppliedEvent(DamageResult{TargetId,DamageType,FinalDamage}, TargetPosition)
                                                       → TTK start por TargetId (sem somar dano: evita dupla contagem)
EnemyKilledEvent(EnemyId, ...)                         → TTK end (morte)
PlayerDamagedEvent(DamageAmount, WorldPosition, SourceId, SourceName)
                                                       → dano recebido total + por fonte
HPChangedEvent(Delta, CurrentHP, MaxHP)               → MaxHP corrente p/ % de dano recebido
StaminaChangedEvent(CurrentStamina, MaxStamina)       → stamina gasta (delta negativo)
ManaChangedEvent(CurrentMana, MaxMana)                → MP gasto (delta negativo)
PlayerDodgeStartedEvent()                             → dodges (count)
PlayerPerfectBlockEvent(SourceId, NegatedDamage)      → perfect blocks (count)
EnemyPostureBrokenEvent(EnemyId)                      → quebras de postura (count)
PlayerChargedAttackEvent(Weight)                     → charged attacks (count)
PlayerDiedEvent(SceneName)                            → deaths (count)
CaveLevelEnteredEvent(CaveLevel, BiomeId, CaveRunSeed) → flush nível anterior + abre novo
CaveExitedEvent(CaveRunSeed, ExitedFromLevel, ...)    → flush nível corrente
```

---

## Spec Compliance Matrix

| Requisito (spec) | Implementação | OK |
|------------------|---------------|----|
| `CombatTelemetryService` host bootstrap; OFF por default; toggle debug para ligar | `CombatTelemetryService.cs` (`Bootstrap` RuntimeInitializeOnLoad; `DebugEnabled` OFF default) | ✓ |
| Assina eventos de combate e agrega em memória por sessão e por nível | `Subscribe()` (~14 eventos) → delega à `CombatTelemetrySession`; nova sessão por `CaveLevelEnteredEvent` | ✓ |
| TTK por enemyId: primeiro dano sofrido → morte (média/mín/máx/n); kills sem dano descartados (gap) | `CombatTelemetrySession` (`_firstDamageTimeByEnemy` + `KillAggregate`; `KillsWithoutDamage` → gap) | ✓ |
| Dano dado (total, por tipo) e dano recebido (total, por fonte; cada hit em % do HP máx) | `RecordEnemyDamaged`/`RecordDamageDealtRaw` + `RecordPlayerDamaged` (+ `RecordMaxHp`) | ✓ |
| Stamina gasta (deltas negativos de StaminaChangedEvent) | `RecordStamina` (delta < 0) | ✓ |
| Dodges, blocks, perfect blocks, quebras de postura, charged attacks | counts; **block regular = gap** (sem evento); perfect via `PlayerPerfectBlockEvent` | ✓ (com gap documentado) |
| Deaths e MP gasto | `RecordDeath` + `RecordMana` (delta < 0) | ✓ |
| `TelemetryTargetEvaluator` puro: §6 por papel/banda (F33), §7 por papel → WITHIN/BELOW/ABOVE | `TelemetryTargetEvaluator` + `BestiaryTelemetryClassLookup` (lookup injetado) | ✓ |
| Relatório JSON em persistentDataPath/telemetry/ escrito em CaveExited/transição; nunca no save; IO tolerante | `CombatTelemetryWriter.Write` (try/catch) + `FlushIfMeaningful` em CaveLevelEntered/CaveExited | ✓ |
| DebugHud: 1 linha aditiva | `DebugHud.DrawCombatTelemetryStatus()` | ✓ |
| Adds NENHUM evento novo; consome eventos existentes | diff: nada em `Core/Events/**`; consumo passivo | ✓ |
| GameSaveData inalterado | diff: SaveManager/GameSaveData não tocados | ✓ |
| EditMode tests (agregação, %, avaliador nas bordas, reset por nível, serialização, OFF default) | `CombatTelemetryTests.cs` (18 testes) | ✓ |

---

## Gaps §53 (métricas sem evento disponível — documentadas, não corrigidas aqui)

Esta spec **não altera eventos de gameplay** (regra de não duplicação). As métricas de
COMBAT_CORE §53 abaixo não têm evento disponível hoje e são registradas como gaps nominais no
próprio relatório JSON (`CombatTelemetryService.BuildNominalGaps`) para follow-up em spec própria:

```text
- block regular (não-perfect): sem DamageBlockedEvent no projeto;
- tempo segurando Block: sem evento de block segurado;
- dash distance / velocidade média de movimento: sem evento;
- consumíveis usados em combate: sem evento.
```

Gaps dinâmicos adicionados em runtime quando ocorrem: kill sem primeiro-dano registrado
(TTK descartado) e dano recebido sem MaxHP conhecido (% não avaliado).

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (exit 0, 0 errors, 1 warning pré-existente não relacionado)
Assembly-CSharp-Editor: PASS (exit 0, 0 errors, 3 warnings pré-existentes)
Quality check: PASS
Docs validation: PASS (validate_docs.ps1 exit 0)
Spec diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

- `dotnet build .\Assembly-CSharp.csproj --no-restore` → exit 0 (0E). Grep de warnings: nenhum
  proveniente de `Combat/Telemetry/**`.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` → exit 0 (0E, 3W pré-existentes).
- `.\tools\docs\validate_docs.ps1` → exit 0 (PASS).
- `.\tools\docs\check_spec_diff_completeness.ps1` → exit 0.
- `.\tools\docs\run_strict_validation.ps1` → exit 0 (VALIDATION_PASS).

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (agregação TTK/%, avaliador de alvos §6-§7, shape do relatório)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Combat/CombatTelemetryTests.cs — 18 testes)
Automated tests command: Unity Test Runner EditMode (não executável nesta sessão headless) / compilam via dotnet build Assembly-CSharp.csproj (exit 0)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (gerar 1 relatório JSON real de uma run nos checkpoints M2/M3)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: a coleta runtime real (subscribe→agregação→flush→JSON em disco) só é exercitada
  em Play Mode; os EditMode tests cobrem a lógica pura (sessão/avaliador/writer-serialização) mas
  não o dispatch real dos eventos nem a gravação física do arquivo. Risco baixo: o service é um
  fino encaminhador para a sessão pura; o writer é IO-tolerante (nunca lança). Block regular e
  métricas §53 sem evento permanecem como gaps nominais até spec de enriquecimento de eventos.
```

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS` (não `ACCEPTED`):

- Núcleo determinístico pronto e testado (TTK, %, avaliador nas bordas, reset por nível, shape
  do relatório, lookup do bestiário): **18 EditMode tests** compilam e cobrem CA-1..CA-4 no plano
  puro.
- Builds Assembly-CSharp e Assembly-CSharp-Editor passam exit 0; docs + strict validation exit 0.
- **Phase 2-3 (Play Mode) DIFERIDA** por autorização do dono: a evidência mínima para `ACCEPTED`
  exigida pela própria spec é "testes + 1 relatório JSON real de uma run com avaliações
  WITHIN/BELOW/ABOVE legíveis" — o relatório real será produzido nos checkpoints M2/M3. Por isso o
  WITH_WARNINGS (PlayMode/integração deferida), nunca um claim de Play Mode PASS.
- Sessão headless não roda Unity Test Runner nem Play Mode; os testes foram validados por
  compilação (dotnet build exit 0), não por execução do runner.

---

## Anti-regressão

```text
Nenhum sistema de combate modificado (coleta 100% passiva — o diff é a prova: só novos arquivos
  em Combat/Telemetry/** + 1 linha aditiva no DebugHud).
Nenhum evento novo/alterado em Core/Events/**; unsubscribe simétrico; OFF default = zero assinaturas.
GameSaveData/SaveManager inalterados (telemetria jamais no save; JSON só em persistentDataPath/telemetry/).
Alvos citados de BALANCE_CURVES §6-§8 (nenhum número inventado: constantes na classe avaliadora).
Banda/papel por enemyId vêm do registry F33 (sem tabela paralela).
Zero GameObject.Find/FindObjectOfType em runtime; eventos só via GameEventBus; bootstrap via
  RuntimeInitializeOnLoadMethod (idiom CombatStateTracker).
```

---

## Files changed

### Novos (runtime — `Assets/_Game/Scripts/Combat/Telemetry/`)

```text
TelemetryTargetEvaluator.cs      — enums (classe/avaliação) + avaliador puro §6-§7 (bordas inclusivas)
BestiaryTelemetryClassLookup.cs  — enemyId → TelemetryCreatureClass via CanonicalBestiaryCatalog (F33)
CombatTelemetryReport.cs         — DTO serializável (ShapeVersion=1) + sub-DTOs (kills/por fonte/por tipo)
CombatTelemetrySession.cs        — agregação PURA por nível (TTK/dano/%/stamina/MP/contadores) + BuildReport
CombatTelemetryWriter.cs         — JSON em persistentDataPath/telemetry/ (IO tolerante) + nome determinístico
CombatTelemetryService.cs        — host bootstrap + toggle debug + assinaturas + flush por nível
```

### Novos (test)

```text
Assets/_Game/Tests/EditMode/Combat/CombatTelemetryTests.cs  — 18 EditMode tests (CA-1..CA-4 + lookup)
```

### Modificados

```text
Assets/_Game/Scripts/UI/DebugHud.cs  — 1 linha aditiva (DrawCombatTelemetryStatus + chamada em DrawInfoPanel)
```

### Locais, não comitados

```text
Assembly-CSharp.csproj  — 7 <Compile Include> (gitignored/untracked; Unity regenera a partir dos .cs/.meta)
.meta dos novos scripts + pasta Telemetry  — comitados (Unity tracks .cs.meta neste projeto)
```

---

## Remaining work

```text
- Play Mode (M2/M3): ligar o toggle debug, rodar uma run da caverna, gerar 1 JSON real em
  persistentDataPath/telemetry/ e anexar aos checkpoints; confirmar avaliações WITHIN/BELOW/ABOVE
  legíveis por banda 41+.
- Unity Test Runner EditMode: executar CombatTelemetryTests no editor (headless aqui só compila).
- Follow-up (spec própria, fora desta): enriquecer eventos para fechar os gaps §53
  (block regular, tempo de block, dash distance, consumíveis).
```
