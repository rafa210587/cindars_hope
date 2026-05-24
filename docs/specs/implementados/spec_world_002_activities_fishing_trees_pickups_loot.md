# SPEC - World activities, fishing, trees, pickups e loot

> Spec ID: spec_world_activities_fishing_trees_pickups_loot
> Status: Implementado parcial
> Ordem de execucao: 05
> Depende de: 00-04
> Bloqueia: 06, 07, 10
> Tipo: Runtime
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar pesca, arvores, pickups persistentes e loot tables de atividades do mundo, preservando inventory/save/farm ja implementados.
> Fora de escopo: stamina final, durabilidade final, isca/bait, clima real, economy pricing, crafting recipes finais, enemy/cave loot final, Packages, ProjectSettings e docs_old.
> Evidencia: `Assets/_Game/Scripts/Loot/LootTableSO.cs`, `Assets/_Game/Scripts/World/FishingSpot.cs`, `Assets/_Game/Scripts/World/TreeNode.cs`, `Assets/_Game/Scripts/World/Data/TreeDataSO.cs`

## Resultado da implementacao 2026-05-24

Implementado parcial:

- Criado `LootTableSO` com entries por `ItemId`, quantidade min/max, peso e tags futuras.
- `FishingSpot` passou a exigir rod, iniciar casting, abrir janela simples de timing e resolver item via loot table opcional.
- Catch nao remove estado se inventory cheio; falha com feedback sem perda de item.
- `TreeDataSO` recebeu HP, tool/tier, madeira por hit, multiplicador final, regrowth e loot table hook.
- `TreeNode` passou a ter HP, madeira por hit, bonus de hit final >= 2x, stump e regrowth por dia.
- Save de arvore ganhou HP atual, stump e regrowth restante.
- Save de pickup ganhou campos de ID persistente para pickup dinamico futuro.

Pendencias reais:

- Tree drops ainda entram no inventory quando nao ha spawner/registry persistente conectado; spawner dinamico real fica pendente.
- FarmScene nao foi editada para garantir dois fishing spots fixos.
- Cave procedural fishing spot 10%/max 1 por level nao foi integrado para evitar regressao em snapshots sem Play Mode.
- Play Mode manual completo e Unity compile formal ficaram bloqueados por ambiente/licenca.

Fontes absorvidas:
- specs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT/spec.md
- specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_world_activities_fishing_trees_pickups_loot.md

---

# /speckit.specify

## Contexto

O projeto ja possui atividades MVP: cortar arvore, pescar peixe comum e coletar pickups persistentes.

Evidencias atuais:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Save/SaveManager.cs
docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md
docs/specs/implementados/spec_world_001_pickups_persistentes_save_load.md
```

Esta spec evolui essas atividades para contratos de gameplay, loot e persistencia sem quebrar pickups existentes, inventory slots, save migration ou farm contextual.

## Pre-condicoes

Implementar runtime somente depois de specs 02, 03 e 04 estarem realmente implementadas.

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Cave/**
```

Se inventory slots, save migration ou farm menu ainda nao existirem, registrar bloqueio/risco e nao implementar comportamento que dependa deles.

## Problema

Gaps atuais:

- Fishing ainda nao tem minigame/timing real.
- Fishing nao usa loot tables por spot/contexto.
- Fishing spots ainda nao estao definidos para Farm e Cave.
- Arvores nao tem HP/tier/regrowth configuravel completo.
- Arvores nao entregam madeira por hit de forma balanceada.
- Pickups persistentes precisam preservar comportamento de save/load sem reaparecer indevidamente.
- Loot tables ainda nao sao fonte padronizada para world activities.
- Stamina/durability ainda nao devem ser finalizadas nesta spec.

## Objetivo

Completar pesca, arvores, pickups persistentes e loot tables de world activities com contratos claros de dados, save/load, eventos e anti-regressao.

## Decisoes aprovadas

- Fishing usa `FishingSpot` explicito.
- Na fazenda devem existir 2 fishing spots fixos.
- Na cave, fishing spot e procedural: 10% de chance por level; quando cair nos 10%, gerar apenas 1 fishing spot naquele level.
- Fishing exige Fishing Rod disponivel/equipada conforme sistema atual.
- Fishing MVP usa timing window simples.
- Isca/bait fica fora do escopo.
- LootTableSO oficial deve ser usado por world activities nesta spec, inicialmente fishing e trees.
- Enemy/cave loot final fica compativel/futuro, sem forcar implementacao nesta spec.
- Arvore usa HP numerico + axe tier.
- Arvores concedem madeira a cada hit.
- Tool/axe de baixa qualidade gera pouquissima madeira por hit.
- Hit final da arvore gera pelo menos 2x a madeira de um hit normal equivalente.
- Arvore cortada vira stump e pode regrow por `RegrowthDays`.
- Tree drops devem virar pickups persistentes no mundo.
- Fish catch tenta ir para inventory; se inventory estiver cheio, criar pickup persistente proximo ao player ou falhar sem perda.
- Pickups de cena usam ID estavel; pickups dinamicos usam runtime generated ID persistido.
- Stamina/durability final fora do escopo; somente hooks opcionais.
- Acoes sem escolha executam direto ao apertar `E`; nao abrir menu se houver apenas uma acao valida.
- Menu vertical so deve aparecer quando houver multiplas opcoes reais.

## User stories / engineering stories

- Como jogador, quero pescar em spots claros, com chance e resultado compreensivel.
- Como jogador, quero que arvores deem madeira durante os hits e mais madeira no hit final.
- Como jogador, quero que pickups coletados nao reaparecam indevidamente apos save/load.
- Como desenvolvedor, quero loot tables reutilizaveis para activities sem invadir enemy/cave loot final.
- Como agente, devo preservar comportamento existente de pickups e save enquanto evoluo world activities.

## Fishing

### Fishing spots

Tipos:

```text
FarmFixedFishingSpot
CaveProceduralFishingSpot
```

Regras Farm:

- FarmScene deve ter 2 fishing spots fixos.
- Spots fixos devem ter IDs estaveis.
- Spots fixos usam `FishingSpotDataSO` ou configuracao equivalente.

Regras Cave:

- Ao gerar cada level de cave, aplicar chance de 10% para fishing spot.
- Se a chance passar, gerar exatamente 1 fishing spot naquele level.
- Se a chance falhar, nao gerar fishing spot naquele level.
- Fishing spot procedural deve respeitar walkable/safe position e nao bloquear path critico.
- O spot gerado deve ser reprodutivel dentro da mesma run/snapshot quando cave replay estiver ativo.

### Fishing action

- Acao exige Fishing Rod disponivel/equipada conforme sistema atual.
- Sem rod: feedback claro, sem iniciar pesca.
- Pressionar `E` em fishing spot com uma unica acao valida inicia pesca direto.
- Se algum spot futuro tiver multiplas opcoes, pode abrir menu contextual vertical.

### Timing window MVP

Fluxo:

```text
E em FishingSpot
Validar Fishing Rod
Iniciar casting
Delay curto
Abrir timing window
Jogador confirma com E/Space dentro da janela
Resultado: catch, fail ou rare catch
```

Resultado deve vir de loot table e dificuldade do spot.

### Fishing loot

- Usar `LootTableSO` ou `FishingLootTableSO` compatível com `LootTableSO` oficial.
- Resultado de catch tenta adicionar item ao inventory.
- Se inventory cheio, criar pickup persistente proximo ao player ou falhar sem perda.
- Rare catch pode ser peso/entry especial na loot table, sem sistema separado complexo.

## Trees

### Tree data

Criar/usar `TreeDataSO` ou equivalente:

```text
TreeId
MaxHp
RequiredToolType = Axe
MinimumToolTier
WoodPerHitMin
WoodPerHitMax
FinalHitMultiplier default 2
RegrowthDays
LootTable
```

### Tree action

- Pressionar `E` em arvore com Axe valida executa hit direto.
- Nao abrir menu se a unica acao valida for cortar.
- Se Axe ausente ou tier insuficiente, mostrar feedback e nao aplicar dano.

### Wood per hit

- Cada hit valido deve gerar madeira.
- Axe/tool de baixa qualidade deve gerar pouquissima madeira por hit.
- Hit final deve gerar pelo menos `2x` a madeira de um hit normal equivalente.
- Madeira gerada deve sair como pickup persistente ou entrar no inventory apenas se regra explicita permitir.
- Padrao desta spec: tree drops viram pickups persistentes.

### Stump/regrowth

- Quando HP chega a 0, arvore vira stump.
- Stump salva `RegrowthRemainingDays`.
- Se `RegrowthDays > 0`, stump volta a arvore quando timer chegar a 0.
- Se `RegrowthDays <= 0`, nao regenera automaticamente.

## Pickups persistentes

Regras:

- Pickup de cena usa ID estavel: `SceneId + PickupIndex + ItemId` ou equivalente.
- Pickup dinamico gerado por loot usa `RuntimeGeneratedPickupId` salvo em `WorldSaveData`.
- Nao destruir GameObject antes de registrar estado coletado.
- Coletar pickup deve atualizar save/runtime antes de remover visualmente.
- Save/load deve restaurar coletados e nao coletados.
- Pickups dinamicos nao podem duplicar apos save/load.

## Loot tables

Criar/usar `LootTableSO` oficial para world activities.

Entry minima:

```text
ItemId
MinAmount
MaxAmount
Weight
RequiredTags opcional
```

Tags/condicoes futuras preparadas, sem obrigar sistemas ainda ausentes:

```text
BiomeTag
TimeOfDayTag
WeatherTag
ToolTier
SpotTag
```

Nesta spec, `LootTableSO` deve ser usado por fishing e trees. Enemy/cave loot final fica para specs futuras.

## UI/input contextual

Regra geral:

- Se houver apenas uma acao valida, `E` executa direto.
- Se houver multiplas opcoes, abrir menu vertical contextual.
- Menu contextual deve seguir padrao da Farm spec 04 quando aplicavel.
- Nao criar UI final ampla nesta spec.

Exemplos:

```text
Tree + Axe valida: E corta direto.
FishingSpot + Rod valida: E inicia pesca direto.
Pickup: E coleta direto.
Objeto com multiplas opcoes futuras: abrir menu vertical.
```

## Stamina/durability

Fora do escopo final desta spec.

Permitido preparar hooks simples:

```text
StaminaCost
DurabilityCost
ToolTierRequirement
```

Nao ativar custo final nem quebrar tools antes das specs 09/10.

## Save/load

Persistir somente tipos simples.

World save deve preservar:

```text
TreeId / TreeInstanceId
TreeHp
IsChopped / IsStump
RegrowthRemainingDays
PickupInstanceId
ItemId
Amount
IsCollected
FishingSpotId fixo ou procedural snapshot data quando necessario
```

Nao serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.

## Invariantes anti-regressao

Esta spec nao pode quebrar:

- pickups persistentes ja existentes;
- save/load de collected/uncollected pickups;
- inventory slots/capacity;
- farm plot interaction/menu da spec 04;
- cave generation/replay/snapshots;
- resource nodes da cave;
- hotbar/HUD existente;
- interacao `E` fora de world activities;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

Se algum sistema dependente ainda nao existir, bloquear acao com feedback/pendencia clara em vez de criar fallback que perde item ou duplica estado.

## Criterios de aceite

- FarmScene possui 2 fishing spots fixos com IDs estaveis.
- Cave level tem 10% de chance de gerar fishing spot; se gerar, apenas 1 por level.
- Fishing exige Fishing Rod e usa timing window simples.
- Fishing usa loot table.
- Fish catch nao perde item se inventory estiver cheio.
- Arvores usam HP, Axe/tier e TreeDataSO ou equivalente.
- Arvores geram madeira por hit.
- Hit final de arvore gera pelo menos 2x madeira de hit normal equivalente.
- Arvore cortada vira stump e respeita regrowth configuravel.
- Tree drops viram pickups persistentes.
- Pickups de cena e dinamicos persistem corretamente.
- LootTableSO e usado por fishing e trees.
- Acoes sem escolha executam direto com `E`, sem menu desnecessario.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Core/Events/**
docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md
docs/specs/implementados/spec_world_001_pickups_persistentes_save_load.md
```

Managers/bridges Unity devem ser finos. Regras de loot, tree hit e fishing result devem ficar em classes testaveis quando possivel.

## Ordem segura de implementacao

1. Revalidar pickups persistentes atuais.
2. Criar/ajustar LootTableSO sem migrar enemy/cave loot final.
3. Integrar tree drops com loot/pickups persistentes.
4. Implementar tree HP/hit/stump/regrowth.
5. Implementar fishing spots fixos da Farm.
6. Implementar fishing timing MVP.
7. Integrar cave procedural fishing spot com 10%/max 1 por level.
8. Persistir pickups dinamicos e tree state.
9. Rodar validacoes e atualizar tracking.

## Fluxos

### Tree hit

```text
Player pressiona E em arvore
Validar Axe/tier
Calcular dano
Gerar madeira por hit como pickup persistente
Aplicar dano
Se HP <= 0, gerar final hit bonus >= 2x normal e virar stump
Salvar estado
```

### Fishing

```text
Player pressiona E em FishingSpot
Validar Fishing Rod
Iniciar casting/timing window
Confirmar no timing
Resolver loot table
Adicionar ao inventory ou gerar pickup persistente sem perda
Salvar estado quando aplicavel
```

### Pickup

```text
Player pressiona E em pickup
Validar inventory/capacity quando necessario
Registrar collected no save/runtime
Remover visual somente apos registro
```

### Cave fishing spot

```text
Durante geracao do level
Roll 10%
Se sucesso, escolher posicao segura/walkable
Criar 1 FishingSpot procedural
Persistir no snapshot/run para replay
```

## Eventos candidatos

Usar existentes ou criar seguindo padrao `*Event`:

```text
TreeHitEvent
TreeChoppedEvent
TreeRegrownEvent
FishingStartedEvent
FishCaughtEvent
FishingFailedEvent
PickupCollectedEvent
LootSpawnedEvent
```

Nao duplicar evento se ja existir equivalente.

## Riscos de regressao

- Pickup dinamico duplicar apos save/load.
- Tree drop apagar item se pickup falhar.
- Fish catch perder item se inventory estiver cheio.
- Fishing spot procedural quebrar path/replay da cave.
- Loot table alterar balance de enemy/cave antes da spec correta.
- Acao `E` abrir menu desnecessario ou bloquear interacoes existentes.

## Mitigacao

- Tree/fishing so removem/entregam item apos sucesso de inventory/pickup.
- Pickups dinamicos recebem ID persistido.
- Cave fishing spot deve usar safe/walkable position e snapshot/replay.
- Enemy/cave loot final nao e alterado nesta spec.
- Menu so aparece quando houver multiplas opcoes.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar estado real de World/Fishing/Pickups/Save/Cave antes de alterar runtime.
- [ ] Confirmar specs 02-04 implementadas antes de runtime.
- [ ] Criar/ajustar LootTableSO para world activities.
- [ ] Criar/ajustar TreeDataSO e Tree runtime HP/stump/regrowth.
- [ ] Implementar madeira por hit e bonus de hit final >= 2x.
- [ ] Integrar tree drops com pickups persistentes.
- [ ] Criar/ajustar FishingSpotDataSO.
- [ ] Garantir 2 fishing spots fixos na Farm.
- [ ] Implementar timing window MVP de fishing.
- [ ] Implementar cave fishing spot procedural: 10% chance, max 1 por level.
- [ ] Persistir pickups dinamicos e tree state.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Core/Events/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Fishing spots fixos/procedurais implementados conforme regra.
- Fishing timing MVP e loot table implementados.
- Trees com HP, hit drops, final bonus, stump e regrowth implementadas.
- Pickups persistentes preservados e estendidos para drops dinamicos.
- LootTableSO usado por fishing e trees.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

1. FarmScene possui 2 fishing spots fixos.
2. Pescar com e sem Fishing Rod.
3. Validar timing catch/fail.
4. Validar fish catch com inventory cheio sem perda.
5. Cortar arvore com Axe de tier baixo e validar pouca madeira por hit.
6. Validar hit final com pelo menos 2x madeira normal.
7. Salvar/carregar arvore cortada/stump/regrowth.
8. Coletar pickup, salvar, carregar e validar que nao reaparece.
9. Gerar cave levels suficientes e validar regra 10%/max 1 fishing spot por level.
10. Validar que cave replay/snapshot nao rerolla fishing spot ja gerado.
11. Validar que acoes simples executam direto com `E` sem menu desnecessario.
