# WAVE INTEGRATION 11 - Skill Movement Actions Mapping

**Date:** 2026-06-09
**Status:** BUILD_VALIDATED_MOVEMENT_RUNTIME_FIX_PENDING_HUMAN_PLAYMODE
**Patch:** WAVE_INTEGRATION_11_MOVEMENT_ACTIONS_RUNTIME_FIX

---

## Non-slot movement/defensive actions

| Action | Base availability | Input | Skill tree interactions | Cost | Cooldown | Distance/rule | ImplementNow | Notes |
|---|---|---|---|---|---|---|---:|---|
| Dash | Desde tutorial/progressao inicial | Space + direction | Survival/melee_dodge_training melhora cooldown; Survival general melhora recovery | 40 Stamina | 1.0s | 3.5 tiles base | YES | PlayerDashController BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE; colisao via PlayerMovementDisplacementResolver/Collider2D.Cast; nao ocupa active slot |
| Dodge | Desde o comeco | double tap directional (< 0.25s) | Survival/melee_dodge_training melhora custo/recovery; survival.sinal_retirada reduz custo (DEFERRED_RUNTIME_EFFECT) | 40 Stamina | 0.6s | 1.5 tiles | YES | PlayerDodgeController BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE; DirectionalDoubleTapDetector; colisao via PlayerMovementDisplacementResolver/Collider2D.Cast; nao ocupa active slot |
| Block | Desde runtime basico de defesa | Left Shift | melee_guarded_block melhora absorcao futura; Melee/Guarda Firme melhora futura | 10 Stamina/s ao segurar | n/a | Slow de movimento 0.5x; reducao de dano deferred | YES | PlayerBlockController BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE; slow funcional agora; combat damage reduction deferred |

---

## Dash - Detalhes

```text
Controller: PlayerDashController.cs
Runtime binding: PlayerMovementActionRuntimeBootstrap attaches to GameBootstrap.Instance.PlayerManager.gameObject after scene load when missing
Input: Space + WASD/Arrow; falls back to LastFacingDirection if no current MoveInput
Cost: 40 Stamina (TODO_INTEGRATION_NOT_FINAL - valor final depende de refinement)
Cooldown: 1.0s (TODO_INTEGRATION_NOT_FINAL)
Distance: 3.5 tiles (base)
Cap global: ~8 tiles se Survival/Passo de Impulso aplicar bonus
Direction: facing/movement direction no momento do input
Collision: nao atravessa solidos com blocking colliders (Collider2D.Cast)
Bounds: nao sai da cena se bordas/limites tiverem colliders
Feedback: PlayerActionFeedbackEvent("Dash!")
Sem direcao/facing: PlayerActionFeedbackEvent("Dash bloqueado: nenhuma direcao disponivel.")
Stamina insuficiente: PlayerActionFeedbackEvent("Stamina insuficiente para dash.")
Cooldown ativo: PlayerActionFeedbackEvent("Dash em cooldown (N.Ns).")
```

## Dodge - Detalhes

```text
Controller: PlayerDodgeController.cs (com DirectionalDoubleTapDetector)
Runtime binding: PlayerMovementActionRuntimeBootstrap attaches to GameBootstrap.Instance.PlayerManager.gameObject after scene load when missing
Input: double tap WASD/Arrow em < 0.25s (DOUBLE_TAP_WINDOW_PENDING - configuravel)
Cost: 40 Stamina (TODO_INTEGRATION_NOT_FINAL)
Cooldown: 0.6s (TODO_INTEGRATION_NOT_FINAL)
Distance: 1.5 tiles
Direction: canonical direction from double tap
Collision: nao atravessa solidos com blocking colliders (Collider2D.Cast)
Bounds: nao sai da cena se bordas/limites tiverem colliders
Feedback: PlayerActionFeedbackEvent("Dodge!")
Stamina insuficiente: PlayerActionFeedbackEvent("Stamina insuficiente para dodge.")
Cooldown ativo: PlayerActionFeedbackEvent("Dodge em cooldown (N.Ns).")
```

## Block - Detalhes

```text
Status: BLOCK_SLOW_RUNTIME_BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE
Controller: PlayerBlockController.cs
Runtime binding: PlayerMovementActionRuntimeBootstrap attaches to GameBootstrap.Instance.PlayerManager.gameObject after scene load when missing
Input: hold Left Shift
Cost: 10 Stamina/s while held (TODO_INTEGRATION_NOT_FINAL)
Slow: PlayerController.SpeedMultiplier * 0.5 while held (TODO_INTEGRATION_NOT_FINAL)
Feedback: PlayerActionFeedbackEvent("Block.") and PlayerActionFeedbackEvent("Block released.")
Deferred: frontal damage reduction remains deferred until combat runtime/posture integration.
```

---

## Skill Tree Interactions por Action

### Dash

| Tree | SkillId | Interacao com Dash | Status |
|---|---|---|---|
| melee | melee_battle_dash | Unlock de "Arrancada de Combate" (skill de combat, nao o Dash base) | EquippableSkill separado |
| melee | melee_dodge_training | Reduz custo/cooldown de dodge (nao dash); DodgeCostReduction | PARTIAL |
| survival | survival_safe_step | MoveSpeedBonus +0.05 (afeta movimento geral, pode afetar Dash distance) | PARTIAL |

### Dodge

| Tree | SkillId | Interacao com Dodge | Status |
|---|---|---|---|
| melee | melee_dodge_training | DodgeCostReduction -0.1 (melhora custo de Dodge) | PARTIAL |
| survival | survival.sinal_retirada | Reduz custo Stamina de Dodge por buff curto | DEFERRED_RUNTIME_EFFECT |
| survival | survival_emergency_roll | Skill separado de "Rolamento de Emergencia" (EquippableSkill) | EquippableSkill separado |

### Block

| Tree | SkillId | Interacao com Block | Status |
|---|---|---|---|
| melee | melee_guarded_block | Futuro upgrade de absorcao/dano do Block; runtime basico de slow nao ocupa active slot | BLOCK_SLOW_RUNTIME_READY_DAMAGE_REDUCTION_DEFERRED |
| melee | melee_guarded_stance | DefenseFlat +1 (afeta absorcao de dano quando Block existir) | PARTIAL |

---

*Created: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)*
*Updated: 2026-06-09 (WAVE_INTEGRATION_11_MOVEMENT_ACTIONS_RUNTIME_FIX_PENDING_HUMAN_PLAYMODE)*
