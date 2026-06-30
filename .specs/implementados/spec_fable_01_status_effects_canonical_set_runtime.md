# SPEC — Status Effects: Conjunto Canônico Completo + Aplicação em Skills/Spells

> **Spec ID:** `fable_01_spec_status_effects_canonical_set_runtime`
> **Status:** BUILD_VALIDATED (executada — E06; evidência: docs/validation/fable_01_spec_status_effects_canonical_set_runtime_execution_report.md)
> **Wave:** FABLE — Gap Closure Bloco A (combate)
> **Priority:** P1
> **Type:** Runtime / Data
> **Domain:** Combat
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_A
> **Can run with:** fable_04, fable_09, fable_11, fable_12
> **Must not run with:** fable_02, fable_03, fable_06, fable_08 (consomem os contratos criados aqui)
> **Repo lock scope:** `Assets/_Game/Scripts/Combat/StatusEffect/**`, `StatusEffectDatabase`
> **Depends on:**
> - WAVE 06 (StatusEffectManager/EnemyStatusRuntimeTicker existentes)
> - slice 2026-06-12 (executores de skill com hook `statusEffectId`)
> **Blocks:**
> - `fable_02`, `fable_06`, `fable_08`
> **Scope:** completar os 10 status canônicos da direction e ligá-los a spells, skills e ações inimigas.
> **Out of scope:** balance final, VFX/arte, status em companions/pets, Corruption endgame completo.

required_adrs: []
required_game_rules: [combat_rules.md]

---

# /speckit.specify

## Contexto

`STATUS_EFFECTS_DIRECTION.md` define 10 status canônicos: Bleed, Burn, Chill, Poison, Stun,
Root, Fear, ConfusionLite, DurabilityStress e Corruption. O runtime atual
(`Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectSO.cs` + `StatusEffectManager` +
`EnemyStatusRuntimeTicker`) implementa apenas Poison, Burn, Bleed e Stun. O slice de
2026-06-12 criou executores de skill (`ProjectileSkillEffectExecutor`) com parâmetro
`statusEffectId` **não utilizado** por falta de IDs confirmados no `StatusEffectDatabase`.
Esta spec fecha a camada de status para que F02 (armas), F06 (vulnerabilidades) e F08
(spell shapes) consumam um contrato estável.

## Problema

Sem o conjunto completo: skills como `combat.ranged.bleeding_arrow` e `combat.magic.ice_bind`
prometem efeito no nome mas não aplicam nada; vulnerabilidades por status (F06) não têm alvo;
a direction de equipment (DurabilityStress) e a lore (Corruption/Pedra Negra) ficam sem âncora
mecânica. Cada spec futura inventaria seus próprios IDs e divergiria.

## Objetivo

Ao final desta spec, o projeto deve ter os 10 `StatusEffectType` canônicos com semântica
mecânica implementada no ticker, IDs estáveis registrados (`status_bleed`, `status_burn`,
`status_chill`, `status_poison`, `status_stun`, `status_root`, `status_fear`,
`status_confusion_lite`, `status_durability_stress`, `status_corruption`), gerador editor de
assets do database, e aplicação efetiva em pelo menos 4 skills e 2 ações inimigas — sem
alterar save schema.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectSO.cs (enum com 4 tipos)
- Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectManager.cs (player-side)
- Assets/_Game/Scripts/Combat/StatusEffect/EnemyStatusRuntimeTicker.cs (enemy-side DoT)
- Assets/_Game/Scripts/Combat/StatusEffect/ActiveStatusEffect.cs
- EnemyHealth.ApplyStatusEffect(StatusEffectSO)
- ProjectileBehaviour.InitializeWithStatus(...) (chance de aplicar no hit)
- GameBootstrap.StatusEffectDatabase (StatusEffectDatabaseSO com TryGetById)
- Executores de skill com parâmetro statusEffectId (slice 2026-06-12)
Parcial:
- Assets/_Game/Data/Combat/StatusEffectDatabase.asset (entradas precisam ser auditadas na Fase 0)
Não existe:
- Tipos Chill/Root/Fear/ConfusionLite/DurabilityStress/Corruption
- Aplicação de status no player por ações inimigas
- Gerador editor dos 10 assets canônicos
```

## Engineering stories

```text
Como executor de skill, quero resolver statusEffectId por ID estável para aplicar efeito no hit.
Como EnemyBrain, quero que EnemyActionSO.StatusApplicationIds funcione contra o player.
Como sistema de equipment futuro (F03), quero DurabilityStress acelerando desgaste de forma central.
Como ticker, quero semântica única por tipo (DoT, slow, root, flee, inversão leve, desgaste, corrupção).
```

## Escopo

```text
Inclui:
- estender StatusEffectType com os 6 tipos faltantes;
- semântica no EnemyStatusRuntimeTicker: Chill (multiplicador de moveSpeed), Root (velocidade 0),
  Fear (forçar estado Retreat no EnemyBrain por N segundos), ConfusionLite (inverter direção de
  movimento por N segundos), Corruption (DoT fraco + flag para sistemas futuros);
- semântica player-side no StatusEffectManager: Chill/Root afetando PlayerController,
  DurabilityStress acelerando EquipmentManager.RegisterEquipmentUsage, Stun bloqueando ataque;
- editor generator CindarsHope/Combat/Generate Canonical Status Effects (10 assets via AssetDatabase);
- wiring: ice_bind→status_chill(0.5x, 2s), bleeding_arrow→status_bleed, toxic_cloud→status_poison,
  grito_desafio→status_fear(1.5s, chance 0.5);
- 2 ações inimigas exemplares com StatusApplicationIds válidos (via gerador existente de actions);
- EditMode tests da semântica determinística (durações, multiplicadores, stacking policy).
```

## Fora de escopo

```text
Não inclui:
- VFX/SFX/ícones de status (fase de arte);
- status em companions/pets (sistemas futuros);
- Corruption endgame (purificação na Fonte — WAVE 10 hooks apenas);
- rebalance dos 4 status existentes;
- save de status ativos (transientes por design — documentar).
```

## Regras de não duplicação

```text
Não criar segundo StatusEffectManager nem segundo ticker.
Não criar enum paralelo de status — estender o existente.
Não criar caminho de aplicação fora de EnemyHealth.ApplyStatusEffect / StatusEffectManager.
```

## Critérios de aceite

### CA-1 Enum e semântica completos
- `StatusEffectType` contém os 10 tipos canônicos.
- Ticker trata cada tipo com semântica distinta e testável.
- Evidência: EditMode tests por tipo em `Assets/_Game/Tests/EditMode/Core/StatusEffectCanonicalTests.cs`.

### CA-2 IDs estáveis e gerador
- Menu editor gera/atualiza os 10 assets no `StatusEffectDatabase` com IDs `status_*` documentados.
- Evidência: log do gerador + `ValidateStatusEffectDatabase` (editor validator, 10 entradas, IDs únicos).

### CA-3 Aplicação real
- `ice_bind` aplica Chill em inimigo atingido (log CombatLog + teste de resolução de ID).
- Ação inimiga com `StatusApplicationIds` aplica status no player respeitando `StatusApplyChance`.
- Evidência: testes de resolução + cenário humano final.

### CA-4 Não regressão
- Poison/Burn/Bleed/Stun mantêm comportamento atual (testes de caracterização antes de estender).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/StatusEffect/
  StatusEffectSO.cs            (enum estendido + campos Multiplier/FlagDuration)
  EnemyStatusRuntimeTicker.cs  (semântica nova)
  StatusEffectManager.cs       (player-side nova semântica)
  PlayerStatusReceiver.cs      (NOVO — ponte ação inimiga → StatusEffectManager)
Assets/_Game/Scripts/Editor/Combat/
  GenerateCanonicalStatusEffects.cs (NOVO)
  ValidateStatusEffectDatabase.cs   (NOVO)
Assets/_Game/Tests/EditMode/Core/
  StatusEffectCanonicalTests.cs     (NOVO)
```

## Contratos

### Data contracts
`StatusEffectSO`: + `MoveSpeedMultiplier` (Chill), + `BehaviorOverrideSeconds` (Fear/ConfusionLite/Root),
+ `DurabilityWearMultiplier` (DurabilityStress). Campos com defaults neutros (1f/0f) — assets antigos intactos.

### Runtime contracts
`EnemyBrain.ApplyExternalBehaviorOverride(EnemyBrainState state, float seconds)` (NOVO, usado por Fear).
`StatusEffectManager.ApplyToPlayer(StatusEffectSO effect)` (NOVO ou auditar existente).

### Event contracts
`StatusEffectAppliedEvent(targetId, statusId)` — NOVO, publicado em ambas as direções (HUD futuro).

### Save contracts
N/A — status ativos são transientes (documentado; alinhado a SAVE_LOAD_FULL_STATE: UI/efeito volátil não persiste).

### UI contracts
N/A — feedback via PlayerActionFeedbackEvent existente.

## Sistemas afetados

```text
Combat (status), Enemy AI (Fear/override), Player (slow/root/stun), Equipment (wear hook), Event bus, Editor tooling
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/StatusEffect/**
Assets/_Game/Scripts/Enemy/EnemyBrain.cs (só método ApplyExternalBehaviorOverride)
Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs (só registros com statusEffectId)
Assets/_Game/Scripts/Core/Events/StatusEffectEvents.cs (novo)
Assets/_Game/Scripts/Editor/Combat/**
Assets/_Game/Tests/EditMode/Core/**
docs/validation/**
Assembly-CSharp.csproj / Assembly-CSharp-Editor.csproj (Compile includes)
```

## Arquivos proibidos

```text
Assets/**/*.unity, Assets/**/*.prefab
Assets/**/*.asset manual (somente via gerador AssetDatabase)
Packages/**, ProjectSettings/**
SaveManager / GameSaveData
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria: enumerar entradas atuais do StatusEffectDatabase.asset e usos de StatusEffectType.
### Fase 1 — Enum + campos novos + semântica no ticker/manager (com testes de caracterização antes).
### Fase 2 — PlayerStatusReceiver + Fear override no EnemyBrain + evento.
### Fase 3 — Gerador editor + validator + wiring dos executores de skill.
### Fase 4 — Testes EditMode + run_strict_validation + execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: fable_bloco_A
- Can run with: fable_04, fable_09, fable_11, fable_12
- Must not run with: fable_02, fable_03, fable_06, fable_08
- Shared files/systems que exigem lock: `Combat/StatusEffect/**`, `ActiveSkillExecutionController.cs`
- Reason: F02/F03/F06/F08 consomem o enum/IDs definidos aqui.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: YES (StatusEffectAppliedEvent)
Changes existing events: NO
Requires unsubscribe pattern: YES (subscribers de HUD futuros)
```

## Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: YES (via gerador editor apenas)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: Fear override conflitar com AttackWindup do EnemyBrain.
Mitigação: override só fora de AttackWindup/AttackRecover; teste de transição.
Risco: Chill/Root no player travar input permanentemente em edge case.
Mitigação: duração máxima clampada + auto-expire no Update do manager.
```

## Rollback

```text
Remover arquivos novos; reverter enum (campos novos têm default neutro, assets antigos intactos);
desfazer registros de statusEffectId nos executores.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar StatusEffectDatabase.asset e usos do enum (Fase 0).
- [ ] T002 — Testes de caracterização Poison/Burn/Bleed/Stun.
- [ ] T003 — Estender enum + campos do SO com defaults neutros.
- [ ] T004 — Semântica Chill/Root/Fear/ConfusionLite/Corruption no EnemyStatusRuntimeTicker.
- [ ] T005 — Semântica player-side (Chill/Root/Stun/DurabilityStress) + PlayerStatusReceiver.
- [ ] T006 — EnemyBrain.ApplyExternalBehaviorOverride + StatusEffectAppliedEvent.
- [ ] T007 — GenerateCanonicalStatusEffects + ValidateStatusEffectDatabase (editor).
- [ ] T008 — Wiring statusEffectId em 4 skills + 2 enemy actions.
- [ ] T009 — EditMode tests (semântica por tipo, IDs, stacking).
- [ ] T010 — csproj includes + run_strict_validation.ps1 + execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (cenário final do lote)
- Requires regression test: YES (caracterização dos 4 status atuais)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode passando + cenário humano do lote com status visíveis em combate

## Definition of Done

```text
10 tipos canônicos com semântica testada; gerador + validator criados; 4 skills e 2 enemy
actions aplicando status; builds 0E; report criado; nenhuma claim de ACCEPTED.
```

## Anti-regressão

```text
Não alterar IDs/semântica dos 4 status existentes.
Não aplicar status via caminho fora de ApplyStatusEffect/StatusEffectManager.
Não persistir status ativos no save.
Não usar GameObject.Find.
```

## Notas para execução posterior

```text
F06 usará os IDs status_* nas StatusVulnerability tags.
F03 usará DurabilityWearMultiplier.
Corruption ganha profundidade na promoção das specs WAVE 19/20 (features_futuras).
```


---

## EMENDA 2026-06-12 (pós-canonização dos catálogos — VINCULANTE)

```text
1. O conjunto canônico tem 15 status (COMBAT_CORE §31), não 10: adicionar também Slow
   (DISTINTO de Chill), HeatStress e ColdStress ao enum/ticker. Hunger e Fatigue são
   geridos pelos sistemas próprios (HungerManager/F16) — NÃO entram no ticker; documentar.
2. IDs: status_slow, status_heat_stress, status_cold_stress somam-se aos 10 já listados
   (gerador cria 13 assets; Hunger/Fatigue sem asset).
3. Fonte: STATUS_EFFECTS_DIRECTION vence em semântica; esta spec implementa o conjunto.
```
