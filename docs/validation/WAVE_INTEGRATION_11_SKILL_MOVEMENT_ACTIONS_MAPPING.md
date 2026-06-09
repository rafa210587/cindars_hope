# WAVE INTEGRATION 11 - Skill Movement Actions Mapping

**Date:** 2026-06-09
**Status:** BUILD_VALIDATED_RUNTIME_INPUT_FIX_PENDING_HUMAN_PLAYMODE
**Patch:** WAVE_INTEGRATION_11_RUNTIME_INPUT_FIX

---

## Non-slot movement/defensive actions

| Action | Base availability | Input | Skill tree interactions | Cost | Cooldown | Distance/rule | ImplementNow | Notes |
|---|---|---|---|---|---|---|---:|---|
| Dash | Desde tutorial/progressao inicial | Space + direction | Survival/melee_dodge_training melhora cooldown; Survival general melhora recovery | 40 Stamina | 1.0s | 3.5 tiles base | YES | PlayerDashController BUILD_VALIDATED_RUNTIME_BOUND; colisao via Collider2D.Cast; nao atravessa solidos com blocking colliders; bounds dependem de colliders de borda |
| Dodge | Desde o comeco | double tap directional (< 0.25s) | Survival/melee_dodge_training melhora custo/recovery; survival.sinal_retirada reduz custo (DEFERRED_RUNTIME_EFFECT) | 40 Stamina | 0.6s | 1.5 tiles | YES | PlayerMovementAbilityController BUILD_VALIDATED_RUNTIME_BOUND; DirectionalDoubleTapDetector; colisao via Collider2D.Cast |
| Block | Desbloqueado por Melee/melee_guarded_block rank 1 | Left Shift | melee_guarded_block melhora absorcao; Melee/Guarda Firme melhora (nao implementado) | Stamina drain ao segurar + ao receber impacto | n/a | Frontal damage reduction | NO | BLOCK_RUNTIME_DEFERRED - aguarda combat runtime; input/state pode ser preparado mas sem efeito real |

---

## Dash - Detalhes

```text
Controller: PlayerDashController.cs
Runtime binding: attached to GameBootstrap.Instance.PlayerManager.gameObject after scene load when missing
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
Controller: PlayerMovementAbilityController.cs (com DirectionalDoubleTapDetector)
Runtime binding: attached together with PlayerDashController when missing
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

## Block - Status

```text
Status: BLOCK_RUNTIME_DEFERRED_WITH_REASON
Input previsto: Left Shift
Reason: combat runtime (damage handling, shield state, posture system) nao existe nesta wave.
Unlock: melee_guarded_block rank 1 (skill catalog presente)
Effect previsto:
  - reduz dano frontal por rank
  - drena Stamina ao segurar e ao receber impacto
  - melhora com Guarda Firme
Residual risk: Block pode ser preparado como input state sem efeito real.
               Sera implementado quando CombatManager/PostureSystem existir.
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
| melee | melee_guarded_block | Unlock de Block base (EquippableSkill) | BLOCK_RUNTIME_DEFERRED |
| melee | melee_guarded_stance | DefenseFlat +1 (afeta absorcao de dano quando Block existir) | PARTIAL |

---

*Created: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)*
*Updated: 2026-06-09 (WAVE_INTEGRATION_11_RUNTIME_INPUT_FIX_PENDING_HUMAN_PLAYMODE)*
