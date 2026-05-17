# Cindar's Hope — Sincronização Fase 8 / PR-003 Assets MVP v1.0

> **Status:** Documento de alinhamento operacional  
> **Criado em:** 2026-05-16  
> **Branch:** `docs/sync-fase8-pr003-assets`  
> **Motivo:** alinhar plano, arquitetura, histórias e specs após a decisão prática de inserir `PR-003 — Assets de dados MVP` antes dos managers e gameplay.

---

## 1. Decisão consolidada

A execução real da Fase 8 passa a usar esta ordem de fundação:

```text
PR-001 — Core foundation
PR-002 — Data contracts e registries
PR-003 — Assets de dados MVP
PR-004 — Editor data validator
PR-005 — Bootstrap managers vazios
PR-006 — NewGameState e inventário inicial
```

Racional:

- Managers e inventário inicial não devem depender de dados hardcoded.
- `PlayerDataSO`, `ItemDatabaseSO` e `SeedDatabaseSO` precisam existir antes dos systems que consomem dados.
- Criar os assets antes do gameplay reduz risco de Codex espalhar valores fixos em MonoBehaviour.
- Um validador Editor-only logo após os assets reduz risco de IDs duplicados, referências quebradas e registries incompletos.

---

## 2. Impacto em arquitetura

### 2.1 Fonte de dados do MVP

A partir do PR-003, o MVP deve usar assets em:

```text
Assets/_Game/Data/Items/
Assets/_Game/Data/Seeds/
Assets/_Game/Data/Config/
Assets/_Game/Data/Registries/
```

Sistemas futuros devem receber referências via `[SerializeField]` ou bootstrap/installer de cena, nunca por busca global em runtime.

### 2.2 IDs estáveis

IDs mínimos obrigatórios do MVP:

```text
seed_wheat
seed_carrot
item_crop_wheat
item_crop_carrot
item_fish_common
item_wood
item_tool_fishing_rod_basic
```

Regra: save e eventos persistíveis usam IDs, não referências Unity.

### 2.3 Cana Básica

A ferramenta inicial de pesca é parte do inventário inicial mínimo e deve existir como item:

```text
Asset: Assets/_Game/Data/Items/Item_Cana_Basica.asset
Id: item_tool_fishing_rod_basic
Category: Tool
MaxStack: 1
IsEquippable: true
```

`PlayerData.asset` deve incluir:

```text
Item_Semente_Trigo x5
Item_Semente_Cenoura x3
Item_Cana_Basica x1
```

### 2.4 Sprites de crescimento

`SeedDataSO.GrowthStageSprites` pode ficar vazio durante o PR-003.

Todo código futuro de plantio/crescimento deve tratar:

```text
GrowthStageSprites == null
GrowthStageSprites.Length == 0
índice de estágio fora do range
```

Fallback esperado:

```text
usar sprite placeholder seguro ou cor/estado visual simples sem lançar exceção
```

Nenhum PR futuro pode assumir que sprites finais existem.

---

## 3. Impacto em histórias FARM

### 3.1 FARM-021 — Inventário base

Antes de implementar inventário funcional, garantir que `PlayerData.asset` contenha os itens iniciais:

```text
seed_wheat x5
seed_carrot x3
item_tool_fishing_rod_basic x1
```

Critério adicional:

- inventário inicial deve ser construído a partir de `PlayerDataSO.StartingItems`, não hardcoded no manager.

### 3.2 FARM-051 / FARM-052 — Pesca

A pesca futura depende de:

```text
item_tool_fishing_rod_basic
item_fish_common
```

Critério adicional:

- sistema de pesca deve checar posse da cana por ID ou por item de categoria Tool apropriado;
- pesca deve adicionar `item_fish_common` por ID/resolução via registry.

### 3.3 FARM-012 / FARM-013 / FARM-015 — Plantio, crescimento e colheita

Plantio deve depender de:

```text
SeedDatabaseSO
SeedDataSO.SeedItem
SeedDataSO.HarvestItems
SeedDataSO.HarvestAmounts
```

Critérios adicionais:

- plantio consome o item `SeedItem.Id`;
- colheita adiciona os itens definidos em `HarvestItems`/`HarvestAmounts`;
- se `HarvestItems` e `HarvestAmounts` tiverem tamanhos diferentes, validator deve falhar antes do gameplay.

---

## 4. Impacto em specs

### 4.1 PR-003 deve ser considerado pré-requisito de gameplay

Specs de inventário, plantio, pesca, venda e save devem assumir que estes assets existem:

```text
ItemDatabase.asset
SeedDatabase.asset
PlayerData.asset
```

### 4.2 PR-004 recomendado — Editor data validator

Antes de managers e gameplay, criar ferramenta editor-only:

```text
Assets/_Game/Scripts/Editor/DataValidation/CindarsHopeDataValidator.cs
```

Menu esperado:

```text
CindarsHope/Validate/Validate MVP Data
```

Validações mínimas:

- IDs vazios.
- IDs duplicados.
- ItemDatabase contém os 7 itens mínimos.
- SeedDatabase contém as 2 seeds mínimas.
- SeedDataSO.SeedItem preenchido.
- SeedDataSO.HarvestItems preenchido.
- SeedDataSO.HarvestAmounts com mesmo tamanho de HarvestItems.
- PlayerData.StartingItems contém trigo x5, cenoura x3, cana básica x1.
- Nenhuma referência obrigatória está nula.

---

## 5. Atualização de sequência operacional

A sequência recomendada após PR-003 é:

```text
1. Validar PR-003 no Unity.
2. Garantir que Cana Básica está no ItemDatabase e PlayerData.
3. Remover arquivos fora de escopo, como BillingMode.json, se aparecerem.
4. Mergear PR-003 em dev.
5. Executar PR-004 — Editor data validator.
6. Só então seguir para managers.
```

---

## 6. Itens que não devem ser feitos ainda

Ainda não implementar:

```text
InventoryManager funcional
PlayerManager funcional
FarmScene
Plantio
Colheita
Save/load
HUD
sprites finais
prefabs finais
```

Esses itens continuam nas próximas fatias da Fase 8.

---

## 7. Fonte de verdade

Para sequência operacional da Fase 8, usar esta prioridade:

```text
1. PROJECT_LOG.md
2. docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md, já sincronizado para v1.1 no conteúdo
3. docs/FASE8_SYNC_PR003_ASSETS_v1.0.md
4. docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md
5. docs/FASE7_SPEC_MVP_FARM_v2.2.md
6. docs/GDD_v2.6.md
7. docs/ARCH_fase4_v2.2.md
```

Se algum documento antigo citar a ordem anterior, seguir esta sincronização e registrar a divergência no `PROJECT_LOG.md`.
