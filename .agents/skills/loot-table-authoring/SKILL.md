---
name: loot-table-authoring
description: Estrutura drop/reward tables — weights, rarity/quality, rolls seeded (determinismo), reward table contract e integração com enemy/cave/quest rewards — reusando o LootTableResolver/EnemyLootResolver existentes. Use em 06_spec_loot_table_reward_table_contract, 06_enemy_elite_boss_drop_tables, 06_cave_treasure_mining_loot_snapshot, ou ao adicionar/alterar qualquer tabela de loot, drop de inimigo/boss ou reward table.
---

# Skill: Autoria de Loot Table

O projeto já tem dois resolvers determinísticos por seed (`LootTableResolver` para tables de fonte e `EnemyLootResolver` para inimigos, ambos `System.Random`-based e documentados sob ADR-0005 / cave-stable-run); esta skill garante que toda nova table role pelo mesmo contrato seeded em vez de introduzir randomness não-determinística.

## Sistemas existentes (reusar, não duplicar)

- `LootTableResolver` (`Assets/_Game/Scripts/Loot/LootTableResolver.cs`) — `Resolve(table, LootRollContext)`: aplica `GuaranteedDrops`, depois `WeightedDrops` (escolha ponderada por `Weight`), `RareDrops` (`DropChance` por entry), `UniqueDrops` (guard por `GrantedUniqueItemIds`), `GoldRange` e `FirstTimeBonus`. Usa `new Random(context.Seed)` — **determinístico por seed**.
- `EnemyLootResolver` (`Loot/EnemyLootResolver.cs`) — resolver PURO e determinístico para inimigos: `Roll(table, seed, isElite, isMinibossOrBoss, warnings)`, com `BuildLootSeed(caveRunSeed, enemyInstanceId)` = `StableHash("{runSeed}|{instanceId}|loot")` (FNV-1a, sem GUID/timestamp). Essência da banda: 8% comum / +25% elite / 100% miniboss/boss.
- `LootTableDefinition` (`Loot/LootTableDefinition.cs`) — buckets (`GuaranteedDrops`/`WeightedDrops`/`RareDrops`/`UniqueDrops`), `QualityRollProfile`/`RarityRollProfile`/`QuantityRollProfile`, `RepeatFarmRules` (cooldown, `MaxGrantsPerDay`), `PityRules` (safety net), `GoldRange`, `SourceType`.
- `LootEntry` (`Loot/LootEntry.cs`) — `Weight`, `DropChance`, `QuantityMin/Max`, `RequiredFlag`/`ForbiddenFlag`, `LootEntryQualityPolicy` (None/Fixed/RollFromProfile/SeededFromSnapshot), `LootEntryRarityPolicy`, `IsUniqueReward`/`IsLoreReward`/`FirstTimeOnly`/`IsProgressionCritical`/`IsProtected`.
- `RewardTableDefinition` (`Loot/RewardTableDefinition.cs`) — contrato de reward separado do loot: `GuaranteedRewards`/`ChoiceRewards`, `GrantedFlags`/`GrantedRecipes`, `GoldRange`, `FirstTimeBonus`, `IsOneTimeOnly`, `RepeatRules`. Resultado em `RewardGrantResult`.
- `LootTableSO` (`Loot/LootTableSO.cs`) — asset autorável (`TableId`, `Entries`, `GuaranteedEntries`, `EssenceItemId`/`EssenceCommonChance`/`EssenceEliteBonusChance`, `EquipmentEntries`). **Atenção:** `LootTableSO.TryRoll` legado usa `UnityEngine.Random` (estado global, NÃO determinístico) — use o `EnemyLootResolver.Roll` seeded para qualquer drop que entre num snapshot.
- `LootSourceType` (enum) e `LootTableValidator` (`ProtectedItemIds`, regra de `IsProgressionCritical` ⇒ exige `PityRules` safety net).

## Procedimento

1. **Table como dado, role pelo resolver.** Crie a `LootTableDefinition`/`LootTableSO` ou `RewardTableDefinition` e role SEMPRE via `LootTableResolver.Resolve` / `EnemyLootResolver.Roll` — nunca um segundo loop de roll ad-hoc.
2. **Determinismo por seed obrigatório.** Todo roll que possa entrar num save/snapshot recebe um seed estável (`BuildLootSeed` ou `LootRollContext.Seed`) derivado de `CaveRunSeed` + id de instância via `StableHash`. Proibido `UnityEngine.Random`, `Guid.NewGuid()` ou timestamp no caminho de roll seeded (skill: rng-and-determinism). Só jitter puramente visual pode usar `UnityEngine.Random`.
3. **Loot de cave NÃO re-rola.** Na MESMA `CaveRunSeed`, revisitar a mesma instância (inimigo, treasure chest, mining node) tem de produzir o MESMO drop até ser coletado; persista o outcome/estado de coletado, não re-role on revisit (rule: cave-stable-run). Mesmo (table, seed) → mesmo resultado é o contrato central.
4. **Weights e rarity/quality.** Use `Weight` para a escolha ponderada e `LootEntryQualityPolicy`/`RarityPolicy` (com `QualityRollProfile`/`RarityRollProfile`) para tier — preferindo `SeededFromSnapshot` quando o resultado precisa sobreviver a reload. Sem magic numbers de chance espalhados: vivem no profile/SO.
5. **Reward table contract.** Rewards de quest/milestone usam `RewardTableDefinition` (separado de loot de farm), com `IsOneTimeOnly`/`FirstTimeBonus` para itens não repetíveis. Aplique reward via o caminho idempotente de quest quando a fonte for quest (skill: quest-authoring — `QuestRewardApplicator`), não duplicando a aplicação aqui.
6. **Itens protegidos e progression-critical.** Itens em `ProtectedItemIds` não podem ser common repeat loot; entry `IsProgressionCritical` exige `PityRules` safety net (validado por `LootTableValidator`) para evitar softlock de progressão.
7. **Integração enemy/cave/quest.** Drop de inimigo/elite/boss passa por `EnemyLootResolver` (essência por tier); treasure/mining de cave entra no snapshot seeded; reward de quest passa pelo applicator de quest. Reward consistency (valor dentro da faixa esperada) é responsabilidade de (skill: economy-balance-tuning).

## Testes

EditMode tests (skill: editmode-test-authoring) são exigidos pelo (rule: testing-quality-gate) — os resolvers são C# puro:

- **mesmo seed → mesmo drop**: `EnemyLootResolver.Roll(table, seed)` e `LootTableResolver.Resolve` produzem resultado idêntico para o mesmo (table, seed);
- **seeds diferentes → distribuições diferentes** (sanidade do weighting);
- **persistência de outcome**: drop de cave salvo + reload + revisit = mesmo drop (sem re-roll), estado de coletado respeitado;
- guarantee/weight/rare/unique buckets aplicados na ordem certa;
- `UniqueDrops`/`FirstTimeBonus`/`IsOneTimeOnly` concedidos uma única vez;
- `LootTableValidator`: progression-critical sem `PityRules` falha; item protegido como repeat loot falha;
- `BuildLootSeed` estável (paridade FNV-1a, sem GUID/timestamp).

## Onde se aplica

- **06_spec_loot_table_reward_table_contract** — o contrato de table/reward e o roll seeded.
- **06_enemy_elite_boss_drop_tables** — drop tables por família/tier via `EnemyLootResolver`.
- **06_cave_treasure_mining_loot_snapshot** — outcome de loot de cave seeded e persistido (sem re-roll).

## Relacionados

- (skill: rng-and-determinism) — seed por sistema, persistir o outcome.
- (rule: cave-stable-run) — loot de cave não pode re-rollar na mesma run.
- (skill: economy-balance-tuning) — reward consistency (valor dentro da faixa de balance).
- (skill: data-catalog-authoring) — tables/SO como dados com id estável + validator.
- (skill: editmode-test-authoring) — mesmo seed → mesmo drop, persistência de outcome.
