# Skills — fase mecânica 2B: movimentos, posture e provocação melee

> **Spec ID:** spec_skills_12_melee_movement_posture_v1  
> **Status:** IMPLEMENTADA E VALIDADA — 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime / Skills+Combat+Enemy / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_11_melee_equipment_shapes_v1  
> **Blocks:** spec_skills_13_ranged_identity_v1 e UI  
> **Repo lock scope/Validation/Executor:** Ownership / Unity+EditMode+PlayMode / execute-spec-strict

# /speckit.specify

## Spec

Dar identidades distintas a Avanço de Aço, Arrancada, Salto, Grito e Quebra-Guarda. Toda movimentação usa
`PlayerMovementDisplacementResolver`, para em obstáculo e nunca teleporta através de parede.

- **AC01:** Avanço: 12/15/18, 2,5 tiles, arco 110° e +25% posture somente em alvo em recovery.
- **AC02:** Arrancada: 8/10/12, 3 tiles, atravessa inimigo leve mas não parede.
- **AC03:** Salto: 14/17/20, ponto válido, impacto raio 1,4; alvo inválido recusa sem custo.
- **AC04:** Grito: 6/7/8, raio 2,2, taunt 3/4/5 s em inimigo simples; elite reduz, boss só recebe estabilidade em janela.
- **AC05:** Quebra-Guarda: 16/20/24, posture×3; bloqueio soma 25% posture e quebra publica a critical window existente.
- **AC06:** somente cancelamento/falha antes do commit restitui custo e evita cooldown. Whiff, colisão ou interrupção depois do commit mantêm custo/cooldown; nenhuma skill altera cave snapshot.
- **AC07:** controle forte usa DR compartilhada por alvo: 100/60/30% nas três aplicações elegíveis e imunidade de 4 s depois da terceira. Grito usa a duração já reduzida de elite; boss só recebe controle em janela explicitamente elegível. A regra migra para `docs/game_rules/combat_rules.md`.

Fora: boss disable, arte/animação e capstone.

# /speckit.plan

CREATE `MeleeMovementProfileResolver.cs` (`TryResolveDestination(...)`), `EnemySkillReactionAdapter.cs`
(`ApplyTaunt`, `IsInRecovery`, `IsBlocking`) e `ControlDiminishingReturnsState.cs`. MODIFY executor melee
para delegar deslocamento e reações; reusar `EnemyPostureState`, `EnemyBrain`,
`PlayerMovementDisplacementResolver` e eventos existentes.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeMovementProfileResolver.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/Effects/EnemySkillReactionAdapter.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/DefaultSkillActionCatalog.cs`
- `Assets/_Game/Scripts/Enemy/EnemyBrain.cs`
- `Assets/_Game/Scripts/Combat/EnemyPostureState.cs`
- `Assets/_Game/Scripts/Combat/ControlDiminishingReturnsState.cs` (CREATE)
- `docs/game_rules/combat_rules.md`
- testes novos `Assets/_Game/Tests/EditMode/Skills/MeleeMovementPostureTests.cs` e `Assets/_Game/Tests/PlayMode/Composition/SkillMeleeMovementPlayModeTests.cs`

Edge cases: displacement concorrente, parede fina, alvo morre durante windup, elite/boss e callback após
caster destruído. A alteração de `EnemyBrain` limita-se a uma API explícita de prioridade temporária.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Resolver destinos/cancelamentos determinísticos | AC01–AC03, AC06 |
| [x] | T02 | Expor adapter mínimo de recovery/block/taunt e DR compartilhada | AC01, AC04, AC05, AC07 |
| [x] | T03 | Integrar cinco actionIds sem números duplicados | AC01–AC07 |
| [x] | T04 | EditMode de fórmulas/transições e PlayMode com obstáculos/elite/boss/DR | AC01–AC07 |
| [x] | T05 | Validar `failed=0` e revisão de não regressão | AC01–AC07 |
