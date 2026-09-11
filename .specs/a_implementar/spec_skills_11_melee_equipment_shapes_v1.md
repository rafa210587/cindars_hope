# Skills — fase mecânica 2A: melee, equipamento e formas

> **Spec ID:** spec_skills_11_melee_equipment_shapes_v1  
> **Status:** IMPLEMENTADA E VALIDADA — 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime / Skills+Combat / P0  
> **Parallelizable:** NO — compartilha executor e ActionData com as fases seguintes  
> **Repo lock scope:** somente Ownership  
> **Depends on:** spec_skills_10b_action_timing_commit_v1  
> **Blocks:** spec_skills_12_melee_movement_posture_v1 e UI de skills  
> **Validation:** Unity compile + EditMode Skills/Combat + PlayMode em Cave  
> **Executor:** Codex/Claude via execute-spec-strict

# /speckit.specify

## Spec

Fechar as formas melee sem deslocamento: Corte da Mão Secundária exige offhand leve e Corte Giratório
preserva o slice 10/12/14, máximo seis e queda de 70% após o terceiro. Whiff é execução válida e consome
STA/cooldown; ausência de equipamento recusa antes do commit.

### Requisitos e critérios

- **R01/AC01:** `skill_melee_offhand_cut` recusa com `requires_offhand` sem arma secundária melee leve e íntegra em `LeftHand`; com gate válido causa 8/10/12 em arco 140°×1,2, um hit por `EnemyHealth`. Melee leve significa `WeightClass.Light` e tipo diferente de Bow/Staff/Wand/Tool.
- **R02/AC02:** `skill_melee_whirl_cut` mantém 10/12/14, seis alvos e 100/100/100/70/70/70%; o sétimo recebe zero.
- **R03/AC03:** custo/cooldown vêm apenas de `SkillActionSO`; nenhuma tabela numérica volta ao executor.
- **R04/AC04:** feedback de recusa usa `PlayerActionFeedbackEvent`; sucesso só é publicado após o commit.
- **R05/AC05:** o fluxo vivo de equipamento permite escolher as adagas canônicas para `LeftHand`; ao equipar arma com durabilidade ainda não rastreada, inicializa com `WeaponDataSO.DurabilityMax`.

Fora do escopo: avanço, salto, taunt, posture especial, animação/VFX/SFX e UI.

# /speckit.plan

## Contratos

CRIAR `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeEquipmentGate.cs` com
`MeleeEquipmentReadiness Evaluate(GameObject caster, string actionId)`; DTO contém `CanExecute` e
`FailureKey`. MODIFICAR `MeleeStrikeSkillEffectExecutor.Execute` para chamar o gate antes de STA e
`DefaultSkillActionCatalog.BuildAll` apenas se algum dado contratado estiver ausente. Reusar
`EquipmentManager`, `EquippedItemResolver` e databases do bootstrap; não criar manager ou cache paralelo.
O gerador autoraliza `AllowedEquipmentSlots=[LeftHand,RightHand]` nas três adagas canônicas. A regra de UI
continua guiada por esses slots explícitos. `EquipmentManager.EquipItem` inicializa durabilidade apenas quando
resolve uma arma viva e ainda não existe ledger; load existente continua soberano.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeEquipmentGate.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/DefaultSkillActionCatalog.cs`
- `Assets/_Game/Scripts/UI/EquipmentSlotRules.cs`
- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs`
- `Assets/_Game/Scripts/Editor/Items/GenerateCanonicalItemCatalog.cs`
- `Assets/_Game/Data/Items/item_weapon_dagger_copper.asset`
- `Assets/_Game/Data/Items/item_weapon_dagger_steel.asset`
- `Assets/_Game/Data/Items/item_weapon_dagger_mithril.asset`
- `Assets/_Game/Tests/EditMode/Skills/MeleeEquipmentAndShapeTests.cs` (CREATE)
- `Assets/_Game/Tests/PlayMode/Composition/SkillMeleeShapesPlayModeTests.cs` (CREATE)

Algoritmo: readiness → stamina → coleta/dedupe → filtro de arco → ordenação distância+EnemyInstanceId → cap/falloff
→ dano/posture comum → cooldown/feedback. Edge cases: dois colliders do mesmo inimigo, offhand quebrada,
sexto/sétimo equidistantes, arco leve usado como offhand e caster destruído.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Autorizar adagas em LeftHand pelo gerador/regra e inicializar durabilidade no equip | AC01, AC05 |
| [x] | T02 | Criar gate puro sobre o equipamento vivo | AC01, AC04–AC05 |
| [x] | T03 | Integrar gate antes do custo; preservar pipeline data-driven | AC01, AC03 |
| [x] | T04 | Fixar testes de arco, dedupe, seis alvos, falloff, whiff e compatibilidade de slot | AC01–AC05 |
| [x] | T05 | PlayMode: sem offhand/leve/quebrada, arco recusado e 7 inimigos | AC01–AC05 |
| [x] | T06 | Gerar assets, validar e fechar com revisão não regressiva | AC01–AC05 |

DoD esperado: runner EditMode filtrado retorna `UNITY_EDITMODE: PASS; failed=0`; PlayMode retorna todos os
cenários nomeados PASS. Sem evidência pré-declarada.
