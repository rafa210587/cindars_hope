# Cindar's Hope — 2D Pixel Art RPG + Farm Sim

Um jogo 2D em estilo pixel art combinando simulação de fazenda (inspirado em Harvest Moon/Stardew Valley) com exploração, combate e comércio.

## Estado Atual

- **Fase:** FASE 9B-1 Cave/Combat MVP ✅ Concluída
- **Branch principal:** `dev`
- **Last Updated:** 2026-05-18

### Sistemas Implementados

- ✅ **Core:** EventBus, TimeManager, SaveManager, BootstrapManager
- ✅ **Farm MVP:** PlotSystem, TreeSystem, FishingSpot, ItemPickup, Seedling/Growth
- ✅ **Town MVP:** NPCs, Pip (merchant), BuyItemPoint, SellAllPoint, PortalSystem
- ✅ **Economy:** GoldManager, ShopSystem, SellPoint, price-per-item
- ✅ **Hunger:** HungerManager, FoodConsumer, HP restoration
- ✅ **Crafting MVP:** CraftingSystem, CraftingPoint, RecipeDatabase
- ✅ **Save/Load:** JSON persistence, cross-scene restoration, CurrentScene tracking
- ✅ **Cave MVP:** CaveScene, Portal Farm↔Cave, spawn points
- ✅ **Combat MVP:** PlayerAttackController (melee punch), EnemyHealth, EnemyChaseController, EnemyContactDamage
- ✅ **Scene Generators:** CreateMvpFarmScene, CreateMvpTownScene, CreateMvpCaveScene (Editor menus)
- ✅ **Validators:** MvpSceneValidator para Farm/Town/Cave

### Menus Unity Disponíveis

```
CindarsHope/
├── Scenes/
│   ├── Create MVP FarmScene        → gera Farm com plots/trees/shops
│   ├── Create MVP TownScene        → gera Town com NPCs/buy/sell points
│   └── Create MVP CaveScene        → gera Cave com portal/Slime/spawn points
└── Validate/
    ├── Validate MVP Data           → valida ScriptableObjects do jogo
    ├── Validate Farm Town MVP      → valida estrutura de FarmScene e TownScene
    └── Validate Cave MVP (se houver) → valida estrutura de CaveScene
```

## Como Abrir no Unity

1. Clone o repositório em `D:\Projetos\Jogos\Cindars_hope\cindars_hope`
2. Abra em Unity (versão LTS recomendada)
3. Vá para cena **Assets/_Game/Scenes/BootScene.unity** (ou regenere as cenas via menus acima)
4. Play Mode: use WASD para mover, J para atacar (apenas perto de inimigos)

## Fluxo Operacional de Agentes

### Regra de Continuidade

Antes de qualquer tarefa:
1. Ler `PROJECT_LOG.md` — estado, decisões recentes, pendências
2. Ler `AGENTS.md` / `CLAUDE.md` — regras permanentes
3. Ler documentos de referência da tarefa

Ao final de tarefa relevante:
- Atualizar `PROJECT_LOG.md` com branch, escopo, arquivos, testes, pendências
- Listar arquivos permitidos/proibidos
- Executar `git status --short` e `git diff --stat` antes do commit

### Git Policy para Agentes

**Permitido:**
- Criar/usar branch local indicada
- Alterar somente arquivos permitidos no escopo
- Criar commits locais em português
- Atualizar `PROJECT_LOG.md`
- Entregar lista de commits, arquivos, testes

**Proibido:**
- `git push`
- Abrir PR/MR
- `git merge`
- `git stash` (sem autorização explícita)
- `git clean` / `git reset --hard`
- Commitar fora do escopo

**Push, PR/MR, merge e cleanup são responsabilidade humana.**

## Arquitetura Técnica

### Padrões

- **EventBus:** comunicação via GameEventBus.Publish/Subscribe (sem FindObject, sem comunicação direta)
- **Persistência:** GameBootstrap singleton + managers persistentes entre cenas
- **SceneInstallers:** FarmSceneRuntimeReferenceInstaller, TownSceneRuntimeReferenceInstaller, CaveSceneRuntimeReferenceInstaller
- **Serialização:** JSON via SaveData (não serializa GameObject/Transform/MonoBehaviour)

### Contratos Key

**Cross-Scene:**
- GameBootstrap.Instance persiste entre cenas
- InventoryManager, PlayerManager, TimeManager, SaveManager, HungerManager, CraftingManager, EconomyManager persistem
- CurrentSceneName/CurrentScenePath salvos para reload correto

**Combat MVP:**
- PlayerAttackController detecta EnemyHealth por componente (GetComponentInParent + GetComponent)
- EnemyContactDamage detecta PlayerManager via GameBootstrap.Instance ou componente
- EnemyChaseController recebe target via gerador, persegue lentamente se Player em raio
- **NÃO depender de tags** — preferir componentes

**Geradores:**
- Reproduzíveis: não requerem state anterior
- Não alteram ProjectSettings, tags, layers
- Não usam FindObjectsByType/FindObjectOfType em runtime
- Avisos de Sorting Layer são aceitáveis MVP, registrados como dívida

## Smoke Test Checklist

- [ ] Farm: plantar seed → passar dias → colher → vender → salvar/carregar
- [ ] Town: andar, falar com Pip, comprar seeds, voltar à Farm
- [ ] Cave: ir da Farm, andar na Cave, socar Slime (J), receber/causar dano, voltar à Farm
- [ ] Save: salvar em qualquer cena, fechar jogo, reabrir, estado restaurado
- [ ] HUD: HP, Gold, Hunger, Inventory aparecem e atualizam
- [ ] Console: sem erro vermelho

## Próximas Waves Recomendadas

1. **PR-092** — Limpar warnings (Sorting Layer, SceneSpawnPoint, Input Manager deprecado)
2. **PR-093** — Combat Feel: knockback, invulnerability frames, feedback visual placeholder
3. **PR-094** — Cave drops/resource node hardening
4. **PR-095** — Inventory UI real MVP
5. **PR-096** — Shop UI real MVP
6. **PR-097** — Crafting UI real MVP
7. **PR-098** — Dialogue box NPC
8. **PR-099** — Primeira quest simples Farm↔Town↔Cave
9. **Visual Slice** — todas as features integradas
10. **Fase 10** — Arte final/polish/release prep

## Documentação Principal

- **README.md** ← você está aqui
- **PROJECT_LOG.md** — histórico de PRs, decisões, estado
- **AGENTS.md** — regras de agente, fluxo git
- **CLAUDE.md** — regras de código, convenções, padrões
- **docs/GDD_v2.6.md** — design do jogo completo
- **docs/ARCH_fase4_v2.2.md** — arquitetura técnica detalhada
- **docs/FASE9A_HANDOFF_FARM_TOWN_SAVE_v1.0.md** — entrega FASE 9A
- **docs/FASE9B_CAVE_COMBAT_MVP_v1.0.md** — entrega FASE 9B-1
- **docs/NEXT_WAVES_ROADMAP_v1.0.md** — roadmap pós 9B-1

## Contato / Issues

Reportar bugs ou sugestões em PROJECT_LOG.md com contexto e reprodução.

---

**Last commit:** `docs: sincronizar projeto pos fase 9b cave combat`
**Branch:** `dev`
**Build Status:** ✅ Compila, todos os sistemas rodando no MVP
