# Skills — fase mecânica 3: ranged charge, trajetória, bleed e marca

> **Spec ID:** spec_skills_13_ranged_identity_v1  
> **Status:** IMPLEMENTADA E VALIDADA — 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime / Skills+Combat / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_12_melee_movement_posture_v1  
> **Blocks:** spec_skills_14_magic_shapes_v1 e UI  
> **Repo lock scope/Validation/Executor:** Ownership / Unity+EditMode+PlayMode / execute-spec-strict

# /speckit.specify

## Spec

- **AC01:** Disparo Carregado sustenta 0,20–1,10 s; release interpola dano 8→20, alcance 5→9, posture 0,5→1,25 e custo 12→26 STA; stagger cancela sem commit.
- **AC02:** Linha Perfurante atinge no máximo cinco antes de obstáculo, com 100/80/64/51/41% e um hit/alvo.
- **AC03:** Tiro Triplo limita o mesmo alvo a dois hits; o segundo causa 50%.
- **AC04:** Flecha Sangrante aplica Bleed e reaplicação substitui/reinicia duração sem empilhar dano.
- **AC05:** Presa Marcada custa 10 STA, seleciona deterministicamente o inimigo válido mais próximo em até 8 tiles, dura 6/8/10 s e dá +8/12/16% ranged do caster + reveal; reaplicar move a marca. O bônus vale para projéteis de skill e ataques comuns de arco desse caster.
- **AC06:** falha de alvo/recurso ou cancelamento pré-commit não cobra recurso/cooldown e publica failure key. Depois do commit, whiff/obstáculo mantêm custo e cooldown.
- **AC07:** no release, readiness e reserva de STA são atômicas e precedem a materialização do projétil; nunca existe projétil sem pagamento nem pagamento duplicado.

Fora: capstone lunar, animação, projéteis finais e input fora dos slots 1–4.

# /speckit.plan

CREATE `ChargedSkillCastState.cs`, `MarkedPreyState.cs`, `MarkedPreySkillEffectExecutor.cs` e
`RangedProjectileHitPolicy.cs`. MODIFY `ActiveSkillExecutionController.Update/TryUseSlot` para begin/hold/release
somente do actionId carregado; MODIFY `ProjectileSkillEffectExecutor` e `ProjectileSpawnRequest/Behaviour`
apenas para payload de falloff/overlap/posture. `ProjectileSpawnService` propaga o payload comum e os dois
caminhos de arco do player fornecem a identidade do caster. Reusar status database e evento de refresh.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ProjectileSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ChargedSkillCastState.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/Effects/MarkedPreyState.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/Effects/MarkedPreySkillEffectExecutor.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/Effects/RangedProjectileHitPolicy.cs` (CREATE)
- `Assets/_Game/Scripts/Combat/Weapon/ProjectileSpawnRequest.cs`
- `Assets/_Game/Scripts/Combat/Weapon/ProjectileBehaviour.cs`
- `Assets/_Game/Scripts/Combat/Weapon/ProjectileSpawnService.cs`
- `Assets/_Game/Scripts/Combat/BowArrowAttackService.cs`
- `Assets/_Game/Scripts/Combat/PlayerAttackController.cs`
- `Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs`
- `Assets/_Game/Scripts/Skills/DefaultSkillActionCatalog.cs`
- testes `SkillRangedIdentityTests.cs` e `SkillRangedIdentityPlayModeTests.cs` (CREATE)

Pseudocódigo charge: press→snapshot action/rank; hold scaled time; cancel on stagger/modal/slot change;
release→validate target/resource→reserve+commit STA→spawn→cooldown. Edge cases: tap precoce, pausa, dois colliders,
parede, target destruído e marca movida.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Estado puro de charge e cancelamento | AC01, AC06 |
| [x] | T02 | Hit policy de pierce/fan/bleed | AC02–AC04 |
| [x] | T03 | Executor e estado de marca | AC05, AC06 |
| [x] | T04 | Integrar input/controller/catalog e commit atômico sem duplicar tuning | AC01–AC07 |
| [x] | T05 | EditMode + PlayMode single/linha/fan/obstáculo/whiff | AC01–AC07 |
| [x] | T06 | Validar com `failed=0` e fechar | AC01–AC07 |

## Evidence

- Relatório: `docs/validation/skills_sdd_v1/mechanics/PHASE_03_RANGED_IDENTITY_REPORT.md`
- Catálogo: PASS, 66 nós / 5 árvores / 31 ações.
- EditMode Skills: 74/74 PASS.
- PlayMode integrado: 30/30 PASS; foco ranged final: 5/5 PASS.
- Revisão independente: WARN não bloqueante; AC01–AC07 aprovados. Risco residual de lifecycle dos defaults registrado no relatório.
