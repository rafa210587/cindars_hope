# SPEC — Skill Effects: Substituir FeedbackOnly por Efeitos Reais (Combat + Farm)

> **Spec ID:** `fable_75_spec_skill_effects_real_integration`
> **Status:** A implementar
> **Wave:** FABLE Batch 12
> **Priority:** P2
> **Type:** Runtime / Integration
> **Domain:** Combat / Farm / Skills
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_skills_real
> **Can run with:** fable_74, fable_58, fable_71
> **Must not run with:** fable_29 (catálogo de skill migration — scope sobreposto em SkillNodeIds),
>   fable_66 (stamina de farm — escopo sobreposto em FarmPlot stamina)
> **Repo lock scope:**
>   `Assets/_Game/Scripts/Skills/Runtime/Effects/FeedbackOnlySkillEffectExecutor.cs`,
>   `Assets/_Game/Scripts/Skills/Runtime/Effects/FarmCropSkillEffectExecutor.cs`,
>   `Assets/_Game/Scripts/Skills/Runtime/Effects/` (novos executors),
>   `Assets/_Game/Scripts/Skills/Runtime/SkillEffectRegistry.cs` (ou equivalente de registro),
>   `Assets/_Game/Scripts/Skills/Runtime/SkillEffectContext.cs`
> **Depends on:**
>   - fable_01 (status effects: status_bleed, status_burn, status_slow já implementados)
>   - fable_02 (DerivedStats: BaseDamage, Cooldown usados pelas skill actions)
>   - fable_03 (equipment baselines: stamina cost por tipo de arma)
>   - fable_18 (vitais derivados: StaminaManager API confirmada)
>   - WAVE_INTEGRATION_11 (pipeline de skill effects já wired; FeedbackOnly substituível)
> **Blocks:**
>   - fable_42 (curva de progressão usa pontos de skill adquiridos — efeitos precisam existir
>     para os skill points valerem algo)
>   - fable_29 (catálogo migration pressupõe executors reais por EffectId)
> **Scope:** substituir `FeedbackOnlySkillEffectExecutor` nas 14 nodes que o usam por
> executors reais que aplicam efeito de gameplay concreto (dano bônus, status, regen,
> stamina deduction); e fechar o TODO de stamina em `FarmCropSkillEffectExecutor`.
> **Out of scope:** criação de novos nodes de skill não listados abaixo, sistemas de aura
> passiva (F29), combat telemetry (F59), áudio/VFX de skill (F58), refactor do SkillTree UI.

required_adrs: [ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [combat_rules.md, farm_rules.md, player_rules.md]

---

# /speckit.specify

## Contexto

A `WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH` adicionou 14 novos nodes ao
`DefaultSkillCatalog` e registrou todos com `FeedbackOnlySkillEffectExecutor` — executor que
sempre retorna `Success` sem aplicar efeito real, mas dispara o cooldown. O código documenta:

```csharp
// Status: DEFERRED_RUNTIME_EFFECT
// TODO_INTEGRATION_NOT_FINAL: Replace with a concrete executor when the target runtime
// system (combat, utility, or farm) is implemented.
// Blocks final acceptance: NO (authoring and pipeline are registered; effect is placeholder)
```

Além disso, `FarmCropSkillEffectExecutor.cs:9` tem:

```csharp
// TODO_INTEGRATION_NOT_FINAL: Stamina cost from caster is not yet deducted here.
// Final design should deduct stamina from the caster's StaminaManager via SkillEffectContext.
```

Os 14 nodes placeholder são (fonte: `WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH`
report e `DefaultSkillCatalog`):
- **Melee (3):** `skill_melee_heavy_strike_2`, `skill_melee_bleed_edge`, `skill_melee_posture_break`
- **Magic (2):** `skill_magic_arcane_bolt_2`, `skill_magic_fire_nova_2`
- **Survival (5):** `skill_survival_stamina_regen_2`, `skill_survival_hunger_resist`,
  `skill_survival_sprint_efficiency`, `skill_survival_poison_resist`, `skill_survival_endurance`
- **Crafting (4):** `skill_crafting_tool_efficiency`, `skill_crafting_quality_bonus`,
  `skill_crafting_recipe_unlock_2`, `skill_crafting_batch_size`

## Problema

Nodes de skill que custam pontos e têm cooldown não fazem nada. Nenhum efeito de gameplay
é aplicado — o player gasta stamina para ativar e recebe apenas o feedback toast. Isso torna
o sistema de skill tree sem valor observável para os 14 nodes mais novos, afetando tanto
combat (melee/magic) quanto farm (crafting) e survival loop (survival).

## Objetivo

Ao final desta spec, cada um dos 14 nodes executa um efeito real e mensurável:
- **Melee:** dano bônus ou aplicação de status via `PlayerDamageReceiver` / `DamageAppliedEvent`
- **Magic:** dano em AoE ou projétil bônus via `SpellCastService`
- **Survival:** modificador de regen/resist via `StaminaManager` ou `PlayerNeedsManager`
- **Crafting:** multiplicador de yield ou desconto de tempo via `CraftingRuntime`

E `FarmCropSkillEffectExecutor` deduz stamina do caster via `SkillEffectContext.Caster`.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/FeedbackOnlySkillEffectExecutor.cs
Assets/_Game/Scripts/Skills/Runtime/Effects/FarmCropSkillEffectExecutor.cs
Assets/_Game/Scripts/Skills/Runtime/SkillEffectContext.cs
Assets/_Game/Scripts/Skills/Data/DefaultSkillCatalog.cs (ou equivalente)
docs/design/gameplay/combat/SKILL_NUMERIC_ADDENDUM_v1.0.md  (dano/cd/custo dos 14 nodes)
docs/game_rules/combat_rules.md
docs/game_rules/farm_rules.md
docs/game_rules/player_rules.md
.claude/skills/player-ability-runtime/SKILL.md
.claude/rules/testing-quality-gate.md
.claude/rules/no-magic-balance-values.md
```

## Estado atual do repo

```text
FeedbackOnlySkillEffectExecutor  — retorna Success + cooldown, sem efeito real
FarmCropSkillEffectExecutor      — rega o canteiro mas não deduz stamina do caster
SkillEffectContext               — tem Caster (PlayerManager ref), TargetObject, SkillNode
StaminaManager                   — API: SpendStamina(float), HasStamina(float)
StatusEffectDatabase             — status_bleed, status_burn, status_slow (fable_01)
PlayerDamageReceiver             — ApplyDamage(DamageRequest)
SpellCastService                 — ResolveCast(SpellCastPlan); suporta Nova/Projectile
CraftingRuntime                  — ProcessJob(CraftingJob); campos: TimeMultiplier, YieldBonus
```

## Design canônico dos executors (fonte: SKILL_NUMERIC_ADDENDUM + combat_rules)

> Implementador deve ler `SKILL_NUMERIC_ADDENDUM_v1.0.md` para os valores exatos.
> Os ranges abaixo são orientação; os valores finais vêm do addendum.

| Node | Efeito real | Sistema alvo |
|------|-------------|--------------|
| `skill_melee_heavy_strike_2` | +X% dano em ataques pesados (duração do buff) | DamageAppliedEvent modifier |
| `skill_melee_bleed_edge` | Aplica status_bleed ao acertar (chance Y%) | StatusEffectDatabase |
| `skill_melee_posture_break` | Reduz threshold de posture break em Z% | PostureSystem |
| `skill_magic_arcane_bolt_2` | +X dano em arcane bolts | SpellCastService |
| `skill_magic_fire_nova_2` | +raio em fire nova | SpellCastService nova radius |
| `skill_survival_stamina_regen_2` | +X% regen de stamina por segundo | StaminaManager modifier |
| `skill_survival_hunger_resist` | -Y% drain de fome | PlayerNeedsManager |
| `skill_survival_sprint_efficiency` | -Z% custo de stamina por sprint | PlayerSpeedComposer / StaminaManager |
| `skill_survival_poison_resist` | +W% resistência a poison/venom | StatusEffectDatabase resist |
| `skill_survival_endurance` | +X MaxStamina derivado | DerivedStatsCalculator |
| `skill_crafting_tool_efficiency` | -Y% durabilidade consumida por uso de ferramenta | EquipmentManager |
| `skill_crafting_quality_bonus` | +Z% chance de qualidade superior no craftado | CraftingRuntime |
| `skill_crafting_recipe_unlock_2` | Desbloqueia receita de tier 2 sem nível | CraftingRuntime / RecipeDatabase |
| `skill_crafting_batch_size` | +1 item no output de batch craft | CraftingRuntime YieldBonus |

## Tarefas

### T1 — Audit (Phase 0)

- [ ] Confirmar IDs exatos dos 14 nodes em `DefaultSkillCatalog` (ou onde estão registrados)
- [ ] Confirmar API de cada sistema alvo (StaminaManager, CraftingRuntime, StatusEffectDatabase)
- [ ] Ler `SKILL_NUMERIC_ADDENDUM_v1.0.md` — extrair valores canônicos
- [ ] Criar `docs/validation/fable_75_phase0_audit_matrix.md`

### T2 — FarmCropSkillEffectExecutor stamina

- [ ] Em `FarmCropSkillEffectExecutor.TryExecute`: se `context.Caster` não é null e tem
      `StaminaManager`, chamar `SpendStamina(skillNode.StaminaCost)` antes da rega
- [ ] Se stamina insuficiente: retornar `SkillEffectResult.Failed("Stamina insuficiente")`
      (não regar e não consumir stamina)
- [ ] Remover TODO `TODO_INTEGRATION_NOT_FINAL: Stamina cost from caster is not yet deducted`

### T3 — Executors Melee

- [ ] `MeleeHeavyStrikeBoostExecutor`: aplica modifier temporário via `GameEventBus`
      (`SkillBuffActivatedEvent` novo ou reuso de `PlayerBuffChangedEvent` existente)
- [ ] `MeleeBleedEdgeExecutor`: subscreve `EnemyHitEvent`; aplica `status_bleed` com chance
      via `StatusEffectDatabase.TryApply`
- [ ] `MeleePostureBreakExecutor`: reduz `PostureDamageThreshold` via buffer local no
      `PlayerAttackController` (field `_postureBreakBonus`) pelo tempo ativo do skill

### T4 — Executors Magic

- [ ] `MagicArcaneBoltBoostExecutor`: adiciona `DamageBonus` ao próximo cast de `arcane_bolt`
      via context propagado ao `SpellCastService`
- [ ] `MagicFireNovaBoostExecutor`: aumenta `NovaRadius` do próximo cast de `fire_nova`
      via context propagado ao `SpellCastService`

### T5 — Executors Survival

- [ ] `SurvivalStaminaRegenBoostExecutor`: registra modificador em `StaminaManager.AddRegenModifier`
- [ ] `SurvivalHungerResistExecutor`: registra modificador em `PlayerNeedsManager.AddHungerDrainModifier`
- [ ] `SurvivalSprintEfficiencyExecutor`: registra `SpeedFactorKind.SprintEfficiency` no
      `PlayerSpeedComposer` + desconto de custo no `StaminaManager`
- [ ] `SurvivalPoisonResistExecutor`: registra resistência em `StatusEffectDatabase.SetResistance`
- [ ] `SurvivalEnduranceExecutor`: incrementa `MaxStamina` via `DerivedStatsCalculator`
      (publica `StatsRecalculatedEvent`)

### T6 — Executors Crafting

- [ ] `CraftingToolEfficiencyExecutor`: registra `DurabilityDrainModifier` no `EquipmentManager`
- [ ] `CraftingQualityBonusExecutor`: registra `QualityChanceBonus` no `CraftingRuntime`
- [ ] `CraftingRecipeUnlock2Executor`: chama `RecipeDatabase.UnlockRecipe(tier: 2)` pelos
      IDs listados no addendum
- [ ] `CraftingBatchSizeExecutor`: incrementa `YieldBonus` no `CraftingRuntime`

### T7 — Registro dos executors

- [ ] Substituir registros de `FeedbackOnlySkillEffectExecutor` pelos novos executors
      em `SkillEffectRegistry` (ou onde o pipeline os mapeia por EffectId)
- [ ] Manter `FeedbackOnlySkillEffectExecutor` para EffectIds ainda sem executor real
      (não deletar — pode haver nodes fora deste lote)

### T8 — EditMode tests

- [ ] `FarmCropStaminaDeductionTests.cs`: stamina debitada, falha se insuficiente
- [ ] `MeleeBleedEdgeExecutorTests.cs`: status_bleed aplicado com chance correta
- [ ] `SurvivalStaminaRegenBoostTests.cs`: modificador registrado e removido ao expirar
- [ ] `CraftingBatchSizeExecutorTests.cs`: YieldBonus incrementado

### T9 — Gate de build

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
.\tools\docs\validate_docs.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

## Riscos de regressão

| Risco | Mitigação |
|-------|-----------|
| Modifier de regen/resist não removido ao expirar skill | Cada executor guarda handle; remove em `OnSkillExpired` ou em `OnDisable` do bootstrap |
| FarmCrop stamina deduction quebra slice mode (sem player) | Guard: `if (context.Caster == null) skip deduction` |
| Executor novo quebra pipeline de skill existente | Não modificar `ISkillEffectExecutor` interface — só adicionar implementações |
| `CraftingRecipeUnlock2Executor` desbloqueia receita que não existe | Validar IDs contra `RecipeDatabaseSO` no Awake; logar warning se ausente |

## Critérios de aceitação

1. Os 14 nodes listados têm executor real registrado (não `FeedbackOnly`)
2. `FarmCropSkillEffectExecutor` deduz stamina; falha graciosamente se insuficiente
3. EditMode tests passam para os 4 executors testados
4. Assembly-CSharp 0E/0W; validate_docs exit 0
5. Em Play Mode: ativar `skill_melee_bleed_edge` e acertar um inimigo aplica status_bleed
   visível no DebugHud

## Stop conditions

- `SKILL_NUMERIC_ADDENDUM_v1.0.md` não existe → parar e reportar ao humano
- Sistema alvo (StaminaManager, CraftingRuntime) não expõe API de modificador → reportar débito
- Build falha → parar imediatamente

## Report obrigatório

Criar: `docs/validation/fable_75_skill_effects_real_integration_execution_report.md`

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES
Changed deterministic logic:    YES (bleed chance, stamina deduction)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        dotnet test (EditMode)
Manual Play Mode scenario:      docs/validation/fable_75_playmode_scenario.md
Justification if no tests:      N/A — lógica determinística obrigatória
Residual risk:                  executors de magic (arcane bolt/nova) dependem do contexto
                                do SpellCastService; podem precisar de ajuste em runtime
```
