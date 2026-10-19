# spec_arch_enemybrain_debug_telemetry_v19

Status: Implementado e BUILD_VALIDADO.

## Escopo

Recorte incremental da Fase E (EnemyBrain) do plano de modularização (`docs/architecture/CLAUDE_MODULARIZATION_REMAINING_PLAN.md`).

Objetivo: extrair a telemetria de diagnóstico (logs de combate/aviso) do `EnemyBrain` para um colaborador dedicado, sem alterar dano, cooldown, range, stun, posture, movimento, seleção de ação, loot, cave scaling, saves, cenas, prefabs, IDs ou balanceamento.

## Mudança implementada

- Criado `EnemyDebugTelemetry` (`Assets/_Game/Scripts/Enemy/EnemyDebugTelemetry.cs`), classe C# pura instanciada por `EnemyBrain` (`private readonly EnemyDebugTelemetry _telemetry = new EnemyDebugTelemetry();`).
- Migrados para o colaborador, preservando texto e semântica byte-a-byte:
  - `LogThreatExpiredOnce(enemyId, packId, context)` — log one-shot `"CombatLog: EnemyThreatExpired. EnemyId=..., PackId=..."`, com o guard `_threatExpiredLogged` movido para dentro do colaborador. `EnemyBrain` chama `_telemetry.ResetThreatExpiredLog()` nos três pontos onde o guard era resetado (`OnEnable`, reengajamento em `EvaluateState`, `OnPackAlert`).
  - `LogBossSwapActionSetMissing(enemyId, actionSetId, context)` — `Debug.LogWarning("CombatLog: BossSwapActionSetMissing. EnemyId=..., ActionSetId=...")`.
  - `LogEliteWardedResisted(enemyId, context)` — `CombatLog.Log("CombatLog: EliteWardedResistedStatus. EnemyId=..., Affix=Warded.")`.
  - `LogEliteVolatileExploding(enemyId, damage, playerMaxHp, telegraphSeconds, context)` — `CombatLog.Log("CombatLog: EliteVolatileExploding. EnemyId=..., Damage=..., CapMaxHp=..., Telegraph=...s.")`.
  - `LogEnemyPackLeashReset(enemyId, packId, anchor, context)` — `CombatLog.Log("CombatLog: EnemyPackLeashReset. EnemyId=..., PackId=..., Anchor=(...,...).")`.
- `EnemyBrain.cs` passou a chamar esses métodos no lugar dos logs inline; o método privado `LogThreatExpiredOnce()` (sem parâmetros) foi removido do brain.
- Registrado `Assets\_Game\Scripts\Enemy\EnemyDebugTelemetry.cs` em `CindarsHope.Runtime.csproj` (lista explícita de `<Compile Include>`; não há glob nesse csproj).

## Desvio deliberado do pedido original (reportado, não silencioso)

O pedido de execução citava `AnnouncePackEngagementOnce` como exemplo de método a extrair para telemetria. Após ler o código real, esse método **não é um log** — ele chama `_packCoordinator.Alert(...)`, ou seja, é lógica de gameplay real (acordar o pack quando o inimigo detecta o alvo), guardada por `_packEngagedAnnounced` que também controla comportamento (evita alertar o pack mais de uma vez por engajamento), não apenas suprimir log repetido. Mover isso para uma classe chamada "telemetria de diagnóstico" misturaria responsabilidade de gameplay real dentro de um colaborador que deveria ser só relato, e contradiz a regra de não mudar gameplay/gameplay-design-patterns. `AnnouncePackEngagementOnce` e a flag `_packEngagedAnnounced` **permaneceram no `EnemyBrain`** sem alteração.

Pela mesma razão, os guards `_wardedStatusConsumed` (Warded) e `_volatileExploded` (Volatile) **não foram movidos** — eles decidem comportamento real (resistir ao primeiro status; explodir só uma vez), o log é uma consequência colateral desse estado, não a razão de existir do guard. Apenas a chamada de log nesses dois pontos foi extraída para o colaborador (stateless), preservando texto e local de disparo; o estado de gameplay continua no brain.

O log `CombatLog: EnemyPackLeashReset` (em `ResetToAnchorAndHeal`) não é one-shot (dispara toda vez que o pack reseta o leash), mas é puramente diagnóstico — foi incluído na extração por consistência de responsabilidade (telemetria de diagnóstico do brain), sem exigir estado adicional.

## Não-regressão

- Nenhum texto de log foi alterado (comparação byte-a-byte contra o original).
- Semântica one-shot preservada: `EnemyThreatExpired` continua logando só uma vez por janela de engajamento (reset nos mesmos três pontos de antes).
- `BossSwapActionSetMissing`, `EliteWardedResistedStatus`, `EliteVolatileExploding`, `EnemyPackLeashReset` continuam disparando exatamente nos mesmos pontos do fluxo, com os mesmos parâmetros e o mesmo contexto (`this`, o `GameObject` do inimigo).
- Nenhuma mudança em dano, cooldown, range, stun, posture, movimento, seleção de ação, loot, cave scaling, elite affixes, pack coordination, saves, cenas, prefabs, IDs ou balanceamento.
- Sem `FindObjectOfType`/`GameObject.Find` novos; sem namespace proibido.
- Nenhum par mútuo novo: `EnemyDebugTelemetry` só referencia `CindarsHope.Combat.CombatLog` (Enemy já era mutuamente dependente de Combat antes deste corte).
- Usings de `EnemyBrain.cs` conferidos: `using CindarsHope.Combat;` continua em uso (via `CindarsHope.Combat.EnemyHealth _health`), nenhum using ficou órfão.

## Evidência

- `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
- `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS.
  - Results: `TestResults/enemybrain-debug-telemetry-editmode.xml`
  - Log: `Logs/enemybrain-debug-telemetry.log`
- `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - Antes: `RuntimeModuleEdges=227`, `MutualModulePairs=38`.
  - Depois: `RuntimeModuleEdges=227`, `MutualModulePairs=38`.
  - Sem par mútuo novo; sem ciclo novo.

## Testing Quality Gate

Mudança é refactor de extração puro (mesma lógica movida, sem comportamento novo); coberta por EditMode existente (2747/2747 PASS) mais o build 7/7. Não há EditMode dedicado a mensagens de log (CombatLog é gated por `Verbose`, default off, não asserted em teste). Residual risk: nenhum teste automatizado cobre o texto exato dos logs; risco mitigado por comparação manual byte-a-byte no diff e por `CombatLog`/`Debug.LogWarning` serem chamados nos mesmos pontos de código, com os mesmos argumentos.

## Pendências

Fase E do plano ainda tem itens anteriores na lista de extração (`EnemyTargetingPolicy`, `EnemyMovementStateMachine`, `EnemyActionSelectionStrategy`, `EnemyCombatContext`, `EnemyBrainConfigurationResolver`) — a maior parte já foi coberta por specs anteriores (`EnemyTargetingController`, `EnemyBrainTuningResolver`, `EnemyActionSelectionStrategy`, `EnemyBrainConfigurationPolicy`); `EnemyDebugTelemetry` fecha o último item nomeado da lista da Fase E.
