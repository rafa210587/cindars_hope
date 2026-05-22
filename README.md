# Cindar's Hope — 2D Pixel Art RPG + Farm Sim

Um jogo 2D em estilo pixel art combinando simulação de fazenda com exploração, combate, comércio, crafting e progressão por ferramentas.

## Estado Atual

- **Fase implementada:** FASE 9B-1 Cave/Combat MVP ✅ concluída
- **Próxima fase planejada:** FASE 9C — Tools, Farm Actions e Combat Refinement
- **Branch principal:** `dev`
- **Last Updated:** 2026-05-18

### Sistemas Implementados

- ✅ **Core:** EventBus, TimeManager, SaveManager, BootstrapManager
- ✅ **Farm MVP:** PlotSystem, TreeSystem, FishingSpot, ItemPickup, crescimento e colheita
- ✅ **Town MVP:** NPC Pip, BuyItemPoint, SellAllPoint, PortalSystem
- ✅ **Economy:** Gold, compra, venda e política de itens vendáveis
- ✅ **Hunger:** HungerManager, FoodConsumer, perda de HP por fome vazia
- ✅ **Crafting MVP:** CraftingManager, CraftingPoint, RecipeDatabase
- ✅ **Save/Load:** JSON persistence, cross-scene restoration, CurrentScene tracking
- ✅ **Cave MVP:** CaveScene, Portal Farm↔Cave, spawn points
- ✅ **Combat MVP:** ataque melee simples, EnemyHealth, EnemyChaseController, EnemyContactDamage, drops, hit flash e knockback
- ✅ **Scene Generators:** CreateMvpFarmScene, CreateMvpTownScene, CreateMvpCaveScene
- ✅ **Validators:** MvpSceneValidator para Farm/Town/Cave

### Limitações atuais assumidas

- Plantio ainda é simplificado; precisa sair de seleção automática de seed para seed ativa/UI/hotbar.
- Árvores ainda precisam exigir machado para corte real.
- Pesca já exige vara por ID, mas ainda não usa um sistema genérico de ferramentas.
- Inventário já tem item equipável, mas ainda não há EquipmentManager.
- Combate já tem base data-driven para inimigo, mas o ataque do player ainda precisa migrar para arma equipada.
- Ainda não há dodge lateral/para trás.
- Ainda não há arma à distância física nem magia à distância.

### Menus Unity Disponíveis

```text
CindarsHope/
├── Scenes/
│   ├── Create MVP FarmScene        → gera Farm com plots/trees/shops
│   ├── Create MVP TownScene        → gera Town com NPCs/buy/sell points
│   └── Create MVP CaveScene        → gera Cave com portal/Slime/spawn points
└── Validate/
    ├── Validate MVP Data           → valida ScriptableObjects do jogo
    ├── Validate Farm Town MVP      → valida estrutura de FarmScene e TownScene
    └── Validate Cave MVP           → valida estrutura de CaveScene, quando disponível
```

## Como Abrir no Unity

1. Clone o repositório fora de OneDrive/Dropbox/Google Drive.
2. Abra em Unity LTS.
3. Regere as cenas pelos menus `CindarsHope/Scenes/*` se necessário.
4. Abra FarmScene ou CaveScene.
5. Play Mode: WASD move, E interage, Tab avança dia, J ataca.

## Fluxo Operacional de Agentes

### Regra de Continuidade

Antes de qualquer tarefa:

1. Ler `PROJECT_LOG.md`.
2. Ler `AGENTS.md` / `CLAUDE.md`.
3. Ler documentos de referência da tarefa.
4. Confirmar branch e escopo.

Ao final de tarefa relevante:

- Atualizar `PROJECT_LOG.md` com branch, escopo, arquivos, testes e pendências.
- Listar arquivos permitidos/proibidos.
- Executar `git status --short` e `git diff --stat` antes do commit local.

### Git Policy para Agentes

**Permitido:**

- Criar/usar branch local indicada.
- Alterar somente arquivos permitidos no escopo.
- Criar commits locais em português.
- Atualizar `PROJECT_LOG.md`.
- Entregar lista de commits, arquivos e testes.

**Proibido sem pedido humano explícito:**

- `git push`
- abrir PR/MR
- `git merge`
- `git stash`
- `git clean`
- `git reset --hard`
- commitar fora do escopo

## Arquitetura Técnica

### Padrões

- **EventBus:** comunicação via `GameEventBus.Publish/Subscribe`.
- **Persistência:** `GameBootstrap` singleton + managers persistentes entre cenas.
- **SceneInstallers:** `FarmSceneRuntimeReferenceInstaller`, `TownSceneRuntimeReferenceInstaller`, `CaveSceneRuntimeReferenceInstaller`.
- **Serialização:** JSON via SaveData; nunca serializar `GameObject`, `Transform`, `MonoBehaviour` ou `ScriptableObject`.
- **Dados:** stats, itens, inimigos, receitas, ferramentas e armas devem ser ScriptableObject.
- **Detecção gameplay:** preferir componente a tag (`GetComponentInParent<T>()`), sem depender de ProjectSettings.

### Contratos atuais importantes

**Cross-Scene:**

- `GameBootstrap.Instance` persiste entre cenas.
- `InventoryManager`, `PlayerManager`, `TimeManager`, `SaveManager`, `HungerManager`, `CraftingManager`, `EconomyManager` persistem.
- Save guarda `CurrentSceneName`/`CurrentScenePath`.

**Combat MVP:**

- `PlayerAttackController` detecta `EnemyHealth` por componente.
- `EnemyContactDamage` detecta `PlayerManager` via `GameBootstrap.Instance` ou componente.
- `EnemyChaseController` recebe target via gerador.
- `EnemyDataSO` centraliza stats de inimigo.

**FASE 9C planejada:**

- `ToolDataSO`, `ToolTier`, `ToolRequirement`, `EquipmentManager`.
- `WeaponDataSO`, `WeaponType`, `WeaponDatabaseSO`.
- `PlayerCombatController` substituindo ataque hardcoded.
- `PlayerDodgeController` para esquiva lateral/para trás.

## Smoke Test Checklist atual

- [ ] Farm: plantar seed → passar dias → colher → vender → salvar/carregar.
- [ ] Town: andar, falar com Pip, comprar seeds, voltar à Farm.
- [ ] Cave: ir da Farm, andar na Cave, socar Slime, receber/causar dano, voltar à Farm.
- [ ] Save: salvar em qualquer cena, fechar jogo, reabrir, estado restaurado.
- [ ] HUD: HP, Gold, Hunger, Inventory aparecem e atualizam.
- [ ] Console: sem erro vermelho.

## Próximas Waves Recomendadas

A prioridade agora é **FASE 9C — Tools, Farm Actions e Combat Refinement**, antes de UI final e antes de arte final.

1. **PR-100** — Tool contracts: `ToolType`, `ToolTier`, `ToolDataSO`, `ToolRequirement`, eventos.
2. **PR-101** — Tool assets MVP: Hoe, Sickle, Axe, Pickaxe, FishingRod.
3. **PR-102** — `EquipmentManager` mínimo + save/load de ferramenta/arma.
4. **PR-103** — Plantio com ferramenta e seed explícita.
5. **PR-104** — Colheita com ferramenta/yield/tier.
6. **PR-105** — Árvores exigem Axe para corte real; fallback limitado sem ferramenta.
7. **PR-106** — FishingSpot migra para `ToolRequirement`.
8. **PR-107** — Weapon contracts: `WeaponType`, `WeaponDataSO`, `WeaponDatabaseSO`.
9. **PR-108** — Weapon slot no `EquipmentManager`.
10. **PR-109** — `PlayerCombatController` melee data-driven.
11. **PR-110** — Dodge lateral/para trás.
12. **PR-111** — Arma de distância física com projétil.
13. **PR-112** — Magia à distância com projétil mágico.
14. **Depois:** UI real MVP, primeira quest, visual slice e arte final.

## Documentação Principal

- **README.md** ← você está aqui
- **PROJECT_LOG.md** — histórico de PRs, decisões, estado
- **AGENTS.md** — regras de agente e fluxo git
- **CLAUDE.md** — regras de código, convenções e padrões
- **docs/design/GDD_v2.6.md** — design do jogo completo
- **docs/architecture/ARCH_fase4_v2.2.md** — arquitetura técnica detalhada
- **docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md** — contratos core, eventos, IDs e save
- **docs_old/FASE9A_HANDOFF_FARM_TOWN_SAVE_v1.0.md** — entrega FASE 9A
- **docs_old/FASE9B_CAVE_COMBAT_MVP_v1.0.md** — entrega FASE 9B-1
- **docs_old/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md** — próxima fase de ferramentas/farm/combat
- **docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md** — roadmap pós 9B-1, agora apontando para 9C

## Contato / Issues

Reportar bugs ou sugestões em `PROJECT_LOG.md` com contexto, reprodução e cena afetada.

---

**Branch:** `dev`  
**Build Status:** ✅ MVP compila/roda conforme estado registrado; validação Unity local ainda deve ser feita após cada wave.
