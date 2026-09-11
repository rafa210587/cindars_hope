# Skills — fase mecânica 1: dados, rank e readiness

> **Spec ID:** spec_skills_10_mechanics_data_rank_readiness_v1
> **Status:** IMPLEMENTADA E VALIDADA — 2026-09-10
> **Wave:** SKILLS_SDD_V1
> **Priority:** P0
> **Type:** Runtime/Data
> **Domain:** Skills
> **Revision:** 1 — 2026-09-10
> **Parallelizable:** NO — contrato compartilhado por todas as famílias
> **Repo lock scope:** arquivos listados em Ownership
> **Depends on:** specs implementadas 01/02/03; refinamento funcional v3 aprovado
> **Blocks:** mecânicas Melee, Ranged, Magic, Survival/Crafting, passivas, UI e animações
> **Validation level:** Unity compile + EditMode Skills + PlayMode foundation
> **Executor:** Codex/subagent com revisão do root

# /speckit.specify

## Problema e resultado

O jogador pode gastar pontos em ranks que não chegam aos executores. O rank cap é apenas dinâmico por
profundidade e não respeita o máximo autorado de cada ação/capstone. Nós dormentes podem alcançar a rota
de execução, e o catálogo ainda guarda pré-requisitos contraditórios ou incompletos.

Ao concluir esta spec, rank e readiness formam um contrato único: cada execução recebe o rank comprado,
ações implementadas escalam pelo payload aprovado, nós respeitam `AuthoredMaxRank`, nós dormentes recusam
antes de custo/cooldown e os capstones exigem seus dois IDs vivos.

## Requisitos

- **R01:** adicionar `AuthoredMaxRank` a `SkillNodeDataSO`, default 5, clamp 1–5.
- **R02:** compra/rank-up usa `min(DynamicRankCap, AuthoredMaxRank)`; capstones têm máximo 3.
- **R03:** as 31 ações recebem definições `SkillActionSO` de runtime, com números do refinamento e sem alterar IDs.
- **R04:** `SkillEffectContext.Rank` e `ActionData` recebem rank e tuning resolvidos pelo `SkillTreeManager`.
- **R05:** executores melee/projectile leem custo, dano, alcance, contagem, pierce e cooldown de `ActionData`; dano usa `base + perRank × (rank−1)`.
- **R06:** nó `NotYetExecutable` recusa antes de resolver executor, custo ou cooldown.
- **R07:** Presa Marcada exige `ranged_steady_hand`; capstones exigem os dois IDs definidos no refinamento.
- **R08:** ausência/valor inválido de rank usa 1, preservando saves e testes legados.

## Critérios de aceitação

- **AC01:** catálogo permanece com 66 nós, 31 equipáveis e 35 passivos/capstones.
- **AC02:** capstone não sobe acima de R3 mesmo com tier dinâmico 5.
- **AC03:** ação R1–3 não sobe a R4; ação R1–5 pode subir conforme tier.
- **AC04:** Corte Giratório R1/R2/R3 resolve 10/12/14; máximo seis alvos; alvos 4–6 recebem 70%; sétimo não recebe dano.
- **AC05:** habilidade dormente equipada retorna recusa e mantém recurso/cooldown.
- **AC06:** todos os pré-requisitos propostos existem na mesma árvore e compra exige ambos.
- **AC07:** nenhum manager, catálogo paralelo, Unity reference em save ou edição manual de YAML.

# /speckit.plan

## Ownership

### Modificar

- `Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs`
- `Assets/_Game/Scripts/Skills/SkillActionSO.cs`
- `Assets/_Game/Scripts/Skills/SkillTreeManager.cs`
- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/SkillPurchaseService.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectContext.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ProjectileSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `Assets/_Game/Scripts/Editor/Skills/GenerateCanonicalSkillCatalog.cs`
- `Assets/_Game/Tests/EditMode/Skills/CanonicalCatalogTests.cs`
- `Assets/_Game/Tests/EditMode/Skills/SkillExecutionFoundationTests.cs`

### Criar

- `Assets/_Game/Scripts/Skills/DefaultSkillActionCatalog.cs`
- `Assets/_Game/Tests/EditMode/Skills/SkillRankReadinessTests.cs`
- `Assets/_Game/Tests/PlayMode/Composition/SkillExecutionFoundationPlayModeTests.cs`
- `Assets/_Game/Data/Skills/Actions/**`
- `Assets/_Game/Data/Skills/Actions.meta`
- `Assets/_Game/Data/Skills/SkillActionDatabase.asset`
- `Assets/_Game/Data/Skills/SkillActionDatabase.asset.meta`
- `Assets/_Game/Data/Skills/Nodes/*.asset` (somente campos gerados por `GenerateCanonicalSkillCatalog`)
- `docs/validation/skills_sdd_v1/mechanics/PHASE_01_DATA_RANK_READINESS_REPORT.md`

### Proibido nesta fase

UI/prefabs/scenes, save schema, imagens, animações, novos nós, novos managers e mecânicas específicas de
charge/chain/cloud/taunt/repair. Esses itens pertencem às fases seguintes.

## Contratos e edições

1. `SkillNodeDataSO.AuthoredMaxRank` é serializável e validado entre 1 e 5.
2. `SkillPurchaseService.GetEffectiveRankCap(node,state)` centraliza o mínimo entre cap dinâmico e autorado;
   `TryRankUp` e `CanRankUp` usam a mesma função.
3. `DefaultSkillCatalog` aplica max 3 a todos os capstones e às ações declaradas R1–3; demais ações ficam 5.
4. `DefaultSkillCatalog` substitui/reconstrói listas de pré-requisitos somente nos cinco capstones e Presa.
5. `DefaultSkillActionCatalog.BuildAll()` cria 31 definições transitórias como fallback, inclusive Quick Repair dormente; `SkillTreeManager` indexa assets quando atribuídos e fallback quando ausentes.
6. `ActiveSkillExecutionController` valida dormência, resolve `SkillActionSO` e preenche `context.Rank/ActionData`.
7. Melee/Projectile deixam de ser autoridade de números e calculam a partir de `ActionData`; campos semânticos ainda inexistentes no SO são adicionados como dados simples.
8. `GenerateCanonicalSkillCatalog` passa a gerar/atualizar 31 actions e um `SkillActionDatabaseSO` via AssetDatabase, sem YAML manual.
9. O primeiro slice comportamental completo é Corte Giratório: ordenação estável por distância+ID, dedupe por `EnemyHealth`, máximo seis, 70% após o terceiro.

Pseudocódigo:

```text
effectiveCap = clamp(min(state.DynamicRankCap(tree), node.AuthoredMaxRank), 1, 5)
rank = clamp(state.GetRank(nodeId), 1, effectiveCap)
damage = action.BaseDamage + action.DamagePerRank * (rank - 1)
if node.NotYetExecutable => fail before context/executor/resource/cooldown
```

## Falhas e compatibilidade

- Assets antigos com zero em `AuthoredMaxRank` são normalizados para 5.
- Slot legado contendo actionId continua resolvendo o nodeId e seu rank.
- ID desconhecido continua recusado sem mutação.
- Pré-requisito ausente no catálogo falha em teste de integridade; não recebe fallback.
- Rank excessivo vindo de save permanece normalizado pela política vigente e é limitado na execução.

# /speckit.tasks

| Done | ID | Dependência | Edição | Cobertura |
|---|---|---|---|---|
| [x] | T01 | — | Adicionar `AuthoredMaxRank` e cap efetivo compartilhado | R01–R03, AC02–AC03 |
| [x] | T02 | T01 | Autorar máximos/pré-requisitos dos nós e 31 SkillAction definitions data-driven | R03, R07, AC01, AC06 |
| [x] | T03 | T01–T02 | Indexar actions, propagar rank/tuning e bloquear dormência antes do commit | R04, R06, R08, AC05 |
| [x] | T04 | T03 | Consumir tuning nos executores e concluir slice do Corte Giratório | R05, AC04 |
| [x] | T05 | T01–T04 | Criar testes determinísticos de cap, pré-requisito, rank e dormência | AC01–AC06 |
| [x] | T06 | T05 | Rodar EditMode Skills e PlayMode foundation; registrar XML/log/report | AC01–AC07 |
| [x] | T07 | T06 | Revisão independente de não regressão e closeout | AC01–AC07 |

## Testes nomeados

- `Capstone_EffectiveCapStopsAtThree`: rank 3 aceita; rank 4 recusa.
- `CanonicalThreeRankActive_RefusesRankFour`: ação canônica R1–3 não alcança R4.
- `ActiveR5_UsesDynamicCap`: ação R1–5 acompanha 2/3/4/5 por profundidade.
- `Catalog_PrerequisitesExistInSameTree`: cada prerequisite resolve e tem o mesmo TreeId.
- `Capstone_RequiresBothBranches`: falta de qualquer ramo bloqueia compra.
- `WhirlDamage_RanksResolveTenTwelveFourteen`: fórmula por rank vem do action data.
- `WhirlTargets_CapsSixAndFallsOffAfterThird`: 7 alvos; somente 6 atingidos, 4–6 a 70%.
- `DormantEquippedAction_ConsumesNothingAndStartsNoCooldown`: nenhum custo nem cooldown é iniciado.

## Prontidão

READY_FOR_IMPLEMENTATION. O pedido humano desta sessão aprovou executar o refinamento por fases. D02–D10
entram como baseline v1 e continuam sujeitos ao playtest final; esta spec aplica apenas D02 e contratos de
rank/readiness, sem antecipar as mecânicas das fases seguintes.

## Fontes

- `docs/refinements/a_implementar/ref_skill_catalog_functional_contract_v3.md`
- `.specs/features_futuras/skills_sdd_v1/spec_skills_03_dados_rank_readiness_v1.md`
- `.specs/features_futuras/skills_sdd_v1/spec_skills_05_mecanicas_ativas_v1.md`
- `docs/game_rules/skill_tree_rules.md`
