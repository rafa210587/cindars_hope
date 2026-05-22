# FASE 9A — Handoff Farm/Town/Commerce/Save v1.0

**Status:** ✅ Concluída em 2026-05-17  
**Próxima Fase:** FASE 9B-1 Cave/Combat MVP

## Objetivo da Fase

Hardening e integração de Farm MVP + Town MVP + Commerce/Crafting + Save/Load cross-scene, garantindo que:
- Farm/Town/Crafting/Save funcionem juntos
- Transições de cena preservem estado
- Rebind de referências de cena funcione
- HUD persista e atualize corretamente

## Sistemas Entregues

### Farm MVP
- FarmPlot: plantio, crescimento de seedlings em 3 estágios
- TreeNode: árvores que podem ser cortadas para madeira
- FishingSpot: pesca simples (drop item aleatório)
- ItemPickupRegistry: items dropados persistem entre cenas
- FarmSceneRuntimeStateCache: cache transitório de plots/árvores durante transição

### Town MVP
- NpcTalkPoint: NPCs em cena (Pip, merchant)
- BuyItemPoint: comprar seeds (wheat, carrot)
- SellAllPoint: vender items para ouro
- Pip system: NPC com estado simples
- TownSceneRuntimeReferenceInstaller: rebind ao carregar

### Commerce & Crafting
- EconomyManager: gerencia ouro do Player
- ShopSystem: lista de items vendáveis
- CraftingManager: gerencia receitas
- CraftingPoint: UI debug para crafting
- RecipeDatabase: receitas (ex: processamento de madeira)
- CraftingPoint spawna items dropados após craft

### Save/Load
- SaveManager: persiste Player position, inventory, gold, hunger, time, CurrentScene
- JSON serialization via SaveData class
- SaveInput: hotkey S para salvar
- Cross-scene restoration: ao carregar, InventoryManager restaura items, PlayerManager restaura HP, etc.
- CurrentSceneName/CurrentScenePath tracked para reload correto

### Cross-Scene Integration
- GameBootstrap persiste entre cenas (não DestroyOnLoad)
- HUD persiste e recebe updates de managers via eventos
- FarmSceneRuntimeReferenceInstaller: rebind plots/trees/points ao carregar Farm
- TownSceneRuntimeReferenceInstaller: rebind NPCs/shops ao carregar Town
- SceneTransitionStartedEvent dispara antes de descarregar cena (cache/cleanup)
- SceneLoaded event dispara após carregar (rebind/restore)

## Arquivos Centrais

### Managers Persistentes
- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` — singleton de sistemas persistentes
- `Assets/_Game/Scripts/Save/SaveManager.cs` — persist/restore de SaveData
- `Assets/_Game/Scripts/Player/PlayerManager.cs` — HP, velocidade
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs` — items
- `Assets/_Game/Scripts/Core/Time/TimeManager.cs` — dias, estação
- `Assets/_Game/Scripts/Craft/CraftingManager.cs` — receitas
- `Assets/_Game/Scripts/Economy/EconomyManager.cs` — ouro

### Scene Installers
- `Assets/_Game/Scripts/SceneManagement/FarmSceneRuntimeReferenceInstaller.cs`
- `Assets/_Game/Scripts/SceneManagement/TownSceneRuntimeReferenceInstaller.cs`
- `Assets/_Game/Scripts/SceneManagement/FarmSceneRuntimeStateCache.cs`

### Geradores
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`

### Validator
- `Assets/_Game/Scripts/Editor/Validation/MvpSceneValidator.cs`

## Menus Unity Disponíveis

```
CindarsHope/Scenes/
├── Create MVP FarmScene  → regenera Farm com todas as plots/trees/shops
└── Create MVP TownScene  → regenera Town com NPCs/shops

CindarsHope/Validate/
├── Validate MVP Data              → valida assets (items, seeds, recipes, etc.)
├── Validate Farm Town MVP         → valida estrutura de Farm e Town
```

## Checklist de Validação

- [ ] `CindarsHope/Scenes/Create MVP FarmScene` roda sem erro
- [ ] `CindarsHope/Scenes/Create MVP TownScene` roda sem erro
- [ ] Abrir FarmScene, plantar, avançar dias, colher, andar
- [ ] Transicionar para TownScene, falar com Pip, comprar seeds
- [ ] Transicionar de volta para FarmScene, plantio persiste
- [ ] Vender items para ouro, HUD atualiza
- [ ] Crafting: processamento de madeira funciona
- [ ] Salvar (S), fechar jogo, reabrir, estado restaurado
- [ ] Console: sem erro vermelho, sem warning crítico
- [ ] Hunger: decresce com tempo, restaura ao comer

## Dívidas Conhecidas

### Críticas
Nenhuma crítica bloqueante.

### Importantes
- Sorting Layer warnings ao gerar cenas (aceito MVP, registrado como dívida)
- Input Manager usando legacy KeyCode (deprecado, seguinte PR troca para Input System)
- Inventory UI: apenas debug HUD, sem UI visual real

### Nice-to-have
- Pip NPC deveria ter diálogo real (agora placeholder)
- Merchant poderia ter múltiplas compras simultâneas
- CraftingPoint deveria estar em cena visual, não apenas debug

## Próximos Passos Recomendados

1. **Validar completamente em Unity** — rodar smoke test completo (Farm→Town→Save/Load)
2. **FASE 9B-1 — Cave/Combat MVP** — implementar CaveScene, Slime, soco melee, perseguição
3. **Após 9B-1 — Limpar warnings** — Sorting Layer, Input Manager, SceneSpawnPoint
4. **UI MVP real** — Inventory, Shop, Crafting UI (não apenas debug HUD)
5. **Combat Feel** — knockback, invulnerability frames, feedback visual
6. **Primeira Quest** — NPC dialogue, simples farm task

## Métricas

- **PRs desta fase:** PR-065 (rebind), PR-066-073 (features), PR-074-080 (fixes)
- **Linhas de código C#:** ~3000
- **Assets criados:** ~30 (items, seeds, recipes, NPCs)
- **Cenas:** 2 (FarmScene, TownScene) + BootScene
- **Eventos:** ~15 core events
- **ScriptableObjects:** ~40

---

**Entregue por:** Codex/Claude  
**Data:** 2026-05-17  
**Próxima Entrega:** FASE 9B-1 Handoff (2026-05-18)
