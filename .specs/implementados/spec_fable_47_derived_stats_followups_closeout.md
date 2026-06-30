# SPEC — Player: Fechamento dos Follow-ups de Derived Stats (Speed Composer, Craft/Repair, Duração de Status)

> **Spec ID:** `fable_47_spec_derived_stats_followups_closeout`
> **Status:** A implementar
> **Wave:** FABLE Batch 9
> **Priority:** P2
> **Type:** Runtime
> **Domain:** Player / Combat
> **Parallelizable:** NO
> **Parallel group:** N/A (PlayerController/movement lock)
> **Can run with:** specs sem player movement/combat/craft
> **Must not run with:** qualquer spec que toque PlayerController, movement controllers, PlayerStatusReceiver, CraftingRuntime ou EquipmentManager (ex.: F16/F18 re-execuções, F27, F39)
> **Repo lock scope:** `Player/PlayerController.cs`, `Player/Movement/**`, `Player/Conditions/**`, `Combat/StatusEffect/PlayerStatusReceiver.cs`, `Craft/CraftingRuntime.cs`, `Equipment/EquipmentManager.cs`
> **Depends on:**
> - `fable_18_spec_derived_stats_vitals_application_runtime` (EXECUTADA — origem dos 3 follow-ups)
> - `fable_01_spec_status_effects_canonical_set_runtime` (EXECUTADA — set canônico Poison/Chill)
> - `fable_16_spec_player_fatigue_sleep_collapse_wiring` (EXECUTADA — fadiga escreve em SpeedMultiplier)
> **Blocks:** N/A (fecha débito; nada novo depende)
> **Scope:** fechar os 3 follow-ups registrados no report da F18 — MoveSpeed derivado via compositor de fatores nomeados, hooks de CraftTimeReduction/RepairEfficiencyBonus e duração de status no player reduzida por resistência.
> **Out of scope:** novos derived stats, rebalance de fórmulas F18, mudanças no DerivedStatsCalculator, status de inimigos.

required_adrs: []
required_game_rules: [combat_rules.md]

---

# /speckit.specify

## Contexto

O report `docs/validation/fable_18_spec_derived_stats_vitals_application_runtime_execution_report.md`
registra 3 FOLLOW-UPs explícitos e autorizados pela própria spec F18:
(1) **MoveSpeed derivado no PlayerController** — não aplicado porque `SpeedMultiplier` é um
mutável compartilhado escrito por 5 sistemas (block, dash/displacement, fadiga/exhausted,
status, e agora derived) e "compor mais um fator permanente sem refactor de composição é
risco de regressão"; (2) **CraftTimeReduction/RepairEfficiencyBonus** — hooks invasivos em
CraftingRuntime/EquipmentManager deferidos; (3) **duração de status no player reduzida por
resistência** — Poison/Chill encurtados, exigia acoplar PlayerStatusReceiver ao applier.
A auditoria do repo confirma os escritores diretos de `SpeedMultiplier`:
`PlayerMovementDisplacementResolver` (0f/restore), `PlayerMovementAbilityController`
(0f dash/restore), `PlayerBlockController` (×fator/restore), `PlayerConditionService`
(×ExhaustedSpeedMultiplier on/off) e `PlayerStatusReceiver` (×fator de status/restore) —
todos com o padrão frágil "guardar valor anterior e restaurar".

## Problema

O padrão atual de escrita direta é uma corrida: dois sistemas ativos simultaneamente
(ex.: block durante exhausted) restauram valores um do outro fora de ordem e o multiplicador
fica errado permanentemente. Adicionar o fator derived da F18 por cima multiplicaria o bug.
Sem os hooks de craft/repair, dois derived stats calculados pela F18 não fazem NADA — stat
morto visível na UI. Sem duração por resistência, resistências só reduzem dano, contrariando
a direction de derived attributes (resistência também encurta efeitos).

## Objetivo

Ao final desta spec, deve existir um `PlayerSpeedComposer` com fatores nomeados que
substitui TODAS as escritas diretas em `SpeedMultiplier` (com testes de caracterização do
comportamento atual ANTES do refactor), o fator permanente `DerivedMoveSpeed` (F18) deve
estar aplicado, `CraftTimeReduction`/`RepairEfficiencyBonus` devem ter efeito mensurável em
CraftingRuntime/EquipmentManager, e a duração de status no player deve usar
`duração × (1 − min(0.5, resist × 0.02))` — sem mudar nenhum valor de fórmula da F18.

## Fontes obrigatórias lidas

```text
docs/validation/fable_18_spec_derived_stats_vitals_application_runtime_execution_report.md (§FOLLOW-UPs)
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md (semântica dos stats)
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- PlayerController.SpeedMultiplier (get/set público; consumido no FixedUpdate junto com
  GetHungerMoveSpeedModifier — fome fica FORA do composer, caminho próprio preservado);
- 5 escritores diretos confirmados: PlayerMovementDisplacementResolver (linhas ~53/73),
  PlayerMovementAbilityController (~80/123/149), PlayerBlockController (~132/162),
  PlayerConditionService (~299/303), PlayerStatusReceiver (~125-134);
- DerivedStatsCalculator + PlayerVitalsApplier (F18 — fonte única; expõe MoveSpeed derivado
  e resistências); PlayerCombatStatsProvider (F02);
- CraftingRuntime (Craft/), EquipmentManager (Equipment/) — pontos de hook;
- PlayerStatusReceiver: DurationTurns=segundos, clamp 1-30 (F01).
Não existe:
- composer de fatores, fator derived aplicado, hooks craft/repair, duração por resistência.
Auditar Fase 0: TODOS os call sites de SpeedMultiplier (leitura e escrita — inclusive
EnemyBrain tem propriedade própria homônima: NÃO tocar); fluxo real de duração de job no
CraftingRuntime e de reparo no EquipmentManager; como o applier F18 expõe resistências.
```

## Engineering stories

```text
Como PlayerController, quero velocidade = produto de fatores nomeados, não um mutável disputado.
Como block+exhausted simultâneos, quero remover meu fator sem corromper o do outro.
Como jogador com CraftTimeReduction, quero jobs de craft mensuravelmente mais rápidos.
Como jogador com resistência a veneno, quero Poison mais curto, não só mais fraco.
Como mantenedor, quero testes de caracterização que provem que o refactor não mudou nada.
```

## Escopo

```text
Inclui:
- testes de CARACTERIZAÇÃO antes do refactor: matriz de cenários do comportamento atual
  (block on/off, dash, displacement, exhausted on/off, status slow apply/expire, e pares
  simultâneos) capturando o valor efetivo de SpeedMultiplier — rodam contra o código velho
  e o novo (paridade, exceto bugs de corrida documentados como correção);
- PlayerSpeedComposer (puro, testável): SetFactor(SpeedFactorKind, float),
  ClearFactor(kind), Value = produto clampado [0, +] com floor MinSpeedFloor para fatores
  de status (preserva regra F01); kinds: Displacement, Dash, Block, Exhausted, Status,
  DerivedMoveSpeed; hospedado no PlayerController (campo + SpeedMultiplier vira leitura do
  composer; setter público mantido por compat → escreve fator Legacy com warning DEV — ou
  removido se a Fase 0 provar zero call sites externos restantes);
- migração dos 5 escritores para SetFactor/ClearFactor (sem snapshot/restore manual);
- fator DerivedMoveSpeed: PlayerVitalsApplier (F18) aplica o MoveSpeed derivado no composer
  na mesma invalidação por evento de equipamento (fecha follow-up 1);
- hooks craft/repair (fecha follow-up 2): CraftingRuntime aplica duração efetiva =
  duraçãoBase × (1 − CraftTimeReduction) no início do job (clamp documentado);
  EquipmentManager aplica RepairEfficiencyBonus no ponto único de reparo (mais durabilidade
  restaurada por reparo — fórmula linear simples, constante nomeada);
- duração de status por resistência (fecha follow-up 3): no PlayerStatusReceiver,
  duração final = duraçãoBase × (1 − min(0.5, resist × 0.02)), usando a resistência do
  eixo correto via fonte F18 (Poison→Toxic, Chill→Ice/Cold; mapeamento já canônico no
  receiver F18), mantendo clamp 1-30s do F01;
- EditMode tests: composer (produto, clear restaura, fatores simultâneos, floor), paridade
  de caracterização, fator derived aplicado/re-aplicado por evento, fórmula de craft/repair,
  fórmula de duração (resist 0 / 10 / 25 / 50+ → cap 50%).
```

## Fora de escopo

```text
Não inclui: mudar fórmulas/valores do DerivedStatsCalculator (F18); GetHungerMoveSpeedModifier
(fome permanece fora do composer); SpeedMultiplier do EnemyBrain (propriedade própria de
inimigo — intocada); novos derived stats; UI de stats; rebalance de block/dash/fadiga.
```

## Regras de não duplicação

```text
Não criar segundo calculator/applier — consumir F18 (fonte única).
Não criar segundo caminho de velocidade — composer é O caminho; escrita direta morre.
Não duplicar mapeamento DamageType→resistência — reutilizar o do receiver F18.
Não criar hook de craft paralelo — efeito no fluxo de job existente do CraftingRuntime.
```

## Critérios de aceite

### CA-1 Composer sem regressão
- Todos os cenários de caracterização passam (paridade com comportamento atual); cenários
  de corrida (block+exhausted simultâneos, status expirando durante dash) ficam CORRETOS
  no composer (cada fator removido independentemente) e o report documenta a diferença.
- Evidência: CharacterizationTests + SpeedComposerTests.

### CA-2 MoveSpeed derivado vivo
- Equipar item com MoveSpeed derivado altera a velocidade efetiva via fator DerivedMoveSpeed;
  desequipar restaura; re-aplica no mesmo evento de invalidação F18.
- Evidência: teste de aplicação por evento + cenário humano.

### CA-3 Craft/Repair mensuráveis
- CraftTimeReduction reduz a duração efetiva do job (teste com fórmula exata);
  RepairEfficiencyBonus aumenta durabilidade restaurada por reparo (teste).
- Evidência: testes de fórmula nos pontos únicos.

### CA-4 Duração por resistência
- Poison com resist Toxic 10 dura ×0.8; com resist 25 dura ×0.5; com resist 50 dura ×0.5
  (cap); Chill idem no eixo Ice; clamp 1-30s preservado.
- Evidência: testes paramétricos da fórmula.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Player/Movement/
  PlayerSpeedComposer.cs       (NOVO — puro: fatores nomeados → produto)
  SpeedFactorKind.cs           (NOVO — enum)
Assets/_Game/Scripts/Player/PlayerController.cs (SpeedMultiplier → composer)
Assets/_Game/Scripts/Player/Movement/{PlayerMovementDisplacementResolver,
  PlayerMovementAbilityController, PlayerBlockController}.cs (migração)
Assets/_Game/Scripts/Player/Conditions/PlayerConditionService.cs (migração)
Assets/_Game/Scripts/Combat/StatusEffect/PlayerStatusReceiver.cs (migração + duração por resist)
Assets/_Game/Scripts/Player/PlayerVitalsApplier.cs (fator DerivedMoveSpeed)
Assets/_Game/Scripts/Craft/CraftingRuntime.cs (CraftTimeReduction)
Assets/_Game/Scripts/Equipment/EquipmentManager.cs (RepairEfficiencyBonus)
Assets/_Game/Tests/EditMode/Player/{SpeedComposerTests,SpeedCharacterizationTests,
  DerivedFollowupsTests}.cs
docs/validation/fable_47_spec_derived_stats_followups_closeout_execution_report.md
```

## Contratos

### Data contracts
N/A novos (stats já existem no DerivedStatsCalculator F18). Constantes nomeadas:
caps de craft/repair, fator 0.02 e cap 0.5 da duração.
### Runtime contracts
PlayerSpeedComposer: SetFactor/ClearFactor/Value (puro, sem Unity); PlayerController
mantém superfície pública compatível (SpeedMultiplier legível).
### Event contracts
N/A novos — reusa a invalidação por EquipmentSlotChangedEvent (F18).
### Save contracts
N/A — nada persiste (fatores são runtime; duração de status não é salva hoje — inalterado).
### UI contracts
N/A.

## Sistemas afetados

```text
Player movement (controller + 3 movement controllers + conditions), status effects no
player, vitals applier (F18), crafting jobs, reparo de equipamento, testes EditMode.
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Player/** (controller, Movement/, Conditions/, PlayerVitalsApplier)
Assets/_Game/Scripts/Combat/StatusEffect/PlayerStatusReceiver.cs
Assets/_Game/Scripts/Craft/CraftingRuntime.cs
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Tests/EditMode/Player/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manuais ; Packages/** ; ProjectSettings/**
DerivedStatsCalculator / PlayerCombatStatsProvider (fórmulas F18/F02 intocadas)
EnemyBrain.cs (SpeedMultiplier de inimigo é OUTRA propriedade — fora do escopo)
HungerManager / GetHungerMoveSpeedModifier (caminho da fome preservado)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria: todos os call sites de SpeedMultiplier; fluxos reais de craft job e reparo; exposição de resistências do applier F18.
### Fase 1 — Testes de caracterização do comportamento ATUAL (rodam verdes antes de qualquer mudança).
### Fase 2 — PlayerSpeedComposer + testes puros; PlayerController passa a ler do composer.
### Fase 3 — Migrar os 5 escritores (um por commit lógico) mantendo caracterização verde.
### Fase 4 — Fator DerivedMoveSpeed (applier F18) + hooks craft/repair + duração por resistência + testes.
### Fase 5 — run_strict_validation + execution report (fechando os 3 follow-ups com referência ao report F18).
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: specs sem player/combat/craft
- Must not run with: F16/F18/F27/F39 e qualquer spec de movement/status/craft
- Shared files/systems that require lock: PlayerController e todos os movement controllers
- Reason: refactor transversal do caminho de velocidade do player.

## Impacto em save/load

```text
Does this change save schema? NO. Does this add a save section? NO.
Does this require migration? NO. Does this persist Unity references? N/A.
```

## Impacto em eventos

```text
Adds events: NO | Changes existing events: NO | Requires unsubscribe pattern: NO (assinaturas existentes mantidas)
```

## Impacto em UI/Unity

```text
Changes UI: NO | Changes scenes: NO | Changes prefabs: NO | Changes assets: NO
Requires Play Mode final validation: YES (feel de movimento: block/dash/fadiga/status + craft)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: refactor mudar o feel de movimento silenciosamente. Mitigação: caracterização ANTES
(CA-1) + cenário humano com os 5 sistemas.
Risco: call site esquecido escrevendo direto. Mitigação: Fase 0 lista exaustiva; setter
legado com warning DEV (ou compile error se removido) força descoberta.
Risco: duração por resistência tornar Chill irrelevante no endgame. Mitigação: cap 50%
canônico da fórmula + valores nos testes paramétricos para playtest futuro.
Risco: hook de craft colidir com modificadores existentes de job. Mitigação: aplicar no
ponto único de criação do job com teste de composição.
```

## Rollback

```text
Reverter migração devolve escritas diretas (caracterização prova equivalência); fator
derived removível isoladamente no applier; hooks de craft/repair e duração por resistência
são aditivos e removíveis um a um. Nenhum dado persistido afetado.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: call sites de SpeedMultiplier + fluxos craft/reparo + resistências F18.
- [ ] T002 — Testes de caracterização do comportamento atual (verdes pré-refactor).
- [ ] T003 — PlayerSpeedComposer + SpeedFactorKind + testes puros + leitura no controller.
- [ ] T004 — Migrar 5 escritores (displacement, dash, block, exhausted, status) mantendo paridade.
- [ ] T005 — Fator DerivedMoveSpeed no PlayerVitalsApplier + teste por evento.
- [ ] T006 — CraftTimeReduction (CraftingRuntime) + RepairEfficiencyBonus (EquipmentManager) + testes.
- [ ] T007 — Duração por resistência no PlayerStatusReceiver (fórmula + cap + clamp F01) + testes.
- [ ] T008 — csproj; run_strict_validation; execution report fechando os 3 follow-ups.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (composição de velocidade, 3 fórmulas)
- Requires EditMode tests: YES (caracterização + composer + fórmulas)
- Requires PlayMode automated or final human scenario: YES (feel de movimento + craft real)
- Requires regression test: YES (caracterização É o teste de regressão do refactor)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: caracterização verde + cenário humano cobrindo
  block durante fadiga, dash com status de slow ativo, craft acelerado e Poison encurtado

## Definition of Done

```text
Zero escritas diretas em PlayerController.SpeedMultiplier fora do composer; fator derived
aplicado; craft/repair com efeito testado; duração por resistência com cap 50%; os 3
follow-ups do report F18 referenciados como FECHADOS no novo report; builds 0E; sem claim
ACCEPTED.
```

## Anti-regressão

```text
Feel atual preservado (caracterização); fome continua fora do composer; MinSpeedFloor de
status (F01) preservado; clamp 1-30s de duração preservado; fórmulas F18 intocadas;
EnemyBrain.SpeedMultiplier intocado; nenhum sistema fica com fator órfão após morte/respawn
(clear em teardown testado).
```

## Notas para execução posterior

```text
Se a Fase 0 encontrar um 6º escritor de SpeedMultiplier não listado, incluí-lo na migração
e registrar no report. Balance fino de craft/repair/duração = playtest futuro.
```
