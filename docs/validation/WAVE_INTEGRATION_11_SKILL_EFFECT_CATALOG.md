# WAVE_INTEGRATION_11 — Skill Effect Catalog

**Date:** 2026-06-08
**Status:** BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
**Last update:** spec_codex_05_skill_placeholder_debt (2026-07-03) — mapeamento legado corrigido +
ledger de débito canônico adicionado (ver seção no final do arquivo).

---

## Effect Pipeline Overview

```
Player presses 1-4
  → ActiveSkillExecutionController reads equipped skill action ID from SkillTreeManager.State
  → SkillActionToEffectId maps action ID → effect ID
  → SkillEffectRegistry.Resolve(effectId) returns executor
  → SkillTargetResolver.Resolve(targetType, caster) returns target GameObject
  → ISkillEffectExecutor.Execute(SkillEffectContext) returns SkillEffectResult
  → PlayerActionFeedbackEvent published with result message
```

---

## Registered Effects

| Effect ID | Executor Class | Category | Target Type | Description | Status |
|-----------|---------------|----------|-------------|-------------|--------|
| `farm.crop.water_skill` | `FarmCropSkillEffectExecutor` | Farm | CurrentInteractable | Waters the currently targeted FarmPlot. **spec_codex_05:** agora alcançado via `skill_crafting_irrigador_portatil` (realocado de `skill_crafting_field_patch`, mapeamento legado errado) | IMPLEMENTED |
| `melee.avanco_aco` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Avanço de Aço — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `melee.grito_desafio` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Grito de Desafio — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `melee.investida_quebra_guarda` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Investida Quebra-Guarda — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `magic.chama_breve` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Chama Breve — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `magic.rajada_gelida` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Rajada Gélida — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.sinal_retirada` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Sinal de Retirada — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.isca_improvisada` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Isca Improvisada — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.kit_emergencia` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Kit de Emergência — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.instinto_sobrevivencia` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Instinto de Sobrevivência — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `survival.campo_seguro` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Campo Seguro — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `crafting.field_patch` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Reparo de Campo — **spec_codex_05:** novo effectId honesto (antes reusava `farm.crop.water_skill` por engano); feedback + cooldown; efeito real de reparo deferred | DEFERRED_RUNTIME_EFFECT |
| `crafting.bomba_improvisada` | `FeedbackOnlySkillEffectExecutor` | Combat | None | Bomba Improvisada — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `crafting.mecanismo_campo` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Mecanismo de Campo — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |
| `crafting.marca_eficiencia` | `FeedbackOnlySkillEffectExecutor` | Utility | None | Marca de Eficiência — feedback + cooldown; efeito real deferred | DEFERRED_RUNTIME_EFFECT |

---

## Skill Action → Effect ID Mappings

Defined in `ActiveSkillExecutionController.SkillActionToEffectId` (static dictionary):

### Original WI11 mappings

**spec_codex_05 (2026-07-03):** `skill_survival_emergency_roll` foi removido do catálogo (fable_70,
skill cortada). `skill_crafting_field_patch` **não aponta mais** para `farm.crop.water_skill` — o
mapeamento "BRIDGE (debug demo)" abaixo é histórico e reflete o estado ANTES da correção. Ver
tabela "Balance Patch mappings" abaixo para o estado atual, e a seção "Skill Effect Debt Ledger"
no final deste arquivo para o critério de fechamento.

| Skill Action ID | Effect ID | Status |
|----------------|-----------|--------|
| ~~`skill_survival_emergency_roll`~~ | ~~`farm.crop.water_skill`~~ | REMOVED (fable_70 — skill cortada) |
| ~~`skill_crafting_field_patch` → `farm.crop.water_skill`~~ | — | CORRIGIDO — ver mapeamento atual abaixo (spec_codex_05) |
| `skill_melee_offhand_cut` | `combat.melee.offhand_cut` | NO_EXECUTOR (deferred) |
| `skill_melee_guarded_block` | `combat.melee.block` | NO_EXECUTOR (deferred) |
| `skill_melee_battle_dash` | `combat.melee.battle_dash` | NO_EXECUTOR (deferred) |
| `skill_melee_leap_attack` | `combat.melee.leap_attack` | NO_EXECUTOR (deferred) |
| `skill_melee_whirl_cut` | `combat.melee.whirl_cut` | NO_EXECUTOR (deferred) |
| `skill_ranged_charged_shot` | `combat.ranged.charged_shot` | NO_EXECUTOR (deferred) |
| `skill_ranged_line_piercer` | `combat.ranged.line_piercer` | NO_EXECUTOR (deferred) |
| `skill_ranged_multishot_fan` | `combat.ranged.multishot_fan` | NO_EXECUTOR (deferred) |
| `skill_ranged_bleeding_arrow` | `combat.ranged.bleeding_arrow` | NO_EXECUTOR (deferred) |
| `skill_ranged_marked_prey` | `combat.ranged.marked_prey` | NO_EXECUTOR (deferred) |
| `skill_magic_fire_spark` | `combat.magic.fire_spark` | NO_EXECUTOR (deferred) |
| `skill_magic_ice_bind` | `combat.magic.ice_bind` | NO_EXECUTOR (deferred) |
| `skill_magic_toxic_cloud` | `combat.magic.toxic_cloud` | NO_EXECUTOR (deferred) |
| `skill_magic_lightning_chain` | `combat.magic.lightning_chain` | NO_EXECUTOR (deferred) |
| `skill_magic_elemental_ward` | `combat.magic.elemental_ward` | NO_EXECUTOR (deferred) |
| `skill_magic_slowing_sigils` | `combat.magic.slowing_sigils` | NO_EXECUTOR (deferred) |

### Balance Patch mappings (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)

| Skill Action ID | Effect ID | Status |
|----------------|-----------|--------|
| `skill_melee_avanco_aco` | `melee.avanco_aco` | FEEDBACK_EXECUTOR |
| `skill_melee_grito_desafio` | `melee.grito_desafio` | FEEDBACK_EXECUTOR |
| `skill_melee_investida_quebra_guarda` | `melee.investida_quebra_guarda` | FEEDBACK_EXECUTOR |
| `skill_magic_chama_breve` | `magic.chama_breve` | FEEDBACK_EXECUTOR |
| `skill_magic_rajada_gelida` | `magic.rajada_gelida` | FEEDBACK_EXECUTOR |
| `skill_survival_sinal_retirada` | `survival.sinal_retirada` | FEEDBACK_EXECUTOR |
| `skill_survival_isca_improvisada` | `survival.isca_improvisada` | FEEDBACK_EXECUTOR |
| `skill_survival_kit_emergencia` | `survival.kit_emergencia` | FEEDBACK_EXECUTOR |
| `skill_survival_instinto_sobrevivencia` | `survival.instinto_sobrevivencia` | FEEDBACK_EXECUTOR |
| `skill_survival_campo_seguro` | `survival.campo_seguro` | FEEDBACK_EXECUTOR |
| `skill_crafting_bomba_improvisada` | `crafting.bomba_improvisada` | FEEDBACK_EXECUTOR |
| `skill_crafting_mecanismo_campo` | `crafting.mecanismo_campo` | FEEDBACK_EXECUTOR |
| `skill_crafting_marca_eficiencia` | `crafting.marca_eficiencia` | FEEDBACK_EXECUTOR |

### Current mapping — post spec_codex_05 (2026-07-03)

| Skill Action ID | Effect ID | Status |
|----------------|-----------|--------|
| `skill_crafting_field_patch` | `crafting.field_patch` | FEEDBACK_EXECUTOR — mensagem honesta de reparo (nunca mais reusa `farm.crop.water_skill`) |
| `skill_crafting_irrigador_portatil` | `farm.crop.water_skill` | REAL_EXECUTOR — realocado de `field_patch`; "irrigador" = regar, mapeamento semanticamente correto |

```text
FEEDBACK_EXECUTOR = FeedbackOnlySkillEffectExecutor registered; returns Success + feedback message;
                    active slot cooldown triggers; no gameplay effect applied.
                    TODO_INTEGRATION_NOT_FINAL
NO_EXECUTOR = EffectId mapped but no executor registered → "Efeito X sem executor. (Deferred)"
BRIDGE = debug mapping to farm.crop.water_skill for vertical slice
```

---

## Effect Contract

### ISkillEffectExecutor

```csharp
public interface ISkillEffectExecutor
{
    string EffectId { get; }
    SkillEffectCategory Category { get; }
    SkillEffectTargetType TargetType { get; }
    SkillEffectResult Execute(SkillEffectContext context);
}
```

### SkillEffectContext

Fields available to every executor:
- `SkillActionId` — the action ID from the skill tree node
- `EffectId` — the resolved effect ID
- `ActiveSlotIndex` — which slot (0-3) was pressed
- `Caster` — the player GameObject
- `Target` — resolved target (may be null)
- `WorldPosition` — target world position
- `SceneName` — current scene name
- `Time` — `Time.time` at execution

### SkillEffectResult

Factory methods:
- `SkillEffectResult.Succeeded(message, costSpent, cooldownStarted)` — `Success = true`
- `SkillEffectResult.Failed(reason, message)` — `Success = false`

---

## Extension Guide: Adding a New Effect

1. Create `YourSkillEffectExecutor.cs` implementing `ISkillEffectExecutor`
2. Set `EffectId`, `Category`, `TargetType`
3. Implement `Execute(SkillEffectContext)` — return `SkillEffectResult.Succeeded/Failed`
4. Register in `ActiveSkillExecutionController.RegisterFeedbackExecutors()` or `Bootstrap()`:
   ```csharp
   _registry.Register(new YourSkillEffectExecutor());
   ```
5. Add mapping in `SkillActionToEffectId`:
   ```csharp
   { "skill_your_action_id", "your.effect.id" }
   ```
6. Add entry to this catalog
7. Add entry to `Assembly-CSharp.csproj` `<Compile Include>` block

---

## Movement Abilities (Not Skill Slots)

Movement abilities are NOT in the active skill slot pipeline. They have dedicated controllers:

| Ability | Input | Distance | Cost | Cooldown | Controller | Status |
|---------|-------|----------|------|---------|-----------|--------|
| Dash | Space + WASD/Arrow | 3.5 tiles | 40 Stamina | 1.0s | `PlayerDashController` | BUILD_VALIDATED |
| Dodge | Double-tap WASD/Arrow | 1.5 tiles | 40 Stamina | 0.6s | `PlayerMovementAbilityController` | BUILD_VALIDATED |
| Block | Left Shift | — | Stamina drain | — | BLOCK_RUNTIME_DEFERRED | DEFERRED |

---

## Skill Effect Debt Ledger — spec_codex_05 (2026-07-03)

Ledger canônico das skills `FeedbackOnlySkillEffectExecutor` que ainda não têm sistema de apoio
real. Cada linha lista: skill ID, effectId, sistema-alvo necessário, e o **critério de fechamento**
(quando considerar a dívida quitada). Auditoria de sistema existente confirmou **nenhum** sistema de
apoio real hoje para nenhuma das 6 linhas abaixo (grep em `Assets/_Game/Scripts/Combat/**`,
`Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Craft*/**`).

| Skill ID | Effect ID | Sistema-alvo necessário | Critério de fechamento |
|---|---|---|---|
| `skill_crafting_field_patch` | `crafting.field_patch` | Reparo (durabilidade de ferramenta/equipamento ou estrutura de campo) | Fechar quando existir um alvo de reparo natural para uma skill ativa de utilidade — ex.: reusar `EquipmentDurabilityTracker.Repair(...)` no item equipado do caster, OU um sistema de reparo de estrutura de campo dedicado. Trocar `FeedbackOnlySkillEffectExecutor("crafting.field_patch", ...)` por um executor real que chama esse sistema. |
| `combat.ranged.marked_prey` (skill `skill_ranged_marked_prey`) | `combat.ranged.marked_prey` | Sistema de marcação de alvo (aggro tag + bônus de dano contra alvo marcado) | Fechar quando existir um componente `MarkedTarget`/`PreyMark` (ou equivalente) consultável pelo dano de combate; o executor real deve aplicar a marca ao `context.Target` e o pipeline de dano deve consultar essa marca para o bônus. |
| `combat.magic.elemental_ward` (skill `skill_magic_elemental_ward`) | `combat.magic.elemental_ward` | Shield/ward que absorve dano elemental | Fechar quando existir um sistema de barreira/absorção de dano (distinto dos status effects de buff/debuff atuais); o executor real deve instanciar/ativar o ward no caster com duração e capacidade de absorção. |
| `survival.sinal_retirada` (skill `skill_survival_sinal_retirada`) | `survival.sinal_retirada` | Redução de aggro em área / retirada segura | Fechar quando existir um valor de "aggro" explícito e reduzível por skill no enemy AI (hoje a detecção é direta, sem aggro acumulável); o executor real deve reduzir esse valor para inimigos próximos. |
| `survival.isca_improvisada` (skill `skill_survival_isca_improvisada`) | `survival.isca_improvisada` | Lure/isca que atrai inimigos para um ponto | Fechar quando existir um objeto `Lure`/`Bait` que o enemy AI reconhece como alvo de atração prioritário; o executor real deve instanciar esse objeto na posição do caster/mira. |
| `crafting.marca_eficiencia` (skill `skill_crafting_marca_eficiencia`) | `crafting.marca_eficiencia` | Buff temporário de velocidade/eficiência de crafting | Fechar quando o crafting runtime (`CraftingRuntime`/`CraftingJob`) expuser um hook de multiplicador de tempo de job aplicável por skill; o executor real deve aplicar esse multiplicador com duração/expiração. |

**Nota sobre `crafting.irrigador_portatil`:** removido deste ledger em 2026-07-03 (spec_codex_05) —
recebeu o executor real `FarmCropSkillEffectExecutor` (`farm.crop.water_skill`), realocado do
mapeamento legado errado de `skill_crafting_field_patch`. Não é mais dívida.

**Fora de escopo desta spec:** implementar qualquer um dos sistemas-alvo acima. Esta spec só corrige
o mapeamento legado (`field_patch` → `water_skill`) e formaliza o ledger; a implementação de cada
sistema é trabalho de uma spec de gameplay completa e dedicada.

---

*Catalog created: 2026-06-08 (WAVE_INTEGRATION_11)*
*Updated: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH — 14 new FeedbackOnlySkillEffectExecutor registrations)*
*Updated: 2026-07-03 (spec_codex_05_skill_placeholder_debt — mapeamento legado field_patch/water_skill corrigido; irrigador_portatil recebeu executor real; ledger de débito canônico com critério de fechamento adicionado)*
