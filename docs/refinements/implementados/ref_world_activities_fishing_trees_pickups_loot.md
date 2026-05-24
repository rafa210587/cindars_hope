# ref_world_activities_fishing_trees_pickups_loot

> Status: Implementado parcial
> Spec relacionada: `docs/specs/implementados/spec_world_002_activities_fishing_trees_pickups_loot.md`
> Objetivo: completar arvores, pesca, pickups persistentes e loot tables de atividades do mundo.

## Resultado da implementacao 2026-05-24

Entregue parcialmente:

- Loot table oficial para world activities.
- Fishing timing MVP com loot table opcional.
- Tree HP/stump/regrowth e madeira por hit/final hit.
- DTOs de save preparados para pickup dinamico.

Pendencias:

- Spawner persistente real para drops dinamicos.
- Dois fishing spots fixos na FarmScene.
- Fishing spot procedural de cave 10%.

---

## 1. Estado atual

O projeto ja possui atividades MVP: cortar arvore, pescar peixe comum e coletar pickups persistentes.

Evidencia:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Save/SaveManager.cs
docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md
docs/specs/implementados/spec_world_001_pickups_persistentes_save_load.md
```

---

## 2. Gaps

- Fishing ainda nao tem timing/minigame real.
- Fishing nao usa loot tables por spot/contexto.
- Fishing spots fixos/procedurais nao estao formalizados.
- Arvores nao tem HP/tier/regrowth configuravel completo.
- Arvores nao entregam madeira por hit de forma balanceada.
- Pickups persistentes precisam preservar comportamento de save/load sem reaparecer indevidamente.
- Loot tables ainda nao sao fonte padronizada para world activities.
- Stamina/durability final ainda nao deve ser implementada nesta spec.

---

## 3. Decisoes aprovadas

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

---

## 4. Fishing

### Fishing spots

Tipos esperados:

```text
FarmFixedFishingSpot
CaveProceduralFishingSpot
```

Farm:

- FarmScene deve ter 2 fishing spots fixos.
- Spots fixos devem ter IDs estaveis.
- Spots fixos usam `FishingSpotDataSO` ou configuracao equivalente.

Cave:

- Ao gerar cada level de cave, aplicar chance de 10% para fishing spot.
- Se a chance passar, gerar exatamente 1 fishing spot naquele level.
- Se a chance falhar, nao gerar fishing spot naquele level.
- Fishing spot procedural deve respeitar walkable/safe position e nao bloquear path critico.
- O spot gerado deve ser reprodutivel dentro da mesma run/snapshot quando cave replay estiver ativo.

### Fishing action

- Exige Fishing Rod disponivel/equipada conforme sistema atual.
- Sem rod: feedback claro e pesca nao inicia.
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

---

## 5. Trees

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
- Madeira gerada deve sair como pickup persistente por padrao.

### Stump/regrowth

- Quando HP chega a 0, arvore vira stump.
- Stump salva `RegrowthRemainingDays`.
- Se `RegrowthDays > 0`, stump volta a arvore quando timer chegar a 0.
- Se `RegrowthDays <= 0`, nao regenera automaticamente.

---

## 6. Pickups persistentes

Regras:

- Pickup de cena usa ID estavel: `SceneId + PickupIndex + ItemId` ou equivalente.
- Pickup dinamico gerado por loot usa `RuntimeGeneratedPickupId` salvo em `WorldSaveData`.
- Nao destruir GameObject antes de registrar estado coletado.
- Coletar pickup deve atualizar save/runtime antes de remover visualmente.
- Save/load deve restaurar coletados e nao coletados.
- Pickups dinamicos nao podem duplicar apos save/load.

---

## 7. Loot

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

---

## 8. Invariantes anti-regressao

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

---

## 9. Arquivos provaveis

```text
Assets/_Game/Scripts/World/TreeNode.cs
Assets/_Game/Scripts/World/ItemPickup.cs
Assets/_Game/Scripts/World/ItemPickupRegistry.cs
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Loot/LootTableSO.cs
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Cave/**
```

---

## 10. Definition of Done

- [ ] FarmScene possui 2 fishing spots fixos com IDs estaveis.
- [ ] Cave level tem 10% de chance de gerar fishing spot; se gerar, apenas 1 por level.
- [ ] Fishing exige Fishing Rod e usa timing window simples.
- [ ] Fishing usa loot table.
- [ ] Fish catch nao perde item se inventory estiver cheio.
- [ ] Arvores usam HP, Axe/tier e TreeDataSO ou equivalente.
- [ ] Arvores geram madeira por hit.
- [ ] Hit final de arvore gera pelo menos 2x madeira de hit normal equivalente.
- [ ] Arvore cortada vira stump e respeita regrowth configuravel.
- [ ] Tree drops viram pickups persistentes.
- [ ] Pickups de cena e dinamicos persistem corretamente.
- [ ] LootTableSO e usado por fishing e trees.
- [ ] Acoes sem escolha executam direto com `E`, sem menu desnecessario.
- [ ] Invariantes anti-regressao preservadas.

---

## 11. Validacao

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
12. Validar Unity compile validation e docs validation.
