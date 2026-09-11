# Skills — fase mecânica 7A: passivas de combate e sobrevivência

> **Spec ID:** spec_skills_17_combat_survival_passives_v1  
> **Status:** PLAYMODE_VALIDATED — 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime / Skills+Player+Combat / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_16_crafting_actions_v1  
> **Blocks:** spec_skills_18_crafting_economy_passives_v1 e UI  
> **Repo lock scope/Validation/Executor:** Ownership / Unity+EditMode+PlayMode / execute-spec-strict

# /speckit.specify

## Spec

Conectar todas as passivas Melee/Ranged/Magic/Survival ao consumidor real e corrigir efeitos substitutos.
Cada rank altera o valor exibível; passivas não ocupam slot nem executam animação.

**Veredito de equilíbrio pré-implementação:** `SOUND_WITH_AMENDMENTS`. A implementação
deve preservar a identidade do nó no modifier agregado; tipos genéricos não podem apagar
a árvore, o equipamento requerido ou a ação específica. Os valores e fórmulas abaixo são
normativos e substituem os placeholders atuais.

- **AC01:** modifiers flat/percentuais de ataque, defesa, HP, STA, MP, velocidade de projétil, dodge e status são agregados uma única vez pelo `SkillEffectAggregator`. Totais incondicionais e canais tipados chegam uma vez ao `DerivedStatsCalculator`; gates contextuais consomem o valor derivado sem re-somar a lista de modifiers. Dodge consome o mesmo snapshot agregado e não pode acumular aplicações.
- **AC02:** Canalização Rápida usa +8/16/24% da regen base, sem escalar bônus externos. Poço de Mana mantém +10 MP/rank, máximo +50; Fio Arcano mantém +1 Magic Attack/rank, máximo +5; Domínio do Raio Arcano mantém +1 somente no Arcane Bolt/rank, máximo +5.
- **AC03:** Passo Seguro reduz apenas a parcela perdida pela penalidade de terreno em 15/25/35%, sem bônus em chão normal. `Terrain` é fator próprio e não sobrescreve `Status`.
- **AC04:** Rações -10/20/30% hunger drain; fontes de hunger compõem multiplicativamente com piso final de 25%. Recuperação reduz em 10/20/30% a duração somente de Poison/Burn/Slow, depois da resistência: `clamp(base × resistanceMultiplier × recoveryMultiplier, 1, 30)`.
- **AC05:** resistências publicam valor e redução efetiva pela fórmula vigente; nenhuma passiva sem consumer fica comprável como se operacional.
- **AC06:** purchase/rank/respec/load recomputam de forma idempotente via `SkillDerivedStatsChangedEvent`.
- **AC07:** a matriz de identidade é normativa: Pegada +1 Melee Attack/rank (máximo +5) nunca alcança projétil/magia; Postura +1 DEF/rank (máximo +3) exige arma melee e escudo válido/não quebrado; Fluxo melhora recovery em 5/10/15% e exige duas armas melee leves válidas; Ímpeto dá +5% dano/rank (máximo +25%) e exige arma explicitamente marcada `RequiresTwoHands`; Treino reduz só o custo de Dodge em 10/20/30%, compõe multiplicativamente com Sinal de Retirada e respeita piso final de 50% do custo base.
- **AC08:** Mão Firme mantém +1 Bow Damage/rank (máximo +5); Mira Longa +0,5/rank (máximo +1,5); Encaixe melhora recovery do disparo em 8/16/24%, sem tocar cooldown compartilhado; Passos de Kiting concede +4% MoveSpeed/rank (máximo +20%) por 2 s após disparo, apenas enquanto o movimento se afasta do alvo/ameaça determinística localizada num raio máximo autorado de 20 tiles; Afinação concede +1 velocidade de projétil/rank (máximo +5).
- **AC09:** Pulmões +10 STA/rank (máximo +50), Pele Dura +5 HP/rank (máximo +25) e resistências Toxic/Cold/Heat +1/rank (máximo +5) mantêm os valores atuais. Dano recebido continua `max(1, raw - defense - resistance)` e redução de duração por resistência continua `1 - min(0,50, resistência × 0,02)`.
- **AC10:** `AuthoredMaxRank=3` em Postura Guardada, Fluxo, Treino de Esquiva, Mira Longa, Encaixe Rápido, Canalização Rápida, Rações, Recuperação Instintiva e Passo Seguro; todos os demais passivos não-capstone desta fase têm máximo 5. Nenhum rank comprado pode ser ornamental.

Fora: crafting/economia, capstones, rebalance final, UI e animações.

# /speckit.plan

Reusar `SkillEffectAggregator`, `SkillModifierHooks` e `DerivedStatsCalculator`. Criar adapters puros somente
para hunger, terrain e status duration. Remover `EffectPending` apenas depois do consumer testado.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/SkillEffectAggregator.cs`
- `Assets/_Game/Scripts/Skills/SkillModifierHooks.cs`
- `Assets/_Game/Scripts/Foundation/SkillPassiveModifier.cs` (preservar identidade/origem)
- `Assets/_Game/Scripts/Foundation/SkillModifierType.cs`
- `Assets/_Game/Scripts/Player/DerivedStatsCalculator.cs`
- `Assets/_Game/Scripts/Player/PlayerVitalsApplier.cs`
- `Assets/_Game/Scripts/Player/HungerManager.cs`
- `Assets/_Game/Scripts/Save/SaveData.cs` e `Assets/_Game/Scripts/Save/Providers/PlayerSectionProvider.cs` (remainder fracionário de hunger, campo aditivo simples)
- evento Foundation/Core de snapshot de resistências, se necessário para materializar AC05
- `Assets/_Game/Scripts/Player/Movement/SpeedFactorKind.cs`
- `Assets/_Game/Scripts/Player/Movement/PlayerSpeedComposer.cs` (fator tipado de terreno)
- `Assets/_Game/Scripts/Cave/Runtime/CaveHazardTile.cs` (separar o fator de terreno)
- `Assets/_Game/Scripts/Combat/PlayerCombatStatsProvider.cs`
- `Assets/_Game/Scripts/Combat/BowArrowAttackService.cs`
- `Assets/_Game/Scripts/Combat/SpellCastService.cs`
- `Assets/_Game/Scripts/Combat/StatusEffect/PlayerStatusReceiver.cs`
- `Assets/_Game/Scripts/Player/StatusEffectManager.cs`
- `Assets/_Game/Scripts/Core/StatusEffect.cs`
- `Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs` (handedness explícita)
- catálogo/gerador canônico de armas somente para preencher `RequiresTwoHands`
- controllers ativos de Dodge e routing de ataque somente nos pontos de consumo provados
- `Assets/_Game/Scripts/Skills/Runtime/PassiveSurvivalModifierAdapter.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/PassiveCombatModifierAdapter.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/RangedKitingRuntime.cs` (CREATE)
- testes `CombatSurvivalPassiveTests.cs` e `SkillPassiveRecomputePlayModeTests.cs` (CREATE)

No início da execução, resolver o path exato do owner de terreno; se diferir, emendar Ownership antes de
editar. Edge cases: load repetido, respec, chão normal, status instantâneo e bônus externos de regen.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Matriz normativa node→modifier→consumer/equipamento e fórmula em três marcos | AC01–AC05, AC07–AC10 |
| [x] | T02 | Regen base e terrain penalty | AC02, AC03 |
| [x] | T03 | Hunger/status duration/resistências | AC04, AC05 |
| [x] | T04 | Recompute idempotente e limpar flags somente com prova | AC05, AC06 |
| [x] | T05 | EditMode + PlayMode purchase/rank/respec/load e matriz de identidades | AC01–AC10 |
| [x] | T06 | Validar `failed=0` e closeout | AC01–AC10 |
