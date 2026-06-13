# SPEC — Combate: Telemetria de Play Mode (TTK, stamina, janelas) vs Alvos de Balance

> **Spec ID:** `fable_59_spec_combat_telemetry_playmode`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P3
> **Type:** Runtime / Tooling
> **Domain:** Combat / Balance
> **Parallelizable:** YES (lock próprio Combat/Telemetry/** — só consome eventos)
> **Parallel group:** fable_batch10_telemetry (grupo próprio)
> **Can run with:** F54, F55, F56, F57, F58, F60
> **Must not run with:** N/A (nenhuma spec toca Combat/Telemetry/**)
> **Repo lock scope:** `Assets/_Game/Scripts/Combat/Telemetry/**` (novo), DebugHud (linha aditiva)
> **Depends on:**
> - F02 (executada — eventos de dano/combate publicados)
> - F24 (E13 — moves/elite affixes: roster final de eventos de inimigo)
> - F33 (E21 — bestiário 60 criaturas: bandas/tier por enemyId)
> **Blocks:** N/A (consumido pelos checkpoints M2/M3 do plano mestre)
> **Scope:** CombatTelemetryService (toggle debug) coletando métricas por sessão e relatório JSON local por nível da caverna com comparação contra os alvos de balance.
> **Out of scope:** telemetria online/upload, auto-tuning de balance, métricas fora de combate (forrageio/economia), UI rica de dashboards.

required_adrs: []
required_game_rules: [combat_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

COMBAT_CORE PARTE Q (§53) manda registrar em playtest: "TTK por inimigo/papel, Stamina
gasta por categoria, Stamina recuperada, HP perdido, MP gasto, número de Dodges/Dashes,
tempo segurando Block, Block impacts, hits em MinorOpening/CriticalWindow/CoreExposed,
consumíveis usados, deaths/retreats..." — e fixa a regra: usar telemetria para detectar
extremos (trivial, injusto, esponja, sem counterplay), nunca chutar. A pendência da
mesma direction é literal: "Definir Play Mode telemetry para TTK/Stamina/HP/MP/movement/
dash distance".

BALANCE_CURVES é quem dá os ALVOS: §6 TTK (Comum 3-6s, Elite 12-20s, Miniboss 45-90s,
Boss 2-4min), §7 dano recebido (comum 4-8% do HP máx, elite 10-15%, boss telegrafado
18-30%), §8 a tabela de dano esperado por banda — e a PARTE G declara a dívida que esta
spec paga: "Telemetria de Play Mode (Combat Core §53) para validar §6-§8 nas bandas
41+". A PARTE F é vinculante no método: "TTK e dano recebido como ALVOS de telemetria —
desvio ajusta criatura, não curva".

O plano mestre consome isto nos checkpoints M2/M3 (validação humana por lote): sem
relatório objetivo, a validação de combate é opinião.

## Problema

Hoje não existe NENHUMA medição: TTK, dano e stamina só podem ser avaliados "no olho",
o que torna impossível validar as fichas do bestiário (F33) contra a tabela §8 e
detectar os extremos que a direction proíbe. Sem coleta passiva por eventos, a
alternativa seria instrumentar cada sistema de combate na mão — invasivo, frágil e
inaceitável a esta altura da fila. E se as métricas vazarem para o save, viram lixo
persistente (telemetria é descartável por definição).

## Objetivo

Ao final desta spec, deve existir um `CombatTelemetryService` (host bootstrap, OFF por
default, ligado por toggle debug) que coleta passivamente via GameEventBus, por sessão e
por nível da caverna: TTK por enemyId (spawn/primeiro-dano→morte), dano dado/recebido
(por fonte), stamina gasta, dodges/blocks/tempo de block/perfect blocks, quebras de
postura e deaths; ao sair do nível (ou da caverna), grava um relatório JSON local FORA
do save (`persistentDataPath/telemetry/`) com as métricas agregadas E a comparação
contra os alvos (TTK §6 por papel da criatura via banda do bestiário; dano recebido §7
em % do HP máx), marcando cada métrica como WITHIN_TARGET / BELOW / ABOVE — pronto para
os checkpoints M2/M3.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md (PARTE Q §53-54)
docs/design/gameplay/combat/BALANCE_CURVES_DIRECTION_v1.0.md (PARTE C §6-8, PARTE F/G)
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md (TTK/janelas)
.claude/rules/testing-quality-gate.md
.claude/skills/event-bus-pattern/SKILL.md
.claude/skills/editmode-test-authoring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- GameEventBus + eventos de combate (F02/F27 e cadeia de inimigos):
  EnemySpawnedEvent, EnemyDamagedEvent, EnemyKilledEvent, DamageAppliedEvent,
  DamageBlockedEvent, PlayerDamagedEvent, PlayerPerfectBlockEvent,
  EnemyPostureBrokenEvent, PlayerChargedAttackEvent, PlayerDodgeStartedEvent/
  PlayerDodgeEndedEvent, StaminaChangedEvent, HPChangedEvent, ManaChangedEvent,
  PlayerDiedEvent, CaveLevelEnteredEvent, CaveExitedEvent,
  VulnerabilityWindowStartedEvent/EndedEvent;
- bestiário F33 (enemyId → banda/tier/papel — fonte dos alvos por criatura);
- DebugHud (linha aditiva de status da telemetria);
- padrão de toggle debug do projeto (auditar: como CaveDebugLevelSkipController liga).
Não existe:
- qualquer coleta/relatório de métricas de combate.
Auditar Fase 0:
- shape real dos eventos (campos disponíveis: enemyId? amount? sourceId?) — a coleta
  usa SÓ o que os eventos carregam (faltou campo = registrar gap no report, NÃO
  alterar evento de gameplay nesta spec);
- mapeamento enemyId → papel/banda (registry do bestiário F33);
- evento de block segurado (tempo de block): existe? senão derivar de
  DamageBlockedEvent count e registrar gap;
- toggle debug canônico do projeto.
```

## Engineering stories

```text
Como validador humano (M2/M3), quero um JSON por nível com TTK/dano/stamina comparados
  aos alvos, para aprovar balance com evidência e não com impressão.
Como tuning de bestiário, quero TTK por enemyId marcado WITHIN/BELOW/ABOVE, para saber
  QUAL criatura ajustar (desvio ajusta criatura, não curva — PARTE F).
Como arquitetura, quero coleta 100% passiva por eventos, para a telemetria não tocar
  nenhum sistema de combate.
Como save, quero telemetria FORA do GameSaveData, para métricas nunca virarem estado.
```

## Escopo

```text
Inclui:
- CombatTelemetryService (host bootstrap; OFF por default; toggle debug para ligar):
  assina eventos de combate e agrega em memória por sessão e por nível da caverna;
- métricas v1 (POR NÍVEL e total da sessão):
  - TTK por enemyId: timestamp do primeiro dano sofrido pela criatura → morte
    (média/mín/máx/n); kills sem dano registrado descartados (gap logado);
  - dano dado (total, por tipo se o evento carregar) e dano recebido (total, por
    fonte; cada hit também em % do HP máx no momento);
  - stamina gasta (deltas negativos de StaminaChangedEvent durante combate);
  - dodges (count), blocks (DamageBlockedEvent count), perfect blocks (count),
    quebras de postura (count), charged attacks (count);
  - deaths (PlayerDiedEvent) e MP gasto (deltas de ManaChangedEvent);
- TelemetryTargetEvaluator (classe pura): compara métricas com alvos — TTK §6 pelo
  papel/banda da criatura (bestiário F33), dano recebido §7 por papel — e marca
  WITHIN_TARGET/BELOW/ABOVE com os limites citados;
- relatório: JSON local em Application.persistentDataPath/telemetry/
  (combat_<data>_<runId>_level<N>.json) escrito em CaveExitedEvent/transição de nível;
  NUNCA no GameSaveData; writer tolerante a falha de IO (log, nunca exceção);
- DebugHud: 1 linha aditiva ("Telemetry: ON — level 12, 8 kills");
- EditMode tests: agregação de TTK com eventos sintéticos, % de dano recebido,
  avaliador de alvos (WITHIN/BELOW/ABOVE nas bordas), reset por nível, serialização
  do relatório (shape estável), coleta OFF por default (zero efeito desligada).
```

## Fora de escopo

```text
Não inclui:
- upload/telemetria online ou agregação entre sessões;
- auto-tuning/ajuste automático de criaturas;
- métricas de §53 sem evento disponível (dash distance, body block, velocidade média,
  consumíveis) — registradas como gaps nominais no report p/ follow-up;
- UI/dashboard além da linha no DebugHud;
- alteração de QUALQUER evento de gameplay para enriquecer a coleta (gap documentado).
```

## Regras de não duplicação

```text
Coleta SOMENTE via GameEventBus — zero hook direto em sistemas de combate.
Alvos de balance citados de BALANCE_CURVES §6-8 — não inventar números novos.
Banda/papel por enemyId vem do registry do bestiário (F33) — não criar tabela paralela.
Relatório fora do save — nunca estender GameSaveData para métricas.
```

## Critérios de aceite

### CA-1 Coleta passiva com toggle

- OFF por default (zero assinaturas/efeito); ligando o toggle debug, o serviço assina
  e agrega; desligando, para e descarta.
- Evidência: EditMode tests de estado OFF/ON com eventos sintéticos.

### CA-2 TTK por inimigo correto

- Sequência sintética (spawn → primeiro dano t0 → morte t1) produz TTK = t1-t0 por
  enemyId com média/mín/máx/n corretos em múltiplos kills.
- Evidência: EditMode tests de agregação.

### CA-3 Comparação com alvos do balance

- O avaliador marca TTK e dano recebido como WITHIN_TARGET/BELOW/ABOVE usando os
  limites §6-§7 pelo papel/banda do bestiário (testes nas bordas: 2.9s/3s/6s/6.1s
  para comum etc.).
- Evidência: EditMode tests do TelemetryTargetEvaluator.

### CA-4 Relatório JSON local fora do save

- Sair do nível grava JSON com shape estável (versão do shape no payload) em
  persistentDataPath/telemetry/; GameSaveData não muda; falha de IO não quebra o jogo.
- Evidência: EditMode test de serialização do shape + diff sem mudança de save.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/Telemetry/
  CombatTelemetryService.cs   (NOVO — host bootstrap; toggle; assinaturas; agregação)
  CombatTelemetrySession.cs   (NOVO — classe pura: estado agregado por nível/sessão)
  TelemetryTargetEvaluator.cs (NOVO — classe pura: alvos §6-8 → WITHIN/BELOW/ABOVE)
  CombatTelemetryReport.cs    (NOVO — DTO do JSON: shapeVersion, runId, level,
                               métricas, avaliações)
  CombatTelemetryWriter.cs    (NOVO — JSON em persistentDataPath/telemetry/; IO
                               tolerante a falha)
DebugHud                      (1 linha aditiva)
Assets/_Game/Tests/EditMode/Combat/CombatTelemetryTests.cs (NOVO)
docs/validation/fable_59_spec_combat_telemetry_playmode_execution_report.md
```

## Contratos

### Data contracts

- `CombatTelemetryReport` (JSON, shapeVersion=1): runId, caveLevel, banda,
  durationSeconds, kills[] {enemyId, papel, ttkAvg/Min/Max, n, evaluation},
  damageDealtTotal, damageTakenTotal, damageTakenPctAvg {byRole, evaluation},
  staminaSpent, mpSpent, dodges, blocks, perfectBlocks, postureBreaks,
  chargedAttacks, deaths, gaps[] (métricas §53 sem evento disponível).
- Alvos (constantes citadas): TTK comum 3-6s, elite 12-20s, miniboss 45-90s,
  boss 120-240s; dano recebido comum 4-8%, elite 10-15%, boss telegrafado 18-30%.

### Runtime contracts

- `CombatTelemetryService`: toggle debug; Subscribe/Unsubscribe simétricos; delega
  TUDO a CombatTelemetrySession (pura, testável); flush em
  CaveExitedEvent/CaveLevelEnteredEvent (fecha o nível anterior).
- `TelemetryTargetEvaluator`: (papel, métrica) → evaluation; resolve papel/banda via
  registry do bestiário (F33) injetado como lookup (interface fina p/ teste).
- Writer: nunca lança; falha de IO = log de aviso e descarte.

### Event contracts

- Adds: NENHUM evento novo. Consome ~15 eventos existentes (lista no Estado atual).
- Requires unsubscribe pattern: YES (simétrico, e total quando toggle OFF).

### Save contracts

- NENHUMA mudança: relatórios vivem em persistentDataPath/telemetry/, fora do
  GameSaveData; nada da telemetria é restaurado em load.

### UI contracts

- 1 linha no DebugHud (status ON/OFF, nível, kills). Sem tela nova.

## Sistemas afetados

```text
Combat/Telemetry (domínio novo — lock próprio)
DebugHud (linha aditiva)
Nenhum sistema de combate modificado (consumo passivo)
Checkpoints M2/M3 (consumidores do relatório — processo, não código)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/Telemetry/** (tudo novo)
Assets/_Game/Scripts/UI/DebugHud.cs (1 linha aditiva)
host de bootstrap (wiring aditivo — arquivo auditado na Fase 0)
Assets/_Game/Tests/EditMode/Combat/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML
Packages/** ; ProjectSettings/**
QUALQUER evento de gameplay (Core/Events/**) — gaps são documentados, não corrigidos aqui
sistemas de combate (EnemyHealth/PlayerAttack/Stamina/...) — coleta 100% passiva
SaveManager/GameSaveData (telemetria jamais persiste no save)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Shape real dos eventos (campos disponíveis); registry enemyId→papel/banda (F33);
toggle debug canônico; lista de gaps §53 (sem evento) para o report.

### Fase 1 — Sessão e agregação
CombatTelemetrySession (pura) + agregação de TTK/dano/stamina/contadores + testes
sintéticos.

### Fase 2 — Alvos e relatório
TelemetryTargetEvaluator (§6-8, bordas testadas) + CombatTelemetryReport + Writer
(IO tolerante) + testes de shape.

### Fase 3 — Serviço e fechamento
CombatTelemetryService (toggle, assinaturas, flush por nível) + linha no DebugHud +
csproj; run_strict_validation; execution report (com a lista de gaps §53).
```

## Paralelização

- Parallelizable: YES.
- Parallel group: fable_batch10_telemetry (grupo próprio).
- Can run with: F54, F55, F56, F57, F58, F60.
- Must not run with: N/A (nenhuma spec toca Combat/Telemetry/**; DebugHud é linha
  aditiva — coordenar merge com F15-era consumers se houver).
- Shared files/systems that require lock: Combat/Telemetry/** (novo), DebugHud.
- Reason: domínio novo, consumo passivo de eventos; depende de F24/F33 apenas para
  dados (roster de eventos/bandas), não para arquivos.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
Regra crítica: relatórios SÓ em persistentDataPath/telemetry/ — fora do save.
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: YES (~15 assinaturas; total quando OFF)
```

## Impacto em UI/Unity

```text
Changes UI: 1 linha no DebugHud
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (lote final — relatório real de uma run)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION (consumido em M2/M3)
```

## Riscos técnicos

```text
Risco: eventos sem os campos necessários (TTK sem enemyId, dano sem fonte).
Mitigação: Fase 0 audita o shape; coleta usa só o que existe; faltas viram gaps
nominais no report (follow-up enriquece eventos em spec própria) — esta spec NÃO
altera eventos.

Risco: custo de runtime com telemetria ligada (GC/alocação por hit).
Mitigação: agregação incremental sem alocação por evento (structs/contadores);
OFF por default; toggle debug.

Risco: relatório escrito no meio de transição perder o nível.
Mitigação: flush em CaveLevelEnteredEvent (fecha anterior) E CaveExitedEvent;
teste de reset por nível.

Risco: avaliador errar papel/banda (alvo errado por criatura).
Mitigação: lookup do bestiário F33 injetado + testes nas bordas por papel.

Risco: IO falhar (disco/permissão) e quebrar a run.
Mitigação: writer try/catch com log; teste de falha simulada.
```

## Rollback

```text
Remover wiring + pasta Telemetry/** = jogo idêntico ao atual (coleta era passiva).
Arquivos JSON locais são descartáveis. Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar shape dos eventos, registry F33, toggle debug, gaps §53.
- [ ] T002 — CombatTelemetrySession + agregação (TTK/dano/stamina/contadores) +
        testes sintéticos.
- [ ] T003 — TelemetryTargetEvaluator (§6-8) + Report/Writer + testes de bordas/shape.
- [ ] T004 — CombatTelemetryService (toggle/flush) + DebugHud + csproj;
        run_strict_validation; execution report com gaps documentados.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (agregação, avaliador de alvos, shape do relatório)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — gerar 1
  relatório real e anexar aos checkpoints M2/M3)
- Requires regression test: YES (OFF por default = zero efeito; diff sem tocar combate)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + 1 relatório JSON real de uma run
  com avaliações WITHIN/BELOW/ABOVE legíveis

## Definition of Done

```text
CombatTelemetryService passivo com toggle (OFF default); TTK por enemyId, dano
dado/recebido (+% HP), stamina/MP, dodges/blocks/perfects/posture/deaths agregados por
nível; comparação com alvos §6-8 por papel/banda (F33); relatório JSON versionado em
persistentDataPath/telemetry/ (fora do save); gaps §53 documentados; zero mudança em
eventos/sistemas de combate; builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
Nenhum sistema de combate modificado (coleta 100% passiva — diff é a prova).
Nenhum evento novo/alterado; unsubscribe simétrico; OFF default = zero assinaturas.
GameSaveData inalterado (telemetria jamais no save).
Alvos citados de BALANCE_CURVES §6-8 (nenhum número inventado).
Zero GameObject.Find em runtime; eventos só via GameEventBus.
```
