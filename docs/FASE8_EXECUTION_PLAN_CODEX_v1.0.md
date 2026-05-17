# Cindar's Hope — Fase 8: Plano de Execução com Codex v1.1

> **Fase:** 8 de 13  
> **Status:** Pronto para execução local, com sequência de PRs sincronizada em 2026-05-16  
> **Objetivo:** implementar o MVP Fazenda por PRs pequenos, rastreáveis e revisáveis  
> **Depende de:** `PROJECT_LOG.md`, `AGENTS.md`, `CLAUDE.md`, `GDD_v2.6.md`, `ARCH_fase4_v2.2.md`, `FASE7_SPEC_MVP_FARM_v2.2.md`, `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`

---

## 0. Nota de sincronização v1.1

Durante a execução real da Fase 8, a sequência foi ajustada para reduzir risco de retrabalho:

- `PR-001` continua sendo **Core foundation**.
- `PR-002` continua sendo **Data contracts e registries**.
- `PR-003` passa a ser **Assets de dados MVP**, antes de managers e gameplay.
- A partir do antigo `PR-003`, os PRs de código foram deslocados.
- A ferramenta inicial **Cana Básica** deve entrar no pacote de dados MVP, porque o inventário inicial planejado já depende dela.
- `GrowthStageSprites` pode ficar vazio no `PR-003`, mas todo código futuro de `CropTile` deve tratar array vazio/nulo com fallback visual.
- Qualquer agente deve ler e atualizar `PROJECT_LOG.md` antes/depois de tarefas relevantes.

Esta versão substitui a ordem prática da seção 6. Se houver conflito entre esta sequência e versões antigas citadas em outros documentos, usar esta sequência e registrar a divergência no `PROJECT_LOG.md`.

---

## 0.2 Nota de sincronização pós PR-008

PR-001 a PR-008 já foram executados e sincronizados em `dev`. O PR-009 é documental e não altera runtime, assets, cenas, prefabs ou gameplay.

O próximo PR runtime será movimento/input. Antes dele:

- Confirmar New Input System instalado/ativo (`com.unity.inputsystem`).
- Confirmar Cinemachine instalado se o PR usar Cinemachine (`com.unity.cinemachine`).
- Criar `PlayerInputActions.inputactions` no PR de movimento/input, com Action Map `Player`:
  - `Move`
  - `Interact`
  - `Inventory`
  - `Sleep`

Política de VFX:

- VFX não é dependência obrigatória dos sistemas MVP.
- Sistemas de gameplay não devem depender de prefabs VFX inexistentes.
- Feedbacks visuais podem ficar para PR dedicado de polish/feedback.

---

## 1. Princípio central

O Codex deve implementar **fatias pequenas**, não “o jogo inteiro”.

A unidade correta de trabalho é:

```text
1 PR pequeno → 1 objetivo → poucos arquivos → teste manual claro → commit em português
```

O objetivo da Fase 8 é concluir este loop:

```text
BootScene → FarmScene → inventário inicial → plantar → avançar dias → colher → vender → salvar → fechar → reabrir → estado restaurado
```

---

## 2. Regras obrigatórias para qualquer prompt Codex

Todo prompt para Codex deve conter:

1. Documentos a ler.
2. PR alvo.
3. Escopo permitido.
4. Arquivos permitidos.
5. Arquivos proibidos.
6. Critérios de aceite.
7. Teste manual.
8. Regras de arquitetura.
9. O que não fazer.
10. Instrução para atualizar `PROJECT_LOG.md`.

### Prompt base

```md
Leia primeiro:
- PROJECT_LOG.md
- AGENTS.md
- CLAUDE.md
- docs/GDD_v2.6.md
- docs/ARCH_fase4_v2.2.md
- docs/FASE7_SPEC_MVP_FARM_v2.2.md
- docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md
- docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md

Implemente somente:
<PR-ID> — <nome>

Objetivo:
<uma frase>

Branch esperada:
feature/fase8-pr-<numero>-<nome-curto>

Arquivos permitidos:
- <lista exata>

Arquivos proibidos:
- docs/GDD_v2.6.md
- docs/ARCH_fase4_v2.2.md
- docs/FASE7_SPEC_MVP_FARM_v2.2.md
- docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md
- qualquer arquivo fora da lista permitida

Regras:
- Não implementar V2/FULL.
- Não ampliar escopo.
- Não usar GameObject.Find, FindObjectOfType ou FindObjectsByType em runtime.
- Não usar StreamingAssets para save.
- Não serializar referências Unity em JSON.
- Dados de jogo devem ficar em ScriptableObject.
- Comunicação entre sistemas deve usar GameEventBus.
- Atualizar PROJECT_LOG.md ao final.

Ao final, entregue:
- arquivos alterados;
- resumo técnico curto;
- teste manual executável;
- riscos/pendências;
- próximo PR sugerido.
```

---

## 3. Branch strategy

```bash
git checkout dev
git pull origin dev
git checkout -b feature/fase8-pr-001-core-foundation
```

Padrão:

```text
feature/fase8-pr-<numero>-<nome-curto>
```

Exemplos:

```text
feature/fase8-pr-001-core-foundation
feature/fase8-pr-003-mvp-data-assets
feature/fase8-pr-010-plantio-basico
feature/fase8-pr-018-load-boot
```

Commits em português:

```bash
git commit -m "feat: adicionar fundação de eventos core"
git commit -m "feat: adicionar assets de dados do mvp"
git commit -m "fix: corrigir serialização do inventário por id"
git commit -m "test: adicionar validação de stack do inventário"
```

---

## 4. Definition of Ready por PR

Antes de abrir tarefa no Codex:

- [ ] O PR cabe em revisão humana curta.
- [ ] Há lista de arquivos permitidos.
- [ ] Há lista de arquivos proibidos.
- [ ] Há teste manual objetivo.
- [ ] O escopo é MVP, não V2/FULL.
- [ ] O repositório está limpo (`git status`).
- [ ] Unity abre sem erro antes da mudança.
- [ ] `PROJECT_LOG.md` foi consultado.
- [ ] Branch atual corresponde ao PR.

---

## 5. Definition of Done por PR

Um PR só está pronto quando:

- [ ] Unity compila sem erro.
- [ ] Console não tem erros novos.
- [ ] Teste manual passa.
- [ ] Regras de `AGENTS.md` e `CLAUDE.md` foram respeitadas.
- [ ] `PROJECT_LOG.md` foi atualizado.
- [ ] Não há busca global em runtime.
- [ ] Não há hardcode de dados de jogo em MonoBehaviour.
- [ ] Save, se envolvido, usa `Application.persistentDataPath`.
- [ ] Save, se envolvido, usa IDs e tipos simples.
- [ ] Alterações estão pequenas e revisáveis.
- [ ] Commit em português criado.

---

## 6. Sequência de PRs da Fase 8 — ordem vigente

### PR-001 — Core foundation

**Objetivo:** criar fundação mínima de eventos.

**Specs relacionadas:** FARM-001, ARCH seção GameEventBus.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Core/GameEventBus.cs
Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs
Assets/_Game/Scripts/Core/Events/GoldChangedEvent.cs
Assets/_Game/Scripts/Core/Events/InventoryChangedEvent.cs
Assets/_Game/Scripts/Core/Events/SeedPlantedEvent.cs
Assets/_Game/Scripts/Core/Events/CropHarvestedEvent.cs
Assets/_Game/Scripts/Core/Events/TreeChoppedEvent.cs
Assets/_Game/Scripts/Core/Events/FishCaughtEvent.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
Assets/_Game/Scripts/Core/Events/GameSavedEvent.cs
```

**Não fazer:** Player, cena, UI, inventário.

**Teste manual:** Unity compila; nenhum erro no Console.

---

### PR-002 — Data contracts e registries

**Objetivo:** criar contratos de dados por ID, ScriptableObjects base e registries.

**Specs relacionadas:** FARM-011, FARM-021, FARM-071.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Core/Data/IIdentifiedData.cs
Assets/_Game/Scripts/Core/Data/IDataRegistry.cs
Assets/_Game/Scripts/Core/Data/DataRegistrySO.cs
Assets/_Game/Scripts/Core/Data/ItemDatabaseSO.cs
Assets/_Game/Scripts/Core/Data/SeedDatabaseSO.cs
Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs
Assets/_Game/Scripts/Player/Data/PlayerDataSO.cs
PROJECT_LOG.md
```

**Teste manual:** criar assets manualmente no Unity; menus `CindarsHope/Data/*` e `CindarsHope/Database/*` aparecem; campos aparecem no Inspector.

---

### PR-003 — Assets de dados MVP

**Objetivo:** criar os assets ScriptableObject reais do MVP Fazenda, incluindo itens, sementes, PlayerData e registries.

**Motivo da posição:** managers e inventário inicial devem depender de dados concretos, não de dados hardcoded.

**Arquivos permitidos:**

```text
Assets/_Game/Data/Items/Item_Semente_Trigo.asset
Assets/_Game/Data/Items/Item_Semente_Cenoura.asset
Assets/_Game/Data/Items/Item_Trigo.asset
Assets/_Game/Data/Items/Item_Cenoura.asset
Assets/_Game/Data/Items/Item_Fish_Common.asset
Assets/_Game/Data/Items/Item_Wood.asset
Assets/_Game/Data/Items/Item_Cana_Basica.asset
Assets/_Game/Data/Seeds/Seed_Trigo.asset
Assets/_Game/Data/Seeds/Seed_Cenoura.asset
Assets/_Game/Data/Config/PlayerData.asset
Assets/_Game/Data/Registries/ItemDatabase.asset
Assets/_Game/Data/Registries/SeedDatabase.asset
Assets/_Game/Data/**.meta
PROJECT_LOG.md
```

**Dados obrigatórios:**

```text
Item_Semente_Trigo
Id: seed_wheat
Category: Seed
MaxStack: 20
BaseValue: 2

Item_Semente_Cenoura
Id: seed_carrot
Category: Seed
MaxStack: 20
BaseValue: 3

Item_Trigo
Id: item_crop_wheat
Category: Crop
MaxStack: 99
BaseValue: 5
HungerRestore: 20

Item_Cenoura
Id: item_crop_carrot
Category: Crop
MaxStack: 99
BaseValue: 8
HungerRestore: 25

Item_Fish_Common
Id: item_fish_common
Category: Fish
MaxStack: 20
BaseValue: 10
HungerRestore: 15

Item_Wood
Id: item_wood
Category: Material
MaxStack: 99
BaseValue: 1

Item_Cana_Basica
Id: item_tool_fishing_rod_basic
Category: Tool
MaxStack: 1
BaseValue: 0
IsEquippable: true
```

**PlayerData obrigatório:**

```text
MoveSpeed: 5
BaseHP: 100
StartingGold: 50
StartingItems:
- Item_Semente_Trigo x5
- Item_Semente_Cenoura x3
- Item_Cana_Basica x1
```

**SeedData obrigatório:**

```text
Seed_Trigo
Id: seed_wheat
SeedItem: Item_Semente_Trigo
HarvestItems: [Item_Trigo]
HarvestAmounts: [3]
GrowthDays: 3
Period: Both
MinYield: 3
MaxYield: 3
FertilizerYieldMultiplier: 1

Seed_Cenoura
Id: seed_carrot
SeedItem: Item_Semente_Cenoura
HarvestItems: [Item_Cenoura]
HarvestAmounts: [2]
GrowthDays: 4
Period: Both
MinYield: 2
MaxYield: 2
FertilizerYieldMultiplier: 1
```

**Regra sobre sprites:** `GrowthStageSprites` pode ficar vazio neste PR. Todo código futuro de `CropTile` deve tratar vazio/nulo com fallback visual.

**Não fazer:** scripts C#, gameplay, cena, UI, prefabs, sprites finais.

**Teste manual:** abrir assets no Inspector, validar IDs, referências e registries; Console sem erro.

---

### PR-004 — Editor data validator

**Objetivo:** criar ferramenta editor-only para validar dados MVP antes de gameplay.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Editor/DataValidation/CindarsHopeDataValidator.cs
PROJECT_LOG.md
```

**Menu esperado:**

```text
CindarsHope/Validate/Validate MVP Data
```

**Validações mínimas:**

- IDs vazios.
- IDs duplicados por registry.
- `ItemDatabase` contém os itens esperados do MVP.
- `SeedDatabase` contém as seeds esperadas do MVP.
- `SeedDataSO.SeedItem` preenchido.
- `SeedDataSO.HarvestItems` preenchido.
- `SeedDataSO.HarvestAmounts` com mesmo tamanho de `HarvestItems`.
- `PlayerData.StartingItems` sem item nulo.
- `PlayerData.StartingItems` contém trigo x5, cenoura x3 e cana básica x1.

**Não fazer:** runtime gameplay, managers, cenas.

**Teste manual:** rodar menu de validação e confirmar relatório sem erros para os assets do PR-003.

---

### PR-005 — Bootstrap managers vazios

**Objetivo:** criar managers persistentes mínimos sem gameplay completo.

**Arquivos permitidos:**

```text
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Core/Time/TimeManager.cs
Assets/_Game/Scripts/Save/SaveManager.cs
PROJECT_LOG.md
```

**Regras:** managers podem compilar com métodos mínimos/stubs, mas não devem fingir feature completa.

**Teste manual:** objeto bootstrap inicializa managers uma vez; não duplica em reload.

---

### PR-006 — NewGameState e inventário inicial

**Objetivo:** iniciar estado novo com ouro, HP e itens iniciais a partir de `PlayerDataSO`.

**Specs relacionadas:** FARM-021.

**Entregável:** inventário interno com stack, sem UI ainda.

**Teste manual:** Debug/Inspector mostra Semente Trigo x5, Semente Cenoura x3, Cana Básica x1, ouro inicial.

---

### PR-007 — FarmScene mínima

**Objetivo:** criar cena mínima com tilemap, player placeholder e câmera.

**Specs relacionadas:** FARM-001, FARM-003.

**Preferência:** criar Editor script `CreateMvpFarmScene.cs` para reproduzir a cena.

**Teste manual:** abrir FarmScene e ver jogador, chão e bordas.

---

### PR-008 — Movimento e colisão

> Nota pós PR-009 documental: esta entrada representa o próximo PR runtime de movimento/input na sequência operacional atual. A numeração histórica deste plano não deve ser renumerada destrutivamente.

**Pré-requisitos:**

- Confirmar New Input System instalado/ativo (`com.unity.inputsystem`).
- Confirmar Cinemachine instalado se o PR usar Cinemachine (`com.unity.cinemachine`).
- Criar `PlayerInputActions.inputactions` neste PR, com Action Map `Player`: `Move`, `Interact`, `Inventory`, `Sleep`.

**Objetivo:** player se move com WASD/setas, câmera segue e bordas bloqueiam.

**Specs relacionadas:** FARM-002.

**Teste manual:** mover nas 4 direções; diagonal não acelera; jogador não atravessa borda.

---

### PR-009 — Interação genérica

**Objetivo:** implementar `IInteractable`, `InteractionSystem` e hint textual simples.

**Specs relacionadas:** FARM-014.

**Teste manual:** objeto fake interagível mostra hint e responde ao E.

---

### PR-010 — Canteiros e plantio

**Objetivo:** 9 canteiros, menu textual e plantio de sementes.

**Specs relacionadas:** FARM-012, FARM-016.

**Regra de sprite fallback:** se `GrowthStageSprites` estiver vazio/nulo, usar placeholder seguro e não lançar exceção.

**Teste manual:** aproximar do canteiro, pressionar E, escolher trigo, semente reduz, canteiro muda visual.

---

### PR-011 — Crescimento por dia

**Objetivo:** TAB avança dia, publica `DayStartedEvent`, plantas crescem.

**Specs relacionadas:** FARM-013, FARM-031, FARM-032.

**Teste manual:** plantar trigo, avançar 3 dias, canteiro fica pronto.

---

### PR-012 — Colheita e inventário visual

**Objetivo:** colher planta pronta e ver item no inventário textual.

**Specs relacionadas:** FARM-015, FARM-021, FARM-022.

**Teste manual:** colher trigo, inventário mostra quantidade correta, canteiro volta vazio.

---

### PR-013 — Fome e consumo

**Objetivo:** HungerSystem, HUD simples e consumo de comida.

**Specs relacionadas:** FARM-061, FARM-062.

**Teste manual:** fome diminui por passos; usar trigo/peixe restaura fome.

---

### PR-014 — Árvores

**Objetivo:** cortar árvore e receber madeira.

**Specs relacionadas:** FARM-041, FARM-042.

**Teste manual:** E na árvore adiciona madeira e muda nível visual.

---

### PR-015 — Lago e pesca

**Objetivo:** pescar Peixe Comum no lago com cana básica.

**Specs relacionadas:** FARM-051, FARM-052.

**Dependência:** `Item_Cana_Basica.asset` e `PlayerData.StartingItems` com cana básica x1.

**Teste manual:** E no FishingSpot espera 3s e adiciona peixe.

---

### PR-016 — Venda

**Objetivo:** SellPoint e SellMenu textual para vender colheitas/peixe/madeira.

**Specs relacionadas:** FARM-SELL, FARM-032.

**Teste manual:** vender trigo aumenta ouro e remove item.

---

### PR-017 — Save

**Objetivo:** salvar estado em JSON.

**Specs relacionadas:** FARM-071.

**Regras específicas:**

- Path: `Application.persistentDataPath/saves/slot_1.json`.
- Serializar IDs e tipos simples.
- Incluir `SchemaVersion`.

**Teste manual:** TAB/dormir gera `slot_1.json` com dia, ouro, fome, inventário e plots.

---

### PR-018 — Load e Boot

**Objetivo:** carregar save existente e restaurar estado.

**Specs relacionadas:** FARM-072.

**Teste manual:** plantar, avançar dias, colher/vender, salvar, fechar, abrir, estado restaurado.

---

### PR-019 — Feedbacks mínimos

**Objetivo:** popups/fade/VFX mínimos sem alterar regras de gameplay.

**Specs relacionadas:** FARM-015, FARM-031, FARM-042, FARM-052.

**Teste manual:** feedback visual aparece e desaparece sem travar input.

---

### PR-020 — Hardening MVP

**Objetivo:** corrigir bugs, remover logs temporários, validar loop vertical.

**Teste final:**

```text
Novo jogo → plantar trigo → dormir 3x → colher → vender → salvar → fechar → reabrir → dia/ouro/inventário/canteiros restaurados
```

---

## 7. Como revisar saída do Codex

### 7.1 Comandos

```bash
git status
git diff --stat
git diff
```

### 7.2 Busca por violações

```bash
grep -R "GameObject.Find\|FindObjectOfType\|FindObjectsByType\|StreamingAssets" Assets/_Game/Scripts
```

Se aparecer em código runtime, revisar ou rejeitar.

### 7.3 Sinais de PR ruim

- Muitos arquivos alterados fora do escopo.
- Implementação de V2/FULL escondida.
- Dados hardcoded em MonoBehaviour.
- Save serializando `ScriptableObject`.
- Managers se chamando diretamente sem evento.
- Código compila, mas cena depende de configuração manual não documentada.
- `PROJECT_LOG.md` não atualizado.

---

## 8. Política de rollback

Se Codex gerar algo ruim:

```bash
git restore <arquivo>
# ou, se for tudo da tentativa:
git reset --hard HEAD
```

Nunca tentar “consertar em cima” de uma alteração grande e confusa. Melhor reduzir o prompt e refazer.
