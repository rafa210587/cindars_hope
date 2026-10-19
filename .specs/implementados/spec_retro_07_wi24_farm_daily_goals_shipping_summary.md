# Retro-Spec 07 — WI-24 Farm Loop Depth: Metas Diárias + Resumo de Vendas

> **Spec ID:** `spec_retro_07_wi24_farm_daily_goals_shipping_summary`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec
> **Domínio:** Farm / Economia / Save
> **Código que documenta:**
> - `Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalService.cs`
> - `Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalDefinition.cs`
> - `Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalState.cs`
> - `Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalsSaveData.cs`
> - `Assets/_Game/Scripts/Farm/Runtime/ShippingSummaryService.cs`
> - `Assets/_Game/Scripts/Farm/Runtime/FarmLoopFeedbackBridge.cs`
> - `Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalRuntimeBootstrap.cs`
> **Evidência de execução:** `docs/validation/WAVE_INTEGRATION_24_FARM_LOOP_DEPTH_execution_report.md` (BUILD_VALIDATED_FARM_LOOP_DEPTH_READY_PENDING_HUMAN_PLAYMODE) + matrizes `WAVE_INTEGRATION_24_DAILY_GOAL_MATRIX.md`, `WAVE_INTEGRATION_24_SAVE_LOAD_FARM_DAILY_GOAL_MATRIX.md`
> **Supersedida/complementada por:** `fable_13` (save debt closure — o WIRING do `FarmDailyGoalsSaveData` no SaveManager veio dessa spec; a WI-24 entregou apenas Capture/Restore + DTO, com SAVE_LOAD_DAILY_GOAL_DEBT declarado), `fable_20` (HUD de calendário onde metas podem ganhar UI).

---

# /speckit.specify

## Contexto

O loop de farm (plantar → regar → crescer → colher → vender) já funcionava desde as WAVE04–07, mas sem objetivos de curto prazo nem feedback de venda legível. A WAVE_INTEGRATION_24 adicionou profundidade leve: metas diárias resetadas a cada dia, resumo de vendas no HUD e pontes de feedback — tudo 100% por GameEventBus, sem tocar nos sistemas de farm existentes.

## Comportamento implementado

### 1. Metas diárias (`FarmDailyGoalService`)

- Singleton MonoBehaviour (`Instance` estático; duplicatas se autodestroem), garantido em runtime por `FarmDailyGoalRuntimeBootstrap` (`RuntimeInitializeOnLoadMethod(AfterSceneLoad)`: se nenhum `FarmDailyGoalService` existir na cena — `FindAnyObjectByType`, permitido como wiring de bootstrap — cria GameObject DontDestroyOnLoad).
- **Catálogo fixo (2 metas, definidas em código):**

| GoalId | DisplayName | RequiredProgress |
|---|---|---|
| `daily_goal_first_harvest` | Primeira colheita do dia | 1 |
| `daily_goal_sell_first_crop` | Vender primeiro item colhido | 1 |

  (`FarmDailyGoalDefinition`: GoalId, DisplayName, RequiredProgress; recompensa diferida — `DAILY_GOAL_REWARD_DEFERRED`.)
- **Gatilhos (subscriptions GameEventBus):**
  - `CropHarvestedEvent` → +1 em `daily_goal_first_harvest`;
  - `EconomyTransactionCompletedEvent` com `WasSuccessful` e (`TransactionType` ∈ {"sell", "Sell", "shipping"} OU `GoldDelta > 0`) → +1 em `daily_goal_sell_first_crop`;
  - `DayStartedEvent` → `_currentDay = evt.DayNumber` + reset de todas as metas (progress 0, Completed/Claimed false, Day atualizado).
- **Progresso (`AddProgress`)**: meta já `Completed` ignora (idempotência intra-dia); publica `DailyGoalProgressedEvent(goalId, current, required)` a cada incremento; ao atingir `RequiredProgress` marca `Completed` e publica `DailyGoalCompletedEvent(goalId)`.
- **Leitura para UI**: `GetCurrentGoals() : IReadOnlyList<FarmDailyGoalState>` (snapshot).

### 2. Persistência (DTOs + Capture/Restore)

- `FarmDailyGoalState` (`[Serializable]`, JsonUtility-safe): `GoalId, Day, CurrentProgress, RequiredProgress, Completed, Claimed` — apenas tipos simples, zero refs Unity.
- `FarmDailyGoalsSaveData`: `List<FarmDailyGoalState> Goals`.
- `CaptureSaveData()` — cópia profunda dos estados atuais.
- `RestoreFromSaveData(data)` — restaura apenas metas com GoalId conhecido (ignora vazios/desconhecidos); idempotente (não duplica progresso após load).
- **NOTA de autoridade**: a WI-24 entregou os DTOs e as APIs; o campo `DailyGoals` no `GameSaveData` e a chamada Capture/Restore dentro do `SaveManager` foram fechados pela **fable_13** (na WI-24 isso ficou declarado como `SAVE_LOAD_DAILY_GOAL_DEBT`). Citar fable_13; não re-documentar o wiring aqui.

### 3. Resumo de vendas (`ShippingSummaryService`)

- Subscreve `EconomyTransactionCompletedEvent`; para transações bem-sucedidas com `GoldDelta > 0` publica `PlayerActionFeedbackEvent("Vendido: {item}[ xN] por {gold}g", 3s)`.
- ItemId vazio → "item"; quantidade só aparece quando `Amount > 1`.

### 4. Ponte de feedback do loop (`FarmLoopFeedbackBridge`)

- `CropHarvestedEvent` → `PlayerActionFeedbackEvent("Colheita: {item}[ xN] adicionado ao inventário.", 2.5s)`.
- `DailyGoalCompletedEvent` → `"Meta diária concluída!"` (4s).
- `DailyGoalProgressedEvent` com `Current < Required` → `"Meta [{goalId}]: {atual}/{requerido}"` (2s) — progresso parcial apenas; a conclusão tem mensagem própria.
- Todos os toasts chegam ao player via `GameplayFeedbackService` (WI-23 — retro-spec 06).

## Critérios de aceite (verificáveis no código atual)

1. Primeira colheita do dia completa `daily_goal_first_harvest`; colheitas seguintes no mesmo dia não re-disparam conclusão.
2. Primeira venda bem-sucedida (sell/shipping/GoldDelta>0) completa `daily_goal_sell_first_crop`.
3. `DayStartedEvent` reseta progress/Completed/Claimed e atualiza `Day` em ambas as metas.
4. Cada venda com ouro positivo gera toast "Vendido: ... por Ng".
5. `CaptureSaveData`/`RestoreFromSaveData` fazem round-trip sem duplicar progresso e ignoram GoalIds desconhecidos.
6. Nenhuma chamada direta entre sistemas: todas as integrações via GameEventBus; DTOs sem referências Unity.

---

# /speckit.plan

## Arquitetura real

| Arquivo | Responsabilidade |
|---|---|
| `FarmDailyGoalService.cs` | Estado e regras das metas diárias; eventos de progresso/conclusão; Capture/Restore |
| `FarmDailyGoalDefinition.cs` | Definição imutável (catálogo em código) |
| `FarmDailyGoalState.cs` | Estado serializável por meta (DTO) |
| `FarmDailyGoalsSaveData.cs` | DTO raiz da seção de save |
| `ShippingSummaryService.cs` | Venda → toast legível |
| `FarmLoopFeedbackBridge.cs` | Colheita/metas → toasts |
| `FarmDailyGoalRuntimeBootstrap.cs` | Garantia de instância em runtime (padrão *RuntimeBootstrap) |

## Contratos

- Eventos consumidos: `CropHarvestedEvent`, `EconomyTransactionCompletedEvent`, `DayStartedEvent`.
- Eventos publicados: `DailyGoalProgressedEvent(goalId, current, required)`, `DailyGoalCompletedEvent(goalId)`, `PlayerActionFeedbackEvent`.
- `FarmDailyGoalService.Instance`, `GetCurrentGoals()`, `CaptureSaveData()`, `RestoreFromSaveData(FarmDailyGoalsSaveData)`.
- IDs estáveis: `daily_goal_first_harvest`, `daily_goal_sell_first_crop`.
- Save: DTOs JsonUtility-compatíveis; wiring no SaveManager é contrato da fable_13.

## Decisões e invariantes

- **Event-bus only**: serviços de farm não conhecem EconomyManager/InventoryManager/HUD — apenas eventos (regra unity-architecture §2).
- **Save DTO simple types only** (regra unity-architecture §3): estados persistem por GoalId; resolução pós-load por dicionário interno.
- **Idempotência**: meta Completed não acumula; Restore não re-publica eventos.
- **Recompensas diferidas** (`DAILY_GOAL_REWARD_DEFERRED`): metas são feedback/ritmo, não fonte de economia, até a fase de balance final.
- **Detecção de venda tolerante**: aceita variantes "sell"/"Sell"/"shipping" e fallback `GoldDelta > 0` (acoplamento fraco ao formato do TransactionType).

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero)

1. Criar os 3 DTOs (`FarmDailyGoalDefinition`, `FarmDailyGoalState`, `FarmDailyGoalsSaveData`) com os campos exatos.
2. Criar `FarmDailyGoalService` singleton com o catálogo de 2 metas, as 3 subscriptions, `AddProgress` idempotente e os 2 eventos de saída.
3. Criar `FarmDailyGoalRuntimeBootstrap` (RuntimeInitializeOnLoadMethod + FindAnyObjectByType guard + DontDestroyOnLoad).
4. Criar `ShippingSummaryService` e `FarmLoopFeedbackBridge` com as mensagens/durações exatas.
5. Implementar `CaptureSaveData`/`RestoreFromSaveData` e integrar ao SaveManager conforme fable_13 (seção `DailyGoals` no `GameSaveData`).
6. Validar com `ValidateWave24FarmLoopDepth` + checklist humano WI-24.

## Débitos conhecidos

- Recompensas de meta (gold/itens) diferidas; campo `Claimed` existe mas nada o seta hoje.
- Catálogo de metas hard-coded (2 metas); expansão futura deve migrar para dados.
- UI dedicada de metas inexistente (apenas toasts); candidata a fable_14/20.
- Testes EditMode do serviço não foram criados na WI-24 (validação por checklist humano + matrizes); ao reconstruir, cobrir reset diário, idempotência e round-trip de save.
