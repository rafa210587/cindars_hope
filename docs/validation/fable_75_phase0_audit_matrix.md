# fable_75 — Phase 0 Audit Matrix

**Spec:** `fable_75_spec_skill_effects_real_integration`
**Date:** 2026-06-21

## Achado central: discrepância de IDs

A spec lista IDs como `skill_melee_heavy_strike_2`, `skill_melee_bleed_edge`, etc. Esses IDs
**não existem** no `DefaultSkillCatalog` atual. O WI-11 adicionou 14 nodes com IDs diferentes
(notação ponto: `melee.avanco_aco`, etc.). Esta matrix usa os IDs reais do repo.

## Estado dos 14 nodes WI-11 (EffectId → executor registrado)

| EffectId | Executor atual | Status | Dormant? |
|----------|---------------|--------|----------|
| `melee.avanco_aco` | `MeleeStrikeSkillEffectExecutor` | REAL ✅ | não |
| `melee.grito_desafio` | `MeleeStrikeSkillEffectExecutor` | REAL ✅ | não |
| `melee.investida_quebra_guarda` | `MeleeStrikeSkillEffectExecutor` | REAL ✅ | não |
| `magic.chama_breve` | `ProjectileSkillEffectExecutor` | REAL ✅ | não |
| `magic.rajada_gelida` | `ProjectileSkillEffectExecutor` | REAL ✅ | não |
| `crafting.bomba_improvisada` | `ProjectileSkillEffectExecutor` | REAL ✅ | não |
| `survival.kit_emergencia` | `SelfRestoreSkillEffectExecutor` | REAL ✅ | não |
| `survival.instinto_sobrevivencia` | `SelfRestoreSkillEffectExecutor` | REAL ✅ | não |
| `survival.campo_seguro` | `SelfRestoreSkillEffectExecutor` | REAL ✅ | não |
| `survival.sinal_retirada` | `FeedbackOnlySkillEffectExecutor` | DORMANT | sim |
| `survival.isca_improvisada` | `FeedbackOnlySkillEffectExecutor` | DORMANT | sim |
| `crafting.irrigador_portatil` | `FeedbackOnlySkillEffectExecutor` | DORMANT | sim |
| `crafting.mecanismo_campo` | `FeedbackOnlySkillEffectExecutor` | DORMANT | sim |
| `crafting.marca_eficiencia` | `FeedbackOnlySkillEffectExecutor` | DORMANT | sim |

**9/14 já têm executor real.** 5/14 permanecem FeedbackOnly por design intencional.

## Justificativa para nodes dormantes (5)

Confirmado via `SKILL_NUMERIC_ADDENDUM_v1.0.md` §1.4/1.5 e `DefaultSkillCatalog.DormantActiveNodeIds`:
- `survival.sinal_retirada` — sistema de utilidade/marcação pendente (spec futura)
- `survival.isca_improvisada` — sistema de isca/trap pendente (spec futura)
- `crafting.irrigador_portatil` — farm utility pendente (spec futura)
- `crafting.mecanismo_campo` — sistema de campo/mecanismo pendente (spec futura)
- `crafting.marca_eficiencia` — modificador de eficiência pendente (spec futura)

Per regra `spec_quality_gate` e condição de stop da spec: "Sistema alvo não expõe API de
modificador → reportar débito". Estes 5 são débito documentado; não são regressão.

## FarmCrop stamina TODO

`FarmCropSkillEffectExecutor` tinha TODO `TODO_INTEGRATION_NOT_FINAL: Stamina cost from caster
is not yet deducted here`. **Implementado nesta spec**: `TrySpendStamina(WaterSkillStaminaCost=10)`
antes de regar, com guard para caster null.

## APIs auditadas

| Sistema | API real | Especificado na spec | Match? |
|---------|---------|----------------------|--------|
| `StaminaManager` | `TrySpendStamina(int)` returns `bool` | `SpendStamina(float)`, `HasStamina(float)` | DIVERGE — spec usava API errada; usamos a real |
| `FarmPlot` | `CanBeWatered` property, `TryWaterViaSkill()` | correto | OK |
| `SkillEffectContext` | sem campo `SkillNode` | "tem SkillNode" | DIVERGE — custo definido como `const` no executor |

## Decisão sobre ordem de checagem

Stamina verificada APÓS `CanBeWatered` para não debitar stamina se o canteiro não pode ser
regado (falha sem custo). Guard de caster null: skip deduction em slice mode.
